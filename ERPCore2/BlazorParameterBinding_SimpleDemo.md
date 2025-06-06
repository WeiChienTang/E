# Blazor 參數綁定演示

## 簡單演示：參數綁定 vs 方法調用

### 錯誤的理解（類似傳統程式設計）
```csharp
// 錯誤的想法：以為 Blazor 組件像傳統方法調用
public void ShowCustomerDetails()
{
    var customer = GetSelectedCustomer();
    
    // ❌ 以為可以這樣直接調用
    CustomerDetailModal.Show(customer);  // 這不是 Blazor 的做法！
}
```

### 正確的 Blazor 做法
```csharp
// ✅ Blazor 的正確做法
public class CustomersIndex : ComponentBase
{
    private Customer? selectedCustomer;  // 📌 綁定變數
    
    private async Task ShowCustomerDetail(Customer customer)
    {
        // 步驟 1：設置綁定變數
        selectedCustomer = customer;
        
        // 步驟 2：Blazor 自動重新渲染，CustomerDetailModal 收到新參數
        
        // 步驟 3：顯示模態框
        await JS.InvokeVoidAsync("bootstrap.Modal.getOrCreateInstance", 
            "#customerDetailModal").InvokeVoidAsync("show");
    }
}
```

### Razor 模板中的參數綁定
```html
<!-- 參數綁定到變數 -->
<CustomerDetailModal Customer="selectedCustomer" />

<!-- 當 selectedCustomer 改變時，CustomerDetailModal.Customer 也會更新 -->
```

## 實際的資料流

### 情境：用戶點擊表格中的 "ABC公司" 這一行

```
1. 用戶點擊 → TableComponent 觸發 OnRowClick
                     ↓
2. CustomersIndex.ShowCustomerDetail(customer) 被調用
   customer = { Id: 1, CompanyName: "ABC公司", CustomerCode: "C001" }
                     ↓
3. selectedCustomer = customer;
   selectedCustomer 現在指向 ABC公司的資料
                     ↓
4. Blazor 重新渲染 CustomersIndex
                     ↓
5. <CustomerDetailModal Customer="selectedCustomer" />
   Customer 參數收到 ABC公司的資料
                     ↓
6. CustomerDetailModal 顯示 ABC公司的詳細資料
                     ↓
7. 模態框顯示正確的內容
```

## 為什麼不能跳過 `selectedCustomer = customer;`？

### 如果跳過這一步：
```csharp
private async Task ShowCustomerDetail(Customer customer)
{
    // ❌ 跳過設置 selectedCustomer
    // selectedCustomer 仍然是 null 或舊值
    
    await JS.InvokeVoidAsync("bootstrap.Modal.getOrCreateInstance", 
        "#customerDetailModal").InvokeVoidAsync("show");
}
```

### 結果：
```html
<!-- selectedCustomer 仍然是 null -->
<CustomerDetailModal Customer="selectedCustomer" />
<!-- CustomerDetailModal.Customer = null -->
```

模態框會顯示：
- 空白內容
- 或者上一次的客戶資料
- 或者預設值

## 類比說明

想像 Blazor 組件像是一個**電視機**：

### 傳統程式設計（如 WinForms）：
```
你直接告訴電視機要顯示什麼內容
電視機.顯示內容(ABC公司資料)  // 直接調用
```

### Blazor 方式：
```
你需要先把內容放到"頻道"上
頻道 = ABC公司資料           // selectedCustomer = customer
電視機會自動偵測頻道變化      // Blazor 重新渲染
然後顯示新的內容            // CustomerDetailModal 收到新參數
```

## 總結

`selectedCustomer = customer;` 這一步是**必要的**，因為：

1. **Blazor 是響應式框架** - 組件透過參數變化來更新
2. **參數必須綁定到變數** - 不能直接綁定到方法參數
3. **資料流是單向的** - 父組件 → 子組件
4. **重新渲染才會傳遞新參數** - 需要觸發更新機制

這就是為什麼在點擊表格行時，我們必須先設置 `selectedCustomer`，然後才能正確顯示客戶詳細資料的原因！
