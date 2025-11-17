# Future Services - Implementation Roadmap

This document outlines the three services that are **not yet implemented** in the Apivia platform. These services are commented out in `docker-compose.yml` and will be implemented in future phases.

## Overview

The following services have stub implementations but are not production-ready:
- **Git Integration Service**
- **Documentation Service**
- **Collaboration Service**

These services are intentionally excluded from the current deployment to ensure a stable, production-ready platform for the core API-first design functionality.

---

## 1. Git Integration Service

### Purpose
Provides Git repository integration for API specifications, enabling version control, branch management, and synchronization with popular Git platforms (GitHub, GitLab, Bitbucket).

### Core Functionality
- **Repository Connection**: Connect Apivia projects to Git repositories
- **Branch Management**: Create, merge, and manage branches for API specifications
- **Commit Operations**: Commit API spec changes with meaningful messages
- **Pull/Push Operations**: Sync API specifications between Apivia and Git repositories
- **Merge Request/PR Integration**: Create and manage merge requests/pull requests
- **Webhook Support**: Listen to Git webhooks for automatic synchronization
- **Conflict Resolution**: Handle merge conflicts in API specifications

### Key Features
1. **Multi-Platform Support**
   - GitHub integration with OAuth authentication
   - GitLab integration with personal access tokens
   - Bitbucket Cloud and Server support
   - Azure DevOps Repos integration

2. **Sync Strategies**
   - One-way sync (Apivia → Git or Git → Apivia)
   - Two-way sync with conflict detection
   - Scheduled sync at configurable intervals
   - Manual sync on-demand

3. **File Format Handling**
   - Support for OpenAPI YAML and JSON files
   - Preserve comments and formatting
   - Handle multi-file API specifications

### Database Schema
```sql
GitRepositories (Id, ProjectId, ProviderType, RepositoryUrl, Branch, AccessToken, SyncStrategy)
GitSyncHistory (Id, RepositoryId, SyncType, Status, CommitHash, SyncedAt)
GitWebhooks (Id, RepositoryId, WebhookUrl, Secret, Events)
```

### Dependencies
- **External Libraries**:
  - `LibGit2Sharp` - Git operations in .NET
  - `Octokit.NET` - GitHub API client
  - `GitLabApiClient` - GitLab API client
- **Infrastructure**: None (uses external Git providers)
- **Services**: API Design Service (for spec synchronization)

### Estimated Complexity
- **Development Effort**: 3-4 weeks (1 developer)
- **Testing Effort**: 1-2 weeks
- **Complexity Level**: **High**
  - Multiple platform integrations
  - OAuth flows and security
  - Conflict resolution logic
  - Webhook handling

### Implementation Priority
**Phase 4** - Important for enterprise adoption, but not critical for core functionality

### Risks & Challenges
- OAuth token management and refresh
- Handling large repository histories
- Rate limiting on Git platforms
- Complex merge conflict scenarios
- Security concerns with storing access tokens

---

## 2. Documentation Service

### Purpose
Automatically generate and publish beautiful, interactive API documentation from OpenAPI specifications, with support for multiple output formats and hosting options.

### Core Functionality
- **Documentation Generation**: Generate docs from OpenAPI specs
- **Multiple Formats**: Support ReDoc, Swagger UI, Stoplight Elements
- **Custom Branding**: Apply custom themes, logos, and colors
- **Versioning**: Maintain multiple documentation versions
- **Publishing**: Deploy docs to various hosting platforms
- **Search & Navigation**: Full-text search and intuitive navigation
- **Code Samples**: Generate code examples in multiple languages

### Key Features
1. **Documentation Renderers**
   - ReDoc (responsive, three-panel layout)
   - Swagger UI (interactive API explorer)
   - Stoplight Elements (modern, customizable)
   - Custom HTML/CSS templates
   - Markdown documentation export

2. **Publishing Targets**
   - Internal hosting on Apivia platform
   - MinIO/S3 for static site hosting
   - Netlify/Vercel deployment
   - GitHub Pages integration
   - Custom domain support

