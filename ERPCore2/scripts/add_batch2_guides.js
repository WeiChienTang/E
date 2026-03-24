const fs = require('fs');
const path = require('path');
const guideDir = path.join(__dirname, '..', 'Models', 'FeatureGuides', 'GuideDefinitions');
const resxDir = path.join(__dirname, '..', 'Resources');
const pagesDir = path.join(__dirname, '..', 'Components', 'Pages');

// ==================== Guide Definitions ====================
const guides = [
  // Items
  { cls: "ItemGuide", prefix: "item", wire: "Items/ItemEditModalComponent.razor",
    desc: "Guide.Item.Description", tabOverview: "Guide.Item.TabOverview",
    fields: [["Field.Code","Guide.Item.Field.Code"],["Field.Name","Guide.Item.Field.Name"],["Field.ItemCategory","Guide.Item.Field.Category"],["Field.Unit","Guide.Item.Field.Unit"],["Field.Size","Guide.Item.Field.Size"],["Field.Supplier","Guide.Item.Field.Supplier"],["Field.Description","Guide.Item.Field.Spec"],["Field.Remarks","Guide.Item.Field.Remarks"]],
    actions: [["Guide.Item.Action.PrintLabel","Guide.Item.Action.Print"]],
    tips: ["Guide.Item.Tip1"], warnings: ["Guide.Item.Warning1"],
    faqs: [["Guide.Item.Faq1Q","Guide.Item.Faq1A"],["Guide.Item.Faq2Q","Guide.Item.Faq2A"]]
  },
  { cls: "ItemCompositionGuide", prefix: "icomp", wire: "Items/ItemCompositionEditModalComponent.razor",
    desc: "Guide.ItemComposition.Description",
    fields: [["Field.Code","Guide.ItemComposition.Field.Code"],["Field.Name","Guide.ItemComposition.Field.Name"],["Field.Description","Guide.ItemComposition.Field.Spec"],["Field.Remarks","Guide.ItemComposition.Field.Remarks"]],
    tips: ["Guide.ItemComposition.Tip1"]
  },
  // Customers
  { cls: "CustomerGuide", prefix: "cust", wire: "Customers/CustomerEditModal/CustomerEditModalComponent.razor",
    desc: "Guide.Customer.Description", tabOverview: "Guide.Customer.TabOverview",
    fields: [["Field.CustomerCode","Guide.Customer.Field.Code"],["Field.Name","Guide.Customer.Field.Name"],["Field.CustomerType","Guide.Customer.Field.Type"],["Field.ContactPerson","Guide.Customer.Field.Contact"],["Field.Phone","Guide.Customer.Field.Phone"],["Field.Email","Guide.Customer.Field.Email"],["Field.Address","Guide.Customer.Field.Address"],["Field.Remarks","Guide.Customer.Field.Remarks"]],
    tips: ["Guide.Customer.Tip1"], warnings: ["Guide.Customer.Warning1"],
    faqs: [["Guide.Customer.Faq1Q","Guide.Customer.Faq1A"]]
  },
  { cls: "CustomerVisitGuide", prefix: "cv", wire: "Customers/CustomerVisitEditModalComponent.razor",
    desc: "Guide.CustomerVisit.Description",
    fields: [["Field.Customer","Guide.CustomerVisit.Field.Customer"],["Field.VisitDate","Guide.CustomerVisit.Field.Date"],["Field.VisitType","Guide.CustomerVisit.Field.Type"],["Field.Employee","Guide.CustomerVisit.Field.Employee"],["Field.Remarks","Guide.CustomerVisit.Field.Remarks"]]
  },
  { cls: "CustomerComplaintGuide", prefix: "ccmp", wire: "Customers/CustomerComplaintEditModalComponent.razor",
    desc: "Guide.CustomerComplaint.Description",
    fields: [["Field.Customer","Guide.CustomerComplaint.Field.Customer"],["Field.Name","Guide.CustomerComplaint.Field.Title"],["Field.Description","Guide.CustomerComplaint.Field.Description"],["Field.Remarks","Guide.CustomerComplaint.Field.Remarks"]],
    tips: ["Guide.CustomerComplaint.Tip1"]
  },
  { cls: "CustomerBankAccountGuide", prefix: "cba", wire: "Customers/CustomerBankAccountEditModalComponent.razor",
    desc: "Guide.CustomerBankAccount.Description",
    fields: [["Field.BankName","Guide.CustomerBankAccount.Field.Bank"],["Field.Code","Guide.CustomerBankAccount.Field.AccountNumber"],["Field.Name","Guide.CustomerBankAccount.Field.AccountName"],["Field.Remarks","Guide.CustomerBankAccount.Field.Remarks"]]
  },
  // Suppliers
  { cls: "SupplierGuide", prefix: "supp", wire: "Suppliers/SupplierEditModal/SupplierEditModalComponent.razor",
    desc: "Guide.Supplier.Description", tabOverview: "Guide.Supplier.TabOverview",
    fields: [["Field.Code","Guide.Supplier.Field.Code"],["Field.Name","Guide.Supplier.Field.Name"],["Field.ContactPerson","Guide.Supplier.Field.Contact"],["Field.Phone","Guide.Supplier.Field.Phone"],["Field.Email","Guide.Supplier.Field.Email"],["Field.Address","Guide.Supplier.Field.Address"],["Field.Remarks","Guide.Supplier.Field.Remarks"]],
    tips: ["Guide.Supplier.Tip1"],
    faqs: [["Guide.Supplier.Faq1Q","Guide.Supplier.Faq1A"]]
  },
  { cls: "SupplierVisitGuide", prefix: "sv", wire: "Suppliers/SupplierVisitEditModalComponent.razor",
    desc: "Guide.SupplierVisit.Description",
    fields: [["Field.Supplier","Guide.SupplierVisit.Field.Supplier"],["Field.VisitDate","Guide.SupplierVisit.Field.Date"],["Field.VisitType","Guide.SupplierVisit.Field.Type"],["Field.Employee","Guide.SupplierVisit.Field.Employee"],["Field.Remarks","Guide.SupplierVisit.Field.Remarks"]]
  },
  { cls: "SupplierBankAccountGuide", prefix: "sba", wire: "Suppliers/SupplierBankAccountEditModalComponent.razor",
    desc: "Guide.SupplierBankAccount.Description",
    fields: [["Field.BankName","Guide.SupplierBankAccount.Field.Bank"],["Field.Code","Guide.SupplierBankAccount.Field.AccountNumber"],["Field.Name","Guide.SupplierBankAccount.Field.AccountName"],["Field.Remarks","Guide.SupplierBankAccount.Field.Remarks"]]
  },
  // Employees
  { cls: "EmployeeGuide", prefix: "emp", wire: "Employees/EmployeeEditModal/EmployeeEditModalComponent.razor",
    desc: "Guide.Employee.Description", tabOverview: "Guide.Employee.TabOverview",
    fields: [["Field.EmployeeCode","Guide.Employee.Field.Code"],["Field.FullName","Guide.Employee.Field.Name"],["Field.Gender","Guide.Employee.Field.Gender"],["Field.Birthday","Guide.Employee.Field.Birthday"],["Field.PhoneNumber","Guide.Employee.Field.Phone"],["Field.Department","Guide.Employee.Field.Department"],["Field.EmployeePosition","Guide.Employee.Field.Position"],["Field.HireDate","Guide.Employee.Field.HireDate"],["Field.Remarks","Guide.Employee.Field.Remarks"]],
    tips: ["Guide.Employee.Tip1"], warnings: ["Guide.Employee.Warning1"]
  },
  { cls: "EmployeeLicenseGuide", prefix: "elic", wire: "Employees/EmployeeEditModal/EmployeeLicenseEditModalComponent.razor",
    desc: "Guide.EmployeeLicense.Description",
    fields: [["Field.LicenseName","Guide.EmployeeLicense.Field.Name"],["Field.LicenseNumber","Guide.EmployeeLicense.Field.Number"],["Field.IssuingAuthority","Guide.EmployeeLicense.Field.Authority"],["Field.ExpiryDate","Guide.EmployeeLicense.Field.Expiry"],["Field.Remarks","Guide.EmployeeLicense.Field.Remarks"]]
  },
  { cls: "EmployeeToolGuide", prefix: "etool", wire: "Employees/EmployeeEditModal/EmployeeToolEditModalComponent.razor",
    desc: "Guide.EmployeeTool.Description",
    fields: [["Field.ToolName","Guide.EmployeeTool.Field.Name"],["Field.SerialModel","Guide.EmployeeTool.Field.Serial"],["Field.AssignDate","Guide.EmployeeTool.Field.AssignDate"],["Field.ReturnDate","Guide.EmployeeTool.Field.ReturnDate"]]
  },
  { cls: "EmployeeTrainingRecordGuide", prefix: "etr", wire: "Employees/EmployeeEditModal/EmployeeTrainingRecordEditModalComponent.razor",
    desc: "Guide.EmployeeTrainingRecord.Description",
    fields: [["Field.CourseName","Guide.EmployeeTrainingRecord.Field.Course"],["Field.TrainingDate","Guide.EmployeeTrainingRecord.Field.Date"],["Field.TrainingHours","Guide.EmployeeTrainingRecord.Field.Hours"],["Field.TrainingOrganization","Guide.EmployeeTrainingRecord.Field.Organization"]]
  },
  // Vehicles
  { cls: "VehicleGuide", prefix: "veh", wire: "Vehicles/VehicleEditModalComponent.razor",
    desc: "Guide.Vehicle.Description",
    fields: [["Field.Code","Guide.Vehicle.Field.Code"],["Field.Name","Guide.Vehicle.Field.LicensePlate"],["Field.VehicleType","Guide.Vehicle.Field.Type"],["Field.Description","Guide.Vehicle.Field.Spec"],["Field.Remarks","Guide.Vehicle.Field.Remarks"]],
    tips: ["Guide.Vehicle.Tip1"]
  },
  { cls: "VehicleMaintenanceGuide", prefix: "vmaint", wire: "Vehicles/VehicleMaintenanceEditModalComponent.razor",
    desc: "Guide.VehicleMaintenance.Description",
    fields: [["Field.Name","Guide.VehicleMaintenance.Field.Vehicle"],["Field.Description","Guide.VehicleMaintenance.Field.Description"],["Field.Remarks","Guide.VehicleMaintenance.Field.Remarks"]]
  },
  // Equipment
  { cls: "EquipmentGuide", prefix: "equip", wire: "Equipment/EquipmentEditModalComponent.razor",
    desc: "Guide.Equipment.Description",
    fields: [["Field.Code","Guide.Equipment.Field.Code"],["Field.Name","Guide.Equipment.Field.Name"],["Field.Description","Guide.Equipment.Field.Description"],["Field.Remarks","Guide.Equipment.Field.Remarks"]],
    tips: ["Guide.Equipment.Tip1"]
  },
  { cls: "EquipmentMaintenanceGuide", prefix: "emaint", wire: "Equipment/EquipmentMaintenanceEditModalComponent.razor",
    desc: "Guide.EquipmentMaintenance.Description",
    fields: [["Field.Name","Guide.EquipmentMaintenance.Field.Equipment"],["Field.Description","Guide.EquipmentMaintenance.Field.Description"],["Field.Remarks","Guide.EquipmentMaintenance.Field.Remarks"]]
  },
  // CRM
  { cls: "CrmLeadGuide", prefix: "crm", wire: "Crm/CrmLeadEditModalComponent.razor",
    desc: "Guide.CrmLead.Description",
    fields: [["Field.Name","Guide.CrmLead.Field.CompanyName"],["Field.ContactPerson","Guide.CrmLead.Field.Contact"],["Field.Phone","Guide.CrmLead.Field.Phone"],["Field.Email","Guide.CrmLead.Field.Email"],["Field.Description","Guide.CrmLead.Field.Industry"],["Field.Remarks","Guide.CrmLead.Field.Remarks"]],
    actions: [["Guide.CrmLead.Action.ConvertLabel","Guide.CrmLead.Action.Convert"]],
    tips: ["Guide.CrmLead.Tip1"], warnings: ["Guide.CrmLead.Warning1"]
  },
  { cls: "CrmLeadFollowUpGuide", prefix: "crmfu", wire: "Crm/CrmLeadFollowUpEditModalComponent.razor",
    desc: "Guide.CrmLeadFollowUp.Description",
    fields: [["Field.Name","Guide.CrmLeadFollowUp.Field.Type"],["Field.Description","Guide.CrmLeadFollowUp.Field.Notes"],["Field.Remarks","Guide.CrmLeadFollowUp.Field.Remarks"]]
  },
  // ScaleManagement
  { cls: "ScaleRecordGuide", prefix: "scr", wire: "ScaleManagement/ScaleRecordEditModalComponent.razor",
    desc: "Guide.ScaleRecord.Description",
    fields: [["Field.Code","Guide.ScaleRecord.Field.Code"],["Field.Name","Guide.ScaleRecord.Field.Vehicle"],["Field.Customer","Guide.ScaleRecord.Field.Customer"],["Field.Remarks","Guide.ScaleRecord.Field.Remarks"]],
    actions: [["Guide.ScaleRecord.Action.ReadLabel","Guide.ScaleRecord.Action.Read"]],
    tips: ["Guide.ScaleRecord.Tip1"]
  },
  // Documents
  { cls: "DocumentGuide", prefix: "doc", wire: "Documents/DocumentEditModalComponent.razor",
    desc: "Guide.Document.Description",
    fields: [["Field.Name","Guide.Document.Field.Title"],["Field.Description","Guide.Document.Field.Content"],["Field.Remarks","Guide.Document.Field.Remarks"]],
    tips: ["Guide.Document.Tip1"]
  },
  // Warehouse
  { cls: "InventoryStockGuide", prefix: "invs", wire: "Warehouse/InventoryStockEditModalComponent.razor",
    desc: "Guide.InventoryStock.Description",
    fields: [["Field.Name","Guide.InventoryStock.Field.Item"],["Field.Warehouse","Guide.InventoryStock.Field.Warehouse"],["Field.Remarks","Guide.InventoryStock.Field.Remarks"]],
    tips: ["Guide.InventoryStock.Tip1"]
  },
  { cls: "InventoryTransactionGuide", prefix: "invt", wire: "Warehouse/InventoryTransactionEditModalComponent.razor",
    desc: "Guide.InventoryTransaction.Description", fields: []
  },
  { cls: "MaterialIssueGuide", prefix: "mi", wire: "Warehouse/MaterialIssueEditModalComponent.razor",
    desc: "Guide.MaterialIssue.Description",
    fields: [["Field.Code","Guide.MaterialIssue.Field.Code"],["Field.Department","Guide.MaterialIssue.Field.Department"],["Field.Employee","Guide.MaterialIssue.Field.Employee"],["Field.Remarks","Guide.MaterialIssue.Field.Remarks"]],
    tips: ["Guide.MaterialIssue.Tip1"]
  },
  { cls: "StockTakingGuide", prefix: "stk", wire: "Warehouse/StockTakingEditModalComponent.razor",
    desc: "Guide.StockTaking.Description",
    fields: [["Field.Code","Guide.StockTaking.Field.Code"],["Field.Warehouse","Guide.StockTaking.Field.Warehouse"],["Field.Employee","Guide.StockTaking.Field.Employee"],["Field.Remarks","Guide.StockTaking.Field.Remarks"]],
    tips: ["Guide.StockTaking.Tip1"], warnings: ["Guide.StockTaking.Warning1"]
  },
  // Financial
  { cls: "SetoffDocumentGuide", prefix: "setoff", wire: "FinancialManagement/SetoffDocumentEditModalComponent.razor",
    desc: "Guide.SetoffDocument.Description",
    fields: [["Field.Code","Guide.SetoffDocument.Field.Code"],["Field.Name","Guide.SetoffDocument.Field.Type"],["Field.Remarks","Guide.SetoffDocument.Field.Remarks"]],
    tips: ["Guide.SetoffDocument.Tip1"], warnings: ["Guide.SetoffDocument.Warning1"]
  },
  { cls: "BankStatementGuide", prefix: "bs", wire: "FinancialManagement/BankStatementEditModalComponent.razor",
    desc: "Guide.BankStatement.Description",
    fields: [["Field.Company","Guide.BankStatement.Field.Company"],["Field.BankName","Guide.BankStatement.Field.BankAccount"],["Field.Remarks","Guide.BankStatement.Field.Remarks"]],
    tips: ["Guide.BankStatement.Tip1"]
  },
  // Accounting
  { cls: "AccountItemGuide", prefix: "acct", wire: "Accounting/AccountItemEditModalComponent.razor",
    desc: "Guide.AccountItem.Description",
    fields: [["Field.Code","Guide.AccountItem.Field.Code"],["Field.Name","Guide.AccountItem.Field.Name"],["Field.Description","Guide.AccountItem.Field.Type"],["Field.Remarks","Guide.AccountItem.Field.Remarks"]],
    tips: ["Guide.AccountItem.Tip1"]
  },
  { cls: "FiscalPeriodGuide", prefix: "fp", wire: "Accounting/FiscalPeriodEditModalComponent.razor",
    desc: "Guide.FiscalPeriod.Description",
    fields: [["Field.Name","Guide.FiscalPeriod.Field.Name"],["Field.Remarks","Guide.FiscalPeriod.Field.Remarks"]],
    warnings: ["Guide.FiscalPeriod.Warning1"]
  },
  { cls: "JournalEntryGuide", prefix: "je", wire: "Accounting/JournalEntryEditModalComponent.razor",
    desc: "Guide.JournalEntry.Description",
    fields: [["Field.Code","Guide.JournalEntry.Field.Code"],["Field.Description","Guide.JournalEntry.Field.Description"],["Field.Remarks","Guide.JournalEntry.Field.Remarks"]],
    tips: ["Guide.JournalEntry.Tip1"], warnings: ["Guide.JournalEntry.Warning1"]
  },
  // Production
  { cls: "ManufacturingOrderGuide", prefix: "mo", wire: "ProductionManagement/ManufacturingOrderEditModalComponent.razor",
    desc: "Guide.ManufacturingOrder.Description",
    fields: [["Field.Code","Guide.ManufacturingOrder.Field.Code"],["Field.Name","Guide.ManufacturingOrder.Field.Item"],["Field.Remarks","Guide.ManufacturingOrder.Field.Remarks"]]
  },
];

