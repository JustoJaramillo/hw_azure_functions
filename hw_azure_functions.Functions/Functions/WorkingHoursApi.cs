using hw_azure_functions.Common.Models;
using hw_azure_functions.Common.Responses;
using hw_azure_functions.Functions.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace hw_azure_functions.Functions.Functions
{
    public static class WorkingHoursApi
    {
        /*
         * Function to create and Entry
         */
        [FunctionName(nameof(CreateEntry))]
        public static async Task<IActionResult> CreateEntry(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "entry")] HttpRequest req,
            [Table("workinghours", Connection = "AzureWebJobsStorage")] CloudTable workingHoursTable,
            ILogger log)
        {
            log.LogInformation("New timestamp received.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            WorkingHoursEntry workingHoursEntry = JsonConvert.DeserializeObject<WorkingHoursEntry>(requestBody);

            if (string.IsNullOrEmpty(workingHoursEntry?.EmployeeId.ToString()) || workingHoursEntry?.EmployeeId.ToString() == "0")
            {
                return new BadRequestObjectResult(new Response
                {
                    IsSuccess = false,
                    Message = "The request must have an Employee Id"
                });
            }

            WorkingHoursEntity workingHoursEntity = new WorkingHoursEntity
            {
                EmployeeId = workingHoursEntry.EmployeeId,
                RecordDate = workingHoursEntry.RecordDate,
                RecordType = workingHoursEntry.RecordType,
                Consolidated = false,
                ETag = "*",
                PartitionKey = "WORKINGHOURS",
                RowKey = Guid.NewGuid().ToString()
            };

            TableOperation addOperation = TableOperation.Insert(workingHoursEntity);
            await workingHoursTable.ExecuteAsync(addOperation);

            string message = "New timestamp store in table.";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {
                IsSuccess = true,
                Message = message,
                Result = workingHoursEntity
            });
        }

        /*
         * Function to update and Entry
         */
        [FunctionName(nameof(UpdateEntry))]
        public static async Task<IActionResult> UpdateEntry(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "entry/{id}")] HttpRequest req,
            [Table("workinghours", Connection = "AzureWebJobsStorage")] CloudTable workingHoursTable,
            string id,
            ILogger log)
        {
            log.LogInformation($"Update request for entry with employee id {id} received.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            WorkingHoursEntry workingHoursEntry = JsonConvert.DeserializeObject<WorkingHoursEntry>(requestBody);

            /*
             * Validate employee id
             */
            TableOperation findOperantion = TableOperation.Retrieve<WorkingHoursEntity>("WORKINGHOURS", id);
            TableResult findResult = await workingHoursTable.ExecuteAsync(findOperantion);

            if (findResult == null)
            {
                return new BadRequestObjectResult(new Response
                {
                    IsSuccess = false,
                    Message = "EmployeeId not found."
                });
            }

            //Update entry
            WorkingHoursEntity workingHoursEntity = (WorkingHoursEntity)findResult.Result;

            if (!string.IsNullOrEmpty(workingHoursEntry?.RecordDate.ToString()))
            {
                workingHoursEntity.RecordDate = workingHoursEntry.RecordDate;
                workingHoursEntity.RecordType = workingHoursEntry.RecordType;
            }

            TableOperation editOperation = TableOperation.Replace(workingHoursEntity);
            await workingHoursTable.ExecuteAsync(editOperation);

            string message = $"Record entry with employee id {id} updated in table.";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {
                IsSuccess = true,
                Message = message,
                Result = workingHoursEntity
            });
        }


        /*
         * Function to get all entries
         */
        [FunctionName(nameof(GetAllEntries))]
        public static async Task<IActionResult> GetAllEntries(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "entry")] HttpRequest req,
            [Table("workinghours", Connection = "AzureWebJobsStorage")] CloudTable workingHoursTable,
            ILogger log)
        {
            log.LogInformation("Get all entries received.");


            TableQuery<WorkingHoursEntity> query = new TableQuery<WorkingHoursEntity>();
            TableQuerySegment<WorkingHoursEntity> entries = await workingHoursTable.ExecuteQuerySegmentedAsync(query, null);


            string message = "Retrieve all entries.";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {
                IsSuccess = true,
                Message = message,
                Result = entries
            });
        }

        /*
         * Function to get entry by Id
         */
        [FunctionName(nameof(GetEntryById))]
        public static async Task<IActionResult> GetEntryById(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "entry/{id}")] HttpRequest req,
            [Table("workinghours", Connection = "AzureWebJobsStorage")] CloudTable workingHoursTable,
            string id,
            ILogger log)
        {
            log.LogInformation($"Get entry by id {id} received.");

            TableOperation findOperantion = TableOperation.Retrieve<WorkingHoursEntity>("WORKINGHOURS", id);
            TableResult findResult = await workingHoursTable.ExecuteAsync(findOperantion);

            if (findResult == null)
            {
                return new BadRequestObjectResult(new Response
                {
                    IsSuccess = false,
                    Message = "Entry not found"
                });
            }

            string message = $"Entry id {id} Retrieved.";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {
                IsSuccess = true,
                Message = message,
                Result = findResult.Result
            });
        }




        /*
          * Function to get entry by Id
          */
        [FunctionName(nameof(DeleteEntryById))]
        public static async Task<IActionResult> DeleteEntryById(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "entry/{id}")] HttpRequest req,
            [Table("workinghours", "WORKINGHOURS", "{id}", Connection = "AzureWebJobsStorage")] WorkingHoursEntity workingHoursEntity,
            [Table("workinghours", Connection = "AzureWebJobsStorage")] CloudTable workingHoursTable,
            string id,
            ILogger log)
        {
            log.LogInformation($"Delete entry by id {id} received.");

            if (workingHoursEntity == null)
            {
                return new BadRequestObjectResult(new Response
                {
                    IsSuccess = false,
                    Message = "Entry not found"
                });
            }

            await workingHoursTable.ExecuteAsync(TableOperation.Delete(workingHoursEntity));
            string message = $"Entry id {workingHoursEntity.RowKey} deleted.";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {
                IsSuccess = true,
                Message = message,
                Result = workingHoursEntity
            });
        }


        /*
         * Function to get consolidate
         */
        [FunctionName(nameof(GetTotalTime))]
        public static async Task<IActionResult> GetTotalTime(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "totaltime")] HttpRequest req,
            [Table("totaltimeworked", Connection = "AzureWebJobsStorage")] CloudTable totalTimeWorkedTable,
            ILogger log)
        {
            log.LogInformation("Get all consoildated received.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            TotalTimeWorked totalTimeWorked = JsonConvert.DeserializeObject<TotalTimeWorked>(requestBody);

            string filterDate = TableQuery.GenerateFilterConditionForDate("Date", QueryComparisons.Equal, DateTime.Parse(totalTimeWorked.Date.ToString()));
            TableQuery<TotalTimeWorkedEntity> query = new TableQuery<TotalTimeWorkedEntity>().Where(filterDate);
            TableQuerySegment<TotalTimeWorkedEntity> consolidate = await totalTimeWorkedTable.ExecuteQuerySegmentedAsync(query, null);

            string message = "Retrieve all consolidate records.";
            log.LogInformation(message);

            if (consolidate.Results.Count < 1)
            {
                message = $"No record founds for day {totalTimeWorked.Date}.";
            }

            return new OkObjectResult(new Response
            {
                IsSuccess = true,
                Message = message,
                Result = consolidate
            });
        }

        /*
         * Function consolidated
         */
        [FunctionName(nameof(Scheduled))]
        public static async Task<IActionResult> Scheduled(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "sc")] HttpRequest req,
            [Table("workinghours", Connection = "AzureWebJobsStorage")] CloudTable workingHoursTable,
            [Table("totaltimeworked", Connection = "AzureWebJobsStorage")] CloudTable totalTimeWorkedTable,
            ILogger log)
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

            return new OkObjectResult(new Response
            {
                IsSuccess = true,
                Message = message,
            });
        }
    }
}
