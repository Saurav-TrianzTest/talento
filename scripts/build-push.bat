@echo off
setlocal enabledelayedexpansion

echo === Build and Push Script for trianz-enterprise-operations ===

REM Ask for registry type
echo Select target registry:
echo 1. AWS ECR
echo 2. Docker Hub
set /p REGISTRY_CHOICE="Enter choice (1 or 2): "

REM Ask for image tag
set /p IMAGE_TAG="Enter IMAGE_TAG (press Enter for 'latest'): "

REM Sanitize project name
set PROJECT_NAME=Trianz.Enterprise.Operations
for /f "usebackq delims=" %%i in (`powershell -Command "$p = '%PROJECT_NAME%' -replace '[^a-zA-Z0-9]', '-'; $p = $p.ToLower(); $p = $p.Trim('-'); Write-Output $p"`) do set IMAGE_NAME=%%i

REM Default tag to latest if empty
if "!IMAGE_TAG!"=="" (
    set IMAGE_TAG=latest
)

REM Sanitize tag
for /f "usebackq delims=" %%i in (`powershell -Command "$t = '!IMAGE_TAG!' -replace '[^a-zA-Z0-9._-]', '-'; $t = $t.Trim('-'); Write-Output $t"`) do set IMAGE_TAG=%%i

if "!REGISTRY_CHOICE!"=="1" (
    REM AWS ECR
    set /p AWS_REGION="Enter AWS Region (e.g., us-east-1): "
    set /p REGISTRY_URL="Enter AWS ECR Registry URL (e.g., 123456789012.dkr.ecr.us-east-1.amazonaws.com): "
    set /p ECR_REPO="Enter ECR Repository Name: "

    REM Login to ECR
    echo Logging in to AWS ECR...
    aws ecr get-login-password --region !AWS_REGION! ^| docker login --username AWS --password-stdin !REGISTRY_URL!
    if !ERRORLEVEL! neq 0 (
        echo ECR login failed
        exit /b 1
    )

    REM Create repository if it doesn't exist
    echo Checking if ECR repository exists...
    aws ecr describe-repositories --repository-names !ECR_REPO! --region !AWS_REGION! >nul 2>&1
    if !ERRORLEVEL! neq 0 (
        echo Creating ECR repository...
        aws ecr create-repository --repository-name !ECR_REPO! --region !AWS_REGION!
    )

    set FULL_IMAGE_NAME=!REGISTRY_URL!/!ECR_REPO!:!IMAGE_TAG!
) else (
    REM Docker Hub
    set /p DOCKER_USERNAME="Enter Docker Hub Username: "
    call :get_password
    echo Logging in to Docker Hub...
    echo !DOCKER_PASSWORD! | docker login --username !DOCKER_USERNAME! --password-stdin
    if !ERRORLEVEL! neq 0 (
        echo Docker Hub login failed
        exit /b 1
    )

    set FULL_IMAGE_NAME=!DOCKER_USERNAME!/!IMAGE_NAME!:!IMAGE_TAG!
)

REM Build the image
echo Building Docker image: !FULL_IMAGE_NAME!
docker build -f Dockerfile -t "!FULL_IMAGE_NAME!" .
if !ERRORLEVEL! neq 0 (
    echo Docker build failed
    exit /b 1
)

REM Push the image
echo Pushing Docker image: !FULL_IMAGE_NAME!
docker push "!FULL_IMAGE_NAME!"
if !ERRORLEVEL! neq 0 (
    echo Docker push failed
    exit /b 1
)

echo === Build and Push Complete ===
echo Image: !FULL_IMAGE_NAME!
goto :eof

:get_password
    set /p "DOCKER_PASSWORD=Enter Docker Hub Password: " <nul
    echo.
    goto :eof