// ==================== Generate .cs files ====================
function genCs(g) {
  let sections = `
            new GuideSection
            {
                Id = "guide-${g.prefix}-overview",
                TitleKey = "Guide.Overview",
                Icon = "bi-info-circle",
                BookmarkLabel = "概述",
                BookmarkColor = "#3B82F6",
                Type = GuideSectionType.Description,
                Items = { new("${g.desc}")${g.tabOverview ? `, new("${g.tabOverview}")` : ''} }
            },`;

  if (g.fields && g.fields.length > 0) {
    const items = g.fields.map(f => `                    new("${f[0]}", "${f[1]}")`).join(',\n');
    sections += `

            new GuideSection
            {
                Id = "guide-${g.prefix}-fields",
                TitleKey = "Guide.FieldDescriptions",
                Icon = "bi-input-cursor-text",
                BookmarkLabel = "欄位",
                BookmarkColor = "#F59E0B",
                Type = GuideSectionType.FieldList,
                Items =
                {
${items}
                }
            },`;
  }

  if (g.actions && g.actions.length > 0) {
    const items = g.actions.map(a => `                    new("${a[0]}", "${a[1]}")`).join(',\n');
    sections += `

            new GuideSection
            {
                Id = "guide-${g.prefix}-actions",
                TitleKey = "Guide.SalesOrder.ActionsTitle",
                Icon = "bi-gear",
                BookmarkLabel = "功能",
                BookmarkColor = "#D946EF",
                Type = GuideSectionType.FieldList,
                Items =
                {
${items}
                }
            },`;
  }

  if ((g.tips && g.tips.length > 0) || (g.warnings && g.warnings.length > 0)) {
    const tipItems = (g.tips || []).map(t => `                    new("${t}", GuideItemStyle.Tip)`);
    const warnItems = (g.warnings || []).map(w => `                    new("${w}", GuideItemStyle.Warning)`);
    const allItems = [...tipItems, ...warnItems].join(',\n');
    sections += `

            new GuideSection
            {
                Id = "guide-${g.prefix}-tips",
                TitleKey = "Guide.Tips",
                Icon = "bi-lightbulb",
                BookmarkLabel = "提示",
                BookmarkColor = "#06B6D4",
                Type = GuideSectionType.Tips,
                Items =
                {
${allItems}
                }
            },`;
  }

  if (g.faqs && g.faqs.length > 0) {
    const items = g.faqs.map(f => `                    new("${f[0]}", "${f[1]}")`).join(',\n');
    sections += `

            new GuideSection
            {
                Id = "guide-${g.prefix}-faq",
                TitleKey = "Guide.SalesOrder.FaqTitle",
                Icon = "bi-question-diamond",
                BookmarkLabel = "FAQ",
                BookmarkColor = "#6366F1",
                Type = GuideSectionType.FAQ,
                Items =
                {
${items}
                }
            },`;
  }

  return `using System.Runtime.CompilerServices;

namespace ERPCore2.Models.FeatureGuides.GuideDefinitions;

public static class ${g.cls}
{
    private static string GetSourcePath([CallerFilePath] string? path = null) => path!;

    public static FeatureGuideDefinition Create() => new()
    {
        SourceFile = GetSourcePath(),
        Sections = new()
        {${sections}
        }
    };
}
`;
}

