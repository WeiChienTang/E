const fs = require('fs');
const path = require('path');
const resxDir = path.join(__dirname, '..', 'Resources');

function escXml(s) { return s.replace(/&/g,'&amp;').replace(/</g,'&lt;').replace(/>/g,'&gt;'); }
function insertKeys(file, keys) {
  let content = fs.readFileSync(file, 'utf8');
  const marker = 'name="Modal.DefaultTitle"';
  const idx = content.indexOf(marker);
  if (idx === -1) { console.log("ERROR: " + path.basename(file)); return; }
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

const zhTW = {
  // Shared key
  "Guide.TabDescriptions": "分頁功能說明",

  // Employee tab descriptions
  "Guide.Employee.Tab.EmployeeData": "填寫員工的基本個人資料、聯絡方式、緊急聯絡人、部門與職位等核心資訊",
  "Guide.Employee.Tab.VehicleInfo": "管理配發給此員工的公務車輛，可新增、編輯或解除車輛關聯",
  "Guide.Employee.Tab.PersonalTools": "記錄配發給員工的個人工具或設備（如：筆電、手機、鑰匙等），追蹤領用與歸還狀態",
  "Guide.Employee.Tab.Training": "記錄員工參加的訓練課程，包含課程名稱、時數、訓練機構與成績",
  "Guide.Employee.Tab.Permissions": "精細控制此員工可存取的系統功能模組與操作權限，勾選即授權",
  "Guide.Employee.Tab.Payroll": "維護此員工的薪資結構設定，包含底薪、津貼等各項薪資明細（需啟用薪資模組）",
  "Guide.Employee.Tab.BankAccounts": "維護員工的薪資轉帳銀行帳戶資訊，供每月發薪使用",
  "Guide.Employee.Tab.Photo": "上傳與管理員工的個人照片",
  "Guide.Employee.Tab.Documents": "管理與此員工相關的文件附件（需啟用文件模組）",

  // Employee additional field descriptions
  "Guide.Employee.BasicFieldsTitle": "基本個人資料",
  "Guide.Employee.ContactFieldsTitle": "聯絡與組織資訊",
  "Guide.Employee.Field.EnglishName": "員工英文姓名，用於英文報表或國際溝通",
  "Guide.Employee.Field.IdNumber": "身分證字號，用於勞健保與報稅",
  "Guide.Employee.Field.Nationality": "國籍",
  "Guide.Employee.Field.MaritalStatus": "婚姻狀態（未婚/已婚/其他）",
  "Guide.Employee.Field.BloodType": "血型資訊",
  "Guide.Employee.Field.Mobile": "員工手機號碼，主要聯絡方式",
  "Guide.Employee.Field.Phone": "市話或分機號碼",
  "Guide.Employee.Field.Email": "電子郵件信箱",
  "Guide.Employee.Field.HomeAddress": "戶籍地址",
  "Guide.Employee.Field.MailingAddress": "通訊地址（實際居住地址）",
  "Guide.Employee.Field.EmergencyContact": "緊急聯絡人姓名",
  "Guide.Employee.Field.EmergencyPhone": "緊急聯絡人電話",
  "Guide.Employee.Field.ResignDate": "離職日期（在職員工此欄為空）",
  "Guide.Employee.Field.EmploymentStatus": "在職狀態（在職/離職/留職停薪等）",
  "Guide.Employee.Tip2": "薪資與銀行帳戶分頁需要先儲存員工資料後才會出現。",
};

const enUS = {
  "Guide.TabDescriptions": "Tab Descriptions",
  "Guide.Employee.Tab.EmployeeData": "Enter core personal info, contacts, emergency contacts, department, and position",
  "Guide.Employee.Tab.VehicleInfo": "Manage company vehicles assigned to this employee; add, edit, or unlink vehicles",
  "Guide.Employee.Tab.PersonalTools": "Track tools and equipment issued to the employee (laptop, phone, keys, etc.) with assign/return dates",
  "Guide.Employee.Tab.Training": "Record training courses attended, including course name, hours, organization, and results",
  "Guide.Employee.Tab.Permissions": "Fine-grained control over which system modules and operations this employee can access",
  "Guide.Employee.Tab.Payroll": "Maintain this employee's salary structure including base pay and allowances (requires Payroll module)",
  "Guide.Employee.Tab.BankAccounts": "Maintain salary transfer bank account information for monthly payroll",
  "Guide.Employee.Tab.Photo": "Upload and manage employee photos",
  "Guide.Employee.Tab.Documents": "Manage document attachments related to this employee (requires Documents module)",
  "Guide.Employee.BasicFieldsTitle": "Personal Information",
  "Guide.Employee.ContactFieldsTitle": "Contact & Organization",
  "Guide.Employee.Field.EnglishName": "English name for international reports and communication",
  "Guide.Employee.Field.IdNumber": "National ID number for insurance and tax filing",
  "Guide.Employee.Field.Nationality": "Nationality",
  "Guide.Employee.Field.MaritalStatus": "Marital status (single/married/other)",
  "Guide.Employee.Field.BloodType": "Blood type information",
  "Guide.Employee.Field.Mobile": "Employee mobile number, primary contact method",
  "Guide.Employee.Field.Phone": "Landline or extension number",
  "Guide.Employee.Field.Email": "Email address",
  "Guide.Employee.Field.HomeAddress": "Registered home address",
  "Guide.Employee.Field.MailingAddress": "Mailing address (actual residence)",
  "Guide.Employee.Field.EmergencyContact": "Emergency contact name",
  "Guide.Employee.Field.EmergencyPhone": "Emergency contact phone",
  "Guide.Employee.Field.ResignDate": "Resignation date (blank for active employees)",
  "Guide.Employee.Field.EmploymentStatus": "Employment status (active/resigned/leave of absence, etc.)",
  "Guide.Employee.Tip2": "Payroll and Bank Account tabs appear only after saving the employee record.",
};

insertKeys(path.join(resxDir, "SharedResource.resx"), zhTW);
insertKeys(path.join(resxDir, "SharedResource.en-US.resx"), enUS);
insertKeys(path.join(resxDir, "SharedResource.ja-JP.resx"), enUS);
insertKeys(path.join(resxDir, "SharedResource.zh-CN.resx"), enUS);
insertKeys(path.join(resxDir, "SharedResource.fil.resx"), enUS);
console.log("Done!");
