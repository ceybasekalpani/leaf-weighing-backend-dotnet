USE [Setup_tbl_Kandedola];
GO

IF OBJECT_ID(N'dbo.UserPermissions', N'U') IS NOT NULL
BEGIN
    DECLARE @DefaultsAlreadyTrue BIT = CASE WHEN
        EXISTS
        (
            SELECT 1
            FROM sys.default_constraints AS defaults
            INNER JOIN sys.columns AS columns
                ON columns.object_id = defaults.parent_object_id
               AND columns.column_id = defaults.parent_column_id
            WHERE defaults.parent_object_id = OBJECT_ID(N'dbo.UserPermissions')
              AND columns.name = N'IsCanAddDeduction'
              AND defaults.definition IN (N'(1)', N'((1))')
        )
        AND EXISTS
        (
            SELECT 1
            FROM sys.default_constraints AS defaults
            INNER JOIN sys.columns AS columns
                ON columns.object_id = defaults.parent_object_id
               AND columns.column_id = defaults.parent_column_id
            WHERE defaults.parent_object_id = OBJECT_ID(N'dbo.UserPermissions')
              AND columns.name = N'IsCanAddCount'
              AND defaults.definition IN (N'(1)', N'((1))')
        )
        THEN 1 ELSE 0 END;

    IF OBJECT_ID(N'dbo.DF_UserPermissions_IsCanAddDeduction', N'D') IS NOT NULL
        ALTER TABLE dbo.UserPermissions DROP CONSTRAINT DF_UserPermissions_IsCanAddDeduction;

    IF OBJECT_ID(N'dbo.DF_UserPermissions_IsCanAddCount', N'D') IS NOT NULL
        ALTER TABLE dbo.UserPermissions DROP CONSTRAINT DF_UserPermissions_IsCanAddCount;

    ALTER TABLE dbo.UserPermissions
        ADD CONSTRAINT DF_UserPermissions_IsCanAddDeduction
        DEFAULT (1) FOR IsCanAddDeduction;

    ALTER TABLE dbo.UserPermissions
        ADD CONSTRAINT DF_UserPermissions_IsCanAddCount
        DEFAULT (1) FOR IsCanAddCount;

    IF @DefaultsAlreadyTrue = 0
    BEGIN
        UPDATE dbo.UserPermissions
        SET IsCanAddDeduction = 1,
            IsCanAddCount = 1;
    END;

    INSERT INTO dbo.UserPermissions (UserInd, IsCanAddDeduction, IsCanAddCount)
    SELECT users.Ind, 1, 1
    FROM dbo.UserSetup AS users
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.UserPermissions AS permissions
        WHERE permissions.UserInd = users.Ind
    );
END;
GO
