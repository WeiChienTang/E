using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Helpers;
using ERPCore2.Data.SeedDataManager.Interfaces;
using Microsoft.EntityFrameworkCore;

// 使用別名來避免命名衝突
using EntitySalesReturnReason = ERPCore2.Data.Entities.SalesReturnReason;

namespace ERPCore2.Data.SeedDataManager.Seeders
{
    /// <summary>
    /// 銷貨退貨原因資料種子
    /// </summary>
    public class SalesReturnReasonSeeder : IDataSeeder
    {
        public int Order => 11; // 設定執行順序，在基本資料之後
        public string Name => "銷貨退貨原因資料";

        public async Task SeedAsync(AppDbContext context)
        {
            await SeedSalesReturnReasonsAsync(context);
        }

        private static async Task SeedSalesReturnReasonsAsync(AppDbContext context)
        {
            if (await context.SalesReturnReasons.AnyAsync()) return;

            var (createdAt1, createdBy) = SeedDataHelper.GetSystemCreateInfo(30);
            var (createdAt2, _) = SeedDataHelper.GetSystemCreateInfo(25);
            var (createdAt3, _) = SeedDataHelper.GetSystemCreateInfo(20);
            var (createdAt4, _) = SeedDataHelper.GetSystemCreateInfo(15);
            var (createdAt5, _) = SeedDataHelper.GetSystemCreateInfo(10);
            var (createdAt6, _) = SeedDataHelper.GetSystemCreateInfo(5);
            var (createdAt7, _) = SeedDataHelper.GetSystemCreateInfo(1);

            var reasons = new[]
            {
                new EntitySalesReturnReason
                {
                    Code = "RR001",
                    Name = "品質不良",
                    Remarks = "產品品質不符合標準或有瑕疵",
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt1,
                    CreatedBy = createdBy
                },
                new EntitySalesReturnReason
                {
                    Code = "RR002",
                    Name = "規格不符",
                    Remarks = "產品規格與訂單要求不符",
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt2,
                    CreatedBy = createdBy
                },
                new EntitySalesReturnReason
                {
                    Code = "RR003",
                    Name = "數量錯誤",
                    Remarks = "出貨數量與訂單不符",
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt3,
                    CreatedBy = createdBy
                },
                new EntitySalesReturnReason
                {
                    Code = "RR004",
                    Name = "客戶要求",
                    Remarks = "客戶主動要求退貨",
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt4,
                    CreatedBy = createdBy
                },
                new EntitySalesReturnReason
                {
                    Code = "RR005",
                    Name = "過期商品",
                    Remarks = "商品已過有效期限",
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt5,
                    CreatedBy = createdBy
                },
                new EntitySalesReturnReason
                {
                    Code = "RR006",
                    Name = "運送損壞",
                    Remarks = "運送過程中造成的損壞",
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt6,
                    CreatedBy = createdBy
                },
            };

            await context.SalesReturnReasons.AddRangeAsync(reasons);
            await context.SaveChangesAsync();
        }
    }
}