let csCount = 0;
for (const g of guides) {
  const fp = path.join(guideDir, `${g.cls}.cs`);
  if (fs.existsSync(fp)) continue;
  fs.writeFileSync(fp, genCs(g), 'utf8');
  csCount++;
}
console.log(`Created ${csCount} Guide .cs files`);

// ==================== resx keys ====================
const zhTW = {
  // Field keys still missing
  "Field.VisitDate":"拜訪日期","Field.VisitType":"拜訪類型","Field.ItemCategory":"品項分類","Field.Unit":"單位","Field.Size":"尺寸",
  // Item
  "Guide.Item.Description":"品項管理用於維護所有商品、原料、半成品的基本資料，包含名稱、分類、單位、規格與供應商關聯。","Guide.Item.TabOverview":"本頁面包含多個分頁：「品項資料」填寫基本資訊；「會計設定」設定科目對應；「品項照片」上傳圖片。",
  "Guide.Item.Field.Code":"品項代碼，系統自動產生或手動輸入","Guide.Item.Field.Name":"品項名稱","Guide.Item.Field.Category":"品項分類，影響銷售與報表統計","Guide.Item.Field.Unit":"計量單位（如：個、箱、公斤）","Guide.Item.Field.Size":"品項規格尺寸","Guide.Item.Field.Supplier":"主要供應廠商","Guide.Item.Field.Spec":"品項規格描述","Guide.Item.Field.Remarks":"品項的補充說明",
  "Guide.Item.Action.PrintLabel":"列印","Guide.Item.Action.Print":"列印品項明細報表",
  "Guide.Item.Tip1":"可在品項中設定 BOM 組成表，定義產品的原料配方。","Guide.Item.Warning1":"刪除品項前請確認無相關訂單或庫存記錄。",
  "Guide.Item.Faq1Q":"如何設定品項的 BOM 組成？","Guide.Item.Faq1A":"在品項列表中透過右鍵選單或組成表功能進入 BOM 編輯。","Guide.Item.Faq2Q":"為什麼品項無法在銷售單中選用？","Guide.Item.Faq2A":"請檢查品項分類是否勾選「可銷售」選項。",
  // ItemComposition
  "Guide.ItemComposition.Description":"BOM 組成表定義產品的原料配方與用量，生產排程與領料作業會依據此配方計算所需材料。",
  "Guide.ItemComposition.Field.Code":"組成表代碼","Guide.ItemComposition.Field.Name":"組成表名稱","Guide.ItemComposition.Field.Spec":"配方規格說明","Guide.ItemComposition.Field.Remarks":"組成表的補充說明",
  "Guide.ItemComposition.Tip1":"一個品項可有多組配方，生產時可選擇適用的配方。",
  // Customer
  "Guide.Customer.Description":"客戶管理用於維護客戶的基本資料、聯絡方式、付款條件等，是銷售流程的核心資料。","Guide.Customer.TabOverview":"本頁面包含多個分頁：「客戶資料」填寫基本資訊；「車輛」管理關聯車輛；「銀行帳戶」維護帳戶；「拜訪」記錄拜訪；「投訴」處理客訴；「交易」查看歷史交易。",
  "Guide.Customer.Field.Code":"客戶代碼","Guide.Customer.Field.Name":"客戶名稱（公司名）","Guide.Customer.Field.Type":"客戶類型分類","Guide.Customer.Field.Contact":"主要聯絡人","Guide.Customer.Field.Phone":"聯絡電話","Guide.Customer.Field.Email":"電子郵件","Guide.Customer.Field.Address":"客戶地址","Guide.Customer.Field.Remarks":"客戶的補充說明",
  "Guide.Customer.Tip1":"可將客戶資料一鍵複製為廠商，節省重複輸入。","Guide.Customer.Warning1":"刪除客戶前請確認無相關訂單、出貨或應收記錄。",
  "Guide.Customer.Faq1Q":"如何將客戶同時設為廠商？","Guide.Customer.Faq1A":"使用「複製為廠商」功能，系統會自動建立對應的廠商資料。",
  // CustomerVisit
  "Guide.CustomerVisit.Description":"客戶拜訪記錄用於記錄業務員拜訪客戶的時間、目的與內容，方便追蹤客戶關係維護。",
  "Guide.CustomerVisit.Field.Customer":"被拜訪的客戶","Guide.CustomerVisit.Field.Date":"拜訪日期","Guide.CustomerVisit.Field.Type":"拜訪類型（如：例行、問題處理、業務開發）","Guide.CustomerVisit.Field.Employee":"負責拜訪的業務員","Guide.CustomerVisit.Field.Remarks":"拜訪的補充說明",
  // CustomerComplaint
  "Guide.CustomerComplaint.Description":"客戶投訴管理用於記錄並追蹤客戶的投訴案件，包含投訴內容、處理進度與解決方案。",
  "Guide.CustomerComplaint.Field.Customer":"投訴的客戶","Guide.CustomerComplaint.Field.Title":"投訴案件標題","Guide.CustomerComplaint.Field.Description":"投訴的詳細內容","Guide.CustomerComplaint.Field.Remarks":"投訴的補充說明",
  "Guide.CustomerComplaint.Tip1":"投訴狀態可追蹤處理進度，從「待處理」到「已結案」全程記錄。",
  // CustomerBankAccount
  "Guide.CustomerBankAccount.Description":"客戶銀行帳戶管理用於維護客戶的收付款帳戶資訊，沖款時可直接選用。",
  "Guide.CustomerBankAccount.Field.Bank":"往來銀行","Guide.CustomerBankAccount.Field.AccountNumber":"銀行帳號","Guide.CustomerBankAccount.Field.AccountName":"帳戶戶名","Guide.CustomerBankAccount.Field.Remarks":"帳戶的補充說明",
  // Supplier
  "Guide.Supplier.Description":"廠商管理用於維護供應商的基本資料、聯絡方式、付款條件等，是採購流程的核心資料。","Guide.Supplier.TabOverview":"本頁面包含多個分頁：「廠商資料」填寫基本資訊；「車輛」管理車輛；「品項」查看供應品項；「銀行帳戶」維護帳戶；「拜訪」記錄拜訪。",
  "Guide.Supplier.Field.Code":"廠商代碼","Guide.Supplier.Field.Name":"廠商名稱（公司名）","Guide.Supplier.Field.Contact":"主要聯絡人","Guide.Supplier.Field.Phone":"聯絡電話","Guide.Supplier.Field.Email":"電子郵件","Guide.Supplier.Field.Address":"廠商地址","Guide.Supplier.Field.Remarks":"廠商的補充說明",
  "Guide.Supplier.Tip1":"可將廠商資料一鍵複製為客戶，節省重複輸入。",
  "Guide.Supplier.Faq1Q":"如何查看廠商供應的品項？","Guide.Supplier.Faq1A":"在「品項」分頁中可查看此廠商供應的所有品項與報價。",
  // SupplierVisit
  "Guide.SupplierVisit.Description":"廠商拜訪記錄用於記錄拜訪廠商的時間、目的與內容。",
  "Guide.SupplierVisit.Field.Supplier":"被拜訪的廠商","Guide.SupplierVisit.Field.Date":"拜訪日期","Guide.SupplierVisit.Field.Type":"拜訪類型","Guide.SupplierVisit.Field.Employee":"負責拜訪的人員","Guide.SupplierVisit.Field.Remarks":"拜訪的補充說明",
  // SupplierBankAccount
  "Guide.SupplierBankAccount.Description":"廠商銀行帳戶管理用於維護廠商的付款帳戶資訊，沖款時可直接選用。",
  "Guide.SupplierBankAccount.Field.Bank":"往來銀行","Guide.SupplierBankAccount.Field.AccountNumber":"銀行帳號","Guide.SupplierBankAccount.Field.AccountName":"帳戶戶名","Guide.SupplierBankAccount.Field.Remarks":"帳戶的補充說明",
  // Employee
  "Guide.Employee.Description":"員工管理用於維護員工個人資料、部門職位、聯絡方式等，是人資管理的核心模組。","Guide.Employee.TabOverview":"本頁面包含多個分頁：「員工資料」填寫基本資訊；「車輛」管理配車；「個人工具」記錄配發工具；「訓練」記錄訓練；「權限」設定系統權限。",
  "Guide.Employee.Field.Code":"員工編號","Guide.Employee.Field.Name":"員工姓名","Guide.Employee.Field.Gender":"性別","Guide.Employee.Field.Birthday":"出生日期","Guide.Employee.Field.Phone":"手機號碼","Guide.Employee.Field.Department":"所屬部門","Guide.Employee.Field.Position":"職位","Guide.Employee.Field.HireDate":"到職日期","Guide.Employee.Field.Remarks":"員工的補充說明",
  "Guide.Employee.Tip1":"員工權限分頁可精細控制每位員工可存取的功能模組。","Guide.Employee.Warning1":"停用員工帳號後，該員工將無法登入系統。",
  // EmployeeLicense
  "Guide.EmployeeLicense.Description":"員工證照管理用於記錄員工持有的專業證照，可設定到期提醒。",
  "Guide.EmployeeLicense.Field.Name":"證照名稱","Guide.EmployeeLicense.Field.Number":"證照號碼","Guide.EmployeeLicense.Field.Authority":"發照機關","Guide.EmployeeLicense.Field.Expiry":"到期日期（可設定提醒天數）","Guide.EmployeeLicense.Field.Remarks":"證照的補充說明",
  // EmployeeTool
  "Guide.EmployeeTool.Description":"員工工具管理用於記錄配發給員工的個人工具或設備。",
  "Guide.EmployeeTool.Field.Name":"工具名稱","Guide.EmployeeTool.Field.Serial":"序號或型號","Guide.EmployeeTool.Field.AssignDate":"配發日期","Guide.EmployeeTool.Field.ReturnDate":"歸還日期（空白表示尚未歸還）",
  // EmployeeTrainingRecord
  "Guide.EmployeeTrainingRecord.Description":"員工訓練記錄用於記錄員工參加的訓練課程與成果。",
  "Guide.EmployeeTrainingRecord.Field.Course":"課程名稱","Guide.EmployeeTrainingRecord.Field.Date":"訓練日期","Guide.EmployeeTrainingRecord.Field.Hours":"訓練時數","Guide.EmployeeTrainingRecord.Field.Organization":"訓練機構",
  // Vehicle
  "Guide.Vehicle.Description":"車輛管理用於維護公司車輛的基本資料，包含車牌、類型、擁有者等，可追蹤保養記錄。",
  "Guide.Vehicle.Field.Code":"車輛代碼","Guide.Vehicle.Field.LicensePlate":"車牌號碼","Guide.Vehicle.Field.Type":"車輛類型","Guide.Vehicle.Field.Spec":"車輛規格描述","Guide.Vehicle.Field.Remarks":"車輛的補充說明",
  "Guide.Vehicle.Tip1":"車輛可關聯至員工、客戶或廠商，方便追蹤歸屬。",
  // VehicleMaintenance
  "Guide.VehicleMaintenance.Description":"車輛保養記錄用於追蹤車輛的維修與保養歷史，可設定下次保養提醒。",
  "Guide.VehicleMaintenance.Field.Vehicle":"保養的車輛","Guide.VehicleMaintenance.Field.Description":"保養內容描述","Guide.VehicleMaintenance.Field.Remarks":"保養的補充說明",
  // Equipment
  "Guide.Equipment.Description":"設備管理用於維護公司設備的基本資料，追蹤設備狀態、負責人與價值。",
  "Guide.Equipment.Field.Code":"設備代碼","Guide.Equipment.Field.Name":"設備名稱","Guide.Equipment.Field.Description":"設備描述","Guide.Equipment.Field.Remarks":"設備的補充說明",
  "Guide.Equipment.Tip1":"可透過保養記錄追蹤設備的維護歷史與下次保養時間。",
  // EquipmentMaintenance
  "Guide.EquipmentMaintenance.Description":"設備保養記錄用於追蹤設備的維修與保養歷史。",
  "Guide.EquipmentMaintenance.Field.Equipment":"保養的設備","Guide.EquipmentMaintenance.Field.Description":"保養內容描述","Guide.EquipmentMaintenance.Field.Remarks":"保養的補充說明",
  // CRM
  "Guide.CrmLead.Description":"潛在客戶管理用於追蹤商機開發進度，記錄聯絡資訊與預估商機金額，可轉為正式客戶。",
  "Guide.CrmLead.Field.CompanyName":"公司或個人名稱","Guide.CrmLead.Field.Contact":"聯絡人姓名","Guide.CrmLead.Field.Phone":"聯絡電話","Guide.CrmLead.Field.Email":"電子郵件","Guide.CrmLead.Field.Industry":"所屬產業","Guide.CrmLead.Field.Remarks":"潛在客戶的補充說明",
  "Guide.CrmLead.Action.ConvertLabel":"轉為客戶","Guide.CrmLead.Action.Convert":"將潛在客戶資料轉為正式客戶，系統會自動建立客戶資料",
  "Guide.CrmLead.Tip1":"在「跟進」分頁中可記錄每次聯繫的進度與下次跟進日期。","Guide.CrmLead.Warning1":"轉為客戶後潛在客戶記錄將標記為已轉換，不可復原。",
  // CrmLeadFollowUp
  "Guide.CrmLeadFollowUp.Description":"潛在客戶跟進記錄用於記錄每次聯繫的方式、內容與下次跟進計畫。",
  "Guide.CrmLeadFollowUp.Field.Type":"跟進方式（如：電話、拜訪、郵件）","Guide.CrmLeadFollowUp.Field.Notes":"跟進內容與結果","Guide.CrmLeadFollowUp.Field.Remarks":"跟進的補充說明",
  // ScaleRecord
  "Guide.ScaleRecord.Description":"過磅記錄用於記錄車輛進出場的秤重資料，自動計算淨重並可列印過磅單。",
  "Guide.ScaleRecord.Field.Code":"過磅單號","Guide.ScaleRecord.Field.Vehicle":"過磅車輛","Guide.ScaleRecord.Field.Customer":"關聯客戶","Guide.ScaleRecord.Field.Remarks":"過磅的補充說明",
  "Guide.ScaleRecord.Action.ReadLabel":"讀取磅秤","Guide.ScaleRecord.Action.Read":"從連接的磅秤設備讀取即時重量",
  "Guide.ScaleRecord.Tip1":"進場與出場重量可透過磅秤設備自動讀取，系統自動計算淨重。",
  // Document
  "Guide.Document.Description":"文件管理用於上傳、分類與管理公司文件，支援多檔案附件。",
  "Guide.Document.Field.Title":"文件標題","Guide.Document.Field.Content":"文件內容描述","Guide.Document.Field.Remarks":"文件的補充說明",
  "Guide.Document.Tip1":"可在文件中附加多個檔案，支援常見的文件格式。",
  // InventoryStock
  "Guide.InventoryStock.Description":"庫存管理用於查看與維護品項的庫存數量、倉庫位置與成本資訊。",
  "Guide.InventoryStock.Field.Item":"庫存品項","Guide.InventoryStock.Field.Warehouse":"存放倉庫","Guide.InventoryStock.Field.Remarks":"庫存的補充說明",
  "Guide.InventoryStock.Tip1":"庫存數量通常由進貨、出貨、退貨等單據自動更新，不建議手動修改。",
  // InventoryTransaction
  "Guide.InventoryTransaction.Description":"庫存異動記錄為唯讀頁面，顯示品項進出庫的所有歷史記錄，方便追蹤庫存變動原因。",
  // MaterialIssue
  "Guide.MaterialIssue.Description":"領料單用於記錄生產所需材料的領用，從倉庫扣減對應庫存。",
  "Guide.MaterialIssue.Field.Code":"領料單號","Guide.MaterialIssue.Field.Department":"領料部門","Guide.MaterialIssue.Field.Employee":"領料人員","Guide.MaterialIssue.Field.Remarks":"領料的補充說明",
  "Guide.MaterialIssue.Tip1":"領料單可關聯排程，系統會依 BOM 配方自動計算所需材料數量。",
  // StockTaking
  "Guide.StockTaking.Description":"盤點單用於記錄倉庫盤點作業，比對系統庫存與實際盤點數量，產生差異報告。",
  "Guide.StockTaking.Field.Code":"盤點單號","Guide.StockTaking.Field.Warehouse":"盤點倉庫","Guide.StockTaking.Field.Employee":"盤點人員","Guide.StockTaking.Field.Remarks":"盤點的補充說明",
  "Guide.StockTaking.Tip1":"盤點完成後可一鍵產生庫存調整，系統會自動修正差異數量。","Guide.StockTaking.Warning1":"確認盤點結果並調整庫存後，操作無法復原，請仔細核對。",
  // SetoffDocument
  "Guide.SetoffDocument.Description":"沖款單用於處理應收應付帳款的沖銷作業，包含收款、付款與退款沖帳。",
  "Guide.SetoffDocument.Field.Code":"沖款單號","Guide.SetoffDocument.Field.Type":"沖款類型（收款/付款/退款）","Guide.SetoffDocument.Field.Remarks":"沖款的補充說明",
  "Guide.SetoffDocument.Tip1":"沖款單通常從出貨單、退貨單或進貨單「轉沖款」按鈕建立。","Guide.SetoffDocument.Warning1":"沖款完成後會影響應收應付餘額，確認前請仔細核對金額。",
  // BankStatement
  "Guide.BankStatement.Description":"銀行對帳單管理用於匯入銀行交易明細，與系統帳務進行核對。",
  "Guide.BankStatement.Field.Company":"對帳公司","Guide.BankStatement.Field.BankAccount":"對帳銀行帳戶","Guide.BankStatement.Field.Remarks":"對帳的補充說明",
  "Guide.BankStatement.Tip1":"支援 CSV 格式匯入，匯入後可逐筆比對系統交易記錄。",
  // AccountItem
  "Guide.AccountItem.Description":"會計科目管理用於建立與維護會計科目表，支援多層級架構（資產、負債、收入、費用等）。",
  "Guide.AccountItem.Field.Code":"科目代碼","Guide.AccountItem.Field.Name":"科目名稱","Guide.AccountItem.Field.Type":"科目類型與方向","Guide.AccountItem.Field.Remarks":"科目的補充說明",
  "Guide.AccountItem.Tip1":"科目可建立上下層級關係，子科目會自動歸入上級科目彙總。",
  // FiscalPeriod
  "Guide.FiscalPeriod.Description":"會計期間管理用於定義與控制帳務期間的開關狀態（開啟/關閉/鎖定）。",
  "Guide.FiscalPeriod.Field.Name":"期間名稱","Guide.FiscalPeriod.Field.Remarks":"期間的補充說明",
  "Guide.FiscalPeriod.Warning1":"關閉或鎖定期間後，該期間內的傳票將無法新增或修改。",
  // JournalEntry
  "Guide.JournalEntry.Description":"傳票管理用於建立會計分錄，記錄借方與貸方的科目與金額，借貸必須平衡。",
  "Guide.JournalEntry.Field.Code":"傳票號碼","Guide.JournalEntry.Field.Description":"傳票摘要說明","Guide.JournalEntry.Field.Remarks":"傳票的補充說明",
  "Guide.JournalEntry.Tip1":"新增分錄行時，系統會即時檢查借貸是否平衡。","Guide.JournalEntry.Warning1":"傳票儲存後借貸必須平衡，不平衡的傳票無法儲存。",
  // ManufacturingOrder
  "Guide.ManufacturingOrder.Description":"製令管理用於追蹤生產排程的執行狀態，包含品項、數量、進度與相關領料記錄。",
  "Guide.ManufacturingOrder.Field.Code":"製令單號","Guide.ManufacturingOrder.Field.Item":"生產品項","Guide.ManufacturingOrder.Field.Remarks":"製令的補充說明",
};