3. **Customization Options**
   - Logo and brand colors
   - Custom CSS and JavaScript
   - Header/footer customization
   - Authentication requirements (public/private docs)
   - SEO metadata configuration

4. **Interactive Features**
   - Try-it-out functionality
   - Authentication configuration UI
   - Response examples with syntax highlighting
   - Download OpenAPI spec button
   - API changelog generation

### Database Schema
```sql
DocumentationSites (Id, ApiSpecId, Renderer, Theme, IsPublished, PublishedUrl)
DocumentationVersions (Id, SiteId, Version, PublishedAt, IsActive)
CustomThemes (Id, WorkspaceId, Name, LogoUrl, CssContent, JavaScriptContent)
```

### Dependencies
- **External Libraries**:
  - `ReDoc` - React-based documentation
  - `Swagger UI` - Official Swagger renderer
  - `Prism.js` - Syntax highlighting
  - `Minio.NET` - MinIO SDK for storage
- **Infrastructure**: MinIO (for static file storage)
- **Services**: API Design Service (for spec retrieval)

### Estimated Complexity
- **Development Effort**: 2-3 weeks (1 developer)
- **Testing Effort**: 1 week
- **Complexity Level**: **Medium**
  - Well-established documentation tools
  - Mostly integration work
  - Template customization complexity
  - Publishing automation

### Implementation Priority
**Phase 5** - Nice to have, improves user experience but not blocking

### Risks & Challenges
- Rendering performance for large API specifications
- Keeping up with ReDoc/Swagger UI updates
- Custom theme validation and security
- CDN and caching strategies for published docs
- SSO integration for private documentation

---

## 3. Collaboration Service

### Purpose
Enable real-time collaboration on API specifications with comments, proposals, approvals, and notifications to facilitate team-based API design workflows.

### Core Functionality
- **Real-time Collaboration**: Multiple users editing simultaneously
- **Comments & Discussions**: Thread-based commenting on API elements
- **Change Proposals**: Suggest changes with approval workflows
- **Notifications**: Real-time alerts for comments, approvals, changes
- **Activity Streams**: View team activity on projects
- **Approval Workflows**: Define and enforce approval processes
- **@Mentions**: Mention team members in comments
- **Review Cycles**: Formal review and approval cycles

