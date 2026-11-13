# Deployment Guide: Trianz.Enterprise.Operations

## Prerequisites and System Requirements

### AWS Account
- An active AWS account with permissions to create EKS clusters, ECR repositories, and associated resources.
- IAM permissions for EKS, ECR, and Secrets Manager.

### Required Tools
- **Docker** (Windows containers enabled) — version 20.10+ with Windows container support.
- **AWS CLI** — version 2.x, configured with `aws configure`.
- **kubectl** — version 1.27+ (matching your EKS cluster version).
- **eksctl** — for creating EKS clusters with Windows node groups.
- **PowerShell** — version 5.1+ (for .bat scripts on Windows).

### EKS Cluster Requirements
- EKS cluster version 1.24+ with a **Windows node group** (Windows Server 2019 or 2022).
- The EKS cluster must have the **AWS Load Balancer Controller** installed for ALB ingress support.
- An **EFS file system** and **EFS CSI driver** (optional, for persistent file storage).
- An **ElastiCache for Redis** cluster (for distributed session state and static variable replacement).
- An **RDS SQL Server** instance or **SQL Server on EC2** (for the database backend).

## Local Setup with Docker Compose

1. Ensure Docker Desktop is running with Windows containers enabled.
2. Create a `.env` file in the project root with the following variables:
   ```
   DB_CONNECTION_STRING=Server=your-db-server;Database=YourDB;User Id=your-user;Password=your-password;
   ADO_CONNECTION_STRING=Server=your-db-server;Database=YourDB;User Id=your-user;Password=your-password;
   REDIS_CONNECTION_STRING=your-redis-host:6379
   ```
3. Build and run locally:
   ```bash
   docker-compose up --build
   ```
4. Access the application at `http://localhost:8080`.
5. Access the health endpoint at `http://localhost:8080/health`.

## Building and Pushing the Container Image

### Using build-push.sh (Linux/macOS)
```bash
chmod +x scripts/build-push.sh
./scripts/build-push.sh
```

### Using build-push.bat (Windows)
```cmd
scripts\build-push.bat
```

The script will:
1. Ask you to select the target registry (AWS ECR or Docker Hub).
2. Ask for the image tag (defaults to `latest`).
3. For AWS ECR: log in with `aws ecr get-login-password`, create the repository if it doesn't exist.
4. For Docker Hub: log in with your credentials.
5. Build the Docker image using the multi-stage Dockerfile.
6. Push the image to the selected registry.

## Deploying to AWS EKS

### EKS Cluster Setup

1. Create an EKS cluster with a Windows node group:
   ```bash
   eksctl create cluster --name trianz-enterprise-operations-cluster --version 1.27 --region us-east-1
   eksctl create nodegroup --cluster trianz-enterprise-operations-cluster --name windows-ng --node-type t3.medium --nodes 2 --nodes-max 4 --node-ami-family WindowsServer2019
   ```

2. Install the AWS Load Balancer Controller:
   ```bash
   eksctl utils associate-iam-oidc-provider --cluster trianz-enterprise-operations-cluster --region us-east-1 --approve
   # Follow the EKS documentation to install the ALB controller
   ```

3. Configure kubectl:
   ```bash
   aws eks update-kubeconfig --region us-east-1 --name trianz-enterprise-operations-cluster
   ```

### Deployment Walkthrough

The deployment consists of the following Kubernetes manifests in the `kubernetes/` directory:

1. **namespace.yaml** — Creates the `trianz-enterprise-operations` namespace.
2. **deployment.yaml** — Defines the Deployment with 2 replicas, container image, environment variables, resource limits, and liveness/readiness probes.
3. **service.yaml** — Creates a ClusterIP service exposing port 80 on the container.
4. **ingress.yaml** — Creates an ALB ingress with the AWS Load Balancer Controller annotations, targeting `trianz-enterprise-operations.example.com`.

### Deploying the Application

1. Run the deployment script:
   ```bash
   chmod +x scripts/deploy-image.sh
   ./scripts/deploy-image.sh
   ```

