# 便利性功能設計：便條貼 & 行事曆

## 更新日期
2026-03-02

---

## 概述

本文件描述兩個以**使用者生產力**為核心的便利性功能：

| 功能 | 說明 | 進入方式 |
|------|------|----------|
| **便條貼** | 個人專屬的快速記事工具，在不離開當前頁面的情況下記錄資訊 | QuickActionMenu → 便條貼 / `Alt+N` |
| **行事曆** | 個人事項追蹤，未來可整合 ERP 重要日期（如訂單到期、收貨日） | QuickActionMenu → 行事曆 / `Alt+C` |

---

## 核心設計原則

ERP 使用者最大的操作痛點是：**開啟工具時失去當前頁面脈絡**。使用者正在審查採購單時若要記錄資訊，全屏 Modal 會遮蓋整個畫面，導致需要記憶或反覆切換。

因此，兩個功能均採用 **右側滑入面板（Slide-in Drawer）** 模式：

1. **非阻塞（Non-blocking）**：面板疊加在頁面右側，使用者仍可看到主頁面內容
2. **非跳頁（No navigation）**：不離開當前操作的 ERP 頁面
3. **快速關閉**：點擊半透明遮罩或按 `ESC` 即可關閉
4. **資料持久化**：資料儲存於 DB（每位使用者獨立），換電腦、換瀏覽器不遺失

---

## UX 互動設計

### 共用面板行為規範

```
┌──────────────────────────────────────────┐
│         (半透明遮罩，點擊關閉)            │  ← 點擊關閉面板
│                            ┌─────────────┤
│    [原本的 ERP 頁面]        │  [面板內容] │
│    (使用者仍可看到)          │             │
│                            │             │
│                            └─────────────┤
└──────────────────────────────────────────┘
```

| 行為 | 規格 |
|------|------|
| 面板寬度（桌面） | 380–420 px |
| 面板寬度（平板） | 320 px |
| 面板寬度（手機） | 100%（全屏 Drawer） |
| 動畫 | `translateX(100%) → translateX(0)`，300ms ease-out |
| 遮罩透明度 | `rgba(0, 0, 0, 0.3)` |
| ESC 鍵 | 關閉面板（與現有 Modal ESC 行為一致） |
| 同時開啟限制 | 同一時間只允許一個面板開啟（便條貼 / 行事曆互斥） |

---

## 功能一：便條貼（Sticky Notes）

### UX 流程

```
使用者點擊 QuickActionMenu「便條貼」
        ↓
QuickActionMenu 關閉（isExpanded = false）
        ↓
右側面板滑入（StickyNoteDrawer.razor）
        ↓
┌─────────────────────────────────┐
│ 📌 便條貼                  [×]  │
├─────────────────────────────────┤
│ [🔍 搜尋便條...]  [+ 新增]      │
├─────────────────────────────────┤
│ ● 下午3點確認採購單              │  ← 點擊展開編輯
│   🟡 黃色  今天 14:32       [⋮] │  ← 右鍵選單：刪除/變色
├─────────────────────────────────┤
│ ● 聯繫客戶A確認交期              │
│   🟢 綠色  昨天             [⋮] │
├─────────────────────────────────┤
│ ● 重要：月底前審核財報            │
│   🔴 紅色  3天前            [⋮] │
└─────────────────────────────────┘
```

### 點擊便條展開（行內編輯）

```
┌─────────────────────────────────┐
│ ● [textarea 直接編輯中...]       │
│   🟡 ○ 🟢 ○ 🔵 ○ 🔴 ○     [刪除]│  ← 顏色選擇器 + 刪除
│                            [儲存]│
└─────────────────────────────────┘
```

### 色彩分類方案

| 顏色 | 用途建議 | CSS 變數 |
|------|---------|----------|
| 🟡 黃色（預設） | 一般備忘 | `#fef9c3` / `#a16207` |
| 🟢 綠色 | 完成項目 / 已確認 | `#dcfce7` / `#15803d` |
| 🔵 藍色 | 參考資訊 | `#dbeafe` / `#1d4ed8` |
| 🔴 紅色 | 緊急 / 重要 | `#fee2e2` / `#b91c1c` |

