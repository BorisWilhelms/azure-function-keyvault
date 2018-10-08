# Azure KeyVault Extensions for Azure Functions v2

- [Azure KeyVault Extensions for Azure Functions v2](#azure-keyvault-extensions-for-azure-functions-v2)
    - [About](#about)
    - [How to configure](#how-to-configure)
    - [Azure Deployment](#azure-deployment)

## About
This repo contains extensions for Azure KeyVault in Azure Function v2. It uses the `Microsoft.Extensions.Configuration.AzureKeyVault` library to include Azure KeyVault into the configuration of your Azure Functions. This lets you use Azure KeyVault with existing bindings on all `AutoResolve` properties, as well as properties that are filled from the configuration, e.g. connection strings. In addition you can use the `AzureKeyVaultClient` to get an `IKeyVaultClient` instance into your function. 

Since this extensions uses the existing configuration provider, all requirements and restrictions are also valid for this extension.

Example usage:

```
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
```
Values for `blobpath` and `storageconnection` are now also pulled from Azure KeyVault, if there are no values present in the normal config. The `AzureKeyVaultClient` Attributes binds an instance of an `IKeyVaultClient` into the function for advanced usage.


## How to configure
The Azure KeyVault extensions are available as a [nuget package](https://www.nuget.org/packages/Willezone.Azure.WebJobs.Extensions.AzureKeyVault). 
Once the package is added to function project, a `WebJobsStartup` is needed to register and configure the the extension.

This is an example `WebJobsStartup` class
```
using ExampleFunction;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Willezone.Azure.WebJobs.Extensions.AzureKeyVault;

[assembly: WebJobsStartup(typeof(Startup))]
namespace ExampleFunction
{
    public class Startup : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder)
        {
            // Create temporary service provider to access configuration.
            var tempProvider = builder.Services.BuildServiceProvider();
            var config = tempProvider.GetRequiredService<IConfiguration>();
            builder.AddAzureKeyVault(config["AzureKeyVault_Uri"]);
        }
    }
}
```
The nuget package contains two extension methods to register the extensions.

```c#
AddAzureKeyVault(this IWebJobsBuilder builder, string vault, string clientId, string clientSecret)
```
This configures the extension to use a client id and client secret to access the KeyVault.

```c#
AddAzureKeyVault(this IWebJobsBuilder builder, string vault)
```
This configures the extension to use managed service identity to access the KeyVault

## Azure Deployment
Currently there is an issue when publishing your function application that the required `extensions.json` is not created correctly. The issue is discussed [here](https://github.com/Azure/azure-functions-host/issues/3386#issuecomment-419565714). Luckily there is a workaround for this: Just copy the [Directory.Build.targets](tools/Directory.Build.targets) file into your Azure Functions project, this will then create the correct `extensions.json` file.