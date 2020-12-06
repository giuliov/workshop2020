using Functions.Tests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using WorkshopAPI.DataModels;
using Xunit;

namespace WorkshopAPI.UnitTests
{
    public class EntitiesFunctionTests
    {
        private readonly ILogger logger = TestFactory.CreateLogger();

        [Fact]
        public async void Http_trigger_should_return_same_entity()
        {
            // Arrange
            var request = TestFactory.CreateHttpRequest();
            var entity = new SomeEntity { Name = "TestName", Description="Some useful info", RowKey="dummyRowKey" };
            // Act
            var response = (OkObjectResult)await EntitiesFunction.Run(request, entity, Guid.Empty.ToString(), logger);
            // Assert
            Assert.Equal(entity, response.Value);
        }
    }
}