### 資料實體設計

```csharp
// Data/Entities/PersonalTools/StickyNote.cs
public class StickyNote
{
    public int Id { get; set; }

    // 所有者（每位使用者有自己的便條）
    public int EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;

    // 內容
    public string Content { get; set; } = string.Empty;  // 便條文字（最多 1000 字）

    // 顏色（Yellow / Green / Blue / Red）
    public StickyNoteColor Color { get; set; } = StickyNoteColor.Yellow;

    // 可選：標記與哪個模組相關（未來擴展用）
    public string? ModuleTag { get; set; }  // e.g., "Purchase", "Sales"

    // 時間戳記
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // 排序（拖曳排序用，預留）
    public int SortOrder { get; set; }
}

public enum StickyNoteColor
{
    Yellow = 1,
    Green = 2,
    Blue = 3,
    Red = 4
}
```

### 服務層介面

```csharp
// Services/PersonalTools/IStickyNoteService.cs
public interface IStickyNoteService
{
    Task<List<StickyNote>> GetByEmployeeIdAsync(int employeeId);
    Task<StickyNote> CreateAsync(int employeeId, string content, StickyNoteColor color);
    Task<StickyNote> UpdateAsync(int noteId, string content, StickyNoteColor color);
    Task DeleteAsync(int noteId, int employeeId);  // 確保只能刪除自己的便條
    Task<List<StickyNote>> SearchAsync(int employeeId, string keyword);
}
```

---

## 功能二：行事曆（Calendar）

### 版本規劃

| 版本 | 範圍 | 說明 |
|------|------|------|
| **Phase 1（初版）** | 個人事項（手動新增） | 先做好基礎 CRUD，不整合 ERP 資料 |
| **Phase 2（進階）** | ERP 資料整合 | 自動顯示採購到貨日、銷售訂單截止、訂單審核期限等 |

### UX 流程（Phase 1）

```
使用者點擊 QuickActionMenu「行事曆」
        ↓
右側面板滑入（CalendarDrawer.razor）
        ↓
┌──────────────────────────────────┐
│ 📅 行事曆                   [×]  │
├──────────────────────────────────┤
│ ◀  2026 年 3 月  ▶  [月][週]    │
│                                  │
│  日  一  二  三  四  五  六      │
│   1   2   3  [4]  5   6   7     │  ← [4] = 今天，高亮顯示
│   8   9  10  11  12  13  14     │
│  15  16  17  18  19  20  21     │  ← 有事項的日期顯示小點 ●
│  22  23  24  25  26  27  28     │
│  29  30  31                     │
├──────────────────────────────────┤
│ 今日（3/4 週三）                 │
│ ● 09:00  部門會議        [🔵]   │
│ ● 14:00  供應商來訪      [🟡]   │
│                                  │
│ 明日（3/5 週四）                 │
│ ● 全天   採購單截止      [🔴]   │
│                                  │
│ [+ 新增事項]                     │
└──────────────────────────────────┘
```

### 新增事項（行內表單）

```
點擊 [+ 新增事項] 展開快速表單：
┌──────────────────────────────────┐
│ 事項名稱 [________________]      │
│ 日期     [2026/03/04]            │
│ 時間     [09:00] (可選，空白=全天) │
│ 顏色     🟡 ○ 🟢 ○ 🔵 ○ 🔴 ○   │
│                   [取消]  [儲存] │
└──────────────────────────────────┘
```

### 月/週視圖切換

| 視圖 | 適用情境 | 面板中的呈現 |
|------|---------|-------------|
| **月視圖** | 查看整月分佈 | 迷你月曆（6行 × 7列），有事項日期顯示小點 |
| **週視圖** | 查看本週詳細時程 | 7欄時間格（每欄代表一天），較直覺 |

### 資料實體設計

