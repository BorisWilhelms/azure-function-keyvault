using Microsoft.Azure.KeyVault;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.IO;
using Willezone.Azure.WebJobs.Extensions.AzureKeyVault;

namespace ExampleFunction
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static void Run(
            [BlobTrigger("%blobpath%", Connection = "storageconnection")]Stream myBlob,
            string name,
            ILogger log,
            [AzureKeyVaultClient]IKeyVaultClient client)
        {
            log.LogInformation($"API Version: {client.ApiVersion}");
            log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");
        }
    }
}