2. The script will:
   - Ask for the AWS region and EKS cluster name.
   - Ask for the full image URI (e.g., `123456789012.dkr.ecr.us-east-1.amazonaws.com/trianz-enterprise-operations:latest`).
   - Ask for environment variable values for all configuration settings.
   - Ask for credential values (DB connection strings, Redis connection string) with hidden input.
   - Configure kubectl for the EKS cluster.
   - Create a Kubernetes Secret with the credentials.
   - Replace placeholders in `deployment.yaml` with the provided values.
   - Apply the namespace, deployment, service, and ingress manifests.
   - Wait for the deployment rollout to complete.
   - Display the deployed resources and application URL.

### Manual Deployment

If you prefer to deploy manually:

```bash
# 1. Configure kubectl
aws eks update-kubeconfig --region us-east-1 --name trianz-enterprise-operations-cluster

# 2. Apply namespace
kubectl apply -f kubernetes/namespace.yaml

# 3. Create secrets
kubectl create secret generic trianz-enterprise-operations-secrets \
    -n trianz-enterprise-operations \
    --from-literal="DB_CONNECTION_STRING=Server=your-db..." \
    --from-literal="ADO_CONNECTION_STRING=Server=your-db..." \
    --from-literal="REDIS_CONNECTION_STRING=your-redis:6379"

# 4. Update deployment.yaml with your image URI and config values
# 5. Apply manifests
kubectl apply -f kubernetes/deployment.yaml
kubectl apply -f kubernetes/service.yaml
kubectl apply -f kubernetes/ingress.yaml

# 6. Wait for rollout
kubectl rollout status deployment/trianz-enterprise-operations -n trianz-enterprise-operations
```

## Configuration Management

### Environment Variables

The application reads configuration from environment variables at runtime. The following environment variables are used:

#### Credentials (stored in Kubernetes Secret `trianz-enterprise-operations-secrets`):
| Variable | Description |
|---|---|
| `DB_CONNECTION_STRING` | Entity Framework database connection string |
| `ADO_CONNECTION_STRING` | ADO.NET connection string for direct SQL access |
| `REDIS_CONNECTION_STRING` | Redis connection string for distributed cache/session |

#### Application Settings (injected as environment variables):
| Variable | Description |
|---|---|
| `ITS-OPS` | IT operations team user IDs |
| `Assurance` | Assurance team user IDs |
| `ClusterMaster` | Cluster master user IDs |
| `businessGroupId` | Business group IDs |
| `ManagerId` | Manager user ID |
| `PHId` | Practice Head user ID |
| `CreateTR-RL` | Resource leads for TR creation |
| `Mail-RL` | Resource leads for mail |
| `OnlyTRVisibleEmployee` | Employee ID for TR visibility |
| `ClusterLeadview` | Cluster lead view setting |
| `clustermanager` | Cluster manager ID |
| `Percentage` | Percentage value |
| `IsEmailSent` | Email sending toggle |
| `OnSiteCountryName` | On-site country name |
| `OnsiteEmployeeId` | On-site employee IDs |
| `ITSDashboardViewers` | IT dashboard viewer IDs |
| `DHAndRLRoleUsers` | DH and RL role user IDs |
| `HostNo` | SMTP host number |
| `EmailFrom` | Email sender address |
| `TRRestricted` | Restricted TR user IDs |
| `ActiveAssignmentDownload` | Active assignment download IDs |
| `RiseTeam` | Rise team user IDs |
| `OpsStatus` | Operations status |
| `DefaultRole` | Default user role |
| `RoleManagement` | Role management settings |
| `PortNo` | SMTP port number |
| `Talento` | Talento URL |
| `EmployeeURL` | Employee service URL |
| `NumberingCliURL` | Numbering client URL |
| `HRRFCliUrl` | HRRF client URL |
| `HRRFNumberingCliUrl` | HRRF numbering client URL |
| `NotificationCliUrl` | Notification client URL |
| `GetOpportunityByNameorCode` | Opportunity service URL |
| `GetLookupService` | Lookup service URL |
| `GetEmployeeService` | Employee service URL |
| `ClientId` | OAuth client ID |
| `Tenant` | OAuth tenant |
| `Authority` | OAuth authority |
| `redirectUri` | OAuth redirect URI |

### Web.config Configuration

The application's `web.config` contains empty placeholder values for connection strings and app settings. At runtime, the application code reads from environment variables first, falling back to `web.config` values. In the container, environment variables are injected via Kubernetes ConfigMaps and Secrets.