```csharp
// Data/Entities/PersonalTools/CalendarEvent.cs
public class CalendarEvent
{
    public int Id { get; set; }

    // 所有者
    public int EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;

    // 事項內容
    public string Title { get; set; } = string.Empty;  // 事項名稱（最多 200 字）
    public string? Description { get; set; }           // 詳細說明（預留）

    // 時間
    public DateOnly EventDate { get; set; }    // 事項日期
    public TimeOnly? EventTime { get; set; }   // 事項時間（null = 全天事項）

    // 分類與顏色
    public CalendarEventColor Color { get; set; } = CalendarEventColor.Blue;
    public CalendarEventType EventType { get; set; } = CalendarEventType.Personal;

    // Phase 2 預留：ERP 資料來源（若來自系統自動建立）
    public string? SourceModule { get; set; }  // e.g., "PurchaseOrder"
    public int? SourceId { get; set; }          // e.g., PurchaseOrderId

    // 時間戳記
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public enum CalendarEventColor
{
    Yellow = 1,  // 一般
    Green = 2,   // 已完成
    Blue = 3,    // 參考
    Red = 4      // 緊急
}

public enum CalendarEventType
{
    Personal = 1,  // 手動新增（Phase 1）
    System = 2     // ERP 系統自動（Phase 2）
}
```

### 服務層介面

```csharp
// Services/PersonalTools/ICalendarEventService.cs
public interface ICalendarEventService
{
    // Phase 1：個人事項
    Task<List<CalendarEvent>> GetByMonthAsync(int employeeId, int year, int month);
    Task<List<CalendarEvent>> GetUpcomingAsync(int employeeId, int days = 7);
    Task<CalendarEvent> CreateAsync(int employeeId, string title, DateOnly date, TimeOnly? time, CalendarEventColor color);
    Task<CalendarEvent> UpdateAsync(int eventId, int employeeId, string title, DateOnly date, TimeOnly? time, CalendarEventColor color);
    Task DeleteAsync(int eventId, int employeeId);

    // Phase 2（預留）：ERP 整合事項
    // Task SyncPurchaseOrderDeadlines(int employeeId);
    // Task SyncSalesOrderDeadlines(int employeeId);
}
```

---

## 元件架構設計

### 檔案結構

```
Components/
└── Shared/
    └── PersonalTools/                    ← 新增資料夾
        ├── StickyNoteDrawer.razor        ← 便條貼面板
        ├── StickyNoteDrawer.razor.css    ← 面板樣式
        ├── CalendarDrawer.razor          ← 行事曆面板
        └── CalendarDrawer.razor.css      ← 面板樣式

Data/
└── Entities/
    └── PersonalTools/                    ← 新增資料夾
        ├── StickyNote.cs
        └── CalendarEvent.cs

Services/
└── PersonalTools/                        ← 新增資料夾
    ├── IStickyNoteService.cs
    ├── StickyNoteService.cs
    ├── ICalendarEventService.cs
    └── CalendarEventService.cs

Migrations/
└── XXXXXXXX_AddPersonalToolsTables.cs    ← 新增 Migration
```

### 整合方式（MainLayout.razor）

便條貼和行事曆面板宣告於 `MainLayout.razor`，與 PageSearch、ShortcutKeys 等現有全域元件平行管理：

```razor
@* 既有元件（保持不變）*@
<GenericSearchModalComponent @ref="pageSearchModal" ... />
<QuickActionMenu OnPageSearchClick="@OpenPageSearch"
                 OnShortcutKeysClick="@OpenShortcutKeys"
                 OnStickyNotesClick="@OpenStickyNotes"     @* 新增 *@
                 OnCalendarClick="@OpenCalendar" />         @* 新增 *@

@* 新增：便利性功能面板 *@
<StickyNoteDrawer IsVisible="@showStickyNotes"
                  IsVisibleChanged="@((bool v) => showStickyNotes = v)" />

<CalendarDrawer IsVisible="@showCalendar"
                IsVisibleChanged="@((bool v) => showCalendar = v)" />
```

```csharp
// MainLayout.razor @code 新增
private bool showStickyNotes = false;
private bool showCalendar = false;

private void OpenStickyNotes()
{
    showCalendar = false;   // 關閉另一個面板（互斥）
    showStickyNotes = true;
    StateHasChanged();
}

private void OpenCalendar()
{
    showStickyNotes = false;  // 關閉另一個面板（互斥）
    showCalendar = true;
    StateHasChanged();
}
```