// en-US
const enUS = {
  "Field.VisitDate":"Visit Date","Field.VisitType":"Visit Type","Field.ItemCategory":"Item Category","Field.Unit":"Unit","Field.Size":"Size",
  "Guide.Item.Description":"Item management maintains all products, raw materials, and semi-finished goods.","Guide.Item.TabOverview":"Multiple tabs: Item Data, Accounting, Photos.","Guide.Item.Field.Code":"Item code","Guide.Item.Field.Name":"Item name","Guide.Item.Field.Category":"Category for sales and reporting","Guide.Item.Field.Unit":"Measurement unit","Guide.Item.Field.Size":"Item size/spec","Guide.Item.Field.Supplier":"Primary supplier","Guide.Item.Field.Spec":"Specification description","Guide.Item.Field.Remarks":"Additional notes","Guide.Item.Action.PrintLabel":"Print","Guide.Item.Action.Print":"Print item detail report","Guide.Item.Tip1":"Set up BOM compositions to define product recipes.","Guide.Item.Warning1":"Ensure no related orders or stock before deleting.","Guide.Item.Faq1Q":"How to set up BOM?","Guide.Item.Faq1A":"Use the composition feature from item list right-click menu.","Guide.Item.Faq2Q":"Why can't item be selected in sales?","Guide.Item.Faq2A":"Check if the item category has 'Saleable' checked.",
  "Guide.ItemComposition.Description":"BOM compositions define product recipes and material quantities for production scheduling.","Guide.ItemComposition.Field.Code":"Composition code","Guide.ItemComposition.Field.Name":"Composition name","Guide.ItemComposition.Field.Spec":"Recipe specification","Guide.ItemComposition.Field.Remarks":"Additional notes","Guide.ItemComposition.Tip1":"One item can have multiple recipes; select the appropriate one during production.",
  "Guide.Customer.Description":"Customer management maintains customer info, contacts, and payment terms — core data for sales.","Guide.Customer.TabOverview":"Multiple tabs: Customer Data, Vehicles, Bank Accounts, Visits, Complaints, Transactions, Accounting.","Guide.Customer.Field.Code":"Customer code","Guide.Customer.Field.Name":"Customer/company name","Guide.Customer.Field.Type":"Customer type","Guide.Customer.Field.Contact":"Primary contact","Guide.Customer.Field.Phone":"Phone","Guide.Customer.Field.Email":"Email","Guide.Customer.Field.Address":"Address","Guide.Customer.Field.Remarks":"Additional notes","Guide.Customer.Tip1":"Copy customer data to create a supplier record in one click.","Guide.Customer.Warning1":"Ensure no related orders or receivables before deleting.","Guide.Customer.Faq1Q":"How to also set as supplier?","Guide.Customer.Faq1A":"Use 'Copy to Supplier' to auto-create supplier record.",
  "Guide.CustomerVisit.Description":"Customer visit records track salesperson visits for relationship management.","Guide.CustomerVisit.Field.Customer":"Visited customer","Guide.CustomerVisit.Field.Date":"Visit date","Guide.CustomerVisit.Field.Type":"Visit type","Guide.CustomerVisit.Field.Employee":"Visiting salesperson","Guide.CustomerVisit.Field.Remarks":"Additional notes",
  "Guide.CustomerComplaint.Description":"Customer complaint management tracks complaint cases with resolution progress.","Guide.CustomerComplaint.Field.Customer":"Complaining customer","Guide.CustomerComplaint.Field.Title":"Complaint title","Guide.CustomerComplaint.Field.Description":"Complaint details","Guide.CustomerComplaint.Field.Remarks":"Additional notes","Guide.CustomerComplaint.Tip1":"Track complaint status from 'Pending' to 'Resolved'.",
  "Guide.CustomerBankAccount.Description":"Customer bank account management for payment settlement.","Guide.CustomerBankAccount.Field.Bank":"Bank","Guide.CustomerBankAccount.Field.AccountNumber":"Account number","Guide.CustomerBankAccount.Field.AccountName":"Account holder name","Guide.CustomerBankAccount.Field.Remarks":"Additional notes",
  "Guide.Supplier.Description":"Supplier management maintains vendor info, contacts, and payment terms — core data for procurement.","Guide.Supplier.TabOverview":"Multiple tabs: Supplier Data, Vehicles, Items, Bank Accounts, Visits.","Guide.Supplier.Field.Code":"Supplier code","Guide.Supplier.Field.Name":"Supplier/company name","Guide.Supplier.Field.Contact":"Primary contact","Guide.Supplier.Field.Phone":"Phone","Guide.Supplier.Field.Email":"Email","Guide.Supplier.Field.Address":"Address","Guide.Supplier.Field.Remarks":"Additional notes","Guide.Supplier.Tip1":"Copy supplier data to create a customer record in one click.","Guide.Supplier.Faq1Q":"How to view supplier items?","Guide.Supplier.Faq1A":"Check the Items tab for all products supplied with pricing.",
  "Guide.SupplierVisit.Description":"Supplier visit records track vendor visits.","Guide.SupplierVisit.Field.Supplier":"Visited supplier","Guide.SupplierVisit.Field.Date":"Visit date","Guide.SupplierVisit.Field.Type":"Visit type","Guide.SupplierVisit.Field.Employee":"Visiting employee","Guide.SupplierVisit.Field.Remarks":"Additional notes",
  "Guide.SupplierBankAccount.Description":"Supplier bank account management for payment processing.","Guide.SupplierBankAccount.Field.Bank":"Bank","Guide.SupplierBankAccount.Field.AccountNumber":"Account number","Guide.SupplierBankAccount.Field.AccountName":"Account holder name","Guide.SupplierBankAccount.Field.Remarks":"Additional notes",
  "Guide.Employee.Description":"Employee management maintains personal info, department, position, and contacts.","Guide.Employee.TabOverview":"Multiple tabs: Employee Data, Vehicles, Tools, Training, Permissions, Payroll, Photos.","Guide.Employee.Field.Code":"Employee ID","Guide.Employee.Field.Name":"Full name","Guide.Employee.Field.Gender":"Gender","Guide.Employee.Field.Birthday":"Date of birth","Guide.Employee.Field.Phone":"Mobile number","Guide.Employee.Field.Department":"Department","Guide.Employee.Field.Position":"Job position","Guide.Employee.Field.HireDate":"Hire date","Guide.Employee.Field.Remarks":"Additional notes","Guide.Employee.Tip1":"The Permissions tab controls granular module access per employee.","Guide.Employee.Warning1":"Deactivating an employee account prevents system login.",
  "Guide.EmployeeLicense.Description":"Employee license management tracks professional certifications with expiry alerts.","Guide.EmployeeLicense.Field.Name":"License name","Guide.EmployeeLicense.Field.Number":"License number","Guide.EmployeeLicense.Field.Authority":"Issuing authority","Guide.EmployeeLicense.Field.Expiry":"Expiry date (with alert days)","Guide.EmployeeLicense.Field.Remarks":"Additional notes",
  "Guide.EmployeeTool.Description":"Employee tool management tracks tools and equipment assigned to employees.","Guide.EmployeeTool.Field.Name":"Tool name","Guide.EmployeeTool.Field.Serial":"Serial number or model","Guide.EmployeeTool.Field.AssignDate":"Assignment date","Guide.EmployeeTool.Field.ReturnDate":"Return date (blank if not returned)",
  "Guide.EmployeeTrainingRecord.Description":"Employee training records track courses and results.","Guide.EmployeeTrainingRecord.Field.Course":"Course name","Guide.EmployeeTrainingRecord.Field.Date":"Training date","Guide.EmployeeTrainingRecord.Field.Hours":"Training hours","Guide.EmployeeTrainingRecord.Field.Organization":"Training organization",
  "Guide.Vehicle.Description":"Vehicle management maintains fleet info including plates, types, and ownership.","Guide.Vehicle.Field.Code":"Vehicle code","Guide.Vehicle.Field.LicensePlate":"License plate","Guide.Vehicle.Field.Type":"Vehicle type","Guide.Vehicle.Field.Spec":"Vehicle specifications","Guide.Vehicle.Field.Remarks":"Additional notes","Guide.Vehicle.Tip1":"Vehicles can be linked to employees, customers, or suppliers.",
  "Guide.VehicleMaintenance.Description":"Vehicle maintenance records track repair and service history.","Guide.VehicleMaintenance.Field.Vehicle":"Vehicle being serviced","Guide.VehicleMaintenance.Field.Description":"Maintenance description","Guide.VehicleMaintenance.Field.Remarks":"Additional notes",
  "Guide.Equipment.Description":"Equipment management maintains company equipment info, status, and value tracking.","Guide.Equipment.Field.Code":"Equipment code","Guide.Equipment.Field.Name":"Equipment name","Guide.Equipment.Field.Description":"Description","Guide.Equipment.Field.Remarks":"Additional notes","Guide.Equipment.Tip1":"Track maintenance history and next service dates via maintenance records.",
  "Guide.EquipmentMaintenance.Description":"Equipment maintenance records track repair and service history.","Guide.EquipmentMaintenance.Field.Equipment":"Equipment being serviced","Guide.EquipmentMaintenance.Field.Description":"Maintenance description","Guide.EquipmentMaintenance.Field.Remarks":"Additional notes",
  "Guide.CrmLead.Description":"Lead management tracks business opportunities with contact info and estimated values; can convert to customers.","Guide.CrmLead.Field.CompanyName":"Company or person name","Guide.CrmLead.Field.Contact":"Contact name","Guide.CrmLead.Field.Phone":"Phone","Guide.CrmLead.Field.Email":"Email","Guide.CrmLead.Field.Industry":"Industry","Guide.CrmLead.Field.Remarks":"Additional notes","Guide.CrmLead.Action.ConvertLabel":"Convert to Customer","Guide.CrmLead.Action.Convert":"Convert lead to a formal customer record","Guide.CrmLead.Tip1":"Use the Follow-up tab to record each contact and plan next steps.","Guide.CrmLead.Warning1":"Converting to customer is irreversible; the lead is marked as converted.",
  "Guide.CrmLeadFollowUp.Description":"Lead follow-up records track each contact method, content, and next steps.","Guide.CrmLeadFollowUp.Field.Type":"Follow-up method (call, visit, email)","Guide.CrmLeadFollowUp.Field.Notes":"Follow-up content and results","Guide.CrmLeadFollowUp.Field.Remarks":"Additional notes",
  "Guide.ScaleRecord.Description":"Scale records capture vehicle weighing data with auto net-weight calculation and printing.","Guide.ScaleRecord.Field.Code":"Record number","Guide.ScaleRecord.Field.Vehicle":"Weighed vehicle","Guide.ScaleRecord.Field.Customer":"Related customer","Guide.ScaleRecord.Field.Remarks":"Additional notes","Guide.ScaleRecord.Action.ReadLabel":"Read Scale","Guide.ScaleRecord.Action.Read":"Read real-time weight from connected scale device","Guide.ScaleRecord.Tip1":"Entry and exit weights can be auto-read from scale devices.",
  "Guide.Document.Description":"Document management for uploading, categorizing, and managing company documents.","Guide.Document.Field.Title":"Document title","Guide.Document.Field.Content":"Content description","Guide.Document.Field.Remarks":"Additional notes","Guide.Document.Tip1":"Multiple file attachments supported in common formats.",
  "Guide.InventoryStock.Description":"Inventory management for viewing and maintaining stock quantities, locations, and costs.","Guide.InventoryStock.Field.Item":"Stock item","Guide.InventoryStock.Field.Warehouse":"Storage warehouse","Guide.InventoryStock.Field.Remarks":"Additional notes","Guide.InventoryStock.Tip1":"Stock quantities are usually auto-updated by transactions; manual edits not recommended.",
  "Guide.InventoryTransaction.Description":"Inventory transaction log is read-only, showing all stock movement history for tracking.",
  "Guide.MaterialIssue.Description":"Material issue records track materials withdrawn from warehouse for production.","Guide.MaterialIssue.Field.Code":"Issue number","Guide.MaterialIssue.Field.Department":"Issuing department","Guide.MaterialIssue.Field.Employee":"Issued to","Guide.MaterialIssue.Field.Remarks":"Additional notes","Guide.MaterialIssue.Tip1":"Can link to production schedules; system auto-calculates required materials from BOM.",
  "Guide.StockTaking.Description":"Stock taking records physical inventory counts, compares with system stock, and generates difference reports.","Guide.StockTaking.Field.Code":"Count number","Guide.StockTaking.Field.Warehouse":"Count warehouse","Guide.StockTaking.Field.Employee":"Counter","Guide.StockTaking.Field.Remarks":"Additional notes","Guide.StockTaking.Tip1":"After counting, one-click adjustment auto-corrects stock differences.","Guide.StockTaking.Warning1":"Stock adjustments are irreversible; verify carefully before confirming.",
  "Guide.SetoffDocument.Description":"Setoff documents handle AR/AP settlement including collections, payments, and refunds.","Guide.SetoffDocument.Field.Code":"Setoff number","Guide.SetoffDocument.Field.Type":"Setoff type (collection/payment/refund)","Guide.SetoffDocument.Field.Remarks":"Additional notes","Guide.SetoffDocument.Tip1":"Usually created via 'Convert to Setoff' from delivery, return, or receiving documents.","Guide.SetoffDocument.Warning1":"Completed setoffs affect AR/AP balances; verify amounts carefully.",
  "Guide.BankStatement.Description":"Bank statement management imports bank transaction details for reconciliation.","Guide.BankStatement.Field.Company":"Reconciliation company","Guide.BankStatement.Field.BankAccount":"Bank account","Guide.BankStatement.Field.Remarks":"Additional notes","Guide.BankStatement.Tip1":"Supports CSV import; imported transactions can be matched one by one.",
  "Guide.AccountItem.Description":"Chart of accounts management with multi-level hierarchy (assets, liabilities, income, expenses).","Guide.AccountItem.Field.Code":"Account code","Guide.AccountItem.Field.Name":"Account name","Guide.AccountItem.Field.Type":"Account type and direction","Guide.AccountItem.Field.Remarks":"Additional notes","Guide.AccountItem.Tip1":"Sub-accounts auto-roll up to parent accounts in summaries.",
  "Guide.FiscalPeriod.Description":"Fiscal period management controls accounting period status (open/closed/locked).","Guide.FiscalPeriod.Field.Name":"Period name","Guide.FiscalPeriod.Field.Remarks":"Additional notes","Guide.FiscalPeriod.Warning1":"Closing or locking a period prevents adding or editing journal entries within it.",
  "Guide.JournalEntry.Description":"Journal entry management creates accounting entries with balanced debit and credit lines.","Guide.JournalEntry.Field.Code":"Entry number","Guide.JournalEntry.Field.Description":"Entry description/memo","Guide.JournalEntry.Field.Remarks":"Additional notes","Guide.JournalEntry.Tip1":"System checks debit-credit balance in real-time as lines are added.","Guide.JournalEntry.Warning1":"Entries must be balanced; unbalanced entries cannot be saved.",
  "Guide.ManufacturingOrder.Description":"Manufacturing order management tracks production schedule execution, quantities, and related material issues.","Guide.ManufacturingOrder.Field.Code":"Order number","Guide.ManufacturingOrder.Field.Item":"Product item","Guide.ManufacturingOrder.Field.Remarks":"Additional notes",
};

