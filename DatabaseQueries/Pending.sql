CREATE TABLE Firms
(
    FirmId INT IDENTITY(1,1) PRIMARY KEY,

    FirmName NVARCHAR(100) NOT NULL,
    FirmCode NVARCHAR(50) NOT NULL UNIQUE,

    Address NVARCHAR(255) NULL,
    ContactNumber NVARCHAR(20) NULL,
    ContactPerson NVARCHAR(100) NULL,
    GstNumber NVARCHAR(30) NULL,
    LogoImagePath NVARCHAR(500) NULL,

    IsActive BIT NOT NULL DEFAULT 1,

    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME NULL,
    IsDeleted BIT NOT NULL DEFAULT 0
);

CREATE TABLE Categories
(
    CategoryId INT IDENTITY(1,1) PRIMARY KEY,
    FirmId INT NOT NULL,  
    CategoryName NVARCHAR(100) NOT NULL,
    ParentCategoryId INT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME NULL,
	IsDeleted BIT NOT NULL DEFAULT 0,
    
);

CREATE TABLE Products
(
    ProductId INT IDENTITY(1,1) PRIMARY KEY,
    FirmId INT NOT NULL,
    CategoryId INT NOT NULL,
    ProductName NVARCHAR(150) NOT NULL,
    Barcode NVARCHAR(50) NULL,
    Unit NVARCHAR(20) NOT NULL,        -- kg, gm, ltr, pcs
    IsLooseItem BIT NOT NULL DEFAULT 0,
    MRP DECIMAL(10,2) NOT NULL,
    SalePrice DECIMAL(10,2) NOT NULL,
    GSTPercent DECIMAL(5,2) NOT NULL DEFAULT 0,
	LowStockAlert INT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME NULL   

    -- CONSTRAINT FK_Products_Firms FOREIGN KEY (FirmId)REFERENCES Firms(FirmId),
    -- CONSTRAINT FK_Products_Categories FOREIGN KEY (CategoryId) REFERENCES Categories(CategoryId),
    -- CONSTRAINT CHK_Product_Prices CHECK (SalePrice <= MRP),
    -- CONSTRAINT CHK_GST_Percent CHECK (GSTPercent >= 0 AND GSTPercent <= 100),
    -- CONSTRAINT UQ_Product_Barcode_Firm UNIQUE (FirmId, Barcode)
);
CREATE TABLE ProductImages
(
    ProductImageId INT IDENTITY(1,1) PRIMARY KEY,
    FirmId INT NOT NULL,         
    ProductId INT NOT NULL,
    ImageUrl NVARCHAR(500) NOT NULL,
    IsPrimary BIT NOT NULL DEFAULT 0,
    SortOrder INT NOT NULL DEFAULT 0,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME NULL,
    DeletedAt DATETIME NULL

    --CONSTRAINT FK_ProductImages_Firms FOREIGN KEY (FirmId) REFERENCES Firms(FirmId),

    -- CONSTRAINT FK_ProductImages_Products FOREIGN KEY (ProductId) REFERENCES Products(ProductId),

    -- CONSTRAINT UQ_Product_PrimaryImage UNIQUE (ProductId, IsPrimary) WHERE IsPrimary = 1
);


CREATE TABLE ProductBatches
(
    BatchId INT IDENTITY PRIMARY KEY,
    FirmId INT NULL,
    ProductId INT NOT NULL,
    BatchNumber NVARCHAR(50) NULL,
    MRP DECIMAL(10,2) NOT NULL,
    CostPrice DECIMAL(10,2) NOT NULL,
    SalePrice DECIMAL(10,2) NOT NULL,
    Quantity DECIMAL(18,3) NOT NULL,
    RemainingQty DECIMAL(18,3) NOT NULL,
    ManufactureDate DATE NULL,
    ExpiryDate DATE NULL,
	IsDeleted BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME DEFAULT GETDATE(),
	UpdatedAt DATETIME NULL,
    DeletedAt DATETIME NULL
	    --CONSTRAINT FK_ProductBatches_Firms FOREIGN KEY (FirmId) REFERENCES Firms(FirmId),
		-- CONSTRAINT FK_ProductBatches_Products FOREIGN KEY (ProductId) REFERENCES Products(ProductId),

);
CREATE TABLE StockTransactions
(
    TransactionId INT IDENTITY PRIMARY KEY,
    FirmId INT NULL,
    ProductId INT NOT NULL,
    BatchId INT NULL,
    TransactionType VARCHAR(20) NOT NULL,
    Quantity DECIMAL(18,3) NOT NULL,
    IsIncrease BIT NOT NULL,
    ReferenceId INT NULL,
    ReferenceType VARCHAR(20) NULL,
    Notes NVARCHAR(500),
    CreatedBy INT NULL,
	CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME NULL,
    DeletedAt DATETIME NULL

	-- CONSTRAINT FK_StockTransaction_ProductBatches FOREIGN KEY (BatchId) REFERENCES ProductBatches(BatchId),
	--CONSTRAINT FK_StockTransaction_Firms FOREIGN KEY (FirmId) REFERENCES Firms(FirmId),
	-- CONSTRAINT FK_StockTransaction_Products FOREIGN KEY (ProductId) REFERENCES Products(ProductId),
);

CREATE TABLE StockAdjustments
(
    AdjustmentId INT IDENTITY PRIMARY KEY,
    FirmId INT NULL,
    ProductId INT NOT NULL,
    BatchId INT NULL,
    AdjustmentType VARCHAR(20),
    Quantity DECIMAL(18,3),
    Reason NVARCHAR(500),
    CreatedBy INT NULL,
	CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME NULL,
    DeletedAt DATETIME NULL

  --CONSTRAINT FK_StockAdjustment_Firms FOREIGN KEY (FirmId) REFERENCES Firms(FirmId),
  -- CONSTRAINT FK_StockAdjustment_Products FOREIGN KEY (ProductId) REFERENCES Products(ProductId),
  	-- CONSTRAINT FK_StockAdjustment_ProductBatches FOREIGN KEY (BatchId) REFERENCES ProductBatches(BatchId),

);


    CREATE TABLE Sales
(
    SaleId INT IDENTITY PRIMARY KEY,

    FirmId INT NOT NULL,

    InvoiceNumber VARCHAR(50) NOT NULL,

    Subtotal DECIMAL(10,2) NOT NULL,

    GST DECIMAL(10,2) NOT NULL,

    Total DECIMAL(10,2) NOT NULL,

    PaymentMethod VARCHAR(20),

    CreatedAt DATETIME DEFAULT GETUTCDATE()
);



CREATE TABLE SaleItems
(
    SaleItemId INT IDENTITY PRIMARY KEY,

    SaleId INT NOT NULL,

    ProductId INT NOT NULL,

    Quantity DECIMAL(10,2),

    Price DECIMAL(10,2),

    Total DECIMAL(10,2)
);
