using System;
using Microsoft.Azure.Cosmos.Table;

namespace WorkshopAPI.DataModels
{
    public class SomeEntity : TableEntity
    {
        public const string PARTITION_KEY = "defaultPartition";

        public SomeEntity()
        {
            PartitionKey = PARTITION_KEY;
            RowKey = Guid.NewGuid().ToString();
        }

        public string Name { get; set; }

        public string Description { get; set; }

        public int SomeNumber { get; set; }
    }
}
