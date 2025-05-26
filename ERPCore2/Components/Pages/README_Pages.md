# ERPCore2 Pages 設計風格指南

## 頁面開發流程

### 開發步驟
1. **HTML 原型階段**
   - 編輯/新建頁面時，先使用 HTML 標籤撰寫頁面結構
   - 使用基本的 HTML 元素建立頁面佈局和功能
   - 進行 build 測試，確保頁面功能正常運作

2. **組件化階段**
   - 當 HTML 版本確認無誤後，開始將可重複使用的部分改為共享組件
   - **優先使用現有組件**：先在現有的共享組件中搜尋是否有適合的組件
   - **創建新組件**：如果沒有適合的現有組件，則創建新的共享組件
   - **組件分類存放**：根據組件的種類和功能性質放到適合的資料夾中
   - 逐步替換 HTML 標籤為自訂組件
   - 提高程式碼重用性和維護性

### 檔案組織原則
- **依功能性質分類**：所有頁面和組件必須依照其業務性質分在不同的資料夾中
- **模組化結構**：相關的頁面、組件和服務放在同一模組資料夾內
- **清晰的命名規則**：檔案和資料夾名稱應該清楚表達其功能
- **共享組件統一管理**：共享組件依據類型分類存放，確保重用性和維護性

## 共享組件開發規範

### 組件搜尋與選用原則
1. **現有組件優先**
   - 在開始組件化之前，必須先搜尋現有的共享組件庫
   - 檢查是否有符合需求的現有組件可以直接使用
   - 評估現有組件是否可透過參數設定滿足需求

2. **組件評估標準**
   - **功能相似性**：核心功能是否一致
   - **樣式相容性**：是否符合設計風格指南
   - **參數擴展性**：是否可透過參數客製化
   - **維護成本**：重用 vs 新建的成本評估

3. **新組件創建時機**
   - 現有組件無法滿足功能需求
   - 現有組件修改成本過高
   - 新功能具有高重用潛力

### 共享組件目錄結構
```
Components/
├── Shared/                    # 共享組件根目錄
│   ├── UI/                   # 基礎 UI 組件
│   │   ├── Buttons/          # 按鈕組件
│   │   ├── Forms/            # 表單組件
│   │   ├── Tables/           # 表格組件
│   │   ├── Modals/           # 對話框組件
│   │   ├── Navigation/       # 導航組件
│   │   └── Cards/            # 卡片組件
│   ├── Business/             # 業務邏輯組件
│   │   ├── Customer/         # 客戶相關組件
│   │   ├── Order/            # 訂單相關組件
│   │   ├── Product/          # 產品相關組件
│   │   └── Report/           # 報表相關組件
│   └── Layout/               # 佈局組件
│       ├── Headers/          # 標題組件
│       ├── Footers/          # 頁腳組件
│       └── Sidebars/         # 側邊欄組件
├── Pages/                    # 頁面組件
└── Layout/                   # 主要佈局
```

### 組件開發流程

#### 第一步：組件搜尋
```powershell
# 搜尋現有組件的方法
# 1. 檔案名稱搜尋
Get-ChildItem -Path "Components\Shared" -Recurse -Name "*.razor" | Where-Object { $_ -like "*Button*" }

# 2. 內容搜尋
Select-String -Path "Components\Shared\**\*.razor" -Pattern "button|btn" -CaseSensitive:$false
```

#### 第二步：組件評估
- 檢查組件參數和屬性
- 測試組件在目標場景的適用性
- 評估樣式和行為是否符合需求

#### 第三步：組件選用或創建
- **直接使用**：現有組件完全符合需求
- **參數擴展**：透過新增參數支援新需求
- **創建新組件**：無適合的現有組件

### 組件命名規範
- **基礎 UI 組件**：`{功能}Component.razor` (如：`ButtonComponent.razor`)
- **業務組件**：`{業務領域}{功能}Component.razor` (如：`CustomerSearchComponent.razor`)
- **佈局組件**：`{位置}Layout.razor` (如：`HeaderLayout.razor`)

