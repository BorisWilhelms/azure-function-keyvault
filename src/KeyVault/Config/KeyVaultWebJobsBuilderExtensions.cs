using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureKeyVault;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Willezone.Azure.WebJobs.Extensions.AzureKeyVault
{
    public static class AzureKeyVaultWebJobsBuilderExtensions
    {
        /// <summary>
        /// Adds an <see cref="T:Microsoft.Extensions.Configuration.IConfigurationProvider" /> that reads configuration values from the Azure KeyVault using client secrets.
        /// </summary>
        /// <param name="builder">The <see cref="IWebJobsBuilder" /> to add to.</param>
        /// <param name="vault">The Azure KeyVault uri.</param>
        /// <param name="clientId">The application client id.</param>
        /// <param name="clientSecret">The client secret to use for authentication.</param>
        public static IWebJobsBuilder AddAzureKeyVault(this IWebJobsBuilder builder, string vault, string clientId, string clientSecret)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (clientSecret == null)
            {
                throw new ArgumentNullException(nameof(clientSecret));
            }

            if (String.IsNullOrWhiteSpace(vault))
            {
                throw new ArgumentException("Vault can not be null or whitespace.", nameof(vault));
            }

            if (String.IsNullOrWhiteSpace(clientId))
            {
                throw new ArgumentException("Client Id can not be null or whitespace.", nameof(clientId));
            }

            if (String.IsNullOrWhiteSpace(clientSecret))
            {
                throw new ArgumentException("Client Secret can not be null or whitespace.", nameof(clientSecret));
            }

            return builder.AddAzureKeyVault(configurationBuilder =>
            {
                Task<string> callback(string authority, string resource, string _) =>
                    GetTokenFromClientSecret(authority, resource, clientId, clientSecret);

                var keyVaultClient = new KeyVaultClient(callback, new HttpClient());
                configurationBuilder.AddAzureKeyVault(vault, keyVaultClient, new DefaultKeyVaultSecretManager());

                return keyVaultClient;
            });
        }

        /// <summary>
        /// Adds an <see cref="T:Microsoft.Extensions.Configuration.IConfigurationProvider" /> that reads configuration values from the Azure KeyVault using Managed Service Identity.
        /// </summary>
        /// <param name="builder">The <see cref="IWebJobsBuilder" /> to add to.</param>
        /// <param name="vault">The Azure KeyVault uri.</param>
        /// <returns></returns>
        public static IWebJobsBuilder AddAzureKeyVault(this IWebJobsBuilder builder, string vault)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (String.IsNullOrWhiteSpace(vault))
            {
                throw new ArgumentException("Vault can not be null or whitespace.", nameof(vault));
            }

            return builder.AddAzureKeyVault(configurationBuilder =>
            {
                var azureServiceTokenProvider = new AzureServiceTokenProvider();
                var callback = new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback);
                var keyVaultClient = new KeyVaultClient(callback, new HttpClient());
                configurationBuilder.AddAzureKeyVault(vault, keyVaultClient, new DefaultKeyVaultSecretManager());

                return keyVaultClient;
            });
        }

        private static IWebJobsBuilder AddAzureKeyVault(this IWebJobsBuilder builder, Func<ConfigurationBuilder, IKeyVaultClient> configure)
        {
            var configurationBuilder = new ConfigurationBuilder();
            var descriptor = builder.Services.FirstOrDefault(d => d.ServiceType == typeof(IConfiguration));
            if (descriptor?.ImplementationInstance is IConfigurationRoot configuration)
            {
                configurationBuilder.AddConfiguration(configuration);
            }

            var client = configure(configurationBuilder);

            var config = configurationBuilder.Build();
            builder.Services.Replace(ServiceDescriptor.Singleton(typeof(IConfiguration), config));
            builder.Services.AddSingleton(client);
            builder.AddExtension<AzureKeyVaultExtensionConfigProvider>();

            return builder;
        }

        private static async Task<string> GetTokenFromClientSecret(string authority, string resource, string clientId, string clientSecret)
        {
            var authContext = new AuthenticationContext(authority);
            var clientCred = new ClientCredential(clientId, clientSecret);
            var result = await authContext.AcquireTokenAsync(resource, clientCred);
            return result.AccessToken;
        }
    }
}
