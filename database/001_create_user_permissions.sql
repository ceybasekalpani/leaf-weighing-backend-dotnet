USE [Setup_tbl_Kandedola];
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.UserSetup')
      AND name = N'UX_UserSetup_Ind'
)
BEGIN
    CREATE UNIQUE INDEX UX_UserSetup_Ind ON dbo.UserSetup (Ind);
END;
GO

IF OBJECT_ID(N'dbo.UserPermissions', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.UserPermissions
    (
        UserInd INT NOT NULL,
        IsCanAddDeduction BIT NOT NULL CONSTRAINT DF_UserPermissions_IsCanAddDeduction DEFAULT (1),
        IsCanAddCount BIT NOT NULL CONSTRAINT DF_UserPermissions_IsCanAddCount DEFAULT (1),
        CONSTRAINT PK_UserPermissions PRIMARY KEY (UserInd),
        CONSTRAINT FK_UserPermissions_UserSetup
            FOREIGN KEY (UserInd) REFERENCES dbo.UserSetup (Ind)
            ON DELETE CASCADE
    );
END;
GO

INSERT INTO dbo.UserPermissions (UserInd, IsCanAddDeduction, IsCanAddCount)
SELECT users.Ind, 1, 1
FROM dbo.UserSetup AS users
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.UserPermissions AS permissions
    WHERE permissions.UserInd = users.Ind
);
GO
