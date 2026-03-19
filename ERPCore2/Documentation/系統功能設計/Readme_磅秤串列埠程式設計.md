# 磅秤串列埠程式設計

## 更新日期
2026-03-18

---

## 📋 概述

本文件說明 ERPCore2 中磅秤串列埠通訊的程式實作，包含服務架構、資料解析邏輯、UI 操作流程，以及各元件間的互動關係。

---

## 一、服務架構

### 1.1 SerialPortService（Singleton）

核心通訊服務，負責串列埠的開啟/關閉、資料接收與解析。

```
ISerialPortService
├── IsOpen                    # 串列埠是否已開啟
├── CurrentSettings           # 目前的連線設定
├── GetAvailablePortNames()   # 取得系統可用 COM Port 清單
├── ApplySettings()           # 儲存參數（不開啟連線）
├── Open()                    # 開啟串列埠連線
├── Close()                   # 關閉串列埠連線
└── DataReceived event        # 資料接收事件
```

**為什麼必須是 Singleton？**

`System.IO.Ports.SerialPort` 代表一個實體硬體連線，同一個 COM Port 不能被多個 instance 同時開啟。Blazor Server 中每個使用者 session 是一個 Scoped 生命週期，如果 SerialPortService 是 Scoped，切換頁面後再回來就會得到新的 instance，之前的串列埠連線就遺失了。Singleton 確保整個應用程式共用同一個串列埠連線。

### 1.2 SerialPortSettings（連線參數模型）

| 屬性 | 類型 | 預設值 | 說明 |
|------|------|--------|------|
| PortName | string | "COM1" | 串列埠名稱 |
| BaudRate | int | 2400 | 鮑率（需與 LT-100 SF2 一致） |
| Parity | string | "Even" | 同位檢查：Even / Odd / None |
| DataBits | int | 7 | 資料位元數 |
| StopBits | int | 1 | 停止位元數 |

> 預設值完全對應 LT-100 出廠預設的 E.7.1 / 2400。

### 1.3 ScaleDataReceivedEventArgs（解析結果模型）

| 屬性 | 類型 | 說明 |
|------|------|------|
| RawData | string | 原始資料字串 |
| StabilityStatus | string | 穩定狀態：ST / US / OL |
| WeightMode | string | 重量模式：GS / NT |
| IsPositive | bool | 正值為 true，負值為 false |
| WeightValue | decimal | 重量數值（含正負） |
| Unit | string | 單位：kg / lb / g / t |
| IsStable | bool | 是否穩定（StabilityStatus == "ST"） |
| IsOverload | bool | 是否過載（StabilityStatus == "OL"） |
| IsValid | bool | 解析是否成功 |
| ErrorMessage | string? | 解析失敗的錯誤訊息 |

---

## 二、資料接收與解析流程

### 2.1 接收流程

```
SerialPort.DataReceived（背景執行緒）
    │
    ↓
OnSerialDataReceived()
    │  讀取 ReadExisting() 並累積到 Buffer
    │
    ↓
以 '\n' 斷行，逐行取出
    │  TrimEnd('\r', '\n') 清除結尾符號
    │
    ↓
ParseScaleData(line)
    │  嘗試 ScaleDataPatternFull（完整格式）
    │  失敗則嘗試 ScaleDataPatternShort（簡潔格式）
    │
    ↓
DataReceived?.Invoke(this, parsed)
    │  觸發事件，通知訂閱者
    │
    ↓
ScaleRecordEditModalComponent.OnScaleDataReceived()
    │  透過 InvokeAsync() 切回 Blazor UI 執行緒
    └─→ 判斷穩定狀態 → 帶入欄位 → 計算淨重
```

### 2.2 資料解析策略（雙正則匹配）

程式使用兩個正則表達式依序嘗試，以涵蓋 LT-100 所有 SF5/SF6 設定組合：

**第一優先：ScaleDataPatternFull（完整格式，對應 Format 1/2/4）**

```
(?<status>ST|US|OL)\s*,\s*(?<mode>GS|NT)\s*,\s*(?<sign>[+\-])?(?<value>[\d.\s]+)\s*,?\s*(?<unit>kg|lb|g|t)
```

設計要點：
- `(?<sign>[+\-])?` — 正負號為可選，相容 SF6「無+-」設定
- `(?<value>[\d.\s]+)` — 數值允許空白字元，相容 Format 4（無小數點補空白）
- `,?\s*(?<unit>...)` — 單位前逗號為可選，相容 Format 2/4

