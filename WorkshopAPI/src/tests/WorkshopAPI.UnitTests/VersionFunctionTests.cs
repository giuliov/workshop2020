using Functions.Tests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using Xunit;

namespace WorkshopAPI.UnitTests
{
    public class VersionFunctionTests
    {
        private readonly ILogger logger = TestFactory.CreateLogger();

        [Fact]
        public async void Succeeds_and_returns_a_correctly_formatted_value()
        {
            // Arrange
            var request = TestFactory.CreateHttpRequest();
            // Act
            var response = (OkObjectResult)await VersionFunction.Run(request, logger);
            // Assert
            Assert.Equal(200, response.StatusCode);
            Assert.True(Version.TryParse(response.Value.ToString(), out Version parsed));
        }
    }
}
