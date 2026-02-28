using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.SeedDataManager.Interfaces;
using ERPCore2.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.SeedDataManager.Seeders
{
    /// <summary>
    /// 檔案分類種子器
    /// </summary>
    public class DocumentCategorySeeder : IDataSeeder
    {
        public int Order => 5;
        public string Name => "檔案分類";

        public async Task SeedAsync(AppDbContext context)
        {
            var existingNames = await context.DocumentCategories
                .Select(c => c.Name)
                .ToHashSetAsync();

            var categories = new[]
            {
                new DocumentCategory
                {
                    Code = "DCAT-GOV",
                    Name = "政府公文",
                    Source = DocumentSource.Government,
                    DefaultAccessLevel = DocumentAccessLevel.Normal,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System",
                    Remarks = "政府機關發文、法規公告、許可證照等"
                },
                new DocumentCategory
                {
                    Code = "DCAT-VND",
                    Name = "廠商合約",
                    Source = DocumentSource.Vendor,
                    DefaultAccessLevel = DocumentAccessLevel.Sensitive,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System",
                    Remarks = "廠商報價、採購合約、供應協議等"
                },
                new DocumentCategory
                {
                    Code = "DCAT-CST",
                    Name = "客戶文件",
                    Source = DocumentSource.Customer,
                    DefaultAccessLevel = DocumentAccessLevel.Normal,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System",
                    Remarks = "客戶簽約資料、訂單確認書等"
                },
                new DocumentCategory
                {
                    Code = "DCAT-INT",
                    Name = "內部文件",
                    Source = DocumentSource.Internal,
                    DefaultAccessLevel = DocumentAccessLevel.Normal,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System",
                    Remarks = "公司內部作業程序、規章制度等"
                },
                new DocumentCategory
                {
                    Code = "DCAT-OTH",
                    Name = "其他文件",
                    Source = DocumentSource.Other,
                    DefaultAccessLevel = DocumentAccessLevel.Normal,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System",
                    Remarks = "其他類型的文件"
                }
            };

            var toAdd = categories.Where(c => !existingNames.Contains(c.Name)).ToArray();
            if (toAdd.Any())
            {
                await context.DocumentCategories.AddRangeAsync(toAdd);
                await context.SaveChangesAsync();
            }
        }
    }
}
