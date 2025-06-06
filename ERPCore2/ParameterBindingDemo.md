# Blazor 參數綁定演示

## 傳統方法調用 vs Blazor 參數綁定

### 傳統方法調用 (C# 風格)
```csharp
// 父類
public class Parent 
{
    public void CallChild(string data) 
    {
        var child = new Child();
        child.ProcessData(data); // ← 直接傳遞參數
    }
}

// 子類
public class Child 
{
    public void ProcessData(string data) // ← 接收參數
    {
        Console.WriteLine(data);
    }
}
```

### Blazor 參數綁定 (Razor 風格)
```razor
<!-- 父組件 -->
<ChildComponent Data="@parentData" />

@code {
    private string parentData = "初始值";
    
    private void UpdateData() 
    {
        parentData = "新值"; // ← 修改綁定變數
        // Blazor 自動將新值傳遞給子組件
    }
}

<!-- 子組件 -->
@code {
    [Parameter] public string Data { get; set; } // ← 自動接收新值
    
    protected override void OnParametersSet()
    {
        // 當參數改變時自動調用
        Console.WriteLine($"接收到新值: {Data}");
    }
}
```

## 在我們的客戶管理系統中

### 資料流程
```
1. 使用者點擊表格行
   ↓
2. ShowCustomerDetail(customer) 被調用
   ↓  
3. selectedCustomer = customer; (修改綁定變數)
   ↓
4. Blazor 偵測到 selectedCustomer 改變
   ↓
5. 自動將新值傳遞給 CustomerDetailModal.Customer 參數
   ↓
6. CustomerDetailModal 重新渲染，顯示新的客戶資料
   ↓
7. customerDetailModal.ShowAsync() 顯示模態
```

### 關鍵理解
- `selectedCustomer = customer;` **不是**傳遞參數給方法
- 而是**設定綁定變數的值**
- Blazor 會**自動**將這個值傳遞給子組件的參數
- 這就是 Blazor 的「反應式」特性

## 類比說明
想像一下電視遙控器：
- **傳統方法**: 你走到電視前，手動調整頻道 (直接調用方法)
- **Blazor 綁定**: 你按遙控器按鈕，電視自動響應 (響應式綁定)

在 Blazor 中，`selectedCustomer` 就像遙控器信號，`CustomerDetailModal` 就像電視，會自動響應信號的變化！
