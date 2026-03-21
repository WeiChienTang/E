using Microsoft.EntityFrameworkCore;
using ERPCore2.Data.Context;
using ERPCore2.Data.SeedDataManager.Interfaces;
using ERPCore2.Models.Enums;

namespace ERPCore2.Data.SeedDataManager.Seeders
{
    /// <summary>
    /// 為現有會計科目設定 CashFlowCategory，使 FN014 現金流量表（IAS 7 間接法）能正確運作。
    ///
    /// 分類邏輯（依商業會計項目表 112 年度科目代碼結構）：
    ///
    /// Cash（現金及約當現金）：
    ///   科目代碼以 "111" 開頭
    ///
    /// OperatingNonCash（非現金調整）：
    ///   名稱含「累計折舊」或「累計攤銷」（加回折舊/攤銷費用）
    ///
    /// OperatingWorkingCapital（流動資金變動）：
    ///   流動資產（應收票據/帳款/存貨/預付等）：118x, 119x, 120x, 121x, 122x,
    ///                                         123x, 124x, 1261-1270（預付款項）
    ///   流動負債（應付票據/帳款/預收/應計）：216x, 217x, 218x, 219x-220x, 221x, 222x
    ///   不含短期借款（211x, 212x → Financing）
    ///
    /// Investing（投資活動）：
    ///   非流動金融資產：131x-138x
    ///   不動產廠房設備：139x-146x（Level 4 代碼含 139-146 起頭）
    ///   無形資產：147x-154x
    ///   其他非流動資產：155x-159x
    ///   （已標記 OperatingNonCash 的科目不會被覆寫）
    ///
    /// Financing（籌資活動）：
    ///   短期借款：211x, 212x
    ///   一年內到期長期負債：223x
    ///   非流動負債：231x-239x
    ///   資本/資本公積：311x-329x
    ///   庫藏股票：351x, 352x
    ///
    /// 冪等性：若已有任何科目設定 CashFlowCategory，則直接略過（不覆寫手動設定）。
    /// </summary>
    public class AccountItemCashFlowCategorySeeder : IDataSeeder
    {
        public int Order => 16; // 緊接在 AccountItemSeeder(15) 之後

        public string Name => "會計科目現金流量分類";

