## 權限修復說明

### 已修復的編譯錯誤

1. **TableColumnDefinition.Custom 方法不存在**
   - 修復：使用 `TableColumnDefinition.Template` 替代
   - 影響檔案：`MaterialIndex.razor`

2. **Supplier 實體屬性名稱錯誤**
   - 修復：將 `Supplier.Name` 改為 `Supplier.CompanyName`
   - 修復：將 `Supplier.Code` 改為 `Supplier.SupplierCode`
   - 影響檔案：`MaterialEdit.razor`, `MaterialDetail.razor`

3. **SupplierContact 聯絡資訊存取錯誤**
   - 修復：使用 `ContactType.TypeName` 和 `SupplierContact.ContactValue`
   - 影響檔案：`MaterialDetail.razor`

4. **Null reference 警告**
   - 修復：添加 null-forgiving operator (!)
   - 影響檔案：`MaterialDetail.razor`, `WeatherDetail.razor`

### 權限設定完善

#### 1. AuthSeeder.cs 更新
已添加完整的 BOM 基礎元素權限：

**材質管理權限：**
- `Material.Create` - 建立材質
- `Material.Read` - 檢視材質
- `Material.Update` - 修改材質
- `Material.Delete` - 刪除材質

**天氣管理權限：**
- `Weather.Create` - 建立天氣
- `Weather.Read` - 檢視天氣
- `Weather.Update` - 修改天氣
- `Weather.Delete` - 刪除天氣

**顏色管理權限：**
- `Color.Create` - 建立顏色
- `Color.Read` - 檢視顏色
- `Color.Update` - 修改顏色
- `Color.Delete` - 刪除顏色

#### 2. PermissionCheck.razor 更新
已將所有 BOM 基礎元素的 CRUD 權限添加到開發模式允許清單中，確保在開發階段可以正常測試功能。

### 建議的資料庫更新

由於新增了權限設定，建議執行以下命令來更新資料庫：

```bash
dotnet ef migrations add AddBOMFoundationPermissions
dotnet ef database update
```

或者重新執行資料種子：

```bash
# 在應用程式啟動時會自動執行 AuthSeeder
```

### 完成的功能驗證

✅ Material 頁面編譯成功  
✅ 權限設定完整  
✅ 導航選單整合  
✅ 錯誤修復完成  
✅ 警告處理完成  

現在 Material 管理功能已經完全可用，並整合到 ERPCore2 系統中。