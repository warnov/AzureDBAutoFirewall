using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using System;
using System.Linq;

namespace AzureDBAutoFirewall.CoreConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            System.Environment.SetEnvironmentVariable("sqlServerName", "yyyyyyyyyyyyyy");
            System.Environment.SetEnvironmentVariable("subscriptionId", "xxxxxxxxxxxxxxxxxx");
            System.Environment.SetEnvironmentVariable("azureAuthLocation", @"c:\aaa\bbbb.azureauth");


            var oldIp = "186.154.aaa.yyy";
            string clientIP = "191.aaa.89.xxx";
            var sqlServerName = Environment.GetEnvironmentVariable("sqlServerName");
            var authFile = Environment.GetEnvironmentVariable("azureAuthLocation");
            var subscriptionId = Environment.GetEnvironmentVariable("subscriptionId");
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
