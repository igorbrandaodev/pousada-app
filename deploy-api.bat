@echo off
echo ==========================================
echo    PousadaApp API Deployment Script
echo ==========================================
echo.

:: Run tests first
echo [1/5] Running tests...
cd api
dotnet test PousadaApp.sln --configuration Release --verbosity minimal

if %ERRORLEVEL% neq 0 (
    echo ERROR: Tests failed! Fix tests before deploying.
    pause
    exit /b 1
)

:: Build and publish the API
echo [2/5] Building API for Linux runtime...
dotnet publish src\PousadaApp.API\PousadaApp.API.csproj --configuration Release --output ..\publish-linux --runtime linux-x64 --self-contained false

if %ERRORLEVEL% neq 0 (
    echo ERROR: Build failed!
    pause
    exit /b 1
)

cd ..

:: Create deployment package
echo [3/5] Creating deployment package...
if exist pousadaapp-api.zip del pousadaapp-api.zip
powershell -Command "Compress-Archive -Path '.\publish-linux\*' -DestinationPath '.\pousadaapp-api.zip' -Force"

if not exist pousadaapp-api.zip (
    echo ERROR: Failed to create deployment package!
    pause
    exit /b 1
)

:: Deploy to Azure
echo [4/5] Deploying to Azure App Service...
powershell -Command "& az webapp deployment source config-zip --resource-group pousadaapp-rg --name pousadaapp-api --src pousadaapp-api.zip"

if %ERRORLEVEL% neq 0 (
    echo ERROR: Deployment failed!
    echo Make sure you are logged in: az login
    pause
    exit /b 1
)

:: Test the deployment
echo [5/5] Testing deployment...
echo Waiting 10 seconds for deployment to complete...
timeout /t 10 /nobreak > nul

powershell -Command "try { $response = Invoke-WebRequest -Uri 'https://pousadaapp-api.azurewebsites.net/swagger/index.html' -TimeoutSec 30; Write-Host 'SUCCESS: API is running! Status:' $response.StatusCode -ForegroundColor Green } catch { Write-Host 'WARNING: Health check failed -' $_.Exception.Message -ForegroundColor Yellow }"

echo.
echo ==========================================
echo         API Deployment Complete!
echo ==========================================
echo.
echo Cleaning up temporary files...
if exist publish-linux rmdir /s /q publish-linux
if exist pousadaapp-api.zip del pousadaapp-api.zip

echo.
echo Done! Press any key to exit...
pause > nul
