using System;

namespace Willezone.Azure.WebJobs.Extensions.AzureKeyVault
{
    public class ClientCredentials
    {
        public ClientCredentials(string clientId, string clientSecret)
        {
            if (String.IsNullOrWhiteSpace(clientId))
            {
                throw new ArgumentException(nameof(clientId));
            }

            if (String.IsNullOrWhiteSpace(clientSecret))
            {
                throw new ArgumentException(nameof(clientSecret));
            }

            ClientId = clientId;
            ClientSecret = clientSecret;
        }

        public string ClientId { get; }

        public string ClientSecret { get; }
    }
}
