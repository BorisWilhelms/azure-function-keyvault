using Microsoft.Azure.KeyVault;
using Microsoft.Azure.WebJobs.Description;
using System;

namespace Willezone.Azure.WebJobs.Extensions.AzureKeyVault
{
    /// <summary>
    /// Attribute used to get an instance of a <see cref="IKeyVaultClient"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = true)]
    [Binding]
    public sealed class AzureKeyVaultClientAttribute : Attribute
    {
    }
}
