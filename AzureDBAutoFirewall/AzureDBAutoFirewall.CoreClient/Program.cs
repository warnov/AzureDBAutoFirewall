﻿using AzureDBAutoFirewall.Lib;
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
            
            Console.WriteLine("Processing Request on Azure...");
            var result = EasyPost.Post(auth.Url, firewallManager);
            Console.WriteLine(result.Content.ReadAsStringAsync().Result);
        }
    }
}