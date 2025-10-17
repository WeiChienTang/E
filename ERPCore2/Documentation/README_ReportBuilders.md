# 報表 HTML 建構器使用指南

## 📋 概述

為了減少撰寫報表時的重複程式碼，我們建立了一套通用的 HTML 建構器（Builder Pattern），讓報表的生成更簡潔、更易維護。

## 🎯 核心優勢

- **減少 70% 的重複程式碼**：不再需要手寫重複的 HTML 標籤
- **型別安全**：使用泛型和 Lambda 表達式，編譯期檢查錯誤
- **易於維護**：樣式調整只需修改 Builder，所有報表自動套用
- **Fluent API**：鏈式呼叫，程式碼更簡潔易讀
- **高度可重用**：適用於所有報表（採購單、進貨單、退貨單、銷貨單等）

## 🛠️ 可用的建構器

### 1. ReportHeaderBuilder - 表頭建構器

**用途**：生成標準的三欄式表頭（左側公司資訊、中間標題、右側頁次）

**使用範例**：
```csharp
var headerBuilder = new ReportHeaderBuilder();
headerBuilder
    .SetCompanyInfo(company?.TaxId, company?.Phone, company?.Fax)
    .SetTitle(company?.CompanyName, "採購單")
    .SetPageInfo(currentPage, totalPages);

html.Append(headerBuilder.Build());
```

**API 方法**：
- `SetCompanyInfo(taxId, phone, fax)` - 設定公司資訊（左側區域）
- `AddLeftInfoRow(label, value)` - 自訂新增左側資訊列
- `SetTitle(companyName, reportTitle)` - 設定報表標題（中間區域）
- `SetPageInfo(currentPage, totalPages)` - 設定頁次資訊（右側區域）
- `SetCustomPageInfo(pageInfo)` - 自訂頁次資訊
- `SetCssClass(cssClass)` - 設定外層 CSS 類別
- `Build()` - 建構 HTML
- `Clear()` - 清空內容（重複使用）

---

### 2. ReportInfoSectionBuilder - 資訊區塊建構器

**用途**：生成標準的 Grid 佈局資訊區塊（標籤 + 值）

**使用範例**：
```csharp
var infoBuilder = new ReportInfoSectionBuilder();
infoBuilder
    .AddField("採購單號", purchaseOrder.PurchaseOrderNumber)
    .AddDateField("採購日期", purchaseOrder.OrderDate)
    .AddDateField("交貨日期", purchaseOrder.ExpectedDeliveryDate)
    .AddField("廠商名稱", supplier?.CompanyName)
    .AddField("聯絡人", supplier?.ContactPerson)
    .AddField("統一編號", supplier?.TaxNumber)
    .AddField("送貨地址", company?.Address, columnSpan: 3);  // 跨 3 欄

html.Append(infoBuilder.Build());
```

**API 方法**：
- `AddField(label, value, columnSpan, customStyle)` - 新增資訊欄位
- `AddFieldIf(condition, label, value, ...)` - 條件式新增欄位
- `AddDateField(label, date, columnSpan)` - 新增日期欄位（自動格式化 yyyy/MM/dd）
- `AddDateTimeField(label, dateTime, columnSpan)` - 新增日期時間欄位
- `AddAmountField(label, amount, columnSpan)` - 新增金額欄位（千分位小數）
- `AddQuantityField(label, quantity, columnSpan)` - 新增數量欄位（千分位整數）
- `SetCssClass(cssClass)` - 設定外層 CSS 類別
- `Build()` - 建構 HTML
- `Clear()` - 清空內容

**重要參數**：
- `columnSpan` - 跨欄數（預設 1），用於「送貨地址」等需要跨多欄的欄位

---

### 3. ReportTableBuilder<T> - 表格建構器

**用途**：生成標準的明細表格（支援泛型與欄位定義）

**使用範例**：
```csharp
var tableBuilder = new ReportTableBuilder<PurchaseOrderDetail>();
tableBuilder
    .AddIndexColumn("序號", "5%", startRowNum)
    .AddTextColumn("品名", "25%", detail => productDict.GetValueOrDefault(detail.ProductId)?.Name ?? "")
    .AddQuantityColumn("數量", "8%", detail => detail.OrderQuantity)
    .AddTextColumn("單位", "5%", detail => "個", "text-center")
    .AddAmountColumn("單價", "12%", detail => detail.UnitPrice)
    .AddAmountColumn("小計", "15%", detail => detail.SubtotalAmount)
    .AddTextColumn("備註", "30%", detail => detail.Remarks ?? "");

html.Append(tableBuilder.Build(orderDetails, startRowNum));
```

