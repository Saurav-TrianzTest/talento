@echo off
setlocal enabledelayedexpansion

echo === Deploy Image to AWS EKS: trianz-enterprise-operations ===

REM Ask for AWS region
set /p AWS_REGION="Enter AWS Region (e.g., us-east-1): "

REM Ask for EKS cluster name
set /p CLUSTER_NAME="Enter EKS Cluster Name: "

REM Ask for image URI
set /p IMAGE_URI="Enter full Image URI (registry/repo:tag): "

REM Ask for environment variables
echo.
echo Enter values for environment variables (press Enter to skip):

set /p ITS_OPS="Enter value for ITS-OPS (or press Enter to skip): "
set /p ASSURANCE="Enter value for Assurance (or press Enter to skip): "
set /p CLUSTER_MASTER="Enter value for ClusterMaster (or press Enter to skip): "
set /p BUSINESS_GROUP_ID="Enter value for businessGroupId (or press Enter to skip): "
set /p MANAGER_ID="Enter value for ManagerId (or press Enter to skip): "
set /p PH_ID="Enter value for PHId (or press Enter to skip): "
set /p CREATE_TR_RL="Enter value for CreateTR-RL (or press Enter to skip): "
set /p MAIL_RL="Enter value for Mail-RL (or press Enter to skip): "
set /p ONLY_TR_VISIBLE="Enter value for OnlyTRVisibleEmployee (or press Enter to skip): "
set /p CLUSTER_LEAD_VIEW="Enter value for ClusterLeadview (or press Enter to skip): "
set /p CLUSTER_MANAGER="Enter value for clustermanager (or press Enter to skip): "
set /p PERCENTAGE="Enter value for Percentage (or press Enter to skip): "
set /p IS_EMAIL_SENT="Enter value for IsEmailSent (or press Enter to skip): "
set /p ON_SITE_COUNTRY="Enter value for OnSiteCountryName (or press Enter to skip): "
set /p ONSITE_EMP_ID="Enter value for OnsiteEmployeeId (or press Enter to skip): "
set /p ITS_DASHBOARD_VIEWERS="Enter value for ITSDashboardViewers (or press Enter to skip): "
set /p DH_RL_ROLE_USERS="Enter value for DHAndRLRoleUsers (or press Enter to skip): "
set /p HOST_NO="Enter value for HostNo (or press Enter to skip): "
set /p EMAIL_FROM="Enter value for EmailFrom (or press Enter to skip): "
set /p TR_RESTRICTED="Enter value for TRRestricted (or press Enter to skip): "
set /p ACTIVE_ASSIGN_DOWNLOAD="Enter value for ActiveAssignmentDownload (or press Enter to skip): "
set /p RISE_TEAM="Enter value for RiseTeam (or press Enter to skip): "
set /p OPS_STATUS="Enter value for OpsStatus (or press Enter to skip): "
set /p DEFAULT_ROLE="Enter value for DefaultRole (or press Enter to skip): "
set /p ROLE_MANAGEMENT="Enter value for RoleManagement (or press Enter to skip): "
set /p PORT_NO="Enter value for PortNo (or press Enter to skip): "
set /p TALENTO="Enter value for Talento (or press Enter to skip): "
set /p EMPLOYEE_URL="Enter value for EmployeeURL (or press Enter to skip): "
set /p NUMBERING_CLI_URL="Enter value for NumberingCliURL (or press Enter to skip): "
set /p HRRF_CLI_URL="Enter value for HRRFCliUrl (or press Enter to skip): "
set /p HRRF_NUMBERING_CLI_URL="Enter value for HRRFNumberingCliUrl (or press Enter to skip): "
set /p NOTIFICATION_CLI_URL="Enter value for NotificationCliUrl (or press Enter to skip): "
set /p GET_OPPORTUNITY="Enter value for GetOpportunityByNameorCode (or press Enter to skip): "
set /p GET_LOOKUP_SERVICE="Enter value for GetLookupService (or press Enter to skip): "
set /p GET_EMPLOYEE_SERVICE="Enter value for GetEmployeeService (or press Enter to skip): "
set /p CLIENT_ID="Enter value for ClientId (or press Enter to skip): "
set /p TENANT="Enter value for Tenant (or press Enter to skip): "
set /p AUTHORITY="Enter value for Authority (or press Enter to skip): "
set /p REDIRECT_URI="Enter value for redirectUri (or press Enter to skip): "

