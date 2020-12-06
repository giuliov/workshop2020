using System;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using WorkshopAPI.DataModels;

namespace Seeder
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            Console.WriteLine("Seeder started");

            if (args.Length < 2) {
                Console.WriteLine("Usage: seeder <num> <storageConnectionString>");
                return 99;
            }
            int numEntities = int.Parse(args[0]);
            string connectionString = args[1];

            var account = CloudStorageAccount.Parse(connectionString);
            var client = account.CreateCloudTableClient();
            var table = client.GetTableReference(nameof(SomeEntity));

            bool created = await table.CreateIfNotExistsAsync();
            Console.WriteLine(created ? "Table created" : "Table existed");

            Console.WriteLine("Adding entitites");
            for (int i = 1; i <= numEntities; i++)
            {
                var entity = new SomeEntity { Name =$"item-{i}", Description = $"This is item nr. {i}", SomeNumber = 42 };
                var insertOperation = TableOperation.InsertOrReplace(entity);
                var result = await table.ExecuteAsync(insertOperation);
                if (result.HttpStatusCode == 204)
                {
                    var outEntity = result.Result as TableEntity;
                    Console.WriteLine($"Inserted {outEntity.RowKey}");
                }
            }
            Console.WriteLine("done.");

            return 0; //all good
        }
    }
}
