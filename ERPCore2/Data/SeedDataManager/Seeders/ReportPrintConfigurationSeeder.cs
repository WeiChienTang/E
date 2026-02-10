using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Models.Enums;
using ERPCore2.Data.SeedDataManager.Interfaces;
using ERPCore2.Models;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.SeedDataManager.Seeders
{
    /// <summary>
    /// 報表列印配置種子資料
    /// 從 ReportRegistry 自動建立所有報表的預設配置
    /// </summary>
    public class ReportPrintConfigurationSeeder : IDataSeeder
    {
        public int Order => 20; // 在基礎資料（紙張設定等）之後執行
        public string Name => "報表列印配置";

        public async Task SeedAsync(AppDbContext context)
        {
            // 從 ReportRegistry 取得所有報表定義
            var allReports = ReportRegistry.GetAllReports();

            if (!allReports.Any())
            {
                return;
            }

            // 取得已存在的報表配置（透過 ReportId 檢查）
            var existingReportIds = await context.ReportPrintConfigurations
                .Select(r => r.ReportId)
                .ToListAsync();

            // 建立尚未存在的報表配置
            var newConfigurations = new List<ReportPrintConfiguration>();
            int codeSequence = existingReportIds.Count + 1;

            foreach (var report in allReports)
            {
                // 如果 ReportId 已存在，跳過
                if (existingReportIds.Contains(report.Id))
                {
                    continue;
                }

                // 建立新的報表列印配置
                var config = new ReportPrintConfiguration
                {
                    Code = $"RPC{codeSequence:D3}",
                    ReportId = report.Id,
                    ReportName = report.Name,
                    PrinterConfigurationId = null,  // 預設無印表機
                    PaperSettingId = null,          // 預設無紙張
                    Remarks = report.Description,
                    Status = report.IsEnabled ? EntityStatus.Active : EntityStatus.Inactive,
                    CreatedAt = DateTime.Now
                };

                newConfigurations.Add(config);
                codeSequence++;
            }

            // 批次新增
            if (newConfigurations.Any())
            {
                await context.ReportPrintConfigurations.AddRangeAsync(newConfigurations);
                await context.SaveChangesAsync();
            }
        }
    }
}