        public async Task SeedAsync(AppDbContext context)
        {
            // 若已有任何科目設定 CashFlowCategory，跳過（保留手動設定）
            if (await context.AccountItems.AnyAsync(a => a.CashFlowCategory != null))
                return;

            // ---- 1. Cash（現金及約當現金）----
            await TagAsync(context,
                a => a.Code != null && a.Code.StartsWith("111"),
                CashFlowCategory.Cash);

            // ---- 2. OperatingNonCash（折舊/攤銷）----
            // 名稱含「累計折舊」或「累計攤銷」的科目（非現金費用加回項目）
            await TagAsync(context,
                a => (a.Name.Contains("累計折舊") || a.Name.Contains("累計攤銷"))
                     && a.CashFlowCategory == null,
                CashFlowCategory.OperatingNonCash);

            // ---- 3. OperatingWorkingCapital（流動資產）----
            // 應收票據 (118x)、應收帳款 (119x)、應收建造合約款 (120x)
            // 其他應收款 (121x)、所得稅資產 (122x)
            await TagCurrentAssets(context, new[] { "118", "119", "120", "121", "122" });

            // 存貨 (123x, 124x)
            await TagCurrentAssets(context, new[] { "123", "124" });

            // 預付款項 (126-127 parent 下的 Level 4 代碼 1261-1270)
            await TagAsync(context,
                a => a.Code != null && a.Code.Length >= 4
                     && (a.Code.StartsWith("126") || a.Code.StartsWith("127"))
                     && a.CashFlowCategory == null,
                CashFlowCategory.OperatingWorkingCapital);

            // 其他流動資產 (128x, 129x)
            await TagCurrentAssets(context, new[] { "128", "129" });

            // ---- 4. OperatingWorkingCapital（流動負債）----
            // 應付票據 (216x)、應付帳款 (217x)、應付建造合約款 (218x)
            // 其他應付款 (219x, 220x)、所得稅負債 (221x)、預收款項 (222x)
            await TagCurrentLiabilities(context,
                new[] { "216", "217", "218", "219", "220", "221", "222" });

            // ---- 5. Investing（非流動資產）----
            // 非流動金融資產 (131x-138x)
            var investingAssetPrefixes = new List<string>();
            for (int i = 131; i <= 138; i++)
                investingAssetPrefixes.Add(i.ToString());

            // PPE (139-146 parent 下的代碼，Level 4 代碼以 139-146 開頭)
            foreach (var n in new[] { "139", "140", "141", "142", "143", "144", "145", "146" })
                investingAssetPrefixes.Add(n);

            // 無形資產 (147-154)
            foreach (var n in new[] { "147", "148", "149", "150", "151", "152", "153", "154" })
                investingAssetPrefixes.Add(n);

            // 其他非流動 (155-159)
            foreach (var n in new[] { "155", "156", "157", "158", "159" })
                investingAssetPrefixes.Add(n);

            await TagAsync(context,
                a => a.Code != null && a.Code.Length >= 4
                     && investingAssetPrefixes.Any(p => a.Code.StartsWith(p))
                     && a.CashFlowCategory == null,   // 不覆寫已標記的 OperatingNonCash
                CashFlowCategory.Investing);

            // ---- 6. Financing（籌資活動）----
            // 短期借款 (211x, 212x)
            await TagAsync(context,
                a => a.Code != null && a.Code.Length >= 4
                     && (a.Code.StartsWith("211") || a.Code.StartsWith("212"))
                     && a.CashFlowCategory == null,
                CashFlowCategory.Financing);

            // 一年內到期長期負債 (223x)
            await TagAsync(context,
                a => a.Code != null && a.Code.Length >= 4
                     && a.Code.StartsWith("223")
                     && a.CashFlowCategory == null,
                CashFlowCategory.Financing);

            // 非流動負債 (231x-239x)
            var nonCurrentLiabPrefixes = new List<string>();
            for (int i = 231; i <= 239; i++)
                nonCurrentLiabPrefixes.Add(i.ToString());

            await TagAsync(context,
                a => a.Code != null && a.Code.Length >= 4
                     && nonCurrentLiabPrefixes.Any(p => a.Code.StartsWith(p))
                     && a.CashFlowCategory == null,
                CashFlowCategory.Financing);

            // 資本/資本公積 (311x-329x)、庫藏股票 (351x, 352x)
            var equityPrefixes = new List<string>();
            for (int i = 311; i <= 319; i++) equityPrefixes.Add(i.ToString());
            for (int i = 321; i <= 329; i++) equityPrefixes.Add(i.ToString());
            equityPrefixes.Add("351");
            equityPrefixes.Add("352");

            await TagAsync(context,
                a => a.Code != null && a.Code.Length >= 4
                     && equityPrefixes.Any(p => a.Code.StartsWith(p))
                     && a.CashFlowCategory == null,
                CashFlowCategory.Financing);
        }

        // ===== 輔助方法 =====

        private static async Task TagCurrentAssets(AppDbContext context, string[] prefixes)
        {
            await TagAsync(context,
                a => a.Code != null && a.Code.Length >= 4
                     && prefixes.Any(p => a.Code.StartsWith(p))
                     && a.CashFlowCategory == null,
                CashFlowCategory.OperatingWorkingCapital);
        }

        private static async Task TagCurrentLiabilities(AppDbContext context, string[] prefixes)
        {
            await TagAsync(context,
                a => a.Code != null && a.Code.Length >= 4
                     && prefixes.Any(p => a.Code.StartsWith(p))
                     && a.CashFlowCategory == null,
                CashFlowCategory.OperatingWorkingCapital);
        }

        private static async Task TagAsync(
            AppDbContext context,
            System.Linq.Expressions.Expression<Func<ERPCore2.Data.Entities.AccountItem, bool>> predicate,
            CashFlowCategory category)
        {
            var accounts = await context.AccountItems.Where(predicate).ToListAsync();
            foreach (var account in accounts)
                account.CashFlowCategory = category;
            if (accounts.Any())
                await context.SaveChangesAsync();
        }
    }
}