REM Ask for credentials
echo.
echo Enter credential values (input will be hidden):
call :get_password DB_CONNECTION_STRING "Enter DB_CONNECTION_STRING: "
call :get_password ADO_CONNECTION_STRING "Enter ADO_CONNECTION_STRING: "
call :get_password REDIS_CONNECTION_STRING "Enter REDIS_CONNECTION_STRING: "

REM Configure kubectl for the EKS cluster
echo Configuring kubectl for EKS cluster...
aws eks update-kubeconfig --region !AWS_REGION! --name !CLUSTER_NAME!
if !ERRORLEVEL! neq 0 (
    echo Failed to configure kubectl
    exit /b 1
)

REM Verify cluster access
echo Verifying cluster access...
kubectl cluster-info
if !ERRORLEVEL! neq 0 (
    echo Failed to access cluster
    exit /b 1
)

REM Apply namespace
echo Applying namespace...
kubectl apply -f kubernetes\namespace.yaml
if !ERRORLEVEL! neq 0 (
    echo Failed to apply namespace
    exit /b 1
)

REM Create Secret for credentials
echo Creating Kubernetes Secret for credentials...
kubectl create secret generic trianz-enterprise-operations-secrets ^
    -n trianz-enterprise-operations ^
    --from-literal="DB_CONNECTION_STRING=!DB_CONNECTION_STRING!" ^
    --from-literal="ADO_CONNECTION_STRING=!ADO_CONNECTION_STRING!" ^
    --from-literal="REDIS_CONNECTION_STRING=!REDIS_CONNECTION_STRING!" ^
    --dry-run=client -o yaml | kubectl apply -f -
if !ERRORLEVEL! neq 0 (
    echo Failed to create secret
    exit /b 1
)

