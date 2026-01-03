

use grocery_management;

	

SET IDENTITY_INSERT Users ON;

    INSERT INTO Users (
        UserId, FirmId, FirmType, UserName, PasswordHash, Email,
        EmailConfirmed, MobileNumber, MobileNumberConfirmed,
        AccessFailedCount, IsActive, LastLoginAt, SecurityStamp,
        LockoutEnd, LockoutEnabled, TwoFactorEnabled, CreatedAt,
        UpdatedAt, IsDeleted
    )
    VALUES (
        1, 1, 'admin', 'admin',
        '$2a$11$hRI1XAuyl6O5N7qT2z1xvecbDi8bwkQryh.WLu5KZ8Cvx19SPj6zm',
        'admin@defaultfirm.com', 1, NULL, 0, 0, 1,
        '2025-10-31 08:27:54.453', '501B4420-BF03-4BE8-AD0A-F0522CC811BF',
        NULL, 1, 0, '2025-10-12 10:17:47.690', NULL, 0
    );

    SET IDENTITY_INSERT Users OFF;


	
    SET IDENTITY_INSERT Roles ON;

	INSERT INTO Roles (RoleId, RoleName)
	VALUES (1, 'Super-Admin'),
	(2, 'Firm-Admin');
	 
	SET IDENTITY_INSERT Roles OFF;

	SET IDENTITY_INSERT UserRoles ON;
	INSERT INTO UserRoles (UserRoleId, UserId, RoleId)
	VALUES (1, 1, 1);
	SET IDENTITY_INSERT UserRoles OFF;