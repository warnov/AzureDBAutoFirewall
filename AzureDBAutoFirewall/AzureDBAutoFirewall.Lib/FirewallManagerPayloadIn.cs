using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;
using WarNov.AzureTableCrudSimplifier;

namespace AzureDBAutoFirewall.Lib
{
    public class FirewallManagerPayloadIn:TableEntity
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

        [IgnoreProperty]
        public TableOperator TableOperator { get; set; }

        public void SetStorage(string azureConnectionString, string tableName)
        {
            TableOperator = new TableOperator(azureConnectionString, tableName);
        }

        public bool Persist()
        {
            return TableOperator.InsertOrMergeEntity<FirewallManagerPayloadIn>(this);
        }

        public bool Authorize()
        {
            var persistedObject = TableOperator.UniqueRecordFromTable<FirewallManagerPayloadIn>(this.PartitionKey, "token");
            if (persistedObject != null)
            {
                this.OldIp = persistedObject.NewIp;
                this.RowKey = DateTime.UtcNow.ToString("o");
                return persistedObject.Token == this.Token;
            }
            return false;
        }

        public bool BatchInsertOrMerge(List<FirewallManagerPayloadIn> entities)
        {
            return TableOperator.InsertOrMergeBatchEntities<FirewallManagerPayloadIn>(entities);
        }
    }
}
