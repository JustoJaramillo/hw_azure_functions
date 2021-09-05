using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace hw_azure_functions.Test.Helpers
{
    class MockCloudTableWorkingHoursEntry : CloudTable
    {
        public MockCloudTableWorkingHoursEntry(Uri tableAddress) : base(tableAddress)
        {
        }

        public MockCloudTableWorkingHoursEntry(Uri tableAbsoluteUri, StorageCredentials credentials) : base(tableAbsoluteUri, credentials)
        {
        }

        public MockCloudTableWorkingHoursEntry(StorageUri tableAddress, StorageCredentials credentials) : base(tableAddress, credentials)
        {
        }

        public override async Task<TableResult> ExecuteAsync(TableOperation operation)
        {
            return await Task.FromResult(new TableResult
            {
                HttpStatusCode = 200,
                Result = TestFactory.GetWorkingHoursEntity()
            });
        }
    }
}
