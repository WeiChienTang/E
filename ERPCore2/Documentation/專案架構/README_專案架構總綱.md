# 專案架構總綱

## 更新日期
2026-02-17

---

## 概述

本資料夾收錄 ERPCore2 專案基礎架構的設計文件，涵蓋 Helpers 結構、Models 命名規範、AppDbContext 設定、Seed Data 管理等底層架構。

---

## 文件導覽

| 文件 | 說明 |
|------|------|
| [README_Helpers結構圖.md](README_Helpers結構圖.md) | Helpers 與 Components/Shared 的區分、資料夾結構、新增 Helper 決策流程 |
| [README_Models使用總綱.md](README_Models使用總綱.md) | Models 命名規範（Dto、Config、Criteria、Info、Item）、資料夾結構、GlobalUsings |
| [README_Context設計.md](README_Context設計.md) | AppDbContext Entity-first 設定原則、Data Annotations vs Fluent API |
| [README_SeedData管理.md](README_SeedData管理.md) | SeedDataManager 系統、7 個 Seeder（Order 0-6）、IDataSeeder 介面 |

---

## 相關文件

- [README_完整頁面設計總綱.md](../完整頁面設計/README_完整頁面設計總綱.md) - 五層頁面架構
- [README_報表系統總綱.md](../報表系統/README_報表系統總綱.md) - 報表系統
- [README_共用元件設計總綱.md](../共用元件設計/README_共用元件設計總綱.md) - 共用元件
- [README_系統功能設計總綱.md](../系統功能設計/README_系統功能設計總綱.md) - 系統功能
