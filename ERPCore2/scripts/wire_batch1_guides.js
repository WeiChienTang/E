const fs = require('fs');
const path = require('path');

const pagesDir = path.join(__dirname, '..', 'Components', 'Pages');

// Map: EditModal file path -> Guide class name
const wiring = {
  "Items/UnitEditModalComponent.razor": "UnitGuide",
  "Items/SizeEditModalComponent.razor": "SizeGuide",
  "Items/ItemCategoryEditModalComponent.razor": "ItemCategoryGuide",
  "Items/CompositionCategoryEditModalComponent.razor": "CompositionCategoryGuide",
  "Sales/SalesReturnReasonEditModalComponent.razor": "SalesReturnReasonGuide",
  "Sales/SalesTargetEditModalComponent.razor": "SalesTargetGuide",
  "Purchase/PurchaseReturnReasonEditModalComponent.razor": "PurchaseReturnReasonGuide",
  "Employees/DepartmentEditModalComponent.razor": "DepartmentGuide",
  "Employees/EmployeePositionEditModalComponent.razor": "EmployeePositionGuide",
  "Employees/RoleEditModalComponent.razor": "RoleGuide",
  "Employees/PermissionEditModalComponent.razor": "PermissionGuide",
  "Vehicles/VehicleTypeEditModalComponent.razor": "VehicleTypeGuide",
  "ScaleManagement/ScaleTypeEditModalComponent.razor": "ScaleTypeGuide",
  "FinancialManagement/BankEditModalComponent.razor": "BankGuide",
  "FinancialManagement/CurrencyEditModalComponent.razor": "CurrencyGuide",
  "FinancialManagement/PaymentMethodEditModalComponent.razor": "PaymentMethodGuide",
  "Documents/DocumentCategoryEditModalComponent.razor": "DocumentCategoryGuide",
  "Equipment/EquipmentCategoryEditModalComponent.razor": "EquipmentCategoryGuide",
  "Warehouse/WarehouseEditModalComponent.razor": "WarehouseGuide",
  "Warehouse/WarehouseLocationEditModalComponent.razor": "WarehouseLocationGuide",
  "Systems/CompanyEditModalComponent.razor": "CompanyGuide",
  "Systems/PaperSettingEditModalComponent.razor": "PaperSettingGuide",
  "Systems/ReportPrintConfigurationEditModalComponent.razor": "ReportPrintConfigurationGuide",
  "Systems/TextMessageTemplateEditModalComponent.razor": "TextMessageTemplateGuide",
  "Payroll/PayrollItemEditModalComponent.razor": "PayrollItemGuide",
  "Payroll/PayrollPeriodEditModalComponent.razor": "PayrollPeriodGuide",
  "Payroll/InsuranceRateEditModalComponent.razor": "InsuranceRateGuide",
  "Payroll/HealthInsuranceGradeEditModalComponent.razor": "HealthInsuranceGradeGuide",
  "Payroll/LaborInsuranceGradeEditModalComponent.razor": "LaborInsuranceGradeGuide",
  "Payroll/MinimumWageEditModalComponent.razor": "MinimumWageGuide",
  "Payroll/WithholdingTaxTableEditModalComponent.razor": "WithholdingTaxTableGuide",
  "Payroll/EmployeeSalaryEditModalComponent.razor": "EmployeeSalaryGuide",
  "Payroll/EmployeeBankAccountEditModalComponent.razor": "EmployeeBankAccountGuide",
  "Payroll/MonthlyAttendanceSummaryEditModalComponent.razor": "MonthlyAttendanceSummaryGuide",
};

const usingLine = '@using ERPCore2.Models.FeatureGuides.GuideDefinitions';
let count = 0;

for (const [relPath, guideName] of Object.entries(wiring)) {
  const filePath = path.join(pagesDir, relPath);
  if (!fs.existsSync(filePath)) {
    console.log(`SKIP (not found): ${relPath}`);
    continue;
  }

  let content = fs.readFileSync(filePath, 'utf8');

  // Skip if already has FeatureGuide
  if (content.includes('FeatureGuide=')) {
    console.log(`SKIP (already has guide): ${relPath}`);
    continue;
  }

  // 1. Add @using if not present
  if (!content.includes(usingLine)) {
    // Insert after the last @using or @inject line before the first HTML tag
    // Find the first line that starts with '<'
    const lines = content.split('\n');
    let insertIdx = 0;
    for (let i = 0; i < lines.length; i++) {
      const trimmed = lines[i].trim();
      if (trimmed.startsWith('@using') || trimmed.startsWith('@inject') || trimmed.startsWith('@implements')) {
        insertIdx = i + 1;
      }
    }
    lines.splice(insertIdx, 0, usingLine);
    content = lines.join('\n');
  }

  // 2. Find ComponentName="...XxxEditModalComponent" and add FeatureGuide after it
  // Pattern: ComponentName="XxxEditModalComponent" followed by > or /> or whitespace
  const compNameRegex = /(ComponentName="[^"]*EditModalComponent")(\s*)(\/?>)/;
  const match = content.match(compNameRegex);
  if (match) {
    const featureGuideParam = `\n                          FeatureGuide="@${guideName}.Create()"`;
    content = content.replace(compNameRegex, `$1${featureGuideParam}$2$3`);
  } else {
    // Try alternative: look for the closing > of GenericEditModalComponent
    // Find last parameter before > or />
    console.log(`WARN: Could not find ComponentName pattern in ${relPath}, trying alternative...`);
    // Find </GenericEditModalComponent> or /> after GenericEditModalComponent
    const genCompMatch = content.match(/<GenericEditModalComponent[\s\S]*?(ComponentName="[^"]*")/);
    if (genCompMatch) {
      const idx = content.indexOf(genCompMatch[1]);
      const afterCompName = idx + genCompMatch[1].length;
      const featureGuideParam = `\n                          FeatureGuide="@${guideName}.Create()"`;
      content = content.slice(0, afterCompName) + featureGuideParam + content.slice(afterCompName);
    } else {
      console.log(`ERROR: Could not wire up ${relPath}`);
      continue;
    }
  }

  fs.writeFileSync(filePath, content, 'utf8');
  count++;
  console.log(`Wired: ${relPath} -> ${guideName}`);
}

console.log(`\nTotal wired: ${count} files`);
