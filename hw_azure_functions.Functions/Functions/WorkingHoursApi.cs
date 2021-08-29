using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage.Table;
using hw_azure_functions.Common.Models;
using hw_azure_functions.Common.Responses;
using hw_azure_functions.Functions.Entities;

namespace hw_azure_functions.Functions.Functions
{
    public static class WorkingHoursApi
    {
        [FunctionName(nameof(CreateEntry))]
        public static async Task<IActionResult> CreateEntry(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "entry")] HttpRequest req,
            [Table("workinghours", Connection = "AzureWebJobsStorage")] CloudTable workingHoursTable,
            ILogger log)
        {
            log.LogInformation("New timestamp received.");


            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            WorkingHoursEntry workingHoursEntry = JsonConvert.DeserializeObject<WorkingHoursEntry>(requestBody);

            if (string.IsNullOrEmpty(workingHoursEntry?.RecordDate.ToString()) || workingHoursEntry?.RecordDate.ToString() == "0")
            {
                return new BadRequestObjectResult(new Response
                {
                    IsSuccess = false,
                    Message = "The request must have an Employee Id"
                });
            }

            //this variable must be define as metodo to evaluete if it is an entry entry=false or an exit entry=true
            //by now it will work as false with the purpose of create the api endpoint to create entry records for employees
            bool EntryType = false;

            WorkingHoursEntity workingHoursEntity = new WorkingHoursEntity
            {
                EmployeeId = workingHoursEntry.EmployeeId,
                RecordDate = DateTime.UtcNow,
                RecordType = EntryType,
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
    }
}
