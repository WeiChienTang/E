# Actions 資料夾說明

## 1. 主要存放的組件類型
- **頁面操作欄組件** - 提供頁面級別的主要操作按鈕容器

## 2. 擁有的組件功能、適用場景

### PageActionBar.razor
- **功能描述**: 頁面操作欄組件，提供統一的頁面操作按鈕排版
- **適用場景**: 
  - 頁面頂部操作按鈕區域
  - 需要區分主要操作和次要操作的場景
  - 列表頁面的批量操作區域
  - 詳情頁面的編輯、刪除等操作

## 🎨 視覺化設計說明

### 元件外觀結構
```
┌─────────────────────────────────────────────────────────────────────────────┐
│                     PageActionBar 容器 (右對齊布局)                          │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│           空白區域                    [主要操作] [次要操作] [次要操作]         │ ← 16px 底邊距
│                                        (藍色)    (灰框)    (紅框)             │   8px 按鈕間距
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 顏色配置 (基於 ERP 系統設計規範)
| 按鈕類型 | 背景色 | 邊框色 | 文字色 | 用途 |
|----------|--------|--------|--------|------|
| **Primary** | `#1E3A8A` (深藍) | `#1E3A8A` | `#FFFFFF` | 主要操作 |
| **OutlinePrimary** | 透明 | `#1E3A8A` | `#1E3A8A` | 檢視類操作 |
| **OutlineWarning** | 透明 | `#EA580C` | `#EA580C` | 編輯類操作 |
| **OutlineDanger** | 透明 | `#DC2626` | `#DC2626` | 刪除類操作 |
| **OutlineSecondary** | 透明 | `#6B7280` | `#6B7280` | 其他操作 |

## 3. 功能說明

### PageActionBar 組件特性
- **彈性佈局**: 使用 Flexbox 進行按鈕排列，右對齊顯示
- **分組管理**: 支援主要操作 (PrimaryActions) 和次要操作 (SecondaryActions) 分組
- **間距控制**: 自動處理按鈕間距，提供一致的視覺效果
- **響應式設計**: 適應不同螢幕尺寸的顯示需求

### 使用方式
```razor
<PageActionBar>
    <PrimaryActions>
        <!-- 主要操作按鈕 -->
    </PrimaryActions>
    <SecondaryActions>
        <!-- 次要操作按鈕 -->
    </SecondaryActions>
</PageActionBar>
```

## 📱 實際使用場景範例

### 場景 1: 列表頁面頁首操作
```razor
<PageActionBar>
    <PrimaryActions>
        <ButtonComponent Text="新增客戶" 
                       Variant="ButtonVariant.Primary" 
                       IconClass="fas fa-plus" />
    </PrimaryActions>
</PageActionBar>
```

**視覺呈現:**
```
┌─────────────────────────────────────────────────────────────┐
│                                          ➕ 新增客戶         │
│                                          [深藍色按鈕]       │
└─────────────────────────────────────────────────────────────┘
```

### 場景 2: 詳情頁面多重操作
```razor
<PageActionBar>
    <PrimaryActions>
        <ButtonComponent Text="編輯" 
                       Variant="ButtonVariant.Primary" 
                       IconClass="fas fa-edit" />
    </PrimaryActions>
    <SecondaryActions>
        <ButtonComponent Text="停用" 
                       Variant="ButtonVariant.OutlineSecondary" 
                       IconClass="fas fa-pause" />
        <ButtonComponent Text="刪除" 
                       Variant="ButtonVariant.OutlineDanger" 
                       IconClass="fas fa-trash" />
    </SecondaryActions>
</PageActionBar>
```

**視覺呈現:**
```
┌─────────────────────────────────────────────────────────────────────────────┐
│                                    ✏️ 編輯   ⏸️ 停用   🗑️ 刪除             │
│                                    [藍色]    [灰框]    [紅框]               │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 場景 3: 表格內行操作 (緊湊型)
```razor
<div class="btn-group" role="group">
    <ButtonComponent Text="" 
                   Variant="ButtonVariant.OutlinePrimary" 
                   Size="ButtonSize.Small"
                   IconClass="fas fa-eye" />
    <ButtonComponent Text="" 
                   Variant="ButtonVariant.OutlineWarning" 
                   Size="ButtonSize.Small"
                   IconClass="fas fa-edit" />
    <ButtonComponent Text="" 
                   Variant="ButtonVariant.OutlineDanger" 
                   Size="ButtonSize.Small"
                   IconClass="fas fa-trash" />
</div>
```

**視覺呈現:**
```
表格操作欄 (小尺寸)
┌───────────────────┐
│  👁️  ✏️  🗑️       │
│ [藍] [黃] [紅]       │ ← 32px 高度，緊密排列
└───────────────────┘
```

## 📐 尺寸與間距規範

### CSS 類別結構
- `.page-action-bar` - 主容器
- `.d-flex justify-content-end` - 右對齊 Flexbox 布局
- `.gap-2` - 按鈕間距 8px (Bootstrap spacing)
- `.mb-3` - 底部邊距 16px

### 尺寸規格
| 屬性 | 標準值 | 小尺寸 | 說明 |
|------|--------|--------|------|
| **按鈕高度** | 38px | 32px | Bootstrap btn / btn-sm |
| **按鈕間距** | 8px | 8px | gap-2 固定間距 |
| **底部邊距** | 16px | 16px | mb-3 固定邊距 |
| **圓角半徑** | 4px | 4px | --radius 變數 |
| **最小寬度** | 80px | 60px | 確保可點擊區域 |

### 響應式行為
- **桌面 (≥1024px)**: 標準尺寸，完整按鈕文字
- **平板 (768px-1023px)**: 標準尺寸，可能縮短文字
- **手機 (≤767px)**: 小尺寸按鈕，圖示優先顯示

## 🎯 設計原則與最佳實踐

### 操作層級設計
1. **主要操作 (Primary)**: 頁面的核心功能，如「新增」、「儲存」
2. **次要操作 (Secondary)**: 輔助功能，如「編輯」、「檢視」
3. **危險操作 (Danger)**: 不可逆操作，如「刪除」，使用紅色警示

### 圖示使用規範
| 操作類型 | 建議圖示 | Font Awesome 類別 |
|----------|----------|-------------------|
| **新增** | ➕ | `fas fa-plus` |
| **編輯** | ✏️ | `fas fa-edit` |
| **檢視** | 👁️ | `fas fa-eye` |
| **刪除** | 🗑️ | `fas fa-trash` |
| **儲存** | 💾 | `fas fa-save` |
| **取消** | ❌ | `fas fa-times` |
| **停用** | ⏸️ | `fas fa-pause` |
| **啟用** | ▶️ | `fas fa-play` |

### 設計原則
- 遵循 ERP 系統的一致性設計規範
- 提供清晰的操作層級區分
- 支援靈活的內容自定義
- **可訪問性**: 確保足夠的顏色對比度和可點擊區域
- **一致性**: 相同操作在不同頁面使用相同的視覺樣式
- **直觀性**: 圖示與操作意義明確對應
