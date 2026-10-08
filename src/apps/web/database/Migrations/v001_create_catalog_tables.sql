-- =====================================================
-- Migration: v001_create_catalog_tables.sql
-- Feature: REQ-01 Catálogos globales
-- Author: agentsky
-- Date: 2026-10-08
-- Description: Crea esquemas y tablas de catálogos globales y auditoría
-- =====================================================

IF SCHEMA_ID(N'Catalogs') IS NULL EXEC(N'CREATE SCHEMA Catalogs');
IF SCHEMA_ID(N'Audit') IS NULL EXEC(N'CREATE SCHEMA Audit');
GO

IF OBJECT_ID(N'Catalogs.Company', N'U') IS NULL
BEGIN
    CREATE TABLE Catalogs.Company
    (
        Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Company PRIMARY KEY,
        Code NVARCHAR(150) NOT NULL,
        Description NVARCHAR(250) NOT NULL,
        CreatedAt DATETIME2(3) NOT NULL CONSTRAINT DF_Company_CreatedAt DEFAULT SYSUTCDATETIME(),
        ModifiedAt DATETIME2(3) NOT NULL CONSTRAINT DF_Company_ModifiedAt DEFAULT SYSUTCDATETIME(),
        RowVersion ROWVERSION
    );

    CREATE UNIQUE INDEX UX_Company_Code ON Catalogs.Company (Code);
    CREATE UNIQUE INDEX UX_Company_Description ON Catalogs.Company (Description);
END;
GO

IF OBJECT_ID(N'Catalogs.Format', N'U') IS NULL
BEGIN
    CREATE TABLE Catalogs.Format
    (
        Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Format PRIMARY KEY,
        Code NVARCHAR(150) NOT NULL,
        Description NVARCHAR(250) NOT NULL,
        CreatedAt DATETIME2(3) NOT NULL CONSTRAINT DF_Format_CreatedAt DEFAULT SYSUTCDATETIME(),
        ModifiedAt DATETIME2(3) NOT NULL CONSTRAINT DF_Format_ModifiedAt DEFAULT SYSUTCDATETIME(),
        RowVersion ROWVERSION
    );

    CREATE UNIQUE INDEX UX_Format_Code ON Catalogs.Format (Code);
    CREATE UNIQUE INDEX UX_Format_Description ON Catalogs.Format (Description);
END;
GO

IF OBJECT_ID(N'Catalogs.Discipline', N'U') IS NULL
BEGIN
    CREATE TABLE Catalogs.Discipline
    (
        Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Discipline PRIMARY KEY,
        Code NVARCHAR(150) NOT NULL,
        Description NVARCHAR(250) NOT NULL,
        CreatedAt DATETIME2(3) NOT NULL CONSTRAINT DF_Discipline_CreatedAt DEFAULT SYSUTCDATETIME(),
        ModifiedAt DATETIME2(3) NOT NULL CONSTRAINT DF_Discipline_ModifiedAt DEFAULT SYSUTCDATETIME(),
        RowVersion ROWVERSION
    );

    CREATE UNIQUE INDEX UX_Discipline_Code ON Catalogs.Discipline (Code);
    CREATE UNIQUE INDEX UX_Discipline_Description ON Catalogs.Discipline (Description);
END;
GO

IF OBJECT_ID(N'Audit.CatalogChanges', N'U') IS NULL
BEGIN
    CREATE TABLE Audit.CatalogChanges
    (
        Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_CatalogChanges PRIMARY KEY,
        EntityType NVARCHAR(50) NOT NULL,
        EntityId UNIQUEIDENTIFIER NOT NULL,
        Action NVARCHAR(20) NOT NULL,
        UserId NVARCHAR(200) NULL,
        Timestamp DATETIME2(3) NOT NULL CONSTRAINT DF_CatalogChanges_Timestamp DEFAULT SYSUTCDATETIME(),
        OldValue NVARCHAR(4000) NULL,
        NewValue NVARCHAR(4000) NULL
    );

    CREATE INDEX IX_CatalogChanges_Entity ON Audit.CatalogChanges (EntityType, EntityId);
    CREATE INDEX IX_CatalogChanges_Timestamp ON Audit.CatalogChanges (Timestamp);
END;
GO
