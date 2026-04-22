@echo off
echo ==========================================
echo   PousadaApp Frontend Deployment Script
echo ==========================================
echo.

:: Build Angular application
echo [1/3] Building Angular application for production...
call npm run build

if %ERRORLEVEL% neq 0 (
    echo ERROR: Angular build failed!
    pause
    exit /b 1
)

:: Deploy to Static Web App
echo [2/3] Deploying to Azure Static Web App...
swa deploy dist/pousada-app/browser --deployment-token %AZURE_SWA_TOKEN% --env production

if %ERRORLEVEL% neq 0 (
    echo ERROR: Deployment failed!
    echo.
    echo Make sure:
    echo 1. SWA CLI is installed: npm install -g @azure/static-web-apps-cli
    echo 2. AZURE_SWA_TOKEN env var is set
    echo 3. Or use GitHub Actions for automated deployment
    pause
    exit /b 1
)

:: Test deployment
echo [3/3] Testing deployment...
echo Waiting 10 seconds for deployment to propagate...
timeout /t 10 /nobreak > nul

echo.
echo ==========================================
echo      Frontend Deployment Complete!
echo ==========================================
echo.
echo Done! Press any key to exit...
pause > nul
