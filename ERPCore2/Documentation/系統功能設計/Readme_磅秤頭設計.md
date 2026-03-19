# 磅秤串列埠通訊設計總綱

## 更新日期
2026-03-18

---

## 📋 概述

ERPCore2 磅秤模組透過 RS-232C 串列埠與 LT-100 重量顯示器通訊，自動讀取車輛進場/出場重量並計算淨重。系統採用**被動監聽模式**，由 LT-100 持續傳送資料，程式端解析穩定狀態後自動帶入欄位。

本模組涉及三個層面：**硬體協議**（LT-100 傳輸規格）、**通訊服務**（串列埠連線與資料解析）、**UI 操作**（讀取按鈕與表單欄位帶入）。

---

## 🏗️ 系統架構圖

```
┌─────────────────────────────────────────────────────────────────────────┐
│                        磅秤通訊系統架構                                   │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  ┌──────────────┐    RS-232C     ┌───────────────────────────────┐     │
│  │  LT-100      │  ────────────→ │  SerialPortService            │     │
│  │  重量顯示器   │  E.7.1/2400   │  (Singleton, 串列埠通訊服務)   │     │
│  │              │  CR LF 斷行    │                               │     │
│  │  SF4: 連續傳送│               │  ┌─────────────────────────┐  │     │
│  │  SF5: Format 1│               │  │ OnSerialDataReceived    │  │     │
│  │  SF6: 有+-有0 │               │  │ → 累積 Buffer           │  │     │
│  └──────────────┘               │  │ → 以 CR LF 斷行         │  │     │
│        ↑                         │  │ → ParseScaleData()      │  │     │
│    荷重元訊號                     │  └───────────┬─────────────┘  │     │
│    (Load Cell)                   │              │ DataReceived   │     │
│                                  └──────────────┼───────────────┘     │
│                                                 ↓                      │
│  ┌──────────────────────────────────────────────────────────────────┐  │
│  │  ScaleRecordEditModalComponent                                    │  │
│  │                                                                    │  │
│  │  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐  │  │
│  │  │ 參數設定 按鈕    │  │ 讀取進場重量     │  │ 讀取出場重量     │  │  │
│  │  │ → 開啟設定 Modal │  │ → 訂閱事件       │  │ → 訂閱事件       │  │  │
│  │  └────────┬────────┘  │ → 等待 ST 穩定   │  │ → 等待 ST 穩定   │  │  │
│  │           ↓            │ → 帶入 EntryWeight│  │ → 帶入 ExitWeight│  │  │
│  │  ScaleSerialPort       │ → 帶入 EntryTime │  │ → 帶入 ExitTime │  │  │
│  │  SettingsModal         │ → 計算 NetWeight  │  │ → 計算 NetWeight │  │  │
│  │  (COM/Baud/Parity)    │ → 自動停止讀取    │  │ → 自動停止讀取   │  │  │
│  │                        └─────────────────┘  └─────────────────┘  │  │
│  └──────────────────────────────────────────────────────────────────┘  │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 📚 文件導覽

本磅秤通訊設計分為兩份詳細文件：

| 文件 | 說明 | 適用場景 |
|------|------|----------|
| [Readme_LT100_RS232通訊協議.md](Readme_LT100_RS232通訊協議.md) | LT-100 硬體端的 RS-232C 傳輸協議規格 | 了解硬體參數設定、資料格式、接腳定義 |
| [Readme_磅秤串列埠程式設計.md](Readme_磅秤串列埠程式設計.md) | ERPCore2 程式端的串列埠通訊實作 | 了解程式架構、解析邏輯、UI 操作流程 |

---

## 🔄 運作流程

### 首次使用

1. **硬體確認**：確認 LT-100 的 SF4 設為「連續傳送」或「穩定後連續傳送」
2. **參數設定**：在 ScaleSerialPortSettingsModal 選擇 COM Port、設定 Baudrate/Parity（需與 LT-100 的 SF2 一致）
3. **套用設定**：點擊「套用」儲存參數

### 每次過磅

1. **車輛進場**：點擊「讀取進場重量」→ 程式自動連線串列埠 → 監聽資料
2. **等待穩定**：LT-100 持續傳送資料，狀態燈顯示即時重量
3. **自動帶入**：收到 ST（穩定）狀態時，自動帶入 EntryWeight + EntryTime
4. **車輛出場**：點擊「讀取出場重量」→ 同樣流程帶入 ExitWeight + ExitTime
5. **自動計算**：NetWeight = EntryWeight - ExitWeight

---

## 📁 相關檔案目錄

```
Services/ScaleManagement/
├── ISerialPortService.cs              # 串列埠服務介面 + 設定模型 + 事件參數
└── SerialPortService.cs               # 串列埠通訊實作（Singleton）

Components/Pages/ScaleManagement/
├── ScaleRecordEditModalComponent.razor # 磅秤紀錄編輯（含讀取按鈕）
└── ScaleSerialPortSettingsModalComponent.razor # RS-232C 參數設定 Modal

Data/Entities/ScaleManagement/
└── ScaleRecord.cs                     # 磅秤紀錄實體（含重量/時間欄位）
```

---

## ⚠️ 關鍵注意事項

1. **SerialPortService 必須為 Singleton**：實體串列埠連線需跨 Blazor Circuit 持久化，已在 `ServiceRegistration.cs` 中以 `AddSingleton` 註冊
2. **LT-100 的 SF4 設定決定能否收到資料**：程式採被動監聽模式，若 LT-100 設為 COMMAND 模式（電腦端連結不傳送），程式無法接收任何數據
3. **傳送格式全相容**：程式支援 LT-100 所有 SF5（Format 1～4）和 SF6（有無正負號/前導零）組合，但建議維持出廠預設值
4. **Windows 專屬**：`System.IO.Ports.SerialPort` 在 .NET 上主要支援 Windows，Linux 需額外安裝套件
5. **Blazor Server 執行緒安全**：串列埠的 DataReceived 事件從背景執行緒觸發，必須透過 `InvokeAsync()` 切回 Blazor 同步上下文才能更新 UI

---

## 相關文件

- [Readme_LT100_RS232通訊協議.md](Readme_LT100_RS232通訊協議.md) - LT-100 硬體通訊協議
- [Readme_磅秤串列埠程式設計.md](Readme_磅秤串列埠程式設計.md) - 程式端串列埠實作
