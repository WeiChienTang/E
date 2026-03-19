using ERPCore2.Data.Entities;
using ERPCore2.Models.Barcode;
using ERPCore2.Models.Reports.FilterAttributes;
using ERPCore2.Models.Reports.FilterCriteria;
using ERPCore2.Services;

namespace ERPCore2.Models.Reports.FilterCriteria;

/// <summary>
/// 品項條碼批次列印篩選條件
/// 實作 IReportFilterCriteria 介面，整合至統一報表架構
/// </summary>
public class ItemBarcodeBatchPrintCriteria : IReportFilterCriteria
{
    /// <summary>
    /// 品項 ID 清單（空表示所有品項）
    /// </summary>
    [FilterFK(typeof(IItemService),
        Group = FilterGroup.Basic,
        Label = "指定品項",
        Placeholder = "搜尋品項...",
        EmptyMessage = "未選擇品項",
        Order = 1)]
    public List<int> ItemIds { get; set; } = new();

    /// <summary>
    /// 品項分類 ID 清單（可用於篩選特定分類的品項）
    /// </summary>
    [FilterFK(typeof(IItemCategoryService),
        Group = FilterGroup.Basic,
        Label = "品項分類",
        Placeholder = "搜尋品項分類...",
        EmptyMessage = "未選擇分類",
        Order = 2)]
    public List<int> CategoryIds { get; set; } = new();

    /// <summary>
    /// 是否只列印有條碼的品項
    /// </summary>
    public bool OnlyWithBarcode { get; set; } = true;

    /// <summary>
    /// 條碼尺寸
    /// </summary>
    public BarcodeSize BarcodeSize { get; set; } = BarcodeSize.Large;

    /// <summary>
    /// 每頁條碼數量
    /// </summary>
    public int BarcodesPerPage { get; set; } = 24;

    /// <summary>
    /// 是否顯示品項名稱
    /// </summary>
    public bool ShowItemName { get; set; } = true;

    /// <summary>
    /// 是否顯示品項編號
    /// </summary>
    public bool ShowItemCode { get; set; } = true;

    /// <summary>
    /// 每個品項的列印數量字典 (ItemId -> PrintQuantity)
    /// </summary>
    public Dictionary<int, int> PrintQuantities { get; set; } = new();

    /// <summary>
    /// 品項編號關鍵字（模糊搜尋）
    /// </summary>
    public string? ItemCodeKeyword { get; set; }

    /// <summary>
    /// 品項名稱關鍵字（模糊搜尋）
    /// </summary>
    public string? ItemNameKeyword { get; set; }

    /// <summary>
    /// 紙張設定（用於預覽渲染）
    /// </summary>
    public PaperSetting? PaperSetting { get; set; }

    /// <summary>
    /// 驗證篩選條件
    /// </summary>
    public bool Validate(out string? errorMessage)
    {
        if (BarcodesPerPage <= 0)
        {
            errorMessage = "每頁條碼數必須大於0";
            return false;
        }

        if (BarcodesPerPage > 100)
        {
            errorMessage = "每頁條碼數不能超過100";
            return false;
        }

        // 驗證列印數量
        foreach (var kvp in PrintQuantities)
        {
            if (kvp.Value <= 0)
            {
                errorMessage = $"品項ID {kvp.Key} 的列印數量必須大於0";
                return false;
            }
            if (kvp.Value > 100)
            {
                errorMessage = $"品項ID {kvp.Key} 的列印數量不能超過100";
                return false;
            }
        }

        // 必須選擇至少一個品項
        if (!ItemIds.Any() && !CategoryIds.Any() &&
            string.IsNullOrWhiteSpace(ItemCodeKeyword) &&
            string.IsNullOrWhiteSpace(ItemNameKeyword))
        {
            errorMessage = "請選擇至少一個品項或設定篩選條件";
            return false;
        }

        errorMessage = null;
        return true;
    }

    /// <summary>
    /// 轉換為查詢參數
    /// </summary>
    public Dictionary<string, object?> ToQueryParameters()
    {
        return new Dictionary<string, object?>
        {
            ["productIds"] = ItemIds.Any() ? ItemIds : null,
            ["categoryIds"] = CategoryIds.Any() ? CategoryIds : null,
            ["onlyWithBarcode"] = OnlyWithBarcode,
            ["barcodeSize"] = BarcodeSize,
            ["barcodesPerPage"] = BarcodesPerPage,
            ["showItemName"] = ShowItemName,
            ["showItemCode"] = ShowItemCode,
            ["printQuantities"] = PrintQuantities.Any() ? PrintQuantities : null,
            ["productCodeKeyword"] = string.IsNullOrWhiteSpace(ItemCodeKeyword) ? null : ItemCodeKeyword,
            ["productNameKeyword"] = string.IsNullOrWhiteSpace(ItemNameKeyword) ? null : ItemNameKeyword
        };
    }

    /// <summary>
    /// 轉換為舊版 ItemBarcodePrintCriteria（相容現有報表服務）
    /// </summary>
    public ItemBarcodePrintCriteria ToLegacyCriteria()
    {
        return new ItemBarcodePrintCriteria
        {
            ItemIds = ItemIds,
            CategoryIds = CategoryIds,
            OnlyWithBarcode = OnlyWithBarcode,
            BarcodeSize = BarcodeSize,
            BarcodesPerPage = BarcodesPerPage,
            ShowItemName = ShowItemName,
            ShowItemCode = ShowItemCode,
            PrintQuantities = PrintQuantities
        };
    }

    /// <summary>
    /// 取得篩選條件摘要
    /// </summary>
    public string GetSummary()
    {
        var summary = new List<string>();

        if (ItemIds.Any())
        {
            summary.Add($"選擇 {ItemIds.Count} 個品項");
        }
        else if (CategoryIds.Any())
        {
            summary.Add($"篩選 {CategoryIds.Count} 個分類");
        }
        else
        {
            summary.Add("全部品項");
        }

        var totalQuantity = PrintQuantities.Values.Sum();
        if (totalQuantity > 0)
        {
            summary.Add($"共 {totalQuantity} 張");
        }

        summary.Add($"尺寸：{GetBarcodeSizeText()}");

        return string.Join(" | ", summary);
    }

    private string GetBarcodeSizeText() => BarcodeSize switch
    {
        BarcodeSize.Small => "小",
        BarcodeSize.Medium => "中",
        BarcodeSize.Large => "大",
        _ => "中"
    };
}
