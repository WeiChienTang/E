using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Helpers;
using ERPCore2.Data.SeedDataManager.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.SeedDataManager.Seeders
{
    /// <summary>
    /// 商品資料種子類別
    /// </summary>
    public class ProductSeeder : IDataSeeder
    {
        public int Order => 22;
        public string Name => "商品資料";

        public async Task SeedAsync(AppDbContext context)
        {
            await SeedProductsAsync(context);
        }

        /// <summary>
        /// 種子商品資料
        /// </summary>
        private static async Task SeedProductsAsync(AppDbContext context)
        {
            if (await context.Products.AnyAsync())
                return; // 資料已存在

            // 取得商品類別
            var electronicsCategory = await context.ProductCategories.FirstOrDefaultAsync(pc => pc.Code == "PC001");
            var officeCategory = await context.ProductCategories.FirstOrDefaultAsync(pc => pc.Code == "PC001");
            var industrialCategory = await context.ProductCategories.FirstOrDefaultAsync(pc => pc.Code == "PC001");
            var softwareCategory = await context.ProductCategories.FirstOrDefaultAsync(pc => pc.Code == "PC001");
            var materialCategory = await context.ProductCategories.FirstOrDefaultAsync(pc => pc.Code == "PC001");

            // 取得供應商
            var supplier1 = await context.Suppliers.FirstOrDefaultAsync(s => s.Code == "S001");
            var supplier2 = await context.Suppliers.FirstOrDefaultAsync(s => s.Code == "S002");
            var supplier3 = await context.Suppliers.FirstOrDefaultAsync(s => s.Code == "S003");
            var supplier4 = await context.Suppliers.FirstOrDefaultAsync(s => s.Code == "S004");

            // 取得單位
            var unitPc = await context.Units.FirstOrDefaultAsync(u => u.Code == "PC");     // 台
            var unitPcs = await context.Units.FirstOrDefaultAsync(u => u.Code == "PCS");   // 個
            var unitPack = await context.Units.FirstOrDefaultAsync(u => u.Code == "PACK"); // 包
            var unitPen = await context.Units.FirstOrDefaultAsync(u => u.Code == "PEN");   // 支
            var unitTop = await context.Units.FirstOrDefaultAsync(u => u.Code == "TOP");   // 頂
            var unitLic = await context.Units.FirstOrDefaultAsync(u => u.Code == "LIC");   // 授權
            var unitSht = await context.Units.FirstOrDefaultAsync(u => u.Code == "SHT");   // 張
            var unitBag = await context.Units.FirstOrDefaultAsync(u => u.Code == "BAG");   // 袋
            var unitRol = await context.Units.FirstOrDefaultAsync(u => u.Code == "ROL");   // 捲

            // var products = new[]
            // {
            //     // 電子商品類
            //     new Product
            //     {
            //         Code = "P001",
            //         Name = "無線滑鼠",
            //         UnitId = unitPcs?.Id,
            //         ProductCategoryId = electronicsCategory?.Id,
            //         Status = EntityStatus.Active,
            //         CreatedAt = DateTime.Now,
            //         CreatedBy = "System"
            //     },
            //     new Product
            //     {
            //         Code = "P002",
            //         Name = "機械式鍵盤",
            //         UnitId = unitPcs?.Id,
            //         ProductCategoryId = electronicsCategory?.Id,
            //         Status = EntityStatus.Active,
            //         CreatedAt = DateTime.Now,
            //         CreatedBy = "System"
            //     },
            //     new Product
            //     {
            //         Code = "P003",
            //         Name = "27吋液晶螢幕",
            //         UnitId = unitPc?.Id,
            //         ProductCategoryId = electronicsCategory?.Id,
            //         Status = EntityStatus.Active,
            //         CreatedAt = DateTime.Now,
            //         CreatedBy = "System"
            //     },
            //     new Product
            //     {
            //         Code = "P004",
            //         Name = "藍牙耳機",
            //         UnitId = unitPcs?.Id,
            //         ProductCategoryId = electronicsCategory?.Id,
            //         Status = EntityStatus.Active,
            //         CreatedAt = DateTime.Now,
            //         CreatedBy = "System"
            //     },
            //     new Product
            //     {
            //         Code = "P005",
            //         Name = "網路攝影機",
            //         UnitId = unitPcs?.Id,
            //         ProductCategoryId = electronicsCategory?.Id,
            //         Status = EntityStatus.Active,
            //         CreatedAt = DateTime.Now,
            //         CreatedBy = "System"
            //     },
            //     new Product
            //     {
            //         Code = "P006",
            //         Name = "USB集線器",
            //         UnitId = unitPcs?.Id,
            //         ProductCategoryId = electronicsCategory?.Id,
            //         Status = EntityStatus.Active,
            //         CreatedAt = DateTime.Now,
            //         CreatedBy = "System"
            //     },
            //     new Product
            //     {
            //         Code = "P007",
            //         Name = "外接硬碟 2TB",
            //         UnitId = unitPcs?.Id,
            //         ProductCategoryId = electronicsCategory?.Id,
            //         Status = EntityStatus.Active,
            //         CreatedAt = DateTime.Now,
            //         CreatedBy = "System"
            //     },
            //     new Product
            //     {
            //         Code = "P008",
            //         Name = "記憶卡 128GB",
            //         UnitId = unitPcs?.Id,
            //         ProductCategoryId = electronicsCategory?.Id,
            //         Status = EntityStatus.Active,
            //         CreatedAt = DateTime.Now,
            //         CreatedBy = "System"
            //     },
                
            //     // 辦公用品類
            //     new Product
            //     {
            //         Code = "P009",
            //         Name = "A4影印紙",
            //         UnitId = unitPack?.Id,
            //         ProductCategoryId = officeCategory?.Id,
            //         Status = EntityStatus.Active,
            //         CreatedAt = DateTime.Now,
            //         CreatedBy = "System"
            //     },
            //     new Product
            //     {
            //         Code = "P010",
            //         Name = "原子筆",
            //         UnitId = unitPen?.Id,
            //         ProductCategoryId = officeCategory?.Id,
            //         Status = EntityStatus.Active,
            //         CreatedAt = DateTime.Now,
            //         CreatedBy = "System"
            //     },
            //     new Product
            //     {
            //         Code = "P011",
            //         Name = "資料夾",
            //         UnitId = unitPcs?.Id,
            //         ProductCategoryId = officeCategory?.Id,
            //         Status = EntityStatus.Active,
            //         CreatedAt = DateTime.Now,
            //         CreatedBy = "System"
            //     },
            //     new Product
            //     {
            //         Code = "P012",
            //         Name = "釘書機",
            //         UnitId = unitPcs?.Id,
            //         ProductCategoryId = officeCategory?.Id,
            //         Status = EntityStatus.Active,
            //         CreatedAt = DateTime.Now,
            //         CreatedBy = "System"
            //     },
            //     new Product
            //     {
            //         Code = "P013",
            //         Name = "便利貼",
            //         UnitId = unitPack?.Id,
            //         ProductCategoryId = officeCategory?.Id,
            //         Status = EntityStatus.Active,
            //         CreatedAt = DateTime.Now,
            //         CreatedBy = "System"
            //     },
            //     new Product
            //     {
            //         Code = "P014",
            //         Name = "迴紋針",
            //         UnitId = unitPack?.Id,
            //         ProductCategoryId = officeCategory?.Id,
            //         Status = EntityStatus.Active,
            //         CreatedAt = DateTime.Now,
            //         CreatedBy = "System"
            //     },
            //     new Product
            //     {
            //         Code = "P015",
            //         Name = "白板筆",
            //         UnitId = unitPen?.Id,
            //         ProductCategoryId = officeCategory?.Id,
            //         Status = EntityStatus.Active,
            //         CreatedAt = DateTime.Now,
            //         CreatedBy = "System"
            //     },
            //     new Product
            //     {
            //         Code = "P016",
            //         Name = "計算機",
            //         UnitId = unitPcs?.Id,
            //         ProductCategoryId = officeCategory?.Id,
            //         Status = EntityStatus.Active,
            //         CreatedAt = DateTime.Now,
            //         CreatedBy = "System"
            //     },

            //     // 工業用品類
            //     new Product
            //     {
            //         Code = "P017",
            //         Name = "安全帽",
            //         UnitId = unitTop?.Id,
            //         ProductCategoryId = industrialCategory?.Id,
            //         Status = EntityStatus.Active,
            //         CreatedAt = DateTime.Now,
            //         CreatedBy = "System"
            //     },
            //     new Product
            //     {
            //         Code = "P018",
            //         Name = "安全鞋",
            //         UnitId = unitPcs?.Id,
            //         ProductCategoryId = industrialCategory?.Id,
            //         Status = EntityStatus.Active,
            //         CreatedAt = DateTime.Now,
            //         CreatedBy = "System"
            //     },
            //     new Product
            //     {
            //         Code = "P019",
            //         Name = "工作手套",
            //         UnitId = unitPcs?.Id,
            //         ProductCategoryId = industrialCategory?.Id,
            //         Status = EntityStatus.Active,
            //         CreatedAt = DateTime.Now,
            //         CreatedBy = "System"
            //     },
            //     new Product
            //     {
            //         Code = "P020",
            //         Name = "防護眼鏡",
            //         UnitId = unitPcs?.Id,
            //         ProductCategoryId = industrialCategory?.Id,
            //         Status = EntityStatus.Active,
            //         CreatedAt = DateTime.Now,
            //         CreatedBy = "System"
            //     },
            //     new Product
            //     {
            //         Code = "P021",
            //         Name = "電動螺絲起子",
            //         UnitId = unitPcs?.Id,
            //         ProductCategoryId = industrialCategory?.Id,
            //         Status = EntityStatus.Active,
            //         CreatedAt = DateTime.Now,
            //         CreatedBy = "System"
            //     },
            //     new Product
            //     {
            //         Code = "P022",
            //         Name = "水平儀",
            //         UnitId = unitPcs?.Id,
            //         ProductCategoryId = industrialCategory?.Id,
            //         Status = EntityStatus.Active,
            //         CreatedAt = DateTime.Now,
            //         CreatedBy = "System"
            //     },
            //     new Product
            //     {
            //         Code = "P023",
            //         Name = "捲尺 5M",
            //         UnitId = unitPcs?.Id,
            //         ProductCategoryId = industrialCategory?.Id,
            //         Status = EntityStatus.Active,
            //         CreatedAt = DateTime.Now,
            //         CreatedBy = "System"
            //     },
            //     new Product
            //     {
            //         Code = "P024",
            //         Name = "工具箱",
            //         UnitId = unitPcs?.Id,
            //         ProductCategoryId = industrialCategory?.Id,
            //         Status = EntityStatus.Active,
            //         CreatedAt = DateTime.Now,
            //         CreatedBy = "System"
            //     },

            //     // 軟體授權類
            //     new Product
            //     {
            //         Code = "P025",
            //         Name = "防毒軟體授權",
            //         UnitId = unitLic?.Id,
            //         ProductCategoryId = softwareCategory?.Id,
            //         Status = EntityStatus.Active,
            //         CreatedAt = DateTime.Now,
            //         CreatedBy = "System"
            //     },
            //     new Product
            //     {
            //         Code = "P026",
            //         Name = "辦公室軟體授權",
            //         UnitId = unitLic?.Id,
            //         ProductCategoryId = softwareCategory?.Id,
            //         Status = EntityStatus.Active,
            //         CreatedAt = DateTime.Now,
            //         CreatedBy = "System"
            //     },
            //     new Product
            //     {
            //         Code = "P027",
            //         Name = "專案管理系統授權",
            //         UnitId = unitLic?.Id,
            //         ProductCategoryId = softwareCategory?.Id,
            //         Status = EntityStatus.Active,
            //         CreatedAt = DateTime.Now,
            //         CreatedBy = "System"
            //     },
            //     new Product
            //     {
            //         Code = "P028",
            //         Name = "雲端儲存服務",
            //         UnitId = unitLic?.Id,
            //         ProductCategoryId = softwareCategory?.Id,
            //         Status = EntityStatus.Active,
            //         CreatedAt = DateTime.Now,
            //         CreatedBy = "System"
            //     },

            //     // 原物料類
            //     new Product
            //     {
            //         Code = "P029",
            //         Name = "不鏽鋼板材",
            //         UnitId = unitSht?.Id,
            //         ProductCategoryId = materialCategory?.Id,
            //         Status = EntityStatus.Active,
            //         CreatedAt = DateTime.Now,
            //         CreatedBy = "System"
            //     },
            //     new Product
            //     {
            //         Code = "P030",
            //         Name = "銅管",
            //         UnitId = unitPcs?.Id,
            //         ProductCategoryId = materialCategory?.Id,
            //         Status = EntityStatus.Active,
            //         CreatedAt = DateTime.Now,
            //         CreatedBy = "System"
            //     },
            //     new Product
            //     {
            //         Code = "P031",
            //         Name = "鋁合金型材",
            //         UnitId = unitPcs?.Id,
            //         ProductCategoryId = materialCategory?.Id,
            //         Status = EntityStatus.Active,
            //         CreatedAt = DateTime.Now,
            //         CreatedBy = "System"
            //     },
            //     new Product
            //     {
            //         Code = "P032",
            //         Name = "塑膠粒",
            //         UnitId = unitBag?.Id,
            //         ProductCategoryId = materialCategory?.Id,
            //         Status = EntityStatus.Active,
            //         CreatedAt = DateTime.Now,
            //         CreatedBy = "System"
            //     },
            //     new Product
            //     {
            //         Code = "P033",
            //         Name = "膠帶",
            //         UnitId = unitRol?.Id,
            //         ProductCategoryId = materialCategory?.Id,
            //         Status = EntityStatus.Active,
            //         CreatedAt = DateTime.Now,
            //         CreatedBy = "System"
            //     },
            //     new Product
            //     {
            //         Code = "P034",
            //         Name = "工業用潤滑油",
            //         UnitId = unitPcs?.Id,
            //         ProductCategoryId = materialCategory?.Id,
            //         Status = EntityStatus.Active,
            //         CreatedAt = DateTime.Now,
            //         CreatedBy = "System"
            //     },
            //     new Product
            //     {
            //         Code = "P035",
            //         Name = "螺絲包",
            //         UnitId = unitPack?.Id,
            //         ProductCategoryId = materialCategory?.Id,
            //         Status = EntityStatus.Active,
            //         CreatedAt = DateTime.Now,
            //         CreatedBy = "System"
            //     },

            //     // 更多電子商品
            //     new Product
            //     {
            //         Code = "P036",
            //         Name = "筆記型電腦散熱墊",
            //         UnitId = unitPcs?.Id,
            //         ProductCategoryId = electronicsCategory?.Id,
            //         Status = EntityStatus.Active,
            //         CreatedAt = DateTime.Now,
            //         CreatedBy = "System"
            //     },
            //     new Product
            //     {
            //         Code = "P037",
            //         Name = "行動電源 20000mAh",
            //         UnitId = unitPcs?.Id,
            //         ProductCategoryId = electronicsCategory?.Id,
            //         Status = EntityStatus.Active,
            //         CreatedAt = DateTime.Now,
            //         CreatedBy = "System"
            //     },
            //     new Product
            //     {
            //         Code = "P038",
            //         Name = "USB充電線",
            //         UnitId = unitPcs?.Id,
            //         ProductCategoryId = electronicsCategory?.Id,
            //         Status = EntityStatus.Active,
            //         CreatedAt = DateTime.Now,
            //         CreatedBy = "System"
            //     },
            //     new Product
            //     {
            //         Code = "P039",
            //         Name = "手機支架",
            //         UnitId = unitPcs?.Id,
            //         ProductCategoryId = electronicsCategory?.Id,
            //         Status = EntityStatus.Active,
            //         CreatedAt = DateTime.Now,
            //         CreatedBy = "System"
            //     },
            //     new Product
            //     {
            //         Code = "P040",
            //         Name = "電腦喇叭",
            //         UnitId = unitPcs?.Id,
            //         ProductCategoryId = electronicsCategory?.Id,
            //         Status = EntityStatus.Active,
            //         CreatedAt = DateTime.Now,
            //         CreatedBy = "System"
            //     },
            //     new Product
            //     {
            //         Code = "P041",
            //         Name = "讀卡機",
            //         UnitId = unitPcs?.Id,
            //         ProductCategoryId = electronicsCategory?.Id,
            //         Status = EntityStatus.Active,
            //         CreatedAt = DateTime.Now,
            //         CreatedBy = "System"
            //     },
            //     new Product
            //     {
            //         Code = "P042",
            //         Name = "HDMI線材",
            //         UnitId = unitPcs?.Id,
            //         ProductCategoryId = electronicsCategory?.Id,
            //         Status = EntityStatus.Active,
            //         CreatedAt = DateTime.Now,
            //         CreatedBy = "System"
            //     },
            //     new Product
            //     {
            //         Code = "P043",
            //         Name = "無線充電板",
            //         UnitId = unitPcs?.Id,
            //         ProductCategoryId = electronicsCategory?.Id,
            //         Status = EntityStatus.Active,
            //         CreatedAt = DateTime.Now,
            //         CreatedBy = "System"
            //     },
            //     new Product
            //     {
            //         Code = "P044",
            //         Name = "電腦護目鏡",
            //         UnitId = unitPcs?.Id,
            //         ProductCategoryId = electronicsCategory?.Id,
            //         Status = EntityStatus.Active,
            //         CreatedAt = DateTime.Now,
            //         CreatedBy = "System"
            //     },
            //     new Product
            //     {
            //         Code = "P045",
            //         Name = "螢幕增高架",
            //         UnitId = unitPcs?.Id,
            //         ProductCategoryId = electronicsCategory?.Id,
            //         Status = EntityStatus.Active,
            //         CreatedAt = DateTime.Now,
            //         CreatedBy = "System"
            //     },

            //     // 更多辦公用品
            //     new Product
            //     {
            //         Code = "P046",
            //         Name = "長尾夾",
            //         UnitId = unitPack?.Id,
            //         ProductCategoryId = officeCategory?.Id,
            //         Status = EntityStatus.Active,
            //         CreatedAt = DateTime.Now,
            //         CreatedBy = "System"
            //     },
            //     new Product
            //     {
            //         Code = "P047",
            //         Name = "修正帶",
            //         UnitId = unitPcs?.Id,
            //         ProductCategoryId = officeCategory?.Id,
            //         Status = EntityStatus.Active,
            //         CreatedAt = DateTime.Now,
            //         CreatedBy = "System"
            //     },
            //     new Product
            //     {
            //         Code = "P048",
            //         Name = "打孔機",
            //         UnitId = unitPcs?.Id,
            //         ProductCategoryId = officeCategory?.Id,
            //         Status = EntityStatus.Active,
            //         CreatedAt = DateTime.Now,
            //         CreatedBy = "System"
            //     },
            //     new Product
            //     {
            //         Code = "P049",
            //         Name = "膠水",
            //         UnitId = unitPcs?.Id,
            //         ProductCategoryId = officeCategory?.Id,
            //         Status = EntityStatus.Active,
            //         CreatedAt = DateTime.Now,
            //         CreatedBy = "System"
            //     },
            //     new Product
            //     {
            //         Code = "P050",
            //         Name = "剪刀",
            //         UnitId = unitPcs?.Id,
            //         ProductCategoryId = officeCategory?.Id,
            //         Status = EntityStatus.Active,
            //         CreatedAt = DateTime.Now,
            //         CreatedBy = "System"
            //     },
            //     new Product
            //     {
            //         Code = "P051",
            //         Name = "美工刀",
            //         UnitId = unitPcs?.Id,
            //         ProductCategoryId = officeCategory?.Id,
            //         Status = EntityStatus.Active,
            //         CreatedAt = DateTime.Now,
            //         CreatedBy = "System"
            //     },
            //     new Product
            //     {
            //         Code = "P052",
            //         Name = "尺規組",
            //         UnitId = unitPcs?.Id,
            //         ProductCategoryId = officeCategory?.Id,
            //         Status = EntityStatus.Active,
            //         CreatedAt = DateTime.Now,
            //         CreatedBy = "System"
            //     },
            //     new Product
            //     {
            //         Code = "P053",
            //         Name = "筆筒",
            //         UnitId = unitPcs?.Id,
            //         ProductCategoryId = officeCategory?.Id,
            //         Status = EntityStatus.Active,
            //         CreatedAt = DateTime.Now,
            //         CreatedBy = "System"
            //     },
            //     new Product
            //     {
            //         Code = "P054",
            //         Name = "文件收納盒",
            //         UnitId = unitPcs?.Id,
            //         ProductCategoryId = officeCategory?.Id,
            //         Status = EntityStatus.Active,
            //         CreatedAt = DateTime.Now,
            //         CreatedBy = "System"
            //     },
            //     new Product
            //     {
            //         Code = "P055",
            //         Name = "標籤貼紙",
            //         UnitId = unitPack?.Id,
            //         ProductCategoryId = officeCategory?.Id,
            //         Status = EntityStatus.Active,
            //         CreatedAt = DateTime.Now,
            //         CreatedBy = "System"
            //     },

            //     // 更多工業用品
            //     new Product
            //     {
            //         Code = "P056",
            //         Name = "反光背心",
            //         UnitId = unitPcs?.Id,
            //         ProductCategoryId = industrialCategory?.Id,
            //         Status = EntityStatus.Active,
            //         CreatedAt = DateTime.Now,
            //         CreatedBy = "System"
            //     },
            //     new Product
            //     {
            //         Code = "P057",
            //         Name = "護膝",
            //         UnitId = unitPcs?.Id,
            //         ProductCategoryId = industrialCategory?.Id,
            //         Status = EntityStatus.Active,
            //         CreatedAt = DateTime.Now,
            //         CreatedBy = "System"
            //     },
            //     new Product
            //     {
            //         Code = "P058",
            //         Name = "工業手電筒",
            //         UnitId = unitPcs?.Id,
            //         ProductCategoryId = industrialCategory?.Id,
            //         Status = EntityStatus.Active,
            //         CreatedAt = DateTime.Now,
            //         CreatedBy = "System"
            //     },
            //     new Product
            //     {
            //         Code = "P059",
            //         Name = "電工膠帶",
            //         UnitId = unitRol?.Id,
            //         ProductCategoryId = industrialCategory?.Id,
            //         Status = EntityStatus.Active,
            //         CreatedAt = DateTime.Now,
            //         CreatedBy = "System"
            //     },
            //     new Product
            //     {
            //         Code = "P060",
            //         Name = "電鑽",
            //         UnitId = unitPcs?.Id,
            //         ProductCategoryId = industrialCategory?.Id,
            //         Status = EntityStatus.Active,
            //         CreatedAt = DateTime.Now,
            //         CreatedBy = "System"
            //     },
            //     new Product
            //     {
            //         Code = "P061",
            //         Name = "砂輪機",
            //         UnitId = unitPcs?.Id,
            //         ProductCategoryId = industrialCategory?.Id,
            //         Status = EntityStatus.Active,
            //         CreatedAt = DateTime.Now,
            //         CreatedBy = "System"
            //     },
            //     new Product
            //     {
            //         Code = "P062",
            //         Name = "電焊面罩",
            //         UnitId = unitPcs?.Id,
            //         ProductCategoryId = industrialCategory?.Id,
            //         Status = EntityStatus.Active,
            //         CreatedAt = DateTime.Now,
            //         CreatedBy = "System"
            //     },
            //     new Product
            //     {
            //         Code = "P063",
            //         Name = "活動扳手",
            //         UnitId = unitPcs?.Id,
            //         ProductCategoryId = industrialCategory?.Id,
            //         Status = EntityStatus.Active,
            //         CreatedAt = DateTime.Now,
            //         CreatedBy = "System"
            //     },
            //     new Product
            //     {
            //         Code = "P064",
            //         Name = "梅花扳手組",
            //         UnitId = unitPcs?.Id,
            //         ProductCategoryId = industrialCategory?.Id,
            //         Status = EntityStatus.Active,
            //         CreatedAt = DateTime.Now,
            //         CreatedBy = "System"
            //     },
            //     new Product
            //     {
            //         Code = "P065",
            //         Name = "老虎鉗",
            //         UnitId = unitPcs?.Id,
            //         ProductCategoryId = industrialCategory?.Id,
            //         Status = EntityStatus.Active,
            //         CreatedAt = DateTime.Now,
            //         CreatedBy = "System"
            //     },

            //     // 更多軟體授權
            //     new Product
            //     {
            //         Code = "P066",
            //         Name = "圖像編輯軟體授權",
            //         UnitId = unitLic?.Id,
            //         ProductCategoryId = softwareCategory?.Id,
            //         Status = EntityStatus.Active,
            //         CreatedAt = DateTime.Now,
            //         CreatedBy = "System"
            //     },
            //     new Product
            //     {
            //         Code = "P067",
            //         Name = "影片剪輯軟體授權",
            //         UnitId = unitLic?.Id,
            //         ProductCategoryId = softwareCategory?.Id,
            //         Status = EntityStatus.Active,
            //         CreatedAt = DateTime.Now,
            //         CreatedBy = "System"
            //     },
            //     new Product
            //     {
            //         Code = "P068",
            //         Name = "會計軟體授權",
            //         UnitId = unitLic?.Id,
            //         ProductCategoryId = softwareCategory?.Id,
            //         Status = EntityStatus.Active,
            //         CreatedAt = DateTime.Now,
            //         CreatedBy = "System"
            //     },
            //     new Product
            //     {
            //         Code = "P069",
            //         Name = "資料庫管理系統授權",
            //         UnitId = unitLic?.Id,
            //         ProductCategoryId = softwareCategory?.Id,
            //         Status = EntityStatus.Active,
            //         CreatedAt = DateTime.Now,
            //         CreatedBy = "System"
            //     },
            //     new Product
            //     {
            //         Code = "P070",
            //         Name = "CAD設計軟體授權",
            //         UnitId = unitLic?.Id,
            //         ProductCategoryId = softwareCategory?.Id,
            //         Status = EntityStatus.Active,
            //         CreatedAt = DateTime.Now,
            //         CreatedBy = "System"
            //     },
            //     new Product
            //     {
            //         Code = "P071",
            //         Name = "備份軟體授權",
            //         UnitId = unitLic?.Id,
            //         ProductCategoryId = softwareCategory?.Id,
            //         Status = EntityStatus.Active,
            //         CreatedAt = DateTime.Now,
            //         CreatedBy = "System"
            //     },
            //     new Product
            //     {
            //         Code = "P072",
            //         Name = "視訊會議系統授權",
            //         UnitId = unitLic?.Id,
            //         ProductCategoryId = softwareCategory?.Id,
            //         Status = EntityStatus.Active,
            //         CreatedAt = DateTime.Now,
            //         CreatedBy = "System"
            //     },

            //     // 更多原物料
            //     new Product
            //     {
            //         Code = "P073",
            //         Name = "鋼筋",
            //         UnitId = unitPcs?.Id,
            //         ProductCategoryId = materialCategory?.Id,
            //         Status = EntityStatus.Active,
            //         CreatedAt = DateTime.Now,
            //         CreatedBy = "System"
            //     },
            //     new Product
            //     {
            //         Code = "P074",
            //         Name = "水泥",
            //         UnitId = unitBag?.Id,
            //         ProductCategoryId = materialCategory?.Id,
            //         Status = EntityStatus.Active,
            //         CreatedAt = DateTime.Now,
            //         CreatedBy = "System"
            //     },
            //     new Product
            //     {
            //         Code = "P075",
            //         Name = "砂石",
            //         UnitId = unitBag?.Id,
            //         ProductCategoryId = materialCategory?.Id,
            //         Status = EntityStatus.Active,
            //         CreatedAt = DateTime.Now,
            //         CreatedBy = "System"
            //     },
            //     new Product
            //     {
            //         Code = "P076",
            //         Name = "油漆",
            //         UnitId = unitPcs?.Id,
            //         ProductCategoryId = materialCategory?.Id,
            //         Status = EntityStatus.Active,
            //         CreatedAt = DateTime.Now,
            //         CreatedBy = "System"
            //     },
            //     new Product
            //     {
            //         Code = "P077",
            //         Name = "木材板",
            //         UnitId = unitSht?.Id,
            //         ProductCategoryId = materialCategory?.Id,
            //         Status = EntityStatus.Active,
            //         CreatedAt = DateTime.Now,
            //         CreatedBy = "System"
            //     },
            //     new Product
            //     {
            //         Code = "P078",
            //         Name = "玻璃纖維",
            //         UnitId = unitRol?.Id,
            //         ProductCategoryId = materialCategory?.Id,
            //         Status = EntityStatus.Active,
            //         CreatedAt = DateTime.Now,
            //         CreatedBy = "System"
            //     },
            //     new Product
            //     {
            //         Code = "P079",
            //         Name = "防水布",
            //         UnitId = unitRol?.Id,
            //         ProductCategoryId = materialCategory?.Id,
            //         Status = EntityStatus.Active,
            //         CreatedAt = DateTime.Now,
            //         CreatedBy = "System"
            //     },
            //     new Product
            //     {
            //         Code = "P080",
            //         Name = "砂紙",
            //         UnitId = unitSht?.Id,
            //         ProductCategoryId = materialCategory?.Id,
            //         Status = EntityStatus.Active,
            //         CreatedAt = DateTime.Now,
            //         CreatedBy = "System"
            //     },
            //     new Product
            //     {
            //         Code = "P081",
            //         Name = "矽利康",
            //         UnitId = unitPcs?.Id,
            //         ProductCategoryId = materialCategory?.Id,
            //         Status = EntityStatus.Active,
            //         CreatedAt = DateTime.Now,
            //         CreatedBy = "System"
            //     },
            //     new Product
            //     {
            //         Code = "P082",
            //         Name = "PVC管",
            //         UnitId = unitPcs?.Id,
            //         ProductCategoryId = materialCategory?.Id,
            //         Status = EntityStatus.Active,
            //         CreatedAt = DateTime.Now,
            //         CreatedBy = "System"
            //     },
            //     new Product
            //     {
            //         Code = "P083",
            //         Name = "電線",
            //         UnitId = unitRol?.Id,
            //         ProductCategoryId = materialCategory?.Id,
            //         Status = EntityStatus.Active,
            //         CreatedAt = DateTime.Now,
            //         CreatedBy = "System"
            //     },
            //     new Product
            //     {
            //         Code = "P084",
            //         Name = "開關插座",
            //         UnitId = unitPcs?.Id,
            //         ProductCategoryId = materialCategory?.Id,
            //         Status = EntityStatus.Active,
            //         CreatedAt = DateTime.Now,
            //         CreatedBy = "System"
            //     },
            //     new Product
            //     {
            //         Code = "P085",
            //         Name = "LED燈泡",
            //         UnitId = unitPcs?.Id,
            //         ProductCategoryId = electronicsCategory?.Id,
            //         Status = EntityStatus.Active,
            //         CreatedAt = DateTime.Now,
            //         CreatedBy = "System"
            //     }
            // };

            // await context.Products.AddRangeAsync(products);
            // await context.SaveChangesAsync();
        }
    }
}
