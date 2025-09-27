@echo off
setlocal

set IMAGE_NAME=myapp
set TAG=latest
set CONFIGURATION=Release

if "%1" neq "" set IMAGE_NAME=%1
if "%2" neq "" set TAG=%2

echo.
echo ===== شروع فرآیند ساخت و Docker =====
echo.

REM Clean previous publish
if exist ".\publish" (
    echo حذف دایرکتوری publish قبلی...
    rmdir /s /q ".\publish"
)

REM Restore packages برای linux-x64
echo بازگردانی پکیج‌ها برای linux-x64...
dotnet restore --runtime linux-x64
if errorlevel 1 (
    echo خطا در restore پکیج‌ها
    exit /b 1
)

REM Build and publish with target framework matching container
echo ساخت و انتشار پروژه...
dotnet publish -c %CONFIGURATION% -o .\publish --runtime linux-x64 --self-contained false
if errorlevel 1 (
    echo خطا در publish پروژه
    exit /b 1
)

REM Build Docker image
echo ساخت Docker image...
docker build -f Dockerfile.optimized -t %IMAGE_NAME%:%TAG% .
if errorlevel 1 (
    echo خطا در ساخت Docker image
    exit /b 1
)

echo.
echo ✅ فرآیند با موفقیت تکمیل شد!
echo Docker image: %IMAGE_NAME%:%TAG%
echo.

REM Show image size
echo اندازه image:
docker images %IMAGE_NAME%:%TAG% --format "table {{.Repository}}\t{{.Tag}}\t{{.Size}}"

endlocal