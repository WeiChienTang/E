using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Models.Enums;
using ERPCore2.Data.SeedDataManager.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.SeedDataManager.Seeders
{
    /// <summary>
    /// 測試用品項資料種子器（僅測試環境使用）
    /// 包含：品項分類 + 20 個品項（依賴 UnitSeeder 先執行）
    /// </summary>
    public class TestItemSeeder : IDataSeeder
    {
        public int Order => 53; // 在 UnitSeeder (Order=6) 之後執行
        public string Name => "測試品項資料";

        public async Task SeedAsync(AppDbContext context)
        {
            if (await context.Items.AnyAsync(i => i.Code != null && i.Code.StartsWith("P")))
                return;

            // 建立測試用品項分類（若尚未存在）
            await SeedCategoriesAsync(context);

            // 取得單位（依賴 UnitSeeder）
            var unitPcs  = await context.Units.FirstOrDefaultAsync(u => u.Code == "PCS");  // 個
            var unitKg   = await context.Units.FirstOrDefaultAsync(u => u.Code == "KG");   // 公斤
            var unitBox  = await context.Units.FirstOrDefaultAsync(u => u.Code == "BOX");  // 箱
            var unitSet  = await context.Units.FirstOrDefaultAsync(u => u.Code == "SET");  // 組
            var unitM    = await context.Units.FirstOrDefaultAsync(u => u.Code == "M");    // 公尺
            var unitL    = await context.Units.FirstOrDefaultAsync(u => u.Code == "L");    // 公升
            var unitBag  = await context.Units.FirstOrDefaultAsync(u => u.Code == "BAG");  // 包
            var unitDrum = await context.Units.FirstOrDefaultAsync(u => u.Code == "DRUM"); // 桶

            // 製程單位（部分品項使用）
            int? prodUnitPcsId  = unitPcs?.Id;
            int? prodUnitLId    = unitL?.Id;
            int? prodUnitKgId   = unitKg?.Id;

            // 取得分類
            var catElec   = await context.ItemCategories.FirstOrDefaultAsync(c => c.Code == "CAT-ELEC");
            var catMech   = await context.ItemCategories.FirstOrDefaultAsync(c => c.Code == "CAT-MECH");
            var catChem   = await context.ItemCategories.FirstOrDefaultAsync(c => c.Code == "CAT-CHEM");
            var catPack   = await context.ItemCategories.FirstOrDefaultAsync(c => c.Code == "CAT-PACK");
            var catOffice = await context.ItemCategories.FirstOrDefaultAsync(c => c.Code == "CAT-OFFICE");

            var items = new List<Item>
            {
                // 電子類
                new Item
                {
                    Code = "P001",
                    Name = "工業用繼電器",
                    Barcode = "4710001000011",
                    UnitId = unitPcs?.Id,
                    ItemCategoryId = catElec?.Id,
                    Specification = "DC24V 10A",
                    TaxRate = 5,
                    StandardCost = 150,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new Item
                {
                    Code = "P002",
                    Name = "伺服馬達驅動器",
                    Barcode = "4710001000028",
                    UnitId = unitPcs?.Id,
                    ItemCategoryId = catElec?.Id,
                    Specification = "200W 單相",
                    TaxRate = 5,
                    StandardCost = 3200,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new Item
                {
                    Code = "P003",
                    Name = "PLC 控制模組",
                    Barcode = "4710001000035",
                    UnitId = unitPcs?.Id,
                    ItemCategoryId = catElec?.Id,
                    Specification = "16DI/16DO",
                    TaxRate = 5,
                    StandardCost = 8500,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new Item
                {
                    Code = "P004",
                    Name = "電源供應器",
                    Barcode = "4710001000042",
                    UnitId = unitPcs?.Id,
                    ItemCategoryId = catElec?.Id,
                    Specification = "24V 10A",
                    TaxRate = 5,
                    StandardCost = 1200,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new Item
                {
                    Code = "P005",
                    Name = "感測器套組",
                    Barcode = "4710001000059",
                    UnitId = unitSet?.Id,
                    ProductionUnitId = prodUnitPcsId,
                    ProductionUnitConversionRate = 3,   // 1 組 = 3 個感測器
                    ItemCategoryId = catElec?.Id,
                    Specification = "光電感測 NPN",
                    TaxRate = 5,
                    StandardCost = 680,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                // 機械類
                new Item
                {
                    Code = "P006",
                    Name = "不鏽鋼螺絲組",
                    Barcode = "4710001000066",
                    UnitId = unitBox?.Id,
                    ProductionUnitId = prodUnitPcsId,
                    ProductionUnitConversionRate = 100, // 1 箱 = 100 個
                    ItemCategoryId = catMech?.Id,
                    Specification = "M6×20mm 100入",
                    TaxRate = 5,
                    StandardCost = 320,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new Item
                {
                    Code = "P007",
                    Name = "軸承",
                    Barcode = "4710001000073",
                    UnitId = unitPcs?.Id,
                    ItemCategoryId = catMech?.Id,
                    Specification = "6205ZZ",
                    TaxRate = 5,
                    StandardCost = 85,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new Item
                {
                    Code = "P008",
                    Name = "油壓缸",
                    Barcode = "4710001000080",
                    UnitId = unitPcs?.Id,
                    ItemCategoryId = catMech?.Id,
                    Specification = "50T 行程200mm",
                    TaxRate = 5,
                    StandardCost = 12000,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new Item
                {
                    Code = "P009",
                    Name = "鋁擠型材料",
                    Barcode = "4710001000097",
                    UnitId = unitM?.Id,
                    ItemCategoryId = catMech?.Id,
                    Specification = "40×40mm T型槽",
                    TaxRate = 5,
                    StandardCost = 180,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new Item
                {
                    Code = "P010",
                    Name = "鑄鐵底板",
                    Barcode = "4710001000103",
                    UnitId = unitPcs?.Id,
                    ItemCategoryId = catMech?.Id,
                    Specification = "300×400×20mm",
                    TaxRate = 5,
                    StandardCost = 2500,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                // 化工類
                new Item
                {
                    Code = "P011",
                    Name = "工業用清洗劑",
                    Barcode = "4710001000110",
                    UnitId = unitL?.Id,
                    ItemCategoryId = catChem?.Id,
                    Specification = "無腐蝕性 pH7",
                    TaxRate = 5,
                    StandardCost = 45,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new Item
                {
                    Code = "P012",
                    Name = "潤滑油脂",
                    Barcode = "4710001000127",
                    UnitId = unitKg?.Id,
                    ItemCategoryId = catChem?.Id,
                    Specification = "NLGI 2級",
                    TaxRate = 5,
                    StandardCost = 220,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new Item
                {
                    Code = "P013",
                    Name = "防鏽油",
                    Barcode = "4710001000134",
                    UnitId = unitDrum?.Id,
                    ProductionUnitId = prodUnitLId,
                    ProductionUnitConversionRate = 200, // 1 桶 = 200 公升
                    ItemCategoryId = catChem?.Id,
                    Specification = "200L/桶",
                    TaxRate = 5,
                    StandardCost = 8500,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new Item
                {
                    Code = "P014",
                    Name = "環氧樹脂膠",
                    Barcode = "4710001000141",
                    UnitId = unitKg?.Id,
                    ItemCategoryId = catChem?.Id,
                    Specification = "雙液型 A+B",
                    TaxRate = 5,
                    StandardCost = 380,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new Item
                {
                    Code = "P015",
                    Name = "工業酒精",
                    Barcode = "4710001000158",
                    UnitId = unitL?.Id,
                    ProductionUnitId = prodUnitKgId,
                    ProductionUnitConversionRate = 0.789m, // 1 L ≈ 0.789 kg（酒精密度）
                    ItemCategoryId = catChem?.Id,
                    Specification = "純度95%",
                    TaxRate = 5,
                    StandardCost = 35,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                // 包材類
                new Item
                {
                    Code = "P016",
                    Name = "瓦楞紙箱",
                    Barcode = "4710001000165",
                    UnitId = unitPcs?.Id,
                    ItemCategoryId = catPack?.Id,
                    Specification = "30×20×20cm 三層",
                    TaxRate = 5,
                    StandardCost = 18,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new Item
                {
                    Code = "P017",
                    Name = "氣泡布",
                    Barcode = "4710001000172",
                    UnitId = unitM?.Id,
                    ItemCategoryId = catPack?.Id,
                    Specification = "寬120cm",
                    TaxRate = 5,
                    StandardCost = 12,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new Item
                {
                    Code = "P018",
                    Name = "封箱膠帶",
                    Barcode = "4710001000189",
                    UnitId = unitPcs?.Id,
                    ItemCategoryId = catPack?.Id,
                    Specification = "48mm×100m",
                    TaxRate = 5,
                    StandardCost = 25,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                // 辦公用品類
                new Item
                {
                    Code = "P019",
                    Name = "A4 影印紙",
                    Barcode = "4710001000196",
                    UnitId = unitBag?.Id,
                    ProductionUnitId = prodUnitPcsId,
                    ProductionUnitConversionRate = 500, // 1 包 = 500 張
                    ItemCategoryId = catOffice?.Id,
                    Specification = "70g 500張/包",
                    TaxRate = 5,
                    StandardCost = 95,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new Item
                {
                    Code = "P020",
                    Name = "標籤貼紙",
                    Barcode = "4710001000202",
                    UnitId = unitBox?.Id,
                    ItemCategoryId = catOffice?.Id,
                    Specification = "40×30mm 1000張/盒",
                    TaxRate = 5,
                    StandardCost = 120,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
            };

            await context.Items.AddRangeAsync(items);
            await context.SaveChangesAsync();
        }

        private static async Task SeedCategoriesAsync(AppDbContext context)
        {
            var categoryCodes = new[] { "CAT-ELEC", "CAT-MECH", "CAT-CHEM", "CAT-PACK", "CAT-OFFICE" };

            if (await context.ItemCategories.AnyAsync(c => c.Code != null && categoryCodes.Contains(c.Code)))
                return;

            var categories = new List<ItemCategory>
            {
                new ItemCategory
                {
                    Code = "CAT-ELEC",
                    Name = "電子零件",
                    IsSaleable = true,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new ItemCategory
                {
                    Code = "CAT-MECH",
                    Name = "機械零組件",
                    IsSaleable = true,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new ItemCategory
                {
                    Code = "CAT-CHEM",
                    Name = "化工原料",
                    IsSaleable = true,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new ItemCategory
                {
                    Code = "CAT-PACK",
                    Name = "包裝材料",
                    IsSaleable = false,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new ItemCategory
                {
                    Code = "CAT-OFFICE",
                    Name = "辦公耗材",
                    IsSaleable = false,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
            };

            await context.ItemCategories.AddRangeAsync(categories);
            await context.SaveChangesAsync();
        }
    }
}
