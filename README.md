# AzureDBAutoFirewall
An Azure Function that automatically adds and remove exceptions from Azure Database Firewall. It ships a dotnet core console client so authorized client machines can request a new firewall exception whenever their IP addresses change.
## Use case
You have a SQL Azure Database server that needs to be directly accessed from many machines outside Azure (not a very good practice). So you have to [add authorized IPs](https://docs.microsoft.com/en-us/azure/sql-database/sql-database-server-level-firewall-rule) for each of those IPs in the servers' firewall. If those client machines don't have a static IP address you will be receiving tons of calls asking you to add new firewall rules every time their IPs change.
## Solution 
Having a small application that runs on the client machines and able to request Azure for a firewall rule update without human intervention will be relieving in terms of support cases. Obviously, this operation re
<!--stackedit_data:
eyJoaXN0b3J5IjpbODE5Mjk2ODg2LC03NTI1NTQ1NDVdfQ==
-->