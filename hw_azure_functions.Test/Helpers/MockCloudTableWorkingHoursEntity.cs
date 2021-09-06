using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace hw_azure_functions.Test.Helpers
{
    public class MockCloudTableWorkingHoursEntity : CloudTable
    {
        public MockCloudTableWorkingHoursEntity(Uri tableAddress) : base(tableAddress)
        {
        }

        public MockCloudTableWorkingHoursEntity(Uri tableAbsoluteUri, StorageCredentials credentials) : base(tableAbsoluteUri, credentials)
        {
        }

        public MockCloudTableWorkingHoursEntity(StorageUri tableAddress, StorageCredentials credentials) : base(tableAddress, credentials)
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
        public override async Task<TableQuerySegment<WorkingHoursEntity>> ExecuteQuerySegmentedAsync<WorkingHoursEntity>(TableQuery<WorkingHoursEntity> query, TableContinuationToken token)
        {
            ConstructorInfo constructor = typeof(TableQuerySegment<WorkingHoursEntity>)
                   .GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)
                   .FirstOrDefault(c => c.GetParameters().Count() == 1);

            return await Task.FromResult(constructor.Invoke(new object[] { TestFactory.GetListWorkingHoursEntity() }) as TableQuerySegment<WorkingHoursEntity>);
        }
    }
}