可匹配的範例：
- `ST,GS,+03000.0kg` （Format 1，預設）
- `ST,GS,+03000.0,kg` （Format 2/4）
- `US,NT,03000.0kg` （SF6 無正負號）
- `ST,GS,+3000.0kg` （SF6 無前導零）

**第二優先：ScaleDataPatternShort（簡潔格式，對應 Format 3）**

```
^\s*(?<sign>[+\-])?(?<value>[\d.\s]+)\s*,?\s*(?<unit>kg|lb|g|t)\s*$
```

可匹配的範例：
- `+03000.0kg` （Format 3，有正負號）
- `03000.0kg` （Format 3 + SF6 無正負號）

**預設值回填規則：**

| 群組 | 未匹配時的預設值 | 原因 |
|------|-----------------|------|
| status | "ST"（穩定） | Format 3 無狀態前綴，假設為穩定是最安全的預設 |
| mode | "GS"（總重） | Format 3 無模式前綴，假設為總重是最常見的情境 |
| sign | 正值 | SF6 無正負號時，重量為正值 |

### 2.3 解析失敗處理

當兩個正則都無法匹配時：
1. `IsValid` 設為 false
2. `ErrorMessage` 記錄原始資料字串
3. `ScaleRecordEditModalComponent` 收到無效資料後，顯示警告通知使用者檢查 LT-100 的 SF4/SF5/SF6 設定

---

## 三、UI 元件設計

### 3.1 ScaleSerialPortSettingsModalComponent（參數設定）

提供串列埠連線參數的 UI 設定介面。

**功能：**
- COM Port 下拉選單（含重新偵測按鈕）
- Baudrate 選項：1200 / 2400 / 4800 / 9600 / 19200 / 38400 / 57600
- Parity 選項：Even / Odd / None
- Data Bits 選項：7 / 8
- Stop Bits 選項：1 / 2
- 常用預設組合快捷按鈕（E.7.1/2400、N.8.1/9600、O.7.1/2400）
- LT-100 端設定提醒（SF4/SF5/SF6 的建議設定）

**操作流程：**
1. 開啟 Modal 時自動偵測可用 COM Port 並載入已儲存的設定
2. 使用者選擇參數或點擊預設組合
3. 點擊「套用」→ 呼叫 `SerialPortService.ApplySettings()` 儲存參數
4. 關閉 Modal，下次點擊「讀取」按鈕時自動使用此設定連線

> 注意：「套用」僅儲存參數，不實際開啟串列埠。串列埠在使用者點擊「讀取進場/出場重量」時才自動開啟。

### 3.2 ScaleRecordEditModalComponent（磅秤紀錄編輯）

在 GenericEditModalComponent 的 CustomActionButtons 區域放置磅秤操作按鈕。

**自訂按鈕：**

| 按鈕 | 功能 | 狀態切換 |
|------|------|----------|
| 參數設定 | 開啟 ScaleSerialPortSettingsModal | — |
| 讀取進場重量 / 停止讀取進場 | 開始/停止監聽串列埠，帶入 EntryWeight + EntryTime | 綠色 ↔ 紅色 |
| 讀取出場重量 / 停止讀取出場 | 開始/停止監聽串列埠，帶入 ExitWeight + ExitTime | 綠色 ↔ 紅色 |

**互斥邏輯：** 進場與出場不能同時讀取，按鈕互相停用。

**讀取中狀態指示：** 顯示黃色 badge 搭配 spinner 動畫，即時顯示目前讀到的重量值或「等待穩定...」。

### 3.3 讀取操作完整流程

```
使用者點擊「讀取進場重量」
    │
    ├─ 串列埠未開啟？
    │   ├─ 無已儲存設定 → 提示「請先設定串列埠」→ 結束
    │   └─ 有已儲存設定 → SerialPortService.Open(settings)
    │       └─ 開啟失敗 → 顯示錯誤訊息 → 結束
    │
    ├─ 訂閱 DataReceived 事件
    ├─ 設定 _readingTarget = "entry"
    ├─ 顯示「開始讀取進場重量，等待穩定重量...」
    │
    ↓ （等待事件觸發）
    
OnScaleDataReceived(e)
    │
    ├─ e.IsValid == false → 顯示解析失敗警告
    │
    ├─ e.IsOverload → 顯示「磅秤過載」警告
    │
    ├─ e.IsStable == false → 更新即時重量顯示（不帶入欄位）
    │
    └─ e.IsStable == true → 
        ├─ Entity.EntryWeight = e.WeightValue
        ├─ Entity.EntryTime = DateTime.Now
        ├─ RecalculateNetWeight()
        ├─ 取消訂閱 DataReceived 事件
        ├─ _readingTarget = null
        ├─ 顯示「已讀取穩定進場重量：xxx kg」
        └─ StateHasChanged()
```

