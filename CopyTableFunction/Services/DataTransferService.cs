using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CopyTableFunction.Services
{
    public class DataTransferService
    { 
        public void TransferData()
        {
            try
            {
                var sourceConnectionString = Environment.GetEnvironmentVariable("sourceConnection"); ;
                string[] sourceTableNames = new string[] { "User", "Test1", "Test2", "Test3" };
                var destinationContainerName = "blobpractice";

                var sourceClient = new TableServiceClient(sourceConnectionString);
                var blobServiceClient = new BlobServiceClient(Environment.GetEnvironmentVariable("DestinationConnection"));
                var blobContainerClient = blobServiceClient.GetBlobContainerClient(destinationContainerName);

                foreach (var sourceTableName in sourceTableNames)
                {
                    var sourceTable = sourceClient.GetTableClient(sourceTableName);

                    var entities = sourceTable.Query<TableEntity>().AsPages();
                    var csvData = new StringBuilder();

                    foreach (var page in entities)
                    {
                        foreach (var entity in page.Values)
                        {
                            csvData.AppendLine(ConvertEntityToCsv(entity));
                        }
                    }

                    // Upload aggregated CSV data to blob storage
                    var currentDateTime = DateTime.Now.ToString("yyyyMMdd_HHmmss"); 
                    var destinationBlobName = $"{sourceTableName}_{currentDateTime}.csv";
                    var blobClient = blobContainerClient.GetBlobClient(destinationBlobName);
                    blobClient.UploadAsync(new MemoryStream(Encoding.UTF8.GetBytes(csvData.ToString())), true);

                    Console.WriteLine($"Data from table '{sourceTableName}' copied successfully.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private string ConvertEntityToCsv(TableEntity entity)
        {
            var csvData = new StringBuilder();

            // Get the properties (column names) from the entity
            var properties = entity.Keys.Where(key => key != "odata.etag").ToArray();

            // Append header row with column names
            csvData.AppendLine(string.Join(",", properties));

            // Append entity data in the correct order
            foreach (var property in properties)
            {
                var value = entity[property].ToString();
                // Check if the value starts with "W/" and remove it
                if (value.StartsWith("W/\"datetime'") && value.EndsWith("'\""))
                {
                    value = value.Replace("W/\"datetime'", "").Replace("'", "").Replace("%3A", ":");
                }
                csvData.Append($"{value},");
            }

            // Remove the trailing comma
            csvData.Length--;

            // Add a new line for the next entity
            csvData.AppendLine();

            return csvData.ToString();
        }
    }
}
