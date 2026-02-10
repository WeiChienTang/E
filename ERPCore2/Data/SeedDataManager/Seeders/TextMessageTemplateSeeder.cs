using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Models.Enums;
using ERPCore2.Helpers;
using ERPCore2.Data.SeedDataManager.Interfaces;
using ERPCore2.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace ERPCore2.Data.SeedDataManager.Seeders
{
    /// <summary>
    /// 文字訊息範本資料種子器
    /// </summary>
    public class TextMessageTemplateSeeder : IDataSeeder
    {
        public int Order => 3; // 在系統參數之後執行
        public string Name => "文字訊息範本資料";

        public async Task SeedAsync(AppDbContext context)
        {
            await SeedTextMessageTemplatesAsync(context);
        }

        private static async Task SeedTextMessageTemplatesAsync(AppDbContext context)
        {
            if (await context.TextMessageTemplates.AnyAsync()) return;

            var (createdAt, createdBy) = SeedDataHelper.GetSystemCreateInfo(0);

            // 預設明細格式設定
            var defaultDetailConfig = new DetailFormatConfig
            {
                ShowProductCode = false,
                ShowProductName = true,
                ShowQuantity = true,
                ShowUnit = true,
                ShowUnitPrice = false,
                ShowSubtotal = false,
                ShowRemark = false
            };

            var templates = new[]
            {
                // 採購單訊息範本
                new TextMessageTemplate
                {
                    Code = "TMPL-001",
                    TemplateCode = "PurchaseOrder",
                    TemplateName = "採購單訊息範本",
                    HeaderText = "{supplierName}您好，\n\n我們希望與貴公司採購以下商品：",
                    FooterText = "如有任何問題，請與我們聯繫，感謝您。\n\n{companyName}",
                    DetailFormatJson = JsonSerializer.Serialize(defaultDetailConfig),
                    IsActive = true,
                    SortOrder = 1,
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt,
                    CreatedBy = createdBy,
                    Remarks = "採購單訊息複製功能預設範本"
                },
                
                // 銷貨單訊息範本（預留）
                new TextMessageTemplate
                {
                    Code = "TMPL-002",
                    TemplateCode = "SalesOrder",
                    TemplateName = "銷貨單訊息範本",
                    HeaderText = "{customerName}您好，\n\n感謝您的訂購，以下是您的訂單內容：",
                    FooterText = "如有任何問題，請與我們聯繫，感謝您的惠顧。\n\n{companyName}",
                    DetailFormatJson = JsonSerializer.Serialize(defaultDetailConfig),
                    IsActive = true,
                    SortOrder = 2,
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt,
                    CreatedBy = createdBy,
                    Remarks = "銷貨單訊息複製功能預設範本"
                },
                
                // 報價單訊息範本（預留）
                new TextMessageTemplate
                {
                    Code = "TMPL-003",
                    TemplateCode = "Quotation",
                    TemplateName = "報價單訊息範本",
                    HeaderText = "{customerName}您好，\n\n以下是我們為您準備的報價內容：",
                    FooterText = "以上報價有效期限為 30 天，如有任何問題請與我們聯繫。\n\n{companyName}",
                    DetailFormatJson = JsonSerializer.Serialize(defaultDetailConfig),
                    IsActive = true,
                    SortOrder = 3,
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt,
                    CreatedBy = createdBy,
                    Remarks = "報價單訊息複製功能預設範本"
                }
            };

            await context.TextMessageTemplates.AddRangeAsync(templates);
            await context.SaveChangesAsync();
        }
    }
}
