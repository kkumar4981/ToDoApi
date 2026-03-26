SET ANSI_NULLS ON;
GO

SET QUOTED_IDENTIFIER ON;
GO

IF DB_ID(N'ToDoApiDb') IS NULL
BEGIN
    CREATE DATABASE [ToDoApiDb];
END
GO

USE [ToDoApiDb];
GO

IF OBJECT_ID(N'dbo.IdentityUsers', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.IdentityUsers
    (
        Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_IdentityUsers PRIMARY KEY,
        UserName NVARCHAR(256) NULL,
        NormalizedUserName NVARCHAR(256) NULL,
        Email NVARCHAR(256) NULL,
        NormalizedEmail NVARCHAR(256) NULL,
        EmailConfirmed BIT NOT NULL CONSTRAINT DF_IdentityUsers_EmailConfirmed DEFAULT 0,
        PasswordHash NVARCHAR(512) NULL,
        SecurityStamp NVARCHAR(100) NULL,
        ConcurrencyStamp NVARCHAR(100) NULL,
        PhoneNumber NVARCHAR(50) NULL,
        PhoneNumberConfirmed BIT NOT NULL CONSTRAINT DF_IdentityUsers_PhoneConfirmed DEFAULT 0,
        TwoFactorEnabled BIT NOT NULL CONSTRAINT DF_IdentityUsers_TwoFactor DEFAULT 0,
        LockoutEnd DATETIMEOFFSET NULL,
        LockoutEnabled BIT NOT NULL CONSTRAINT DF_IdentityUsers_LockoutEnabled DEFAULT 1,
        AccessFailedCount INT NOT NULL CONSTRAINT DF_IdentityUsers_AccessFailed DEFAULT 0,
        CreatedAtUtc DATETIME2 NOT NULL
    );
END
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = N'UX_IdentityUsers_NormalizedUserName'
      AND object_id = OBJECT_ID(N'dbo.IdentityUsers')
)
BEGIN
    CREATE UNIQUE INDEX UX_IdentityUsers_NormalizedUserName
        ON dbo.IdentityUsers(NormalizedUserName)
        WHERE NormalizedUserName IS NOT NULL;
END
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = N'UX_IdentityUsers_NormalizedEmail'
      AND object_id = OBJECT_ID(N'dbo.IdentityUsers')
)
BEGIN
    CREATE UNIQUE INDEX UX_IdentityUsers_NormalizedEmail
        ON dbo.IdentityUsers(NormalizedEmail)
        WHERE NormalizedEmail IS NOT NULL;
END
GO

IF OBJECT_ID(N'dbo.IdentityRoles', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.IdentityRoles
    (
        Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_IdentityRoles PRIMARY KEY,
        Name NVARCHAR(256) NULL,
        NormalizedName NVARCHAR(256) NULL,
        ConcurrencyStamp NVARCHAR(100) NULL
    );
END
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = N'UX_IdentityRoles_NormalizedName'
      AND object_id = OBJECT_ID(N'dbo.IdentityRoles')
)
BEGIN
    CREATE UNIQUE INDEX UX_IdentityRoles_NormalizedName
        ON dbo.IdentityRoles(NormalizedName)
        WHERE NormalizedName IS NOT NULL;
END
GO

