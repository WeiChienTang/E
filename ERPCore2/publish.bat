@echo off
echo Cleaning old publish files...
if exist "publish" rmdir /s /q "publish"

echo Publishing project...
dotnet publish -c Release -o publish --self-contained true -r win-x64

if %errorlevel% equ 0 (
    echo.
    echo ========================================
    echo Publish successful!
    echo Published files location: publish folder
    echo Client executable: publish\ERPCore2.exe
    echo ========================================
    pause
) else (
    echo.
    echo Publish failed! Please check error messages.
    pause
)
