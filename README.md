# AzureDBAutoFirewall
An Azure Function that automatically adds and remove exceptions from Azure Database Firewall. It ships a dotnet core console client so authorized client machines can request a new firewall exception whenever their IP addresses change.

## Use case
You have a SQL Azure Database server that needs to be directly accessed from many machines outside Azure (not a very good practice). So you have to [add authorized IPs](https://docs.microsoft.com/en-us/azure/sql-database/sql-database-server-level-firewall-rule) for each of those IPs in the servers' firewall. But those machines don
<!--stackedit_data:
eyJoaXN0b3J5IjpbMzg5NzU4NjI1XX0=
-->