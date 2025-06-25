@echo off
echo ============================================
echo ERPCore2 自動部署腳本
echo ============================================
echo.

:: 設定變數
set PUBLISH_PATH=%~dp0PublishOutput
set TARGET_PATH=C:\ERPCore2
set BACKUP_PATH=C:\ERPCore2_Backup_%date:~0,4%%date:~5,2%%date:~8,2%

echo 正在準備部署環境...

:: 檢查是否已存在舊版本，若存在則備份
if exist "%TARGET_PATH%" (
    echo 發現現有安裝，正在備份到 %BACKUP_PATH%
    xcopy "%TARGET_PATH%" "%BACKUP_PATH%\" /E /I /H /Y
    if errorlevel 1 (
        echo 備份失敗！
        pause
        exit /b 1
    )
    echo 備份完成
)

:: 停止現有服務（如果正在運行）
echo 正在停止現有服務...
tasklist /FI "IMAGENAME eq ERPCore2.exe" 2>NUL | find /I /N "ERPCore2.exe">NUL
if "%ERRORLEVEL%"=="0" (
    taskkill /IM ERPCore2.exe /F
    timeout /t 3
)

:: 建立目標目錄
if not exist "%TARGET_PATH%" mkdir "%TARGET_PATH%"

:: 複製檔案
echo 正在部署新版本...
xcopy "%PUBLISH_PATH%\*" "%TARGET_PATH%\" /E /I /H /Y
if errorlevel 1 (
    echo 部署失敗！
    pause
    exit /b 1
)

:: 設定權限
echo 正在設定權限...
icacls "%TARGET_PATH%" /grant "IIS_IUSRS:(F)" /T

:: 詢問是否要啟動應用程式
echo.
echo 部署完成！
set /p START_APP=是否要立即啟動應用程式？ (Y/N): 
if /i "%START_APP%"=="Y" (
    echo 正在啟動 ERPCore2...
    cd /d "%TARGET_PATH%"
    start "" "ERPCore2.exe"
    echo 應用程式已啟動
    echo 請在瀏覽器中開啟 http://localhost:5000
)

echo.
echo 部署完成！
pause
