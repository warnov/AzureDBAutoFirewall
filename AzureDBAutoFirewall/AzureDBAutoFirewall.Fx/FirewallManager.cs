using AzureDBAutoFirewall.Lib;
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
using System.Collections.Generic;
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
            //Getting environment variables
            var sqlServerName = Environment.GetEnvironmentVariable("SqlServerName");
            var authFile = Environment.GetEnvironmentVariable("AzureAuthLocation");
            var subscriptionId = Environment.GetEnvironmentVariable("SubscriptionId");
            var storageConnectionString = Environment.GetEnvironmentVariable("AzureConnectionString");
            var controlTableName = Environment.GetEnvironmentVariable("ControlTableName");

            //Receiving parameters into a firewallManager object
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var firewallManager = JsonConvert.DeserializeObject<FirewallManagerPayloadIn>(requestBody);
            firewallManager.NewIp = GetIpFromRequestHeaders(req);
            firewallManager.SetStorage(storageConnectionString, controlTableName);

            //Determining the location of the auth file
            var home = Environment.GetEnvironmentVariable("HOME", EnvironmentVariableTarget.Process);
            var authFilePath = String.IsNullOrEmpty(home) ?
                @$"..\..\..\{authFile}" :
                Path.Combine(home, @"site\wwwroot\Files", authFile);


            if (firewallManager.Authorize())
            {
                try
                {
                    //Get Azure
                    var azure = GetAzure(authFilePath, subscriptionId);

                    //Get SQL Server
                    var sqlServer =
                       (from server in azure.SqlServers.List()
                        where server.Name == sqlServerName
                        select server).FirstOrDefault();

                    //Delete old rule
                    var oldFirewallRule =
                        (from fwrule in sqlServer.FirewallRules.List()
                         where fwrule.StartIPAddress == firewallManager.OldIp
                         select fwrule).FirstOrDefault();
                    if (oldFirewallRule != null)
                        oldFirewallRule.Delete();

                    //Add new Rule
                    var newFirewallRule = sqlServer.FirewallRules.Define(firewallManager.UserName)
                            .WithIPAddress(firewallManager.NewIp)
                            .Create();

                    //Clean the token from FirewallManager to Save storage space in table storage
                    firewallManager.Token = string.Empty;

                    //Audit the operation
                    var firewallEntities = new List<FirewallManagerPayloadIn>()
                    { 
                        //Token registry update (to mark last ip)
                        new FirewallManagerPayloadIn()
                        {
                            PartitionKey = firewallManager.PartitionKey,
                            RowKey = "token",
                            NewIp = firewallManager.NewIp
                        },
                        //Operation insert (to have traceability)
                        firewallManager
                    };
                    //Excute the batch insertion
                    firewallManager.BatchInsertOrMerge(firewallEntities);
                    return new OkObjectResult("Firewall configured OK");
                }
                catch (Exception exc)
                {
                    return new ConflictObjectResult(exc);
                }
            }
            else return new UnauthorizedResult();
        }

        private static IAzure GetAzure(string authFile, string subscriptionId)
        {
            var credentials = SdkContext.AzureCredentialsFactory.FromFile(authFile);

            return Azure
                .Configure()
                .WithLogLevel(HttpLoggingDelegatingHandler.Level.Basic)
                .Authenticate(credentials).WithSubscription(subscriptionId);
        }

        private static string GetIpFromRequestHeaders(HttpRequest request)
        {
            return (request.Headers["X-Forwarded-For"].FirstOrDefault() ?? "").Split(new char[] { ':' }).FirstOrDefault();
        }
    }
}
