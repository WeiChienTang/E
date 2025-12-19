@echo off
echo Cleaning old publish files...
if exist "publish" rmdir /s /q "publish"

echo Publishing project...
dotnet publish ERPCore2.csproj -c Release -o publish --self-contained true -r win-x64

if %errorlevel% equ 0 (
    echo.
    echo Copying additional files...
    copy /y "setup.bat" "publish\setup.bat" >nul
    
    echo.
    echo ========================================
    echo Publish successful!
    echo Published files location: publish folder
    echo.
    echo 交付給客戶時請告知：
    echo 1. 首次安裝或更新時，先執行 setup.bat
    echo 2. 設定完成後，啟動 ERPCore2.exe
    echo 3. 正常啟動也會自動檢查資料庫版本
    echo ========================================
    pause
) else (
    echo.
    echo Publish failed! Please check error messages.
    pause
)