### QuickActionMenu.razor 新增項目

```razor
@* 便條貼 *@
<button class="quick-action-item"
        @onclick="HandleStickyNotesClick"
        title="@L["QuickAction.StickyNotesTooltip"]">
    <span class="bi bi-sticky"></span>
    <span class="quick-action-label">@L["QuickAction.StickyNotes"]</span>
</button>

@* 行事曆 *@
<button class="quick-action-item"
        @onclick="HandleCalendarClick"
        title="@L["QuickAction.CalendarTooltip"]">
    <span class="bi bi-calendar3"></span>
    <span class="quick-action-label">@L["QuickAction.Calendar"]</span>
</button>
```

```csharp
[Parameter] public EventCallback OnStickyNotesClick { get; set; }
[Parameter] public EventCallback OnCalendarClick { get; set; }

private async Task HandleStickyNotesClick()
{
    CloseMenu();
    if (OnStickyNotesClick.HasDelegate)
        await OnStickyNotesClick.InvokeAsync();
}

private async Task HandleCalendarClick()
{
    CloseMenu();
    if (OnCalendarClick.HasDelegate)
        await OnCalendarClick.InvokeAsync();
}
```

---

## 快捷鍵設計

| 快捷鍵 | 功能 | 備註 |
|--------|------|------|
| `Alt + N` | 開啟 / 關閉便條貼面板 | N = Note |
| `Alt + C` | 開啟 / 關閉行事曆面板 | C = Calendar |
| `ESC` | 關閉目前開啟的面板 | 與現有 ESC 行為一致 |

快捷鍵需加入 `ShortcutKeysModalComponent.razor` 的說明清單，分類為「便利性工具」。

---

## 多語言鍵值（需加入所有 5 個 resx 檔）

### 便條貼相關

| Key | zh-TW | en-US | ja-JP | zh-CN | fil |
|-----|-------|-------|-------|-------|-----|
| `QuickAction.StickyNotes` | 便條貼 | Sticky Notes | 付箋 | 便条贴 | Sticky Notes |
| `QuickAction.StickyNotesTooltip` | 開啟便條貼 | Open Sticky Notes | 付箋を開く | 打开便条贴 | Buksan ang Sticky Notes |
| `StickyNote.Title` | 便條貼 | Sticky Notes | 付箋 | 便条贴 | Sticky Notes |
| `StickyNote.NewNote` | 新增便條 | New Note | 新しい付箋 | 新建便条 | Bagong Tala |
| `StickyNote.SearchPlaceholder` | 搜尋便條... | Search notes... | 付箋を検索... | 搜索便条... | Maghanap ng tala... |
| `StickyNote.EmptyMessage` | 尚無便條，點擊「新增」開始記錄 | No notes yet | メモがありません | 暂无便条 | Walang tala pa |
| `StickyNote.DeleteConfirm` | 確定要刪除此便條嗎？ | Delete this note? | この付箋を削除しますか？ | 确定删除此便条？ | Burahin ang talang ito? |
| `StickyNote.SaveSuccess` | 便條已儲存 | Note saved | 付箋を保存しました | 便条已保存 | Na-save ang tala |
| `StickyNote.Placeholder` | 在此輸入備忘內容... | Enter note here... | ここにメモを入力... | 在此输入备忘内容... | Ilagay ang tala dito... |

### 行事曆相關