IF OBJECT_ID(N'dbo.IdentityUserRoles', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.IdentityUserRoles
    (
        UserId UNIQUEIDENTIFIER NOT NULL,
        RoleId UNIQUEIDENTIFIER NOT NULL,
        CONSTRAINT PK_IdentityUserRoles PRIMARY KEY (UserId, RoleId),
        CONSTRAINT FK_IdentityUserRoles_Users FOREIGN KEY (UserId) REFERENCES dbo.IdentityUsers(Id) ON DELETE CASCADE,
        CONSTRAINT FK_IdentityUserRoles_Roles FOREIGN KEY (RoleId) REFERENCES dbo.IdentityRoles(Id) ON DELETE CASCADE
    );
END
GO

IF OBJECT_ID(N'dbo.RefreshTokens', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.RefreshTokens
    (
        Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_RefreshTokens PRIMARY KEY,
        UserId UNIQUEIDENTIFIER NOT NULL,
        Token NVARCHAR(200) NOT NULL,
        SecurityStamp NVARCHAR(100) NOT NULL,
        ExpiresAtUtc DATETIME2 NOT NULL,
        CreatedAtUtc DATETIME2 NOT NULL,
        RevokedAtUtc DATETIME2 NULL,
        CreatedByIp NVARCHAR(45) NULL,
        RevokedByIp NVARCHAR(45) NULL,
        CONSTRAINT FK_RefreshTokens_Users FOREIGN KEY (UserId) REFERENCES dbo.IdentityUsers(Id) ON DELETE CASCADE
    );
END
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = N'UX_RefreshTokens_Token'
      AND object_id = OBJECT_ID(N'dbo.RefreshTokens')
)
BEGIN
    CREATE UNIQUE INDEX UX_RefreshTokens_Token ON dbo.RefreshTokens(Token);
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_IdentityUsers_GetById
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        Id,
        UserName,
        NormalizedUserName,
        Email,
        NormalizedEmail,
        EmailConfirmed,
        PasswordHash,
        SecurityStamp,
        ConcurrencyStamp,
        PhoneNumber,
        PhoneNumberConfirmed,
        TwoFactorEnabled,
        LockoutEnd,
        LockoutEnabled,
        AccessFailedCount,
        CreatedAtUtc
    FROM dbo.IdentityUsers
    WHERE Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_IdentityUsers_GetByNormalizedUserName
    @NormalizedUserName NVARCHAR(256)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        Id,
        UserName,
        NormalizedUserName,
        Email,
        NormalizedEmail,
        EmailConfirmed,
        PasswordHash,
        SecurityStamp,
        ConcurrencyStamp,
        PhoneNumber,
        PhoneNumberConfirmed,
        TwoFactorEnabled,
        LockoutEnd,
        LockoutEnabled,
        AccessFailedCount,
        CreatedAtUtc
    FROM dbo.IdentityUsers
    WHERE NormalizedUserName = @NormalizedUserName;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_IdentityUsers_GetByNormalizedEmail
    @NormalizedEmail NVARCHAR(256)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        Id,
        UserName,
        NormalizedUserName,
        Email,
        NormalizedEmail,
        EmailConfirmed,
        PasswordHash,
        SecurityStamp,
        ConcurrencyStamp,
        PhoneNumber,
        PhoneNumberConfirmed,
        TwoFactorEnabled,
        LockoutEnd,
        LockoutEnabled,
        AccessFailedCount,
        CreatedAtUtc
    FROM dbo.IdentityUsers
    WHERE NormalizedEmail = @NormalizedEmail;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_IdentityUsers_List
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        Id,
        UserName,
        NormalizedUserName,
        Email,
        NormalizedEmail,
        EmailConfirmed,
        PasswordHash,
        SecurityStamp,
        ConcurrencyStamp,
        PhoneNumber,
        PhoneNumberConfirmed,
        TwoFactorEnabled,
        LockoutEnd,
        LockoutEnabled,
        AccessFailedCount,
        CreatedAtUtc
    FROM dbo.IdentityUsers
    ORDER BY CreatedAtUtc DESC;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_IdentityUsers_Create
    @Id UNIQUEIDENTIFIER,
    @UserName NVARCHAR(256) = NULL,
    @NormalizedUserName NVARCHAR(256) = NULL,
    @Email NVARCHAR(256) = NULL,
    @NormalizedEmail NVARCHAR(256) = NULL,
    @EmailConfirmed BIT,
    @PasswordHash NVARCHAR(512) = NULL,
    @SecurityStamp NVARCHAR(100) = NULL,
    @ConcurrencyStamp NVARCHAR(100) = NULL,
    @PhoneNumber NVARCHAR(50) = NULL,
    @PhoneNumberConfirmed BIT,
    @TwoFactorEnabled BIT,
    @LockoutEnd DATETIMEOFFSET = NULL,
    @LockoutEnabled BIT,
    @AccessFailedCount INT,
    @CreatedAtUtc DATETIME2
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.IdentityUsers
    (
        Id,
        UserName,
        NormalizedUserName,
        Email,
        NormalizedEmail,
        EmailConfirmed,
        PasswordHash,
        SecurityStamp,
        ConcurrencyStamp,
        PhoneNumber,
        PhoneNumberConfirmed,
        TwoFactorEnabled,
        LockoutEnd,
        LockoutEnabled,
        AccessFailedCount,
        CreatedAtUtc
    )
    VALUES
    (
        @Id,
        @UserName,
        @NormalizedUserName,
        @Email,
        @NormalizedEmail,
        @EmailConfirmed,
        @PasswordHash,
        @SecurityStamp,
        @ConcurrencyStamp,
        @PhoneNumber,
        @PhoneNumberConfirmed,
        @TwoFactorEnabled,
        @LockoutEnd,
        @LockoutEnabled,
        @AccessFailedCount,
        @CreatedAtUtc
    );
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_IdentityUsers_Update
    @Id UNIQUEIDENTIFIER,
    @UserName NVARCHAR(256) = NULL,
    @NormalizedUserName NVARCHAR(256) = NULL,
    @Email NVARCHAR(256) = NULL,
    @NormalizedEmail NVARCHAR(256) = NULL,
    @EmailConfirmed BIT,
    @PasswordHash NVARCHAR(512) = NULL,
    @SecurityStamp NVARCHAR(100) = NULL,
    @ConcurrencyStamp NVARCHAR(100) = NULL,
    @PhoneNumber NVARCHAR(50) = NULL,
    @PhoneNumberConfirmed BIT,
    @TwoFactorEnabled BIT,
    @LockoutEnd DATETIMEOFFSET = NULL,
    @LockoutEnabled BIT,
    @AccessFailedCount INT,
    @CreatedAtUtc DATETIME2
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.IdentityUsers
    SET
        UserName = @UserName,
        NormalizedUserName = @NormalizedUserName,
        Email = @Email,
        NormalizedEmail = @NormalizedEmail,
        EmailConfirmed = @EmailConfirmed,
        PasswordHash = @PasswordHash,
        SecurityStamp = @SecurityStamp,
        ConcurrencyStamp = @ConcurrencyStamp,
        PhoneNumber = @PhoneNumber,
        PhoneNumberConfirmed = @PhoneNumberConfirmed,
        TwoFactorEnabled = @TwoFactorEnabled,
        LockoutEnd = @LockoutEnd,
        LockoutEnabled = @LockoutEnabled,
        AccessFailedCount = @AccessFailedCount,
        CreatedAtUtc = @CreatedAtUtc
    WHERE Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_IdentityUsers_Delete
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM dbo.IdentityUsers
    WHERE Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_IdentityRoles_GetById
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT Id, Name, NormalizedName, ConcurrencyStamp
    FROM dbo.IdentityRoles
    WHERE Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_IdentityRoles_GetByNormalizedName
    @NormalizedName NVARCHAR(256)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT Id, Name, NormalizedName, ConcurrencyStamp
    FROM dbo.IdentityRoles
    WHERE NormalizedName = @NormalizedName;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_IdentityRoles_List
AS
BEGIN
    SET NOCOUNT ON;

    SELECT Id, Name, NormalizedName, ConcurrencyStamp
    FROM dbo.IdentityRoles
    ORDER BY Name;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_IdentityRoles_Create
    @Id UNIQUEIDENTIFIER,
    @Name NVARCHAR(256) = NULL,
    @NormalizedName NVARCHAR(256) = NULL,
    @ConcurrencyStamp NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.IdentityRoles (Id, Name, NormalizedName, ConcurrencyStamp)
    VALUES (@Id, @Name, @NormalizedName, @ConcurrencyStamp);
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_IdentityRoles_Update
    @Id UNIQUEIDENTIFIER,
    @Name NVARCHAR(256) = NULL,
    @NormalizedName NVARCHAR(256) = NULL,
    @ConcurrencyStamp NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.IdentityRoles
    SET
        Name = @Name,
        NormalizedName = @NormalizedName,
        ConcurrencyStamp = @ConcurrencyStamp
    WHERE Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_IdentityRoles_Delete
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM dbo.IdentityRoles
    WHERE Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_IdentityUserRoles_GetRoleNamesByUserId
    @UserId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT r.Name
    FROM dbo.IdentityUserRoles ur
    INNER JOIN dbo.IdentityRoles r ON r.Id = ur.RoleId
    WHERE ur.UserId = @UserId
    ORDER BY r.Name;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_IdentityUserRoles_Add
    @UserId UNIQUEIDENTIFIER,
    @NormalizedRoleName NVARCHAR(256)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @RoleId UNIQUEIDENTIFIER;
    SELECT @RoleId = Id
    FROM dbo.IdentityRoles
    WHERE NormalizedName = @NormalizedRoleName;

    IF @RoleId IS NULL
    BEGIN
        THROW 50000, 'Role not found.', 1;
    END;

    IF NOT EXISTS
    (
        SELECT 1
        FROM dbo.IdentityUserRoles
        WHERE UserId = @UserId AND RoleId = @RoleId
    )
    BEGIN
        INSERT INTO dbo.IdentityUserRoles (UserId, RoleId)
        VALUES (@UserId, @RoleId);
    END
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_IdentityUserRoles_Remove
    @UserId UNIQUEIDENTIFIER,
    @NormalizedRoleName NVARCHAR(256)
AS
BEGIN
    SET NOCOUNT ON;

    DELETE ur
    FROM dbo.IdentityUserRoles ur
    INNER JOIN dbo.IdentityRoles r ON r.Id = ur.RoleId
    WHERE ur.UserId = @UserId
      AND r.NormalizedName = @NormalizedRoleName;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_IdentityUserRoles_IsInRole
    @UserId UNIQUEIDENTIFIER,
    @NormalizedRoleName NVARCHAR(256)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT CAST(CASE WHEN EXISTS
    (
        SELECT 1
        FROM dbo.IdentityUserRoles ur
        INNER JOIN dbo.IdentityRoles r ON r.Id = ur.RoleId
        WHERE ur.UserId = @UserId
          AND r.NormalizedName = @NormalizedRoleName
    )
    THEN 1 ELSE 0 END AS BIT);
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_IdentityUserRoles_GetUsersInRole
    @NormalizedRoleName NVARCHAR(256)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        u.Id,
        u.UserName,
        u.NormalizedUserName,
        u.Email,
        u.NormalizedEmail,
        u.EmailConfirmed,
        u.PasswordHash,
        u.SecurityStamp,
        u.ConcurrencyStamp,
        u.PhoneNumber,
        u.PhoneNumberConfirmed,
        u.TwoFactorEnabled,
        u.LockoutEnd,
        u.LockoutEnabled,
        u.AccessFailedCount,
        u.CreatedAtUtc
    FROM dbo.IdentityUserRoles ur
    INNER JOIN dbo.IdentityRoles r ON r.Id = ur.RoleId
    INNER JOIN dbo.IdentityUsers u ON u.Id = ur.UserId
    WHERE r.NormalizedName = @NormalizedRoleName;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_RefreshTokens_Create
    @Id UNIQUEIDENTIFIER,
    @UserId UNIQUEIDENTIFIER,
    @Token NVARCHAR(200),
    @SecurityStamp NVARCHAR(100),
    @ExpiresAtUtc DATETIME2,
    @CreatedAtUtc DATETIME2,
    @CreatedByIp NVARCHAR(45) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.RefreshTokens
    (
        Id,
        UserId,
        Token,
        SecurityStamp,
        ExpiresAtUtc,
        CreatedAtUtc,
        CreatedByIp
    )
    VALUES
    (
        @Id,
        @UserId,
        @Token,
        @SecurityStamp,
        @ExpiresAtUtc,
        @CreatedAtUtc,
        @CreatedByIp
    );
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_RefreshTokens_GetByToken
    @Token NVARCHAR(200)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        Id,
        UserId,
        Token,
        SecurityStamp,
        ExpiresAtUtc,
        CreatedAtUtc,
        RevokedAtUtc,
        CreatedByIp,
        RevokedByIp
    FROM dbo.RefreshTokens
    WHERE Token = @Token;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_RefreshTokens_Revoke
    @Id UNIQUEIDENTIFIER,
    @RevokedAtUtc DATETIME2,
    @RevokedByIp NVARCHAR(45) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.RefreshTokens
    SET
        RevokedAtUtc = @RevokedAtUtc,
        RevokedByIp = @RevokedByIp
    WHERE Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_RefreshTokens_RevokeAllForUser
    @UserId UNIQUEIDENTIFIER,
    @RevokedAtUtc DATETIME2,
    @RevokedByIp NVARCHAR(45) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.RefreshTokens
    SET
        RevokedAtUtc = @RevokedAtUtc,
        RevokedByIp = @RevokedByIp
    WHERE UserId = @UserId
      AND RevokedAtUtc IS NULL;
END
GO
