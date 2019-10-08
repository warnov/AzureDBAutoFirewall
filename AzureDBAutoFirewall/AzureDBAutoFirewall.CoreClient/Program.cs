using AzureDBAutoFirewall.Lib;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Reflection;
using WarNov.EasyPost.NetStandard;

namespace AzureDBAutoFirewall.CoreClient
{
    class Program
    {
        static void Main(string[] args)
        {
            var exePath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            var authContent = File.ReadAllText($"{exePath}{Path.DirectorySeparatorChar}auth.json");
            var auth = JsonConvert.DeserializeObject<Auth>(authContent);
            var firewallManager = new FirewallManagerPayloadIn()
            {
                PartitionKey = auth.UserName,
                Token = auth.Token
            };
            string fxUrl = "http://localhost:7071/api/FirewallManager";
            var result = EasyPost.Post(fxUrl, firewallManager);
            Console.WriteLine(result.Content.ReadAsStringAsync().Result);
        }
    }
}