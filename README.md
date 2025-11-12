# APIVIA - API First Design Platform

> **Une plateforme complète d'API First Design qui surpasse Stoplight avec gouvernance des données d'entreprise intégrée**

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![React](https://img.shields.io/badge/React-18-61DAFB?logo=react)](https://react.dev/)
[![TypeScript](https://img.shields.io/badge/TypeScript-5-3178C6?logo=typescript)](https://www.typescriptlang.org/)
[![Docker](https://img.shields.io/badge/Docker-Ready-2496ED?logo=docker)](https://www.docker.com/)
[![Kubernetes](https://img.shields.io/badge/Kubernetes-Ready-326CE5?logo=kubernetes)](https://kubernetes.io/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

---

## 📋 Table des Matières

- [Vue d'Ensemble](#-vue-densemble)
- [Fonctionnalités](#-fonctionnalités)
- [Architecture](#-architecture)
- [Technologies](#-technologies)
- [Démarrage Rapide](#-démarrage-rapide)
- [Documentation](#-documentation)
- [Roadmap](#-roadmap)
- [Contribution](#-contribution)

---

## 🎯 Vue d'Ensemble

**Apivia** est une plateforme moderne d'API First Design qui combine toutes les fonctionnalités de Stoplight avec une innovation majeure : **la gouvernance des données d'entreprise intégrée**.

### Pourquoi Apivia ?

#### ✅ Parité Complète avec Stoplight

- 📝 Éditeur visuel et form-based pour concevoir des APIs
- 💻 Édition directe YAML/JSON avec autocomplétion
- 📊 Modélisation de schémas JSON Schema via interface graphique
- 🔄 Réutilisation de composants et références croisées
- 📖 Documentation technique intégrée (API Reference + guides Markdown)
- ✅ Linting & validation (Spectral) pour conformité
- 🚀 Mocking d'API intégré (Prism)
- 🔄 Intégration Git complète
- 👥 Collaboration en temps réel
- 🏢 Gestion d'équipes et permissions (RBAC)

#### 🚀 Innovations Majeures

**1. Dictionnaire de Données Entreprise**
- Définitions canoniques de toutes les entités métier
- Métadonnées enrichies (propriétaire, domaine, sensibilité, qualité)
- Taxonomies et ontologies
- Source de vérité unique pour les données

**2. Gouvernance des Données Appliquée aux API**
- Référencement bidirectionnel API ↔ Dictionnaire
- Chaque champ d'API lié à une définition métier
- Synchronisation automatique des métadonnées
- Traçabilité complète

**3. Analyse d'Impact Automatique**
- Détection des dépendances en temps réel
- Rapport d'impact visuel avec graphe
- Calcul du niveau de risque (Low/Medium/High/Critical)
- Propagation intelligente des modifications

**4. Conformité & RGPD**
- Marquage automatique des données PII
- Génération de registres RGPD
- Alertes sur exposition de données sensibles
- Support du droit à l'oubli

### Différenciateurs vs Stoplight

| Fonctionnalité | Stoplight | Apivia |
|----------------|-----------|--------|
| OpenAPI v2/v3 | ✅ | ✅ |
| Éditeur Visuel + Code | ✅ | ✅ |
| Mock Servers | ✅ | ✅ |
| Linting (Spectral) | ✅ | ✅ |
| Collaboration Temps Réel | ✅ | ✅ |
| Git Integration | ✅ | ✅ |
| **Dictionnaire de Données** | ❌ | ✅ |
| **Gouvernance des Données** | ❌ | ✅ |
| **Analyse d'Impact** | ❌ | ✅ |
| **Référencement Bidirectionnel** | ❌ | ✅ |
| **Métadonnées Métier** | ❌ | ✅ |
| **Conformité RGPD** | ❌ | ✅ |

---

## ✨ Fonctionnalités

### Core Features (Parité Stoplight)

#### 🎨 Design d'API
- **Éditeur Dual Mode**: Bascule instantanée entre vue formulaire et vue code
- **Autocompletion Intelligente**: Suggestions contextuelles basées sur OpenAPI schema
- **Validation Temps Réel**: Détection instantanée des erreurs
- **Composants Réutilisables**: Bibliothèque de schémas, paramètres, réponses
- **Import/Export**: Support YAML, JSON, Postman Collection

#### 📖 Documentation
- **API Reference Auto-générée**: Documentation interactive
- **Guides Markdown**: Intégration de guides techniques
- **Gestion d'Images**: Upload et organisation de médias
- **Thèmes Personnalisables**: Adaptation à votre charte graphique
- **Export Multi-format**: PDF, HTML
- **Publication Automatique**: Webhooks pour mise à jour continue

#### 🧪 Testing & Validation
- **Mock Servers**: Génération automatique via Prism
- **Données Réalistes**: Faker.js pour données dynamiques
- **Linting Avancé**: Règles Spectral personnalisables
- **Style Guides**: Application de standards d'entreprise
- **Quick Fixes**: Suggestions de correction automatique

#### 👥 Collaboration
- **Édition Temps Réel**: Curseurs et sélections des autres utilisateurs
- **Commentaires & Discussions**: Annotations contextuelles
- **Proposals**: Workflow de review avec diff visuel
- **Notifications**: Alertes en temps réel
- **Historique Complet**: Audit trail de toutes les modifications

#### 🔄 Git Integration
- **Sync Bidirectionnel**: Synchronisation automatique
- **Branch Management**: Gestion des branches dans l'interface
- **Conflict Resolution**: Interface de résolution de conflits
- **Webhooks**: Auto-update sur push
- **History & Blame**: Visualisation de l'historique Git

#### 🏢 Teams & Permissions
- **Workspaces Multi-tenants**: Isolation complète des données
- **RBAC**: Rôles (Owner, Admin, Editor, Viewer, Guest)
- **Permissions Granulaires**: Par projet, par ressource
- **Projets Privés/Publics**: Contrôle de visibilité
- **Invitations**: Gestion des accès par email

### Advanced Features (Au-delà de Stoplight)

#### 📚 Dictionnaire de Données Entreprise

**Gestion Centralisée**:
```
DataEntity: Customer
├─ DisplayName: "Client"
├─ BusinessOwner: "Marie Dupont (CRM Team)"
├─ FunctionalDomain: "Gestion Clients"
├─ Sensitivity: "Confidential"
├─ QualityLevel: "Gold"
└─ Attributes:
   ├─ CustomerID (UUID, Required, PII)
   ├─ Email (String, Required, PII, GDPR: Personal Identifier)
   ├─ FirstName (String, Required, PII)
   ├─ LastName (String, Required, PII)
   └─ CreatedDate (DateTime, Required)
```

**Interface Utilisateur**:
- 🔍 Recherche avancée multi-critères
- 📊 Vue hiérarchique des entités
- 🎨 Éditeur visuel d'attributs
- 📈 Statistiques d'utilisation
- 📖 Documentation métier intégrée

#### 🔗 Référencement Bidirectionnel

**Liaison API ↔ Dictionnaire**:
```yaml
# OpenAPI Schema
components:
  schemas:
    Customer:
      type: object
      x-linked-to-dictionary: "entities/customer"
      x-business-owner: "Marie Dupont"
      x-functional-domain: "Gestion Clients"
      properties:
        id:
          type: string
          format: uuid
          x-linked-to-dictionary: "entities/customer/attributes/CustomerID"
          x-pii-data: true
          x-gdpr-category: "Personal Identifier"
```

**Fonctionnalités**:
- 🎯 Mapping automatique par convention de nommage
- ✏️ Mapping manuel pour cas spécifiques
- 📋 Copie automatique des métadonnées
- 🔀 Synchronisation bidirectionnelle
- 📍 Traçabilité complète

#### 📊 Analyse d'Impact Automatique

**Détection & Rapport**:
```
Impact Analysis Report
═══════════════════════════════════════════

Entity Modified: Customer.Email
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

**Scénarios Couverts**:
- Type changes (breaking/non-breaking)
- Required fields modifications
- Renaming
- Deletion
- Addition de nouveaux champs

#### 🔄 Propagation des Modifications

**Workflow Intelligent**:
1. Utilisateur modifie une entité dans le dictionnaire
2. Système détecte les APIs liées
3. Génération automatique de proposals
4. Review & approval par les équipes
5. Application avec versioning

**Options**:
- ✅ Auto-sync (changements non-breaking)
- 📝 Proposal (changements breaking)
- 🔔 Notify only
- ❌ Block (incompatibilité)

#### 🔒 Conformité & RGPD

- 🏷️ Marquage automatique des données PII
- 📋 Génération de registres RGPD
- ⚠️ Alertes exposition données sensibles
- 📊 Rapports de conformité
- 🗑️ Support droit à l'oubli

---

## 🏗️ Architecture

### Vue d'Ensemble

Apivia utilise une **architecture microservices** avec **event-driven architecture** pour scalabilité et résilience.

```
┌─────────────────────────────────────────────────┐
│           Frontend (React + shadcn/ui)          │
└───────────────────┬─────────────────────────────┘
                    │ HTTPS / SignalR
┌───────────────────┴─────────────────────────────┐
│              API Gateway (YARP)                 │
└───┬───────┬───────┬───────┬───────┬────────┬───┘
    │       │       │       │       │        │
┌───▼──┐┌──▼──┐┌──▼──┐┌───▼──┐┌──▼──┐┌────▼───┐
│ API  ││Data ││Mock ││Lint  ││Docs ││Collab  │
│Design││Dict ││     ││      ││     ││        │
└───┬──┘└──┬──┘└──┬──┘└───┬──┘└──┬──┘└────┬───┘
    │      │      │       │      │        │
    └──────┴──────┴───────┴──────┴────────┘
                   │
    ┌──────────────▼───────────────┐
    │   Event Bus (RabbitMQ)       │
    └──────────────┬───────────────┘
                   │
    ┌──────────────▼───────────────┐
    │   Database (PostgreSQL)      │
    └──────────────────────────────┘
```

### Services

| Service | Responsabilité | Port |
|---------|----------------|------|
| **Gateway** | Routing, Auth, CORS | 5000 |
| **API Design** | CRUD API specs, Validation | 5001 |
| **Data Dictionary** | Gestion dictionnaire données | 5002 |
| **Governance** | Linking, Impact analysis | 5003 |
| **Mock Server** | Gestion serveurs mock (Prism) | 5004 |
| **Linting** | Validation Spectral | 5005 |
| **Documentation** | Génération docs | 5006 |
| **Collaboration** | Temps réel (SignalR) | 5007 |
| **Auth** | Authentication, Users | 5008 |
| **Git Integration** | Sync Git repos | 5009 |

### Infrastructure

- **Database**: PostgreSQL 16
- **Message Broker**: RabbitMQ
- **Cache**: Redis
- **Storage**: MinIO (S3-compatible)
- **Logging**: Seq
- **Monitoring**: Prometheus + Grafana

---

## 🛠️ Technologies

### Backend

| Composant | Technologie | Version |
|-----------|-------------|---------|
| Framework | ASP.NET Core | 8.0 |
| ORM | Entity Framework Core | 8.0 |
| API Gateway | YARP | Latest |
| Real-Time | SignalR | 8.0 |
| Message Bus | RabbitMQ (MassTransit) | Latest |
| Cache | Redis | 7 |
| Database | PostgreSQL | 16 |
| Testing | xUnit + FluentAssertions | Latest |
| Logging | Serilog + Seq | Latest |

### Frontend

| Composant | Technologie | Version |
|-----------|-------------|---------|
| Framework | React | 18 |
| Language | TypeScript | 5 |
| Build Tool | Vite | Latest |
| UI Library | shadcn/ui + Radix UI | Latest |
| Styling | Tailwind CSS | 3 |
| State | Zustand | Latest |
| Data Fetching | TanStack Query | 5 |
| Forms | React Hook Form + Zod | Latest |
| Editor | Monaco Editor | Latest |
| Testing | Vitest + React Testing Library | Latest |

### DevOps

| Composant | Technologie |
|-----------|-------------|
| Containerization | Docker |
| Orchestration | Kubernetes |
| CI/CD | GitHub Actions |
| Registry | GitHub Container Registry |

---

## 🚀 Démarrage Rapide

### Prérequis

- **Docker Desktop** avec Kubernetes activé
- **Git**
- **.NET 8 SDK** (pour développement local)
- **Node.js 20+** (pour développement local)

### Installation - Production (Docker Compose)

**Clone le repository**:
```bash
git clone https://github.com/votre-org/apivia.git
cd apivia
```

**Configure les variables d'environnement**:
```bash
cp .env.example .env
# Éditer .env avec vos valeurs
```

**Démarrer toute l'infrastructure**:
```bash
docker-compose up -d
```

**Accéder à l'application**:
- Frontend: http://localhost:3000
- API Gateway: http://localhost:5000
- Swagger UI: http://localhost:5000/swagger
- RabbitMQ Management: http://localhost:15672
- Seq Logs: http://localhost:5341

**Compte par défaut**:
- Email: `admin@apivia.com`
- Password: `Admin123!`

### Installation - Kubernetes

**Déployer sur Kubernetes**:
```bash
# Créer namespace
kubectl apply -f k8s/namespace.yaml

# Déployer infrastructure
kubectl apply -f k8s/infrastructure/

# Déployer services
kubectl apply -f k8s/services/

# Déployer frontend
kubectl apply -f k8s/frontend/

# Vérifier
kubectl get pods -n apivia
```

**Accéder à l'application**:
```bash
# Port-forward vers gateway
kubectl port-forward -n apivia svc/apivia-gateway 5000:80

# Accéder à http://localhost:5000
```

### Développement Local

**Backend**:
```bash
# Restaurer les packages
dotnet restore

# Démarrer la base de données
docker-compose up -d postgres rabbitmq redis minio seq

# Démarrer les migrations
dotnet ef database update -p src/Shared/Data -s src/Gateway

# Démarrer l'API Gateway
dotnet run --project src/Gateway

# Dans d'autres terminaux, démarrer les services
dotnet run --project src/Services/ApiDesign
dotnet run --project src/Services/DataDictionary
# etc.
```

**Frontend**:
```bash
cd src/Frontend

# Installer les dépendances
npm install

# Démarrer le dev server
npm run dev

# Accéder à http://localhost:3000
```

**Tests**:
```bash
# Backend
dotnet test

# Frontend
cd src/Frontend
npm test
```

---

## 📚 Documentation

### Documentation Technique

- **[ARCHITECTURE.md](./ARCHITECTURE.md)** - Architecture complète du système
- **[TASK_LIST.md](./TASK_LIST.md)** - Liste détaillée des tâches de développement
- **[CONTRIBUTING.md](./CONTRIBUTING.md)** - Guide de contribution
- **[API Documentation](http://localhost:5000/swagger)** - Documentation OpenAPI interactive

### Guides Utilisateur

- **Guide d'Utilisation** - À venir
- **Guide Administrateur** - À venir
- **FAQ** - À venir

---

## 🗓️ Roadmap

### ✅ Phase 0: Setup & Infrastructure (Semaine 1)
- Structure du projet
- Docker & Kubernetes
- CI/CD

### 🔄 Phase 1: Core Backend Services (Semaines 2-4)
- Auth Service
- API Design Service
- Data Dictionary Service
- Governance Engine
- Mock Server Service
- Linting Engine Service

### 📅 Phase 2: Frontend Core (Semaines 5-7)
- Setup React + shadcn/ui
- Pages Auth
- Workspaces & Projects UI
- API Spec Viewer (read-only)

### 📅 Phase 3: Éditeur API (Semaines 8-10)
- Monaco Editor avec YAML
- Éditeur Form-Based
- Schéma Editor
- Bascule Form ↔ Code

### 📅 Phase 4: Data Dictionary UI (Semaines 11-12)
- CRUD Data Dictionary
- Liaison API ↔ Dictionary

### 📅 Phase 5: Gouvernance & Impact (Semaines 13-14)
- Impact Analysis Engine
- Impact Analysis UI
- Propagation workflow

### 📅 Phase 6: Collaboration (Semaines 15-16)
- SignalR Hubs
- Édition temps réel
- Commentaires & Proposals

### 📅 Phase 7: Documentation & Mocking (Semaine 17)
- Documentation Generator
- Mock Server UI

### 📅 Phase 8: Git Integration (Semaine 18)
- Git Service
- Git UI

### 📅 Phase 9: Teams & Permissions (Semaine 19)
- Teams CRUD
- RBAC
- Permissions

### 📅 Phase 10: Polish & Optimisation (Semaines 20-21)
- Performance
- UX Polish
- Accessibility

### 📅 Phase 11: Documentation & Déploiement (Semaine 22)
- Documentation complète
- Scripts déploiement
- Tests finaux

### 📅 Phase 12: MVP Release (Semaine 23)
- 🎉 **Release v1.0**

### 🚀 Post-MVP
- API Testing intégré
- Analytics & Insights
- Marketplace de templates
- AI-Powered features
- Advanced Governance

---

## 🤝 Contribution

Les contributions sont les bienvenues ! Consultez [CONTRIBUTING.md](./CONTRIBUTING.md) pour les guidelines.

### Workflow

1. Fork le projet
2. Créer une branche feature (`git checkout -b feature/amazing-feature`)
3. Commit vos changements (`git commit -m 'Add amazing feature'`)
4. Push vers la branche (`git push origin feature/amazing-feature`)
5. Ouvrir une Pull Request

### Standards

- **Code Style**: Suivre les conventions .NET et React/TypeScript
- **Tests**: Minimum 80% coverage backend, 70% frontend
- **Commits**: Messages conventionnels (feat, fix, docs, etc.)
- **Documentation**: Commenter le code complexe

---

## 📄 License

Ce projet est sous licence MIT. Voir [LICENSE](./LICENSE) pour plus d'informations.

---

## 👥 Équipe

- **Chef de Projet**: À définir
- **Architecte**: À définir
- **Backend Lead**: À définir
- **Frontend Lead**: À définir
- **DevOps Lead**: À définir

---

## 📞 Support

- **Issues**: [GitHub Issues](https://github.com/votre-org/apivia/issues)
- **Discussions**: [GitHub Discussions](https://github.com/votre-org/apivia/discussions)
- **Email**: support@apivia.com

---

## 🙏 Remerciements

- **Stoplight** pour l'inspiration
- **Microsoft** pour .NET et TypeScript
- **Vercel** pour shadcn/ui
- **Communauté Open Source**

---

<div align="center">

**Construit avec ❤️ par l'équipe Apivia**

[Website](https://apivia.com) • [Documentation](https://docs.apivia.com) • [Blog](https://blog.apivia.com)

</div>
