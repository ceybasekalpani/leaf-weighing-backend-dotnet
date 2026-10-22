TFMS setup
==========

The API uses `68.178.166.190,1434`, SQL login `sa`, with:

- `BoughtLeaf_TFMS` for leaf collection data (existing dbo mappings).
- `Setup_tbl_TFMS.CollectionApp.UserSetup` for users.
- `Setup_tbl_TFMS.CollectionApp.UserPermissions` for permissions.

Old local connection strings remain commented in both appsettings files.
The new checked-in strings have empty passwords. Before running the API,
configure the password through environment variables with the helper below.
Do not commit the SQL password.

First run, from PowerShell at the repository root:

```powershell
. .\database\Connect-Tfms.ps1 -MigrateUsers
dotnet run --project .\LeafWeighing.Api
```

The leading dot and space are required to dot-source the helper. It prompts
for the password without displaying it, verifies access to both databases,
then copies local `Setup_tbl_Kandedola.dbo.UserSetup` and `dbo.UserPermissions`.
The destination databases must already exist. The login needs permission to
create the schema/tables and insert rows in `Setup_tbl_TFMS`.

The copy preserves all inspected source columns, including RegDate, identity
IDs, passwords and current permission flags. It creates the FK with cascade
delete and permission defaults of true. Source tables are only read.
Both destination tables and all copied rows are committed together; failures
roll back the destination transaction. If either destination table already
exists, migration stops without replacing or merging data. Do not run the old
001/002 SQL scripts against TFMS; they target the legacy local dbo tables.

For subsequent terminals, omit migration:

```powershell
. .\database\Connect-Tfms.ps1
dotnet run --project .\LeafWeighing.Api
```

Environment variables apply to the current terminal and its child processes.
When running via a service or hosting platform, configure the full
`ConnectionStrings__BoughtLeafDatabase` and `ConnectionStrings__SetupDatabase`
values there instead. The helper does not copy the BoughtLeaf database or
change the API port; stop an existing API process first if port 5000 is busy.