function escXml(s) { return s.replace(/&/g,'&amp;').replace(/</g,'&lt;').replace(/>/g,'&gt;'); }
function insertKeys(file, keys) {
  let content = fs.readFileSync(file, 'utf8');
  const marker = 'name="Modal.DefaultTitle"';
  const idx = content.indexOf(marker);
  if (idx === -1) { console.log("ERROR: marker not found in " + path.basename(file)); return; }
  const lineStart = content.lastIndexOf('\n', idx) + 1;
  const lines = [];
  for (const [key, val] of Object.entries(keys)) {
    if (content.includes(`name="${key}"`)) continue;
    lines.push(`  <data name="${key}" xml:space="preserve"><value>${escXml(val)}</value></data>`);
  }
  if (lines.length === 0) { console.log("No new keys for " + path.basename(file)); return; }
  content = content.slice(0, lineStart) + lines.join('\n') + '\n' + content.slice(lineStart);
  fs.writeFileSync(file, content, 'utf8');
  console.log(`Added ${lines.length} keys to ${path.basename(file)}`);
}

insertKeys(path.join(resxDir, "SharedResource.resx"), zhTW);
insertKeys(path.join(resxDir, "SharedResource.en-US.resx"), enUS);
insertKeys(path.join(resxDir, "SharedResource.ja-JP.resx"), enUS);
insertKeys(path.join(resxDir, "SharedResource.zh-CN.resx"), enUS);
insertKeys(path.join(resxDir, "SharedResource.fil.resx"), enUS);

