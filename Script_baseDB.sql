CREATE DATABASE MairieDB;
GO
USE MairieDB;
GO

-- Table des demandes
CREATE TABLE Demande (
    Id INT IDENTITY PRIMARY KEY,
    NomCitoyen NVARCHAR(100) NOT NULL,
    TypeDemande NVARCHAR(100) NOT NULL,
    Statut NVARCHAR(50) NOT NULL,
    DateCreation DATETIME2 NOT NULL DEFAULT GETDATE()
);
GO

-- Table des rôles utilisateurs
CREATE TABLE UserRoles (
    WindowsId NVARCHAR(200) PRIMARY KEY,
    Role NVARCHAR(50) NOT NULL
);
GO

-- Table d’audit interne
CREATE TABLE AuditLog (
    Id INT IDENTITY PRIMARY KEY,
    WindowsId NVARCHAR(200) NOT NULL,
    Action NVARCHAR(200) NOT NULL,
    DateAction DATETIME2 NOT NULL DEFAULT GETDATE(),
    Resultat NVARCHAR(50) NOT NULL
);
GO

-- Exemple d’initialisation
INSERT INTO UserRoles (WindowsId, Role) VALUES
('MAIRIE\\agent1', 'Agent'),
('MAIRIE\\chef1', 'ChefService'),
('MAIRIE\\admin1', 'Administrateur');
GO
