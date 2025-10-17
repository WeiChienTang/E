# 報表建構器快速參考

## 🚀 快速開始

### 1️⃣ 表頭（Header）
```csharp
var headerBuilder = new ReportHeaderBuilder();
headerBuilder
    .SetCompanyInfo(company?.TaxId, company?.Phone, company?.Fax)
    .SetTitle(company?.CompanyName, "報表標題")
    .SetPageInfo(currentPage, totalPages);
html.Append(headerBuilder.Build());
```

### 2️⃣ 資訊區塊（Info Section）
```csharp
var infoBuilder = new ReportInfoSectionBuilder();
infoBuilder
    .AddField("標籤", "值")
    .AddDateField("日期標籤", dateValue)
    .AddAmountField("金額標籤", amountValue)
    .AddField("跨欄標籤", "值", columnSpan: 3);  // 跨 3 欄
html.Append(infoBuilder.Build());
```

### 3️⃣ 明細表格（Detail Table）
```csharp
var tableBuilder = new ReportTableBuilder<YourDetailType>();
tableBuilder
    .AddIndexColumn("序號", "5%", startRowNum)
    .AddTextColumn("文字欄", "20%", item => item.PropertyName)
    .AddQuantityColumn("數量欄", "10%", item => item.Quantity)
    .AddAmountColumn("金額欄", "15%", item => item.Amount)
    .AddDateColumn("日期欄", "15%", item => item.Date);
html.Append(tableBuilder.Build(detailsList, startRowNum));
```

### 4️⃣ 統計區（Summary）
```csharp
var summaryBuilder = new ReportSummaryBuilder();
summaryBuilder
    .SetRemarks("備註內容")
    .AddAmountItem("金額小計", totalAmount)
    .AddAmountItem("稅額", taxAmount)
    .AddAmountItem("含稅總計", totalIncludingTax);
html.Append(summaryBuilder.Build());
```

### 5️⃣ 簽名區（Signature）
```csharp
var signatureBuilder = new ReportSignatureBuilder();
signatureBuilder.AddSignatures("簽名1", "簽名2", "簽名3");
html.Append(signatureBuilder.Build());
```

---

## 📝 常用方法速查

### ReportInfoSectionBuilder
| 方法 | 用途 | 範例 |
|------|------|------|
| `AddField(label, value)` | 新增一般欄位 | `.AddField("單號", orderNo)` |
| `AddDateField(label, date)` | 新增日期欄位 | `.AddDateField("日期", orderDate)` |
| `AddAmountField(label, amount)` | 新增金額欄位 | `.AddAmountField("金額", total)` |
| `AddQuantityField(label, qty)` | 新增數量欄位 | `.AddQuantityField("數量", qty)` |
| `AddFieldIf(cond, label, value)` | 條件式新增 | `.AddFieldIf(show, "備註", note)` |

### ReportTableBuilder<T>
| 方法 | 用途 | 範例 |
|------|------|------|
| `AddIndexColumn(header, width, start)` | 序號欄 | `.AddIndexColumn("序號", "5%", 0)` |
| `AddTextColumn(header, width, getter)` | 文字欄 | `.AddTextColumn("品名", "25%", x => x.Name)` |
| `AddQuantityColumn(header, width, getter)` | 數量欄 | `.AddQuantityColumn("數量", "10%", x => x.Qty)` |
| `AddAmountColumn(header, width, getter)` | 金額欄 | `.AddAmountColumn("金額", "15%", x => x.Amount)` |
| `AddDateColumn(header, width, getter)` | 日期欄 | `.AddDateColumn("日期", "15%", x => x.Date)` |

### 對齊方式
- `"text-left"` - 靠左（預設）
- `"text-center"` - 置中
- `"text-right"` - 靠右

---

## 💡 實用技巧

### 跨欄欄位
```csharp
.AddField("送貨地址", address, columnSpan: 3)
```

### 條件式欄位
```csharp
.AddFieldIf(hasSupplier, "廠商", supplierName)
```

### 自訂表格欄位
```csharp
.AddColumn("狀態", "10%", (item, index) => 
    item.IsCompleted ? "完成" : "處理中", 
    "text-center")
```

### Dictionary 查詢
```csharp
.AddTextColumn("品名", "25%", 
    detail => productDict.GetValueOrDefault(detail.ProductId)?.Name ?? "")
```

---

## 📊 程式碼減少對比

| 方法 | 重構前 | 重構後 | 減少 |
|------|--------|--------|------|
| GenerateHeader | ~20 行 | ~7 行 | 65% |
| GenerateInfoSection | ~40 行 | ~13 行 | 67% |
| GenerateDetailTable | ~30 行 | ~12 行 | 60% |
| GenerateSummarySection | ~25 行 | ~9 行 | 64% |
| GenerateSignatureSection | ~15 行 | ~5 行 | 67% |

**總計減少約 45-50% 的程式碼量** 🎉

---

## ✅ 檢查清單

建立新報表時，依序使用：
- [ ] ReportHeaderBuilder - 表頭
- [ ] ReportInfoSectionBuilder - 資訊區塊
- [ ] ReportTableBuilder - 明細表格
- [ ] ReportSummaryBuilder - 統計區
- [ ] ReportSignatureBuilder - 簽名區

---

## 📁 檔案位置
```
Services/Reports/Common/
  - ReportHeaderBuilder.cs
  - ReportInfoSectionBuilder.cs
  - ReportTableBuilder.cs
  - ReportSummaryBuilder.cs
  - ReportSignatureBuilder.cs
```

詳細說明請參考：`Documentation/README_ReportBuilders.md`
