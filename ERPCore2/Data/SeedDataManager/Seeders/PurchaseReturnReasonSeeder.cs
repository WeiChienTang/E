using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Models.Enums;
using ERPCore2.Helpers;
using ERPCore2.Data.SeedDataManager.Interfaces;
using Microsoft.EntityFrameworkCore;

// 使用別名來避免命名衝突
using EntityPurchaseReturnReason = ERPCore2.Data.Entities.PurchaseReturnReason;

namespace ERPCore2.Data.SeedDataManager.Seeders
{
    /// <summary>
    /// 進貨退出原因資料種子
    /// </summary>
    public class PurchaseReturnReasonSeeder : IDataSeeder
    {
        public int Order => 12; // 設定執行順序，在銷貨退回原因（11）之後
        public string Name => "進貨退出原因資料";

        public async Task SeedAsync(AppDbContext context)
        {
            await SeedPurchaseReturnReasonsAsync(context);
        }

        private static async Task SeedPurchaseReturnReasonsAsync(AppDbContext context)
        {
            if (await context.PurchaseReturnReasons.AnyAsync()) return;

            var (createdAt1, _) = SeedDataHelper.GetSystemCreateInfo(30);
            var (createdAt2, _) = SeedDataHelper.GetSystemCreateInfo(25);
            var (createdAt3, _) = SeedDataHelper.GetSystemCreateInfo(20);
            var (createdAt4, _) = SeedDataHelper.GetSystemCreateInfo(15);
            var (createdAt5, _) = SeedDataHelper.GetSystemCreateInfo(10);
            var (createdAt6, _) = SeedDataHelper.GetSystemCreateInfo(5);

            var reasons = new[]
            {
                new EntityPurchaseReturnReason
                {
                    Code = "PRR001",
                    Name = "品質不良",
                    Remarks = "進貨商品品質不符合標準或有瑕疵",
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt1
                },
                new EntityPurchaseReturnReason
                {
                    Code = "PRR002",
                    Name = "規格不符",
                    Remarks = "商品規格與採購訂單要求不符",
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt2
                },
                new EntityPurchaseReturnReason
                {
                    Code = "PRR003",
                    Name = "數量錯誤",
                    Remarks = "進貨數量與訂單不符",
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt3
                },
                new EntityPurchaseReturnReason
                {
                    Code = "PRR004",
                    Name = "供應商要求",
                    Remarks = "供應商主動要求取回商品",
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt4
                },
                new EntityPurchaseReturnReason
                {
                    Code = "PRR005",
                    Name = "過期商品",
                    Remarks = "進貨商品已過有效期限",
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt5
                },
                new EntityPurchaseReturnReason
                {
                    Code = "PRR006",
                    Name = "運送損壞",
                    Remarks = "運送過程中造成的損壞",
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt6
                },
            };

            await context.PurchaseReturnReasons.AddRangeAsync(reasons);
            await context.SaveChangesAsync();
        }
    }
}
