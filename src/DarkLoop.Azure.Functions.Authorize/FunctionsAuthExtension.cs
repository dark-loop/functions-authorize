using Microsoft.Azure.WebJobs.Description;
using Microsoft.Azure.WebJobs.Host.Config;

namespace DarkLoop.Azure.Functions.Authorize
{
    [Extension("FunctionsAuthorize")]
    class FunctionsAuthExtension : IExtensionConfigProvider
    {
        public void Initialize(ExtensionConfigContext context)
        {

        }
    }
}
