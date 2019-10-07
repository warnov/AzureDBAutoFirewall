using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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
            var authFile = Environment.GetEnvironmentVariable("azureAuthLocation");
            var subscriptionId = Environment.GetEnvironmentVariable("subscriptionId");
            var storageConnectionString = Environment.GetEnvironmentVariable("storageConnectionString");
            var controlTableName = Environment.GetEnvironmentVariable("controlTableName");
            var azure = GetAzure(authFile, subscriptionId);

            var sqlServer =
               (from server in azure.SqlServers.List()
                where server.Name == sqlServerName
                select server).FirstOrDefault();

            var oldFirewallRule =
                (from fwrule in sqlServer.FirewallRules.List()
                 where fwrule.StartIPAddress == oldIp
                 select fwrule).FirstOrDefault();
            oldFirewallRule.Delete();

            var newFirewallRule = sqlServer.FirewallRules.Define("userReq")
                    .WithIPAddress(clientIP)
                    .Create();

            return name != null
                ? (ActionResult)new OkObjectResult($"Hello, {name}")
                : new BadRequestObjectResult("Please pass a name on the query string or in the request body");
        }

        private static IAzure GetAzure(string authFile, string subscriptionId)
        {
            var credentials = SdkContext.AzureCredentialsFactory.FromFile(authFile);

            return Azure
                .Configure()
                .WithLogLevel(HttpLoggingDelegatingHandler.Level.Basic)
                .Authenticate(credentials).WithSubscription(subscriptionId);
        }
    }
}
