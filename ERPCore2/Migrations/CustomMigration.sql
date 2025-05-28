-- Custom migration to safely rename Industry to IndustryType

-- Step 1: Create new IndustryTypes table
CREATE TABLE [IndustryTypes] (
    [IndustryTypeId] int NOT NULL IDENTITY,
    [IndustryTypeName] nvarchar(100) NOT NULL,
    [IndustryTypeCode] nvarchar(10) NULL,
    [Status] int NOT NULL DEFAULT 1,
    CONSTRAINT [PK_IndustryTypes] PRIMARY KEY ([IndustryTypeId])
);

-- Step 2: Copy data from Industries to IndustryTypes
INSERT INTO [IndustryTypes] ([IndustryTypeName], [IndustryTypeCode], [Status])
SELECT [IndustryName], [IndustryCode], [Status]
FROM [Industries];

-- Step 3: Add new column to Customers table
ALTER TABLE [Customers] ADD [IndustryTypeId] int NULL;

-- Step 4: Update new column with corresponding data
UPDATE [Customers] 
SET [IndustryTypeId] = it.[IndustryTypeId]
FROM [Customers] c
INNER JOIN [Industries] i ON c.[IndustryId] = i.[IndustryId]
INNER JOIN [IndustryTypes] it ON i.[IndustryName] = it.[IndustryTypeName];

-- Step 5: Drop old foreign key constraint
ALTER TABLE [Customers] DROP CONSTRAINT [FK_Customers_Industries_IndustryId];

-- Step 6: Drop old column
ALTER TABLE [Customers] DROP COLUMN [IndustryId];

-- Step 7: Drop old table
DROP TABLE [Industries];

-- Step 8: Create new foreign key constraint
ALTER TABLE [Customers] ADD CONSTRAINT [FK_Customers_IndustryTypes_IndustryTypeId] 
FOREIGN KEY ([IndustryTypeId]) REFERENCES [IndustryTypes] ([IndustryTypeId]) ON DELETE SET NULL;

-- Step 9: Create index
CREATE INDEX [IX_Customers_IndustryTypeId] ON [Customers] ([IndustryTypeId]);
