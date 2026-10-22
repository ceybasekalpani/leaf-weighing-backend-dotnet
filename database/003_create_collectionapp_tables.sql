-- Executed by Connect-Tfms.ps1 inside the same transaction as the data copy.
-- Column definitions match Setup_tbl_Kandedola inspected on 2026-10-20.
IF DB_NAME() <> N'Setup_tbl_TFMS'
    THROW 50001, 'Run this script only against Setup_tbl_TFMS.', 1;

-- Refuse to replace or merge existing destination users/permissions.
IF OBJECT_ID(N'CollectionApp.UserSetup') IS NOT NULL
   OR OBJECT_ID(N'CollectionApp.UserPermissions') IS NOT NULL
    THROW 50002, 'CollectionApp tables already exist. No data was changed; inspect them before migrating again.', 1;

IF SCHEMA_ID(N'CollectionApp') IS NULL
    EXEC(N'CREATE SCHEMA [CollectionApp] AUTHORIZATION [dbo]');

CREATE TABLE [CollectionApp].[UserSetup]
(
    [Ind] INT IDENTITY(1,1) NOT NULL,
    [RegDate] DATETIME NOT NULL DEFAULT (GETDATE()),
    [FullName] NVARCHAR(70) NOT NULL DEFAULT (N'none'),
    [UserName] NVARCHAR(50) NOT NULL DEFAULT (N'none'),
    [Password] NVARCHAR(30) NULL,
    [Admin] BIT NOT NULL DEFAULT (0),
    [AdminLevel] INT NOT NULL DEFAULT (0),
    [Active] BIT NOT NULL DEFAULT (0),
    [TempWorker] BIT NOT NULL DEFAULT (0),
    [Bl_Confirm] BIT NOT NULL DEFAULT (0),
    [Bl_Report] BIT NOT NULL DEFAULT (0),
    [Bl_Transfer] BIT NOT NULL DEFAULT (0),
    [Bl_LeafEditDel] BIT NOT NULL DEFAULT (0),
    [BackDays] INT NOT NULL DEFAULT (0),
    CONSTRAINT [PK_CollectionApp_UserSetup] PRIMARY KEY ([Ind])
);

CREATE TABLE [CollectionApp].[UserPermissions]
(
    [UserInd] INT NOT NULL,
    [IsCanAddDeduction] BIT NOT NULL DEFAULT (1),
    [IsCanAddCount] BIT NOT NULL DEFAULT (1),
    CONSTRAINT [PK_CollectionApp_UserPermissions] PRIMARY KEY ([UserInd]),
    CONSTRAINT [FK_CollectionApp_UserPermissions_UserSetup]
        FOREIGN KEY ([UserInd]) REFERENCES [CollectionApp].[UserSetup] ([Ind])
        ON DELETE CASCADE
);
