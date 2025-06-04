# Tables 資料夾說明

## 1. 主要存放的組件類型
- **表格組件** - 提供統一的資料表格顯示和互動功能

## 2. 擁有的組件功能、適用場景

### TableComponent.razor
- **功能描述**: 泛型資料表格組件，支援任意資料類型的表格顯示和操作
- **適用場景**: 
  - 資料列表頁面的主要顯示組件
  - 管理介面的資料瀏覽
  - 搜尋結果的表格展示
  - 關聯資料的列表顯示
  - 需要操作按鈕的資料表格

## 3. 功能說明

### TableComponent 組件特性
- **泛型支援**: 支援任意資料類型 `TItem` 的表格顯示
- **響應式設計**: 使用 Bootstrap 的 `table-responsive` 確保在各種螢幕尺寸下的適當顯示
- **彈性標題**: 支援動態標題配置和自定義標題內容
- **行樣式**: 支援基於資料狀態的行樣式自定義
- **操作列**: 可選擇性顯示操作按鈕列
- **空狀態處理**: 優雅的無資料狀態顯示
- **自定義模板**: 完全可自定義的行內容和空狀態模板

### 組件參數
- **Items**: 資料項目集合 (`IEnumerable<TItem>`)
- **Headers**: 表格標題字串陣列
- **RowTemplate**: 資料行內容的自定義模板
- **ActionsTemplate**: 操作按鈕的自定義模板
- **EmptyTemplate**: 空狀態的自定義模板
- **ShowHeader**: 是否顯示表格標題
- **ShowActions**: 是否顯示操作列
- **EmptyMessage**: 無資料時的預設訊息
- **ActionsHeader**: 操作列的標題文字
- **TableCssClass**: 表格的額外 CSS 樣式
- **GetRowCssClass**: 取得行樣式的函數

### 使用方式
```razor
<TableComponent TItem="Customer" 
              Items="@customers" 
              Headers="@(new[] { "客戶名稱", "客戶類型", "狀態", "建立日期" })"
              ShowActions="true"
              ActionsHeader="操作">
    
    <RowTemplate Context="customer">
        <td>@customer.Name</td>
        <td>@customer.CustomerType?.Name</td>
        <td>
            <StatusBadgeComponent Status="@customer.Status" />
        </td>
        <td>@customer.CreatedAt.ToString("yyyy-MM-dd")</td>
    </RowTemplate>
    
    <ActionsTemplate Context="customer">
        <ButtonComponent Text="編輯" 
                       Variant="ButtonVariant.OutlinePrimary" 
                       Size="ButtonSize.Small"
                       IconClass="fas fa-edit" />
        <ButtonComponent Text="刪除" 
                       Variant="ButtonVariant.OutlineDanger" 
                       Size="ButtonSize.Small"
                       IconClass="fas fa-trash" />
    </ActionsTemplate>
    
    <EmptyTemplate>
        <div class="text-center py-4">
            <i class="fas fa-users fa-3x text-muted mb-3"></i>
            <h5 class="text-muted">尚未建立任何客戶</h5>
            <p class="text-muted">點擊「新增客戶」按鈕來建立第一個客戶</p>
        </div>
    </EmptyTemplate>
</TableComponent>
```

### 表格樣式系統
- **預設樣式**: 使用 Bootstrap `table` 和 `table-hover` 類別
- **標題樣式**: 淺色背景 `table-light` 標題列
- **行狀態樣式**: 支援基於資料狀態的行樣式變更
- **響應式包裝**: 自動處理水平滾動

### 空狀態處理
- **預設空狀態**: 提供標準的無資料圖示和訊息
- **自定義空狀態**: 支援完全自定義的空狀態模板
- **語意化提示**: 使用圖示和文字提供清晰的使用者指引

### 設計原則
- 提供一致的表格資料展示體驗
- 支援高度的客製化和彈性配置
- 遵循 ERP 系統的視覺設計規範
- 確保響應式和無障礙設計標準