---

## 四、淨重計算邏輯

```
NetWeight = EntryWeight - ExitWeight
```

| 欄位 | 說明 | 觸發時機 |
|------|------|----------|
| EntryWeight | 進場重量（車輛滿載） | 串列埠讀取穩定值 或 手動輸入 |
| EntryTime | 進場時間 | 串列埠讀取時自動帶入 DateTime.Now |
| ExitWeight | 出場重量（車輛空載） | 串列埠讀取穩定值 或 手動輸入 |
| ExitTime | 出場時間 | 串列埠讀取時自動帶入 DateTime.Now |
| NetWeight | 淨重（貨物重量） | EntryWeight 或 ExitWeight 變更時自動計算 |

計算觸發的兩個路徑：
1. **串列埠帶入**：`OnScaleDataReceived` 中直接呼叫 `RecalculateNetWeight()`
2. **手動輸入**：`HandleFieldChangedAsync` 偵測到 EntryWeight 或 ExitWeight 變更時呼叫 `RecalculateNetWeight()`

---

## 五、執行緒安全考量

### 5.1 背景執行緒 → UI 執行緒

`SerialPort.DataReceived` 事件由 .NET 的 ThreadPool 執行緒觸發，不是 Blazor 的同步上下文。直接在此執行緒修改 UI 狀態會導致例外。

解決方式：在 `OnScaleDataReceived` 中使用 `InvokeAsync()` 將所有 UI 操作（修改 Entity 屬性、呼叫 NotificationService、StateHasChanged）包裝在 Blazor 的同步上下文中執行。

### 5.2 Buffer 的 lock 保護

`SerialPortService` 內部使用 `_lock` 物件保護 `_buffer`（StringBuilder），因為 `OnSerialDataReceived` 可能被多次快速觸發，需確保 Buffer 的讀寫不會衝突。

### 5.3 組件釋放後的事件清理

`ScaleRecordEditModalComponent` 實作 `IDisposable`，在 `Dispose()` 中取消訂閱 `DataReceived` 事件，避免組件釋放後仍收到串列埠事件導致 `ObjectDisposedException`。`OnScaleDataReceived` 的 catch 區塊也額外捕捉此例外做靜默處理。

---

## 六、DI 註冊

```csharp
// ServiceRegistration.cs
// RS-232C 串列埠服務（Singleton：實體串列埠連線需跨 Circuit 持久化）
services.AddSingleton<ISerialPortService, SerialPortService>();
```

---

## 七、目前限制與未來擴展方向

### 目前限制

| 項目 | 說明 |
|------|------|
| 被動監聽模式 | 僅支援 LT-100 主動傳送（SF4 連續傳送），不支援 COMMAND 模式主動查詢 |
| 單一磅秤 | Singleton 設計只支援連接一台磅秤，無法同時連接多台 |
| Windows 限制 | `System.IO.Ports.SerialPort` 主要支援 Windows 平台 |

### 未來可擴展

| 方向 | 說明 |
|------|------|
| COMMAND 模式 | 實作 `SendCommand(string cmd)` 方法，支援 SF4 設為「電腦端連結」時主動發送 `RD\r\n` 讀取重量 |
| 多磅秤支援 | 改為 Factory 模式，依 COM Port 建立多個 SerialPort instance |
| 串列埠斷線偵測 | 監聽 `SerialPort.ErrorReceived` 事件，串列埠異常時自動通知使用者 |
| 歷史紀錄 | 記錄每次讀取的原始資料到 Log，便於問題排查 |

---

## 相關文件

- [Readme_磅秤頭設計.md](Readme_磅秤頭設計.md) - 磅秤通訊設計總綱
- [Readme_LT100_RS232通訊協議.md](Readme_LT100_RS232通訊協議.md) - LT-100 硬體通訊協議
