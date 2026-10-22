# Dot-source this script to configure the terminal that will run dotnet run.
# Add -MigrateUsers once to copy the local users and permissions to TFMS.
[CmdletBinding()]
param(
    [switch]$MigrateUsers,
    [string]$SourceConnectionString = 'Server=DESKTOP-445DEIC;Database=Setup_tbl_Kandedola;Integrated Security=True;TrustServerCertificate=True;'
)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Data

$securePassword = Read-Host 'SQL password for sa on 68.178.166.190,1434' -AsSecureString
$credential = New-Object System.Net.NetworkCredential('sa', $securePassword)
$connectionBuilder = New-Object System.Data.SqlClient.SqlConnectionStringBuilder
# Use SQL connection-string keys explicitly. Windows PowerShell treats this
# builder as a dictionary, so property aliases such as DataSource can fail.
$connectionBuilder['Data Source'] = '68.178.166.190,1434'
$connectionBuilder['User ID'] = 'sa'
$connectionBuilder['Password'] = $credential.Password
$connectionBuilder['Encrypt'] = $true
$connectionBuilder['TrustServerCertificate'] = $true
$connectionBuilder['Initial Catalog'] = 'Setup_tbl_TFMS'
$setupConnectionString = $connectionBuilder.ConnectionString
$connectionBuilder['Initial Catalog'] = 'BoughtLeaf_TFMS'
$boughtLeafConnectionString = $connectionBuilder.ConnectionString

# Verify access to both databases before changing the terminal configuration.
foreach ($connectionString in @($setupConnectionString, $boughtLeafConnectionString)) {
    $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
    try {
        $connection.Open()
    }
    finally {
        $connection.Dispose()
    }
}

if ($MigrateUsers) {
    $source = New-Object System.Data.SqlClient.SqlConnection($SourceConnectionString)
    $target = New-Object System.Data.SqlClient.SqlConnection($setupConnectionString)
    $data = New-Object System.Data.DataSet
    $sourceTransaction = $null
    $targetTransaction = $null
    try {
        $source.Open()
        $sourceTransaction = $source.BeginTransaction([System.Data.IsolationLevel]::Serializable)
        $read = $source.CreateCommand()
        $read.Transaction = $sourceTransaction
        $read.CommandText = @'
SELECT [Ind], [RegDate], [FullName], [UserName], [Password], [Admin],
       [AdminLevel], [Active], [TempWorker], [Bl_Confirm], [Bl_Report],
       [Bl_Transfer], [Bl_LeafEditDel], [BackDays]
FROM [dbo].[UserSetup];
SELECT [UserInd], [IsCanAddDeduction], [IsCanAddCount]
FROM [dbo].[UserPermissions];
'@
        $adapter = New-Object System.Data.SqlClient.SqlDataAdapter($read)
        try { [void]$adapter.Fill($data) }
        finally { $adapter.Dispose(); $read.Dispose() }
        $sourceTransaction.Commit()

        $target.Open()
        $targetTransaction = $target.BeginTransaction()
        $create = $target.CreateCommand()
        $create.Transaction = $targetTransaction
        $create.CommandText = Get-Content -LiteralPath (Join-Path $PSScriptRoot '003_create_collectionapp_tables.sql') -Raw
        try { [void]$create.ExecuteNonQuery() }
        finally { $create.Dispose() }

        $tableNames = @('[CollectionApp].[UserSetup]', '[CollectionApp].[UserPermissions]')
        $bulkOptions = [System.Data.SqlClient.SqlBulkCopyOptions]::KeepIdentity -bor
                       [System.Data.SqlClient.SqlBulkCopyOptions]::KeepNulls -bor
                       [System.Data.SqlClient.SqlBulkCopyOptions]::CheckConstraints
        for ($tableIndex = 0; $tableIndex -lt $tableNames.Count; $tableIndex++) {
            $bulk = New-Object System.Data.SqlClient.SqlBulkCopy($target, $bulkOptions, $targetTransaction)
            try {
                $bulk.DestinationTableName = $tableNames[$tableIndex]
                foreach ($column in $data.Tables[$tableIndex].Columns) {
                    [void]$bulk.ColumnMappings.Add($column.ColumnName, $column.ColumnName)
                }
                $bulk.WriteToServer($data.Tables[$tableIndex])
            }
            finally { $bulk.Dispose() }

            $count = $target.CreateCommand()
            $count.Transaction = $targetTransaction
            $count.CommandText = 'SELECT COUNT_BIG(*) FROM ' + $tableNames[$tableIndex]
            try {
                if ([long]$count.ExecuteScalar() -ne $data.Tables[$tableIndex].Rows.Count) {
                    throw 'Copied row count did not match the source. Migration will be rolled back.'
                }
            }
            finally { $count.Dispose() }
        }
        $targetTransaction.Commit()
        Write-Host ('Copied {0} users and {1} permission rows to CollectionApp.' -f $data.Tables[0].Rows.Count, $data.Tables[1].Rows.Count)
    }
    catch {
        if ($null -ne $targetTransaction -and $null -ne $targetTransaction.Connection) {
            $targetTransaction.Rollback()
        }
        throw
    }
    finally {
        if ($null -ne $sourceTransaction) { $sourceTransaction.Dispose() }
        if ($null -ne $targetTransaction) { $targetTransaction.Dispose() }
        $source.Dispose()
        $target.Dispose()
        $data.Dispose()
    }
}

$env:ConnectionStrings__SetupDatabase = $setupConnectionString
$env:ConnectionStrings__BoughtLeafDatabase = $boughtLeafConnectionString
$connectionBuilder.Clear()
$credential = $null
$securePassword.Dispose()
Write-Host 'TFMS connections configured for this PowerShell terminal. Run the API from this same terminal.'
