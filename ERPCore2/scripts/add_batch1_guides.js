const fs = require('fs');
const path = require('path');

const guideDir = path.join(__dirname, '..', 'Models', 'FeatureGuides', 'GuideDefinitions');
const resxDir = path.join(__dirname, '..', 'Resources');

// ==================== STEP 1: Generate Guide .cs files ====================

const guides = [
  // Items
  { cls: "UnitGuide", entity: "Unit", prefix: "unit", desc: "Guide.Unit.Description", fields: [
    ["Field.Code","Guide.Unit.Field.Code"],["Field.Name","Guide.Unit.Field.Name"],["Field.EnglishName","Guide.Unit.Field.EnglishName"],["Field.Remarks","Guide.Unit.Field.Remarks"]
  ]},
  { cls: "SizeGuide", entity: "Size", prefix: "size", desc: "Guide.Size.Description", fields: [
    ["Field.Code","Guide.Size.Field.Code"],["Field.Name","Guide.Size.Field.Name"],["Field.Remarks","Guide.Size.Field.Remarks"]
  ]},
  { cls: "ItemCategoryGuide", entity: "ItemCategory", prefix: "ic", desc: "Guide.ItemCategory.Description", fields: [
    ["Field.Code","Guide.ItemCategory.Field.Code"],["Field.Name","Guide.ItemCategory.Field.Name"],["Field.IsSaleable","Guide.ItemCategory.Field.IsSaleable"],["Field.Remarks","Guide.ItemCategory.Field.Remarks"]
  ]},
  { cls: "CompositionCategoryGuide", entity: "CompositionCategory", prefix: "cc", desc: "Guide.CompositionCategory.Description", fields: [
    ["Field.Code","Guide.CompositionCategory.Field.Code"],["Field.Name","Guide.CompositionCategory.Field.Name"],["Field.Remarks","Guide.CompositionCategory.Field.Remarks"]
  ]},
  // Sales/Purchase reasons
  { cls: "SalesReturnReasonGuide", entity: "SalesReturnReason", prefix: "srr", desc: "Guide.SalesReturnReason.Description", fields: [
    ["Field.Code","Guide.SalesReturnReason.Field.Code"],["Field.Name","Guide.SalesReturnReason.Field.Name"],["Field.Remarks","Guide.SalesReturnReason.Field.Remarks"]
  ]},
  { cls: "SalesTargetGuide", entity: "SalesTarget", prefix: "st", desc: "Guide.SalesTarget.Description", fields: [
    ["Field.Year","Guide.SalesTarget.Field.Year"],["Field.Month","Guide.SalesTarget.Field.Month"],["Field.SalesPerson","Guide.SalesTarget.Field.Salesperson"],["Field.TargetAmount","Guide.SalesTarget.Field.Amount"],["Field.Remarks","Guide.SalesTarget.Field.Remarks"]
  ]},
  { cls: "PurchaseReturnReasonGuide", entity: "PurchaseReturnReason", prefix: "prr", desc: "Guide.PurchaseReturnReason.Description", fields: [
    ["Field.Code","Guide.PurchaseReturnReason.Field.Code"],["Field.Name","Guide.PurchaseReturnReason.Field.Name"],["Field.Remarks","Guide.PurchaseReturnReason.Field.Remarks"]
  ]},
  // Employees
  { cls: "DepartmentGuide", entity: "Department", prefix: "dept", desc: "Guide.Department.Description", fields: [
    ["Field.Code","Guide.Department.Field.Code"],["Field.Name","Guide.Department.Field.Name"],["Field.ManagerId","Guide.Department.Field.Manager"],["Field.DeputyManagerId","Guide.Department.Field.Deputy"],["Field.ParentDepartment","Guide.Department.Field.Parent"],["Field.Phone","Guide.Department.Field.Phone"],["Field.Location","Guide.Department.Field.Location"],["Field.Remarks","Guide.Department.Field.Remarks"]
  ]},
  { cls: "EmployeePositionGuide", entity: "EmployeePosition", prefix: "epos", desc: "Guide.EmployeePosition.Description", fields: [
    ["Field.Code","Guide.EmployeePosition.Field.Code"],["Field.Name","Guide.EmployeePosition.Field.Name"],["Field.Remarks","Guide.EmployeePosition.Field.Remarks"]
  ]},
  { cls: "RoleGuide", entity: "Role", prefix: "role", desc: "Guide.Role.Description", fields: [
    ["Field.Code","Guide.Role.Field.Code"],["Field.Name","Guide.Role.Field.Name"],["Field.Remarks","Guide.Role.Field.Remarks"]
  ]},
  { cls: "PermissionGuide", entity: "Permission", prefix: "perm", desc: "Guide.Permission.Description", fields: [
    ["Field.Code","Guide.Permission.Field.Code"],["Field.Name","Guide.Permission.Field.Name"],["Field.Level","Guide.Permission.Field.Level"],["Field.Remarks","Guide.Permission.Field.Remarks"]
  ]},
  // Vehicles / Scale
  { cls: "VehicleTypeGuide", entity: "VehicleType", prefix: "vt", desc: "Guide.VehicleType.Description", fields: [
    ["Field.Code","Guide.VehicleType.Field.Code"],["Field.Name","Guide.VehicleType.Field.Name"],["Field.Description","Guide.VehicleType.Field.Description"],["Field.Remarks","Guide.VehicleType.Field.Remarks"]
  ]},
  { cls: "ScaleTypeGuide", entity: "ScaleType", prefix: "sct", desc: "Guide.ScaleType.Description", fields: [
    ["Field.Code","Guide.ScaleType.Field.Code"],["Field.Name","Guide.ScaleType.Field.Name"],["Field.ItemId","Guide.ScaleType.Field.Item"],["Field.Description","Guide.ScaleType.Field.Description"],["Field.Remarks","Guide.ScaleType.Field.Remarks"]
  ]},
  // Financial
  { cls: "BankGuide", entity: "Bank", prefix: "bank", desc: "Guide.Bank.Description", fields: [
    ["Field.Code","Guide.Bank.Field.Code"],["Field.BankName","Guide.Bank.Field.BankName"],["Field.BankNameEn","Guide.Bank.Field.BankNameEn"],["Field.SwiftCode","Guide.Bank.Field.SwiftCode"],["Field.Remarks","Guide.Bank.Field.Remarks"]
  ]},
  { cls: "CurrencyGuide", entity: "Currency", prefix: "cur", desc: "Guide.Currency.Description", fields: [
    ["Field.Code","Guide.Currency.Field.Code"],["Field.Name","Guide.Currency.Field.Name"],["Field.Symbol","Guide.Currency.Field.Symbol"],["Field.IsBaseCurrency","Guide.Currency.Field.IsBase"],["Field.ExchangeRate","Guide.Currency.Field.Rate"],["Field.Remarks","Guide.Currency.Field.Remarks"]
  ]},
  { cls: "PaymentMethodGuide", entity: "PaymentMethod", prefix: "pm", desc: "Guide.PaymentMethod.Description", fields: [
    ["Field.Code","Guide.PaymentMethod.Field.Code"],["Field.Name","Guide.PaymentMethod.Field.Name"],["Field.Remarks","Guide.PaymentMethod.Field.Remarks"]
  ]},
  // Documents / Equipment
  { cls: "DocumentCategoryGuide", entity: "DocumentCategory", prefix: "dc", desc: "Guide.DocumentCategory.Description", fields: [
    ["Field.Code","Guide.DocumentCategory.Field.Code"],["Field.Name","Guide.DocumentCategory.Field.Name"],["Field.DefaultAccessLevel","Guide.DocumentCategory.Field.AccessLevel"],["Field.Remarks","Guide.DocumentCategory.Field.Remarks"]
  ]},
  { cls: "EquipmentCategoryGuide", entity: "EquipmentCategory", prefix: "ec", desc: "Guide.EquipmentCategory.Description", fields: [
    ["Field.Code","Guide.EquipmentCategory.Field.Code"],["Field.Name","Guide.EquipmentCategory.Field.Name"],["Field.Description","Guide.EquipmentCategory.Field.Description"],["Field.Remarks","Guide.EquipmentCategory.Field.Remarks"]
  ]},
  // Warehouse
  { cls: "WarehouseGuide", entity: "Warehouse", prefix: "wh", desc: "Guide.Warehouse.Description", fields: [
    ["Field.Code","Guide.Warehouse.Field.Code"],["Field.Name","Guide.Warehouse.Field.Name"],["Field.ContactPerson","Guide.Warehouse.Field.Contact"],["Field.Phone","Guide.Warehouse.Field.Phone"],["Field.Address","Guide.Warehouse.Field.Address"],["Field.Remarks","Guide.Warehouse.Field.Remarks"]
  ], tip: "Guide.Warehouse.Tip1"},
  { cls: "WarehouseLocationGuide", entity: "WarehouseLocation", prefix: "wl", desc: "Guide.WarehouseLocation.Description", fields: [
    ["Field.Code","Guide.WarehouseLocation.Field.Code"],["Field.Name","Guide.WarehouseLocation.Field.Name"],["Field.Warehouse","Guide.WarehouseLocation.Field.Warehouse"],["Field.Zone","Guide.WarehouseLocation.Field.Zone"],["Field.Aisle","Guide.WarehouseLocation.Field.Aisle"],["Field.Level","Guide.WarehouseLocation.Field.Level"],["Field.Position","Guide.WarehouseLocation.Field.Position"],["Field.MaxCapacity","Guide.WarehouseLocation.Field.MaxCapacity"],["Field.Remarks","Guide.WarehouseLocation.Field.Remarks"]
  ]},
  // Systems
  { cls: "CompanyGuide", entity: "Company", prefix: "comp", desc: "Guide.Company.Description", tabOverview: "Guide.Company.TabOverview", fields: [
    ["Field.Code","Guide.Company.Field.Code"],["Field.Name","Guide.Company.Field.Name"],["Field.ShortName","Guide.Company.Field.ShortName"],["Field.TaxId","Guide.Company.Field.TaxId"],["Field.Representative","Guide.Company.Field.Representative"],["Field.Address","Guide.Company.Field.Address"],["Field.Phone","Guide.Company.Field.Phone"],["Field.Email","Guide.Company.Field.Email"],["Field.Website","Guide.Company.Field.Website"],["Field.IsDefault","Guide.Company.Field.IsDefault"]
  ], tip: "Guide.Company.Tip1"},
  { cls: "PaperSettingGuide", entity: "PaperSetting", prefix: "ps", desc: "Guide.PaperSetting.Description", fields: [
    ["Field.Code","Guide.PaperSetting.Field.Code"],["Field.Name","Guide.PaperSetting.Field.Name"],["Field.Width","Guide.PaperSetting.Field.Width"],["Field.Height","Guide.PaperSetting.Field.Height"],["Field.Orientation","Guide.PaperSetting.Field.Orientation"],["Field.Remarks","Guide.PaperSetting.Field.Remarks"]
  ], tip: "Guide.PaperSetting.Tip1"},
  { cls: "ReportPrintConfigurationGuide", entity: "ReportPrintConfiguration", prefix: "rpc", desc: "Guide.ReportPrintConfiguration.Description", fields: [
    ["Field.Code","Guide.ReportPrintConfiguration.Field.Code"],["Field.Name","Guide.ReportPrintConfiguration.Field.ReportName"],["Field.PaperSetting","Guide.ReportPrintConfiguration.Field.PaperSetting"],["Field.Remarks","Guide.ReportPrintConfiguration.Field.Remarks"]
  ]},
  { cls: "TextMessageTemplateGuide", entity: "TextMessageTemplate", prefix: "tmt", desc: "Guide.TextMessageTemplate.Description", fields: []
  , customSteps: ["Guide.TextMessageTemplate.Step1","Guide.TextMessageTemplate.Step2","Guide.TextMessageTemplate.Step3"]},
  // Payroll settings
  { cls: "PayrollItemGuide", entity: "PayrollItem", prefix: "pi", desc: "Guide.PayrollItem.Description", fields: [
    ["Field.Code","Guide.PayrollItem.Field.Code"],["Field.Name","Guide.PayrollItem.Field.Name"],["Field.Description","Guide.PayrollItem.Field.Description"],["Field.Remarks","Guide.PayrollItem.Field.Remarks"]
  ]},
  { cls: "PayrollPeriodGuide", entity: "PayrollPeriod", prefix: "pp", desc: "Guide.PayrollPeriod.Description", fields: [
    ["Field.Name","Guide.PayrollPeriod.Field.Name"],["Field.Year","Guide.PayrollPeriod.Field.Year"],["Field.Month","Guide.PayrollPeriod.Field.Month"],["Field.Remarks","Guide.PayrollPeriod.Field.Remarks"]
  ]},
  { cls: "InsuranceRateGuide", entity: "InsuranceRate", prefix: "ir", desc: "Guide.InsuranceRate.Description", fields: []},
  { cls: "HealthInsuranceGradeGuide", entity: "HealthInsuranceGrade", prefix: "hig", desc: "Guide.HealthInsuranceGrade.Description", fields: []},
  { cls: "LaborInsuranceGradeGuide", entity: "LaborInsuranceGrade", prefix: "lig", desc: "Guide.LaborInsuranceGrade.Description", fields: []},
  { cls: "MinimumWageGuide", entity: "MinimumWage", prefix: "mw", desc: "Guide.MinimumWage.Description", fields: []},
  { cls: "WithholdingTaxTableGuide", entity: "WithholdingTaxTable", prefix: "wtt", desc: "Guide.WithholdingTaxTable.Description", fields: []},
  { cls: "EmployeeSalaryGuide", entity: "EmployeeSalary", prefix: "es", desc: "Guide.EmployeeSalary.Description", fields: []},
  { cls: "EmployeeBankAccountGuide", entity: "EmployeeBankAccount", prefix: "eba", desc: "Guide.EmployeeBankAccount.Description", fields: []},
  { cls: "MonthlyAttendanceSummaryGuide", entity: "MonthlyAttendanceSummary", prefix: "mas", desc: "Guide.MonthlyAttendanceSummary.Description", fields: []},
];