REM Replace placeholders in deployment.yaml
echo Replacing placeholders in deployment.yaml...
powershell -Command "(gc kubernetes\deployment.yaml) -replace '{{IMAGE_URI}}', '!IMAGE_URI!' | Out-File -encoding ASCII kubernetes\deployment.yaml"
powershell -Command "(gc kubernetes\deployment.yaml) -replace '{{ITS-OPS}}', '!ITS_OPS!' | Out-File -encoding ASCII kubernetes\deployment.yaml"
powershell -Command "(gc kubernetes\deployment.yaml) -replace '{{Assurance}}', '!ASSURANCE!' | Out-File -encoding ASCII kubernetes\deployment.yaml"
powershell -Command "(gc kubernetes\deployment.yaml) -replace '{{ClusterMaster}}', '!CLUSTER_MASTER!' | Out-File -encoding ASCII kubernetes\deployment.yaml"
powershell -Command "(gc kubernetes\deployment.yaml) -replace '{{businessGroupId}}', '!BUSINESS_GROUP_ID!' | Out-File -encoding ASCII kubernetes\deployment.yaml"
powershell -Command "(gc kubernetes\deployment.yaml) -replace '{{ManagerId}}', '!MANAGER_ID!' | Out-File -encoding ASCII kubernetes\deployment.yaml"
powershell -Command "(gc kubernetes\deployment.yaml) -replace '{{PHId}}', '!PH_ID!' | Out-File -encoding ASCII kubernetes\deployment.yaml"
powershell -Command "(gc kubernetes\deployment.yaml) -replace '{{CreateTR-RL}}', '!CREATE_TR_RL!' | Out-File -encoding ASCII kubernetes\deployment.yaml"
powershell -Command "(gc kubernetes\deployment.yaml) -replace '{{Mail-RL}}', '!MAIL_RL!' | Out-File -encoding ASCII kubernetes\deployment.yaml"
powershell -Command "(gc kubernetes\deployment.yaml) -replace '{{OnlyTRVisibleEmployee}}', '!ONLY_TR_VISIBLE!' | Out-File -encoding ASCII kubernetes\deployment.yaml"
powershell -Command "(gc kubernetes\deployment.yaml) -replace '{{ClusterLeadview}}', '!CLUSTER_LEAD_VIEW!' | Out-File -encoding ASCII kubernetes\deployment.yaml"
powershell -Command "(gc kubernetes\deployment.yaml) -replace '{{clustermanager}}', '!CLUSTER_MANAGER!' | Out-File -encoding ASCII kubernetes\deployment.yaml"
powershell -Command "(gc kubernetes\deployment.yaml) -replace '{{Percentage}}', '!PERCENTAGE!' | Out-File -encoding ASCII kubernetes\deployment.yaml"
powershell -Command "(gc kubernetes\deployment.yaml) -replace '{{IsEmailSent}}', '!IS_EMAIL_SENT!' | Out-File -encoding ASCII kubernetes\deployment.yaml"
powershell -Command "(gc kubernetes\deployment.yaml) -replace '{{OnSiteCountryName}}', '!ON_SITE_COUNTRY!' | Out-File -encoding ASCII kubernetes\deployment.yaml"
powershell -Command "(gc kubernetes\deployment.yaml) -replace '{{OnsiteEmployeeId}}', '!ONSITE_EMP_ID!' | Out-File -encoding ASCII kubernetes\deployment.yaml"
powershell -Command "(gc kubernetes\deployment.yaml) -replace '{{ITSDashboardViewers}}', '!ITS_DASHBOARD_VIEWERS!' | Out-File -encoding ASCII kubernetes\deployment.yaml"
powershell -Command "(gc kubernetes\deployment.yaml) -replace '{{DHAndRLRoleUsers}}', '!DH_RL_ROLE_USERS!' | Out-File -encoding ASCII kubernetes\deployment.yaml"
powershell -Command "(gc kubernetes\deployment.yaml) -replace '{{HostNo}}', '!HOST_NO!' | Out-File -encoding ASCII kubernetes\deployment.yaml"
powershell -Command "(gc kubernetes\deployment.yaml) -replace '{{EmailFrom}}', '!EMAIL_FROM!' | Out-File -encoding ASCII kubernetes\deployment.yaml"
powershell -Command "(gc kubernetes\deployment.yaml) -replace '{{TRRestricted}}', '!TR_RESTRICTED!' | Out-File -encoding ASCII kubernetes\deployment.yaml"
powershell -Command "(gc kubernetes\deployment.yaml) -replace '{{ActiveAssignmentDownload}}', '!ACTIVE_ASSIGN_DOWNLOAD!' | Out-File -encoding ASCII kubernetes\deployment.yaml"
powershell -Command "(gc kubernetes\deployment.yaml) -replace '{{RiseTeam}}', '!RISE_TEAM!' | Out-File -encoding ASCII kubernetes\deployment.yaml"
powershell -Command "(gc kubernetes\deployment.yaml) -replace '{{OpsStatus}}', '!OPS_STATUS!' | Out-File -encoding ASCII kubernetes\deployment.yaml"
powershell -Command "(gc kubernetes\deployment.yaml) -replace '{{DefaultRole}}', '!DEFAULT_ROLE!' | Out-File -encoding ASCII kubernetes\deployment.yaml"
powershell -Command "(gc kubernetes\deployment.yaml) -replace '{{RoleManagement}}', '!ROLE_MANAGEMENT!' | Out-File -encoding ASCII kubernetes\deployment.yaml"
powershell -Command "(gc kubernetes\deployment.yaml) -replace '{{PortNo}}', '!PORT_NO!' | Out-File -encoding ASCII kubernetes\deployment.yaml"
powershell -Command "(gc kubernetes\deployment.yaml) -replace '{{Talento}}', '!TALENTO!' | Out-File -encoding ASCII kubernetes\deployment.yaml"
powershell -Command "(gc kubernetes\deployment.yaml) -replace '{{EmployeeURL}}', '!EMPLOYEE_URL!' | Out-File -encoding ASCII kubernetes\deployment.yaml"
powershell -Command "(gc kubernetes\deployment.yaml) -replace '{{NumberingCliURL}}', '!NUMBERING_CLI_URL!' | Out-File -encoding ASCII kubernetes\deployment.yaml"
powershell -Command "(gc kubernetes\deployment.yaml) -replace '{{HRRFCliUrl}}', '!HRRF_CLI_URL!' | Out-File -encoding ASCII kubernetes\deployment.yaml"
powershell -Command "(gc kubernetes\deployment.yaml) -replace '{{HRRFNumberingCliUrl}}', '!HRRF_NUMBERING_CLI_URL!' | Out-File -encoding ASCII kubernetes\deployment.yaml"
powershell -Command "(gc kubernetes\deployment.yaml) -replace '{{NotificationCliUrl}}', '!NOTIFICATION_CLI_URL!' | Out-File -encoding ASCII kubernetes\deployment.yaml"
powershell -Command "(gc kubernetes\deployment.yaml) -replace '{{GetOpportunityByNameorCode}}', '!GET_OPPORTUNITY!' | Out-File -encoding ASCII kubernetes\deployment.yaml"
powershell -Command "(gc kubernetes\deployment.yaml) -replace '{{GetLookupService}}', '!GET_LOOKUP_SERVICE!' | Out-File -encoding ASCII kubernetes\deployment.yaml"
powershell -Command "(gc kubernetes\deployment.yaml) -replace '{{GetEmployeeService}}', '!GET_EMPLOYEE_SERVICE!' | Out-File -encoding ASCII kubernetes\deployment.yaml"
powershell -Command "(gc kubernetes\deployment.yaml) -replace '{{ClientId}}', '!CLIENT_ID!' | Out-File -encoding ASCII kubernetes\deployment.yaml"
powershell -Command "(gc kubernetes\deployment.yaml) -replace '{{Tenant}}', '!TENANT!' | Out-File -encoding ASCII kubernetes\deployment.yaml"
powershell -Command "(gc kubernetes\deployment.yaml) -replace '{{Authority}}', '!AUTHORITY!' | Out-File -encoding ASCII kubernetes\deployment.yaml"
powershell -Command "(gc kubernetes\deployment.yaml) -replace '{{redirectUri}}', '!REDIRECT_URI!' | Out-File -encoding ASCII kubernetes\deployment.yaml"

REM Apply deployment, service, and ingress
echo Applying deployment...
kubectl apply -f kubernetes\deployment.yaml
if !ERRORLEVEL! neq 0 (
    echo Failed to apply deployment
    exit /b 1
)

echo Applying service...
kubectl apply -f kubernetes\service.yaml
if !ERRORLEVEL! neq 0 (
    echo Failed to apply service
    exit /b 1
)

echo Applying ingress...
kubectl apply -f kubernetes\ingress.yaml
if !ERRORLEVEL! neq 0 (
    echo Failed to apply ingress
    exit /b 1
)

REM Wait for deployment rollout
echo Waiting for deployment rollout...
kubectl rollout status deployment/trianz-enterprise-operations -n trianz-enterprise-operations
if !ERRORLEVEL! neq 0 (
    echo Deployment rollout failed
    exit /b 1
)

REM Show resources
echo === Deployed Resources ===
kubectl get pods,svc,ingress -n trianz-enterprise-operations

REM Show application URL
echo.
echo === Application URL ===
echo Visit: https://trianz-enterprise-operations.example.com

echo.
echo === Deployment Complete ===
goto :eof

:get_password
    set /p "%~2" <nul
    set /p "%~1="
    echo.
    goto :eof
