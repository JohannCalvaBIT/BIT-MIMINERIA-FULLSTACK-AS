-- =====================================================
-- Rollback: v001_rollback.sql
-- Feature: REQ-01 Catálogos globales
-- Author: agentsky
-- Date: 2026-10-08
-- Description: Revierte v001_create_catalog_tables.sql
-- =====================================================

IF OBJECT_ID(N'Audit.CatalogChanges', N'U') IS NOT NULL
    DROP TABLE Audit.CatalogChanges;
GO

IF OBJECT_ID(N'Catalogs.Discipline', N'U') IS NOT NULL
    DROP TABLE Catalogs.Discipline;
GO

IF OBJECT_ID(N'Catalogs.Format', N'U') IS NOT NULL
    DROP TABLE Catalogs.Format;
GO

IF OBJECT_ID(N'Catalogs.Company', N'U') IS NOT NULL
    DROP TABLE Catalogs.Company;
GO

IF SCHEMA_ID(N'Catalogs') IS NOT NULL
    DROP SCHEMA Catalogs;
GO

IF SCHEMA_ID(N'Audit') IS NOT NULL
    DROP SCHEMA Audit;
GO