### saml.config

The `saml.config` file contains empty `appSettings` values that are read by the SAML library at runtime. These values are injected via environment variables in the Kubernetes deployment.

## Security Considerations and Best Practices

### Container Security
- The container runs on **Windows Server Core** base images (`mcr.microsoft.com/dotnet/framework/aspnet:4.6.1` and `mcr.microsoft.com/dotnet/framework/sdk:4.8`).
- The Dockerfile uses a **multi-stage build** to keep the final image as small as possible by excluding build tools and source files.
- A `.dockerignore` file excludes source code, build artifacts, and temporary files from the build context.

### Credential Management
- All credentials (database connection strings, Redis connection strings) are stored in **Kubernetes Secrets**, never in the container image or deployment manifests.
- The deployment script creates secrets using `kubectl create secret` with values provided via hidden input.
- In production, consider using **AWS Secrets Manager** with the **Secrets Store CSI Driver** for automatic secret rotation.

### Network Security
- The ALB ingress is configured as **internet-facing** with security headers (X-Frame-Options, X-Content-Type-Options, X-XSS-Protection, Content-Security-Policy, Strict-Transport-Security).
- HTTP TRACE, OPTIONS, and HEAD methods are blocked in the IIS configuration.
- The application uses **Forms Authentication** for user authentication.

### Session Security
- Session state is configured to use **StateServer mode** (not InProc), which allows sessions to persist across application restarts.
- For multi-replica deployments, configure **Redis-based session state** by setting the `REDIS_CONNECTION_STRING` environment variable and updating the `sessionState` configuration in `web.config`.
- Session timeout is set to 20 minutes.

### HTTPS
- In production, configure HTTPS by setting up SSL termination at the ALB level.
- The `Strict-Transport-Security` header is already configured in `web.config`.

## .NET Notes

### Runtime Settings
- **Target Framework**: .NET Framework 4.6.1
- **Web Server**: IIS (via `mcr.microsoft.com/dotnet/framework/aspnet:4.6.1` base image)
- **Build Tool**: .NET Framework SDK 4.8 (via `mcr.microsoft.com/dotnet/framework/sdk:4.8` base image)
- **Session State**: StateServer mode (configurable to Redis for distributed deployments)
- **Entity Framework**: EF 6 with empty connection strings in `web.config`, read from environment variables at runtime
- **Authentication**: Forms Authentication with OWIN middleware

### Garbage Collector Tuning
- The .NET Framework garbage collector settings can be tuned in the `web.config` under `<system.web>`:
  ```xml
  <runtime>
    <gcServer enabled="true" />
    <gcConcurrent enabled="false" />
  </runtime>
  ```
- For Windows containers, `gcServer` mode is recommended for multi-core environments.

### Monitoring
- **Application Insights**: Not currently configured. Consider adding Application Insights SDK for request telemetry, exception tracking, and performance monitoring.
- **Logging**: The application writes error logs to `TalentoLog.txt` (and `EmployeeSurveyLog.txt` in the General namespace) in the application's base directory. In containers, consider mounting a volume for log persistence or using a log shipping agent.

### Security Headers (Already Configured)
- `X-Frame-Options: DENY`
- `X-XSS-Protection: 1; mode=block`
- `X-Content-Type-Options: nosniff`
- `X-Download-Options: noopen`
- `Content-Security-Policy: 'self' https://ajax.googleapis.com`
- `Strict-Transport-Security: max-age=31536000; includeSubdomains=always`

### Health Endpoint
- The application exposes a `/health` endpoint (via `HealthController`) that returns a JSON response with status and timestamp.
- This endpoint is used by Kubernetes liveness and readiness probes.
- The endpoint is decorated with `[SkipAuthorizationFilter]` to allow unauthenticated access from Kubernetes probes.

### Known Limitations
- The application uses **WCF service references** (`EmployeeService`) which require the full .NET Framework and cannot run on .NET Core/.NET 5+. This is why the container uses Windows Server Core base images.
- The application uses **ASP.NET MVC 5** which is a legacy framework. Migration to ASP.NET Core is recommended for future modernization but is not part of this containerization effort.
- Some controllers use **in-process session state** (`Session["..."]`). While `web.config` is configured for StateServer mode, a full Redis migration is recommended for multi-replica deployments.