| Key | zh-TW | en-US | ja-JP | zh-CN | fil |
|-----|-------|-------|-------|-------|-----|
| `QuickAction.Calendar` | 行事曆 | Calendar | カレンダー | 日历 | Kalendaryo |
| `QuickAction.CalendarTooltip` | 開啟行事曆 | Open Calendar | カレンダーを開く | 打开日历 | Buksan ang Kalendaryo |
| `Calendar.Title` | 行事曆 | Calendar | カレンダー | 日历 | Kalendaryo |
| `Calendar.NewEvent` | 新增事項 | New Event | 新しい予定 | 新增事项 | Bagong Kaganapan |
| `Calendar.Today` | 今日 | Today | 今日 | 今日 | Ngayon |
| `Calendar.AllDay` | 全天 | All Day | 終日 | 全天 | Buong Araw |
| `Calendar.MonthView` | 月 | Month | 月 | 月 | Buwan |
| `Calendar.WeekView` | 週 | Week | 週 | 周 | Linggo |
| `Calendar.EventTitle` | 事項名稱 | Event Title | タイトル | 事项名称 | Pamagat ng Kaganapan |
| `Calendar.EventDate` | 日期 | Date | 日付 | 日期 | Petsa |
| `Calendar.EventTime` | 時間（空白=全天） | Time (empty = all day) | 時間（空=終日） | 时间（空=全天） | Oras (blangko = buong araw) |
| `Calendar.EmptyDay` | 今日無事項 | No events today | 今日の予定はありません | 今日无事项 | Walang kaganapan ngayon |
| `Calendar.DeleteConfirm` | 確定要刪除此事項嗎？ | Delete this event? | この予定を削除しますか？ | 确定删除此事项？ | Burahin ang kaganapang ito? |

### 快捷鍵說明

| Key | zh-TW | en-US |
|-----|-------|-------|
| `Shortcut.StickyNotes` | 開啟便條貼 | Open Sticky Notes |
| `Shortcut.Calendar` | 開啟行事曆 | Open Calendar |

---

## DB Migration 規劃

```csharp
// 新增兩張資料表（一次 Migration）
public partial class AddPersonalToolsTables : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // StickyNotes 資料表
        migrationBuilder.CreateTable(
            name: "StickyNotes",
            columns: table => new
            {
                Id = table.Column<int>(nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                EmployeeId = table.Column<int>(nullable: false),
                Content = table.Column<string>(maxLength: 1000, nullable: false),
                Color = table.Column<int>(nullable: false, defaultValue: 1),
                ModuleTag = table.Column<string>(maxLength: 50, nullable: true),
                SortOrder = table.Column<int>(nullable: false, defaultValue: 0),
                CreatedAt = table.Column<DateTime>(nullable: false),
                UpdatedAt = table.Column<DateTime>(nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_StickyNotes", x => x.Id);
                table.ForeignKey("FK_StickyNotes_Employees_EmployeeId",
                    x => x.EmployeeId, "Employees", "Id", onDelete: ReferentialAction.Cascade);
            });

        // CalendarEvents 資料表
        migrationBuilder.CreateTable(
            name: "CalendarEvents",
            columns: table => new
            {
                Id = table.Column<int>(nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                EmployeeId = table.Column<int>(nullable: false),
                Title = table.Column<string>(maxLength: 200, nullable: false),
                Description = table.Column<string>(maxLength: 2000, nullable: true),
                EventDate = table.Column<DateOnly>(nullable: false),
                EventTime = table.Column<TimeOnly>(nullable: true),
                Color = table.Column<int>(nullable: false, defaultValue: 3),  // Blue
                EventType = table.Column<int>(nullable: false, defaultValue: 1),  // Personal
                SourceModule = table.Column<string>(maxLength: 50, nullable: true),
                SourceId = table.Column<int>(nullable: true),
                CreatedAt = table.Column<DateTime>(nullable: false),
                UpdatedAt = table.Column<DateTime>(nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_CalendarEvents", x => x.Id);
                table.ForeignKey("FK_CalendarEvents_Employees_EmployeeId",
                    x => x.EmployeeId, "Employees", "Id", onDelete: ReferentialAction.Cascade);
            });

        // 索引（加速依使用者與日期查詢）
        migrationBuilder.CreateIndex("IX_StickyNotes_EmployeeId", "StickyNotes", "EmployeeId");
        migrationBuilder.CreateIndex("IX_CalendarEvents_EmployeeId_EventDate", "CalendarEvents",
            new[] { "EmployeeId", "EventDate" });
    }
}
```

---

## z-index 層級規劃

現有系統的 z-index 分配：

| 元素 | z-index |
|------|---------|
| Modal | 1055 |
| Modal 遮罩 | 1056 |
| QuickActionMenu 遮罩 | 1056 |
| QuickActionMenu 項目 | 1058 |
| QuickActionMenu 主按鈕 | 1059 |

