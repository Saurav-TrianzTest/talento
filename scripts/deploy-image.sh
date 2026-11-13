#!/bin/bash
set -e
set -o pipefail

echo "=== Deploy Image to AWS EKS: trianz-enterprise-operations ==="

# Ask for AWS region
read -p "Enter AWS Region (e.g., us-east-1): " AWS_REGION

# Ask for EKS cluster name
read -p "Enter EKS Cluster Name: " CLUSTER_NAME

# Ask for image URI
read -p "Enter full Image URI (registry/repo:tag): " IMAGE_URI

# Ask for environment variables
echo ""
echo "Enter values for environment variables (press Enter to skip):"

read -p "Enter value for ITS-OPS (or press Enter to skip): " ITS_OPS
read -p "Enter value for Assurance (or press Enter to skip): " ASSURANCE
read -p "Enter value for ClusterMaster (or press Enter to skip): " CLUSTER_MASTER
read -p "Enter value for businessGroupId (or press Enter to skip): " BUSINESS_GROUP_ID
read -p "Enter value for ManagerId (or press Enter to skip): " MANAGER_ID
read -p "Enter value for PHId (or press Enter to skip): " PH_ID
read -p "Enter value for CreateTR-RL (or press Enter to skip): " CREATE_TR_RL
read -p "Enter value for Mail-RL (or press Enter to skip): " MAIL_RL
read -p "Enter value for OnlyTRVisibleEmployee (or press Enter to skip): " ONLY_TR_VISIBLE
read -p "Enter value for ClusterLeadview (or press Enter to skip): " CLUSTER_LEAD_VIEW
read -p "Enter value for clustermanager (or press Enter to skip): " CLUSTER_MANAGER
read -p "Enter value for Percentage (or press Enter to skip): " PERCENTAGE
read -p "Enter value for IsEmailSent (or press Enter to skip): " IS_EMAIL_SENT
read -p "Enter value for OnSiteCountryName (or press Enter to skip): " ON_SITE_COUNTRY
read -p "Enter value for OnsiteEmployeeId (or press Enter to skip): " ONSITE_EMP_ID
read -p "Enter value for ITSDashboardViewers (or press Enter to skip): " ITS_DASHBOARD_VIEWERS
read -p "Enter value for DHAndRLRoleUsers (or press Enter to skip): " DH_RL_ROLE_USERS
read -p "Enter value for HostNo (or press Enter to skip): " HOST_NO
read -p "Enter value for EmailFrom (or press Enter to skip): " EMAIL_FROM
read -p "Enter value for TRRestricted (or press Enter to skip): " TR_RESTRICTED
read -p "Enter value for ActiveAssignmentDownload (or press Enter to skip): " ACTIVE_ASSIGN_DOWNLOAD
read -p "Enter value for RiseTeam (or press Enter to skip): " RISE_TEAM
read -p "Enter value for OpsStatus (or press Enter to skip): " OPS_STATUS
read -p "Enter value for DefaultRole (or press Enter to skip): " DEFAULT_ROLE
read -p "Enter value for RoleManagement (or press Enter to skip): " ROLE_MANAGEMENT
read -p "Enter value for PortNo (or press Enter to skip): " PORT_NO
read -p "Enter value for Talento (or press Enter to skip): " TALENTO
read -p "Enter value for EmployeeURL (or press Enter to skip): " EMPLOYEE_URL
read -p "Enter value for NumberingCliURL (or press Enter to skip): " NUMBERING_CLI_URL
read -p "Enter value for HRRFCliUrl (or press Enter to skip): " HRRF_CLI_URL
read -p "Enter value for HRRFNumberingCliUrl (or press Enter to skip): " HRRF_NUMBERING_CLI_URL
read -p "Enter value for NotificationCliUrl (or press Enter to skip): " NOTIFICATION_CLI_URL
read -p "Enter value for GetOpportunityByNameorCode (or press Enter to skip): " GET_OPPORTUNITY
read -p "Enter value for GetLookupService (or press Enter to skip): " GET_LOOKUP_SERVICE
read -p "Enter value for GetEmployeeService (or press Enter to skip): " GET_EMPLOYEE_SERVICE
read -p "Enter value for ClientId (or press Enter to skip): " CLIENT_ID
read -p "Enter value for Tenant (or press Enter to skip): " TENANT
read -p "Enter value for Authority (or press Enter to skip): " AUTHORITY
read -p "Enter value for redirectUri (or press Enter to skip): " REDIRECT_URI

