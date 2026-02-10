using ERPCore2.Data.Entities;
using ERPCore2.Models.Barcode;
using ERPCore2.Models.Reports.FilterCriteria;

namespace ERPCore2.Models.Reports.FilterCriteria;

/// <summary>
/// 商品條碼批次列印篩選條件
/// 實作 IReportFilterCriteria 介面，整合至統一報表架構
/// </summary>
public class ProductBarcodeBatchPrintCriteria : IReportFilterCriteria
{
    /// <summary>
    /// 商品 ID 清單（空表示所有商品）
    /// </summary>
    public List<int> ProductIds { get; set; } = new();
    
    /// <summary>
    /// 商品分類 ID 清單（可用於篩選特定分類的商品）
    /// </summary>
    public List<int> CategoryIds { get; set; } = new();
    
    /// <summary>
    /// 是否只列印有條碼的商品
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
    /// 是否顯示商品名稱
    /// </summary>
    public bool ShowProductName { get; set; } = true;
    
    /// <summary>
    /// 是否顯示商品編號
    /// </summary>
    public bool ShowProductCode { get; set; } = true;
    
    /// <summary>
    /// 每個商品的列印數量字典 (ProductId -> PrintQuantity)
    /// </summary>
    public Dictionary<int, int> PrintQuantities { get; set; } = new();
    
    /// <summary>
    /// 商品編號關鍵字（模糊搜尋）
    /// </summary>
    public string? ProductCodeKeyword { get; set; }
    
    /// <summary>
    /// 商品名稱關鍵字（模糊搜尋）
    /// </summary>
    public string? ProductNameKeyword { get; set; }
    
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
                errorMessage = $"商品ID {kvp.Key} 的列印數量必須大於0";
                return false;
            }
            if (kvp.Value > 100)
            {
                errorMessage = $"商品ID {kvp.Key} 的列印數量不能超過100";
                return false;
            }
        }
        
        // 必須選擇至少一個商品
        if (!ProductIds.Any() && !CategoryIds.Any() && 
            string.IsNullOrWhiteSpace(ProductCodeKeyword) && 
            string.IsNullOrWhiteSpace(ProductNameKeyword))
        {
            errorMessage = "請選擇至少一個商品或設定篩選條件";
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
            ["productIds"] = ProductIds.Any() ? ProductIds : null,
            ["categoryIds"] = CategoryIds.Any() ? CategoryIds : null,
            ["onlyWithBarcode"] = OnlyWithBarcode,
            ["barcodeSize"] = BarcodeSize,
            ["barcodesPerPage"] = BarcodesPerPage,
            ["showProductName"] = ShowProductName,
            ["showProductCode"] = ShowProductCode,
            ["printQuantities"] = PrintQuantities.Any() ? PrintQuantities : null,
            ["productCodeKeyword"] = string.IsNullOrWhiteSpace(ProductCodeKeyword) ? null : ProductCodeKeyword,
            ["productNameKeyword"] = string.IsNullOrWhiteSpace(ProductNameKeyword) ? null : ProductNameKeyword
        };
    }
    
    /// <summary>
    /// 轉換為舊版 ProductBarcodePrintCriteria（相容現有報表服務）
    /// </summary>
    public ProductBarcodePrintCriteria ToLegacyCriteria()
    {
        return new ProductBarcodePrintCriteria
        {
            ProductIds = ProductIds,
            CategoryIds = CategoryIds,
            OnlyWithBarcode = OnlyWithBarcode,
            BarcodeSize = BarcodeSize,
            BarcodesPerPage = BarcodesPerPage,
            ShowProductName = ShowProductName,
            ShowProductCode = ShowProductCode,
            PrintQuantities = PrintQuantities
        };
    }
    
    /// <summary>
    /// 取得篩選條件摘要
    /// </summary>
    public string GetSummary()
    {
        var summary = new List<string>();

        if (ProductIds.Any())
        {
            summary.Add($"選擇 {ProductIds.Count} 個商品");
        }
        else if (CategoryIds.Any())
        {
            summary.Add($"篩選 {CategoryIds.Count} 個分類");
        }
        else
        {
            summary.Add("全部商品");
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