新增面板的 z-index 建議：

| 元素 | z-index | 理由 |
|------|---------|------|
| 便條貼 / 行事曆面板遮罩 | 1050 | 低於 Modal，避免遮蓋業務 Modal |
| 便條貼 / 行事曆面板 | 1051 | 低於 Modal，若有 Modal 開啟面板仍可見但退後 |

> **設計決策**：面板 z-index 刻意設定低於 Modal（1055），讓使用者在面板開啟時還能操作業務 Modal（例如：開著行事曆面板同時新增採購單）。若面板需要遮蓋 Modal，則需提高至 1060+，但這會影響使用者同時使用兩個工具的體驗。

---

## 實作順序建議

### Phase 1：便條貼（建議優先）

```
Step 1: DB 設計
    → 建立 StickyNote 實體
    → 設定 EF Core 關聯（Employee → StickyNotes）
    → 建立並執行 Migration

Step 2: 服務層
    → IStickyNoteService + StickyNoteService
    → 在 Program.cs 註冊

Step 3: UI 元件
    → StickyNoteDrawer.razor + .css
    → 右側滑入動畫
    → 便條列表（顏色、內容、時間）
    → 行內編輯（點擊展開 textarea）
    → 顏色選擇器

Step 4: 整合
    → QuickActionMenu 新增按鈕與 callback
    → MainLayout 宣告面板元件
    → Alt+N 快捷鍵
    → ShortcutKeysModal 更新說明

Step 5: 多語言
    → 加入所有 resx 鍵值（9 keys × 5 languages）
```

### Phase 2：行事曆

```
Step 1: DB 設計
    → CalendarEvent 實體加入同一 Migration
    → 或新增獨立 Migration

Step 2: 服務層
    → ICalendarEventService + CalendarEventService

Step 3: UI 元件
    → CalendarDrawer.razor + .css
    → 迷你月曆（自製或輕量函式庫）
    → 議程清單（今日 + 近7天）
    → 快速新增表單（行內展開）
    → 月/週視圖切換

Step 4: 整合（同便條貼步驟 4）

Step 5: 多語言（14 keys × 5 languages）
```

### Phase 3（未來）：ERP 整合

```
→ CalendarEventService 新增 SyncFromERP() 方法
→ 掛鉤至採購單、銷售訂單的儲存事件
→ 自動建立 EventType = System 的事項
→ 月曆上以不同樣式顯示系統事項（不可手動刪除）
```

---

## 設計限制與注意事項

1. **便條貼字數上限**：建議 1000 字，並在 UI 顯示剩餘字數（使用現有的 `CharacterCountTextAreaComponent`）

2. **便條數量上限**：建議每位使用者上限 100 則，避免效能問題（超過時提示使用者清理舊便條）

3. **行事曆月曆元件**：Blazor 無內建月曆，需自製或引入輕量函式庫。建議**自製迷你月曆**以保持設計一致性，實作並不複雜（6行 × 7列 Grid）

4. **深色模式**：面板的 CSS 必須使用 CSS 變數（`var(--bg-primary)`），不可使用硬碼顏色，確保深色主題正確運作

5. **ESC 鍵衝突**：面板的 ESC 監聽需在面板開啟時才啟用，且需與 BaseModalComponent 的 ESC 監聽機制協調，避免同時觸發

6. **便條貼顏色深色模式**：便條顏色在深色模式下需使用深色版本（背景更深、文字更亮），而非直接套用淺色版的顏色

---

## 相關文件

- [README_快捷鍵設計.md](../共用元件設計/README_快捷鍵設計.md) — 快捷鍵整合架構
- [README_個人化設定總綱.md](個人化設定/README_個人化設定總綱.md) — 個人化設定架構
- [README_系統功能設計總綱.md](README_系統功能設計總綱.md) — 系統功能總覽
- `Components/Shared/QuickAction/QuickActionMenu.razor` — QuickAction 主選單
- `Components/Layout/MainLayout.razor` — 全域元件整合入口
