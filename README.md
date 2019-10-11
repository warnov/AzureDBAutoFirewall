# AzureDBAutoFirewall
An Azure Function that automatically adds and remove exceptions from Azure Database Firewall. It ships a dotnet core console client so authorized client machines can request a new firewall exception whenever their IP addresses change.
## Use case
You have a SQL Azure Database server that needs to be directly accessed from many machines outside Azure (not a very good practice). So you have to [add authorized IPs](https://docs.microsoft.com/en-us/azure/sql-database/sql-database-server-level-firewall-rule) for each of those IPs in the servers' firewall. If those client machines don't have a static IP address you will be receiving tons of calls asking you to add new firewall rule every time their IPs change.
## Solution 
Having a small application that runs on the client machines and able to request Azure for a firewall rule update without human intervention will be relieving in terms of support cases. Obviously, this operation requires privileged access to Azure that can not be granted to each client. So a centralizing service authorizing requests and executing the firewall rules update is required. And that centralized service is **AzureDBAutoFirewall** that for this case has been deployed on an Azure Function. But you could easily deploy it on any other computing resource such as a WebAPI, a WebSite, a container, etc. 
### How it works
With this proposed solution you will need to have the dotnet core client .exe installed on the machine you want to use to access the Azure SQL Database Server. This .exe must have in its same directory an `auth.json` file that will contain the credentials you will be using to ask for the firewall rules updating. For this implementation, just a username and a token were used (the username is used to name the firewall rule we will be adding/updating on Azure). 
Then, you will need to deploy the Azure Function. It only accepts POST messages. And the post message has included the authentication data and nothing else is required, since the IP is automatically detected by the function itself. So the function authenticate the request (comparing the username and token against an Azure Table) and if authentication is successful it will add/update the rule on the firewall using the [Azure Management Fluent API](https://docs.microsoft.com/en-us/dotnet/azure/dotnet-sdk-azure-concepts?view=azure-dotnet).
This implies that you need to have an Azure Table ready with all the usernames and tokens you want to allow to make this request. This operation of access directly the DB is not something that you will open to all the public. Just a well defined set of users. So I consider having this authentication approach will be enough. Nevertheless, you are welcomed to contribute with more sophisticated mechanisms.
#### Azure Table required structure
A picture is worth a thousand words:
![Initial State](https://lh3.googleusercontent.com/mcmNXDALE6sbgwMKRtBzSdTaHGcRJchZYRcmvAeq3QODZJoFlR5Dnb_jn_wsbpMUkLgR0U7Zpm4 "Initial State")
This is how the table will look before the system starts working. You just add one registry for each of the users you want to have access to the service. Among its username as PartitionKey, you also insert the keyword "token" as RowKey and then the token itself: I recommend a GUID.

You could be tempted to think: "Why not just inserting the token as the RowKey?" But, as this solution also offers audit for all the requests made (when new firewall updates were requested, by whom and through which IPs), we would need 2 tables to have these records and then extra development work to keep these tables in sync. So I came with this approach of using just one table partitioned by users. And the RowKey will have either the word token to identify were to find the token for a user (and then be able to authenticate it) or a datetime indicating a request made by that user, among all the information of that transaction:
![Working State](https://lh3.googleusercontent.com/lQM3sLpvGLh18Brf6SXZZYO6StewjNGhM-rqkxvK5LX5MCseaYf6kxTNAfC62iZIWm1sK5OHbSA "Working State")
Here is the table after some operation ordered descending by RowKey. You can find here that the user developer1 has never requested the service while QATeam has its current firewall rule set to the IP 161.220.150.31; and that this last change was requested on 2019-10-09T19:12:00.9577602Z and finally that the value from the IP before this was 160.22.15.31. So we have a complete traceability here. All this data is filled by the function so you don't have to make any additional manual work besides initializing the table with usernames and tokens.
#### Azure Function required parameters
The function needs to know what table it is going to use to authorize and keep track of the operations. Also, how to access that table (Azure Storage Connection String). Besides, what server firewall is going to be used, and so on. So it is required that you set up these application settings once the function is deployed:

| Setting |Description  |
|--|--|
|SubscriptionId|Id of the subscription owner of the SQL Server we will be working with
|SqlServerName|The name of the server we will be allowing access
|AzureConnectionString|The connection string for the storage account with the control table
|ControlTableName|Name of the Azure table for Auth and Audit
|AzureAuthLocation|Name of the Azure Authorization file

#### Azure Authorization File
By now, you know that all the magic is done by the Azure Function. As mentioned before, the operations are made using the [Azure Management Fluent API](https://docs.microsoft.com/en-us/dotnet/azure/dotnet-sdk-azure-concepts?view=azure-dotnet); so the function needs permissions to read and create resources in your Azure subscription in order to use the Azure Management Libraries for .NET. To this end, you need to [create a service principal](https://docs.microsoft.com/en-us/dotnet/azure/dotnet-sdk-azure-authenticate?view=azure-dotnet#mgmt-file) and configure your app to run with its credentials to grant this access. Service principals provide a way to create a non-interactive account associated with your identity to which you grant only the privileges your app needs to run. You can create one using the Azure CLI:

    az ad sp create-for-rbac --sdk-auth > my.auth
In this example, the service principal will have access to the [Azure Management Fluent API](https://docs.microsoft.com/en-us/dotnet/azure/dotnet-sdk-azure-concepts?view=azure-dotnet) and is stored in a file named `my.auth`. This file should be securely accessible by the function, so you could put it in the file system of the function. For this implementation, we have created a folder called Files under wwwroot in the functions file system and uploaded the file there (you could also store the json content of the file in Azure Vault and then securely read it from there). The name of the file then will be the value for the parameter `AzureAuthLocation` referenced before.
## Execution
Once you have all the pieces deployed:

 1. The table with authorized users
 2. The Azure Function 
    	2.1. App Settings adjusted
    	2.2. Authorization file uploaded    	
 3. The core client application deployed on a client machine with a valid username, token and url for the function.

Then you, as a client just have to launch the core client .exe and let the magic happens. Then, as a sysadmin, you just have to access the table and audit the operations or CRUD users.

<!--stackedit_data:
eyJoaXN0b3J5IjpbMTUxNzkzNzA5NSwtNDc1MzMzNTM4LDE3Nj
g3ODAyNjMsLTIzODk3NDA0NSwtMTk0NjEwNTYyMiwxMjE0ODg5
NjIwLC05OTk1ODMwMDEsMTAwNjIyNDI1MCwtNzUyNTU0NTQ1XX
0=
-->