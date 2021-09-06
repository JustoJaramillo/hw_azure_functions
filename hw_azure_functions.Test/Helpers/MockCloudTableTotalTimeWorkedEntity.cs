using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace hw_azure_functions.Test.Helpers
{
    public class MockCloudTableTotalTimeWorkedEntity : CloudTable
    {
        public MockCloudTableTotalTimeWorkedEntity(Uri tableAddress) : base(tableAddress)
        {
        }

        public MockCloudTableTotalTimeWorkedEntity(Uri tableAbsoluteUri, StorageCredentials credentials) : base(tableAbsoluteUri, credentials)
        {
        }

        public MockCloudTableTotalTimeWorkedEntity(StorageUri tableAddress, StorageCredentials credentials) : base(tableAddress, credentials)
        {
        }

        public override async Task<TableResult> ExecuteAsync(TableOperation operation)
        {
            return await Task.FromResult(new TableResult
            {
                HttpStatusCode = 200,
                Result = TestFactory.GetTotalTimeWorkedEntity()
            });
        }

        public override async Task<TableQuerySegment<TotalTimeWorkedEntity>> ExecuteQuerySegmentedAsync<TotalTimeWorkedEntity>(TableQuery<TotalTimeWorkedEntity> query, TableContinuationToken token)
        {
            ConstructorInfo constructor = typeof(TableQuerySegment<TotalTimeWorkedEntity>)
                   .GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)
                   .FirstOrDefault(c => c.GetParameters().Count() == 1);

            return await Task.FromResult(constructor.Invoke(new object[] { TestFactory.GetListTotalTimeWorkedEntity() }) as TableQuerySegment<TotalTimeWorkedEntity>);
        }
    }
}
