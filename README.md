# AzureDBAutoFirewall
An Azure Function that automatically adds and remove exceptions from Azure Database Firewall. It ships a dotnet core console client so authorized client machines can request a new firewall exception whenever their IP addresses change.
## Use case
You have a SQL Azure Database server that needs to be directly accessed from many machines outside Azure (not a very good practice). So you have to [add authorized IPs](https://docs.microsoft.com/en-us/azure/sql-database/sql-database-server-level-firewall-rule) for each of those IPs in the servers' firewall. If those client machines don't have a static IP address you will be receiving tons of calls asking you to add new firewall rules every time their IPs change.
## Solution 
Having a small application that runs on the client machines and able to request Azure for a firewall rule update without human intervention will be relieving in terms of support cases. Obviously, this operation requires privileged access to Azure that can not be granted to each client. So a centralizing service authorizing requests and executing the firewall rules update is required. And that centralized service is **AzureDBAutoFirewall** that for this case has been deployed on an Azure Function. But you could easily deploy it on any other computing resource such as a WebAPI, a WebSite, a container, etc. 
### How it works
With this proposed solution you will need to have the dotnet core client .exe installed on the machine you want to use to access the Azure SQL Database Server. This .exe must have in its same directory an auth.json file that will contain the credentials you will be using to ask for the firewall rules to update. For this implementation, just a use
<!--stackedit_data:
eyJoaXN0b3J5IjpbLTI1NDcwNDA2NywxMDA2MjI0MjUwLC03NT
I1NTQ1NDVdfQ==
-->