### Key Features
1. **Real-time Features**
   - WebSocket-based live updates
   - Cursor tracking (who's viewing what)
   - Live presence indicators
   - Operational transformation for concurrent edits
   - Conflict-free replicated data types (CRDTs)

2. **Comment System**
   - Inline comments on API paths, schemas, parameters
   - Thread-based discussions
   - Rich text editor with markdown support
   - File attachments
   - Emoji reactions
   - Comment resolution tracking

3. **Proposal & Approval Workflow**
   - Create change proposals
   - Request reviews from specific users/teams
   - Approval/rejection with reasons
   - Required approvals before merging
   - Automated status updates
   - Email notifications

4. **Notification System**
   - In-app notifications
   - Email notifications
   - Webhook notifications
   - Configurable notification preferences
   - Digest emails (daily/weekly summaries)

### Database Schema
```sql
-- Already exists in ApiviaDbContext:
-- Comments (Id, Content, UserId, ProjectId, ApiSpecId, ParentCommentId, CreatedAt)
-- Proposals (Id, Title, Description, ProjectId, Changes, Status, CreatedAt)

-- Additional tables needed:
Notifications (Id, UserId, Type, EntityType, EntityId, IsRead, CreatedAt)
NotificationPreferences (Id, UserId, EmailEnabled, InAppEnabled, DigestFrequency)
Approvals (Id, ProposalId, UserId, Status, Comments, ApprovedAt)
ActivityLogs (Id, UserId, ProjectId, ActionType, EntityType, EntityId, CreatedAt)
```

### Dependencies
- **External Libraries**:
  - `SignalR` - Real-time WebSocket communication
  - `RabbitMQ` - Event bus for notifications
  - `Redis` - Presence tracking and caching
  - `SendGrid/MailKit` - Email notifications
- **Infrastructure**: RabbitMQ, Redis (already in docker-compose)
- **Services**: All services (for cross-service notifications)

### Estimated Complexity
- **Development Effort**: 4-5 weeks (1 developer)
- **Testing Effort**: 2 weeks
- **Complexity Level**: **Very High**
  - Real-time WebSocket infrastructure
  - Operational transformation for concurrent edits
  - Complex notification routing
  - Approval workflow state machine
  - Performance at scale

### Implementation Priority
**Phase 4** - Critical for team collaboration, high user value

### Risks & Challenges
- WebSocket connection management and reconnection
- Scaling SignalR across multiple instances
- Operational transformation complexity
- Notification spam management
- Real-time performance under load
- Browser compatibility for real-time features

---

## Implementation Recommendations

### Phase 4 (Next Priority)
1. **Collaboration Service** - Highest business value for teams
2. **Git Integration Service** - Critical for enterprise workflows

### Phase 5 (Future Enhancement)
1. **Documentation Service** - Improves user experience, not blocking

### Prerequisites Before Implementation

#### All Services
- [ ] Complete unit and integration tests for Phase 1-3 services
- [ ] Load testing and performance optimization of existing services
- [ ] Comprehensive API documentation
- [ ] Monitoring and alerting setup in production

#### Collaboration Service Specific
- [ ] SignalR infrastructure setup and testing
- [ ] Notification delivery system design
- [ ] Approval workflow state machine design
- [ ] Real-time performance benchmarking

#### Git Integration Service Specific
- [ ] OAuth application registration on GitHub/GitLab
- [ ] Secure credential storage strategy
- [ ] Git conflict resolution UI/UX design
- [ ] Webhook endpoint security hardening

#### Documentation Service Specific
- [ ] MinIO bucket and CDN configuration
- [ ] Theme validation and sanitization logic
- [ ] Publishing pipeline automation
- [ ] SEO and metadata strategy

---

## Why These Services Are Not Implemented

### Strategic Reasons
1. **Focus on Core Value**: Phase 1-3 delivers the core API-first design platform
2. **Complexity vs. Benefit**: These services add significant complexity for incremental value
3. **External Dependencies**: Reduce external dependencies in initial release
4. **Market Validation**: Validate core platform before investing in advanced features

### Technical Reasons
1. **Stability First**: Ensure core services are rock-solid before adding complexity
2. **Performance Baseline**: Establish performance metrics before adding real-time features
3. **Security Hardening**: Harden authentication/authorization before OAuth integrations
4. **Scalability Testing**: Validate scaling approach before adding WebSocket infrastructure

### Resource Considerations
1. **Development Bandwidth**: Limited development resources for initial MVP
2. **Testing Effort**: Additional services require exponentially more testing
3. **Maintenance Overhead**: Each service adds ongoing maintenance burden
4. **Documentation Needs**: Each service requires comprehensive documentation

---

## How to Enable Future Services

When these services are ready for implementation:

### 1. Implement Service Logic
- Complete the service implementation in the respective service directory
- Add comprehensive unit and integration tests
- Update service documentation

### 2. Update docker-compose.yml
```yaml
# Uncomment the service block
documentation-service:
  build:
    context: ./src/Services/Documentation
    dockerfile: Dockerfile
  # ... rest of configuration
```

### 3. Update API Gateway
- Add routes in `/home/user/Apivia/src/Gateway/appsettings.json`
- Configure authentication/authorization rules
- Add rate limiting for the new endpoints

### 4. Update Frontend
- Add UI components for new service features
- Integrate API calls
- Update navigation and routing

### 5. Database Migrations
- Add any new entities to `ApiviaDbContext`
- Generate and apply new migrations
- Update seed data if needed

---

## Questions or Contributions?

For questions about future service implementation or to contribute:
- Open an issue on GitHub with label `future-service`
- Contact the architecture team
- Review the main README.md for contribution guidelines

---

**Last Updated**: 2025-11-17
**Status**: Planning Phase
**Owner**: Platform Architecture Team
