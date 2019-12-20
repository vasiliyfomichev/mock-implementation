using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Storage.Auth;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.Configuration;

namespace UploadAssetToBlob
{
    public static class Function1
    {
        [FunctionName("UploadToBlob")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log, ExecutionContext context)
        {
            log.LogInformation("Started image blob upload from Content Hub.");
            
            string url = req.Query["url"];
            string fileName = req.Query["fileName"];


            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            url = url ?? data?.url;
            fileName = fileName ?? data?.filename;

            if(string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(fileName))
            {
                log.LogError($"Image URI: {url} is not a proper URL.");
                return new OkObjectResult($"Error getting original image URL.");
            }

            var container = Environment.GetEnvironmentVariable("ImageContainer");
            var accountKey = Environment.GetEnvironmentVariable("AccountKey");
            var accountName = Environment.GetEnvironmentVariable("AccountName");
            log.LogInformation($"Container: {container}.");
            log.LogInformation($"Account Key: {accountKey}.");
            log.LogInformation($"Account Name: {accountName}.");


            var blobConfiguration = new AzureStorageConfig
            {
                AccountKey = accountKey, 
                ImageContainer = container,
                AccountName = accountName
            };

            var imageRequest = System.Net.WebRequest.Create(url);
            
            using (var stream = imageRequest.GetResponse().GetResponseStream())
            {
                var blobUrl = await UploadFileToStorage(stream, fileName, blobConfiguration);
                return new OkObjectResult($"Image uploaded to {blobUrl}.");
            }
        }


        public static async Task<string> UploadFileToStorage(Stream fileStream, string fileName, AzureStorageConfig _storageConfig)
        {
            // Create storagecredentials object by reading the values from the configuration (appsettings.json)
            var storageCredentials = new StorageCredentials(_storageConfig.AccountName, _storageConfig.AccountKey);

            // Create cloudstorage account by passing the storagecredentials
            var storageAccount = new CloudStorageAccount(storageCredentials, true);

            // Create the blob client.
            var blobClient = storageAccount.CreateCloudBlobClient();

            // Get reference to the blob container by passing the name by reading the value from the configuration (appsettings.json)
            var container = blobClient.GetContainerReference(_storageConfig.ImageContainer);

            // Get the reference to the block blob from the container
            var blockBlob = container.GetBlockBlobReference(fileName);

            // Upload the file
            await blockBlob.UploadFromStreamAsync(fileStream);

            // Get Uri
            var blobUrl = blockBlob.Uri.AbsoluteUri;

            return await Task.FromResult(blobUrl);
        }
    }
}
