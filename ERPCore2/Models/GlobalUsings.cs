// ============================================================================
// Models 資料夾全域 using 宣告
// 讓所有 Models 子資料夾的類別可以在專案中直接使用
// ============================================================================

global using ERPCore2.Models.Barcode;
global using ERPCore2.Models.Configuration;
global using ERPCore2.Models.Documents;
global using ERPCore2.Models.Enums;
global using ERPCore2.Models.Inventory;
global using ERPCore2.Models.Navigation;
global using ERPCore2.Models.Permissions;
global using ERPCore2.Models.Printing;
global using ERPCore2.Models.Schedule;
global using ERPCore2.Models.Reports;

// ============================================================================
// 向後相容性別名
// ============================================================================

// BarcodePrintSettings 已重命名為 BarcodePrintConfig
global using BarcodePrintSettings = ERPCore2.Models.Barcode.BarcodePrintConfig;

// SupplierRecommendation 已重命名為 SupplierRecommendationDto
global using SupplierRecommendation = ERPCore2.Models.Inventory.SupplierRecommendationDto;

// RelatedDocument 已重命名為 RelatedDocumentInfo 以避免與 namespace 衝突
global using RelatedDocument = ERPCore2.Models.Documents.RelatedDocumentInfo;
