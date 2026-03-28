using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using ClosedXML.Excel;
using ERPCore2.Data;
using ERPCore2.Services;
using Microsoft.JSInterop;

namespace ERPCore2.Components.Shared.Page;

/// <summary>
/// Index 頁面匯出功能（partial class）
/// 透過反射掃描 TEntity 所有資料庫欄位，匯出完整的 Entity 資料。
/// 雙行標題：Row 1 = PropertyName（供匯入對應），Row 2 = 本地化顯示名稱（人工閱讀）
/// </summary>
public partial class GenericIndexPageComponent<TEntity, TService>
    where TEntity : BaseEntity
    where TService : IGenericManagementService<TEntity>
{
    /// <summary>匯出 Excel 最大筆數限制，避免記憶體問題</summary>
    private const int ExportMaxRows = 10000;

    /// <summary>快取 TEntity 的可匯出屬性清單（反射結果，整個元件生命週期只算一次）</summary>
    private static List<PropertyInfo>? _exportPropertiesCache;

    // 匯出進度條狀態
    private bool _showExportProgress = false;
    private int _exportProgressValue = 0;
    private string _exportProgressMessage = "";
    private string _exportProgressColorClass = "bg-success";

    private async Task SetExportProgress(int value, string message)
    {
        _exportProgressValue = value;
        _exportProgressMessage = message;
        StateHasChanged();
        await Task.Delay(10);
    }

    /// <summary>
    /// 內建 Excel 匯出處理：取得資料 → 產生 Excel → 瀏覽器下載。
    /// 當外部未綁定 OnExportExcelClick 時，由元件自行處理匯出。
    /// </summary>
    private async Task ExecuteBuiltInExportExcelAsync()
    {
        try
        {
            _showExportProgress = true;
            _exportProgressColorClass = "bg-success";
            await SetExportProgress(10, "正在準備資料...");

            // 1. 取得 Entity 所有可匯出的屬性
            var exportProperties = GetExportableProperties();
            if (exportProperties.Count == 0)
            {
                await NotificationService.ShowWarningAsync("沒有可匯出的欄位");
                return;
            }

            await SetExportProgress(25, "正在讀取資料...");

            // 2. 取得匯出資料（依目前篩選條件，匯出全部）
            var exportData = await GetExportDataAsync();
            if (exportData.Count == 0)
            {
                await NotificationService.ShowWarningAsync("目前沒有資料可匯出");
                return;
            }

            await SetExportProgress(60, "正在產生 Excel...");

            // 3. 產生 Excel
            var fileBytes = GenerateExcelBytes(exportProperties, exportData);

            await SetExportProgress(85, "正在下載...");

            // 4. 透過 JS Interop 觸發瀏覽器下載
            var fileName = $"{PageTitle}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            var base64 = Convert.ToBase64String(fileBytes);
            var success = await JSRuntime.InvokeAsync<bool>("downloadFileFromBase64", fileName, base64,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");

            await SetExportProgress(100, success ? "下載完成！" : "下載失敗");
            await Task.Delay(500);

            if (success)
                await NotificationService.ShowSuccessAsync($"已匯出 {exportData.Count} 筆資料");
            else
                await NotificationService.ShowErrorAsync("下載 Excel 檔案失敗");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"匯出 Excel 失敗: {ex.Message}");
            await NotificationService.ShowErrorAsync($"匯出失敗：{ex.Message}");
        }
        finally
        {
            _showExportProgress = false;
            StateHasChanged();
        }
    }

    /// <summary>
    /// 透過反射取得 TEntity 所有可匯出的屬性（排除導覽屬性和集合）。
    /// 結果會快取，避免重複反射。
    /// </summary>
    private static List<PropertyInfo> GetExportableProperties()
    {
        if (_exportPropertiesCache != null)
            return _exportPropertiesCache;

        _exportPropertiesCache = typeof(TEntity)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(IsExportableProperty)
            .ToList();

        return _exportPropertiesCache;
    }

    /// <summary>
    /// 判斷屬性是否為可匯出的資料庫欄位（純量型別、Enum、Nullable 包裝的純量）。
    /// 排除：導覽屬性（其他 Entity）、集合（ICollection）、無 getter 的屬性。
    /// </summary>
    private static bool IsExportableProperty(PropertyInfo prop)
    {
        if (!prop.CanRead) return false;

        var type = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

        // 允許：基本型別、string、decimal、DateTime 系列、Guid、Enum
        if (type.IsPrimitive) return true;
        if (type == typeof(string)) return true;
        if (type == typeof(decimal)) return true;
        if (type == typeof(DateTime)) return true;
        if (type == typeof(DateOnly)) return true;
        if (type == typeof(TimeOnly)) return true;
        if (type == typeof(DateTimeOffset)) return true;
        if (type == typeof(Guid)) return true;
        if (type.IsEnum) return true;

        // 排除：集合、其他複雜型別
        return false;
    }

    /// <summary>
    /// 取得屬性的本地化顯示名稱。
    /// 優先順序：L["Field.{PropertyName}"] → [Display(Name)] 屬性 → PropertyName
    /// </summary>
    private string GetLocalizedPropertyName(PropertyInfo prop)
    {
        // 1. 嘗試 IStringLocalizer（支援多語言）
        var locKey = $"Field.{prop.Name}";
        var localized = L[locKey];
        if (!localized.ResourceNotFound)
            return localized.Value;

        // 2. 嘗試 [Display(Name)] 屬性（通常是繁體中文）
        var displayAttr = prop.GetCustomAttribute<DisplayAttribute>();
        if (displayAttr != null && !string.IsNullOrEmpty(displayAttr.Name))
            return displayAttr.Name;

        // 3. 最終 fallback：屬性名稱本身
        return prop.Name;
    }

    /// <summary>
    /// 取得匯出資料：伺服器模式重新查詢全部，客戶端模式用已篩選的資料
    /// </summary>
    private async Task<List<TEntity>> GetExportDataAsync()
    {
        if (ServerDataLoader != null)
        {
            // 伺服器模式：用目前篩選條件重新查詢，一次取回全部（上限 ExportMaxRows）
            var result = await ServerDataLoader(searchModel, 1, ExportMaxRows);
            return result.Items;
        }
        else
        {
            // 客戶端模式：直接用已篩選的資料（全量）
            return filteredItems;
        }
    }

    /// <summary>
    /// 使用 ClosedXML 產生 Excel 檔案的 byte[]。
    /// 雙行標題設計：
    ///   Row 1 = PropertyName（資料庫欄位名稱，供匯入系統對應用）
    ///   Row 2 = 本地化顯示名稱（使用者當前語言，方便人工閱讀）
    ///   Row 3+ = 資料列
    /// </summary>
    private byte[] GenerateExcelBytes(List<PropertyInfo> properties, List<TEntity> data)
    {
        using var workbook = new XLWorkbook();
        var sheetName = SanitizeSheetName(PageTitle);
        var worksheet = workbook.Worksheets.Add(sheetName);

        // Row 1：PropertyName（資料庫欄位名稱）— 供匯入程式讀取
        for (int col = 0; col < properties.Count; col++)
        {
            var cell = worksheet.Cell(1, col + 1);
            cell.Value = properties[col].Name;
            cell.Style.Font.Bold = true;
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Fill.BackgroundColor = XLColor.DarkBlue;
            cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        }

        // Row 2：本地化顯示名稱 — 方便人工閱讀
        for (int col = 0; col < properties.Count; col++)
        {
            var cell = worksheet.Cell(2, col + 1);
            cell.Value = GetLocalizedPropertyName(properties[col]);
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.LightGray;
            cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        }

        // Row 3+：資料列
        for (int row = 0; row < data.Count; row++)
        {
            var item = data[row];
            for (int col = 0; col < properties.Count; col++)
            {
                var cell = worksheet.Cell(row + 3, col + 1);
                var prop = properties[col];
                var value = prop.GetValue(item);

                SetCellValueByType(cell, value, prop.PropertyType);
                cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            }
        }

        // 自動調整欄寬
        worksheet.Columns().AdjustToContents(minWidth: 8, maxWidth: 50);

        // 凍結前兩行標題列，方便滾動瀏覽
        worksheet.SheetView.FreezeRows(2);

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    /// <summary>清理 Excel 工作表名稱（移除不允許字元，限制 31 字元）</summary>
    private static string SanitizeSheetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "Export";
        foreach (var c in new[] { ':', '\\', '/', '?', '*', '[', ']' })
            name = name.Replace(c, '_');
        return name.Length > 31 ? name[..31] : name;
    }

    /// <summary>
    /// 依據屬性的 CLR 型別設定 Excel 儲存格的值與格式。
    /// null 值輸出空白儲存格，便於匯入時判斷。
    /// </summary>
    private static void SetCellValueByType(IXLCell cell, object? value, Type propertyType)
    {
        if (value == null)
        {
            cell.Value = Blank.Value;
            return;
        }

        var type = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

        // 數值型別
        if (type == typeof(int) || type == typeof(long) || type == typeof(short) || type == typeof(byte))
        {
            cell.Value = Convert.ToInt64(value);
            return;
        }
        if (type == typeof(decimal))
        {
            cell.Value = (decimal)value;
            cell.Style.NumberFormat.Format = "#,##0.##";
            return;
        }
        if (type == typeof(double))
        {
            cell.Value = (double)value;
            cell.Style.NumberFormat.Format = "#,##0.##";
            return;
        }
        if (type == typeof(float))
        {
            cell.Value = (float)value;
            cell.Style.NumberFormat.Format = "#,##0.##";
            return;
        }

        // 日期時間型別
        if (type == typeof(DateTime))
        {
            cell.Value = (DateTime)value;
            cell.Style.NumberFormat.Format = "yyyy/MM/dd HH:mm:ss";
            return;
        }
        if (type == typeof(DateOnly))
        {
            cell.Value = ((DateOnly)value).ToDateTime(TimeOnly.MinValue);
            cell.Style.NumberFormat.Format = "yyyy/MM/dd";
            return;
        }
        if (type == typeof(DateTimeOffset))
        {
            cell.Value = ((DateTimeOffset)value).LocalDateTime;
            cell.Style.NumberFormat.Format = "yyyy/MM/dd HH:mm:ss";
            return;
        }
        if (type == typeof(TimeOnly))
        {
            cell.Value = ((TimeOnly)value).ToString("HH:mm:ss");
            return;
        }

        // 布林值
        if (type == typeof(bool))
        {
            cell.Value = (bool)value;
            return;
        }

        // Enum → 輸出數值（便於匯入對應）
        if (type.IsEnum)
        {
            cell.Value = Convert.ToInt32(value);
            return;
        }

        // 其他（string、Guid 等）→ 文字
        cell.Value = value.ToString();
    }
}
