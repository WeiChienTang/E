# SystemInfoDisplayComponent 使用說明

## 概述

`SystemInfoDisplayComponent` 是一個通用的系統資訊顯示組件，用於在所有 Detail 頁面中統一展示實體的系統資訊，包括建立時間、更新時間、建立者、更新者和備註等信息。

## 特色

- **統一的設計風格**：採用現代卡片式設計，與 `RolePermissionDisplayComponent` 風格一致
- **美觀的視覺效果**：使用 Bootstrap 樣式和 Font Awesome 圖標
- **靈活的配置**：可選擇顯示時間軸和備註
- **響應式布局**：自適應不同螢幕尺寸
- **完整的國際化**：支援中文顯示

## 組件參數

| 參數名稱 | 類型 | 必填 | 預設值 | 說明 |
|---------|------|------|--------|------|
| `CreatedAt` | `DateTime` | 是 | - | 建立時間 |
| `UpdatedAt` | `DateTime?` | 否 | `null` | 更新時間（可選） |
| `CreatedBy` | `string?` | 否 | `null` | 建立者 |
| `UpdatedBy` | `string?` | 否 | `null` | 更新者 |
| `Remarks` | `string?` | 否 | `null` | 備註 |
| `ShowTimeline` | `string` | 否 | `"false"` | 是否顯示時間軸 |
| `ShowRemarks` | `string` | 否 | `"false"` | 是否顯示備註欄位 |

## 使用方式

### 基本使用

```razor
<SystemInfoDisplayComponent 
    CreatedAt="@(entity?.CreatedAt ?? DateTime.MinValue)"
    UpdatedAt="@entity?.UpdatedAt"
    CreatedBy="@entity?.CreatedBy"
    UpdatedBy="@entity?.UpdatedBy" />
```

### 完整使用（包含備註和時間軸）

```razor
<SystemInfoDisplayComponent 
    CreatedAt="@(entity?.CreatedAt ?? DateTime.MinValue)"
    UpdatedAt="@entity?.UpdatedAt"
    CreatedBy="@entity?.CreatedBy"
    UpdatedBy="@entity?.UpdatedBy"
    Remarks="@entity?.Remarks"
    ShowTimeline="true"
    ShowRemarks="true" />
```

## 在 Detail 頁面中的應用

所有 Detail 頁面都已經更新為使用此組件，標準的系統資訊 Tab 結構如下：

```razor
new GenericDetailPageComponent<TEntity, TService>.TabSection
{
    Id = "system",
    Title = "系統資訊",
    Content = @<div>
        <SystemInfoDisplayComponent 
            CreatedAt="@(entity?.CreatedAt ?? DateTime.MinValue)"
            UpdatedAt="@entity?.UpdatedAt"
            CreatedBy="@entity?.CreatedBy"
            UpdatedBy="@entity?.UpdatedBy"
            Remarks="@entity?.Remarks"
            ShowTimeline="true"
            ShowRemarks="true" />
    </div>
}
```

## 已更新的頁面

以下 Detail 頁面已全部更新為使用新的 SystemInfoDisplayComponent：

1. **員工管理**
   - `EmployeeDetail.razor`
   - `RoleDetail.razor`
   - `PermissionDetail.razor`

2. **客戶管理**
   - `CustomerDetail.razor`

3. **廠商管理**
   - `SupplierDetail.razor`

4. **產品管理**
   - `ProductDetail.razor`

5. **基礎資料**
   - `CustomerTypeDetail.razor`
   - `SupplierTypeDetail.razor`
   - `IndustryTypeDetail.razor`

6. **BOM 基礎資料**
   - `ColorDetail.razor`
   - `MaterialDetail.razor`
   - `WeatherDetail.razor`

## 樣式特色

- **現代卡片設計**：使用 Bootstrap 卡片樣式
- **資訊分組**：基本資訊和時間軸分別顯示
- **圖標增強**：使用 Font Awesome 圖標提升視覺效果
- **狀態標示**：不同類型的資訊使用不同顏色標示
- **空值處理**：優雅處理空值和未設定的情況

## 維護說明

- 組件位置：`Components/Shared/Details/SystemInfoDisplayComponent.razor`
- 樣式文件：`Components/Shared/Details/SystemInfoDisplayComponent.razor.css`
- 當需要修改系統資訊顯示格式時，只需要修改此組件即可，所有 Detail 頁面會自動更新

## 優勢

1. **一致性**：所有頁面的系統資訊顯示完全一致
2. **可維護性**：只需要維護一個組件
3. **可擴展性**：容易添加新的系統資訊欄位
4. **可重用性**：任何需要顯示系統資訊的地方都可以使用
5. **美觀性**：統一的現代化設計風格