### 組件參數設計原則
```csharp
// 良好的組件參數設計範例
[Parameter] public string Text { get; set; } = string.Empty;
[Parameter] public string CssClass { get; set; } = string.Empty;
[Parameter] public ButtonType Type { get; set; } = ButtonType.Primary;
[Parameter] public EventCallback OnClick { get; set; }
[Parameter] public bool IsDisabled { get; set; } = false;
[Parameter] public RenderFragment? ChildContent { get; set; }
```

## 主要色彩組合

### 核心色彩系統
- **主色：深藍色**
  - `#1E3A8A` - 主要深藍
  - `#1F2937` - 替代深藍
- **背景色：白色/淺灰**
  - `#FFFFFF` - 純白背景
  - `#F9FAFB` - 淺灰背景
- **輔助色：中性灰**
  - `#6B7280` - 中等灰色（次要文字）
  - `#D1D5DB` - 淺灰色（邊框）
  - `#374151` - 深灰色（主要文字）

### 色彩使用原則

#### 主色：深藍色
- **用途**：導航列、標題、主要按鈕、連結
- **心理效果**：傳達信任、穩定、專業形象
- **適用場景**：
  - 系統標題和 Logo
  - 主要操作按鈕
  - 選中狀態
  - 重要資訊標示

#### 背景：白色/淺灰
- **用途**：主要內容區域、卡片背景
- **心理效果**：保持清潔、易讀，減少視覺疲勞
- **適用場景**：
  - 頁面主要背景
  - 表格和表單背景
  - 對話框背景

#### 輔助色：中性灰
- **用途**：次要元素、邊框、分隔線
- **心理效果**：中性且不干擾，適合資訊密集的介面
- **適用場景**：
  - 次要文字和說明
  - 表格邊框
  - 輸入框邊框

## 輔助功能色彩

### 狀態指示色
```css
/* 成功狀態 */
.success-color {
  background-color: #059669; /* 綠色 */
  color: #FFFFFF;
}

/* 警告狀態 */
.warning-color {
  background-color: #EA580C; /* 橙色 */
  color: #FFFFFF;
}

/* 錯誤/危險狀態 */
.danger-color {
  background-color: #DC2626; /* 紅色 */
  color: #FFFFFF;
}

/* 資訊狀態 */
.info-color {
  background-color: #1E3A8A; /* 主藍色 */
  color: #FFFFFF;
}
```

### 功能按鈕色彩指南
- **確認/提交**：綠色 (`#059669`)
- **警告/注意**：橙色 (`#EA580C`)
- **刪除/取消**：紅色 (`#DC2626`)
- **一般操作**：主藍色 (`#1E3A8A`)
- **次要操作**：中性灰 (`#6B7280`)

## 設計理念

### 色彩選擇原則
- **藍色**：商業環境中的可靠性、專業性和信任感
- **白色**：簡潔清晰，長時間使用不疲勞
- **灰色**：中性不干擾，適合資訊密集介面

### 實用性考量
- **高可讀性**：深色文字配白底，符合可訪問性標準
- **視覺舒適**：避免鮮豔色彩造成視覺疲勞
- **列印友善**：黑白列印時保持良好對比
- **響應式設計**：不同設備上保持一致性

## CSS 變數定義 可參考此文件 wwwroot/css/variables.css

```css
:root {
  /* 主要色彩 */
  --primary-blue: #1E3A8A;
  --primary-blue-alt: #1F2937;
  --primary-white: #FFFFFF;
  --primary-light-gray: #F9FAFB;
  
  /* 文字色彩 */
  --text-primary: #374151;
  --text-secondary: #6B7280;
  --text-light: #9CA3AF;
  
  /* 邊框和分隔線 */
  --border-color: #D1D5DB;
  --border-light: #E5E7EB;
  
  /* 狀態色彩 */
  --success-color: #059669;
  --warning-color: #EA580C;
  --danger-color: #DC2626;
  --info-color: #1E3A8A;
  
  /* 背景色彩 */
  --bg-primary: #FFFFFF;
  --bg-secondary: #F9FAFB;
  --bg-tertiary: #F3F4F6;
}
```

