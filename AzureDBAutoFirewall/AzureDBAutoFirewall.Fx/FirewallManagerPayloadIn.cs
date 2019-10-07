using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzureDBAutoFirewall.Fx
{
    public class FirewallManagerPayloadIn : TableEntity
    {

        public string Token { get; set; }
        public string OldIp { get; set; }
        public string NewIp { get; set; }

        public FirewallManagerPayloadIn()
        {
            RowKey = DateTime.UtcNow.ToString("o");
        }

        //From PK
        [IgnoreProperty]
        public string UserName
        {
            get
            {
                return PartitionKey;
            }
            set
            {
                PartitionKey = value;
            }
        }

        public bool Persist()
        {
            return Utilities.InsertOrMergeEntity<FirewallManagerPayloadIn>(TABLE_NAME, this);
        }
    }
}
