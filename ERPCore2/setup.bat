@echo off
chcp 65001 >nul
echo ========================================
echo    ERP系統 - 資料庫設定工具
echo ========================================
echo.
echo 此工具將會：
echo 1. 檢查並建立資料庫（如果不存在）
echo 2. 執行所有待處理的資料庫遷移
echo 3. 初始化必要的種子資料
echo.
echo 請確保：
echo - SQL Server 已啟動
echo - appsettings.json 中的連接字串正確
echo.
pause

echo.
echo 正在執行資料庫設定...
echo.

ERPCore2.exe --setup

if %errorlevel% equ 0 (
    echo.
    echo ========================================
    echo 資料庫設定完成！
    echo 現在可以正常啟動 ERPCore2.exe
    echo ========================================
) else (
    echo.
    echo ========================================
    echo 資料庫設定失敗！
    echo 請檢查：
    echo 1. SQL Server 是否正常運行
    echo 2. 連接字串是否正確
    echo 3. 資料庫使用者權限是否足夠
    echo ========================================
)

echo.
pause
