using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureKeyVault;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Linq;
using System.Net.Http;

namespace Willezone.Azure.WebJobs.Extensions.AzureKeyVault
{
    public static class KeyVaultWebJobsBuilderExtensions
    {
        /// <summary>
        /// Adds an <see cref="T:Microsoft.Extensions.Configuration.IConfigurationProvider" /> that reads configuration values from the Azure KeyVault using client secrets.
        /// </summary>
        /// <param name="builder">The <see cref="IWebJobsBuilder" /> to add to.</param>
        /// <param name="vault">The Azure KeyVault uri.</param>
        /// <param name="credentials">The credentials used to access Azure Key Vault</param>
        public static IWebJobsBuilder AddKeyVault(this IWebJobsBuilder builder, string vault, ClientCredentials credentials)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (credentials == null)
            {
                throw new ArgumentNullException(nameof(credentials));
            }

            return builder.AddKeyVault(configurationBuilder => configurationBuilder.AddAzureKeyVault(vault, credentials.ClientId, credentials.ClientSecret));
        }

        /// <summary>
        /// Adds an <see cref="T:Microsoft.Extensions.Configuration.IConfigurationProvider" /> that reads configuration values from the Azure KeyVault using Managed Service Identity.
        /// </summary>
        /// <param name="builder">The <see cref="IWebJobsBuilder" /> to add to.</param>
        /// <param name="vault">The Azure KeyVault uri.</param>
        /// <returns></returns>
        public static IWebJobsBuilder AddKeyVault(this IWebJobsBuilder builder, string vault)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            return builder.AddKeyVault(configurationBuilder =>
            {
                var azureServiceTokenProvider = new AzureServiceTokenProvider();
                var callback = new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback);
                var keyVaultClient = new KeyVaultClient(callback, new HttpClient());
                configurationBuilder.AddAzureKeyVault(vault, keyVaultClient, new DefaultKeyVaultSecretManager());
            });
        }

        private static IWebJobsBuilder AddKeyVault(this IWebJobsBuilder builder, Action<ConfigurationBuilder> configure)
        {
            var configurationBuilder = new ConfigurationBuilder();
            var descriptor = builder.Services.FirstOrDefault(d => d.ServiceType == typeof(IConfiguration));
            if (descriptor?.ImplementationInstance is IConfigurationRoot configuration)
            {
                configurationBuilder.AddConfiguration(configuration);
            }

            configure(configurationBuilder);

            var config = configurationBuilder.Build();
            builder.Services.Replace(ServiceDescriptor.Singleton(typeof(IConfiguration), config));

            return builder;
        }
    }
}
