# Pages 資料夾說明文檔

## 概述
Pages 資料夾包含了 ERPCore2 系統的所有頁面組件，負責使用者介面展示和互動。採用先 HTML 原型後組件化的開發方式，最大化重用共享組件。

## 頁面開發流程

### 兩階段開發模式

#### 第一階段：HTML 原型開發
1. **使用純 HTML 標籤**建立頁面結構和功能
2. **完成基本業務邏輯**實作，確保功能正常運作
3. **進行 Build 測試**，驗證頁面功能無誤
4. **確認需求完整性**，避免後續大幅修改

#### 第二階段：組件化重構
1. **搜尋現有共享組件** - 查閱 `README_Shared.md` 找尋適合的組件
2. **評估組件適用性** - 檢查功能、樣式、參數是否符合需求
3. **選用或創建組件** - 優先使用現有組件，必要時創建新組件
4. **逐步替換 HTML** - 將 HTML 標籤替換為共享組件
5. **測試功能完整性** - 確保組件化後功能無損失

---

## 共享組件使用原則

### 🔍 組件搜尋步驟

#### 1. 查閱 README_Shared.md
- 瀏覽共享組件清單和說明
- 查看組件功能描述和使用範例
- 確認組件所在位置和命名空間

#### 2. 實際檔案搜尋
```powershell
# 按功能關鍵字搜尋組件
Get-ChildItem -Path "Components\Shared" -Recurse -Name "*.razor" | Where-Object { $_ -like "*Button*" }

# 按內容搜尋相關組件
Select-String -Path "Components\Shared\**\*.razor" -Pattern "form|input" -CaseSensitive:$false
```

#### 3. 組件適用性評估
- **功能相似性**：核心功能是否符合需求
- **參數相容性**：現有參數是否支援目標場景
- **樣式一致性**：視覺風格是否符合設計規範
- **擴展可能性**：是否可透過參數調整滿足需求

### 🔧 組件選用決策

#### 直接使用現有組件
```html
<!-- 替換前：HTML 標籤 -->
<button class="btn btn-primary" @onclick="SaveData">儲存</button>

<!-- 替換後：使用共享組件 -->
<ButtonComponent Text="儲存" Type="ButtonType.Primary" OnClick="SaveData" />
```

#### 創建新共享組件
**創建時機**：
- 現有組件無法滿足功能需求
- 新功能具有高重用潛力
- 現有組件修改成本過高

**創建流程**：
1. **確定組件分類** - 根據功能性質選擇存放資料夾
2. **設計組件參數** - 考慮重用性和擴展性
3. **實作組件邏輯** - 遵循共享組件開發規範
4. **更新文檔** - 在 README_Shared.md 中記錄新組件

## 設計規範

### 視覺風格
- **色彩配置**：參考 `wwwroot/css/variables.css` 中定義的 CSS 變數
- **一致性原則**：所有頁面遵循相同的視覺風格和互動模式
- **響應式設計**：確保在不同螢幕尺寸下的適配性

### 命名規範
- **頁面檔案**：`{功能名稱}.razor`（如：`CustomerList.razor`）
- **組件檔案**：`{組件名稱}Component.razor`（如：`CustomerCardComponent.razor`）
- **樣式檔案**：`{檔案名稱}.razor.css`

### 程式碼組織
```csharp
@page "/customers"
@inject ICustomerService CustomerService

<PageTitle>客戶管理</PageTitle>

@* 頁面標題區 *@
<HeaderComponent Title="客戶管理" />

@* 功能按鈕區 *@
<ActionBarComponent>
    <ButtonComponent Text="新增客戶" Type="ButtonType.Primary" OnClick="CreateCustomer" />
</ActionBarComponent>

@* 主要內容區 *@
<MainContentComponent>
    @if (customers != null)
    {
        <DataTableComponent Data="customers" />
    }
</MainContentComponent>

@code {
    private List<Customer>? customers;
    
    protected override async Task OnInitializedAsync()
    {
        customers = await CustomerService.GetAllAsync();
    }
    
    private void CreateCustomer()
    {
        // 業務邏輯
    }
}
```

