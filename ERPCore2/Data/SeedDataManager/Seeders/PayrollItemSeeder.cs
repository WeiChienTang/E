using ERPCore2.Data.Context;
using ERPCore2.Data.Entities.Payroll;
using ERPCore2.Data.SeedDataManager.Interfaces;
using ERPCore2.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.SeedDataManager.Seeders
{
    /// <summary>
    /// 薪資項目種子資料 — 16 個預設項目
    /// </summary>
    public class PayrollItemSeeder : IDataSeeder
    {
        public int Order => 50; // 在基礎資料之後
        public string Name => "薪資項目";

        public async Task SeedAsync(AppDbContext context)
        {
            var now = DateTime.Now;

            // ── 修正既有項目的命名與 IsSystemItem 標記（冪等，可重複執行）──────────────
            // OT2 原命名為「休息日加班費」但實際用於平日加班後2小時；OT_HOLIDAY 原命名為「例假日加班費」
            var nameFixes = new Dictionary<string, (string Name, bool IsSystemItem)>
            {
                { "OT2",        ("平日加班費（後2小時）", true) },
                { "OT_HOLIDAY", ("休息日加班費",          true) },
                { "ABSENT",     ("曠職扣款",               true) },  // 計算引擎依賴此項目，應標為系統項目
            };
            bool anyFixed = false;
            foreach (var (code, (newName, isSystem)) in nameFixes)
            {
                var existing = await context.PayrollItems.FirstOrDefaultAsync(i => i.Code == code);
                if (existing != null && (existing.Name != newName || existing.IsSystemItem != isSystem))
                {
                    existing.Name = newName;
                    existing.IsSystemItem = isSystem;
                    anyFixed = true;
                }
            }
            if (anyFixed)
                await context.SaveChangesAsync();

            // 取得現有項目代碼，逐一檢查並補齊缺少的項目（支援既有資料庫新增項目）
            var existingCodes = await context.PayrollItems
                .Select(i => i.Code)
                .ToHashSetAsync();

            var allItems = new List<PayrollItem>
            {
                // ===== 收入項目 =====
                new PayrollItem
                {
                    Code = "BASE",
                    Name = "本薪",
                    ItemType = PayrollItemType.Income,
                    Category = PayrollItemCategory.Salary,
                    IsTaxable = true,
                    IsInsuranceBasis = true,
                    IsRetirementBasis = true,
                    IsSystemItem = true,
                    SortOrder = 1,
                    Status = EntityStatus.Active,
                    CreatedAt = now
                },
                new PayrollItem
                {
                    Code = "POS_ALLOWANCE",
                    Name = "職務加給",
                    ItemType = PayrollItemType.Income,
                    Category = PayrollItemCategory.Allowance,
                    IsTaxable = true,
                    IsInsuranceBasis = true,
                    IsRetirementBasis = true,
                    IsSystemItem = false,
                    SortOrder = 2,
                    Status = EntityStatus.Active,
                    CreatedAt = now
                },
                new PayrollItem
                {
                    Code = "MEAL",
                    Name = "伙食津貼",
                    ItemType = PayrollItemType.Income,
                    Category = PayrollItemCategory.Allowance,
                    IsTaxable = false,  // 每月 2400 免稅
                    IsInsuranceBasis = false,
                    IsRetirementBasis = false,
                    IsSystemItem = false,
                    SortOrder = 3,
                    Status = EntityStatus.Active,
                    CreatedAt = now
                },
                new PayrollItem
                {
                    Code = "TRANSPORT",
                    Name = "交通津貼",
                    ItemType = PayrollItemType.Income,
                    Category = PayrollItemCategory.Allowance,
                    IsTaxable = false,
                    IsInsuranceBasis = false,
                    IsRetirementBasis = false,
                    IsSystemItem = false,
                    SortOrder = 4,
                    Status = EntityStatus.Active,
                    CreatedAt = now
                },
                new PayrollItem
                {
                    Code = "OT1",
                    Name = "平日加班費（前2小時）",   // 勞基法第24條第1項第1款：×4/3
                    ItemType = PayrollItemType.Income,
                    Category = PayrollItemCategory.Overtime,
                    IsTaxable = true,
                    IsInsuranceBasis = false,
                    IsRetirementBasis = false,
                    IsSystemItem = true,
                    SortOrder = 5,
                    Status = EntityStatus.Active,
                    CreatedAt = now
                },
                new PayrollItem
                {
                    Code = "OT2",
                    Name = "平日加班費（後2小時）",   // 勞基法第24條第1項第2款：×5/3
                    ItemType = PayrollItemType.Income,
                    Category = PayrollItemCategory.Overtime,
                    IsTaxable = true,
                    IsInsuranceBasis = false,
                    IsRetirementBasis = false,
                    IsSystemItem = true,
                    SortOrder = 6,
                    Status = EntityStatus.Active,
                    CreatedAt = now
                },
                new PayrollItem
                {
                    Code = "OT_HOLIDAY",
                    Name = "休息日加班費",            // 勞基法第24條第2項：前2hr×4/3，後續×5/3
                    ItemType = PayrollItemType.Income,
                    Category = PayrollItemCategory.Overtime,
                    IsTaxable = true,
                    IsInsuranceBasis = false,
                    IsRetirementBasis = false,
                    IsSystemItem = true,
                    SortOrder = 7,
                    Status = EntityStatus.Active,
                    CreatedAt = now
                },
                new PayrollItem
                {
                    Code = "OT_NATIONAL",
                    Name = "國定假日加班費",          // 勞基法第39條：月薪已含假日薪，加給1倍時薪（合計×2）
                    ItemType = PayrollItemType.Income,
                    Category = PayrollItemCategory.Overtime,
                    IsTaxable = true,
                    IsInsuranceBasis = false,
                    IsRetirementBasis = false,
                    IsSystemItem = true,
                    SortOrder = 8,
                    Status = EntityStatus.Active,
                    CreatedAt = now
                },
                new PayrollItem
                {
                    Code = "BONUS",
                    Name = "績效獎金",
                    ItemType = PayrollItemType.Income,
                    Category = PayrollItemCategory.Bonus,
                    IsTaxable = true,
                    IsInsuranceBasis = false,
                    IsRetirementBasis = false,
                    IsSystemItem = false,
                    SortOrder = 8,
                    Status = EntityStatus.Active,
                    CreatedAt = now
                },

                // ===== 扣除項目 =====
                new PayrollItem
                {
                    Code = "LI_EE",
                    Name = "勞保費（員工負擔）",
                    ItemType = PayrollItemType.Deduction,
                    Category = PayrollItemCategory.Legal,
                    IsTaxable = false,
                    IsInsuranceBasis = false,
                    IsRetirementBasis = false,
                    IsSystemItem = true,
                    SortOrder = 10,
                    Status = EntityStatus.Active,
                    CreatedAt = now
                },
                new PayrollItem
                {
                    Code = "HI_EE",
                    Name = "健保費（員工負擔）",
                    ItemType = PayrollItemType.Deduction,
                    Category = PayrollItemCategory.Legal,
                    IsTaxable = false,
                    IsInsuranceBasis = false,
                    IsRetirementBasis = false,
                    IsSystemItem = true,
                    SortOrder = 11,
                    Status = EntityStatus.Active,
                    CreatedAt = now
                },
                new PayrollItem
                {
                    Code = "TAX",
                    Name = "代扣所得稅",
                    ItemType = PayrollItemType.Deduction,
                    Category = PayrollItemCategory.Legal,
                    IsTaxable = false,
                    IsInsuranceBasis = false,
                    IsRetirementBasis = false,
                    IsSystemItem = true,
                    SortOrder = 12,
                    Status = EntityStatus.Active,
                    CreatedAt = now
                },
                new PayrollItem
                {
                    Code = "RETIRE_VOL",
                    Name = "勞退自提（員工）",
                    ItemType = PayrollItemType.Deduction,
                    Category = PayrollItemCategory.Legal,
                    IsTaxable = false,
                    IsInsuranceBasis = false,
                    IsRetirementBasis = false,
                    IsSystemItem = true,
                    SortOrder = 13,
                    Status = EntityStatus.Active,
                    CreatedAt = now
                },
                new PayrollItem
                {
                    Code = "ABSENT",
                    Name = "曠職扣款",
                    ItemType = PayrollItemType.Deduction,
                    Category = PayrollItemCategory.Other,
                    IsTaxable = false,
                    IsInsuranceBasis = false,
                    IsRetirementBasis = false,
                    IsSystemItem = true,  // 計算引擎直接依賴此項目，不可刪除
                    SortOrder = 20,
                    Status = EntityStatus.Active,
                    CreatedAt = now
                },
                new PayrollItem
                {
                    Code = "SICK",
                    Name = "病假扣款",
                    ItemType = PayrollItemType.Deduction,
                    Category = PayrollItemCategory.Other,
                    IsTaxable = false,
                    IsInsuranceBasis = false,
                    IsRetirementBasis = false,
                    IsSystemItem = true,
                    SortOrder = 21,
                    Status = EntityStatus.Active,
                    CreatedAt = now
                },
                new PayrollItem
                {
                    Code = "LATE",
                    Name = "遲到扣款",
                    ItemType = PayrollItemType.Deduction,
                    Category = PayrollItemCategory.Other,
                    IsTaxable = false,
                    IsInsuranceBasis = false,
                    IsRetirementBasis = false,
                    IsSystemItem = false,
                    SortOrder = 22,
                    Status = EntityStatus.Active,
                    CreatedAt = now
                },
                new PayrollItem
                {
                    Code = "LOAN",
                    Name = "借款扣還",
                    ItemType = PayrollItemType.Deduction,
                    Category = PayrollItemCategory.Other,
                    IsTaxable = false,
                    IsInsuranceBasis = false,
                    IsRetirementBasis = false,
                    IsSystemItem = false,
                    SortOrder = 22,
                    Status = EntityStatus.Active,
                    CreatedAt = now
                },
                new PayrollItem
                {
                    Code = "OTHER_DED",
                    Name = "其他扣款",
                    ItemType = PayrollItemType.Deduction,
                    Category = PayrollItemCategory.Other,
                    IsTaxable = false,
                    IsInsuranceBasis = false,
                    IsRetirementBasis = false,
                    IsSystemItem = false,
                    SortOrder = 30,
                    Status = EntityStatus.Active,
                    CreatedAt = now
                }
            };

            var itemsToAdd = allItems.Where(i => !existingCodes.Contains(i.Code)).ToList();
            if (itemsToAdd.Any())
            {
                await context.PayrollItems.AddRangeAsync(itemsToAdd);
                await context.SaveChangesAsync();
            }
        }
    }
}
