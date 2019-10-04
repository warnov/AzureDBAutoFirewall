using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;

namespace AzureDBAutoFirewall.Fx
{
    public static class FirewallManager
    {
        [FunctionName("FirewallManager")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {           
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            var oldIp = data?.oldIp;
            string clientIP = req.Headers["X-Forwarded-For"][0];
            var sqlServerName = Environment.GetEnvironmentVariable("sqlServerName");
            var authFile=Environment.GetEnvironmentVariable("azureAuthLocation");
            var subscriptionId= Environment.GetEnvironmentVariable("subscriptionId");
            var azure = GetAzure(authFile, subscriptionId);

          

            return name != null
                ? (ActionResult)new OkObjectResult($"Hello, {name}")
                : new BadRequestObjectResult("Please pass a name on the query string or in the request body");
        }

        private static object GetAzure(string authFile, string subscriptionId)
        {
            throw new NotImplementedException();
        }
    }
}