function genCsFile(g) {
  const fieldItems = g.fields.map(f => `                    new("${f[0]}", "${f[1]}"),`).join('\n');
  const hasFields = g.fields.length > 0;
  const hasSteps = g.customSteps && g.customSteps.length > 0;
  const hasTip = !!g.tip;
  const hasTabOverview = !!g.tabOverview;

  let sections = '';

  // Overview
  sections += `
            new GuideSection
            {
                Id = "guide-${g.prefix}-overview",
                TitleKey = "Guide.Overview",
                Icon = "bi-info-circle",
                BookmarkLabel = "概述",
                BookmarkColor = "#3B82F6",
                Type = GuideSectionType.Description,
                Items =
                {
                    new("${g.desc}"),${hasTabOverview ? `\n                    new("${g.tabOverview}"),` : ''}
                }
            },`;

  // Steps (only for complex ones)
  if (hasSteps) {
    const stepItems = g.customSteps.map(s => `                    new("${s}"),`).join('\n');
    sections += `

            new GuideSection
            {
                Id = "guide-${g.prefix}-steps",
                TitleKey = "Guide.Steps",
                Icon = "bi-list-ol",
                BookmarkLabel = "步驟",
                BookmarkColor = "#10B981",
                Type = GuideSectionType.Steps,
                Items =
                {
${stepItems}
                }
            },`;
  }

  // Fields
  if (hasFields) {
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
${fieldItems}
                }
            },`;
  }

  // Tips
  if (hasTip) {
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
                    new("${g.tip}", GuideItemStyle.Tip),
                }
            },`;
  }

  return `using System.Runtime.CompilerServices;

namespace ERPCore2.Models.FeatureGuides.GuideDefinitions;

/// <summary>
/// ${g.entity} 功能說明定義
/// </summary>
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

// Write all .cs files
let csCount = 0;
for (const g of guides) {
  const filePath = path.join(guideDir, `${g.cls}.cs`);
  if (fs.existsSync(filePath)) { continue; }
  fs.writeFileSync(filePath, genCsFile(g), 'utf8');
  csCount++;
}
console.log(`Created ${csCount} Guide .cs files`);

// ==================== STEP 2: Generate resx keys ====================

const zhTW = {
  // Missing Field.* keys
  "Field.IsSaleable":"可銷售","Field.ManagerId":"主管","Field.DeputyManagerId":"副主管","Field.BankNameEn":"銀行英文名稱","Field.Symbol":"幣別符號","Field.Aisle":"走道","Field.Width":"寬度(cm)","Field.Height":"高度(cm)","Field.Orientation":"方向","Field.TopMargin":"上邊距","Field.BottomMargin":"下邊距","Field.LeftMargin":"左邊距","Field.RightMargin":"右邊距","Field.TaxId":"統一編號","Field.Representative":"負責人","Field.IsDefault":"設為預設","Field.TargetAmount":"目標金額","Field.ItemId":"關聯品項","Entity.ScaleType":"磅秤類型","Field.Warehouse":"所屬倉庫","Field.PaperSetting":"紙張設定",

  // Guide.Unit
  "Guide.Unit.Description":"單位管理用於定義品項的計量單位（如：個、箱、公斤等），所有品項的數量計算都依據此設定。",
  "Guide.Unit.Field.Code":"單位代碼，例如 PCS、BOX、KG","Guide.Unit.Field.Name":"單位的中文名稱","Guide.Unit.Field.EnglishName":"單位的英文名稱，用於匯出英文文件","Guide.Unit.Field.Remarks":"單位的補充說明",

  // Guide.Size
  "Guide.Size.Description":"尺寸管理用於定義品項的規格尺寸（如：S、M、L、XL 等），可在品項設定中關聯使用。",
  "Guide.Size.Field.Code":"尺寸代碼","Guide.Size.Field.Name":"尺寸名稱","Guide.Size.Field.Remarks":"尺寸的補充說明",

  // Guide.ItemCategory
  "Guide.ItemCategory.Description":"品項分類用於將品項依類別歸類管理（如：原料、成品、半成品等），方便查詢與報表統計。",
  "Guide.ItemCategory.Field.Code":"分類代碼","Guide.ItemCategory.Field.Name":"分類名稱","Guide.ItemCategory.Field.IsSaleable":"勾選後此分類的品項可出現在銷售單據中","Guide.ItemCategory.Field.Remarks":"分類的補充說明",

  // Guide.CompositionCategory
  "Guide.CompositionCategory.Description":"組成分類用於將 BOM（物料清單）配方依類別管理，方便組織不同產品線的配方。",
  "Guide.CompositionCategory.Field.Code":"分類代碼","Guide.CompositionCategory.Field.Name":"分類名稱","Guide.CompositionCategory.Field.Remarks":"分類的補充說明",

  // Guide.SalesReturnReason
  "Guide.SalesReturnReason.Description":"銷貨退回原因管理用於預先定義退貨原因選項（如：瑕疵品、數量不符等），建立退貨單時可直接選用。",
  "Guide.SalesReturnReason.Field.Code":"系統自動產生的代碼","Guide.SalesReturnReason.Field.Name":"退回原因名稱","Guide.SalesReturnReason.Field.Remarks":"原因的補充說明",

  // Guide.SalesTarget
  "Guide.SalesTarget.Description":"業績目標管理用於設定各業務員的月度銷售目標金額，搭配儀表板追蹤達成率。",
  "Guide.SalesTarget.Field.Year":"目標年度","Guide.SalesTarget.Field.Month":"目標月份","Guide.SalesTarget.Field.Salesperson":"設定目標的業務員","Guide.SalesTarget.Field.Amount":"當月銷售目標金額","Guide.SalesTarget.Field.Remarks":"目標的補充說明",

  // Guide.PurchaseReturnReason
  "Guide.PurchaseReturnReason.Description":"採購退回原因管理用於預先定義退貨原因選項（如：品質不良、規格不符等），建立退貨單時可直接選用。",
  "Guide.PurchaseReturnReason.Field.Code":"系統自動產生的代碼","Guide.PurchaseReturnReason.Field.Name":"退回原因名稱","Guide.PurchaseReturnReason.Field.Remarks":"原因的補充說明",

  // Guide.Department
  "Guide.Department.Description":"部門管理用於建立公司的組織架構，可設定上下層級關係、主管與副主管，員工需歸屬至部門。",
  "Guide.Department.Field.Code":"部門代碼","Guide.Department.Field.Name":"部門名稱","Guide.Department.Field.Manager":"指定部門主管","Guide.Department.Field.Deputy":"指定部門副主管","Guide.Department.Field.Parent":"上級部門，用於建立組織層級","Guide.Department.Field.Phone":"部門聯絡電話","Guide.Department.Field.Location":"部門所在位置","Guide.Department.Field.Remarks":"部門的補充說明",

  // Guide.EmployeePosition
  "Guide.EmployeePosition.Description":"職位管理用於定義公司的職位名稱（如：經理、主任、工程師等），員工資料中可選擇職位。",
  "Guide.EmployeePosition.Field.Code":"職位代碼","Guide.EmployeePosition.Field.Name":"職位名稱","Guide.EmployeePosition.Field.Remarks":"職位的補充說明",

  // Guide.Role
  "Guide.Role.Description":"角色管理用於定義系統角色（如：管理員、一般使用者等），每個角色可配置不同的權限組合。",
  "Guide.Role.Field.Code":"角色代碼，用於系統識別","Guide.Role.Field.Name":"角色的顯示名稱","Guide.Role.Field.Remarks":"角色的補充說明",

  // Guide.Permission
  "Guide.Permission.Description":"權限檢視頁面，顯示系統中所有已定義的權限項目。權限由系統自動產生，此頁面僅供檢視，不可編輯。",
  "Guide.Permission.Field.Code":"權限識別碼，由系統自動產生","Guide.Permission.Field.Name":"權限的顯示名稱","Guide.Permission.Field.Level":"權限層級（模組 / 功能 / 操作）","Guide.Permission.Field.Remarks":"權限的說明",

  // Guide.VehicleType
  "Guide.VehicleType.Description":"車輛類型管理用於分類車輛（如：小貨車、大卡車、冷藏車等），新增車輛時可選擇類型。",
  "Guide.VehicleType.Field.Code":"類型代碼","Guide.VehicleType.Field.Name":"類型名稱","Guide.VehicleType.Field.Description":"類型的詳細描述","Guide.VehicleType.Field.Remarks":"類型的補充說明",

  // Guide.ScaleType
  "Guide.ScaleType.Description":"磅秤類型管理用於定義過磅作業的廢棄物或物料類型，可關聯品項以便自動計價。",
  "Guide.ScaleType.Field.Code":"類型代碼","Guide.ScaleType.Field.Name":"類型名稱","Guide.ScaleType.Field.Item":"關聯的品項，用於自動帶入單價","Guide.ScaleType.Field.Description":"類型的詳細描述","Guide.ScaleType.Field.Remarks":"類型的補充說明",

  // Guide.Bank
  "Guide.Bank.Description":"銀行管理用於維護往來銀行基本資料，包含銀行代碼、名稱及 SWIFT 代碼，供銀行帳戶設定時選用。",
  "Guide.Bank.Field.Code":"銀行代碼（如 004、012）","Guide.Bank.Field.BankName":"銀行中文名稱","Guide.Bank.Field.BankNameEn":"銀行英文名稱","Guide.Bank.Field.SwiftCode":"國際匯款用的 SWIFT/BIC 代碼","Guide.Bank.Field.Remarks":"銀行的補充說明",

  // Guide.Currency
  "Guide.Currency.Description":"幣別管理用於設定系統支援的貨幣種類與匯率，支援多幣別報價與採購。",
  "Guide.Currency.Field.Code":"幣別代碼（如 TWD、USD、JPY）","Guide.Currency.Field.Name":"幣別名稱","Guide.Currency.Field.Symbol":"幣別符號（如 $、¥、€）","Guide.Currency.Field.IsBase":"勾選後設為系統基準幣別，匯率固定為 1","Guide.Currency.Field.Rate":"相對於基準幣別的匯率","Guide.Currency.Field.Remarks":"幣別的補充說明",

  // Guide.PaymentMethod
  "Guide.PaymentMethod.Description":"付款方式管理用於預先定義收付款方式（如：現金、匯款、支票等），在單據中可直接選用。",
  "Guide.PaymentMethod.Field.Code":"付款方式代碼","Guide.PaymentMethod.Field.Name":"付款方式名稱","Guide.PaymentMethod.Field.Remarks":"付款方式的補充說明",

  // Guide.DocumentCategory
  "Guide.DocumentCategory.Description":"文件分類管理用於組織公司文件的分類架構（如：合約、規範、表單等），可設定預設存取權限。",
  "Guide.DocumentCategory.Field.Code":"分類代碼","Guide.DocumentCategory.Field.Name":"分類名稱","Guide.DocumentCategory.Field.AccessLevel":"此分類文件的預設存取權限","Guide.DocumentCategory.Field.Remarks":"分類的補充說明",

  // Guide.EquipmentCategory
  "Guide.EquipmentCategory.Description":"設備分類管理用於組織設備的分類架構（如：生產設備、辦公設備、運輸設備等）。",
  "Guide.EquipmentCategory.Field.Code":"分類代碼","Guide.EquipmentCategory.Field.Name":"分類名稱","Guide.EquipmentCategory.Field.Description":"分類的詳細描述","Guide.EquipmentCategory.Field.Remarks":"分類的補充說明",

  // Guide.Warehouse
  "Guide.Warehouse.Description":"倉庫管理用於建立與維護倉庫基本資料，每個倉庫可進一步設定儲位。進銷存作業時需指定存放倉庫。",
  "Guide.Warehouse.Field.Code":"倉庫代碼","Guide.Warehouse.Field.Name":"倉庫名稱","Guide.Warehouse.Field.Contact":"倉庫聯絡人","Guide.Warehouse.Field.Phone":"倉庫聯絡電話","Guide.Warehouse.Field.Address":"倉庫地址","Guide.Warehouse.Field.Remarks":"倉庫的補充說明",
  "Guide.Warehouse.Tip1":"建立倉庫後可在編輯模式下新增儲位，進行更精細的庫存管理。",

  // Guide.WarehouseLocation
  "Guide.WarehouseLocation.Description":"儲位管理用於定義倉庫內的存放位置（區域、走道、層架、位置），實現精細化庫存管理。",
  "Guide.WarehouseLocation.Field.Code":"儲位代碼","Guide.WarehouseLocation.Field.Name":"儲位名稱","Guide.WarehouseLocation.Field.Warehouse":"所屬倉庫","Guide.WarehouseLocation.Field.Zone":"區域編號","Guide.WarehouseLocation.Field.Aisle":"走道編號","Guide.WarehouseLocation.Field.Level":"層架編號","Guide.WarehouseLocation.Field.Position":"位置編號","Guide.WarehouseLocation.Field.MaxCapacity":"儲位最大容量","Guide.WarehouseLocation.Field.Remarks":"儲位的補充說明",

  // Guide.Company
  "Guide.Company.Description":"公司管理用於維護企業基本資料，包含統一編號、地址、聯絡方式等。多公司環境下可切換作業公司。",
  "Guide.Company.TabOverview":"本頁面包含多個分頁：「公司資料」填寫基本資訊；「公司 Logo」上傳企業標誌；「銀行帳戶」管理公司帳戶。",
  "Guide.Company.Field.Code":"公司代碼","Guide.Company.Field.Name":"公司全名","Guide.Company.Field.ShortName":"公司簡稱，用於報表顯示","Guide.Company.Field.TaxId":"公司統一編號","Guide.Company.Field.Representative":"公司負責人","Guide.Company.Field.Address":"公司地址","Guide.Company.Field.Phone":"公司電話","Guide.Company.Field.Email":"公司電子郵件","Guide.Company.Field.Website":"公司官方網站","Guide.Company.Field.IsDefault":"設為預設公司，登入後自動選用",
  "Guide.Company.Tip1":"設為預設公司後，所有新建單據會自動帶入此公司資料。",

  // Guide.PaperSetting
  "Guide.PaperSetting.Description":"紙張設定用於定義報表列印使用的紙張規格，包含尺寸、方向與邊距，可供報表列印設定選用。",
  "Guide.PaperSetting.Field.Code":"紙張代碼","Guide.PaperSetting.Field.Name":"紙張名稱（如 A4、Letter）","Guide.PaperSetting.Field.Width":"紙張寬度（公分）","Guide.PaperSetting.Field.Height":"紙張高度（公分）","Guide.PaperSetting.Field.Orientation":"列印方向（直向 / 橫向）","Guide.PaperSetting.Field.Remarks":"紙張的補充說明",
  "Guide.PaperSetting.Tip1":"可使用「查詢紙張」按鈕快速套用常見紙張規格（A4、A5、B5 等）。",

  // Guide.ReportPrintConfiguration
  "Guide.ReportPrintConfiguration.Description":"報表列印設定用於指定每份報表使用的紙張規格，確保列印結果符合預期格式。",
  "Guide.ReportPrintConfiguration.Field.Code":"設定代碼","Guide.ReportPrintConfiguration.Field.ReportName":"關聯的報表名稱（唯讀）","Guide.ReportPrintConfiguration.Field.PaperSetting":"選擇此報表使用的紙張規格","Guide.ReportPrintConfiguration.Field.Remarks":"設定的補充說明",

  // Guide.TextMessageTemplate
  "Guide.TextMessageTemplate.Description":"訊息模板管理用於自訂採購單「複製訊息」的格式，可設定抬頭、明細欄位與頁尾文字。",
  "Guide.TextMessageTemplate.Step1":"設定抬頭文字：輸入訊息開頭的固定文字","Guide.TextMessageTemplate.Step2":"選擇明細欄位：勾選要包含的品項資訊欄位","Guide.TextMessageTemplate.Step3":"設定頁尾文字：輸入訊息結尾的備註文字",

  // Guide.PayrollItem
  "Guide.PayrollItem.Description":"薪資項目管理用於定義薪資計算中的各項加減項（如：底薪、加班費、勞保費等）。",
  "Guide.PayrollItem.Field.Code":"項目代碼","Guide.PayrollItem.Field.Name":"項目名稱","Guide.PayrollItem.Field.Description":"項目的計算說明","Guide.PayrollItem.Field.Remarks":"項目的補充說明",

  // Guide.PayrollPeriod
  "Guide.PayrollPeriod.Description":"薪資期間管理用於定義薪資計算的時間區間，通常以月為單位。",
  "Guide.PayrollPeriod.Field.Name":"期間名稱","Guide.PayrollPeriod.Field.Year":"年度","Guide.PayrollPeriod.Field.Month":"月份","Guide.PayrollPeriod.Field.Remarks":"期間的補充說明",

  // Simple payroll settings (description only)
  "Guide.InsuranceRate.Description":"保費費率管理用於維護勞健保的費率設定，系統會依據費率自動計算員工與雇主的保費分攤金額。",
  "Guide.HealthInsuranceGrade.Description":"健保分級表管理用於維護全民健康保險的投保金額分級，系統依此計算健保費。",
  "Guide.LaborInsuranceGrade.Description":"勞保分級表管理用於維護勞工保險的投保薪資分級，系統依此計算勞保費。",
  "Guide.MinimumWage.Description":"基本工資管理用於維護每年度的基本工資標準，薪資計算時會參考此設定。",
  "Guide.WithholdingTaxTable.Description":"扣繳稅額表管理用於維護薪資所得扣繳稅額對照表，系統依此自動計算所得稅預扣金額。",
  "Guide.EmployeeSalary.Description":"員工薪資設定用於維護個別員工的薪資結構，包含底薪、津貼等各項薪資明細。",
  "Guide.EmployeeBankAccount.Description":"員工銀行帳戶管理用於維護員工的薪資轉帳帳戶資訊。",
  "Guide.MonthlyAttendanceSummary.Description":"月出勤彙總用於記錄員工每月的出勤統計，包含出勤天數、加班時數等，供薪資計算參考。",
};

// ==================== STEP 3: Insert resx keys ====================

function escXml(s) {
  return s.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;');
}

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

// en-US translations for Guide.* (brief)
const enUS = {
  "Field.IsSaleable":"Saleable","Field.ManagerId":"Manager","Field.DeputyManagerId":"Deputy Manager","Field.BankNameEn":"Bank Name (EN)","Field.Symbol":"Symbol","Field.Aisle":"Aisle","Field.Width":"Width (cm)","Field.Height":"Height (cm)","Field.Orientation":"Orientation","Field.TopMargin":"Top Margin","Field.BottomMargin":"Bottom Margin","Field.LeftMargin":"Left Margin","Field.RightMargin":"Right Margin","Field.TaxId":"Tax ID","Field.Representative":"Representative","Field.IsDefault":"Set as Default","Field.TargetAmount":"Target Amount","Field.ItemId":"Related Item","Entity.ScaleType":"Scale Type","Field.Warehouse":"Warehouse","Field.PaperSetting":"Paper Setting",
  "Guide.Unit.Description":"Unit management defines measurement units for items (e.g., PC, BOX, KG).","Guide.Unit.Field.Code":"Unit code, e.g., PCS, BOX, KG","Guide.Unit.Field.Name":"Unit name","Guide.Unit.Field.EnglishName":"English name for exported documents","Guide.Unit.Field.Remarks":"Additional notes",
  "Guide.Size.Description":"Size management defines item specifications (e.g., S, M, L, XL).","Guide.Size.Field.Code":"Size code","Guide.Size.Field.Name":"Size name","Guide.Size.Field.Remarks":"Additional notes",
  "Guide.ItemCategory.Description":"Item categories organize items by type (e.g., raw materials, finished goods) for search and reporting.","Guide.ItemCategory.Field.Code":"Category code","Guide.ItemCategory.Field.Name":"Category name","Guide.ItemCategory.Field.IsSaleable":"When checked, items in this category appear in sales documents","Guide.ItemCategory.Field.Remarks":"Additional notes",
  "Guide.CompositionCategory.Description":"Composition categories organize BOM recipes by product line.","Guide.CompositionCategory.Field.Code":"Category code","Guide.CompositionCategory.Field.Name":"Category name","Guide.CompositionCategory.Field.Remarks":"Additional notes",
  "Guide.SalesReturnReason.Description":"Predefined return reason options (e.g., defective, quantity mismatch) for sales return documents.","Guide.SalesReturnReason.Field.Code":"Auto-generated code","Guide.SalesReturnReason.Field.Name":"Reason name","Guide.SalesReturnReason.Field.Remarks":"Additional notes",
  "Guide.SalesTarget.Description":"Set monthly sales targets per salesperson for dashboard tracking.","Guide.SalesTarget.Field.Year":"Target year","Guide.SalesTarget.Field.Month":"Target month","Guide.SalesTarget.Field.Salesperson":"Salesperson for the target","Guide.SalesTarget.Field.Amount":"Monthly sales target amount","Guide.SalesTarget.Field.Remarks":"Additional notes",
  "Guide.PurchaseReturnReason.Description":"Predefined return reason options (e.g., poor quality, spec mismatch) for purchase returns.","Guide.PurchaseReturnReason.Field.Code":"Auto-generated code","Guide.PurchaseReturnReason.Field.Name":"Reason name","Guide.PurchaseReturnReason.Field.Remarks":"Additional notes",
  "Guide.Department.Description":"Department management builds the organization hierarchy with manager and parent-child relationships.","Guide.Department.Field.Code":"Department code","Guide.Department.Field.Name":"Department name","Guide.Department.Field.Manager":"Department manager","Guide.Department.Field.Deputy":"Deputy manager","Guide.Department.Field.Parent":"Parent department for hierarchy","Guide.Department.Field.Phone":"Department phone","Guide.Department.Field.Location":"Department location","Guide.Department.Field.Remarks":"Additional notes",
  "Guide.EmployeePosition.Description":"Position management defines job titles (e.g., Manager, Engineer).","Guide.EmployeePosition.Field.Code":"Position code","Guide.EmployeePosition.Field.Name":"Position name","Guide.EmployeePosition.Field.Remarks":"Additional notes",
  "Guide.Role.Description":"Role management defines system roles with configurable permission sets.","Guide.Role.Field.Code":"Role code for system identification","Guide.Role.Field.Name":"Role display name","Guide.Role.Field.Remarks":"Additional notes",
  "Guide.Permission.Description":"Permission viewer showing all system-defined permissions. Read-only — permissions are auto-generated.","Guide.Permission.Field.Code":"Permission identifier (auto-generated)","Guide.Permission.Field.Name":"Permission display name","Guide.Permission.Field.Level":"Permission level (Module/Function/Operation)","Guide.Permission.Field.Remarks":"Permission description",
  "Guide.VehicleType.Description":"Vehicle type management categorizes vehicles (e.g., truck, van, refrigerated).","Guide.VehicleType.Field.Code":"Type code","Guide.VehicleType.Field.Name":"Type name","Guide.VehicleType.Field.Description":"Detailed description","Guide.VehicleType.Field.Remarks":"Additional notes",
  "Guide.ScaleType.Description":"Scale type management defines material types for weighing operations, linkable to items for auto-pricing.","Guide.ScaleType.Field.Code":"Type code","Guide.ScaleType.Field.Name":"Type name","Guide.ScaleType.Field.Item":"Related item for auto unit price","Guide.ScaleType.Field.Description":"Detailed description","Guide.ScaleType.Field.Remarks":"Additional notes",
  "Guide.Bank.Description":"Bank management maintains bank info including codes and SWIFT codes for bank account setup.","Guide.Bank.Field.Code":"Bank code (e.g., 004, 012)","Guide.Bank.Field.BankName":"Bank name","Guide.Bank.Field.BankNameEn":"Bank name in English","Guide.Bank.Field.SwiftCode":"SWIFT/BIC code for international transfers","Guide.Bank.Field.Remarks":"Additional notes",
  "Guide.Currency.Description":"Currency management defines supported currencies and exchange rates for multi-currency operations.","Guide.Currency.Field.Code":"Currency code (e.g., TWD, USD, JPY)","Guide.Currency.Field.Name":"Currency name","Guide.Currency.Field.Symbol":"Currency symbol (e.g., $, ¥, €)","Guide.Currency.Field.IsBase":"Set as base currency (rate fixed at 1)","Guide.Currency.Field.Rate":"Exchange rate relative to base currency","Guide.Currency.Field.Remarks":"Additional notes",
  "Guide.PaymentMethod.Description":"Payment method management defines payment options (e.g., cash, transfer, check).","Guide.PaymentMethod.Field.Code":"Payment method code","Guide.PaymentMethod.Field.Name":"Payment method name","Guide.PaymentMethod.Field.Remarks":"Additional notes",
  "Guide.DocumentCategory.Description":"Document categories organize company documents (e.g., contracts, forms) with default access levels.","Guide.DocumentCategory.Field.Code":"Category code","Guide.DocumentCategory.Field.Name":"Category name","Guide.DocumentCategory.Field.AccessLevel":"Default access level for documents in this category","Guide.DocumentCategory.Field.Remarks":"Additional notes",
  "Guide.EquipmentCategory.Description":"Equipment categories organize equipment types (e.g., production, office, transport).","Guide.EquipmentCategory.Field.Code":"Category code","Guide.EquipmentCategory.Field.Name":"Category name","Guide.EquipmentCategory.Field.Description":"Detailed description","Guide.EquipmentCategory.Field.Remarks":"Additional notes",
  "Guide.Warehouse.Description":"Warehouse management maintains warehouse info. Warehouse locations can be set up for detailed inventory tracking.","Guide.Warehouse.Field.Code":"Warehouse code","Guide.Warehouse.Field.Name":"Warehouse name","Guide.Warehouse.Field.Contact":"Contact person","Guide.Warehouse.Field.Phone":"Phone number","Guide.Warehouse.Field.Address":"Warehouse address","Guide.Warehouse.Field.Remarks":"Additional notes","Guide.Warehouse.Tip1":"After creating a warehouse, add locations in edit mode for detailed inventory management.",
  "Guide.WarehouseLocation.Description":"Warehouse location management defines storage positions (zone, aisle, level, position) within warehouses.","Guide.WarehouseLocation.Field.Code":"Location code","Guide.WarehouseLocation.Field.Name":"Location name","Guide.WarehouseLocation.Field.Warehouse":"Parent warehouse","Guide.WarehouseLocation.Field.Zone":"Zone number","Guide.WarehouseLocation.Field.Aisle":"Aisle number","Guide.WarehouseLocation.Field.Level":"Level/shelf number","Guide.WarehouseLocation.Field.Position":"Position number","Guide.WarehouseLocation.Field.MaxCapacity":"Maximum storage capacity","Guide.WarehouseLocation.Field.Remarks":"Additional notes",
  "Guide.Company.Description":"Company management maintains enterprise info including tax ID, address, and contacts. Multi-company environments can switch operating company.","Guide.Company.TabOverview":"Multiple tabs: Company Data, Company Logo, Bank Accounts.","Guide.Company.Field.Code":"Company code","Guide.Company.Field.Name":"Full company name","Guide.Company.Field.ShortName":"Short name for reports","Guide.Company.Field.TaxId":"Tax identification number","Guide.Company.Field.Representative":"Company representative","Guide.Company.Field.Address":"Company address","Guide.Company.Field.Phone":"Company phone","Guide.Company.Field.Email":"Company email","Guide.Company.Field.Website":"Company website","Guide.Company.Field.IsDefault":"Set as default; new documents auto-use this company","Guide.Company.Tip1":"Setting as default company auto-fills company info on all new documents.",
  "Guide.PaperSetting.Description":"Paper settings define print paper specs including size, orientation, and margins for report printing.","Guide.PaperSetting.Field.Code":"Paper code","Guide.PaperSetting.Field.Name":"Paper name (e.g., A4, Letter)","Guide.PaperSetting.Field.Width":"Paper width (cm)","Guide.PaperSetting.Field.Height":"Paper height (cm)","Guide.PaperSetting.Field.Orientation":"Print orientation (Portrait/Landscape)","Guide.PaperSetting.Field.Remarks":"Additional notes","Guide.PaperSetting.Tip1":"Use the 'Query Paper' button to quickly apply common paper specs (A4, A5, B5, etc.).",
  "Guide.ReportPrintConfiguration.Description":"Report print config assigns paper settings to each report for correct print formatting.","Guide.ReportPrintConfiguration.Field.Code":"Config code","Guide.ReportPrintConfiguration.Field.ReportName":"Associated report name (read-only)","Guide.ReportPrintConfiguration.Field.PaperSetting":"Paper setting for this report","Guide.ReportPrintConfiguration.Field.Remarks":"Additional notes",
  "Guide.TextMessageTemplate.Description":"Message template management customizes the 'Copy Message' format for purchase orders.","Guide.TextMessageTemplate.Step1":"Set header text: Enter fixed text at the beginning","Guide.TextMessageTemplate.Step2":"Select detail columns: Check which item fields to include","Guide.TextMessageTemplate.Step3":"Set footer text: Enter closing remarks",
  "Guide.PayrollItem.Description":"Payroll items define salary calculation components (e.g., base salary, overtime, insurance deductions).","Guide.PayrollItem.Field.Code":"Item code","Guide.PayrollItem.Field.Name":"Item name","Guide.PayrollItem.Field.Description":"Calculation description","Guide.PayrollItem.Field.Remarks":"Additional notes",
  "Guide.PayrollPeriod.Description":"Payroll periods define salary calculation time ranges, typically monthly.","Guide.PayrollPeriod.Field.Name":"Period name","Guide.PayrollPeriod.Field.Year":"Year","Guide.PayrollPeriod.Field.Month":"Month","Guide.PayrollPeriod.Field.Remarks":"Additional notes",
  "Guide.InsuranceRate.Description":"Insurance rate management maintains labor and health insurance rates for auto-calculating employer and employee contributions.",
  "Guide.HealthInsuranceGrade.Description":"Health insurance grade table maintains NHI insured salary grades for health insurance calculation.",
  "Guide.LaborInsuranceGrade.Description":"Labor insurance grade table maintains insured salary grades for labor insurance calculation.",
  "Guide.MinimumWage.Description":"Minimum wage management maintains annual minimum wage standards referenced in salary calculations.",
  "Guide.WithholdingTaxTable.Description":"Withholding tax table maintains salary income tax withholding lookup tables for auto-calculating tax deductions.",
  "Guide.EmployeeSalary.Description":"Employee salary settings maintain individual salary structures including base pay and allowances.",
  "Guide.EmployeeBankAccount.Description":"Employee bank account management maintains salary transfer account information.",
  "Guide.MonthlyAttendanceSummary.Description":"Monthly attendance summary records employee attendance stats including work days and overtime hours for payroll.",
};

insertKeys(path.join(resxDir, "SharedResource.resx"), zhTW);
insertKeys(path.join(resxDir, "SharedResource.en-US.resx"), enUS);
// ja-JP, zh-CN, fil: use en-US as fallback
insertKeys(path.join(resxDir, "SharedResource.ja-JP.resx"), enUS);
insertKeys(path.join(resxDir, "SharedResource.zh-CN.resx"), enUS);
insertKeys(path.join(resxDir, "SharedResource.fil.resx"), enUS);

console.log("All done!");
