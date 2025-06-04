# Modals 資料夾說明

## 1. 主要存放的組件類型
- **模態視窗組件** - 提供各種類型的彈出式對話框和管理介面

## 2. 擁有的組件功能、適用場景

### GenericManagementModal.razor
- **功能描述**: 泛型資料管理模態視窗，支援任意實體類型的 CRUD 操作
- **適用場景**: 
  - 系統主檔資料的維護管理
  - 快速新增關聯實體資料
  - 批量資料管理操作
  - 嵌入式資料管理功能

### ContactTypeManagementModal.razor
- **功能描述**: 聯絡方式類型管理的專用模態視窗
- **適用場景**: 
  - 聯絡方式類型的新增、編輯、刪除
  - 聯絡方式類型狀態管理
  - 客戶管理時的聯絡類型維護

### CustomerTypeManagementModal.razor
- **功能描述**: 客戶類型管理的專用模態視窗
- **適用場景**: 
  - 客戶類型的新增、編輯、刪除
  - 客戶類型狀態管理
  - 客戶建立時的類型維護

### IndustryTypeManagementModal.razor
- **功能描述**: 行業類型管理的專用模態視窗
- **適用場景**: 
  - 行業類型的新增、編輯、刪除
  - 行業類型狀態管理
  - 客戶建立時的行業維護

## 3. 功能說明

### GenericManagementModal 特性
- **泛型支援**: 支援任意實體類型的管理操作
- **完整 CRUD**: 提供建立、讀取、更新、刪除的完整功能
- **狀態管理**: 支援實體狀態的切換操作
- **確認對話**: 內建刪除確認機制
- **非同步操作**: 所有操作都使用非同步處理
- **訊息回饋**: 完整的成功/錯誤訊息顯示
- **載入狀態**: 提供操作過程中的載入指示
- **表單驗證**: 整合 DataAnnotations 驗證機制

### 專用管理模態視窗特性
- **特化配置**: 針對特定實體類型的客製化設定
- **預設樣式**: 使用對應的圖示和色彩主題
- **業務邏輯**: 整合特定實體的業務規則
- **服務整合**: 直接注入對應的服務類別

### 使用方式
```razor
<!-- 泛型管理模態視窗 -->
<GenericManagementModal TEntity="ContactType"
                      ModalId="contactTypeModal"
                      Title="聯絡方式類型管理"
                      EntityDisplayName="聯絡方式類型"
                      IconClass="fas fa-phone"
                      Service="@contactTypeService"
                      GetEntityId="@(ct => ct.Id)"
                      GetEntityStatus="@(ct => ct.Status)"
                      SetCreatedBy="@((ct, user) => ct.CreatedBy = user)"
                      SetEntityStatus="@((ct, status) => ct.Status = status)">
    
    <CreateFormContent Context="entity">
        <!-- 表單內容 -->
    </CreateFormContent>
    
    <TableHeaders>
        <!-- 表格標題 -->
    </TableHeaders>
    
    <TableRowContent Context="entity">
        <!-- 表格行內容 -->
    </TableRowContent>
</GenericManagementModal>

<!-- 專用管理模態視窗 -->
<ContactTypeManagementModal @ref="contactTypeModal" />
```

### 核心功能
- **資料載入**: 自動載入最新的實體資料
- **表單處理**: 完整的表單建立和驗證
- **狀態切換**: 一鍵啟用/停用實體狀態
- **安全刪除**: 包含確認對話的刪除操作
- **併發控制**: 使用 SemaphoreSlim 防止併發操作
- **事件通知**: 支援資料變更通知機制

### 設計原則
- 提供一致的資料管理體驗
- 支援響應式設計和無障礙存取
- 遵循 ERP 系統的視覺設計規範
- 確保資料操作的安全性和完整性
