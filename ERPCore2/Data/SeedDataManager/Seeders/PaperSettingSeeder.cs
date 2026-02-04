using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Data.SeedDataManager.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.SeedDataManager.Seeders
{
    /// <summary>
    /// 紙張設定種子資料
    /// 提供常用的標準紙張格式
    /// </summary>
    public class PaperSettingSeeder : IDataSeeder
    {
        public int Order => 7; // 在基礎資料之後
        public string Name => "紙張設定";

        public async Task SeedAsync(AppDbContext context)
        {
            // 檢查是否已經有紙張設定資料
            bool hasData = await context.PaperSettings.AnyAsync();

            if (hasData)
            {
                return;
            }

            // 建立常用紙張設定
            var paperSettings = new List<PaperSetting>
            {
                // A 系列紙張（ISO 216 標準）
                new PaperSetting
                {
                    Code = "PAPER001",
                    Name = "A4",
                    Width = 21.0m,      // 210mm = 21cm
                    Height = 29.7m,     // 297mm = 29.7cm
                    TopMargin = 1.0m,
                    BottomMargin = 1.0m,
                    LeftMargin = 1.0m,
                    RightMargin = 1.0m,
                    Remarks = "國際標準 A4 紙張，最常用的辦公用紙",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now
                },
                new PaperSetting
                {
                    Code = "PAPER002",
                    Name = "A3",
                    Width = 29.7m,      // 297mm = 29.7cm
                    Height = 42.0m,     // 420mm = 42cm
                    TopMargin = 1.0m,
                    BottomMargin = 1.0m,
                    LeftMargin = 1.0m,
                    RightMargin = 1.0m,
                    Remarks = "國際標準 A3 紙張，適合大型報表或海報",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now
                },
                new PaperSetting
                {
                    Code = "PAPER003",
                    Name = "A5",
                    Width = 14.8m,      // 148mm = 14.8cm
                    Height = 21.0m,     // 210mm = 21cm
                    TopMargin = 0.5m,
                    BottomMargin = 0.5m,
                    LeftMargin = 0.5m,
                    RightMargin = 0.5m,
                    Remarks = "國際標準 A5 紙張，適合小型文件或手冊",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now
                },

                // B 系列紙張
                new PaperSetting
                {
                    Code = "PAPER004",
                    Name = "B4",
                    Width = 25.0m,      // 250mm = 25cm
                    Height = 35.3m,     // 353mm = 35.3cm
                    TopMargin = 1.0m,
                    BottomMargin = 1.0m,
                    LeftMargin = 1.0m,
                    RightMargin = 1.0m,
                    Remarks = "B4 紙張，介於 A3 和 A4 之間",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now
                },
                new PaperSetting
                {
                    Code = "PAPER005",
                    Name = "B5",
                    Width = 17.6m,      // 176mm = 17.6cm
                    Height = 25.0m,     // 250mm = 25cm
                    TopMargin = 0.8m,
                    BottomMargin = 0.8m,
                    LeftMargin = 0.8m,
                    RightMargin = 0.8m,
                    Remarks = "B5 紙張，常用於書籍或雜誌",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now
                },

                // 美規紙張
                new PaperSetting
                {
                    Code = "PAPER006",
                    Name = "Letter",
                    Width = 21.59m,     // 8.5 inch = 21.59cm
                    Height = 27.94m,    // 11 inch = 27.94cm
                    TopMargin = 1.0m,
                    BottomMargin = 1.0m,
                    LeftMargin = 1.0m,
                    RightMargin = 1.0m,
                    Remarks = "美規 Letter 紙張 (8.5\" x 11\")",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now
                },
                new PaperSetting
                {
                    Code = "PAPER007",
                    Name = "Legal",
                    Width = 21.59m,     // 8.5 inch = 21.59cm
                    Height = 35.56m,    // 14 inch = 35.56cm
                    TopMargin = 1.0m,
                    BottomMargin = 1.0m,
                    LeftMargin = 1.0m,
                    RightMargin = 1.0m,
                    Remarks = "美規 Legal 紙張 (8.5\" x 14\")，常用於法律文件",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now
                },

                // 出貨單 / 發票常用格式
                new PaperSetting
                {
                    Code = "PAPER008",
                    Name = "出貨單 (三聯式)",
                    Width = 21.0m,
                    Height = 14.0m,
                    TopMargin = 0.5m,
                    BottomMargin = 0.5m,
                    LeftMargin = 0.5m,
                    RightMargin = 0.5m,
                    Remarks = "常用出貨單格式，三聯複寫紙",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now
                },
                new PaperSetting
                {
                    Code = "PAPER009",
                    Name = "統一發票",
                    Width = 19.0m,
                    Height = 11.0m,
                    TopMargin = 0.3m,
                    BottomMargin = 0.3m,
                    LeftMargin = 0.3m,
                    RightMargin = 0.3m,
                    Remarks = "台灣統一發票標準格式",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now
                },

                // 標籤紙
                new PaperSetting
                {
                    Code = "PAPER010",
                    Name = "標籤紙 (4x6)",
                    Width = 10.16m,     // 4 inch = 10.16cm
                    Height = 15.24m,    // 6 inch = 15.24cm
                    TopMargin = 0.2m,
                    BottomMargin = 0.2m,
                    LeftMargin = 0.2m,
                    RightMargin = 0.2m,
                    Remarks = "常用出貨標籤紙 (4\" x 6\")",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now
                },
                new PaperSetting
                {
                    Code = "PAPER011",
                    Name = "條碼標籤 (小)",
                    Width = 5.0m,
                    Height = 3.0m,
                    TopMargin = 0.1m,
                    BottomMargin = 0.1m,
                    LeftMargin = 0.1m,
                    RightMargin = 0.1m,
                    Remarks = "小型條碼標籤紙",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now
                },

                // 收據紙
                new PaperSetting
                {
                    Code = "PAPER012",
                    Name = "熱感收據紙 (80mm)",
                    Width = 8.0m,
                    Height = 30.0m,     // 預設長度，實際可連續列印
                    TopMargin = 0.2m,
                    BottomMargin = 0.2m,
                    LeftMargin = 0.2m,
                    RightMargin = 0.2m,
                    Remarks = "POS 收據機常用 80mm 熱感紙",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now
                },
                new PaperSetting
                {
                    Code = "PAPER013",
                    Name = "熱感收據紙 (58mm)",
                    Width = 5.8m,
                    Height = 30.0m,
                    TopMargin = 0.1m,
                    BottomMargin = 0.1m,
                    LeftMargin = 0.1m,
                    RightMargin = 0.1m,
                    Remarks = "小型 POS 收據機常用 58mm 熱感紙",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now
                },

                // 台灣常用格式
                new PaperSetting
                {
                    Code = "PAPER014",
                    Name = "中一刀",
                    Width = 26.7m,      // 267mm = 26.7cm
                    Height = 38.1m,     // 381mm = 38.1cm (15英吋)
                    TopMargin = 0m,
                    BottomMargin = 0m,
                    LeftMargin = 0m,
                    RightMargin = 0m,
                    Remarks = "台灣常用中一刀紙張，適合報表列印",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now
                },
                new PaperSetting
                {
                    Code = "PAPER015",
                    Name = "中二刀",
                    Width = 26.7m,      // 267mm = 26.7cm
                    Height = 19.05m,    // 190.5mm = 19.05cm (中一刀的一半)
                    TopMargin = 0.5m,
                    BottomMargin = 0.5m,
                    LeftMargin = 1.0m,
                    RightMargin = 1.0m,
                    Remarks = "台灣常用中二刀紙張，為中一刀的一半高度",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now
                },
                new PaperSetting
                {
                    Code = "PAPER016",
                    Name = "報表紙全張",
                    Width = 24.13m,     // 9.5 inch = 24.13cm
                    Height = 27.94m,    // 11 inch = 27.94cm
                    TopMargin = 1.0m,
                    BottomMargin = 1.0m,
                    LeftMargin = 1.27m,  // 0.5 inch = 1.27cm (針孔邊)
                    RightMargin = 1.27m, // 0.5 inch = 1.27cm (針孔邊)
                    Remarks = "連續報表紙全張 (9.5\" x 11\")，針式印表機常用規格",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now
                }
            };

            await context.PaperSettings.AddRangeAsync(paperSettings);
            await context.SaveChangesAsync();
        }
    }
}
