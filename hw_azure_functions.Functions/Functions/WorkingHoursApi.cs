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
        public static IActionResult GetEntryById(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "entry/{id}")] HttpRequest req,
            [Table("workinghours","WORKINGHOURS", "{id}", Connection = "AzureWebJobsStorage")] WorkingHoursEntity workingHoursEntity,
            string id,
            ILogger log)
        {
            log.LogInformation($"Get entry by id {id} received.");

            if (workingHoursEntity == null)
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
                Result = workingHoursEntity
            });
        }
    }
}
