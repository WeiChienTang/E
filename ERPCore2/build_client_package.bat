@echo off
echo ============================================
echo ERPCore2 自動發布腳本
echo ============================================
echo.

:: 設定發布路徑
set PUBLISH_PATH=%~dp0PublishOutput
set CLIENT_PACKAGE_PATH=%~dp0ClientPackage

echo 正在清理舊的發布檔案...
if exist "%PUBLISH_PATH%" rmdir /s /q "%PUBLISH_PATH%"
if exist "%CLIENT_PACKAGE_PATH%" rmdir /s /q "%CLIENT_PACKAGE_PATH%"

echo 正在發布 Release 版本...
dotnet publish -c Release -r win-x64 --self-contained false -o "%PUBLISH_PATH%"

if errorlevel 1 (
    echo 發布失敗！
    pause
    exit /b 1
)

echo 正在建立客戶部署包...
mkdir "%CLIENT_PACKAGE_PATH%"

:: 複製發布檔案
echo 複製應用程式檔案...
xcopy "%PUBLISH_PATH%\*" "%CLIENT_PACKAGE_PATH%\Application\" /E /I /H /Y

:: 複製部署相關檔案
echo 複製部署檔案...
copy "客戶部署指南.md" "%CLIENT_PACKAGE_PATH%\"
copy "deploy.bat" "%CLIENT_PACKAGE_PATH%\"
copy "appsettings.Production.json" "%CLIENT_PACKAGE_PATH%\Application\"
copy "智慧財產權保護說明.md" "%CLIENT_PACKAGE_PATH%\"

:: 建立啟動腳本
echo @echo off > "%CLIENT_PACKAGE_PATH%\Application\Start_ERPCore2.bat"
echo echo 正在啟動 ERPCore2 系統... >> "%CLIENT_PACKAGE_PATH%\Application\Start_ERPCore2.bat"
echo echo 請在瀏覽器開啟 http://localhost:5000 >> "%CLIENT_PACKAGE_PATH%\Application\Start_ERPCore2.bat"
echo echo 或 http://電腦IP:5000 (區域網路存取) >> "%CLIENT_PACKAGE_PATH%\Application\Start_ERPCore2.bat"
echo dotnet ERPCore2.dll --urls "http://0.0.0.0:5000" >> "%CLIENT_PACKAGE_PATH%\Application\Start_ERPCore2.bat"

:: 建立停止腳本
echo @echo off > "%CLIENT_PACKAGE_PATH%\Application\Stop_ERPCore2.bat"
echo echo 正在停止 ERPCore2 系統... >> "%CLIENT_PACKAGE_PATH%\Application\Stop_ERPCore2.bat"
echo taskkill /IM dotnet.exe /F >> "%CLIENT_PACKAGE_PATH%\Application\Stop_ERPCore2.bat"
echo echo 系統已停止 >> "%CLIENT_PACKAGE_PATH%\Application\Stop_ERPCore2.bat"
echo pause >> "%CLIENT_PACKAGE_PATH%\Application\Stop_ERPCore2.bat"

echo.
echo ✅ 客戶部署包建立完成！
echo 📁 位置：%CLIENT_PACKAGE_PATH%
echo.
echo 下一步：將 ClientPackage 資料夾壓縮後提供給客戶
pause