**API 方法**：
- `AddColumn(header, width, valueGetter, alignment)` - 新增自訂欄位
- `AddIndexColumn(header, width, startIndex)` - 新增序號欄位（自動產生）
- `AddTextColumn(header, width, valueGetter, alignment)` - 新增文字欄位
- `AddQuantityColumn(header, width, valueGetter)` - 新增數量欄位（千分位整數）
- `AddAmountColumn(header, width, valueGetter)` - 新增金額欄位（千分位小數）
- `AddDateColumn(header, width, valueGetter)` - 新增日期欄位
- `SetCssClass(cssClass)` - 設定表格 CSS 類別
- `Build(items, startRowNum)` - 建構 HTML
- `Clear()` - 清空欄位定義

**Lambda 表達式參數**：
- `detail => detail.PropertyName` - 取值函式
- `(detail, index) => ...` - 帶索引的取值函式

**對齊方式**：
- `"text-left"` - 靠左對齊
- `"text-center"` - 置中對齊
- `"text-right"` - 靠右對齊

---

### 4. ReportSummaryBuilder - 統計區建構器

**用途**：生成標準的統計區域（左側備註、右側金額統計）

**使用範例**：
```csharp
var summaryBuilder = new ReportSummaryBuilder();
summaryBuilder
    .SetRemarks(purchaseOrder.Remarks)
    .AddAmountItem("金額小計", purchaseOrder.TotalAmount)
    .AddSummaryItem($"稅額({taxRate:F2}%)", purchaseOrder.PurchaseTaxAmount.ToString("N2"))
    .AddAmountItem("含稅總計", purchaseOrder.PurchaseTotalAmountIncludingTax);

html.Append(summaryBuilder.Build());
```

**API 方法**：
- `SetRemarks(remarks)` - 設定備註內容（左側區域）
- `AddSummaryItem(label, value)` - 新增統計項目（右側區域）
- `AddAmountItem(label, amount)` - 新增金額統計項目（千分位小數）
- `AddQuantityItem(label, quantity)` - 新增數量統計項目（千分位整數）
- `AddSummaryItemIf(condition, label, value)` - 條件式新增統計項目
- `SetCssClass(cssClass)` - 設定外層 CSS 類別
- `Build()` - 建構 HTML
- `Clear()` - 清空內容

---

### 5. ReportSignatureBuilder - 簽名區建構器

**用途**：生成標準的簽名區域（多個簽名欄位並排顯示）

**使用範例**：
```csharp
var signatureBuilder = new ReportSignatureBuilder();
signatureBuilder
    .AddSignatures("採購人員", "核准人員", "收貨確認");

html.Append(signatureBuilder.Build());
```

**API 方法**：
- `AddSignature(label)` - 新增單個簽名欄位
- `AddSignatures(params labels)` - 批次新增多個簽名欄位
- `AddSignatureIf(condition, label)` - 條件式新增簽名欄位
- `SetCssClass(cssClass)` - 設定外層 CSS 類別
- `Build()` - 建構 HTML
- `Clear()` - 清空內容

---

## 📊 實際案例比較

### ❌ 重構前（GenerateInfoSection）

```csharp
private void GenerateInfoSection(StringBuilder html, PurchaseOrder purchaseOrder, Supplier? supplier, Company? company)
{
    html.AppendLine("            <div class='print-info-section'>");
    html.AppendLine("                <div class='print-info-grid'>");
    
    html.AppendLine("                    <div class='print-info-item'>");
    html.AppendLine("                        <span class='print-info-label'>採購單號：</span>");
    html.AppendLine($"                        <span class='print-info-value'>{purchaseOrder.PurchaseOrderNumber}</span>");
    html.AppendLine("                    </div>");
    
    html.AppendLine("                    <div class='print-info-item'>");
    html.AppendLine("                        <span class='print-info-label'>採購日期：</span>");
    html.AppendLine($"                        <span class='print-info-value'>{purchaseOrder.OrderDate:yyyy/MM/dd}</span>");
    html.AppendLine("                    </div>");
    
    // ... 重複 5 次以上 ...
    
    html.AppendLine("                </div>");
    html.AppendLine("            </div>");
}
```

**程式碼行數**：約 40 行

---

### ✅ 重構後（使用 Builder）

```csharp
private void GenerateInfoSection(StringBuilder html, PurchaseOrder purchaseOrder, Supplier? supplier, Company? company)
{
    var infoBuilder = new ReportInfoSectionBuilder();
    infoBuilder
        .AddField("採購單號", purchaseOrder.PurchaseOrderNumber)
        .AddDateField("採購日期", purchaseOrder.OrderDate)
        .AddDateField("交貨日期", purchaseOrder.ExpectedDeliveryDate)
        .AddField("廠商名稱", supplier?.CompanyName)
        .AddField("聯絡人", supplier?.ContactPerson)
        .AddField("統一編號", supplier?.TaxNumber)
        .AddField("送貨地址", company?.Address, columnSpan: 3);

    html.Append(infoBuilder.Build());
}
```

**程式碼行數**：約 13 行

**減少比例**：約 67.5% 📉

---

## 🚀 其他報表如何套用

### 範例：進貨單報表