# Ask for credentials
echo ""
echo "Enter credential values (input will be hidden):"
read -s -p "Enter DB_CONNECTION_STRING: " DB_CONNECTION_STRING
echo ""
read -s -p "Enter ADO_CONNECTION_STRING: " ADO_CONNECTION_STRING
echo ""
read -s -p "Enter REDIS_CONNECTION_STRING: " REDIS_CONNECTION_STRING
echo ""

# Configure kubectl for the EKS cluster
echo "Configuring kubectl for EKS cluster..."
aws eks update-kubeconfig --region "$AWS_REGION" --name "$CLUSTER_NAME"

# Verify cluster access
echo "Verifying cluster access..."
kubectl cluster-info || exit 1

# Apply namespace
echo "Applying namespace..."
kubectl apply -f kubernetes/namespace.yaml

# Create Secret for credentials
echo "Creating Kubernetes Secret for credentials..."
kubectl create secret generic trianz-enterprise-operations-secrets \
    -n trianz-enterprise-operations \
    --from-literal="DB_CONNECTION_STRING=$DB_CONNECTION_STRING" \
    --from-literal="ADO_CONNECTION_STRING=$ADO_CONNECTION_STRING" \
    --from-literal="REDIS_CONNECTION_STRING=$REDIS_CONNECTION_STRING" \
    --dry-run=client -o yaml | kubectl apply -f -

