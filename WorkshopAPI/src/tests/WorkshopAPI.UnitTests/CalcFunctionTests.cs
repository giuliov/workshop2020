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
            Environment.SetEnvironmentVariable("FF_USE_V2_ALGORITHM", null, EnvironmentVariableTarget.Process);
            // Act
            var response = (OkObjectResult)await CalcFunction.Run(request, inputValue, logger);
            // Assert
            Assert.Equal(expectedResult, response.Value);
        }

        [Theory]
        [InlineData(0, 14)]
        [InlineData(6, 18)]
        public async void Version2_algorithm(int inputValue, int expectedResult)
        {
            // Arrange
            var request = TestFactory.CreateHttpRequest();
            Environment.SetEnvironmentVariable("FF_USE_V2_ALGORITHM", "true", EnvironmentVariableTarget.Process);
            // Act
            var response = (OkObjectResult)await CalcFunction.Run(request, inputValue, logger);
            // Assert
            Assert.Equal(expectedResult, response.Value);
        }
    }
}
