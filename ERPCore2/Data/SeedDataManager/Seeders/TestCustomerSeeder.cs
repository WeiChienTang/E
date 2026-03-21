using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Models.Enums;
using ERPCore2.Data.SeedDataManager.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.SeedDataManager.Seeders
{
    /// <summary>
    /// 測試用客戶資料種子器（僅測試環境使用）
    /// </summary>
    public class TestCustomerSeeder : IDataSeeder
    {
        public int Order => 50;
        public string Name => "測試客戶資料";

        public async Task SeedAsync(AppDbContext context)
        {
            if (await context.Customers.AnyAsync(c => c.Code != null && c.Code.StartsWith("TC")))
                return;

            var customers = new List<Customer>
            {
                new Customer
                {
                    Code = "TC001",
                    CompanyName = "台灣科技股份有限公司",
                    ContactPerson = "王大明",
                    CompanyContactPhone = "02-2345-6789",
                    TaxNumber = "12345678",
                    ResponsiblePerson = "王董事長",
                    ContactPhone = "02-2345-6789",
                    MobilePhone = "0912-345-678",
                    ContactAddress = "台北市信義區信義路五段7號",
                    Email = "contact@twtech.com.tw",
                    CustomerStatus = CustomerStatus.Active,
                    CustomerType = CustomerType.Enterprise,
                    CustomerSource = CustomerSource.BusinessDevelopment,
                    CreditRating = CreditRating.A,
                    PaymentDays = 30,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new Customer
                {
                    Code = "TC002",
                    CompanyName = "永豐國際貿易有限公司",
                    ContactPerson = "李小華",
                    CompanyContactPhone = "02-2789-0123",
                    TaxNumber = "23456789",
                    ResponsiblePerson = "李總經理",
                    ContactPhone = "02-2789-0123",
                    MobilePhone = "0923-456-789",
                    ContactAddress = "台北市中山區中山北路二段99號",
                    Email = "info@yf-trade.com.tw",
                    CustomerStatus = CustomerStatus.Active,
                    CustomerType = CustomerType.Enterprise,
                    CustomerSource = CustomerSource.Referral,
                    CreditRating = CreditRating.B,
                    PaymentDays = 45,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new Customer
                {
                    Code = "TC003",
                    CompanyName = "東方機械工業股份有限公司",
                    ContactPerson = "陳建國",
                    CompanyContactPhone = "04-2234-5678",
                    TaxNumber = "34567890",
                    ResponsiblePerson = "陳董",
                    ContactPhone = "04-2234-5678",
                    MobilePhone = "0934-567-890",
                    ContactAddress = "台中市西屯區台灣大道三段99號",
                    Email = "service@east-mech.com.tw",
                    CustomerStatus = CustomerStatus.Active,
                    CustomerType = CustomerType.Enterprise,
                    CustomerSource = CustomerSource.Exhibition,
                    CreditRating = CreditRating.A,
                    PaymentDays = 30,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new Customer
                {
                    Code = "TC004",
                    CompanyName = "南星電子股份有限公司",
                    ContactPerson = "林美玲",
                    CompanyContactPhone = "06-2345-6780",
                    TaxNumber = "45678901",
                    ResponsiblePerson = "林董事長",
                    ContactPhone = "06-2345-6780",
                    MobilePhone = "0945-678-901",
                    ContactAddress = "台南市東區東門路一段100號",
                    Email = "sales@southstar-elec.com.tw",
                    CustomerStatus = CustomerStatus.Active,
                    CustomerType = CustomerType.Enterprise,
                    CustomerSource = CustomerSource.Internet,
                    CreditRating = CreditRating.B,
                    PaymentDays = 60,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new Customer
                {
                    Code = "TC005",
                    CompanyName = "高雄化工原料股份有限公司",
                    ContactPerson = "黃志偉",
                    CompanyContactPhone = "07-3456-7890",
                    TaxNumber = "56789012",
                    ResponsiblePerson = "黃總裁",
                    ContactPhone = "07-3456-7890",
                    MobilePhone = "0956-789-012",
                    ContactAddress = "高雄市三民區建國二路200號",
                    Email = "purchase@kh-chem.com.tw",
                    CustomerStatus = CustomerStatus.Active,
                    CustomerType = CustomerType.Enterprise,
                    CustomerSource = CustomerSource.BusinessDevelopment,
                    CreditRating = CreditRating.A,
                    PaymentDays = 30,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new Customer
                {
                    Code = "TC006",
                    CompanyName = "新北精密零件有限公司",
                    ContactPerson = "吳俊德",
                    CompanyContactPhone = "02-8912-3456",
                    TaxNumber = "67890123",
                    ResponsiblePerson = "吳老闆",
                    ContactPhone = "02-8912-3456",
                    MobilePhone = "0967-890-123",
                    ContactAddress = "新北市板橋區文化路一段50號",
                    Email = "order@nb-precision.com.tw",
                    CustomerStatus = CustomerStatus.Active,
                    CustomerType = CustomerType.Enterprise,
                    CustomerSource = CustomerSource.Referral,
                    CreditRating = CreditRating.C,
                    PaymentDays = 60,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new Customer
                {
                    Code = "TC007",
                    CompanyName = "桃園航空物流股份有限公司",
                    ContactPerson = "鄭宗翰",
                    CompanyContactPhone = "03-3456-7891",
                    TaxNumber = "78901234",
                    ResponsiblePerson = "鄭總經理",
                    ContactPhone = "03-3456-7891",
                    MobilePhone = "0978-901-234",
                    ContactAddress = "桃園市大園區航站南路1號",
                    Email = "logistics@ty-air.com.tw",
                    CustomerStatus = CustomerStatus.Active,
                    CustomerType = CustomerType.Enterprise,
                    CustomerSource = CustomerSource.Exhibition,
                    CreditRating = CreditRating.A,
                    PaymentDays = 30,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new Customer
                {
                    Code = "TC008",
                    CompanyName = "苗栗農產品加工廠",
                    ContactPerson = "謝春花",
                    CompanyContactPhone = "037-345-678",
                    TaxNumber = "89012345",
                    ResponsiblePerson = "謝老闆娘",
                    ContactPhone = "037-345-678",
                    MobilePhone = "0989-012-345",
                    ContactAddress = "苗栗縣竹南鎮中正路100號",
                    Email = "farm@miaoli-agri.com.tw",
                    CustomerStatus = CustomerStatus.Active,
                    CustomerType = CustomerType.Enterprise,
                    CustomerSource = CustomerSource.Other,
                    CreditRating = CreditRating.B,
                    PaymentDays = 45,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new Customer
                {
                    Code = "TC009",
                    CompanyName = "彰化紡織股份有限公司",
                    ContactPerson = "施文龍",
                    CompanyContactPhone = "04-7234-5678",
                    TaxNumber = "90123456",
                    ResponsiblePerson = "施董",
                    ContactPhone = "04-7234-5678",
                    MobilePhone = "0900-123-456",
                    ContactAddress = "彰化縣彰化市中正路一段300號",
                    Email = "textile@ch-fabric.com.tw",
                    CustomerStatus = CustomerStatus.Active,
                    CustomerType = CustomerType.Enterprise,
                    CustomerSource = CustomerSource.BusinessDevelopment,
                    CreditRating = CreditRating.B,
                    PaymentDays = 60,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new Customer
                {
                    Code = "TC010",
                    CompanyName = "嘉義木業加工有限公司",
                    ContactPerson = "許志明",
                    CompanyContactPhone = "05-2234-5679",
                    TaxNumber = "01234567",
                    ResponsiblePerson = "許總",
                    ContactPhone = "05-2234-5679",
                    MobilePhone = "0911-234-567",
                    ContactAddress = "嘉義市東區垂楊路200號",
                    Email = "wood@cy-timber.com.tw",
                    CustomerStatus = CustomerStatus.Active,
                    CustomerType = CustomerType.Enterprise,
                    CustomerSource = CustomerSource.Referral,
                    CreditRating = CreditRating.C,
                    PaymentDays = 30,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new Customer
                {
                    Code = "TC011",
                    CompanyName = "雲林食品工業股份有限公司",
                    ContactPerson = "廖雅惠",
                    CompanyContactPhone = "05-5345-6789",
                    TaxNumber = "11223344",
                    ResponsiblePerson = "廖董",
                    ContactPhone = "05-5345-6789",
                    MobilePhone = "0922-345-678",
                    ContactAddress = "雲林縣斗六市鎮南路100號",
                    Email = "food@yl-food.com.tw",
                    CustomerStatus = CustomerStatus.Active,
                    CustomerType = CustomerType.Enterprise,
                    CustomerSource = CustomerSource.Internet,
                    CreditRating = CreditRating.A,
                    PaymentDays = 45,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new Customer
                {
                    Code = "TC012",
                    CompanyName = "屏東冷凍食品有限公司",
                    ContactPerson = "鍾文傑",
                    CompanyContactPhone = "08-7234-5678",
                    TaxNumber = "22334455",
                    ResponsiblePerson = "鍾老闆",
                    ContactPhone = "08-7234-5678",
                    MobilePhone = "0933-456-789",
                    ContactAddress = "屏東縣屏東市自由路200號",
                    Email = "frozen@pt-food.com.tw",
                    CustomerStatus = CustomerStatus.Active,
                    CustomerType = CustomerType.Enterprise,
                    CustomerSource = CustomerSource.Exhibition,
                    CreditRating = CreditRating.B,
                    PaymentDays = 30,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new Customer
                {
                    Code = "TC013",
                    CompanyName = "宜蘭觀光農場有限公司",
                    ContactPerson = "陳淑芬",
                    CompanyContactPhone = "039-345-678",
                    TaxNumber = "33445566",
                    ResponsiblePerson = "陳老闆",
                    ContactPhone = "039-345-678",
                    MobilePhone = "0944-567-890",
                    ContactAddress = "宜蘭縣宜蘭市中山路三段50號",
                    Email = "farm@yl-agri.com.tw",
                    CustomerStatus = CustomerStatus.Active,
                    CustomerType = CustomerType.Enterprise,
                    CustomerSource = CustomerSource.Other,
                    CreditRating = CreditRating.C,
                    PaymentDays = 60,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new Customer
                {
                    Code = "TC014",
                    CompanyName = "花蓮石材開發股份有限公司",
                    ContactPerson = "林哲宇",
                    CompanyContactPhone = "03-8234-5678",
                    TaxNumber = "44556677",
                    ResponsiblePerson = "林董",
                    ContactPhone = "03-8234-5678",
                    MobilePhone = "0955-678-901",
                    ContactAddress = "花蓮縣花蓮市中正路100號",
                    Email = "stone@hl-marble.com.tw",
                    CustomerStatus = CustomerStatus.Active,
                    CustomerType = CustomerType.Enterprise,
                    CustomerSource = CustomerSource.BusinessDevelopment,
                    CreditRating = CreditRating.A,
                    PaymentDays = 30,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new Customer
                {
                    Code = "TC015",
                    CompanyName = "台東農業生技有限公司",
                    ContactPerson = "賴俊賢",
                    CompanyContactPhone = "089-345-678",
                    TaxNumber = "55667788",
                    ResponsiblePerson = "賴總",
                    ContactPhone = "089-345-678",
                    MobilePhone = "0966-789-012",
                    ContactAddress = "台東縣台東市中山路200號",
                    Email = "bio@tt-agri.com.tw",
                    CustomerStatus = CustomerStatus.Active,
                    CustomerType = CustomerType.Enterprise,
                    CustomerSource = CustomerSource.Internet,
                    CreditRating = CreditRating.B,
                    PaymentDays = 45,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new Customer
                {
                    Code = "TC016",
                    CompanyName = "澎湖水產品有限公司",
                    ContactPerson = "張美鳳",
                    CompanyContactPhone = "06-9234-5678",
                    TaxNumber = "66778899",
                    ResponsiblePerson = "張老闆娘",
                    ContactPhone = "06-9234-5678",
                    MobilePhone = "0977-890-123",
                    ContactAddress = "澎湖縣馬公市中正路50號",
                    Email = "seafood@ph-marine.com.tw",
                    CustomerStatus = CustomerStatus.Active,
                    CustomerType = CustomerType.Enterprise,
                    CustomerSource = CustomerSource.Referral,
                    CreditRating = CreditRating.C,
                    PaymentDays = 30,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new Customer
                {
                    Code = "TC017",
                    CompanyName = "金門高粱酒業股份有限公司",
                    ContactPerson = "楊德成",
                    CompanyContactPhone = "082-345-678",
                    TaxNumber = "77889900",
                    ResponsiblePerson = "楊董",
                    ContactPhone = "082-345-678",
                    MobilePhone = "0988-901-234",
                    ContactAddress = "金門縣金城鎮民生路100號",
                    Email = "sales@km-liquor.com.tw",
                    CustomerStatus = CustomerStatus.Active,
                    CustomerType = CustomerType.Enterprise,
                    CustomerSource = CustomerSource.Exhibition,
                    CreditRating = CreditRating.A,
                    PaymentDays = 60,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new Customer
                {
                    Code = "TC018",
                    CompanyName = "新竹半導體設備有限公司",
                    ContactPerson = "蘇世昌",
                    CompanyContactPhone = "03-5345-6789",
                    TaxNumber = "88990011",
                    ResponsiblePerson = "蘇董",
                    ContactPhone = "03-5345-6789",
                    MobilePhone = "0999-012-345",
                    ContactAddress = "新竹市東區光復路二段101號",
                    Email = "equip@hc-semi.com.tw",
                    CustomerStatus = CustomerStatus.Active,
                    CustomerType = CustomerType.Enterprise,
                    CustomerSource = CustomerSource.BusinessDevelopment,
                    CreditRating = CreditRating.A,
                    PaymentDays = 30,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new Customer
                {
                    Code = "TC019",
                    CompanyName = "基隆港埠物流股份有限公司",
                    ContactPerson = "洪信義",
                    CompanyContactPhone = "02-2422-3456",
                    TaxNumber = "99001122",
                    ResponsiblePerson = "洪總",
                    ContactPhone = "02-2422-3456",
                    MobilePhone = "0910-123-456",
                    ContactAddress = "基隆市仁愛區港西街100號",
                    Email = "port@kl-logistics.com.tw",
                    CustomerStatus = CustomerStatus.Active,
                    CustomerType = CustomerType.Enterprise,
                    CustomerSource = CustomerSource.Referral,
                    CreditRating = CreditRating.B,
                    PaymentDays = 45,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new Customer
                {
                    Code = "TC020",
                    CompanyName = "張家個人工作室",
                    ContactPerson = "張小明",
                    CompanyContactPhone = "02-2567-8901",
                    TaxNumber = null,
                    ResponsiblePerson = "張小明",
                    ContactPhone = "02-2567-8901",
                    MobilePhone = "0921-234-567",
                    ContactAddress = "台北市大安區和平東路一段300號",
                    Email = "studio@personal.com.tw",
                    CustomerStatus = CustomerStatus.Active,
                    CustomerType = CustomerType.Individual,
                    CustomerSource = CustomerSource.Internet,
                    CreditRating = CreditRating.C,
                    PaymentDays = 0,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
            };

            await context.Customers.AddRangeAsync(customers);
            await context.SaveChangesAsync();
        }
    }
}