---

## 開發檢查清單

### HTML 原型階段
- [ ] 使用語義化 HTML 標籤建立頁面結構
- [ ] 實作完整的業務邏輯功能
- [ ] 確保無障礙設計（ARIA 標籤、鍵盤導航）
- [ ] 進行 Build 測試，確認功能正常
- [ ] 驗證需求完整性

### 組件化階段
- [ ] **查閱 README_Shared.md** - 搜尋可用的共享組件
- [ ] **評估現有組件** - 檢查功能、參數、樣式相容性
- [ ] **選用適合組件** - 優先使用現有組件
- [ ] **創建新組件（必要時）** - 根據功能分類存放
- [ ] **逐步替換 HTML** - 保持功能完整性
- [ ] **測試組件化結果** - 確認功能無損失
- [ ] **更新組件文檔** - 新組件需更新 README_Shared.md

### 品質檢查
- [ ] 色彩使用符合 variables.css 定義
- [ ] 命名規則符合專案規範
- [ ] 響應式設計測試通過
- [ ] 程式碼結構清晰易維護
- [ ] 組件重用率最大化

---

## 組件化範例

### 表單頁面組件化
```html
<!-- 組件化前：純 HTML -->
<form>
    <div class="form-group">
        <label>客戶名稱</label>
        <input type="text" class="form-control" @bind="customer.Name" />
    </div>
    <div class="form-group">
        <label>聯絡電話</label>
        <input type="tel" class="form-control" @bind="customer.Phone" />
    </div>
    <button type="submit" class="btn btn-primary">儲存</button>
</form>

<!-- 組件化後：使用共享組件 -->
<FormComponent Model="customer" OnSubmit="SaveCustomer">
    <InputTextComponent Label="客戶名稱" @bind-Value="customer.Name" IsRequired="true" />
    <InputTelComponent Label="聯絡電話" @bind-Value="customer.Phone" />
    <ButtonComponent Text="儲存" Type="ButtonType.Primary" IsSubmit="true" />
</FormComponent>
```

### 列表頁面組件化
```html
<!-- 組件化前：純 HTML -->
<table class="table">
    <thead>
        <tr>
            <th>客戶名稱</th>
            <th>聯絡電話</th>
            <th>操作</th>
        </tr>
    </thead>
    <tbody>
        @foreach (var customer in customers)
        {
            <tr>
                <td>@customer.Name</td>
                <td>@customer.Phone</td>
                <td>
                    <button class="btn btn-sm btn-primary" @onclick="() => EditCustomer(customer.Id)">編輯</button>
                    <button class="btn btn-sm btn-danger" @onclick="() => DeleteCustomer(customer.Id)">刪除</button>
                </td>
            </tr>
        }
    </tbody>
</table>

<!-- 組件化後：使用共享組件 -->
<DataTableComponent Data="customers" TItem="Customer">
    <ColumnDefinitions>
        <TableColumnComponent Field="Name" Header="客戶名稱" />
        <TableColumnComponent Field="Phone" Header="聯絡電話" />
        <TableActionColumnComponent>
            <ButtonComponent Text="編輯" Type="ButtonType.Primary" Size="Small" OnClick="() => EditCustomer(context.Id)" />
            <ButtonComponent Text="刪除" Type="ButtonType.Danger" Size="Small" OnClick="() => DeleteCustomer(context.Id)" />
        </TableActionColumnComponent>
    </ColumnDefinitions>
</DataTableComponent>
```

---

## 最佳實踐

1. **先求功能完整，再求組件美化** - HTML 原型確保功能正確性
2. **最大化組件重用** - 優先使用現有共享組件
3. **漸進式組件化** - 小步重構，保持功能穩定
4. **文檔同步更新** - 新組件及時更新 README_Shared.md
5. **保持設計一致性** - 遵循 variables.css 定義的視覺規範

---

*本指南確保 Pages 開發的一致性和效率，透過兩階段開發流程和共享組件最大化，提升程式碼品質和維護性。*