
CREATE DATABASE grocery_management;

use grocery_management;

CREATE TABLE Firms
(
    FirmId INT IDENTITY PRIMARY KEY,
    FirmName VARCHAR(100) NOT NULL,
    FirmCode VARCHAR(20) UNIQUE,
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME NULL,
	IsDeleted BIT DEFAULT 0
);

CREATE TABLE Users (
    UserId INT IDENTITY(1,1) PRIMARY KEY,
    FirmId INT NULL,
	FirmType NVARCHAR(20) NULL,		-- Admin, Mill, Company
    UserName VARCHAR(30) UNIQUE NOT NULL,
    PasswordHash VARCHAR(255) NOT NULL,  -- Store only hashed passwords (updated length)
    Email VARCHAR(255) NULL,             -- Increased length for modern emails
    EmailConfirmed BIT DEFAULT 0,
    MobileNumber VARCHAR(20) NULL,       -- Standardized length
    MobileNumberConfirmed BIT DEFAULT 0,
    AccessFailedCount TINYINT DEFAULT 0,
    IsActive BIT DEFAULT 1,
	LastLoginAt DATETIME NULL,
    SecurityStamp UNIQUEIDENTIFIER DEFAULT NEWID(),  -- Forces logout on critical changes
    LockoutEnd DATETIME NULL,            -- Account lockout expiration
    LockoutEnabled BIT DEFAULT 1,        -- Enable account lockout
    TwoFactorEnabled BIT DEFAULT 0,      -- For 2FA support
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME NULL,
    IsDeleted BIT DEFAULT 0,
    
    -- Constraints

    CONSTRAINT CHK_UserName_Format CHECK (UserName LIKE '[a-zA-Z0-9@_\-.]%'),
    CONSTRAINT CHK_Email_Format CHECK (Email IS NULL OR Email LIKE '%_@__%.__%'),
    CONSTRAINT CHK_MobileNumber_Format CHECK (
        MobileNumber IS NULL OR 
        (MobileNumber LIKE '[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]' 
        AND LEN(MobileNumber) = 10))
);


CREATE TABLE UserSessions (
    SessionId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId INT NOT NULL,
	Jti NVARCHAR(450) NULL,
    AccessToken VARCHAR(MAX) NULL,           -- Can store JWT if needed (usually not recommended)
    RefreshToken VARCHAR(255) NOT NULL,      -- Hashed refresh token
    RefreshTokenExpiry DATETIME NOT NULL,
    DeviceId VARCHAR(255) NULL,              -- For device fingerprinting
    IPAddress VARCHAR(50) NULL,
    UserAgent VARCHAR(512) NULL,             -- Full browser/device info
    CreatedAt DATETIME DEFAULT GETDATE(),
    LastRefreshedAt DATETIME NULL,
    IsRevoked BIT DEFAULT 0,
    RevokedAt DATETIME NULL,
    RevokedReason VARCHAR(100) NULL,
    
    -- Foreign key with no cascade to preserve sessions on user deletion
    -- CONSTRAINT FK_UserSessions_Users FOREIGN KEY (UserId) 
    --    REFERENCES Users(UserId),
    
    -- Indexes for performance
    INDEX IX_UserSessions_UserId NONCLUSTERED (UserId),
    INDEX IX_UserSessions_RefreshToken NONCLUSTERED (RefreshToken)
);


CREATE TABLE Roles
(
	RoleId SMALLINT IDENTITY PRIMARY KEY,
	RoleName VARCHAR(50),
    IsActive BIT DEFAULT 1,
    IsDeleted BIT DEFAULT 0
);

CREATE TABLE UserRoles
(
    UserRoleId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    RoleId SMALLINT NOT NULL,
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME NULL,
    IsActive BIT DEFAULT 1,
    IsDeleted BIT DEFAULT 0
   
    -- Foreign key constraints
  --  CONSTRAINT FK_UserRoles_Users FOREIGN KEY (UserId)  REFERENCES Users(UserId),
  --  CONSTRAINT FK_UserRoles_Roles FOREIGN KEY (RoleId)  REFERENCES Roles(RoleId),
        
    -- Composite unique constraint
    CONSTRAINT UQ_User_Role UNIQUE (UserId, RoleId)
);
