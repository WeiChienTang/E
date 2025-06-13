using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Data.SeedDataManager.Helpers;
using ERPCore2.Data.SeedDataManager.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.SeedDataManager.Seeders
{
    /// <summary>
    /// 供應商資料種子器
    /// </summary>
    public class SupplierSeeder : IDataSeeder
    {
        public int Order => 3;
        public string Name => "供應商資料";

        public async Task SeedAsync(AppDbContext context)
        {
            await SeedSuppliersAsync(context);
        }

        /// <summary>
        /// 初始化示例供應商資料
        /// </summary>
        private static async Task SeedSuppliersAsync(AppDbContext context)
        {
            if (await context.Suppliers.AnyAsync())
                return; // 供應商資料已存在

            // 取得供應商類型和行業類型
            var manufacturerType = await context.SupplierTypes.FirstOrDefaultAsync(st => st.TypeName == "製造商");
            var agentType = await context.SupplierTypes.FirstOrDefaultAsync(st => st.TypeName == "代理商");
            var wholesalerType = await context.SupplierTypes.FirstOrDefaultAsync(st => st.TypeName == "批發商");
            var retailerType = await context.SupplierTypes.FirstOrDefaultAsync(st => st.TypeName == "零售商");
            var serviceType = await context.SupplierTypes.FirstOrDefaultAsync(st => st.TypeName == "服務商");
            var materialType = await context.SupplierTypes.FirstOrDefaultAsync(st => st.TypeName == "原料供應商");
            var equipmentType = await context.SupplierTypes.FirstOrDefaultAsync(st => st.TypeName == "設備供應商");
            var softwareType = await context.SupplierTypes.FirstOrDefaultAsync(st => st.TypeName == "軟體供應商");

            var itIndustry = await context.IndustryTypes.FirstOrDefaultAsync(it => it.IndustryTypeCode == "IT");
            var mfgIndustry = await context.IndustryTypes.FirstOrDefaultAsync(it => it.IndustryTypeCode == "MFG");
            var svcIndustry = await context.IndustryTypes.FirstOrDefaultAsync(it => it.IndustryTypeCode == "SVC");
            var trdIndustry = await context.IndustryTypes.FirstOrDefaultAsync(it => it.IndustryTypeCode == "TRD");
            var conIndustry = await context.IndustryTypes.FirstOrDefaultAsync(it => it.IndustryTypeCode == "CON");
            var finIndustry = await context.IndustryTypes.FirstOrDefaultAsync(it => it.IndustryTypeCode == "FIN");
            var rtlIndustry = await context.IndustryTypes.FirstOrDefaultAsync(it => it.IndustryTypeCode == "RTL");

            var suppliers = new[]
            {
                new Supplier
                {
                    SupplierCode = "S001",
                    CompanyName = "精密科技製造股份有限公司",
                    ContactPerson = "張總經理",
                    TaxNumber = "20123456",
                    PaymentTerms = "月結30天",
                    CreditLimit = 5000000,
                    SupplierTypeId = manufacturerType?.Id ?? 1,
                    IndustryTypeId = mfgIndustry?.Id ?? 1,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now.AddDays(-90),
                    CreatedBy = "System"
                },
                new Supplier
                {
                    SupplierCode = "S002",
                    CompanyName = "台灣電子元件有限公司",
                    ContactPerson = "李經理",
                    TaxNumber = "20234567",
                    PaymentTerms = "貨到付款",
                    CreditLimit = 3000000,
                    SupplierTypeId = manufacturerType?.Id ?? 1,
                    IndustryTypeId = mfgIndustry?.Id ?? 1,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now.AddDays(-85),
                    CreatedBy = "System"
                },
                new Supplier
                {
                    SupplierCode = "S003",
                    CompanyName = "全球軟體代理商",
                    ContactPerson = "王協理",
                    TaxNumber = "20345678",
                    PaymentTerms = "預付款",
                    CreditLimit = 2000000,
                    SupplierTypeId = agentType?.Id ?? 2,
                    IndustryTypeId = itIndustry?.Id ?? 2,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now.AddDays(-80),
                    CreatedBy = "System"
                },
                new Supplier
                {
                    SupplierCode = "S004",
                    CompanyName = "優質原料供應企業",
                    ContactPerson = "劉副總",
                    TaxNumber = "20456789",
                    PaymentTerms = "月結45天",
                    CreditLimit = 4000000,
                    SupplierTypeId = materialType?.Id ?? 6,
                    IndustryTypeId = mfgIndustry?.Id ?? 1,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now.AddDays(-75),
                    CreatedBy = "System"
                },
                new Supplier
                {
                    SupplierCode = "S005",
                    CompanyName = "智慧設備工程公司",
                    ContactPerson = "陳總監",
                    TaxNumber = "20567890",
                    PaymentTerms = "分期付款",
                    CreditLimit = 8000000,
                    SupplierTypeId = equipmentType?.Id ?? 7,
                    IndustryTypeId = mfgIndustry?.Id ?? 1,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now.AddDays(-70),
                    CreatedBy = "System"
                },
                new Supplier
                {
                    SupplierCode = "S006",
                    CompanyName = "專業服務顧問集團",
                    ContactPerson = "黃執行長",
                    TaxNumber = "20678901",
                    PaymentTerms = "月結15天",
                    CreditLimit = 1500000,
                    SupplierTypeId = serviceType?.Id ?? 5,
                    IndustryTypeId = svcIndustry?.Id ?? 3,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now.AddDays(-65),
                    CreatedBy = "System"
                },
                new Supplier
                {
                    SupplierCode = "S007",
                    CompanyName = "國際貿易批發商",
                    ContactPerson = "林董事長",
                    TaxNumber = "20789012",
                    PaymentTerms = "現金交易",
                    CreditLimit = 6000000,
                    SupplierTypeId = wholesalerType?.Id ?? 3,
                    IndustryTypeId = trdIndustry?.Id ?? 4,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now.AddDays(-60),
                    CreatedBy = "System"
                },
                new Supplier
                {
                    SupplierCode = "S008",
                    CompanyName = "創新科技軟體公司",
                    ContactPerson = "吳技術長",
                    TaxNumber = "20890123",
                    PaymentTerms = "月結60天",
                    CreditLimit = 2500000,
                    SupplierTypeId = softwareType?.Id ?? 8,
                    IndustryTypeId = itIndustry?.Id ?? 2,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now.AddDays(-55),
                    CreatedBy = "System"
                },
                new Supplier
                {
                    SupplierCode = "S009",
                    CompanyName = "高精密機械製造廠",
                    ContactPerson = "蔡廠長",
                    TaxNumber = "20901234",
                    PaymentTerms = "月結30天",
                    CreditLimit = 7000000,
                    SupplierTypeId = manufacturerType?.Id ?? 1,
                    IndustryTypeId = mfgIndustry?.Id ?? 1,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now.AddDays(-50),
                    CreatedBy = "System"
                },
                new Supplier
                {
                    SupplierCode = "S010",
                    CompanyName = "環保材料科技股份有限公司",
                    ContactPerson = "鄭研發經理",
                    TaxNumber = "21012345",
                    PaymentTerms = "貨到付款",
                    CreditLimit = 3500000,
                    SupplierTypeId = materialType?.Id ?? 6,
                    IndustryTypeId = mfgIndustry?.Id ?? 1,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now.AddDays(-45),
                    CreatedBy = "System"
                },
                new Supplier
                {
                    SupplierCode = "S011",
                    CompanyName = "數位行銷服務公司",
                    ContactPerson = "謝營運長",
                    TaxNumber = "21123456",
                    PaymentTerms = "預付50%",
                    CreditLimit = 1200000,
                    SupplierTypeId = serviceType?.Id ?? 5,
                    IndustryTypeId = svcIndustry?.Id ?? 3,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now.AddDays(-40),
                    CreatedBy = "System"
                },
                new Supplier
                {
                    SupplierCode = "S012",
                    CompanyName = "建築工程設備租賃",
                    ContactPerson = "施工程師",
                    TaxNumber = "21234567",
                    PaymentTerms = "日結",
                    CreditLimit = 5500000,
                    SupplierTypeId = equipmentType?.Id ?? 7,
                    IndustryTypeId = conIndustry?.Id ?? 5,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now.AddDays(-35),
                    CreatedBy = "System"
                },
                new Supplier
                {
                    SupplierCode = "S013",
                    CompanyName = "進口食品原料商",
                    ContactPerson = "賴採購經理",
                    TaxNumber = "21345678",
                    PaymentTerms = "月結21天",
                    CreditLimit = 2800000,
                    SupplierTypeId = wholesalerType?.Id ?? 3,
                    IndustryTypeId = trdIndustry?.Id ?? 4,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now.AddDays(-30),
                    CreatedBy = "System"
                },
                new Supplier
                {
                    SupplierCode = "S014",
                    CompanyName = "雲端服務解決方案",
                    ContactPerson = "何系統架構師",
                    TaxNumber = "21456789",
                    PaymentTerms = "年繳",
                    CreditLimit = 1800000,
                    SupplierTypeId = softwareType?.Id ?? 8,
                    IndustryTypeId = itIndustry?.Id ?? 2,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now.AddDays(-25),
                    CreatedBy = "System"
                },
                new Supplier
                {
                    SupplierCode = "S015",
                    CompanyName = "辦公用品批發中心",
                    ContactPerson = "沈業務主任",
                    TaxNumber = "21567890",
                    PaymentTerms = "月結14天",
                    CreditLimit = 800000,
                    SupplierTypeId = wholesalerType?.Id ?? 3,
                    IndustryTypeId = rtlIndustry?.Id ?? 7,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now.AddDays(-20),
                    CreatedBy = "System"
                },
                new Supplier
                {
                    SupplierCode = "S016",
                    CompanyName = "專業物流運輸服務",
                    ContactPerson = "徐物流經理",
                    TaxNumber = "21678901",
                    PaymentTerms = "月結7天",
                    CreditLimit = 2200000,
                    SupplierTypeId = serviceType?.Id ?? 5,
                    IndustryTypeId = svcIndustry?.Id ?? 3,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now.AddDays(-18),
                    CreatedBy = "System"
                },
                new Supplier
                {
                    SupplierCode = "S017",
                    CompanyName = "工業自動化設備公司",
                    ContactPerson = "游自動化工程師",
                    TaxNumber = "21789012",
                    PaymentTerms = "分期三期",
                    CreditLimit = 9000000,
                    SupplierTypeId = equipmentType?.Id ?? 7,
                    IndustryTypeId = mfgIndustry?.Id ?? 1,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now.AddDays(-15),
                    CreatedBy = "System"
                },
                new Supplier
                {
                    SupplierCode = "S018",
                    CompanyName = "國際化學原料進口商",
                    ContactPerson = "湯化工專員",
                    TaxNumber = "21890123",
                    PaymentTerms = "貨到付款",
                    CreditLimit = 4500000,
                    SupplierTypeId = materialType?.Id ?? 6,
                    IndustryTypeId = mfgIndustry?.Id ?? 1,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now.AddDays(-12),
                    CreatedBy = "System"
                },
                new Supplier
                {
                    SupplierCode = "S019",
                    CompanyName = "金融科技解決方案",
                    ContactPerson = "范金融顧問",
                    TaxNumber = "21901234",
                    PaymentTerms = "月結30天",
                    CreditLimit = 3200000,
                    SupplierTypeId = softwareType?.Id ?? 8,
                    IndustryTypeId = finIndustry?.Id ?? 6,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now.AddDays(-10),
                    CreatedBy = "System"
                },
                new Supplier
                {
                    SupplierCode = "S020",
                    CompanyName = "清潔維護服務集團",
                    ContactPerson = "溫服務經理",
                    TaxNumber = "22012345",
                    PaymentTerms = "月結15天",
                    CreditLimit = 900000,
                    SupplierTypeId = serviceType?.Id ?? 5,
                    IndustryTypeId = svcIndustry?.Id ?? 3,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now.AddDays(-8),
                    CreatedBy = "System"
                },
                new Supplier
                {
                    SupplierCode = "S021",
                    CompanyName = "生技醫療器材製造",
                    ContactPerson = "盧醫材研發",
                    TaxNumber = "22123456",
                    PaymentTerms = "月結45天",
                    CreditLimit = 6500000,
                    SupplierTypeId = manufacturerType?.Id ?? 1,
                    IndustryTypeId = mfgIndustry?.Id ?? 1,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now.AddDays(-7),
                    CreatedBy = "System"
                },
                new Supplier
                {
                    SupplierCode = "S022",
                    CompanyName = "電商平台代理商",
                    ContactPerson = "董電商經理",
                    TaxNumber = "22234567",
                    PaymentTerms = "現金交易",
                    CreditLimit = 1600000,
                    SupplierTypeId = agentType?.Id ?? 2,
                    IndustryTypeId = rtlIndustry?.Id ?? 7,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now.AddDays(-6),
                    CreatedBy = "System"
                },
                new Supplier
                {
                    SupplierCode = "S023",
                    CompanyName = "再生能源設備供應商",
                    ContactPerson = "鍾能源工程師",
                    TaxNumber = "22345678",
                    PaymentTerms = "分期付款",
                    CreditLimit = 12000000,
                    SupplierTypeId = equipmentType?.Id ?? 7,
                    IndustryTypeId = mfgIndustry?.Id ?? 1,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now.AddDays(-5),
                    CreatedBy = "System"
                },
                new Supplier
                {
                    SupplierCode = "S024",
                    CompanyName = "多媒體設計服務工作室",
                    ContactPerson = "韓創意總監",
                    TaxNumber = "22456789",
                    PaymentTerms = "預付70%",
                    CreditLimit = 700000,
                    SupplierTypeId = serviceType?.Id ?? 5,
                    IndustryTypeId = svcIndustry?.Id ?? 3,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now.AddDays(-4),
                    CreatedBy = "System"
                },
                new Supplier
                {
                    SupplierCode = "S025",
                    CompanyName = "智慧家居產品製造",
                    ContactPerson = "龔產品經理",
                    TaxNumber = "22567890",
                    PaymentTerms = "月結30天",
                    CreditLimit = 3800000,
                    SupplierTypeId = manufacturerType?.Id ?? 1,
                    IndustryTypeId = mfgIndustry?.Id ?? 1,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now.AddDays(-3),
                    CreatedBy = "System"
                },
                new Supplier
                {
                    SupplierCode = "S026",
                    CompanyName = "進口精品代理商",
                    ContactPerson = "嚴品牌經理",
                    TaxNumber = "22678901",
                    PaymentTerms = "預付款",
                    CreditLimit = 2400000,
                    SupplierTypeId = agentType?.Id ?? 2,
                    IndustryTypeId = trdIndustry?.Id ?? 4,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now.AddDays(-2),
                    CreatedBy = "System"
                },
                new Supplier
                {
                    SupplierCode = "S027",
                    CompanyName = "資安防護軟體公司",
                    ContactPerson = "巫資安專家",
                    TaxNumber = "22789012",
                    PaymentTerms = "年繳優惠",
                    CreditLimit = 2100000,
                    SupplierTypeId = softwareType?.Id ?? 8,
                    IndustryTypeId = itIndustry?.Id ?? 2,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now.AddDays(-1),
                    CreatedBy = "System"
                },
                new Supplier
                {
                    SupplierCode = "S028",
                    CompanyName = "食品包裝材料供應",
                    ContactPerson = "聶包材專員",
                    TaxNumber = "22890123",
                    PaymentTerms = "月結21天",
                    CreditLimit = 1900000,
                    SupplierTypeId = materialType?.Id ?? 6,
                    IndustryTypeId = mfgIndustry?.Id ?? 1,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new Supplier
                {
                    SupplierCode = "S029",
                    CompanyName = "人力資源顧問公司",
                    ContactPerson = "簡人資顧問",
                    TaxNumber = "22901234",
                    PaymentTerms = "月結10天",
                    CreditLimit = 1100000,
                    SupplierTypeId = serviceType?.Id ?? 5,
                    IndustryTypeId = svcIndustry?.Id ?? 3,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new Supplier
                {
                    SupplierCode = "S030",
                    CompanyName = "測試檢驗設備製造",
                    ContactPerson = "顏品管工程師",
                    TaxNumber = "23012345",
                    PaymentTerms = "分期六期",
                    CreditLimit = 8500000,
                    SupplierTypeId = equipmentType?.Id ?? 7,
                    IndustryTypeId = mfgIndustry?.Id ?? 1,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new Supplier
                {
                    SupplierCode = "S031",
                    CompanyName = "紡織原料進口貿易",
                    ContactPerson = "喻紡織專員",
                    TaxNumber = "23123456",
                    PaymentTerms = "貨到付款",
                    CreditLimit = 3600000,
                    SupplierTypeId = materialType?.Id ?? 6,
                    IndustryTypeId = trdIndustry?.Id ?? 4,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new Supplier
                {
                    SupplierCode = "S032",
                    CompanyName = "區塊鏈技術服務",
                    ContactPerson = "傅區塊鏈專家",
                    TaxNumber = "23234567",
                    PaymentTerms = "專案付款",
                    CreditLimit = 2700000,
                    SupplierTypeId = serviceType?.Id ?? 5,
                    IndustryTypeId = itIndustry?.Id ?? 2,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new Supplier
                {
                    SupplierCode = "S033",
                    CompanyName = "醫療耗材批發商",
                    ContactPerson = "程醫材採購",
                    TaxNumber = "23345678",
                    PaymentTerms = "月結14天",
                    CreditLimit = 4200000,
                    SupplierTypeId = wholesalerType?.Id ?? 3,
                    IndustryTypeId = rtlIndustry?.Id ?? 7,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new Supplier
                {
                    SupplierCode = "S034",
                    CompanyName = "AI人工智慧解決方案",
                    ContactPerson = "童AI研發經理",
                    TaxNumber = "23456789",
                    PaymentTerms = "里程碑付款",
                    CreditLimit = 4800000,
                    SupplierTypeId = softwareType?.Id ?? 8,
                    IndustryTypeId = itIndustry?.Id ?? 2,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new Supplier
                {
                    SupplierCode = "S035",
                    CompanyName = "環境工程顧問公司",
                    ContactPerson = "燕環工顧問",
                    TaxNumber = "23567890",
                    PaymentTerms = "月結30天",
                    CreditLimit = 3300000,
                    SupplierTypeId = serviceType?.Id ?? 5,
                    IndustryTypeId = svcIndustry?.Id ?? 3,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new Supplier
                {
                    SupplierCode = "S036",
                    CompanyName = "光電產品製造廠",
                    ContactPerson = "葉光電工程師",
                    TaxNumber = "23678901",
                    PaymentTerms = "月結45天",
                    CreditLimit = 5600000,
                    SupplierTypeId = manufacturerType?.Id ?? 1,
                    IndustryTypeId = mfgIndustry?.Id ?? 1,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new Supplier
                {
                    SupplierCode = "S037",
                    CompanyName = "國際快遞物流代理",
                    ContactPerson = "詹物流專員",
                    TaxNumber = "23789012",
                    PaymentTerms = "週結",
                    CreditLimit = 1400000,
                    SupplierTypeId = agentType?.Id ?? 2,
                    IndustryTypeId = svcIndustry?.Id ?? 3,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new Supplier
                {
                    SupplierCode = "S038",
                    CompanyName = "精密儀器設備商",
                    ContactPerson = "閔精密技師",
                    TaxNumber = "23890123",
                    PaymentTerms = "分期四期",
                    CreditLimit = 10500000,
                    SupplierTypeId = equipmentType?.Id ?? 7,
                    IndustryTypeId = mfgIndustry?.Id ?? 1,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new Supplier
                {
                    SupplierCode = "S039",
                    CompanyName = "資料庫管理軟體商",
                    ContactPerson = "單資料庫專家",
                    TaxNumber = "23901234",
                    PaymentTerms = "授權年費",
                    CreditLimit = 2900000,
                    SupplierTypeId = softwareType?.Id ?? 8,
                    IndustryTypeId = itIndustry?.Id ?? 2,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new Supplier
                {
                    SupplierCode = "S040",
                    CompanyName = "暫停合作製造商",
                    ContactPerson = "廖停用聯絡人",
                    TaxNumber = "24012345",
                    PaymentTerms = "暫停",
                    CreditLimit = 0,
                    SupplierTypeId = manufacturerType?.Id ?? 1,
                    IndustryTypeId = mfgIndustry?.Id ?? 1,
                    Status = EntityStatus.Inactive,
                    CreatedAt = DateTime.Now.AddDays(-120),
                    CreatedBy = "System"
                }
            };

            await context.Suppliers.AddRangeAsync(suppliers);
            await context.SaveChangesAsync();
        }
    }
}
