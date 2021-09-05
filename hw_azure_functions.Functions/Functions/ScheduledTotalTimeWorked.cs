using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using hw_azure_functions.Common.Responses;
using hw_azure_functions.Functions.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;

namespace hw_azure_functions.Functions.Functions
{
    public static class ScheduledTotalTimeWorked
    {
        [FunctionName("ScheduledTotalTimeWorked")]
        public static async Task Run(
            [TimerTrigger("0 */60 * * * *")]TimerInfo myTimer,
            [Table("workinghours", Connection = "AzureWebJobsStorage")] CloudTable workingHoursTable,
            [Table("totaltimeworked", Connection = "AzureWebJobsStorage")] CloudTable totalTimeWorkedTable,
            ILogger log
            )
        {
            log.LogInformation($"Consolidating functions executed at: {DateTime.UtcNow}.");


            string filterEmployees = TableQuery.GenerateFilterConditionForBool("Consolidated", QueryComparisons.Equal, false);
            TableQuery<WorkingHoursEntity> query = new TableQuery<WorkingHoursEntity>().Where(filterEmployees);
            TableQuerySegment<WorkingHoursEntity> entries = await workingHoursTable.ExecuteQuerySegmentedAsync(query, null);
            List<WorkingHoursEntity> orderedEntries = entries.OrderBy(x => x.EmployeeId).ThenBy(x => x.RecordDate).ToList();


            int totalRecordsConsolidaded = 0;
            if (orderedEntries.Count > 1)
            {

                for (int i = 0; i < orderedEntries.Count;)
                {
                    if (orderedEntries.Count == i + 1)
                    {
                        break;
                    }

                    if (orderedEntries[i].RecordType == true && orderedEntries[i + 1].RecordType == false)
                    {
                        i++;
                        continue;
                    }

                    if (orderedEntries[i].EmployeeId == orderedEntries[i + 1].EmployeeId)
                    {
                        totalRecordsConsolidaded++;
                        string filterEmployeeId = TableQuery.GenerateFilterConditionForInt("EmployeeId", QueryComparisons.Equal, orderedEntries[i].EmployeeId);
                        TableQuery<TotalTimeWorkedEntity> queryByEmployeeId = new TableQuery<TotalTimeWorkedEntity>().Where(filterEmployeeId);
                        TableQuerySegment<TotalTimeWorkedEntity> totalTimeWorked = await totalTimeWorkedTable.ExecuteQuerySegmentedAsync(queryByEmployeeId, null);
                        List<TotalTimeWorkedEntity> total = totalTimeWorked.Results;

                        TimeSpan timeMesure = (orderedEntries[i + 1].RecordDate - orderedEntries[i].RecordDate);
                        DateTime dayDate = new DateTime(orderedEntries[i].RecordDate.Year, orderedEntries[i].RecordDate.Month, orderedEntries[i].RecordDate.Day);

                        TotalTimeWorkedEntity totalTimeWorkedEntity = new TotalTimeWorkedEntity
                        {
                            EmployeeId = orderedEntries[i].EmployeeId,
                            Date = dayDate,
                            MinutesWorked = (int)timeMesure.TotalMinutes,
                            ETag = "*",
                            PartitionKey = "TOTALTIMEWORKED",
                            RowKey = Guid.NewGuid().ToString()
                        };

                        WorkingHoursEntity workingHoursEntity = new WorkingHoursEntity();

                        TableResult findFirstRecord = await workingHoursTable.ExecuteAsync(TableOperation.Retrieve<WorkingHoursEntity>("WORKINGHOURS", orderedEntries[i].RowKey));
                        workingHoursEntity = (WorkingHoursEntity)findFirstRecord.Result;
                        workingHoursEntity.Consolidated = true;
                        _ = await workingHoursTable.ExecuteAsync(TableOperation.Replace(workingHoursEntity));

                        TableResult findSecondRecord = await workingHoursTable.ExecuteAsync(TableOperation.Retrieve<WorkingHoursEntity>("WORKINGHOURS", orderedEntries[i + 1].RowKey));
                        workingHoursEntity = (WorkingHoursEntity)findSecondRecord.Result;
                        workingHoursEntity.Consolidated = true;
                        _ = await workingHoursTable.ExecuteAsync(TableOperation.Replace(workingHoursEntity));

                        if (totalTimeWorked.Results.Count == 0)
                        {
                            _ = await totalTimeWorkedTable.ExecuteAsync(TableOperation.Insert(totalTimeWorkedEntity));
                        }
                        else
                        {
                            TableResult findEmployeeRecord = await totalTimeWorkedTable.ExecuteAsync(TableOperation.Retrieve<TotalTimeWorkedEntity>("TOTALTIMEWORKED", totalTimeWorked.Results.ElementAt(0).RowKey));
                            totalTimeWorkedEntity = (TotalTimeWorkedEntity)findEmployeeRecord.Result;
                            totalTimeWorkedEntity.MinutesWorked += (int)timeMesure.TotalMinutes;
                            _ = await totalTimeWorkedTable.ExecuteAsync(TableOperation.Replace(totalTimeWorkedEntity));
                        }



                    }
                    i++;
                }
            }


            string message = $"Total records consolidated {totalRecordsConsolidaded} at: {DateTime.UtcNow}.";
            log.LogInformation(message);
        }
    }
}
