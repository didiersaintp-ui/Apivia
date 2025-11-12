# APIVIA - Liste Détaillée des Tâches de Développement

> **Objectif**: Développement complet de la plateforme API First Design en 23 semaines

---

## 📊 Vue d'Ensemble

- **Durée totale**: 23 semaines (~6 mois)
- **Nombre de phases**: 12
- **Services backend**: 10
- **Technologies**: .NET 8, React, PostgreSQL, Kubernetes, Docker

---

## Phase 0: Setup & Infrastructure (Semaine 1)

### 🎯 Objectif
Environnement de développement prêt et infrastructure de base opérationnelle

### ✅ Tâches

#### 0.1 Structure du Projet

- [ ] **0.1.1** Créer le repository Git
  - Initialiser avec .gitignore (.NET, Node, IDE)
  - Créer README.md initial
  - Définir la structure de branches (main, develop, feature/*)

- [ ] **0.1.2** Créer la solution .NET
  ```bash
  dotnet new sln -n Apivia
  mkdir -p src/{Gateway,Services/{ApiDesign,DataDictionary,Governance,MockServer,Linting,Documentation,Collaboration,Auth,GitIntegration},Shared}
  mkdir -p tests/{Services,Integration,E2E}
  ```

- [ ] **0.1.3** Créer les projets .NET de base
  ```bash
  # Gateway
  dotnet new webapi -n Apivia.Gateway -o src/Gateway

  # Services
  dotnet new webapi -n Apivia.Services.ApiDesign -o src/Services/ApiDesign
  dotnet new webapi -n Apivia.Services.DataDictionary -o src/Services/DataDictionary
  dotnet new webapi -n Apivia.Services.Governance -o src/Services/Governance
  dotnet new webapi -n Apivia.Services.MockServer -o src/Services/MockServer
  dotnet new webapi -n Apivia.Services.Linting -o src/Services/Linting
  dotnet new webapi -n Apivia.Services.Documentation -o src/Services/Documentation
  dotnet new webapi -n Apivia.Services.Collaboration -o src/Services/Collaboration
  dotnet new webapi -n Apivia.Services.Auth -o src/Services/Auth
  dotnet new webapi -n Apivia.Services.GitIntegration -o src/Services/GitIntegration

  # Shared libraries
  dotnet new classlib -n Apivia.Shared.Common -o src/Shared/Common
  dotnet new classlib -n Apivia.Shared.Data -o src/Shared/Data
  dotnet new classlib -n Apivia.Shared.Messaging -o src/Shared/Messaging

  # Tests
  dotnet new xunit -n Apivia.Services.ApiDesign.Tests -o tests/Services/ApiDesign.Tests
  dotnet new xunit -n Apivia.Services.DataDictionary.Tests -o tests/Services/DataDictionary.Tests
  # ... (autres tests)
  ```

- [ ] **0.1.4** Créer le projet Frontend
  ```bash
  npm create vite@latest src/Frontend -- --template react-ts
  cd src/Frontend
  npm install
  npx shadcn-ui@latest init
  ```

- [ ] **0.1.5** Ajouter tous les projets à la solution
  ```bash
  dotnet sln add src/**/*.csproj
  dotnet sln add tests/**/*.csproj
  ```

#### 0.2 Configuration des Outils de Développement

- [ ] **0.2.1** Configurer EditorConfig
  - Créer `.editorconfig` (indentation, charset, line endings)

- [ ] **0.2.2** Configurer StyleCop & Analyzers (.NET)
  - Ajouter packages NuGet: `StyleCop.Analyzers`, `SonarAnalyzer.CSharp`
  - Créer `stylecop.json` avec règles personnalisées
  - Ajouter `Directory.Build.props` pour configuration globale

- [ ] **0.2.3** Configurer ESLint & Prettier (Frontend)
  - Installer `eslint`, `prettier`, `eslint-config-prettier`
  - Créer `.eslintrc.json` et `.prettierrc`
  - Ajouter scripts npm: `lint`, `format`

- [ ] **0.2.4** Configurer Git Hooks (Husky)
  - Installer husky
  - Pre-commit: lint-staged
  - Pre-push: tests

#### 0.3 Infrastructure Docker

- [ ] **0.3.1** Créer Dockerfile pour chaque service backend
  - Template multi-stage (build, publish, runtime)
  - Optimisation layers
  - Health checks

- [ ] **0.3.2** Créer Dockerfile pour Frontend
  - Build stage (npm build)
  - Runtime stage (nginx)
  - Configuration nginx pour SPA

- [ ] **0.3.3** Créer docker-compose.yml pour développement
  - PostgreSQL
  - RabbitMQ
  - Redis
  - MinIO
  - Seq
  - Tous les services backend
  - Frontend
  - Réseaux et volumes

- [ ] **0.3.4** Créer .dockerignore pour chaque projet

#### 0.4 Infrastructure Kubernetes

- [ ] **0.4.1** Créer structure k8s/
  ```
  k8s/
  ├── namespace.yaml
  ├── configmaps/
  ├── secrets/
  ├── storage/
  ├── infrastructure/
  ├── services/
  ├── frontend/
  └── ingress.yaml
  ```

- [ ] **0.4.2** Créer manifests Namespace
  - Namespace `apivia`
  - Labels et annotations

- [ ] **0.4.3** Créer manifests Storage
  - PVC pour PostgreSQL (10Gi)
  - PVC pour RabbitMQ (5Gi)
  - PVC pour Redis (2Gi)
  - PVC pour MinIO (20Gi)
  - PVC pour Seq (5Gi)

- [ ] **0.4.4** Créer manifests Infrastructure
  - PostgreSQL StatefulSet
  - RabbitMQ StatefulSet
  - Redis StatefulSet
  - MinIO StatefulSet
  - Seq StatefulSet
  - Services ClusterIP pour chacun

- [ ] **0.4.5** Créer manifests pour Services Backend
  - Deployment pour chaque service
  - Service ClusterIP pour chaque service
  - ConfigMaps pour configuration
  - Secrets pour credentials

- [ ] **0.4.6** Créer manifest Gateway
  - Deployment
  - Service LoadBalancer (port 80/443)

- [ ] **0.4.7** Créer manifest Frontend
  - Deployment
  - Service ClusterIP

- [ ] **0.4.8** Créer Ingress (optionnel)
  - Traefik ou nginx-ingress
  - Routes vers Gateway

- [ ] **0.4.9** Créer scripts de déploiement
  - `deploy.sh` - Déploiement complet
  - `undeploy.sh` - Suppression
  - `update-service.sh` - Mise à jour d'un service

#### 0.5 Base de Données

- [ ] **0.5.1** Créer DbContext (Entity Framework)
  - `ApiviaDbContext` dans Shared.Data
  - Configuration des entities
  - Conventions de nommage

- [ ] **0.5.2** Créer les entités de base
  - User, Workspace, Team, TeamMember
  - Project, ApiSpec
  - DataDictionary, DataEntity, DataAttribute
  - ApiSchemaElement
  - Comment, Proposal
  - AuditLog, ImpactAnalysis

- [ ] **0.5.3** Configurer les relations (Fluent API)
  - One-to-Many
  - Many-to-Many
  - Cascade delete
  - Indexes

- [ ] **0.5.4** Créer migration initiale
  ```bash
  dotnet ef migrations add InitialCreate -p src/Shared/Data -s src/Gateway
  ```

- [ ] **0.5.5** Créer seed data pour développement
  - Users de test
  - Workspaces de test
  - Projets de test
  - Données du dictionnaire de test

- [ ] **0.5.6** Script de reset base de données
  ```bash
  dotnet ef database drop -f
  dotnet ef database update
  ```

#### 0.6 CI/CD

- [ ] **0.6.1** Configurer GitHub Actions
  - Workflow build & test (.NET)
  - Workflow build & test (Frontend)
  - Workflow Docker build
  - Workflow Kubernetes deploy (staging)

- [ ] **0.6.2** Créer `.github/workflows/backend-ci.yml`
  - Checkout
  - Setup .NET
  - Restore
  - Build
  - Test avec coverage
  - Upload coverage à Codecov

- [ ] **0.6.3** Créer `.github/workflows/frontend-ci.yml`
  - Checkout
  - Setup Node
  - Install dependencies
  - Lint
  - Test
  - Build

- [ ] **0.6.4** Créer `.github/workflows/docker-build.yml`
  - Build images Docker
  - Push vers registry (GitHub Container Registry)
  - Tag avec version

#### 0.7 Documentation

- [ ] **0.7.1** Créer README.md complet
  - Description du projet
  - Prérequis
  - Installation
  - Utilisation
  - Architecture
  - Contribution

- [ ] **0.7.2** Créer CONTRIBUTING.md
  - Guidelines de contribution
  - Code style
  - Commit messages
  - Pull requests

- [ ] **0.7.3** Créer docs/
  - `docs/SETUP.md` - Guide d'installation détaillé
  - `docs/DEVELOPMENT.md` - Guide de développement
  - `docs/API.md` - Documentation API
  - `docs/ARCHITECTURE.md` - Lien vers ARCHITECTURE.md

### 📦 Livrables Phase 0

- ✅ Repository Git configuré avec structure complète
- ✅ Solution .NET avec tous les projets
- ✅ Projet Frontend React + TypeScript
- ✅ Docker Compose fonctionnel (local dev)
- ✅ Manifests Kubernetes complets
- ✅ Base de données avec migrations
- ✅ CI/CD pipeline de base
- ✅ Documentation initiale

### ⏱️ Estimation: 5 jours

---

## Phase 1: Core Backend Services (Semaines 2-4)

### 🎯 Objectif
Services backend principaux opérationnels avec authentification et gestion des API specs

---

## Semaine 2: Auth & API Design Service

### ✅ Tâches

#### 1.1 Shared Libraries

- [ ] **1.1.1** Créer `Apivia.Shared.Common`
  - `Result<T>` pattern pour gestion d'erreurs
  - `ErrorResponse` pour API errors
  - `PagedList<T>` pour pagination
  - Extension methods utiles
  - Constants

- [ ] **1.1.2** Créer `Apivia.Shared.Data`
  - `BaseEntity` avec Id, CreatedAt, ModifiedAt
  - `AuditableEntity` avec CreatedBy, ModifiedBy
  - `IRepository<T>` interface
  - `GenericRepository<T>` implémentation
  - `IUnitOfWork` interface

- [ ] **1.1.3** Créer `Apivia.Shared.Messaging`
  - `IEventBus` interface
  - `RabbitMQEventBus` implémentation (MassTransit)
  - Event base classes
  - Message contracts

#### 1.2 Auth Service

- [ ] **1.2.1** Installer packages NuGet
  - `Microsoft.AspNetCore.Identity.EntityFrameworkCore`
  - `Microsoft.IdentityModel.Tokens`
  - `System.IdentityModel.Tokens.Jwt`
  - `BCrypt.Net-Next`
  - `FluentValidation`

- [ ] **1.2.2** Configurer Identity
  - Extend `IdentityUser` → `ApplicationUser`
  - Configure `AuthDbContext`
  - Configure Identity options (password, lockout)

- [ ] **1.2.3** Créer DTOs
  - `RegisterRequest`
  - `LoginRequest`
  - `LoginResponse`
  - `RefreshTokenRequest`
  - `ChangePasswordRequest`

- [ ] **1.2.4** Créer `JwtTokenService`
  - `GenerateAccessToken(User)`
  - `GenerateRefreshToken()`
  - `ValidateRefreshToken(token)`
  - Configuration JWT (appsettings.json)

- [ ] **1.2.5** Créer `AuthController`
  - `POST /api/auth/register` - Registration
  - `POST /api/auth/login` - Login
  - `POST /api/auth/refresh` - Refresh token
  - `POST /api/auth/logout` - Logout
  - `POST /api/auth/change-password` - Change password

- [ ] **1.2.6** Valider avec FluentValidation
  - `RegisterRequestValidator`
  - `LoginRequestValidator`
  - Email validation
  - Password strength validation

- [ ] **1.2.7** Créer tests unitaires
  - Test `JwtTokenService`
  - Test `AuthController`
  - Test validation
  - Coverage >80%

- [ ] **1.2.8** Documenter avec OpenAPI
  - Annotations Swagger
  - Exemples de requêtes/réponses
  - Documentation des erreurs

#### 1.3 API Design Service

- [ ] **1.3.1** Installer packages NuGet
  - `Microsoft.EntityFrameworkCore.Design`
  - `Npgsql.EntityFrameworkCore.PostgreSQL`
  - `AutoMapper.Extensions.Microsoft.DependencyInjection`
  - `FluentValidation.AspNetCore`
  - `YamlDotNet`
  - `NJsonSchema`

- [ ] **1.3.2** Créer entités
  - `Project` (Id, WorkspaceId, Name, Description, Visibility, GitRepoUrl, GitBranch)
  - `ApiSpec` (Id, ProjectId, Version, Content, Format, OpenApiVersion, Status)
  - `ApiSpecVersion` (historique)

- [ ] **1.3.3** Configurer DbContext
  - `ApiDesignDbContext : DbContext`
  - Fluent API configuration
  - Indexes

- [ ] **1.3.4** Créer DTOs
  - `ProjectDto`, `CreateProjectRequest`, `UpdateProjectRequest`
  - `ApiSpecDto`, `CreateApiSpecRequest`, `UpdateApiSpecRequest`
  - `ImportSpecRequest`, `ExportSpecRequest`
  - `ValidateSpecRequest`, `ValidationResult`

- [ ] **1.3.5** Créer AutoMapper profiles
  - `ProjectProfile`
  - `ApiSpecProfile`

- [ ] **1.3.6** Créer services
  - `IProjectService` & `ProjectService`
    - `GetAllProjects(workspaceId)`
    - `GetProjectById(id)`
    - `CreateProject(request)`
    - `UpdateProject(id, request)`
    - `DeleteProject(id)`
  - `IApiSpecService` & `ApiSpecService`
    - `GetAllSpecs(projectId)`
    - `GetSpecById(id)`
    - `CreateSpec(projectId, request)`
    - `UpdateSpec(id, request)`
    - `DeleteSpec(id)`
    - `GetSpecVersions(id)`

- [ ] **1.3.7** Créer `OpenApiValidationService`
  - `ValidateOpenApiV2(yaml/json)`
  - `ValidateOpenApiV3(yaml/json)`
  - Parse avec YamlDotNet
  - Validate schema avec NJsonSchema
  - Retourner `ValidationResult` avec erreurs

- [ ] **1.3.8** Créer `SpecImportExportService`
  - `ImportFromYaml(content)`
  - `ImportFromJson(content)`
  - `ImportFromPostman(collection)`
  - `ExportToYaml(specId)`
  - `ExportToJson(specId)`

- [ ] **1.3.9** Créer controllers
  - `ProjectsController`
    - `GET /api/projects`
    - `GET /api/projects/{id}`
    - `POST /api/projects`
    - `PUT /api/projects/{id}`
    - `DELETE /api/projects/{id}`
  - `ApiSpecsController`
    - `GET /api/projects/{projectId}/specs`
    - `GET /api/specs/{id}`
    - `POST /api/projects/{projectId}/specs`
    - `PUT /api/specs/{id}`
    - `DELETE /api/specs/{id}`
    - `GET /api/specs/{id}/versions`
  - `SpecValidationController`
    - `POST /api/specs/validate`
  - `SpecImportExportController`
    - `POST /api/specs/import`
    - `POST /api/specs/{id}/export`

- [ ] **1.3.10** Valider avec FluentValidation
  - Validators pour tous les DTOs
  - Règles métier (noms uniques, etc.)

- [ ] **1.3.11** Créer tests unitaires
  - Test services
  - Test controllers
  - Test validation OpenAPI
  - Test import/export
  - Coverage >80%

- [ ] **1.3.12** Créer tests d'intégration
  - Test end-to-end avec DB in-memory
  - Test scenarios CRUD complets

- [ ] **1.3.13** Documenter avec OpenAPI
  - Swagger annotations
  - Exemples

#### 1.4 API Gateway (YARP)

- [ ] **1.4.1** Installer packages NuGet
  - `Yarp.ReverseProxy`
  - `Microsoft.AspNetCore.Authentication.JwtBearer`

- [ ] **1.4.2** Configurer routes YARP (appsettings.json)
  - Route vers Auth Service
  - Route vers API Design Service
  - Route vers Data Dictionary Service (préparation)
  - Route vers autres services

- [ ] **1.4.3** Configurer JWT Authentication
  - Valider tokens depuis Auth Service
  - Configuration JWT options

- [ ] **1.4.4** Configurer CORS
  - AllowedOrigins (frontend)
  - AllowedMethods, AllowedHeaders
  - AllowCredentials

- [ ] **1.4.5** Créer Health Check endpoint
  - `GET /health`
  - Check tous les services downstream

- [ ] **1.4.6** Logging et monitoring
  - Serilog configuration
  - Request/Response logging
  - Performance logging

- [ ] **1.4.7** Rate limiting (optionnel)
  - AspNetCoreRateLimit
  - Configuration par endpoint

- [ ] **1.4.8** Tests
  - Test routing
  - Test authentication
  - Test CORS

### 📦 Livrables Semaine 2

- ✅ Auth Service complet et testé
- ✅ API Design Service complet et testé
- ✅ API Gateway (YARP) configuré
- ✅ Authentication JWT fonctionnelle
- ✅ Documentation OpenAPI
- ✅ Tests: >80% coverage

### ⏱️ Estimation: 5 jours

---

## Semaine 3: Data Dictionary & Governance

### ✅ Tâches

#### 1.5 Data Dictionary Service

- [ ] **1.5.1** Installer packages NuGet
  - EF Core, AutoMapper, FluentValidation (déjà standard)

- [ ] **1.5.2** Créer entités
  - `DataDictionary` (Id, WorkspaceId, Name, Description)
  - `DataEntity` (Id, DictionaryId, Name, DisplayName, Description, BusinessOwner, FunctionalDomain, Sensitivity, QualityLevel, Metadata JSONB)
  - `DataAttribute` (Id, DataEntityId, Name, Description, DataType, IsRequired, Format, AllowedValues, Metadata JSONB)
  - Enums: `DataSensitivityLevel`, `DataQualityLevel`

- [ ] **1.5.3** Configurer DbContext
  - `DataDictionaryDbContext`
  - Fluent API avec support JSONB (PostgreSQL)
  - Indexes sur Name, FunctionalDomain, Sensitivity

- [ ] **1.5.4** Créer DTOs
  - `DataDictionaryDto`, `CreateDataDictionaryRequest`
  - `DataEntityDto`, `CreateDataEntityRequest`, `UpdateDataEntityRequest`
  - `DataAttributeDto`, `CreateDataAttributeRequest`, `UpdateDataAttributeRequest`
  - `DataEntitySearchRequest`, `DataEntitySearchResult`

- [ ] **1.5.5** Créer services
  - `IDataDictionaryService` & `DataDictionaryService`
    - CRUD dictionaries
  - `IDataEntityService` & `DataEntityService`
    - CRUD entities
    - `SearchEntities(criteria)`
    - `GetEntitiesByDomain(domain)`
    - `GetEntitiesBySensitivity(level)`
  - `IDataAttributeService` & `DataAttributeService`
    - CRUD attributes

- [ ] **1.5.6** Créer `DataEntitySearchService`
  - Full-text search sur Name, Description
  - Filtres: Domain, Sensitivity, QualityLevel
  - Pagination

- [ ] **1.5.7** Créer controllers
  - `DataDictionariesController`
    - `GET /api/data-dictionaries`
    - `GET /api/data-dictionaries/{id}`
    - `POST /api/data-dictionaries`
    - `PUT /api/data-dictionaries/{id}`
    - `DELETE /api/data-dictionaries/{id}`
  - `DataEntitiesController`
    - `GET /api/data-entities?dictionaryId={id}`
    - `GET /api/data-entities/{id}`
    - `POST /api/data-entities`
    - `PUT /api/data-entities/{id}`
    - `DELETE /api/data-entities/{id}`
    - `GET /api/data-entities/search?term={term}&domain={domain}`
    - `GET /api/data-entities/domains`
  - `DataAttributesController`
    - CRUD pour attributes

- [ ] **1.5.8** Valider avec FluentValidation
  - Validators pour tous les DTOs
  - Business rules

- [ ] **1.5.9** Créer tests unitaires
  - Test services
  - Test controllers
  - Test search
  - Coverage >80%

- [ ] **1.5.10** Créer tests d'intégration
  - Test CRUD complet
  - Test search scenarios

- [ ] **1.5.11** Documenter avec OpenAPI

#### 1.6 Governance Engine Service

- [ ] **1.6.1** Créer entités
  - `ApiSchemaElement` (Id, ApiSpecId, SchemaPath, DataEntityId, DataAttributeId, LinkedAt, LinkedBy, AutoSynced)
  - `ImpactAnalysis` (Id, DataEntityId, ChangeType, ProposedChanges JSONB, AffectedApis JSONB, OverallRiskLevel, Status, CreatedAt, CreatedBy)
  - Enums: `ChangeType`, `RiskLevel`, `ImpactAnalysisStatus`

- [ ] **1.6.2** Configurer DbContext
  - `GovernanceDbContext`
  - Indexes

- [ ] **1.6.3** Créer DTOs
  - `LinkApiToEntityRequest`, `LinkApiToEntityResponse`
  - `UnlinkApiFromEntityRequest`
  - `ImpactAnalysisRequest`, `ImpactAnalysisResult`
  - `AffectedApiInfo`, `ChangeProposal`

- [ ] **1.6.4** Créer `ApiSchemaLinkingService`
  - `LinkSchemaToEntity(apiSpecId, schemaPath, dataEntityId, attributeId?)`
  - `UnlinkSchema(linkId)`
  - `GetLinksForApiSpec(apiSpecId)`
  - `GetLinksForEntity(entityId)`
  - `SuggestLinks(apiSpecId)` - Auto-suggest basé sur noms

- [ ] **1.6.5** Créer `OpenApiParsingService`
  - Parse OpenAPI spec (YAML/JSON)
  - Extract schemas
  - Build schema tree (JSONPath pour chaque property)
  - `GetAllSchemaElements(specContent)` → List<SchemaElement>

- [ ] **1.6.6** Créer `ImpactAnalysisService`
  - `AnalyzeImpact(entityId, proposedChanges)`
    - Récupérer tous les ApiSchemaElements liés
    - Pour chaque API affectée, analyser le type de changement
    - Calculer RiskLevel (Low/Medium/High/Critical)
    - Générer rapport avec suggestions
  - `GetImpactHistory(entityId)`

- [ ] **1.6.7** Créer `RiskCalculator`
  - Règles de calcul du risque:
    - Type change = High risk
    - Required → Optional = Low risk
    - Optional → Required = High risk
    - Rename = Medium risk
    - Delete = Critical risk
    - Add new = Low risk

- [ ] **1.6.8** Créer controllers
  - `ApiSchemaLinkingController`
    - `POST /api/governance/link`
    - `DELETE /api/governance/link/{id}`
    - `GET /api/governance/links?apiSpecId={id}`
    - `GET /api/governance/links?entityId={id}`
    - `POST /api/governance/suggest-links`
  - `ImpactAnalysisController`
    - `POST /api/governance/analyze-impact`
    - `GET /api/governance/impact-analyses?entityId={id}`
    - `GET /api/governance/impact-analyses/{id}`

- [ ] **1.6.9** Créer tests unitaires
  - Test linking logic
  - Test impact analysis
  - Test risk calculation
  - Coverage >80%

- [ ] **1.6.10** Créer tests d'intégration
  - Test scenarios end-to-end
  - Test avec vrais OpenAPI specs

- [ ] **1.6.11** Documenter avec OpenAPI

### 📦 Livrables Semaine 3

- ✅ Data Dictionary Service complet
- ✅ Governance Engine Service complet
- ✅ Linking API ↔ Dictionary fonctionnel
- ✅ Impact Analysis basique
- ✅ Tests: >80% coverage
- ✅ Documentation OpenAPI

### ⏱️ Estimation: 5 jours

---

## Semaine 4: Mock & Lint Services

### ✅ Tâches

#### 1.7 Mock Server Service

- [ ] **1.7.1** Installer packages NuGet
  - Standard EF Core, etc.
  - Aucun package spécial (utilise Prism CLI externe)

- [ ] **1.7.2** Créer entités
  - `MockServer` (Id, ApiSpecId, Port, IsRunning, ProcessId, StartedAt, Configuration JSONB)
  - `MockRequest` (Id, MockServerId, Timestamp, Method, Path, StatusCode, ResponseTime)

- [ ] **1.7.3** Créer DTOs
  - `StartMockServerRequest` (SpecId, Port?, DynamicData, Scenarios?)
  - `StartMockServerResponse` (MockId, Url, Status)
  - `StopMockServerRequest`
  - `MockServerStatusDto`
  - `MockRequestLogDto`

- [ ] **1.7.4** Installer Prism CLI
  - Créer Dockerfile avec Node.js + Prism
  - `npm install -g @stoplight/prism-cli`
  - Test: `prism mock --version`

- [ ] **1.7.5** Créer `PrismService`
  - `StartMockServer(specContent, port, dynamicData)`
    - Sauvegarder spec dans fichier temporaire
    - Lancer process Prism: `prism mock {file} -p {port} -d`
    - Capturer ProcessId
    - Vérifier que le serveur démarre (health check)
  - `StopMockServer(processId)`
    - Kill process
  - `GetServerStatus(mockId)`
    - Check si process tourne

- [ ] **1.7.6** Créer `MockServerService`
  - `StartMock(request)`
    - Récupérer ApiSpec
    - Appeler PrismService.StartMockServer
    - Enregistrer MockServer en DB
  - `StopMock(mockId)`
  - `GetActiveMocks()`
  - `GetMockLogs(mockId)`

- [ ] **1.7.7** Créer controllers
  - `MockServersController`
    - `POST /api/mocks/start`
    - `POST /api/mocks/{id}/stop`
    - `GET /api/mocks/{id}/status`
    - `GET /api/mocks/active`
    - `GET /api/mocks/{id}/logs`

- [ ] **1.7.8** Gérer cleanup
  - Cleanup au shutdown de l'application (kill tous les process)
  - Cleanup des vieux mock servers (timeout)

- [ ] **1.7.9** Créer tests
  - Test PrismService (démarrage/arrêt)
  - Test MockServerService
  - Test controllers
  - Tests d'intégration avec vrai Prism

- [ ] **1.7.10** Documenter avec OpenAPI

#### 1.8 Linting Engine Service

- [ ] **1.8.1** Créer entités
  - `LintRuleset` (Id, Name, Description, RulesContent JSONB, IsDefault)
  - `LintResult` (Id, ApiSpecId, RulesetId, IsValid, Errors JSONB, Warnings JSONB, Info JSONB, Timestamp)

- [ ] **1.8.2** Créer DTOs
  - `ValidateSpecRequest` (SpecContent, RulesetId?)
  - `ValidationResult` (IsValid, Errors[], Warnings[], Info[])
  - `LintRulesetDto`, `CreateRulesetRequest`
  - `LintError` (Code, Message, Path, Severity, Range?)

- [ ] **1.8.3** Installer Spectral CLI
  - Créer Dockerfile avec Node.js + Spectral
  - `npm install -g @stoplight/spectral-cli`
  - Test: `spectral --version`

- [ ] **1.8.4** Créer rulesets Spectral
  - `default-ruleset.yaml` - Règles par défaut Spectral
  - `custom-ruleset.yaml` - Règles custom exemple
  - Stocker dans `/rulesets/`

- [ ] **1.8.5** Créer `SpectralService`
  - `LintSpec(specContent, rulesetPath)`
    - Sauvegarder spec dans fichier temporaire
    - Lancer Spectral: `spectral lint {file} --ruleset {ruleset} --format json`
    - Parser output JSON
    - Mapper vers `ValidationResult`
  - Gestion des erreurs Spectral

- [ ] **1.8.6** Créer `LintingService`
  - `ValidateSpec(request)`
    - Récupérer ruleset (ou default)
    - Appeler SpectralService
    - Enregistrer LintResult en DB
  - `GetRulesets()`
  - `CreateRuleset(request)`
  - `UpdateRuleset(id, request)`
  - `DeleteRuleset(id)`

- [ ] **1.8.7** Créer controllers
  - `LintingController`
    - `POST /api/lint/validate`
    - `GET /api/lint/rulesets`
    - `GET /api/lint/rulesets/{id}`
    - `POST /api/lint/rulesets`
    - `PUT /api/lint/rulesets/{id}`
    - `DELETE /api/lint/rulesets/{id}`

- [ ] **1.8.8** Créer tests
  - Test SpectralService
  - Test LintingService
  - Test avec vrais specs (valid/invalid)
  - Test custom rules

- [ ] **1.8.9** Documenter avec OpenAPI

### 📦 Livrables Semaine 4

- ✅ Mock Server Service complet
- ✅ Linting Engine Service complet
- ✅ Intégration Prism opérationnelle
- ✅ Intégration Spectral opérationnelle
- ✅ Tests: >80% coverage
- ✅ Documentation OpenAPI

### ⏱️ Estimation: 5 jours

---

## 📦 Livrables Phase 1 (Semaines 2-4)

- ✅ **6 services backend opérationnels**:
  - Auth Service
  - API Design Service
  - Data Dictionary Service
  - Governance Engine Service
  - Mock Server Service
  - Linting Engine Service

- ✅ **API Gateway** (YARP) configuré et routant vers tous les services
- ✅ **Authentication** JWT complète
- ✅ **Database** avec migrations et seed data
- ✅ **Tests**: >80% code coverage
- ✅ **Documentation** OpenAPI pour chaque service
- ✅ **Docker** images pour chaque service
- ✅ **Kubernetes** manifests à jour

### ⏱️ Estimation Totale Phase 1: 15 jours (3 semaines)

---

## Phase 2: Frontend Core (Semaines 5-7)

### 🎯 Objectif
Interface utilisateur de base fonctionnelle avec auth et visualisation des API specs

---

## Semaine 5: Setup Frontend & Auth UI

### ✅ Tâches

#### 2.1 Architecture Frontend

- [ ] **2.1.1** Installer dépendances principales
  ```bash
  npm install react-router-dom zustand @tanstack/react-query axios
  npm install zod react-hook-form @hookform/resolvers
  npm install lucide-react clsx tailwind-merge
  npm install @radix-ui/react-dialog @radix-ui/react-dropdown-menu
  ```

- [ ] **2.1.2** Configurer shadcn/ui
  ```bash
  npx shadcn-ui@latest init
  npx shadcn-ui@latest add button card input label toast
  npx shadcn-ui@latest add dialog dropdown-menu avatar
  npx shadcn-ui@latest add table tabs select
  ```

- [ ] **2.1.3** Créer structure de dossiers
  ```
  src/
  ├── components/
  │   ├── ui/              # shadcn components
  │   ├── auth/            # Auth components
  │   ├── layout/          # Layout components
  │   ├── projects/        # Project components
  │   └── common/          # Common components
  ├── pages/
  │   ├── auth/
  │   ├── workspaces/
  │   ├── projects/
  │   └── settings/
  ├── hooks/
  ├── services/
  │   └── api/
  ├── stores/
  ├── types/
  ├── utils/
  └── lib/
  ```

- [ ] **2.1.4** Configurer Tailwind CSS
  - Configuration colors, fonts
  - Dark mode support (optionnel)
  - Custom utilities

- [ ] **2.1.5** Configurer ESLint & Prettier
  - Règles React
  - Import order
  - Prettier config

#### 2.2 API Client Setup

- [ ] **2.2.1** Créer `src/services/api/client.ts`
  - Axios instance avec base URL
  - Interceptors pour JWT token
  - Error handling
  - Retry logic

- [ ] **2.2.2** Créer `src/services/api/auth.api.ts`
  - `login(email, password)`
  - `register(email, password, name)`
  - `refreshToken()`
  - `logout()`
  - `changePassword(oldPassword, newPassword)`

- [ ] **2.2.3** Configurer TanStack Query
  - QueryClient configuration
  - DevTools (dev mode)
  - Default options (staleTime, cacheTime)

#### 2.3 State Management

- [ ] **2.3.1** Créer `src/stores/authStore.ts` (Zustand)
  - State: `user`, `accessToken`, `refreshToken`, `isAuthenticated`
  - Actions: `setAuth`, `clearAuth`, `setUser`
  - Persist to localStorage

- [ ] **2.3.2** Créer `src/stores/uiStore.ts`
  - State: `sidebarOpen`, `theme`
  - Actions: `toggleSidebar`, `setTheme`

#### 2.4 Routing & Navigation

- [ ] **2.4.1** Configurer React Router
  - Routes publiques (Login, Register)
  - Routes protégées (Dashboard, Projects, etc.)
  - 404 page

- [ ] **2.4.2** Créer `src/components/auth/ProtectedRoute.tsx`
  - Vérifier authentication
  - Redirect vers /login si non authentifié

- [ ] **2.4.3** Créer structure de routes
  ```tsx
  /login
  /register
  /
    /workspaces
    /workspaces/:id/projects
    /projects/:id
    /projects/:id/specs/:specId
    /data-dictionary
    /settings
  ```

#### 2.5 Layout Components

- [ ] **2.5.1** Créer `src/components/layout/AppLayout.tsx`
  - Header avec navigation
  - Sidebar
  - Main content area
  - Footer

- [ ] **2.5.2** Créer `src/components/layout/Header.tsx`
  - Logo
  - Breadcrumb navigation
  - User menu (dropdown)
  - Notifications (placeholder)

- [ ] **2.5.3** Créer `src/components/layout/Sidebar.tsx`
  - Navigation links
  - Workspace selector
  - Collapsible
  - Active link highlighting

- [ ] **2.5.4** Créer `src/components/layout/Breadcrumb.tsx`
  - Dynamic breadcrumb basé sur route

#### 2.6 Auth Pages

- [ ] **2.6.1** Créer `src/pages/auth/LoginPage.tsx`
  - Formulaire login (email, password)
  - Validation avec Zod + React Hook Form
  - Error handling
  - Link vers Register
  - "Remember me" (optionnel)

- [ ] **2.6.2** Créer `src/pages/auth/RegisterPage.tsx`
  - Formulaire register (name, email, password, confirm password)
  - Validation
  - Error handling
  - Link vers Login

- [ ] **2.6.3** Créer `src/hooks/useAuth.ts`
  - `login(credentials)`
  - `register(userData)`
  - `logout()`
  - Utilise authStore + TanStack Query

- [ ] **2.6.4** Implémenter auto-refresh token
  - Interceptor Axios pour détecter 401
  - Appeler refresh token endpoint
  - Retry requête originale

#### 2.7 Common Components

- [ ] **2.7.1** Créer `src/components/common/LoadingSpinner.tsx`
  - Spinner pour chargements

- [ ] **2.7.2** Créer `src/components/common/ErrorMessage.tsx`
  - Affichage d'erreurs

- [ ] **2.7.3** Créer `src/components/common/PageHeader.tsx`
  - Header de page avec titre, description, actions

- [ ] **2.7.4** Créer `src/components/common/EmptyState.tsx`
  - État vide avec icône, message, CTA

#### 2.8 Tests Frontend

- [ ] **2.8.1** Configurer Vitest
  - vitest.config.ts
  - Test setup

- [ ] **2.8.2** Configurer React Testing Library
  - Custom render avec providers (Router, Query, Zustand)

- [ ] **2.8.3** Créer tests pour auth
  - Test LoginPage
  - Test RegisterPage
  - Test useAuth hook
  - Test ProtectedRoute

### 📦 Livrables Semaine 5

- ✅ Architecture frontend complète
- ✅ Routing configuré
- ✅ Layout principal
- ✅ Pages d'auth fonctionnelles
- ✅ Authentication flow complet
- ✅ Tests unitaires frontend

### ⏱️ Estimation: 5 jours

---

## Semaine 6: Workspaces & Projects UI

### ✅ Tâches

#### 2.9 Workspaces UI

- [ ] **2.9.1** Créer API client
  - `src/services/api/workspaces.api.ts`
    - `getWorkspaces()`
    - `getWorkspace(id)`
    - `createWorkspace(data)`
    - `updateWorkspace(id, data)`
    - `deleteWorkspace(id)`

- [ ] **2.9.2** Créer types TypeScript
  - `src/types/workspace.ts`
    - `Workspace`, `CreateWorkspaceRequest`, `UpdateWorkspaceRequest`

- [ ] **2.9.3** Créer `src/pages/workspaces/WorkspacesListPage.tsx`
  - Liste des workspaces (cards grid)
  - Bouton "Create Workspace"
  - Search & filters
  - Pagination

- [ ] **2.9.4** Créer `src/components/workspaces/WorkspaceCard.tsx`
  - Affichage workspace (name, description, members count)
  - Actions (edit, delete)
  - Click → Navigate vers projects

- [ ] **2.9.5** Créer `src/components/workspaces/CreateWorkspaceDialog.tsx`
  - Dialog avec formulaire
  - Fields: Name, Description
  - Validation
  - Submit → Create workspace

- [ ] **2.9.6** Créer `src/components/workspaces/EditWorkspaceDialog.tsx`
  - Similar à Create mais avec données existantes
  - Update workspace

- [ ] **2.9.7** Créer hooks
  - `src/hooks/useWorkspaces.ts`
    - `useWorkspaces()` - Get all
    - `useWorkspace(id)` - Get one
    - `useCreateWorkspace()` - Mutation
    - `useUpdateWorkspace()` - Mutation
    - `useDeleteWorkspace()` - Mutation

#### 2.10 Projects UI

- [ ] **2.10.1** Créer API client
  - `src/services/api/projects.api.ts`
    - `getProjects(workspaceId)`
    - `getProject(id)`
    - `createProject(data)`
    - `updateProject(id, data)`
    - `deleteProject(id)`

- [ ] **2.10.2** Créer types TypeScript
  - `src/types/project.ts`
    - `Project`, `CreateProjectRequest`, `UpdateProjectRequest`

- [ ] **2.10.3** Créer `src/pages/projects/ProjectsListPage.tsx`
  - Liste des projets du workspace
  - Table ou cards
  - Bouton "Create Project"
  - Search & filters
  - Sort (date, name)

- [ ] **2.10.4** Créer `src/components/projects/ProjectCard.tsx`
  - Affichage project (name, description, specs count, last updated)
  - Actions (edit, delete, settings)
  - Click → Navigate vers project detail

- [ ] **2.10.5** Créer `src/components/projects/CreateProjectDialog.tsx`
  - Formulaire: Name, Description, Visibility (Public/Private)
  - Git integration fields (optionnel pour Phase 8)

- [ ] **2.10.6** Créer `src/components/projects/EditProjectDialog.tsx`
  - Update project

- [ ] **2.10.7** Créer hooks
  - `src/hooks/useProjects.ts`
    - `useProjects(workspaceId)`
    - `useProject(id)`
    - `useCreateProject()`
    - `useUpdateProject()`
    - `useDeleteProject()`

#### 2.11 Navigation & State

- [ ] **2.11.1** Implémenter workspace context
  - `src/contexts/WorkspaceContext.tsx`
  - Current workspace state
  - Switch workspace action

- [ ] **2.11.2** Update Sidebar
  - Afficher workspaces
  - Switch workspace
  - Afficher projects du workspace courant

- [ ] **2.11.3** Update Breadcrumb
  - Workspace > Projects > Project
  - Cliquable

#### 2.12 Tests

- [ ] **2.12.1** Tests Workspaces
  - Test WorkspacesListPage
  - Test CreateWorkspaceDialog
  - Test hooks

- [ ] **2.12.2** Tests Projects
  - Test ProjectsListPage
  - Test CreateProjectDialog
  - Test hooks

### 📦 Livrables Semaine 6

- ✅ Gestion workspaces complète
- ✅ Gestion projects complète
- ✅ Navigation fonctionnelle
- ✅ Tests frontend

### ⏱️ Estimation: 5 jours

---

## Semaine 7: API Spec Viewer (Read-Only)

### ✅ Tâches

#### 2.13 API Specs API Client

- [ ] **2.13.1** Créer `src/services/api/apiSpecs.api.ts`
  - `getApiSpecs(projectId)`
  - `getApiSpec(id)`
  - `getApiSpecContent(id)`
  - `validateSpec(content)`

- [ ] **2.13.2** Créer types TypeScript
  - `src/types/apiSpec.ts`
    - `ApiSpec`, `OpenApiDocument`, `PathItem`, `Schema`, etc.

#### 2.14 Monaco Editor Setup

- [ ] **2.14.1** Installer Monaco Editor
  ```bash
  npm install @monaco-editor/react monaco-yaml
  ```

- [ ] **2.14.2** Créer `src/components/editor/MonacoYamlEditor.tsx`
  - Monaco Editor wrapper
  - Configuration YAML syntax
  - Read-only mode pour cette phase
  - Syntax highlighting
  - Line numbers

- [ ] **2.14.3** Configurer monaco-yaml
  - OpenAPI schema pour autocompletion (pour Phase 3)
  - Validation

#### 2.15 API Spec Viewer

- [ ] **2.15.1** Créer `src/pages/projects/ProjectDetailPage.tsx`
  - Tabs: Overview, API Specs, Settings
  - Overview: Project info, stats
  - API Specs: Liste des specs
  - Settings: Project settings

- [ ] **2.15.2** Créer `src/pages/specs/ApiSpecViewerPage.tsx`
  - Layout avec sidebar + main content
  - Sidebar: Tree view du spec
  - Main: Affichage du contenu
  - Tabs: Read Mode vs Code Mode

- [ ] **2.15.3** Créer `src/components/specs/SpecTreeView.tsx`
  - Arbre de navigation:
    - Info
    - Servers
    - Paths
      - /users
        - GET
        - POST
      - /users/{id}
        - GET
        - PUT
        - DELETE
    - Components
      - Schemas
      - Parameters
      - Responses
      - Security Schemes
  - Cliquable pour naviguer

- [ ] **2.15.4** Créer `src/components/specs/SpecReadView.tsx`
  - Affichage formaté (pas raw YAML)
  - Section Info: Title, Version, Description
  - Section Servers
  - Section Paths: Pour chaque endpoint
    - Method + Path
    - Summary, Description
    - Parameters (table)
    - Request Body (schema)
    - Responses (table)
  - Section Schemas: Pour chaque schéma
    - Properties (table)
    - Type, Required, Description

- [ ] **2.15.5** Créer `src/components/specs/SpecCodeView.tsx`
  - Monaco Editor en read-only
  - Affichage raw YAML/JSON
  - Syntax highlighting
  - Search in code

- [ ] **2.15.6** Créer `src/components/specs/EndpointCard.tsx`
  - Affichage d'un endpoint
  - Badge HTTP method (coloré)
  - Path
  - Description
  - Parameters, Request, Responses

- [ ] **2.15.7** Créer `src/components/specs/SchemaViewer.tsx`
  - Affichage d'un schéma JSON
  - Table des properties
  - Type, Required, Format, Description
  - Nested schemas

#### 2.16 Search & Navigation

- [ ] **2.16.1** Créer `src/components/specs/SpecSearchBar.tsx`
  - Recherche dans le spec
  - Full-text search
  - Highlight results

- [ ] **2.16.2** Implémenter navigation
  - Click sur tree view → Scroll vers section
  - Smooth scroll
  - Highlight active section

#### 2.17 Tests

- [ ] **2.17.1** Tests Viewer
  - Test SpecTreeView
  - Test SpecReadView
  - Test SpecCodeView
  - Test navigation

### 📦 Livrables Semaine 7

- ✅ Visualisation complète des API specs
- ✅ Monaco Editor intégré
- ✅ Tree view navigation
- ✅ Read mode + Code mode
- ✅ Recherche dans spec
- ✅ Tests frontend

### ⏱️ Estimation: 5 jours

---

## 📦 Livrables Phase 2 (Semaines 5-7)

- ✅ **Frontend fonctionnel** avec navigation complète
- ✅ **Authentication UI** (Login, Register)
- ✅ **Gestion Workspaces** (CRUD)
- ✅ **Gestion Projects** (CRUD)
- ✅ **Visualisation API Specs** (read-only)
- ✅ **Monaco Editor** intégré
- ✅ **Design responsive** avec Tailwind + shadcn/ui
- ✅ **Tests frontend** avec Vitest + React Testing Library

### ⏱️ Estimation Totale Phase 2: 15 jours (3 semaines)

---

# 🎯 RÉSUMÉ - OBJECTIF 1 COMPLÉTÉ

## Livrables Objectif 1

### ✅ 1. Recherche Complète

**Technologies Backend**:
- ✅ .NET 8 avec ASP.NET Core
- ✅ Entity Framework Core 8 pour PostgreSQL
- ✅ YARP pour API Gateway
- ✅ SignalR pour temps réel
- ✅ RabbitMQ (MassTransit) pour messaging
- ✅ Intégration Spectral (linting)
- ✅ Intégration Prism (mock servers)

**Technologies Frontend**:
- ✅ React 18 + TypeScript
- ✅ Vite (build tool)
- ✅ shadcn/ui + Radix UI (components modernes accessibles)
- ✅ Tailwind CSS
- ✅ Zustand (state management)
- ✅ TanStack Query (data fetching)
- ✅ Monaco Editor (éditeur code)
- ✅ SignalR Client (collaboration)

**Infrastructure**:
- ✅ Docker + Docker Compose
- ✅ Kubernetes (Docker Desktop)
- ✅ PostgreSQL 16
- ✅ RabbitMQ
- ✅ Redis
- ✅ MinIO (S3-compatible)
- ✅ Seq (logging)

### ✅ 2. Architecture Complète

- ✅ **Architecture microservices** détaillée avec 10 services
- ✅ **Modèle de données** complet avec schémas SQL
- ✅ **Dictionnaire de données** avec gouvernance intégrée
- ✅ **Analyse d'impact** automatique
- ✅ **Infrastructure as Code** (Docker Compose + K8s manifests)

### ✅ 3. Liste de Tâches Détaillée

- ✅ **Plan de développement** sur 23 semaines (6 mois)
- ✅ **12 phases** structurées
- ✅ **Tâches granulaires** avec estimation
- ✅ **Livrables clairs** pour chaque phase

### ✅ 4. Documentation

- ✅ **ARCHITECTURE.md** - 800+ lignes de documentation complète
- ✅ **TASK_LIST.md** - Liste détaillée de toutes les tâches
- ✅ Prêt pour **Objectif 2**: Développement complet

---

# 📋 PROCHAINES ÉTAPES - OBJECTIF 2

## Démarrage du Développement

Nous avons maintenant tout le nécessaire pour démarrer l'Objectif 2 : **Développement complet de l'application**.

### Approche proposée:

1. **Phase 0** (Semaine 1): Setup infrastructure et structure projet
2. **Phase 1** (Semaines 2-4): Développement backend core services
3. **Phase 2** (Semaines 5-7): Développement frontend core
4. **Phase 3** (Semaines 8-10): Éditeur API complet
5. **Phase 4** (Semaines 11-12): Data Dictionary UI
6. **Phase 5** (Semaines 13-14): Gouvernance & Impact Analysis
7. **Phase 6** (Semaines 15-16): Collaboration temps réel
8. **Phase 7** (Semaine 17): Documentation & Mocking
9. **Phase 8** (Semaine 18): Git Integration
10. **Phase 9** (Semaine 19): Teams & Permissions
11. **Phase 10** (Semaines 20-21): Polish & Optimisation
12. **Phase 11** (Semaine 22): Documentation & Déploiement
13. **Phase 12** (Semaine 23): MVP Release

### Métriques de Qualité à Atteindre:

- ✅ Code Coverage: >80% (backend), >70% (frontend)
- ✅ Code professionnel, commenté, testable
- ✅ 100% fonctionnel dès le premier lancement (`docker-compose up`)
- ✅ Architecture microservices robuste
- ✅ UI moderne avec excellente ergonomie

---

**Prêt à passer à l'Objectif 2 ? 🚀**
