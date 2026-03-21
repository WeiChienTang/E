using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Models.Enums;
using ERPCore2.Data.SeedDataManager.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.SeedDataManager.Seeders
{
    /// <summary>
    /// 測試用廠商資料種子器（僅測試環境使用）
    /// </summary>
    public class TestSupplierSeeder : IDataSeeder
    {
        public int Order => 51;
        public string Name => "測試廠商資料";

        public async Task SeedAsync(AppDbContext context)
        {
            if (await context.Suppliers.AnyAsync(s => s.Code != null && s.Code.StartsWith("TS")))
                return;

            var suppliers = new List<Supplier>
            {
                new Supplier
                {
                    Code = "TS001",
                    CompanyName = "台灣原料供應股份有限公司",
                    ContactPerson = "王原料",
                    SupplierContactPhone = "02-2345-1111",
                    TaxNumber = "11111111",
                    ResponsiblePerson = "王總經理",
                    ContactPhone = "02-2345-1111",
                    MobilePhone = "0912-111-111",
                    ContactAddress = "台北市內湖區內湖路一段100號",
                    Email = "supply@tw-raw.com.tw",
                    SupplierStatus = SupplierStatus.Active,
                    SupplierType = SupplierType.Manufacturer,
                    PaymentDays = 30,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new Supplier
                {
                    Code = "TS002",
                    CompanyName = "宏達電子零件有限公司",
                    ContactPerson = "李電子",
                    SupplierContactPhone = "02-2789-2222",
                    TaxNumber = "22222222",
                    ResponsiblePerson = "李老闆",
                    ContactPhone = "02-2789-2222",
                    MobilePhone = "0923-222-222",
                    ContactAddress = "台北市南港區經貿二路200號",
                    Email = "parts@htc-elec.com.tw",
                    SupplierStatus = SupplierStatus.Active,
                    SupplierType = SupplierType.Trader,
                    PaymentDays = 45,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new Supplier
                {
                    Code = "TS003",
                    CompanyName = "中台鋼鐵工業股份有限公司",
                    ContactPerson = "陳鋼鐵",
                    SupplierContactPhone = "04-2234-3333",
                    TaxNumber = "33333333",
                    ResponsiblePerson = "陳董",
                    ContactPhone = "04-2234-3333",
                    MobilePhone = "0934-333-333",
                    ContactAddress = "台中市大里區東榮路300號",
                    Email = "steel@ct-steel.com.tw",
                    SupplierStatus = SupplierStatus.Active,
                    SupplierType = SupplierType.Manufacturer,
                    PaymentDays = 60,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new Supplier
                {
                    Code = "TS004",
                    CompanyName = "南方化工原料股份有限公司",
                    ContactPerson = "林化工",
                    SupplierContactPhone = "06-2345-4444",
                    TaxNumber = "44444444",
                    ResponsiblePerson = "林董事長",
                    ContactPhone = "06-2345-4444",
                    MobilePhone = "0945-444-444",
                    ContactAddress = "台南市仁德區中山路500號",
                    Email = "chem@sf-chem.com.tw",
                    SupplierStatus = SupplierStatus.Active,
                    SupplierType = SupplierType.Manufacturer,
                    PaymentDays = 30,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new Supplier
                {
                    Code = "TS005",
                    CompanyName = "高雄機械零組件有限公司",
                    ContactPerson = "黃機械",
                    SupplierContactPhone = "07-3456-5555",
                    TaxNumber = "55555555",
                    ResponsiblePerson = "黃老闆",
                    ContactPhone = "07-3456-5555",
                    MobilePhone = "0956-555-555",
                    ContactAddress = "高雄市左營區博愛路400號",
                    Email = "parts@kh-mech.com.tw",
                    SupplierStatus = SupplierStatus.Active,
                    SupplierType = SupplierType.Trader,
                    PaymentDays = 45,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new Supplier
                {
                    Code = "TS006",
                    CompanyName = "北台包裝材料股份有限公司",
                    ContactPerson = "吳包裝",
                    SupplierContactPhone = "02-8912-6666",
                    TaxNumber = "66666666",
                    ResponsiblePerson = "吳總",
                    ContactPhone = "02-8912-6666",
                    MobilePhone = "0967-666-666",
                    ContactAddress = "新北市樹林區中山路200號",
                    Email = "pack@nt-pack.com.tw",
                    SupplierStatus = SupplierStatus.Active,
                    SupplierType = SupplierType.Manufacturer,
                    PaymentDays = 30,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new Supplier
                {
                    Code = "TS007",
                    CompanyName = "桃竹苗資訊科技有限公司",
                    ContactPerson = "鄭資訊",
                    SupplierContactPhone = "03-3456-7777",
                    TaxNumber = "77777777",
                    ResponsiblePerson = "鄭總監",
                    ContactPhone = "03-3456-7777",
                    MobilePhone = "0978-777-777",
                    ContactAddress = "桃園市中壢區中央西路一段100號",
                    Email = "it@tcc-tech.com.tw",
                    SupplierStatus = SupplierStatus.Active,
                    SupplierType = SupplierType.ServiceProvider,
                    PaymentDays = 30,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new Supplier
                {
                    Code = "TS008",
                    CompanyName = "彰雲嘉農業原料有限公司",
                    ContactPerson = "謝農業",
                    SupplierContactPhone = "05-5345-8888",
                    TaxNumber = "88888888",
                    ResponsiblePerson = "謝老闆",
                    ContactPhone = "05-5345-8888",
                    MobilePhone = "0989-888-888",
                    ContactAddress = "雲林縣虎尾鎮林科路100號",
                    Email = "agri@ccy-farm.com.tw",
                    SupplierStatus = SupplierStatus.Active,
                    SupplierType = SupplierType.Manufacturer,
                    PaymentDays = 60,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new Supplier
                {
                    Code = "TS009",
                    CompanyName = "新竹精密模具股份有限公司",
                    ContactPerson = "施模具",
                    SupplierContactPhone = "03-5345-9999",
                    TaxNumber = "99999999",
                    ResponsiblePerson = "施董",
                    ContactPhone = "03-5345-9999",
                    MobilePhone = "0900-999-999",
                    ContactAddress = "新竹縣竹北市縣政二路100號",
                    Email = "mold@hc-mold.com.tw",
                    SupplierStatus = SupplierStatus.Active,
                    SupplierType = SupplierType.Manufacturer,
                    PaymentDays = 45,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new Supplier
                {
                    Code = "TS010",
                    CompanyName = "東部木材原料股份有限公司",
                    ContactPerson = "許木材",
                    SupplierContactPhone = "03-8234-0011",
                    TaxNumber = "10101010",
                    ResponsiblePerson = "許老闆",
                    ContactPhone = "03-8234-0011",
                    MobilePhone = "0911-001-001",
                    ContactAddress = "花蓮縣吉安鄉中山路三段200號",
                    Email = "wood@east-timber.com.tw",
                    SupplierStatus = SupplierStatus.Active,
                    SupplierType = SupplierType.Trader,
                    PaymentDays = 30,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new Supplier
                {
                    Code = "TS011",
                    CompanyName = "全球進口貿易有限公司",
                    ContactPerson = "廖進口",
                    SupplierContactPhone = "02-2345-1122",
                    TaxNumber = "11221122",
                    ResponsiblePerson = "廖總",
                    ContactPhone = "02-2345-1122",
                    MobilePhone = "0922-112-112",
                    ContactAddress = "台北市松山區南京東路五段100號",
                    Email = "import@global-trade.com.tw",
                    SupplierStatus = SupplierStatus.Active,
                    SupplierType = SupplierType.Agent,
                    PaymentDays = 60,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new Supplier
                {
                    Code = "TS012",
                    CompanyName = "台中印刷耗材有限公司",
                    ContactPerson = "鍾印刷",
                    SupplierContactPhone = "04-2234-2233",
                    TaxNumber = "22332233",
                    ResponsiblePerson = "鍾老闆",
                    ContactPhone = "04-2234-2233",
                    MobilePhone = "0933-223-223",
                    ContactAddress = "台中市北區健行路300號",
                    Email = "print@tc-print.com.tw",
                    SupplierStatus = SupplierStatus.Active,
                    SupplierType = SupplierType.Trader,
                    PaymentDays = 30,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new Supplier
                {
                    Code = "TS013",
                    CompanyName = "南台灣橡膠製品股份有限公司",
                    ContactPerson = "陳橡膠",
                    SupplierContactPhone = "06-2345-3344",
                    TaxNumber = "33443344",
                    ResponsiblePerson = "陳董事長",
                    ContactPhone = "06-2345-3344",
                    MobilePhone = "0944-334-334",
                    ContactAddress = "台南市安南區安和路三段100號",
                    Email = "rubber@st-rubber.com.tw",
                    SupplierStatus = SupplierStatus.Active,
                    SupplierType = SupplierType.Manufacturer,
                    PaymentDays = 45,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new Supplier
                {
                    Code = "TS014",
                    CompanyName = "北部電線電纜有限公司",
                    ContactPerson = "林電纜",
                    SupplierContactPhone = "02-8912-4455",
                    TaxNumber = "44554455",
                    ResponsiblePerson = "林老闆",
                    ContactPhone = "02-8912-4455",
                    MobilePhone = "0955-445-445",
                    ContactAddress = "新北市新莊區中正路500號",
                    Email = "cable@nw-wire.com.tw",
                    SupplierStatus = SupplierStatus.Active,
                    SupplierType = SupplierType.Manufacturer,
                    PaymentDays = 60,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new Supplier
                {
                    Code = "TS015",
                    CompanyName = "西部塑膠原料股份有限公司",
                    ContactPerson = "賴塑膠",
                    SupplierContactPhone = "04-7234-5566",
                    TaxNumber = "55665566",
                    ResponsiblePerson = "賴董",
                    ContactPhone = "04-7234-5566",
                    MobilePhone = "0966-556-556",
                    ContactAddress = "彰化縣和美鎮彰美路一段200號",
                    Email = "plastic@west-plastic.com.tw",
                    SupplierStatus = SupplierStatus.Active,
                    SupplierType = SupplierType.Manufacturer,
                    PaymentDays = 30,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new Supplier
                {
                    Code = "TS016",
                    CompanyName = "中部螺絲五金有限公司",
                    ContactPerson = "張五金",
                    SupplierContactPhone = "04-2234-6677",
                    TaxNumber = "66776677",
                    ResponsiblePerson = "張老闆",
                    ContactPhone = "04-2234-6677",
                    MobilePhone = "0977-667-667",
                    ContactAddress = "台中市烏日區中山路一段100號",
                    Email = "hardware@c-screw.com.tw",
                    SupplierStatus = SupplierStatus.Active,
                    SupplierType = SupplierType.Trader,
                    PaymentDays = 45,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new Supplier
                {
                    Code = "TS017",
                    CompanyName = "高雄紙箱包裝工廠",
                    ContactPerson = "楊紙箱",
                    SupplierContactPhone = "07-3456-7788",
                    TaxNumber = "77887788",
                    ResponsiblePerson = "楊老闆",
                    ContactPhone = "07-3456-7788",
                    MobilePhone = "0988-778-778",
                    ContactAddress = "高雄市大社區中山路300號",
                    Email = "box@kh-carton.com.tw",
                    SupplierStatus = SupplierStatus.Active,
                    SupplierType = SupplierType.Manufacturer,
                    PaymentDays = 30,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new Supplier
                {
                    Code = "TS018",
                    CompanyName = "台北倉儲物流服務有限公司",
                    ContactPerson = "蘇物流",
                    SupplierContactPhone = "02-2345-8899",
                    TaxNumber = "88998899",
                    ResponsiblePerson = "蘇總",
                    ContactPhone = "02-2345-8899",
                    MobilePhone = "0999-889-889",
                    ContactAddress = "台北市北投區承德路七段200號",
                    Email = "storage@tp-warehouse.com.tw",
                    SupplierStatus = SupplierStatus.Active,
                    SupplierType = SupplierType.ServiceProvider,
                    PaymentDays = 30,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new Supplier
                {
                    Code = "TS019",
                    CompanyName = "嘉南紡紗原料股份有限公司",
                    ContactPerson = "洪紡紗",
                    SupplierContactPhone = "06-2234-9900",
                    TaxNumber = "99009900",
                    ResponsiblePerson = "洪董事長",
                    ContactPhone = "06-2234-9900",
                    MobilePhone = "0910-990-990",
                    ContactAddress = "台南市永康區永安路100號",
                    Email = "yarn@cn-fiber.com.tw",
                    SupplierStatus = SupplierStatus.Active,
                    SupplierType = SupplierType.Manufacturer,
                    PaymentDays = 60,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new Supplier
                {
                    Code = "TS020",
                    CompanyName = "國際代理進口有限公司",
                    ContactPerson = "陳代理",
                    SupplierContactPhone = "02-2789-0011",
                    TaxNumber = "00110011",
                    ResponsiblePerson = "陳副總",
                    ContactPhone = "02-2789-0011",
                    MobilePhone = "0921-001-122",
                    ContactAddress = "台北市中正區重慶南路一段100號",
                    Email = "agent@intl-agent.com.tw",
                    SupplierStatus = SupplierStatus.Active,
                    SupplierType = SupplierType.Agent,
                    PaymentDays = 45,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
            };

            await context.Suppliers.AddRangeAsync(suppliers);
            await context.SaveChangesAsync();
        }
    }
}
