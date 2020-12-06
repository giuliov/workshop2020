using Functions.Tests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using Xunit;

namespace WorkshopAPI.UnitTests
{
    public class CalcFunctionTests
    {
        private readonly ILogger logger = TestFactory.CreateLogger();

        [Theory]
        [InlineData(0, 21)]
        [InlineData(6, 24)]
        public async void Version1_algorithm(int inputValue, int expectedResult)
        {
            // Arrange
            var request = TestFactory.CreateHttpRequest();
            // Act
            var response = (OkObjectResult)await CalcFunction.Run(request, inputValue, logger);
            // Assert
            Assert.Equal(expectedResult, response.Value);
        }
    }
}