# Replace placeholders in deployment.yaml
echo "Replacing placeholders in deployment.yaml..."
sed -i.bak "s|{{IMAGE_URI}}|$IMAGE_URI|g" kubernetes/deployment.yaml
sed -i.bak "s|{{ITS-OPS}}|$ITS_OPS|g" kubernetes/deployment.yaml
sed -i.bak "s|{{Assurance}}|$ASSURANCE|g" kubernetes/deployment.yaml
sed -i.bak "s|{{ClusterMaster}}|$CLUSTER_MASTER|g" kubernetes/deployment.yaml
sed -i.bak "s|{{businessGroupId}}|$BUSINESS_GROUP_ID|g" kubernetes/deployment.yaml
sed -i.bak "s|{{ManagerId}}|$MANAGER_ID|g" kubernetes/deployment.yaml
sed -i.bak "s|{{PHId}}|$PH_ID|g" kubernetes/deployment.yaml
sed -i.bak "s|{{CreateTR-RL}}|$CREATE_TR_RL|g" kubernetes/deployment.yaml
sed -i.bak "s|{{Mail-RL}}|$MAIL_RL|g" kubernetes/deployment.yaml
sed -i.bak "s|{{OnlyTRVisibleEmployee}}|$ONLY_TR_VISIBLE|g" kubernetes/deployment.yaml
sed -i.bak "s|{{ClusterLeadview}}|$CLUSTER_LEAD_VIEW|g" kubernetes/deployment.yaml
sed -i.bak "s|{{clustermanager}}|$CLUSTER_MANAGER|g" kubernetes/deployment.yaml
sed -i.bak "s|{{Percentage}}|$PERCENTAGE|g" kubernetes/deployment.yaml
sed -i.bak "s|{{IsEmailSent}}|$IS_EMAIL_SENT|g" kubernetes/deployment.yaml
sed -i.bak "s|{{OnSiteCountryName}}|$ON_SITE_COUNTRY|g" kubernetes/deployment.yaml
sed -i.bak "s|{{OnsiteEmployeeId}}|$ONSITE_EMP_ID|g" kubernetes/deployment.yaml
sed -i.bak "s|{{ITSDashboardViewers}}|$ITS_DASHBOARD_VIEWERS|g" kubernetes/deployment.yaml
sed -i.bak "s|{{DHAndRLRoleUsers}}|$DH_RL_ROLE_USERS|g" kubernetes/deployment.yaml
sed -i.bak "s|{{HostNo}}|$HOST_NO|g" kubernetes/deployment.yaml
sed -i.bak "s|{{EmailFrom}}|$EMAIL_FROM|g" kubernetes/deployment.yaml
sed -i.bak "s|{{TRRestricted}}|$TR_RESTRICTED|g" kubernetes/deployment.yaml
sed -i.bak "s|{{ActiveAssignmentDownload}}|$ACTIVE_ASSIGN_DOWNLOAD|g" kubernetes/deployment.yaml
sed -i.bak "s|{{RiseTeam}}|$RISE_TEAM|g" kubernetes/deployment.yaml
sed -i.bak "s|{{OpsStatus}}|$OPS_STATUS|g" kubernetes/deployment.yaml
sed -i.bak "s|{{DefaultRole}}|$DEFAULT_ROLE|g" kubernetes/deployment.yaml
sed -i.bak "s|{{RoleManagement}}|$ROLE_MANAGEMENT|g" kubernetes/deployment.yaml
sed -i.bak "s|{{PortNo}}|$PORT_NO|g" kubernetes/deployment.yaml
sed -i.bak "s|{{Talento}}|$TALENTO|g" kubernetes/deployment.yaml
sed -i.bak "s|{{EmployeeURL}}|$EMPLOYEE_URL|g" kubernetes/deployment.yaml
sed -i.bak "s|{{NumberingCliURL}}|$NUMBERING_CLI_URL|g" kubernetes/deployment.yaml
sed -i.bak "s|{{HRRFCliUrl}}|$HRRF_CLI_URL|g" kubernetes/deployment.yaml
sed -i.bak "s|{{HRRFNumberingCliUrl}}|$HRRF_NUMBERING_CLI_URL|g" kubernetes/deployment.yaml
sed -i.bak "s|{{NotificationCliUrl}}|$NOTIFICATION_CLI_URL|g" kubernetes/deployment.yaml
sed -i.bak "s|{{GetOpportunityByNameorCode}}|$GET_OPPORTUNITY|g" kubernetes/deployment.yaml
sed -i.bak "s|{{GetLookupService}}|$GET_LOOKUP_SERVICE|g" kubernetes/deployment.yaml
sed -i.bak "s|{{GetEmployeeService}}|$GET_EMPLOYEE_SERVICE|g" kubernetes/deployment.yaml
sed -i.bak "s|{{ClientId}}|$CLIENT_ID|g" kubernetes/deployment.yaml
sed -i.bak "s|{{Tenant}}|$TENANT|g" kubernetes/deployment.yaml
sed -i.bak "s|{{Authority}}|$AUTHORITY|g" kubernetes/deployment.yaml
sed -i.bak "s|{{redirectUri}}|$REDIRECT_URI|g" kubernetes/deployment.yaml

# Remove backup files
rm -f kubernetes/deployment.yaml.bak

# Apply deployment, service, and ingress
echo "Applying deployment..."
kubectl apply -f kubernetes/deployment.yaml

echo "Applying service..."
kubectl apply -f kubernetes/service.yaml

echo "Applying ingress..."
kubectl apply -f kubernetes/ingress.yaml

# Wait for deployment rollout
echo "Waiting for deployment rollout..."
kubectl rollout status deployment/trianz-enterprise-operations -n trianz-enterprise-operations

# Show resources
echo "=== Deployed Resources ==="
kubectl get pods,svc,ingress -n trianz-enterprise-operations

# Show application URL
echo ""
echo "=== Application URL ==="
echo "Visit: https://trianz-enterprise-operations.example.com"

echo ""
echo "=== Deployment Complete ==="