```csharp
// 表頭
var headerBuilder = new ReportHeaderBuilder();
headerBuilder
    .SetCompanyInfo(company?.TaxId, company?.Phone, company?.Fax)
    .SetTitle(company?.CompanyName, "進貨單")
    .SetPageInfo(currentPage, totalPages);
html.Append(headerBuilder.Build());

// 資訊區塊
var infoBuilder = new ReportInfoSectionBuilder();
infoBuilder
    .AddField("進貨單號", receipt.ReceiptNumber)
    .AddDateField("進貨日期", receipt.ReceiptDate)
    .AddField("廠商名稱", supplier?.CompanyName)
    .AddField("聯絡人", supplier?.ContactPerson)
    .AddField("送貨地址", company?.Address, columnSpan: 3);
html.Append(infoBuilder.Build());

// 明細表格
var tableBuilder = new ReportTableBuilder<PurchaseReceiptDetail>();
tableBuilder
    .AddIndexColumn("序號", "5%", startRowNum)
    .AddTextColumn("品名", "25%", detail => productDict[detail.ProductId]?.Name ?? "")
    .AddQuantityColumn("數量", "8%", detail => detail.ReceivedQuantity)
    .AddAmountColumn("單價", "12%", detail => detail.UnitPrice)
    .AddAmountColumn("小計", "15%", detail => detail.SubtotalAmount);
html.Append(tableBuilder.Build(receiptDetails, startRowNum));

// 統計區
var summaryBuilder = new ReportSummaryBuilder();
summaryBuilder
    .SetRemarks(receipt.Remarks)
    .AddAmountItem("金額小計", receipt.TotalAmount)
    .AddAmountItem("稅額", receipt.TaxAmount)
    .AddAmountItem("含稅總計", receipt.TotalAmountIncludingTax);
html.Append(summaryBuilder.Build());

// 簽名區
var signatureBuilder = new ReportSignatureBuilder();
signatureBuilder.AddSignatures("進貨人員", "驗收人員", "主管核准");
html.Append(signatureBuilder.Build());
```

---

## 💡 進階技巧

### 1. 條件式欄位顯示

```csharp
infoBuilder
    .AddField("採購單號", purchaseOrder.PurchaseOrderNumber)
    .AddFieldIf(showSupplier, "廠商名稱", supplier?.CompanyName)
    .AddFieldIf(hasDeliveryDate, "交貨日期", deliveryDate?.ToString("yyyy/MM/dd"));
```

### 2. 自訂欄位樣式

```csharp
infoBuilder
    .AddField("重要事項", importantNote, columnSpan: 3, customStyle: "color: red; font-weight: bold");
```

### 3. 複雜的表格欄位

```csharp
tableBuilder
    .AddColumn("狀態", "10%", (detail, index) => 
    {
        return detail.IsCompleted ? "已完成" : "進行中";
    }, "text-center");
```

### 4. 重複使用 Builder

```csharp
var infoBuilder = new ReportInfoSectionBuilder();

// 第一頁
infoBuilder.AddField("欄位1", "值1").AddField("欄位2", "值2");
html.Append(infoBuilder.Build());

// 清空後重複使用於第二頁
infoBuilder.Clear();
infoBuilder.AddField("欄位3", "值3").AddField("欄位4", "值4");
html.Append(infoBuilder.Build());
```

---

## 🎨 CSS 樣式相容性

所有 Builder 產生的 HTML 都與現有的 `print-styles.css` 完全相容，使用以下 CSS 類別：

- `.print-header` / `.print-company-header`
- `.print-info-section` / `.print-info-grid` / `.print-info-item`
- `.print-table` / `thead` / `tbody` / `th` / `td`
- `.print-summary` / `.print-summary-left` / `.print-summary-right`
- `.print-signature-section` / `.print-signature-item`

如需調整樣式，只需修改 CSS，所有報表自動套用。

---

## ✅ 最佳實踐

1. **統一使用 Builder**：新報表一律使用 Builder，保持程式碼風格一致
2. **善用型別安全**：利用 Lambda 表達式和泛型，減少執行期錯誤
3. **避免硬編碼**：將常用的欄位定義抽成靜態方法或設定
4. **保持簡潔**：使用鏈式呼叫（Fluent API），一氣呵成
5. **適時註解**：對於特殊欄位（如跨欄）加上註解說明

---

## 📁 檔案位置

所有 Builder 類別位於：
```
Services/Reports/Common/
  - ReportHeaderBuilder.cs
  - ReportInfoSectionBuilder.cs
  - ReportTableBuilder.cs
  - ReportSummaryBuilder.cs
  - ReportSignatureBuilder.cs
```

---

## 🔄 版本歷程

- **v1.0**（2025-10-17）：初版建立，包含 5 個核心 Builder
- 已套用至 `PurchaseOrderReportService`

---

## 📞 技術支援

如有問題或建議，請聯繫開發團隊。