## 頁面組件設計原則

### HTML 開發階段
- 使用語義化 HTML 標籤建立頁面結構
- 確保無障礙設計 (ARIA 標籤、鍵盤導航)
- 先完成功能性，再考慮美化

### 組件化階段
- 識別可重複使用的 UI 元素
- 建立共享組件庫
- 保持組件的單一職責原則

### UI 組件類型

#### 導航組件
- 主藍色背景，白色文字
- 懸停效果使用較淺藍色

#### 表格組件
- 白色背景，淺灰邊框
- 斑馬條紋使用極淺灰色

#### 表單組件
- 白色背景，清晰標籤
- 狀態顏色用於驗證回饋

#### 按鈕組件
- 主要按鈕：主藍色
- 次要按鈕：灰色邊框
- 危險操作：紅色

## 開發規範

### 檔案結構規範
```
Pages/
├── Customers/          # 客戶相關頁面
├── Orders/            # 訂單相關頁面
├── Products/          # 產品相關頁面
├── Reports/           # 報表相關頁面
└── System/            # 系統管理頁面
```

### 命名規則
- 頁面檔案：`{功能名稱}.razor`
- 組件檔案：`{組件名稱}Component.razor`
- 樣式檔案：`{檔案名稱}.razor.css`

### 開發檢查清單
- [ ] HTML 結構語義化完整
- [ ] **搜尋現有共享組件** - 確認是否有可重用的組件
- [ ] **評估組件適用性** - 檢查功能、樣式、參數相容性
- [ ] **選用或創建組件** - 優先使用現有組件，必要時創建新組件
- [ ] **組件分類存放** - 新組件按類型放入適當的共享組件資料夾
- [ ] 色彩符合設計規範
- [ ] 響應式設計測試通過
- [ ] 無障礙設計檢查完成
- [ ] Build 測試無錯誤
- [ ] 組件化改造評估

## 共享組件使用範例

### 使用現有組件
```html
<!-- 替換前：HTML 標籤 -->
<button class="btn btn-primary" onclick="submitForm()">提交</button>

<!-- 替換後：使用現有共享組件 -->
<ButtonComponent Text="提交" Type="ButtonType.Primary" OnClick="submitForm" />
```

### 創建新共享組件
如果現有組件不符合需求，創建新組件的步驟：

1. **確定組件類型和存放位置**
```
Components/Shared/UI/Forms/DatePickerComponent.razor  # 基礎 UI 組件
Components/Shared/Business/Customer/CustomerCardComponent.razor  # 業務組件
```

2. **設計組件參數**
```csharp
@* DatePickerComponent.razor *@
[Parameter] public DateTime? Value { get; set; }
[Parameter] public EventCallback<DateTime?> ValueChanged { get; set; }
[Parameter] public string Label { get; set; } = string.Empty;
[Parameter] public bool IsRequired { get; set; } = false;
[Parameter] public string CssClass { get; set; } = string.Empty;
```

3. **實作組件邏輯**
```html
<div class="form-group @CssClass">
    <label class="form-label">@Label @if(IsRequired){<span class="text-danger">*</span>}</label>
    <input type="date" class="form-control" 
           value="@(Value?.ToString("yyyy-MM-dd"))" 
           @onchange="OnDateChanged" />
</div>
```

### 組件重構建議
- **小步重構**：一次替換一個 HTML 區塊
- **測試驅動**：每次替換後進行功能測試
- **漸進式改進**：先求功能正確，再優化參數設計

---

*此設計風格指南適用於 ERPCore2 系統所有頁面開發，確保程式碼品質和使用者體驗的一致性。*