// ==================== Wire up EditModals ====================
const usingLine = '@using ERPCore2.Models.FeatureGuides.GuideDefinitions';
let wireCount = 0;
for (const g of guides) {
  const fp = path.join(pagesDir, g.wire);
  if (!fs.existsSync(fp)) { console.log(`SKIP (not found): ${g.wire}`); continue; }
  let content = fs.readFileSync(fp, 'utf8');
  if (content.includes('FeatureGuide=')) { console.log(`SKIP (already): ${g.wire}`); continue; }

  // Add @using
  if (!content.includes(usingLine)) {
    const lines = content.split('\n');
    let insertIdx = 0;
    for (let i = 0; i < lines.length; i++) {
      const t = lines[i].trim();
      if (t.startsWith('@using') || t.startsWith('@inject') || t.startsWith('@implements')) insertIdx = i + 1;
    }
    lines.splice(insertIdx, 0, usingLine);
    content = lines.join('\n');
  }

  // Add FeatureGuide parameter
  const regex = /(ComponentName="[^"]*EditModalComponent")(\s*)(\/?>)/;
  if (regex.test(content)) {
    content = content.replace(regex, `$1\n                          FeatureGuide="@${g.cls}.Create()"$2$3`);
  } else {
    console.log(`WARN: No ComponentName pattern in ${g.wire}`);
    continue;
  }

  fs.writeFileSync(fp, content, 'utf8');
  wireCount++;
  console.log(`Wired: ${g.wire}`);
}
console.log(`\nTotal wired: ${wireCount}`);