## Troubleshooting

### Pod Fails to Start
1. Check the pod logs:
   ```bash
   kubectl logs -n trianz-enterprise-operations deployment/trianz-enterprise-operations
   ```
2. Verify the image was pulled correctly:
   ```bash
   kubectl describe pod -n trianz-enterprise-operations
   ```
3. Ensure the Windows node group is healthy:
   ```bash
   kubectl get nodes -l kubernetes.io/os=windows
   ```

### Session State Issues
- If users are experiencing session loss across requests, verify that the StateServer or Redis connection string is correctly configured.
- For multi-replica deployments, ensure Redis is configured and the `REDIS_CONNECTION_STRING` environment variable is set.

### Database Connection Issues
- Verify that the `DB_CONNECTION_STRING` and `ADO_CONNECTION_STRING` secrets are correctly set.
- Ensure the EKS worker nodes can reach the database server (check VPC security groups and network ACLs).

### Ingress Issues
- Verify the AWS Load Balancer Controller is installed and healthy.
- Check the ingress status:
  ```bash
  kubectl describe ingress -n trianz-enterprise-operations
  ```
- Verify the ALB was created in the AWS console.

### Image Pull Issues
- Ensure you are logged in to ECR:
  ```bash
  aws ecr get-login-password --region us-east-1 | docker login --username AWS --password-stdin YOUR_ACCOUNT.dkr.ecr.us-east-1.amazonaws.com
  ```
- Verify the image exists in ECR:
  ```bash
  aws ecr describe-images --repository-name trianz-enterprise-operations --region us-east-1
  ```

## Scaling and Management

### Horizontal Pod Autoscaler (HPA)
To create an HPA for the application:
```yaml
apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
metadata:
  name: trianz-enterprise-operations-hpa
  namespace: trianz-enterprise-operations
spec:
  scaleTargetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: trianz-enterprise-operations
  minReplicas: 2
  maxReplicas: 10
  metrics:
    - type: Resource
      resource:
        name: cpu
        target:
          type: Utilization
          averageUtilization: 70
```

### Rolling Updates
To perform a rolling update:
1. Build and push a new image.
2. Update the image in the deployment:
   ```bash
   kubectl set image deployment/trianz-enterprise-operations \
     trianz-enterprise-operations=YOUR_NEW_IMAGE_URI \
     -n trianz-enterprise-operations
   ```
3. Monitor the rollout:
   ```bash
   kubectl rollout status deployment/trianz-enterprise-operations -n trianz-enterprise-operations
   ```

### Rollbacks
To rollback to a previous deployment:
```bash
kubectl rollout undo deployment/trianz-enterprise-operations -n trianz-enterprise-operations
```

To rollback to a specific revision:
```bash
kubectl rollout history deployment/trianz-enterprise-operations -n trianz-enterprise-operations
kubectl rollout undo deployment/trianz-enterprise-operations --to-revision=2 -n trianz-enterprise-operations
```

### Updating Configuration
To update environment variables without rebuilding the image:
1. Update the Kubernetes Secret:
   ```bash
   kubectl create secret generic trianz-enterprise-operations-secrets \
     -n trianz-enterprise-operations \
     --from-literal="DB_CONNECTION_STRING=new-value" \
     --dry-run=client -o yaml | kubectl apply -f -
   ```
2. Restart the deployment:
   ```bash
   kubectl rollout restart deployment/trianz-enterprise-operations -n trianz-enterprise-operations
   ```

## Rollback Instructions

If the deployment fails:
1. Check the rollout status:
   ```bash
   kubectl rollout status deployment/trianz-enterprise-operations -n trianz-enterprise-operations
   ```
2. If the rollout fails, rollback immediately:
   ```bash
   kubectl rollout undo deployment/trianz-enterprise-operations -n trianz-enterprise-operations
   ```
3. Check the previous revision:
   ```bash
   kubectl rollout history deployment/trianz-enterprise-operations -n trianz-enterprise-operations
   ```
4. If needed, rollback to a specific revision:
   ```bash
   kubectl rollout undo deployment/trianz-enterprise-operations --to-revision=N -n trianz-enterprise-operations
   ```
