@echo off
echo ==========================================
echo   PousadaApp Full Deployment Script
echo ==========================================
echo.
echo This will deploy both API and Frontend
echo.
pause

:: Deploy API first
echo.
echo ==========================================
echo        Deploying API Backend...
echo ==========================================
call deploy-api.bat

if %ERRORLEVEL% neq 0 (
    echo.
    echo ERROR: API deployment failed!
    echo Stopping deployment process.
    pause
    exit /b 1
)

:: Deploy Frontend
echo.
echo ==========================================
echo        Deploying Frontend...
echo ==========================================
call deploy-frontend.bat

if %ERRORLEVEL% neq 0 (
    echo.
    echo ERROR: Frontend deployment failed!
    pause
    exit /b 1
)

echo.
echo ==========================================
echo    All Deployments Complete!
echo ==========================================
echo.
echo API: Check Azure Portal for URL
echo Frontend: Check Azure Static Web Apps for URL
echo.
echo Done! Press any key to exit...
pause > nul
