using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Data.SeedDataManager.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.SeedDataManager.Seeders
{
    public class InventorySeeder : IDataSeeder
    {
        public int Order => 8; // 在單位資料之後
        public string Name => "庫存異動管理";

        public async Task SeedAsync(AppDbContext context)
        {
            // 檢查是否已經有庫存異動類型資料
            bool hasData = await context.InventoryTransactionTypes.AnyAsync();

            if (hasData)
            {
                return;
            }

            // 建立異動類型
            var transactionTypes = new List<InventoryTransactionType>
            {
                new InventoryTransactionType
                {
                    Code = "IN001",
                    TypeName = "採購入庫",
                    TransactionType = InventoryTransactionTypeEnum.Purchase,
                    AffectsCost = true,
                    RequiresApproval = true,
                    AutoGenerateNumber = true,
                    NumberPrefix = "PI",
                    IsActive = true,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new InventoryTransactionType
                {
                    Code = "IN002",
                    TypeName = "生產入庫",
                    TransactionType = InventoryTransactionTypeEnum.ProductionCompletion,
                    AffectsCost = true,
                    RequiresApproval = false,
                    AutoGenerateNumber = true,
                    NumberPrefix = "MI",
                    IsActive = true,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new InventoryTransactionType
                {
                    Code = "IN003",
                    TypeName = "調撥入庫",
                    TransactionType = InventoryTransactionTypeEnum.Transfer,
                    AffectsCost = false,
                    RequiresApproval = true,
                    AutoGenerateNumber = true,
                    NumberPrefix = "TI",
                    IsActive = true,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new InventoryTransactionType
                {
                    Code = "OUT001",
                    TypeName = "銷售出庫",
                    TransactionType = InventoryTransactionTypeEnum.Sale,
                    AffectsCost = false,
                    RequiresApproval = false,
                    AutoGenerateNumber = true,
                    NumberPrefix = "SO",
                    IsActive = true,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new InventoryTransactionType
                {
                    Code = "OUT002",
                    TypeName = "調撥出庫",
                    TransactionType = InventoryTransactionTypeEnum.Transfer,
                    AffectsCost = false,
                    RequiresApproval = true,
                    AutoGenerateNumber = true,
                    NumberPrefix = "TO",
                    IsActive = true,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new InventoryTransactionType
                {
                    Code = "OUT003",
                    TypeName = "盤虧出庫",
                    TransactionType = InventoryTransactionTypeEnum.StockTaking,
                    AffectsCost = true,
                    RequiresApproval = true,
                    AutoGenerateNumber = true,
                    NumberPrefix = "LO",
                    IsActive = true,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new InventoryTransactionType
                {
                    Code = "ADJ001",
                    TypeName = "盤點調整",
                    TransactionType = InventoryTransactionTypeEnum.Adjustment,
                    AffectsCost = true,
                    RequiresApproval = true,
                    AutoGenerateNumber = true,
                    NumberPrefix = "ADJ",
                    IsActive = true,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                }
            };

            await context.InventoryTransactionTypes.AddRangeAsync(transactionTypes);
            await context.SaveChangesAsync();

        }
    }
}
