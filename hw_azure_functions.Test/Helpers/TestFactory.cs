using hw_azure_functions.Common.Models;
using hw_azure_functions.Functions.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using System;
using System.IO;

namespace hw_azure_functions.Test.Helpers
{
    public class TestFactory
    {
        public static WorkingHoursEntity GetWorkingHoursEntity()
        {
            return new WorkingHoursEntity
            {
                ETag = "*",
                PartitionKey = "WORKINGHOURS",
                RowKey = Guid.NewGuid().ToString(),
                EmployeeId = 1,
                RecordDate = DateTime.UtcNow,
                RecordType = false,
                Consolidated = false
            };
        }
        public static DefaultHttpRequest CreateHttpRequest(Guid EmployeeId, WorkingHoursEntry workingHoursEntryRequest)
        {
            string request = JsonConvert.SerializeObject(workingHoursEntryRequest);
            return new DefaultHttpRequest(new DefaultHttpContext())
            {
                Body = GenerateStringFromString(request),
                Path = $"/{EmployeeId}"
            };
        }

        public static DefaultHttpRequest CreateHttpRequest(Guid EmployeeId)
        {

            return new DefaultHttpRequest(new DefaultHttpContext())
            {
                Path = $"/{EmployeeId}"
            };
        }
        public static DefaultHttpRequest CreateHttpRequest(WorkingHoursEntry workingHoursEntryRequest)
        {
            string request = JsonConvert.SerializeObject(workingHoursEntryRequest);
            return new DefaultHttpRequest(new DefaultHttpContext())
            {
                Body = GenerateStringFromString(request),
            };
        }

        public static DefaultHttpRequest CreateHttpRequest()
        {
            return new DefaultHttpRequest(new DefaultHttpContext());
        }

        public static WorkingHoursEntry GetWorkingHoursEntryRequest()
        {
            return new WorkingHoursEntry
            {
                EmployeeId = 1,
                RecordDate = DateTime.UtcNow,
                RecordType = false,
                Consolidated = false
            };
        }

        public static Stream GenerateStringFromString(string stringToConvert)
        {
            MemoryStream memoryStream = new MemoryStream();
            StreamWriter streamWriter = new StreamWriter(memoryStream);
            streamWriter.Write(stringToConvert);
            streamWriter.Flush();
            memoryStream.Position = 0;
            return memoryStream;
        }

        public static ILogger CreateLogger(LoggerTypes loggerTypes = LoggerTypes.Null)
        {
            ILogger logger;
            if (loggerTypes == LoggerTypes.List)
            {
                logger = new ListLogger();
            }
            else
            {
                logger = NullLoggerFactory.Instance.CreateLogger("Null Logger");
            }
            return logger;
        }
    }
}
