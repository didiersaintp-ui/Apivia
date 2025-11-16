# 🚀 Apivia - Deployment Guide

> Complete deployment instructions for the Apivia API First Design Platform

---

## 📋 Table of Contents

1. [Prerequisites](#prerequisites)
2. [Quick Start (Docker Compose)](#quick-start-docker-compose)
3. [Configuration](#configuration)
4. [Database Setup](#database-setup)
5. [Running Services](#running-services)
6. [Accessing Services](#accessing-services)
7. [Production Deployment](#production-deployment)
8. [Troubleshooting](#troubleshooting)

---

## Prerequisites

### Required Software:
- **Docker Desktop** (Windows/Mac) or **Docker Engine** (Linux)
  - Version: 20.10 or higher
  - Docker Compose: 2.0 or higher
- **.NET 8 SDK** (for local development)
  - Download: https://dotnet.microsoft.com/download/dotnet/8.0
- **Node.js 18+** (for frontend development)
  - Download: https://nodejs.org/

### System Requirements:
- **RAM**: Minimum 8GB, Recommended 16GB
- **Disk Space**: Minimum 10GB free
- **OS**: Windows 10/11 with Docker Desktop, macOS, or Linux

---

## Quick Start (Docker Compose)

### 1. Clone the Repository
```bash
git clone <repository-url>
cd Apivia
```

### 2. Configure Environment Variables
```bash
# Copy example environment file
cp .env.example .env

# Edit .env with your configuration
# Important: Change JWT_SECRET and DATABASE_PASSWORD in production!
```

### 3. Start All Services
```bash
# Start all services in background
docker-compose up -d

# View logs
docker-compose logs -f

# Check service health
docker-compose ps
```

### 4. Create Database Migration
```bash
# Run migrations (first time setup)
docker-compose exec gateway dotnet ef database update --project /app/Shared/Data

# Or from local machine with .NET 8 SDK:
cd src/Shared/Data
dotnet ef migrations add InitialCreate --startup-project ../../Gateway
dotnet ef database update --startup-project ../../Gateway
```

### 5. Access the Application
- **Frontend**: http://localhost:3000
- **API Gateway**: http://localhost:5000
- **Swagger UI**: http://localhost:5000/swagger
- **Seq Logs**: http://localhost:5341
- **RabbitMQ Management**: http://localhost:15672 (guest/guest)
- **MinIO Console**: http://localhost:9001 (minioadmin/minioadmin)

---

## Configuration

### Environment Variables (.env)

```bash
# Database
POSTGRES_DB=apivia
POSTGRES_USER=apivia
POSTGRES_PASSWORD=Change_This_Password_123!
DATABASE_URL=Host=postgres;Port=5432;Database=apivia;Username=apivia;Password=Change_This_Password_123!

# JWT Authentication
JWT_SECRET=Your_Super_Secret_JWT_Key_Change_This_In_Production_Min_32_Chars!
JWT_ISSUER=Apivia
JWT_AUDIENCE=Apivia.Client
JWT_EXPIRATION_MINUTES=60
JWT_REFRESH_EXPIRATION_DAYS=7

# RabbitMQ
RABBITMQ_DEFAULT_USER=apivia
RABBITMQ_DEFAULT_PASS=Change_This_Password_123!
RABBITMQ_URL=amqp://apivia:Change_This_Password_123!@rabbitmq:5672

# Redis
REDIS_URL=redis:6379

# MinIO (S3-compatible storage)
MINIO_ROOT_USER=minioadmin
MINIO_ROOT_PASSWORD=Change_This_Password_123!
MINIO_URL=minio:9000

# Seq (Logging)
SEQ_URL=http://seq:5341

# Service URLs (Internal)
AUTH_SERVICE_URL=http://auth:8080
API_DESIGN_SERVICE_URL=http://apidesign:8080
DATA_DICTIONARY_SERVICE_URL=http://datadictionary:8080
GOVERNANCE_SERVICE_URL=http://governance:8080
MOCK_SERVER_SERVICE_URL=http://mockserver:8080
LINTING_SERVICE_URL=http:// linting:8080

# Frontend
VITE_API_BASE_URL=http://localhost:5000
```

### IMPORTANT Security Notes:
⚠️ **CHANGE ALL PASSWORDS IN PRODUCTION!**
- JWT_SECRET: Must be at least 32 characters
- POSTGRES_PASSWORD: Use strong password
- RABBITMQ_DEFAULT_PASS: Use strong password
- MINIO_ROOT_PASSWORD: Use strong password

---

## Database Setup

### Automatic Setup (Recommended)
```bash
# Docker Compose will automatically:
# 1. Create PostgreSQL database
# 2. Run health checks
# 3. Wait for database to be ready

# Apply migrations
docker-compose exec gateway dotnet ef database update --project /app/Shared/Data
```

### Manual Setup
```bash
# Connect to PostgreSQL
docker-compose exec postgres psql -U apivia -d apivia

# Check tables
\dt

# Check specific table
\d "Users"

# Exit
\q
```

### Create First Admin User
```bash
# Use Auth Service API to register first user
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@apivia.com",
    "password": "Admin@123456",
    "confirmPassword": "Admin@123456",
    "fullName": "Admin User"
  }'
```

---

## Running Services

### Start All Services
```bash
docker-compose up -d
```

### Start Specific Service
```bash
docker-compose up -d auth
docker-compose up -d apidesign
```

### Stop All Services
```bash
docker-compose down
```

### Stop and Remove Volumes (⚠️ Data Loss)
```bash
docker-compose down -v
```

### Restart Service
```bash
docker-compose restart auth
```

### View Logs
```bash
# All services
docker-compose logs -f

# Specific service
docker-compose logs -f auth

# Last 100 lines
docker-compose logs --tail=100 auth
```

### Rebuild Service
```bash
# Rebuild specific service
docker-compose build auth

# Rebuild and start
docker-compose up -d --build auth
```

---

## Accessing Services

### 1. Frontend Application
**URL**: http://localhost:3000

**Features**:
- User registration and login
- Workspace management
- Project and API Spec management
- Data Dictionary browser
- Impact Analysis dashboard

### 2. API Gateway
**URL**: http://localhost:5000
**Swagger**: http://localhost:5000/swagger

**Routes**:
- `/api/auth/*` → Auth Service
- `/api/projects/*` → API Design Service
- `/api/apispecs/*` → API Design Service
- `/api/datadictionaries/*` → Data Dictionary Service
- `/api/dataentities/*` → Data Dictionary Service
- `/api/dataattributes/*` → Data Dictionary Service
- `/api/linkage/*` → Governance Engine
- `/api/impactanalysis/*` → Governance Engine

### 3. Individual Services (Direct Access)

**Auth Service**: http://localhost:5001
- Swagger: http://localhost:5001/swagger
- Health: http://localhost:5001/health

**API Design Service**: http://localhost:5002
- Swagger: http://localhost:5002/swagger
- Health: http://localhost:5002/health

**Data Dictionary Service**: http://localhost:5003
- Swagger: http://localhost:5003/swagger
- Health: http://localhost:5003/health

**Governance Engine**: http://localhost:5004
- Swagger: http://localhost:5004/swagger
- Health: http://localhost:5004/health

### 4. Infrastructure Services

**Seq (Centralized Logging)**: http://localhost:5341
- View all application logs
- Search and filter logs
- Create alerts

**RabbitMQ Management**: http://localhost:15672
- Username: guest
- Password: guest
- View queues and messages

**MinIO Console**: http://localhost:9001
- Username: minioadmin
- Password: minioadmin (or your configured password)
- Manage file storage

---

## Production Deployment

### 1. Kubernetes Deployment (Recommended)

```bash
# Create namespace
kubectl create namespace apivia

# Apply configurations
kubectl apply -f k8s/namespace.yaml
kubectl apply -f k8s/configmap.yaml
kubectl apply -f k8s/secrets.yaml
kubectl apply -f k8s/postgres.yaml
kubectl apply -f k8s/rabbitmq.yaml
kubectl apply -f k8s/redis.yaml
kubectl apply -f k8s/minio.yaml
kubectl apply -f k8s/seq.yaml
kubectl apply -f k8s/services/
kubectl apply -f k8s/ingress.yaml

# Check status
kubectl get pods -n apivia
kubectl get services -n apivia
```

### 2. Docker Swarm Deployment

```bash
# Initialize swarm
docker swarm init

# Deploy stack
docker stack deploy -c docker-compose.prod.yml apivia

# Check status
docker stack services apivia
docker stack ps apivia
```

### 3. Cloud Platforms

#### Azure (Azure Kubernetes Service)
```bash
# Create AKS cluster
az aks create --resource-group apivia-rg --name apivia-cluster \
  --node-count 3 --enable-addons monitoring

# Get credentials
az aks get-credentials --resource-group apivia-rg --name apivia-cluster

# Deploy
kubectl apply -f k8s/
```

#### AWS (EKS)
```bash
# Create EKS cluster
eksctl create cluster --name apivia-cluster --region us-east-1 \
  --nodegroup-name standard-workers --node-type t3.medium \
  --nodes 3

# Deploy
kubectl apply -f k8s/
```

#### Google Cloud (GKE)
```bash
# Create GKE cluster
gcloud container clusters create apivia-cluster \
  --num-nodes=3 --machine-type=n1-standard-2

# Get credentials
gcloud container clusters get-credentials apivia-cluster

# Deploy
kubectl apply -f k8s/
```

---

## Production Checklist

### Security:
- ✅ Change all default passwords
- ✅ Use strong JWT_SECRET (32+ characters)
- ✅ Enable HTTPS/TLS
- ✅ Configure CORS properly
- ✅ Use secrets management (Azure Key Vault, AWS Secrets Manager, etc.)
- ✅ Enable authentication on all services
- ✅ Configure firewall rules
- ✅ Regular security updates

### Performance:
- ✅ Configure connection pooling (PostgreSQL, Redis)
- ✅ Enable caching (Redis)
- ✅ Set up CDN for frontend
- ✅ Configure database indexes
- ✅ Enable compression (gzip, brotli)
- ✅ Set up load balancing
- ✅ Configure auto-scaling
- ✅ Optimize Docker images (multi-stage builds)

### Monitoring:
- ✅ Configure Seq for centralized logging
- ✅ Set up Application Insights / Datadog / New Relic
- ✅ Configure health check endpoints
- ✅ Set up alerting (email, Slack, PagerDuty)
- ✅ Monitor resource usage (CPU, RAM, disk)
- ✅ Track API response times
- ✅ Set up database monitoring

### Backup:
- ✅ Configure PostgreSQL backups (daily)
- ✅ Configure MinIO backups
- ✅ Test restore procedures
- ✅ Set up backup retention policy
- ✅ Store backups in separate location

### High Availability:
- ✅ Run multiple instances of each service
- ✅ Configure database replication
- ✅ Set up Redis cluster
- ✅ Configure RabbitMQ cluster
- ✅ Use managed services when possible
- ✅ Implement circuit breakers
- ✅ Configure retry policies

---

## Troubleshooting

### Service Won't Start

**Check logs**:
```bash
docker-compose logs service-name
```

**Common issues**:
- Database not ready → Wait for health check
- Port already in use → Change port in docker-compose.yml
- Missing environment variables → Check .env file
- Insufficient memory → Increase Docker memory limit

### Database Connection Issues

**Check database is running**:
```bash
docker-compose ps postgres
```

**Test connection**:
```bash
docker-compose exec postgres psql -U apivia -d apivia -c "SELECT 1"
```

**Reset database** (⚠️ Data Loss):
```bash
docker-compose down -v
docker-compose up -d postgres
# Wait for health check, then apply migrations
```

### Migration Issues

**Reset migrations**:
```bash
# Delete Migrations folder
rm -rf src/Shared/Data/Migrations

# Create new migration
cd src/Shared/Data
dotnet ef migrations add InitialCreate --startup-project ../../Gateway
dotnet ef database update --startup-project ../../Gateway
```

### Frontend Can't Connect to Backend

**Check environment variables**:
```bash
# In src/Frontend/.env
VITE_API_BASE_URL=http://localhost:5000
```

**Check CORS configuration** in Gateway/Program.cs:
```csharp
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.WithOrigins("http://localhost:3000")
               .AllowAnyHeader()
               .AllowAnyMethod()
               .AllowCredentials();
    });
});
```

### Performance Issues

**Check resource usage**:
```bash
docker stats
```

**Increase Docker resources**:
- Docker Desktop → Settings → Resources
- Increase Memory to 8GB+
- Increase CPUs to 4+

**Check database performance**:
```sql
-- Slow queries
SELECT * FROM pg_stat_statements
ORDER BY mean_exec_time DESC
LIMIT 10;

-- Missing indexes
SELECT schemaname, tablename, attname, n_distinct, correlation
FROM pg_stats
WHERE schemaname NOT IN ('pg_catalog', 'information_schema')
ORDER BY abs(correlation) DESC;
```

### Logs Not Appearing in Seq

**Check Seq is running**:
```bash
docker-compose ps seq
```

**Check Serilog configuration** in appsettings.json:
```json
{
  "Serilog": {
    "WriteTo": [
      {
        "Name": "Seq",
        "Args": {
          "serverUrl": "http://seq:5341"
        }
      }
    ]
  }
}
```

---

## Support

### Documentation:
- **Architecture**: See ARCHITECTURE.md
- **Development**: See TASK_LIST.md
- **Progress**: See FINAL_PROGRESS_REPORT.md

### Getting Help:
1. Check logs: `docker-compose logs -f`
2. Check health endpoints: http://localhost:5000/health
3. Review Seq logs: http://localhost:5341
4. Check service documentation in Swagger UI

### Common Commands Reference:

```bash
# Start all services
docker-compose up -d

# Stop all services
docker-compose down

# View logs
docker-compose logs -f

# Restart service
docker-compose restart service-name

# Rebuild service
docker-compose up -d --build service-name

# Check status
docker-compose ps

# Execute command in container
docker-compose exec service-name bash

# View resource usage
docker stats

# Clean up everything (⚠️ Data Loss)
docker-compose down -v
docker system prune -a
```

---

## Next Steps

After successful deployment:

1. ✅ Create admin user via `/api/auth/register`
2. ✅ Login and get JWT token
3. ✅ Create workspace
4. ✅ Create project
5. ✅ Create API specification
6. ✅ Create data dictionary
7. ✅ Link API to data dictionary
8. ✅ Run impact analysis

---

<div align="center">

## 🎉 Your Apivia Platform is Ready!

Access the application at **http://localhost:3000**

API Documentation at **http://localhost:5000/swagger**

Logs at **http://localhost:5341**

---

**Need Help?** Check the documentation or review the logs

**Found a Bug?** Create an issue in the repository

</div>
