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
            var tempProvider = builder.Services.BuildServiceProvider();
            var config = tempProvider.GetRequiredService<IConfiguration>();
            builder.AddAzureKeyVault(config["AzureKeyVault_Uri"]);
            //builder.AddAzureKeyVault(config["AzureKeyVault_Uri"], config["AzureKeyVault_ClientId"], config["AzureKeyVault_ClientSecret"]);
        }
    }
}
