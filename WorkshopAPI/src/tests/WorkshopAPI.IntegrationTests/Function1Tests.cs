using Microsoft.Azure.Cosmos.Table;
using System;
using System.Net;
using System.Net.Http;
using WorkshopAPI.DataModels;
using Xunit;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WorkshopAPI.IntegrationTests
{
    public class Function1Tests
    {
        static HttpClient httpClient = new HttpClient();


        [Fact]
        public async void Add_one_entity_and_retrieve_it()
        {           
            string connectionString = Environment.GetEnvironmentVariable("StorageConnectionString");
            string baseUrl = Environment.GetEnvironmentVariable("BaseUrl");

            // Arrange
            var account = CloudStorageAccount.Parse(connectionString);
            var tableClient = account.CreateCloudTableClient();
            var table = tableClient.GetTableReference(nameof(SomeEntity));
            var entityToInsert = new SomeEntity { Name = "iTest-item", Description = $"This item was created by Integration Tests", SomeNumber = 42 };
            var insertOperation = TableOperation.Insert(entityToInsert);
            var insertResult = await table.ExecuteAsync(insertOperation);
            Assert.Equal(204 /*HttpStatusCode.NoContent*/, insertResult.HttpStatusCode);
            var resultInsertedEntity = insertResult.Result as TableEntity;
            Assert.NotNull(resultInsertedEntity);
            // Act
            var getResponse = await httpClient.GetAsync($"{baseUrl}/api/entities/{resultInsertedEntity.RowKey}");
            // Assert
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
            var getContent = await getResponse.Content.ReadAsStringAsync();
            var resultGetEntity = JObject.Parse(getContent);
            string name = (string)resultGetEntity["name"];
            Assert.Equal("iTest-item", name);
            var timestamp = (DateTimeOffset)resultGetEntity["timestamp"];
            Assert.Equal(resultInsertedEntity.Timestamp, timestamp);
        }

        [Fact, Trait("Category", "SmokeTests")]
        public async void Get_a_wellknown_entity()
        {
            string connectionString = Environment.GetEnvironmentVariable("StorageConnectionString");
            string baseUrl = Environment.GetEnvironmentVariable("BaseUrl");

            // Arrange
            string wellKnownEntityRowKey = "00000000-1111-2222-3333-444444444444";
            // Act
            var getResponse = await httpClient.GetAsync($"{baseUrl}/api/entities/{wellKnownEntityRowKey}");
            // Assert
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
            var getContent = await getResponse.Content.ReadAsStringAsync();
            var resultGetEntity = JObject.Parse(getContent);
            string name = (string)resultGetEntity["name"];
            Assert.Equal("well-known-item", name);
        }
    }
}
