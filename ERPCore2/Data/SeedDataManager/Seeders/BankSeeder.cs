using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Models.Enums;
using ERPCore2.Data.SeedDataManager.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.SeedDataManager.Seeders
{
    /// <summary>
    /// 台灣金融機構代碼種子器（永久有效）
    /// Code = 金融機構代碼（3碼）、BankName = 中文名稱、BankNameEn = 英文名稱、SwiftCode = 國際匯款代碼
    /// 資料來源：金融監督管理委員會、中央銀行、各銀行官網
    /// </summary>
    public class BankSeeder : IDataSeeder
    {
        public int Order => 25;
        public string Name => "台灣金融機構代碼";

        public async Task SeedAsync(AppDbContext context)
        {
            if (await context.Banks.AnyAsync())
                return;

            var banks = new List<Bank>
            {
                // ── 公股銀行 ──────────────────────────────────────────────────────────────────────────────────────────
                new Bank { Code = "004", BankName = "台灣銀行",                 BankNameEn = "Bank of Taiwan",                          SwiftCode = "BKTWTWTP",    Status = EntityStatus.Active },
                new Bank { Code = "005", BankName = "土地銀行",                 BankNameEn = "Land Bank of Taiwan",                     SwiftCode = "LBOTTWTP",    Status = EntityStatus.Active },
                new Bank { Code = "006", BankName = "合作金庫商業銀行",         BankNameEn = "Taiwan Cooperative Bank",                 SwiftCode = "TACBTWTP",    Status = EntityStatus.Active },
                new Bank { Code = "007", BankName = "第一商業銀行",             BankNameEn = "First Commercial Bank",                   SwiftCode = "FCBKTWTP",    Status = EntityStatus.Active },
                new Bank { Code = "008", BankName = "華南商業銀行",             BankNameEn = "Hua Nan Commercial Bank",                 SwiftCode = "HNCBTWTP",    Status = EntityStatus.Active },
                new Bank { Code = "009", BankName = "彰化商業銀行",             BankNameEn = "Chang Hwa Commercial Bank",               SwiftCode = "CCBCTWTP",    Status = EntityStatus.Active },
                new Bank { Code = "015", BankName = "中華輸出入銀行",           BankNameEn = "The Export-Import Bank of the R.O.C.",    SwiftCode = "EROCTWTP",    Status = EntityStatus.Active },
                new Bank { Code = "017", BankName = "兆豐國際商業銀行",         BankNameEn = "Mega International Commercial Bank",      SwiftCode = "ICBCTWTP",    Status = EntityStatus.Active },
                new Bank { Code = "018", BankName = "全國農業金庫",             BankNameEn = "Agricultural Bank of Taiwan",             SwiftCode = "AGBTTWTP",    Status = EntityStatus.Active },
                new Bank { Code = "050", BankName = "台灣中小企業銀行",         BankNameEn = "Taiwan Business Bank",                   SwiftCode = "MTBKTWTP",    Status = EntityStatus.Active },
                new Bank { Code = "700", BankName = "中華郵政",                 BankNameEn = "Chunghwa Post",                          SwiftCode = null,          Status = EntityStatus.Active },

                // ── 本國民營銀行 ──────────────────────────────────────────────────────────────────────────────────────
                new Bank { Code = "011", BankName = "上海商業儲蓄銀行",         BankNameEn = "Shanghai Commercial & Savings Bank",      SwiftCode = "SCSBTWTX",    Status = EntityStatus.Active },
                new Bank { Code = "012", BankName = "台北富邦商業銀行",         BankNameEn = "Taipei Fubon Commercial Bank",            SwiftCode = "TPBKTWTP",    Status = EntityStatus.Active },
                new Bank { Code = "013", BankName = "國泰世華商業銀行",         BankNameEn = "Cathay United Bank",                     SwiftCode = "UWCBTWTP",    Status = EntityStatus.Active },
                new Bank { Code = "016", BankName = "高雄銀行",                 BankNameEn = "Bank of Kaohsiung",                      SwiftCode = null,          Status = EntityStatus.Active },
                new Bank { Code = "048", BankName = "王道商業銀行",             BankNameEn = "O-Bank",                                 SwiftCode = "ISESTWTP",    Status = EntityStatus.Active },
                new Bank { Code = "053", BankName = "台中商業銀行",             BankNameEn = "Taichung Commercial Bank",               SwiftCode = null,          Status = EntityStatus.Active },
                new Bank { Code = "054", BankName = "京城商業銀行",             BankNameEn = "King's Town Bank",                       SwiftCode = null,          Status = EntityStatus.Active },
                new Bank { Code = "101", BankName = "瑞興商業銀行",             BankNameEn = "Taipei Star Bank",                       SwiftCode = null,          Status = EntityStatus.Active },
                new Bank { Code = "102", BankName = "華泰商業銀行",             BankNameEn = "Hwatai Bank",                            SwiftCode = null,          Status = EntityStatus.Active },
                new Bank { Code = "103", BankName = "新光商業銀行",             BankNameEn = "Shin Kong Commercial Bank",              SwiftCode = null,          Status = EntityStatus.Active },
                new Bank { Code = "108", BankName = "陽信商業銀行",             BankNameEn = "Sunny Commercial Bank",                  SwiftCode = null,          Status = EntityStatus.Active },
                new Bank { Code = "118", BankName = "板信商業銀行",             BankNameEn = "Bank of Panhsin",                        SwiftCode = null,          Status = EntityStatus.Active },
                new Bank { Code = "147", BankName = "三信商業銀行",             BankNameEn = "Cota Commercial Bank",                   SwiftCode = null,          Status = EntityStatus.Active },
                new Bank { Code = "803", BankName = "聯邦商業銀行",             BankNameEn = "Union Bank of Taiwan",                   SwiftCode = "UBOTTWTP",    Status = EntityStatus.Active },
                new Bank { Code = "805", BankName = "遠東國際商業銀行",         BankNameEn = "Far Eastern International Bank",         SwiftCode = "FEINTWTP",    Status = EntityStatus.Active },
                new Bank { Code = "806", BankName = "元大商業銀行",             BankNameEn = "Yuanta Commercial Bank",                 SwiftCode = null,          Status = EntityStatus.Active },
                new Bank { Code = "807", BankName = "永豐商業銀行",             BankNameEn = "SinoPac Bank",                           SwiftCode = "SINOTWTP",    Status = EntityStatus.Active },
                new Bank { Code = "808", BankName = "玉山商業銀行",             BankNameEn = "E.Sun Commercial Bank",                  SwiftCode = "ESUNTWTP",    Status = EntityStatus.Active },
                new Bank { Code = "809", BankName = "凱基商業銀行",             BankNameEn = "KGI Bank",                              SwiftCode = null,          Status = EntityStatus.Active },
                new Bank { Code = "810", BankName = "星展(台灣)商業銀行",       BankNameEn = "DBS Bank (Taiwan)",                      SwiftCode = "DBSBTWTX",    Status = EntityStatus.Active },
                new Bank { Code = "812", BankName = "台新國際商業銀行",         BankNameEn = "Taishin International Bank",             SwiftCode = "TAISTWTP",    Status = EntityStatus.Active },
                new Bank { Code = "816", BankName = "安泰商業銀行",             BankNameEn = "EnTie Commercial Bank",                  SwiftCode = null,          Status = EntityStatus.Active },
                new Bank { Code = "822", BankName = "中國信託商業銀行",         BankNameEn = "CTBC Bank",                              SwiftCode = "CTCBTWTP",    Status = EntityStatus.Active },
                new Bank { Code = "823", BankName = "將來商業銀行",             BankNameEn = "NEXT Commercial Bank",                   SwiftCode = null,          Status = EntityStatus.Active },
                new Bank { Code = "824", BankName = "連線商業銀行",             BankNameEn = "LINE Bank Taiwan",                       SwiftCode = null,          Status = EntityStatus.Active },
                new Bank { Code = "826", BankName = "樂天國際商業銀行",         BankNameEn = "Rakuten International Commercial Bank",  SwiftCode = null,          Status = EntityStatus.Active },

                // ── 外商銀行（在台設立本地法人）──────────────────────────────────────────────────────────────────────
                new Bank { Code = "021", BankName = "花旗(台灣)商業銀行",       BankNameEn = "Citibank Taiwan",                        SwiftCode = "CITITWTP",    Status = EntityStatus.Active },
                new Bank { Code = "052", BankName = "渣打國際商業銀行",         BankNameEn = "Standard Chartered Bank (Taiwan)",       SwiftCode = "SCBLTWTX",    Status = EntityStatus.Active },
                new Bank { Code = "081", BankName = "匯豐(台灣)商業銀行",       BankNameEn = "HSBC Bank (Taiwan)",                     SwiftCode = "HSBCTWTP",    Status = EntityStatus.Active },

                // ── 外商銀行（在台分行）────────────────────────────────────────────────────────────────────────────────
                new Bank { Code = "020", BankName = "瑞穗銀行台北分行",         BankNameEn = "Mizuho Bank Taipei Branch",              SwiftCode = "MHCBTWTP",    Status = EntityStatus.Active },
                new Bank { Code = "022", BankName = "美國銀行台北分行",         BankNameEn = "Bank of America Taipei Branch",          SwiftCode = "BOFATW2X",    Status = EntityStatus.Active },
                new Bank { Code = "023", BankName = "盤谷銀行台北分行",         BankNameEn = "Bangkok Bank Taipei Branch",             SwiftCode = "BKKBTWTP",    Status = EntityStatus.Active },
                new Bank { Code = "072", BankName = "德意志銀行台北分行",       BankNameEn = "Deutsche Bank Taipei Branch",            SwiftCode = "DEUTTWTP",    Status = EntityStatus.Active },
                new Bank { Code = "075", BankName = "東亞銀行台北分行",         BankNameEn = "Bank of East Asia Taipei Branch",        SwiftCode = "BEASTWTP",    Status = EntityStatus.Active },
                new Bank { Code = "076", BankName = "摩根大通銀行台北分行",     BankNameEn = "JPMorgan Chase Bank Taipei Branch",      SwiftCode = "CHASTWA3",    Status = EntityStatus.Active },
                new Bank { Code = "082", BankName = "法國巴黎銀行台北分行",     BankNameEn = "BNP Paribas Taipei Branch",              SwiftCode = "BNPATWTP",    Status = EntityStatus.Active },
                new Bank { Code = "086", BankName = "東方匯理銀行台北分行",     BankNameEn = "Crédit Agricole CIB Taipei Branch",      SwiftCode = "CRLYTWTP",    Status = EntityStatus.Active },
                new Bank { Code = "098", BankName = "三菱日聯銀行台北分行",     BankNameEn = "MUFG Bank Taipei Branch",                SwiftCode = "BOTKTWTP",    Status = EntityStatus.Active },
                new Bank { Code = "321", BankName = "三井住友銀行台北分行",     BankNameEn = "SMBC Taipei Branch",                    SwiftCode = "SMBCTWTP",    Status = EntityStatus.Active },
                new Bank { Code = "324", BankName = "香港上海滙豐銀行台北分行", BankNameEn = "HSBC (Hong Kong) Taipei Branch",         SwiftCode = "HSBCHKHH",    Status = EntityStatus.Active },

                // ── 大陸地區銀行（在台分行）──────────────────────────────────────────────────────────────────────────
                new Bank { Code = "380", BankName = "中國銀行台北分行",         BankNameEn = "Bank of China Taipei Branch",            SwiftCode = "BKCHTWTP",    Status = EntityStatus.Active },
                new Bank { Code = "381", BankName = "交通銀行台北分行",         BankNameEn = "Bank of Communications Taipei Branch",   SwiftCode = "COMMTWTP",    Status = EntityStatus.Active },
                new Bank { Code = "382", BankName = "中國建設銀行台北分行",     BankNameEn = "China Construction Bank Taipei Branch",  SwiftCode = null,          Status = EntityStatus.Active },
            };

            await context.Banks.AddRangeAsync(banks);
            await context.SaveChangesAsync();
        }
    }
}
