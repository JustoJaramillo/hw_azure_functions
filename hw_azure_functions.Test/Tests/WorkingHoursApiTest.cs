using hw_azure_functions.Common.Models;
using hw_azure_functions.Functions.Entities;
using hw_azure_functions.Functions.Functions;
using hw_azure_functions.Test.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using Xunit;

namespace hw_azure_functions.Test.Tests
{
    public class WorkingHoursApiTest
    {
        private readonly ILogger logger = TestFactory.CreateLogger();

        [Fact]
        public async void CreateEntry_Should_Return_200()
        {
            //Arrange
            MockCloudTableWorkingHoursEntity mockCloudTableWorkingHoursEntity = new MockCloudTableWorkingHoursEntity(new Uri("http://127.0.0.1:10002/devstoreaccount1/reports"));
            WorkingHoursEntry workingHoursEntry = TestFactory.GetWorkingHoursEntryRequest();
            DefaultHttpRequest defaultHttpRequest = TestFactory.CreateHttpRequest(workingHoursEntry);
            //Act
            IActionResult response = await WorkingHoursApi.CreateEntry(defaultHttpRequest, mockCloudTableWorkingHoursEntity, logger);
            //Assert
            OkObjectResult okObjectResult = (OkObjectResult)response;
            Assert.Equal(StatusCodes.Status200OK, okObjectResult.StatusCode);
        }

        [Fact]
        public async void UpdateEntry_Should_Return_200()
        {
            //Arrange
            MockCloudTableWorkingHoursEntity mockCloudTableWorkingHoursEntity = new MockCloudTableWorkingHoursEntity(new Uri("http://127.0.0.1:10002/devstoreaccount1/reports"));
            WorkingHoursEntry workingHoursEntry = TestFactory.GetWorkingHoursEntryRequest();
            Guid EmployeeId = Guid.NewGuid();
            DefaultHttpRequest defaultHttpRequest = TestFactory.CreateHttpRequest(EmployeeId, workingHoursEntry);
            //Act
            IActionResult response = await WorkingHoursApi.UpdateEntry(defaultHttpRequest, mockCloudTableWorkingHoursEntity, EmployeeId.ToString(), logger);
            //Assert
            OkObjectResult okObjectResult = (OkObjectResult)response;
            Assert.Equal(StatusCodes.Status200OK, okObjectResult.StatusCode);
        }


        [Fact]
        public async void GetAllEntries_Should_Return_200()
        {
            //Arrange
            MockCloudTableWorkingHoursEntity mockCloudTableWorkingHoursEntity = new MockCloudTableWorkingHoursEntity(new Uri("http://127.0.0.1:10002/devstoreaccount1/reports"));
            WorkingHoursEntry workingHoursEntry = TestFactory.GetWorkingHoursEntryRequest();
            DefaultHttpRequest defaultHttpRequest = TestFactory.CreateHttpRequest(workingHoursEntry);

            //Act
            IActionResult response = await WorkingHoursApi.GetAllEntries(defaultHttpRequest, mockCloudTableWorkingHoursEntity, logger);

            //Assert
            OkObjectResult okObjectResult = (OkObjectResult)response;
            Assert.Equal(StatusCodes.Status200OK, okObjectResult.StatusCode);
        }

        [Fact]
        public async void GetEntryById_Should_Return_200()
        {
            //Arrange
            MockCloudTableWorkingHoursEntity mockCloudTableWorkingHoursEntity = new MockCloudTableWorkingHoursEntity(new Uri("http://127.0.0.1:10002/devstoreaccount1/reports"));
            WorkingHoursEntry workingHoursEntry = TestFactory.GetWorkingHoursEntryRequest();
            Guid EmployeeId = Guid.NewGuid();
            DefaultHttpRequest defaultHttpRequest = TestFactory.CreateHttpRequest(EmployeeId);


            //Act
            IActionResult response = await WorkingHoursApi.GetEntryById(defaultHttpRequest, mockCloudTableWorkingHoursEntity, EmployeeId.ToString(), logger);

            //Assert
            OkObjectResult okObjectResult = (OkObjectResult)response;
            Assert.Equal(StatusCodes.Status200OK, okObjectResult.StatusCode);
        }

        [Fact]
        public async void DeleteEntryById_Should_Return_200()
        {
            //Arrange
            MockCloudTableWorkingHoursEntity mockCloudTableWorkingHoursEntity = new MockCloudTableWorkingHoursEntity(new Uri("http://127.0.0.1:10002/devstoreaccount1/reports"));
            WorkingHoursEntry workingHoursEntry = TestFactory.GetWorkingHoursEntryRequest();
            Guid EmployeeId = Guid.NewGuid();
            DefaultHttpRequest defaultHttpRequest = TestFactory.CreateHttpRequest(EmployeeId, workingHoursEntry);

            WorkingHoursEntity workingHoursEntity = TestFactory.GetWorkingHoursEntity();

            //Act
            IActionResult response = await WorkingHoursApi.DeleteEntryById(defaultHttpRequest, workingHoursEntity, mockCloudTableWorkingHoursEntity, EmployeeId.ToString(), logger);

            //Assert
            OkObjectResult okObjectResult = (OkObjectResult)response;
            Assert.Equal(StatusCodes.Status200OK, okObjectResult.StatusCode);
        }


        [Fact]
        public async void GetAllTime_Should_Return_200()
        {
            //Arrange
            MockCloudTableTotalTimeWorkedEntity mockCloudTableTotalTimeWorkedEntity = new MockCloudTableTotalTimeWorkedEntity(new Uri("http://127.0.0.1:10002/devstoreaccount1/reports"));
            TotalTimeWorked totalTimeWorked = TestFactory.GetTotalTimeWorkedRequest();
            DefaultHttpRequest defaultHttpRequest = TestFactory.CreateHttpRequest(totalTimeWorked);


            //Act
            IActionResult response = await WorkingHoursApi.GetTotalTime(defaultHttpRequest, mockCloudTableTotalTimeWorkedEntity, logger);

            //Assert
            OkObjectResult okObjectResult = (OkObjectResult)response;
            Assert.Equal(StatusCodes.Status200OK, okObjectResult.StatusCode);
        }

    }
}
