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
- **行點擊功能**: 支援表格行點擊事件，可用於資料檢視或導航
- **操作列**: 可選擇性顯示操作按鈕列
- **空狀態處理**: 優雅的無資料狀態顯示
- **自定義模板**: 完全可自定義的行內容和空狀態模板
- **互動體驗**: 支援滑鼠懸停效果和點擊回饋

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
- **OnRowClick**: 行點擊事件回調函數 (`EventCallback<TItem>`)
- **EnableRowClick**: 是否啟用行點擊功能 (`bool`)
- **RowClickCursor**: 行點擊時的滑鼠游標樣式 (`string`, 預設: "pointer")

### 使用方式
```razor
<TableComponent TItem="Customer" 
              Items="@customers" 
              Headers="@(new[] { "客戶名稱", "客戶類型", "狀態", "建立日期" })"
              ShowActions="true"
              ActionsHeader="操作"
              EnableRowClick="true"
              OnRowClick="@ShowCustomerDetail">
    
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

<!-- 範例：僅使用行點擊功能，隱藏操作列 -->
<TableComponent TItem="Customer" 
              Items="@customers" 
              Headers="@(new[] { "客戶代碼", "公司名稱", "聯絡人", "狀態" })"
              ShowActions="false"
              EnableRowClick="true"
              OnRowClick="@ShowCustomerDetail">
    
    <RowTemplate Context="customer">
        <td>@customer.CustomerCode</td>
        <td>@customer.CompanyName</td>
        <td>@customer.ContactPerson</td>
        <td>
            <StatusBadgeComponent Status="@customer.Status" />
        </td>
    </RowTemplate>
    
    <EmptyTemplate>
        <div class="text-center py-4">
            <i class="fas fa-users fa-3x text-muted mb-3"></i>
            <h5 class="text-muted">尚未建立任何客戶</h5>
            <p class="text-muted">點擊表格行查看客戶詳細資料</p>
        </div>
    </EmptyTemplate>
</TableComponent>
```

### 表格樣式系統
- **預設樣式**: 使用 Bootstrap `table` 和 `table-hover` 類別
- **標題樣式**: 淺色背景 `table-light` 標題列
- **行狀態樣式**: 支援基於資料狀態的行樣式變更
- **響應式包裝**: 自動處理水平滾動
- **互動樣式**: 行點擊功能啟用時自動顯示 pointer 游標

### 行點擊功能
- **條件啟用**: 透過 `EnableRowClick` 參數控制功能開關
- **事件回調**: 使用 `OnRowClick` 參數設定點擊處理函數
- **視覺回饋**: 支援自定義游標樣式，預設為 "pointer"
- **非同步支援**: 點擊處理函數支援非同步操作
- **靈活應用**: 可用於資料檢視、編輯導航、模態視窗顯示等場景

### 空狀態處理
- **預設空狀態**: 提供標準的無資料圖示和訊息
- **自定義空狀態**: 支援完全自定義的空狀態模板
- **語意化提示**: 使用圖示和文字提供清晰的使用者指引

### 設計原則
- 提供一致的表格資料展示體驗
- 支援高度的客製化和彈性配置
- 遵循 ERP 系統的視覺設計規範
- 確保響應式和無障礙設計標準
- 支援多種資料互動模式（檢視、編輯、操作）
- 提供直觀的使用者體驗和視覺回饋

### 最佳實踐
- **行點擊與操作按鈕**: 當啟用行點擊功能時，考慮隱藏操作列以避免使用者困惑
- **視覺一致性**: 使用一致的游標樣式和視覺回饋提升使用者體驗
- **功能明確性**: 在空狀態模板中明確指示使用者可以點擊行進行操作
- **效能考量**: 對於大量資料的表格，確保行點擊處理函數的效能表現
