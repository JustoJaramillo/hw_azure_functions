using hw_azure_functions.Functions.Functions;
using hw_azure_functions.Test.Helpers;
using System;
using Xunit;

namespace hw_azure_functions.Test.Tests
{
    public class ScheduledTotalTimeWorkedTest
    {

        [Fact]
        public void ScheduledTask_Should_Log_Message()
        {
            //Arrange
            MockCloudTableWorkingHoursEntity mockCloudTableWorkingHoursEntity = new MockCloudTableWorkingHoursEntity(new Uri("http://127.0.0.1:10002/devstoreaccount1/reports"));
            MockCloudTableTotalTimeWorkedEntity mockCloudTableTotalTimeWorkedEntity = new MockCloudTableTotalTimeWorkedEntity(new Uri("http://127.0.0.1:10002/devstoreaccount1/reports"));
            ListLogger logger = (ListLogger)TestFactory.CreateLogger(LoggerTypes.List);
            //Act
            _ = ScheduledTotalTimeWorked.Run(null, mockCloudTableWorkingHoursEntity, mockCloudTableTotalTimeWorkedEntity, logger);
            string message = logger.Logs[0];
            //Assert
            Assert.Contains("Consolidating functions executed at:", message);
        }
    }
}
