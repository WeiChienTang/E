using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Models.Enums;
using ERPCore2.Data.SeedDataManager.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.SeedDataManager.Seeders
{
    /// <summary>
    /// 測試用品項合成（BOM）資料種子器（僅測試環境使用）
    /// 包含：物料清單類型 + 2 筆 BOM 主檔 + 明細
    /// 依賴：TestItemSeeder (Order=53)
    /// </summary>
    public class TestItemCompositionSeeder : IDataSeeder
    {
        public int Order => 54; // 在 TestItemSeeder (Order=53) 之後執行
        public string Name => "測試品項合成（BOM）資料";

        public async Task SeedAsync(AppDbContext context)
        {
            if (await context.ItemCompositions.AnyAsync())
                return;

            // 建立物料清單類型
            var categories = new List<CompositionCategory>
            {
                new CompositionCategory
                {
                    Name = "標準製程",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new CompositionCategory
                {
                    Name = "試產配方",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
            };

            await context.CompositionCategories.AddRangeAsync(categories);
            await context.SaveChangesAsync();

            var catStandard = categories[0];
            var catTrial    = categories[1];

            // 取得品項
            var itemP005 = await context.Items.FirstOrDefaultAsync(i => i.Code == "P005"); // 感測器套組
            var itemP008 = await context.Items.FirstOrDefaultAsync(i => i.Code == "P008"); // 油壓缸
            var itemP001 = await context.Items.FirstOrDefaultAsync(i => i.Code == "P001"); // 工業用繼電器
            var itemP004 = await context.Items.FirstOrDefaultAsync(i => i.Code == "P004"); // 電源供應器
            var itemP007 = await context.Items.FirstOrDefaultAsync(i => i.Code == "P007"); // 軸承
            var itemP006 = await context.Items.FirstOrDefaultAsync(i => i.Code == "P006"); // 不鏽鋼螺絲組
            var itemP009 = await context.Items.FirstOrDefaultAsync(i => i.Code == "P009"); // 鋁擠型材料
            var itemP010 = await context.Items.FirstOrDefaultAsync(i => i.Code == "P010"); // 鑄鐵底板
            var itemP012 = await context.Items.FirstOrDefaultAsync(i => i.Code == "P012"); // 潤滑油脂

            // 取得單位
            var unitPcs = await context.Units.FirstOrDefaultAsync(u => u.Code == "PCS");
            var unitBox = await context.Units.FirstOrDefaultAsync(u => u.Code == "BOX");
            var unitM   = await context.Units.FirstOrDefaultAsync(u => u.Code == "M");
            var unitKg  = await context.Units.FirstOrDefaultAsync(u => u.Code == "KG");

            // BOM-001：感測器套組（P005）標準配方
            // 組成：工業用繼電器 ×2、電源供應器 ×1、軸承 ×1
            var bom001 = new ItemComposition
            {
                Name = "感測器套組標準BOM",
                ParentItemId = itemP005?.Id,
                CompositionCategoryId = catStandard.Id,
                Specification = "光電感測 NPN 3件組",
                Status = EntityStatus.Active,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System"
            };

            // BOM-002：油壓缸（P008）標準配方
            // 組成：鑄鐵底板 ×1、鋁擠型材料 ×0.5m、不鏽鋼螺絲組 ×1箱、潤滑油脂 ×0.5kg
            var bom002 = new ItemComposition
            {
                Name = "油壓缸組裝標準BOM",
                ParentItemId = itemP008?.Id,
                CompositionCategoryId = catStandard.Id,
                Specification = "50T 行程200mm 標準組裝",
                Status = EntityStatus.Active,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System"
            };

            await context.ItemCompositions.AddRangeAsync(bom001, bom002);
            await context.SaveChangesAsync();

            // BOM-001 明細
            var details001 = new List<ItemCompositionDetail>
            {
                new ItemCompositionDetail
                {
                    ItemCompositionId = bom001.Id,
                    ComponentItemId   = itemP001!.Id,
                    Quantity          = 2,
                    UnitId            = unitPcs?.Id,
                    ComponentCost     = itemP001?.StandardCost * 2,
                    CreatedAt         = DateTime.UtcNow,
                    CreatedBy         = "System"
                },
                new ItemCompositionDetail
                {
                    ItemCompositionId = bom001.Id,
                    ComponentItemId   = itemP004!.Id,
                    Quantity          = 1,
                    UnitId            = unitPcs?.Id,
                    ComponentCost     = itemP004?.StandardCost,
                    CreatedAt         = DateTime.UtcNow,
                    CreatedBy         = "System"
                },
                new ItemCompositionDetail
                {
                    ItemCompositionId = bom001.Id,
                    ComponentItemId   = itemP007!.Id,
                    Quantity          = 1,
                    UnitId            = unitPcs?.Id,
                    ComponentCost     = itemP007?.StandardCost,
                    CreatedAt         = DateTime.UtcNow,
                    CreatedBy         = "System"
                },
            };

            // BOM-002 明細
            var details002 = new List<ItemCompositionDetail>
            {
                new ItemCompositionDetail
                {
                    ItemCompositionId = bom002.Id,
                    ComponentItemId   = itemP010!.Id,
                    Quantity          = 1,
                    UnitId            = unitPcs?.Id,
                    ComponentCost     = itemP010?.StandardCost,
                    CreatedAt         = DateTime.UtcNow,
                    CreatedBy         = "System"
                },
                new ItemCompositionDetail
                {
                    ItemCompositionId = bom002.Id,
                    ComponentItemId   = itemP009!.Id,
                    Quantity          = 0.5m,
                    UnitId            = unitM?.Id,
                    ComponentCost     = itemP009?.StandardCost * 0.5m,
                    CreatedAt         = DateTime.UtcNow,
                    CreatedBy         = "System"
                },
                new ItemCompositionDetail
                {
                    ItemCompositionId = bom002.Id,
                    ComponentItemId   = itemP006!.Id,
                    Quantity          = 1,
                    UnitId            = unitBox?.Id,
                    ComponentCost     = itemP006?.StandardCost,
                    CreatedAt         = DateTime.UtcNow,
                    CreatedBy         = "System"
                },
                new ItemCompositionDetail
                {
                    ItemCompositionId = bom002.Id,
                    ComponentItemId   = itemP012!.Id,
                    Quantity          = 0.5m,
                    UnitId            = unitKg?.Id,
                    ComponentCost     = itemP012?.StandardCost * 0.5m,
                    CreatedAt         = DateTime.UtcNow,
                    CreatedBy         = "System"
                },
            };

            await context.ItemCompositionDetails.AddRangeAsync(details001);
            await context.ItemCompositionDetails.AddRangeAsync(details002);
            await context.SaveChangesAsync();
        }
    }
}
