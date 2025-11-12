# APIVIA - API First Design Platform
## Architecture Complète & Plan de Développement

> **Vision**: Créer une plateforme d'API First Design surpassant Stoplight avec une gouvernance des données d'entreprise intégrée

---

## 📋 Table des Matières

1. [Vue d'Ensemble](#vue-densemble)
2. [Architecture Technique](#architecture-technique)
3. [Stack Technologique](#stack-technologique)
4. [Composants Principaux](#composants-principaux)
5. [Fonctionnalités](#fonctionnalités)
6. [Modèle de Données](#modèle-de-données)
7. [Infrastructure](#infrastructure)
8. [Plan de Développement](#plan-de-développement)

---

## 🎯 Vue d'Ensemble

### Objectifs

**Apivia** est une plateforme complète d'API First Design qui combine :
- ✅ Toutes les fonctionnalités de Stoplight
- ✅ Une gouvernance des données d'entreprise intégrée
- ✅ Un dictionnaire de données centralisé
- ✅ Une analyse d'impact automatique
- ✅ Une collaboration en temps réel

### Différenciateurs Clés vs Stoplight

| Fonctionnalité | Stoplight | Apivia |
|----------------|-----------|--------|
| OpenAPI v2/v3 Support | ✅ | ✅ |
| Éditeur Visuel + Code | ✅ | ✅ |
| Mock Servers (Prism) | ✅ | ✅ |
| Linting (Spectral) | ✅ | ✅ |
| Collaboration Temps Réel | ✅ | ✅ |
| Git Integration | ✅ | ✅ |
| **Dictionnaire de Données Entreprise** | ❌ | ✅ |
| **Gouvernance des Données** | ❌ | ✅ |
| **Analyse d'Impact Automatique** | ❌ | ✅ |
| **Référencement Bidirectionnel** | ❌ | ✅ |
| **Métadonnées Métier Enrichies** | ❌ | ✅ |
| **Traçabilité Complète** | ❌ | ✅ |

---

## 🏗️ Architecture Technique

### Pattern Architectural

**Architecture Microservices avec Event-Driven Architecture**

```
┌─────────────────────────────────────────────────────────────┐
│                     FRONTEND (React + shadcn/ui)            │
│  ┌────────────┬──────────────┬──────────────┬─────────────┐│
│  │ API Editor │ Data Dict UI │ Documentation│ Collaboration││
│  └────────────┴──────────────┴──────────────┴─────────────┘│
└───────────────────────────┬─────────────────────────────────┘
                            │ HTTPS / SignalR
┌───────────────────────────┴─────────────────────────────────┐
│                      API GATEWAY (YARP)                     │
└───┬────────┬────────┬────────┬────────┬────────┬───────────┘
    │        │        │        │        │        │
┌───▼───┐┌──▼───┐┌──▼───┐┌───▼───┐┌──▼───┐┌───▼────┐
│ API   ││ Data ││ Mock ││ Lint  ││ Docs ││ Collab │
│Design ││ Dict ││Server││Engine ││ Gen  ││ Hub    │
│Service││Service││Service│Service││Service│Service │
└───┬───┘└──┬───┘└──┬───┘└───┬───┘└──┬───┘└───┬────┘
    │       │       │        │       │        │
    └───────┴───────┴────────┴───────┴────────┘
                    │
    ┌───────────────▼────────────────┐
    │     Event Bus (RabbitMQ)       │
    └────────────────────────────────┘
                    │
    ┌───────────────▼────────────────┐
    │   Database Layer (PostgreSQL)  │
    │  ┌──────────┬─────────────┐   │
    │  │ API Specs│ Data Dict   │   │
    │  │ Projects │ Metadata    │   │
    │  │ Users    │ Audit Trail │   │
    │  └──────────┴─────────────┘   │
    └────────────────────────────────┘
                    │
    ┌───────────────▼────────────────┐
    │  Storage (MinIO/S3-compatible) │
    │  - Documents, Images, Exports  │
    └────────────────────────────────┘
```

### Principes de Design

1. **Separation of Concerns**: Chaque service a une responsabilité unique
2. **Event-Driven**: Communication asynchrone via message bus
3. **API-First**: Chaque service expose une API REST OpenAPI complète
4. **Stateless**: Services sans état pour scalabilité horizontale
5. **12-Factor App**: Application cloud-native
6. **CQRS Pattern**: Séparation lecture/écriture pour la gouvernance
7. **Domain-Driven Design**: Domaines métier clairement définis

---

## 🛠️ Stack Technologique

### Backend (.NET 8)

| Composant | Technologie | Usage |
|-----------|-------------|-------|
| **Framework** | ASP.NET Core 8 | Services REST API |
| **ORM** | Entity Framework Core 8 | Accès données |
| **API Gateway** | YARP (Yet Another Reverse Proxy) | Routage & Load Balancing |
| **Real-Time** | SignalR | Collaboration temps réel |
| **Message Bus** | RabbitMQ (MassTransit) | Event-driven architecture |
| **Cache** | Redis | Cache distribué |
| **OpenAPI Gen** | Microsoft.AspNetCore.OpenApi | Génération OpenAPI native .NET 8 |
| **Validation** | FluentValidation | Validation métier |
| **Mapping** | AutoMapper | Mapping DTO/Entities |
| **Auth** | ASP.NET Core Identity + JWT | Authentification/Autorisation |
| **Testing** | xUnit + FluentAssertions + Moq | Tests unitaires/intégration |
| **Logging** | Serilog + Seq | Logging structuré |
| **Metrics** | Prometheus + Grafana | Observabilité |

### Frontend (React)

| Composant | Technologie | Usage |
|-----------|-------------|-------|
| **Framework** | React 18 + TypeScript | UI Framework |
| **Build Tool** | Vite | Build ultra-rapide |
| **UI Library** | shadcn/ui + Radix UI | Composants modernes accessibles |
| **Styling** | Tailwind CSS | Utility-first CSS |
| **State** | Zustand + TanStack Query | State management & data fetching |
| **Forms** | React Hook Form + Zod | Gestion formulaires & validation |
| **Editor** | Monaco Editor + monaco-yaml | Éditeur code avec autocompletion |
| **Real-Time** | SignalR Client | Collaboration temps réel |
| **Routing** | React Router v6 | Routing |
| **Icons** | Lucide React | Icônes modernes |
| **Tables** | TanStack Table | Tables avancées |
| **Drag & Drop** | dnd-kit | Drag & drop |
| **Testing** | Vitest + React Testing Library | Tests frontend |

### Outils Externes (Intégrés)

| Outil | Technologie | Usage |
|-------|-------------|-------|
| **Linting** | Spectral (Node.js CLI) | Validation OpenAPI |
| **Mock Server** | Prism (Node.js CLI) | Serveurs mock |
| **YAML Parsing** | YamlDotNet | Parsing YAML en .NET |
| **JSON Schema** | NJsonSchema | Validation JSON Schema |

### Infrastructure

| Composant | Technologie | Usage |
|-----------|-------------|-------|
| **Orchestration** | Kubernetes (Docker Desktop) | Orchestration conteneurs |
| **Conteneurs** | Docker | Containerisation |
| **Base de Données** | PostgreSQL 16 | Base principale |
| **Storage** | MinIO | Stockage S3-compatible |
| **Message Broker** | RabbitMQ | Message queuing |
| **Cache** | Redis | Cache distribué |
| **Monitoring** | Prometheus + Grafana | Métriques & dashboards |
| **Logging** | Seq | Centralisation logs |
| **Reverse Proxy** | Traefik (optionnel) | Ingress controller |

---

## 🧩 Composants Principaux

### 1. API Design Service

**Responsabilités**:
- CRUD des spécifications OpenAPI v2/v3
- Gestion des projets et workspaces
- Versioning des spécifications
- Import/Export (YAML, JSON, Postman)
- Validation syntaxique
- Gestion des composants réutilisables ($ref)

**Endpoints Principaux**:
```
POST   /api/projects
GET    /api/projects/{id}
PUT    /api/projects/{id}
DELETE /api/projects/{id}

POST   /api/projects/{id}/specs
GET    /api/projects/{id}/specs/{specId}
PUT    /api/projects/{id}/specs/{specId}

POST   /api/specs/validate
POST   /api/specs/import
POST   /api/specs/export
```

### 2. Data Dictionary Service

**Responsabilités**:
- Gestion du dictionnaire de données entreprise
- Définitions canoniques des entités métier
- Référencement bidirectionnel avec API specs
- Enrichissement métadonnées (propriétaire, domaine, sensibilité)
- Gestion des taxonomies et ontologies
- Historique des modifications

**Modèle de Données**:
```csharp
public class DataEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string DisplayName { get; set; }
    public string Description { get; set; }
    public string BusinessOwner { get; set; }
    public string FunctionalDomain { get; set; }
    public DataSensitivityLevel Sensitivity { get; set; }
    public DataQualityLevel QualityLevel { get; set; }
    public List<DataAttribute> Attributes { get; set; }
    public List<ApiReference> ApiReferences { get; set; }
    public AuditInfo Audit { get; set; }
}

public class DataAttribute
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string DataType { get; set; }
    public bool IsRequired { get; set; }
    public string Format { get; set; }
    public List<string> AllowedValues { get; set; }
    public Dictionary<string, object> Metadata { get; set; }
}

public class ApiReference
{
    public Guid ApiSpecId { get; set; }
    public string SchemaPath { get; set; } // e.g., "#/components/schemas/Customer/properties/id"
    public DateTime LinkedAt { get; set; }
}
```

**Endpoints Principaux**:
```
POST   /api/data-dictionary/entities
GET    /api/data-dictionary/entities/{id}
PUT    /api/data-dictionary/entities/{id}
DELETE /api/data-dictionary/entities/{id}

POST   /api/data-dictionary/entities/{id}/link-api
DELETE /api/data-dictionary/entities/{id}/unlink-api/{apiSpecId}

GET    /api/data-dictionary/search?term={term}
GET    /api/data-dictionary/domains
```

### 3. Governance Engine Service

**Responsabilités**:
- Analyse d'impact des modifications
- Détection des dépendances
- Validation des règles de gouvernance
- Génération de rapports d'impact
- Alertes sur modifications critiques
- Traçabilité complète

**Fonctionnalités Clés**:
- **Impact Analysis**: Quand un DataEntity change, identifier toutes les APIs affectées
- **Validation Rules**: Règles métier sur les données (nommage, types, sensibilité)
- **Propagation**: Proposer automatiquement les mises à jour dans les API specs
- **Audit Trail**: Historique complet de toutes les modifications

**Endpoints Principaux**:
```
POST   /api/governance/analyze-impact
       Body: { entityId, changeType, proposedChanges }
       Response: { affectedApis[], affectedProjects[], riskLevel }

POST   /api/governance/validate
       Body: { apiSpec, rules[] }
       Response: { isValid, violations[] }

GET    /api/governance/audit-trail/{entityId}
GET    /api/governance/compliance-report
```

### 4. Mock Server Service

**Responsabilités**:
- Génération de serveurs mock à partir de OpenAPI specs
- Intégration avec Prism CLI
- Gestion du cycle de vie des mocks (start/stop)
- Données de mock réalistes (Faker)
- Scénarios de test personnalisés

**Endpoints Principaux**:
```
POST   /api/mocks/start
       Body: { specId, port?, dynamicData: true/false }
       Response: { mockId, url, status }

POST   /api/mocks/stop/{mockId}
GET    /api/mocks/{mockId}/status
GET    /api/mocks/active
```

### 5. Linting Engine Service

**Responsabilités**:
- Intégration avec Spectral CLI
- Règles de style personnalisables
- Validation OpenAPI best practices
- Rapports de qualité
- Intégration CI/CD

**Endpoints Principaux**:
```
POST   /api/lint/validate
       Body: { spec, rulesetId? }
       Response: { isValid, errors[], warnings[], info[] }

GET    /api/lint/rulesets
POST   /api/lint/rulesets
PUT    /api/lint/rulesets/{id}
```

### 6. Documentation Generator Service

**Responsabilités**:
- Génération de documentation API Reference
- Support Markdown pour guides
- Gestion d'images et assets
- Thèmes personnalisables
- Export PDF/HTML
- Publication automatique

**Endpoints Principaux**:
```
POST   /api/docs/generate/{specId}
GET    /api/docs/{projectId}
POST   /api/docs/{projectId}/publish
GET    /api/docs/{projectId}/export?format=pdf|html
```

### 7. Collaboration Hub Service

**Responsabilités**:
- Collaboration temps réel (SignalR)
- Gestion des curseurs et sélections
- Commentaires et discussions
- Proposals avec diff visuel
- Notifications en temps réel
- Présence utilisateurs

**SignalR Hubs**:
```csharp
public class ProjectCollaborationHub : Hub
{
    public async Task JoinProject(Guid projectId);
    public async Task LeaveProject(Guid projectId);
    public async Task SendCursorPosition(Guid projectId, CursorPosition position);
    public async Task SendEdit(Guid projectId, EditOperation edit);
    public async Task AddComment(Guid projectId, Comment comment);
    public async Task UpdatePresence(Guid projectId, UserPresence presence);
}
```

**Endpoints Principaux**:
```
POST   /api/collaboration/comments
GET    /api/collaboration/comments?projectId={id}
POST   /api/collaboration/proposals
GET    /api/collaboration/proposals/{id}
POST   /api/collaboration/proposals/{id}/approve
```

### 8. User & Team Management Service

**Responsabilités**:
- Authentification (JWT)
- Gestion utilisateurs
- Gestion des équipes
- Rôles et permissions (RBAC)
- Gestion des workspaces
- Invitations

**Rôles**:
- **Owner**: Propriétaire du workspace
- **Admin**: Administration complète
- **Editor**: Édition des projets
- **Viewer**: Lecture seule
- **Guest**: Accès limité à certains projets

**Endpoints Principaux**:
```
POST   /api/auth/register
POST   /api/auth/login
POST   /api/auth/refresh

GET    /api/workspaces
POST   /api/workspaces
GET    /api/workspaces/{id}/teams
POST   /api/workspaces/{id}/teams
POST   /api/workspaces/{id}/invite
```

### 9. Git Integration Service

**Responsabilités**:
- Synchronisation avec Git repositories
- Support GitHub, GitLab, Bitbucket
- Webhooks pour auto-update
- Branch management
- Commit & Push des modifications
- Conflict resolution UI

**Endpoints Principaux**:
```
POST   /api/git/connect
       Body: { provider, repoUrl, branch, credentials }

POST   /api/git/sync/{projectId}
POST   /api/git/commit
POST   /api/git/push
GET    /api/git/status/{projectId}
```

---

## ✨ Fonctionnalités

### Fonctionnalités Core (Parité Stoplight)

#### 1. Éditeur API Design

**Vue Formulaire (Form-Based)**:
- 📝 Création visuelle des endpoints (path, method, parameters)
- 🎨 Interface drag-and-drop pour réorganiser
- 📋 Formulaires intelligents avec validation
- 🔄 Auto-complétion des références ($ref)

**Vue Code**:
- 💻 Éditeur Monaco avec syntax highlighting
- 🔍 Autocompletion YAML/JSON avec schéma OpenAPI
- ✅ Validation en temps réel
- 🔀 Bascule instantanée Form ↔ Code

**Modélisation JSON Schema**:
- 📊 Éditeur graphique pour modèles de données
- 🔧 Support complet JSON Schema Draft 7+
- 🔗 Références croisées entre schémas
- 📚 Bibliothèque de composants réutilisables

#### 2. Documentation

- 📖 API Reference auto-générée
- ✍️ Éditeur Markdown intégré pour guides
- 🖼️ Gestion d'images et médias
- 🎨 Thèmes personnalisables
- 🔍 Recherche full-text
- 📤 Export PDF/HTML
- 🌐 Publication automatique sur URL publique

#### 3. Mock Servers

- 🚀 Génération automatique via Prism
- 🎲 Données dynamiques avec Faker.js
- 📋 Exemples statiques depuis le spec
- 🔧 Configuration de scénarios
- 🌐 URLs accessibles pour tests frontend
- 📊 Logs des requêtes mock

#### 4. Linting & Validation

- ✅ Validation OpenAPI v2/v3
- 📏 Règles Spectral personnalisables
- 🎯 Style guides d'entreprise
- 📊 Rapports de qualité
- 🚨 Alertes en temps réel dans l'éditeur
- 🔧 Quick fixes suggérés

#### 5. Collaboration

- 👥 Édition collaborative temps réel
- 👁️ Curseurs et sélections des autres utilisateurs
- 💬 Commentaires et discussions
- 📝 Proposals avec diff visuel
- 🔔 Notifications en temps réel
- ✅ Workflow de review & approval

#### 6. Git Integration

- 🔄 Sync bidirectionnel avec Git
- 🌿 Gestion des branches
- 📝 Commits depuis l'interface
- 🔀 Merge & conflict resolution
- 🪝 Webhooks pour auto-update
- 📊 History & blame

#### 7. Teams & Permissions

- 👥 Workspaces multi-tenants
- 🏢 Équipes et groupes
- 🔐 RBAC (Owner, Admin, Editor, Viewer, Guest)
- 🎯 Permissions granulaires par projet
- 📧 Invitations par email
- 🔒 Projets privés/publics

### Fonctionnalités Avancées (Au-delà de Stoplight)

#### 8. Dictionnaire de Données Entreprise

**Gestion Centralisée**:
- 📚 Définitions canoniques de toutes les entités métier
- 🏷️ Taxonomies et ontologies
- 📊 Attributs avec métadonnées enrichies
- 👤 Propriétaires métier et domaines fonctionnels
- 🔒 Niveaux de sensibilité (RGPD, confidentialité)
- ⭐ Niveaux de qualité des données

**Interface Utilisateur**:
- 🔍 Recherche avancée par nom, domaine, propriétaire
- 📊 Vue hiérarchique des entités
- 🎨 Éditeur visuel pour définir les attributs
- 📈 Statistiques d'utilisation
- 📖 Documentation métier intégrée

#### 9. Référencement Bidirectionnel

**Liaison API ↔ Dictionnaire**:
- 🔗 Chaque schéma d'API peut être lié à une entité du dictionnaire
- 🔀 Synchronisation des définitions
- 📍 Traçabilité complète : "Customer.id" → DataEntity["Customer"]["id"]
- 🎯 Mapping automatique par convention de nommage
- ✏️ Mapping manuel pour cas particuliers

**Interface de Liaison**:
```
┌─────────────────────────────────────────┐
│ API Schema: Customer                    │
├─────────────────────────────────────────┤
│ Properties:                             │
│  ├─ id (string)                         │
│  │   🔗 Linked to: DataDictionary      │
│  │      Entity: Customer                │
│  │      Attribute: CustomerID           │
│  │   📋 Copy metadata                   │
│  │                                       │
│  ├─ email (string)                      │
│  │   ⚠️ Not linked - Suggest link?     │
│  │                                       │
│  └─ createdAt (datetime)                │
│      🔗 Linked to: AuditInfo.CreatedDate│
└─────────────────────────────────────────┘
```

#### 10. Analyse d'Impact Automatique

**Détection des Impacts**:
- 🔍 Analyse des dépendances en temps réel
- 📊 Rapport d'impact visuel (graphe de dépendances)
- 🎯 Identification des APIs affectées par un changement
- ⚠️ Niveau de risque (Low/Medium/High/Critical)
- 📝 Suggestions de migration

**Scénarios**:
1. **Modification d'une entité du dictionnaire**:
   - → Liste des APIs qui utilisent cette entité
   - → Proposition de mise à jour automatique
   - → Validation des règles de compatibilité

2. **Breaking change détecté**:
   - → Alerte aux équipes concernées
   - → Création automatique d'une issue/proposal
   - → Workflow d'approbation

**Exemple de Rapport**:
```
Impact Analysis Report
═══════════════════════════════════════════

Entity Modified: Customer.email
Change Type: Type change (string → object)
Risk Level: 🔴 HIGH (Breaking Change)

Affected APIs (3):
┌─────────────────────────────────────────┐
│ ❌ Customer Management API v2.1         │
│    - GET /customers/{id}                │
│    - POST /customers                    │
│    Impact: Breaking change              │
│    Suggestion: Create v2.2              │
├─────────────────────────────────────────┤
│ ⚠️  Billing API v1.3                    │
│    - GET /invoices                      │
│    Impact: Data structure change        │
│    Suggestion: Add migration script     │
├─────────────────────────────────────────┤
│ ✅ Analytics API v3.0                   │
│    - No impact (uses old version)       │
└─────────────────────────────────────────┘

Recommended Actions:
1. Create proposal for Customer API v2.2
2. Notify Billing team (#billing-team)
3. Schedule migration meeting
4. Update documentation
```

#### 11. Propagation des Modifications

**Workflow**:
1. Utilisateur modifie une entité dans le dictionnaire
2. Système détecte les APIs liées
3. Génération automatique de proposals pour chaque API
4. Review & approval par les équipes concernées
5. Application des changements avec versioning

**Options de Propagation**:
- ✅ **Auto-sync**: Mise à jour automatique (non-breaking changes)
- 📝 **Proposal**: Création de proposal pour review (breaking changes)
- 🔔 **Notify Only**: Juste une notification
- ❌ **Block**: Bloquer le changement si incompatible

#### 12. Métadonnées Métier Enrichies

**Métadonnées Disponibles**:
```json
{
  "businessMetadata": {
    "displayName": "Identifiant Client",
    "description": "Identifiant unique du client dans le système",
    "businessOwner": "Marie Dupont",
    "businessOwnerEmail": "marie.dupont@company.com",
    "functionalDomain": "Gestion Clients",
    "dataClassification": "Confidential",
    "piiData": true,
    "gdprCategory": "Personal Identifier",
    "retentionPeriod": "7 years",
    "qualityLevel": "Gold",
    "qualityMetrics": {
      "completeness": 0.98,
      "accuracy": 0.95,
      "consistency": 0.99
    },
    "lineage": {
      "sourceSystem": "CRM",
      "lastModified": "2025-01-15",
      "modifiedBy": "john.doe@company.com"
    },
    "compliance": {
      "rgpd": true,
      "sox": false,
      "hipaa": false
    }
  }
}
```

**Injection dans OpenAPI**:
Les métadonnées sont injectées comme extensions OpenAPI (`x-*`):

```yaml
components:
  schemas:
    Customer:
      type: object
      x-business-owner: "Marie Dupont"
      x-functional-domain: "Gestion Clients"
      x-data-classification: "Confidential"
      properties:
        id:
          type: string
          description: "Identifiant unique du client dans le système"
          x-pii-data: true
          x-gdpr-category: "Personal Identifier"
          x-quality-level: "Gold"
          x-linked-to-dictionary: "entities/customer/attributes/id"
```

#### 13. Audit & Traçabilité

**Audit Trail Complet**:
- 📝 Historique de toutes les modifications
- 👤 Qui a modifié quoi et quand
- 🔄 Diff visuel entre versions
- 📊 Rapports d'audit
- 🔍 Recherche dans l'historique
- 📤 Export pour conformité

**Données Auditées**:
- Modifications des API specs
- Modifications du dictionnaire de données
- Liaisons/déliaisons API ↔ Dictionnaire
- Changements de permissions
- Accès aux données sensibles
- Actions de gouvernance

#### 14. Compliance & RGPD

**Fonctionnalités**:
- 🔒 Marquage des données PII
- 📋 Génération de registres RGPD
- ⚠️ Alertes sur exposition de données sensibles
- 📊 Rapports de conformité
- 🔍 Data discovery automatique
- 🗑️ Support du droit à l'oubli

---

## 💾 Modèle de Données

### Schéma Principal

```
┌──────────────────┐       ┌──────────────────┐
│   Workspace      │       │   User           │
├──────────────────┤       ├──────────────────┤
│ Id               │◄─────┐│ Id               │
│ Name             │      ││ Email            │
│ OwnerId          │──────┘│ PasswordHash     │
│ Settings         │       │ CreatedAt        │
└────────┬─────────┘       └──────────────────┘
         │
         │ 1:N
         │
┌────────▼─────────┐       ┌──────────────────┐
│   Team           │       │ TeamMember       │
├──────────────────┤       ├──────────────────┤
│ Id               │◄──────┤ TeamId           │
│ WorkspaceId      │       │ UserId           │
│ Name             │       │ Role             │
└────────┬─────────┘       └──────────────────┘
         │
         │ N:M
         │
┌────────▼─────────┐       ┌──────────────────┐
│   Project        │       │   ApiSpec        │
├──────────────────┤       ├──────────────────┤
│ Id               │       │ Id               │
│ WorkspaceId      │       │ ProjectId        │
│ Name             │       │ Version          │
│ Description      │◄──────┤ Content (YAML)   │
│ Visibility       │       │ Status           │
│ GitRepoUrl       │       │ CreatedAt        │
│ GitBranch        │       │ ModifiedAt       │
└────────┬─────────┘       └────────┬─────────┘
         │                          │
         │                          │ 1:N
         │                          │
         │                 ┌────────▼─────────┐
         │                 │ ApiSchemaElement │
         │                 ├──────────────────┤
         │                 │ Id               │
         │                 │ ApiSpecId        │
         │                 │ SchemaPath       │
         │                 │ DataEntityId     │◄─┐
         │                 │ LinkedAt         │  │
         │                 └──────────────────┘  │
         │                                       │
         │                                       │
┌────────▼─────────┐       ┌────────────────────┴┐
│ DataDictionary   │       │   DataEntity        │
├──────────────────┤       ├─────────────────────┤
│ Id               │       │ Id                  │
│ WorkspaceId      │◄──────┤ DictionaryId        │
│ Name             │       │ Name                │
│ Description      │       │ DisplayName         │
└──────────────────┘       │ Description         │
                           │ BusinessOwner       │
                           │ FunctionalDomain    │
                           │ Sensitivity         │
                           │ QualityLevel        │
                           │ Metadata (JSONB)    │
                           └──────────┬──────────┘
                                      │
                                      │ 1:N
                                      │
                           ┌──────────▼──────────┐
                           │ DataAttribute       │
                           ├─────────────────────┤
                           │ Id                  │
                           │ DataEntityId        │
                           │ Name                │
                           │ DataType            │
                           │ IsRequired          │
                           │ Format              │
                           │ Metadata (JSONB)    │
                           └─────────────────────┘

┌──────────────────┐       ┌──────────────────┐
│ Comment          │       │ Proposal         │
├──────────────────┤       ├──────────────────┤
│ Id               │       │ Id               │
│ ProjectId        │       │ ProjectId        │
│ UserId           │       │ CreatedBy        │
│ Content          │       │ Title            │
│ Position         │       │ Changes (JSON)   │
│ CreatedAt        │       │ Status           │
└──────────────────┘       └──────────────────┘

┌──────────────────┐       ┌──────────────────┐
│ AuditLog         │       │ ImpactAnalysis   │
├──────────────────┤       ├──────────────────┤
│ Id               │       │ Id               │
│ EntityType       │       │ DataEntityId     │
│ EntityId         │       │ ChangeType       │
│ Action           │       │ AffectedApis     │
│ UserId           │       │ RiskLevel        │
│ Changes (JSON)   │       │ CreatedAt        │
│ Timestamp        │       └──────────────────┘
└──────────────────┘
```

### Schémas Détaillés

#### ApiSpec Table

```sql
CREATE TABLE ApiSpecs (
    Id UUID PRIMARY KEY,
    ProjectId UUID NOT NULL REFERENCES Projects(Id),
    Version VARCHAR(50) NOT NULL,
    Content TEXT NOT NULL, -- YAML/JSON content
    Format VARCHAR(10) NOT NULL, -- 'yaml' or 'json'
    OpenApiVersion VARCHAR(10) NOT NULL, -- '2.0', '3.0', '3.1'
    Status VARCHAR(20) NOT NULL, -- 'draft', 'published', 'deprecated'
    CreatedAt TIMESTAMPTZ NOT NULL,
    ModifiedAt TIMESTAMPTZ NOT NULL,
    CreatedBy UUID NOT NULL REFERENCES Users(Id),
    ModifiedBy UUID NOT NULL REFERENCES Users(Id),
    GitCommitSha VARCHAR(40),
    CONSTRAINT UQ_Project_Version UNIQUE (ProjectId, Version)
);

CREATE INDEX IX_ApiSpecs_ProjectId ON ApiSpecs(ProjectId);
CREATE INDEX IX_ApiSpecs_Status ON ApiSpecs(Status);
```

#### DataEntity Table

```sql
CREATE TABLE DataEntities (
    Id UUID PRIMARY KEY,
    DictionaryId UUID NOT NULL REFERENCES DataDictionaries(Id),
    Name VARCHAR(255) NOT NULL,
    DisplayName VARCHAR(255) NOT NULL,
    Description TEXT,
    BusinessOwner VARCHAR(255),
    BusinessOwnerEmail VARCHAR(255),
    FunctionalDomain VARCHAR(255),
    Sensitivity VARCHAR(50), -- 'Public', 'Internal', 'Confidential', 'Restricted'
    QualityLevel VARCHAR(50), -- 'Bronze', 'Silver', 'Gold', 'Platinum'
    Metadata JSONB, -- Flexible metadata storage
    CreatedAt TIMESTAMPTZ NOT NULL,
    ModifiedAt TIMESTAMPTZ NOT NULL,
    CreatedBy UUID NOT NULL REFERENCES Users(Id),
    ModifiedBy UUID NOT NULL REFERENCES Users(Id),
    Version INT NOT NULL DEFAULT 1,
    IsActive BOOLEAN NOT NULL DEFAULT TRUE,
    CONSTRAINT UQ_Dictionary_Name UNIQUE (DictionaryId, Name)
);

CREATE INDEX IX_DataEntities_DictionaryId ON DataEntities(DictionaryId);
CREATE INDEX IX_DataEntities_FunctionalDomain ON DataEntities(FunctionalDomain);
CREATE INDEX IX_DataEntities_Sensitivity ON DataEntities(Sensitivity);
CREATE INDEX IX_DataEntities_Name ON DataEntities(Name);
CREATE INDEX IX_DataEntities_Metadata ON DataEntities USING GIN(Metadata);
```

#### ApiSchemaElement Table (Lien API ↔ Dictionary)

```sql
CREATE TABLE ApiSchemaElements (
    Id UUID PRIMARY KEY,
    ApiSpecId UUID NOT NULL REFERENCES ApiSpecs(Id),
    SchemaPath VARCHAR(500) NOT NULL, -- JSONPath: '#/components/schemas/Customer/properties/id'
    DataEntityId UUID REFERENCES DataEntities(Id),
    DataAttributeId UUID REFERENCES DataAttributes(Id),
    LinkedAt TIMESTAMPTZ NOT NULL,
    LinkedBy UUID NOT NULL REFERENCES Users(Id),
    AutoSynced BOOLEAN NOT NULL DEFAULT FALSE,
    CONSTRAINT UQ_ApiSpec_SchemaPath UNIQUE (ApiSpecId, SchemaPath)
);

CREATE INDEX IX_ApiSchemaElements_ApiSpecId ON ApiSchemaElements(ApiSpecId);
CREATE INDEX IX_ApiSchemaElements_DataEntityId ON ApiSchemaElements(DataEntityId);
```

#### ImpactAnalysis Table

```sql
CREATE TABLE ImpactAnalyses (
    Id UUID PRIMARY KEY,
    DataEntityId UUID REFERENCES DataEntities(Id),
    ChangeType VARCHAR(50) NOT NULL, -- 'TypeChange', 'Rename', 'Delete', 'AddAttribute', etc.
    ProposedChanges JSONB NOT NULL,
    AffectedApis JSONB NOT NULL, -- Array of { apiSpecId, paths[], riskLevel }
    OverallRiskLevel VARCHAR(20) NOT NULL, -- 'Low', 'Medium', 'High', 'Critical'
    Status VARCHAR(20) NOT NULL, -- 'Pending', 'Approved', 'Rejected', 'Applied'
    CreatedAt TIMESTAMPTZ NOT NULL,
    CreatedBy UUID NOT NULL REFERENCES Users(Id),
    ResolvedAt TIMESTAMPTZ,
    ResolvedBy UUID REFERENCES Users(Id)
);

CREATE INDEX IX_ImpactAnalyses_DataEntityId ON ImpactAnalyses(DataEntityId);
CREATE INDEX IX_ImpactAnalyses_Status ON ImpactAnalyses(Status);
```

---

## 🐳 Infrastructure

### Architecture Kubernetes

```yaml
# Vue d'ensemble des services Kubernetes

Namespace: apivia

Deployments:
- apivia-gateway          (1 replica)  → YARP API Gateway
- apivia-api-design       (2 replicas) → API Design Service
- apivia-data-dict        (2 replicas) → Data Dictionary Service
- apivia-governance       (2 replicas) → Governance Engine
- apivia-mock             (1 replica)  → Mock Server Service
- apivia-lint             (1 replica)  → Linting Engine
- apivia-docs             (1 replica)  → Documentation Generator
- apivia-collab           (2 replicas) → Collaboration Hub
- apivia-auth             (2 replicas) → Auth Service
- apivia-git              (1 replica)  → Git Integration
- apivia-frontend         (2 replicas) → React Frontend

StatefulSets:
- postgres                (1 replica)  → PostgreSQL Database
- rabbitmq                (1 replica)  → RabbitMQ Message Broker
- redis                   (1 replica)  → Redis Cache
- minio                   (1 replica)  → MinIO Storage
- seq                     (1 replica)  → Seq Logging

Services:
- All deployments exposed via ClusterIP
- apivia-gateway exposed via LoadBalancer (port 80/443)
- External access: apivia-gateway only

Volumes:
- postgres-data           → PVC 10Gi
- rabbitmq-data           → PVC 5Gi
- redis-data              → PVC 2Gi
- minio-data              → PVC 20Gi
- seq-data                → PVC 5Gi
```

### Docker Compose (pour développement local)

```yaml
version: '3.8'

services:
  # Infrastructure
  postgres:
    image: postgres:16-alpine
    environment:
      POSTGRES_DB: apivia
      POSTGRES_USER: apivia
      POSTGRES_PASSWORD: ${POSTGRES_PASSWORD}
    volumes:
      - postgres-data:/var/lib/postgresql/data
    ports:
      - "5432:5432"

  rabbitmq:
    image: rabbitmq:3-management-alpine
    environment:
      RABBITMQ_DEFAULT_USER: apivia
      RABBITMQ_DEFAULT_PASS: ${RABBITMQ_PASSWORD}
    volumes:
      - rabbitmq-data:/var/lib/rabbitmq
    ports:
      - "5672:5672"
      - "15672:15672"

  redis:
    image: redis:7-alpine
    volumes:
      - redis-data:/data
    ports:
      - "6379:6379"

  minio:
    image: minio/minio
    command: server /data --console-address ":9001"
    environment:
      MINIO_ROOT_USER: apivia
      MINIO_ROOT_PASSWORD: ${MINIO_PASSWORD}
    volumes:
      - minio-data:/data
    ports:
      - "9000:9000"
      - "9001:9001"

  seq:
    image: datalust/seq
    environment:
      ACCEPT_EULA: Y
    volumes:
      - seq-data:/data
    ports:
      - "5341:80"

  # Backend Services
  gateway:
    build: ./src/Gateway
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
    ports:
      - "5000:8080"
    depends_on:
      - api-design
      - data-dict
      - governance

  api-design:
    build: ./src/Services/ApiDesign
    environment:
      - ConnectionStrings__DefaultConnection=Host=postgres;Database=apivia;Username=apivia;Password=${POSTGRES_PASSWORD}
      - RabbitMQ__Host=rabbitmq
      - Redis__Configuration=redis:6379
    depends_on:
      - postgres
      - rabbitmq
      - redis

  data-dict:
    build: ./src/Services/DataDictionary
    environment:
      - ConnectionStrings__DefaultConnection=Host=postgres;Database=apivia;Username=apivia;Password=${POSTGRES_PASSWORD}
      - RabbitMQ__Host=rabbitmq
      - Redis__Configuration=redis:6379
    depends_on:
      - postgres
      - rabbitmq
      - redis

  governance:
    build: ./src/Services/Governance
    environment:
      - ConnectionStrings__DefaultConnection=Host=postgres;Database=apivia;Username=apivia;Password=${POSTGRES_PASSWORD}
      - RabbitMQ__Host=rabbitmq
      - Redis__Configuration=redis:6379
    depends_on:
      - postgres
      - rabbitmq
      - redis

  mock:
    build: ./src/Services/MockServer
    environment:
      - ConnectionStrings__DefaultConnection=Host=postgres;Database=apivia;Username=apivia;Password=${POSTGRES_PASSWORD}
    depends_on:
      - postgres

  lint:
    build: ./src/Services/Linting
    environment:
      - ConnectionStrings__DefaultConnection=Host=postgres;Database=apivia;Username=apivia;Password=${POSTGRES_PASSWORD}
    depends_on:
      - postgres

  docs:
    build: ./src/Services/Documentation
    environment:
      - ConnectionStrings__DefaultConnection=Host=postgres;Database=apivia;Username=apivia;Password=${POSTGRES_PASSWORD}
      - MinIO__Endpoint=minio:9000
      - MinIO__AccessKey=apivia
      - MinIO__SecretKey=${MINIO_PASSWORD}
    depends_on:
      - postgres
      - minio

  collab:
    build: ./src/Services/Collaboration
    environment:
      - ConnectionStrings__DefaultConnection=Host=postgres;Database=apivia;Username=apivia;Password=${POSTGRES_PASSWORD}
      - RabbitMQ__Host=rabbitmq
      - Redis__Configuration=redis:6379
    depends_on:
      - postgres
      - rabbitmq
      - redis

  auth:
    build: ./src/Services/Auth
    environment:
      - ConnectionStrings__DefaultConnection=Host=postgres;Database=apivia;Username=apivia;Password=${POSTGRES_PASSWORD}
      - Jwt__Secret=${JWT_SECRET}
      - Jwt__Issuer=apivia
      - Jwt__Audience=apivia
    depends_on:
      - postgres

  git-integration:
    build: ./src/Services/GitIntegration
    environment:
      - ConnectionStrings__DefaultConnection=Host=postgres;Database=apivia;Username=apivia;Password=${POSTGRES_PASSWORD}
    depends_on:
      - postgres

  # Frontend
  frontend:
    build: ./src/Frontend
    environment:
      - VITE_API_URL=http://localhost:5000
    ports:
      - "3000:80"
    depends_on:
      - gateway

volumes:
  postgres-data:
  rabbitmq-data:
  redis-data:
  minio-data:
  seq-data:
```

### Configuration Kubernetes

Tous les manifests seront dans `/k8s/`:

```
k8s/
├── namespace.yaml
├── configmaps/
│   ├── app-config.yaml
│   └── logging-config.yaml
├── secrets/
│   ├── postgres-secret.yaml
│   ├── rabbitmq-secret.yaml
│   └── jwt-secret.yaml
├── storage/
│   ├── postgres-pvc.yaml
│   ├── rabbitmq-pvc.yaml
│   ├── redis-pvc.yaml
│   ├── minio-pvc.yaml
│   └── seq-pvc.yaml
├── infrastructure/
│   ├── postgres.yaml
│   ├── rabbitmq.yaml
│   ├── redis.yaml
│   ├── minio.yaml
│   └── seq.yaml
├── services/
│   ├── gateway.yaml
│   ├── api-design.yaml
│   ├── data-dict.yaml
│   ├── governance.yaml
│   ├── mock.yaml
│   ├── lint.yaml
│   ├── docs.yaml
│   ├── collab.yaml
│   ├── auth.yaml
│   └── git-integration.yaml
├── frontend/
│   └── frontend.yaml
└── ingress.yaml
```

---

## 📅 Plan de Développement

### Phase 0: Setup & Infrastructure (Semaine 1)

**Objectif**: Environnement de développement prêt

#### Tâches:

1. **Structure du projet**
   - ✅ Créer structure de solution .NET
   - ✅ Créer projet React avec Vite
   - ✅ Configuration Git et GitFlow
   - ✅ Setup CI/CD (GitHub Actions)

2. **Infrastructure as Code**
   - ✅ Docker Compose pour dev local
   - ✅ Manifests Kubernetes
   - ✅ Scripts de déploiement
   - ✅ Helm charts (optionnel)

3. **Base de données**
   - ✅ Schéma PostgreSQL initial
   - ✅ Migrations Entity Framework
   - ✅ Seed data pour développement

4. **Outils de développement**
   - ✅ EditorConfig, .gitignore
   - ✅ ESLint, Prettier pour frontend
   - ✅ StyleCop, SonarAnalyzer pour backend
   - ✅ Tests templates

**Livrables**:
- ✅ Repository configuré
- ✅ Infrastructure locale fonctionnelle (docker-compose up)
- ✅ CI/CD pipeline basique
- ✅ Documentation de setup

---

### Phase 1: Core Backend Services (Semaines 2-4)

**Objectif**: Services backend principaux opérationnels

#### Semaine 2: Auth & API Design Service

**Auth Service**:
- ✅ User registration, login
- ✅ JWT token generation & validation
- ✅ Password hashing (BCrypt)
- ✅ Refresh tokens
- ✅ Email verification (optionnel)
- ✅ Tests unitaires

**API Design Service**:
- ✅ CRUD Projects
- ✅ CRUD API Specs (YAML/JSON)
- ✅ Validation OpenAPI v2/v3
- ✅ Versioning
- ✅ Import/Export
- ✅ Tests unitaires & intégration

**Gateway**:
- ✅ YARP configuration
- ✅ Routing vers services
- ✅ JWT authentication middleware
- ✅ CORS configuration

#### Semaine 3: Data Dictionary & Governance

**Data Dictionary Service**:
- ✅ CRUD Data Entities
- ✅ CRUD Data Attributes
- ✅ Métadonnées JSONB
- ✅ Taxonomies et domaines
- ✅ API de recherche
- ✅ Tests unitaires

**Governance Engine Service**:
- ✅ Linking API schemas ↔ Data entities
- ✅ Détection des modifications
- ✅ Analyse d'impact basique
- ✅ Calcul du risk level
- ✅ Tests unitaires

#### Semaine 4: Mock & Lint Services

**Mock Server Service**:
- ✅ Intégration Prism CLI (via Process)
- ✅ Gestion lifecycle (start/stop)
- ✅ Configuration dynamic/static data
- ✅ Logs des mocks
- ✅ Tests intégration

**Linting Engine Service**:
- ✅ Intégration Spectral CLI
- ✅ Validation avec règles par défaut
- ✅ Règles personnalisées (CRUD)
- ✅ Rapports de qualité
- ✅ Tests intégration

**Livrables Phase 1**:
- ✅ 6 services backend opérationnels
- ✅ API Gateway fonctionnel
- ✅ Auth complète
- ✅ Tests: >80% coverage
- ✅ Documentation OpenAPI pour chaque service

---

### Phase 2: Frontend Core (Semaines 5-7)

**Objectif**: Interface utilisateur de base fonctionnelle

#### Semaine 5: Setup Frontend & Auth

- ✅ Architecture React + TypeScript
- ✅ Setup Tailwind + shadcn/ui
- ✅ Routing (React Router)
- ✅ State management (Zustand)
- ✅ API client (TanStack Query)
- ✅ Pages d'auth (Login, Register)
- ✅ Protected routes
- ✅ Layout principal

#### Semaine 6: Workspaces & Projects

- ✅ Page liste workspaces
- ✅ Création/édition workspace
- ✅ Page liste projets
- ✅ Création/édition projet
- ✅ Navigation breadcrumb
- ✅ Sidebar navigation

#### Semaine 7: API Spec Viewer (Read-only)

- ✅ Affichage spec OpenAPI (read mode)
- ✅ Visualisation endpoints
- ✅ Visualisation schémas
- ✅ Syntax highlighting (Monaco Editor)
- ✅ Vue arborescente
- ✅ Recherche dans le spec

**Livrables Phase 2**:
- ✅ Frontend fonctionnel (read-only)
- ✅ Authentification UI
- ✅ Gestion workspaces/projects
- ✅ Visualisation API specs
- ✅ Design responsive

---

### Phase 3: Éditeur API (Semaines 8-10)

**Objectif**: Éditeur complet form + code

#### Semaine 8: Éditeur Code (Monaco)

- ✅ Intégration Monaco Editor
- ✅ Syntax highlighting YAML/JSON
- ✅ Autocompletion OpenAPI
- ✅ Validation en temps réel
- ✅ Sauvegarde auto
- ✅ Gestion des erreurs

#### Semaine 9: Éditeur Form-Based

- ✅ Vue formulaire pour info générale
- ✅ Édition endpoints (paths)
- ✅ Édition parameters
- ✅ Édition request/response bodies
- ✅ Drag & drop pour réorganiser
- ✅ Validation formulaires

#### Semaine 10: Schéma Editor & Components

- ✅ Éditeur graphique JSON Schema
- ✅ Gestion des $ref
- ✅ Bibliothèque de composants réutilisables
- ✅ Bascule Form ↔ Code
- ✅ Preview live

**Livrables Phase 3**:
- ✅ Éditeur complet (form + code)
- ✅ Édition collaborative (sans real-time encore)
- ✅ Validation & linting en temps réel
- ✅ Tests E2E

---

### Phase 4: Data Dictionary UI (Semaines 11-12)

**Objectif**: Interface complète du dictionnaire de données

#### Semaine 11: CRUD Data Dictionary

- ✅ Page liste entités
- ✅ Création/édition entité
- ✅ Gestion attributs
- ✅ Métadonnées enrichies
- ✅ Recherche & filtres
- ✅ Vue hiérarchique

#### Semaine 12: Liaison API ↔ Dictionary

- ✅ Interface de liaison
- ✅ Suggestions automatiques
- ✅ Mapping manuel
- ✅ Visualisation des liens
- ✅ Synchronisation métadonnées

**Livrables Phase 4**:
- ✅ Dictionnaire de données complet
- ✅ Liaison bidirectionnelle fonctionnelle
- ✅ Tests E2E

---

### Phase 5: Gouvernance & Impact Analysis (Semaines 13-14)

**Objectif**: Analyse d'impact et propagation

#### Semaine 13: Impact Analysis Engine

- ✅ Détection des dépendances (backend)
- ✅ Calcul du risque
- ✅ Génération de rapports
- ✅ Tests complexes

#### Semaine 14: Impact Analysis UI

- ✅ Rapport d'impact visuel
- ✅ Graphe de dépendances
- ✅ Workflow de propagation
- ✅ Proposals automatiques
- ✅ Notifications

**Livrables Phase 5**:
- ✅ Analyse d'impact complète
- ✅ Propagation des modifications
- ✅ Notifications
- ✅ Tests E2E

---

### Phase 6: Collaboration (Semaines 15-16)

**Objectif**: Collaboration temps réel

#### Semaine 15: Collaboration Backend

- ✅ SignalR Hubs
- ✅ Gestion présence
- ✅ Cursors & selections
- ✅ Operational Transformation (OT) basique
- ✅ Comments & discussions

#### Semaine 16: Collaboration UI

- ✅ Intégration SignalR client
- ✅ Affichage curseurs
- ✅ Édition simultanée
- ✅ Commentaires UI
- ✅ Proposals & reviews
- ✅ Notifications temps réel

**Livrables Phase 6**:
- ✅ Collaboration temps réel fonctionnelle
- ✅ Commentaires & discussions
- ✅ Proposals workflow
- ✅ Tests E2E

---

### Phase 7: Documentation & Mocking (Semaine 17)

**Objectif**: Génération docs & mock servers

#### Documentation Service (Backend)

- ✅ Génération API Reference
- ✅ Support Markdown
- ✅ Upload images
- ✅ Thèmes
- ✅ Export PDF/HTML

#### Documentation UI

- ✅ Prévisualisation docs
- ✅ Éditeur Markdown
- ✅ Gestion images
- ✅ Publication

#### Mock Server UI

- ✅ Interface start/stop mocks
- ✅ Configuration
- ✅ Logs des requêtes
- ✅ Scénarios personnalisés

**Livrables Phase 7**:
- ✅ Documentation complète
- ✅ Mock servers opérationnels
- ✅ Tests E2E

---

### Phase 8: Git Integration (Semaine 18)

**Objectif**: Intégration Git complète

#### Git Service (Backend)

- ✅ Clone repositories
- ✅ Commit & push
- ✅ Branch management
- ✅ Webhooks
- ✅ Sync bidirectionnel

#### Git UI

- ✅ Configuration Git repo
- ✅ Commit UI
- ✅ History & diff
- ✅ Conflict resolution
- ✅ Branch switcher

**Livrables Phase 8**:
- ✅ Git integration complète
- ✅ Tests E2E

---

### Phase 9: Teams & Permissions (Semaine 19)

**Objectif**: Gestion multi-utilisateurs

- ✅ Teams CRUD (backend + UI)
- ✅ Invitations
- ✅ RBAC (Owner, Admin, Editor, Viewer, Guest)
- ✅ Permissions granulaires
- ✅ Projet privé/public
- ✅ Audit logs
- ✅ Tests E2E

**Livrables Phase 9**:
- ✅ Multi-tenancy complet
- ✅ Permissions robustes
- ✅ Tests sécurité

---

### Phase 10: Polish & Optimisation (Semaines 20-21)

**Objectif**: Optimisation, polish, finalisation

#### Semaine 20: Performance & Optimisation

- ✅ Optimisation requêtes DB (indexes)
- ✅ Caching stratégique (Redis)
- ✅ Lazy loading frontend
- ✅ Code splitting
- ✅ Bundle optimization
- ✅ Load testing (k6)
- ✅ Performance monitoring

#### Semaine 21: UX Polish

- ✅ Design review
- ✅ Animations & transitions
- ✅ Feedback utilisateur (toasts, loaders)
- ✅ Accessibility (WCAG AA)
- ✅ Mobile responsive
- ✅ Dark mode (optionnel)
- ✅ Onboarding & tooltips

**Livrables Phase 10**:
- ✅ Application optimisée
- ✅ UX polie
- ✅ Tests performance

---

### Phase 11: Documentation & Déploiement (Semaine 22)

**Objectif**: Documentation complète et déploiement production

#### Documentation

- ✅ README complet
- ✅ Guide d'installation
- ✅ Guide utilisateur
- ✅ Guide administrateur
- ✅ Documentation API (OpenAPI)
- ✅ Documentation architecture
- ✅ Troubleshooting guide

#### Déploiement

- ✅ Scripts de déploiement Kubernetes
- ✅ Helm charts finalisés
- ✅ Configuration production
- ✅ Backups & disaster recovery
- ✅ Monitoring & alerting
- ✅ Health checks

#### Tests Finaux

- ✅ Tests E2E complets
- ✅ Tests de charge
- ✅ Tests de sécurité
- ✅ Tests d'accessibilité
- ✅ Tests multi-navigateurs

**Livrables Phase 11**:
- ✅ Documentation complète
- ✅ Application déployable en production
- ✅ Tests exhaustifs
- ✅ Monitoring opérationnel

---

### Phase 12: MVP Release (Semaine 23)

**Objectif**: Release MVP 1.0

- ✅ Release notes
- ✅ Changelog
- ✅ Déploiement production
- ✅ Smoke tests production
- ✅ Feedback loop setup
- ✅ Support documentation

**Livrables Phase 12**:
- 🎉 **APIVIA v1.0 MVP RELEASED**

---

## 📊 Métriques de Qualité

### Code Quality

- **Code Coverage**: >80% (backend), >70% (frontend)
- **Cyclomatic Complexity**: <15 par méthode
- **Maintainability Index**: >60
- **Technical Debt**: <5%

### Performance

- **API Response Time**: <200ms (p95)
- **Page Load Time**: <2s (p95)
- **Time to Interactive**: <3s
- **Lighthouse Score**: >90

### Sécurité

- **OWASP Top 10**: Toutes les vulnérabilités adressées
- **Dependency Vulnerabilities**: 0 critical, 0 high
- **Authentication**: JWT with refresh tokens
- **Authorization**: RBAC with granular permissions
- **Data Encryption**: At rest (DB) and in transit (HTTPS)

### Accessibilité

- **WCAG**: AA compliance
- **Keyboard Navigation**: 100% accessible
- **Screen Reader**: Compatible

---

## 🔧 Outils de Développement

### Backend

```bash
# .NET CLI
dotnet new sln -n Apivia
dotnet new webapi -n ApiDesign -o src/Services/ApiDesign
dotnet new xunit -n ApiDesign.Tests -o tests/Services/ApiDesign.Tests

# EF Migrations
dotnet ef migrations add InitialCreate
dotnet ef database update

# Tests
dotnet test --collect:"XPlat Code Coverage"

# Build & Run
dotnet build
dotnet run --project src/Gateway
```

### Frontend

```bash
# Vite + React
npm create vite@latest frontend -- --template react-ts
cd frontend
npm install

# shadcn/ui
npx shadcn-ui@latest init
npx shadcn-ui@latest add button
npx shadcn-ui@latest add card

# Tests
npm run test
npm run test:coverage

# Build & Run
npm run dev
npm run build
npm run preview
```

### Docker

```bash
# Dev local
docker-compose up -d

# Build images
docker build -t apivia/gateway:latest ./src/Gateway
docker build -t apivia/frontend:latest ./src/Frontend

# Kubernetes
kubectl apply -f k8s/namespace.yaml
kubectl apply -f k8s/
kubectl get pods -n apivia
kubectl logs -f <pod-name> -n apivia
```

---

## 🎯 Prochaines Étapes (Post-MVP)

### Phase 13+: Features Avancées

1. **API Testing**
   - Test runner intégré
   - Test scenarios
   - CI/CD integration

2. **API Versioning**
   - Version management
   - Deprecation workflow
   - Breaking change detection

3. **Analytics & Insights**
   - API usage analytics
   - Popular endpoints
   - Performance metrics

4. **Marketplace**
   - Template library
   - Community components
   - Plugins system

5. **AI-Powered Features**
   - Auto-complete avec AI
   - Génération de specs à partir de description
   - Suggestions de design patterns

6. **Advanced Governance**
   - Policy as Code
   - Compliance automation
   - Cost allocation

---

## 📝 Conclusion

Cette architecture représente une plateforme d'API First Design complète et moderne qui :

✅ **Surpasse Stoplight** avec toutes ses fonctionnalités core
✅ **Innove** avec la gouvernance des données et le dictionnaire d'entreprise
✅ **Est professionnelle** avec C# .NET 8, tests, documentation
✅ **Est moderne** avec React, shadcn/ui, SignalR
✅ **Est containerisée** et prête pour Kubernetes
✅ **Est 100% fonctionnelle** sans prérequis (docker-compose up)

Le plan de développement sur 23 semaines (~6 mois) est réaliste et structuré en phases incrémentales avec des livrables clairs.

**Prêt à commencer le développement ! 🚀**
