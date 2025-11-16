# Apivia Frontend

Modern React frontend for the Apivia API-First Design Platform.

## Technology Stack

- **React 18** - UI framework
- **TypeScript 5** - Type safety
- **Vite** - Build tool and dev server
- **Tailwind CSS 3** - Utility-first CSS framework
- **React Router DOM 6** - Client-side routing
- **TanStack Query (React Query)** - Data fetching and caching
- **Zustand** - State management
- **Axios** - HTTP client
- **Monaco Editor** - Code editor (Phase 3)
- **Radix UI** - Headless UI components

## Features

### Phase 2 - Core Frontend
✅ **Authentication**
- Login and registration pages
- JWT token management
- Protected routes with auto-redirect
- Password validation and strength requirements

✅ **Workspace Management**
- List workspaces with grid layout
- Create new workspaces
- View workspace details with project listings
- Navigate to workspace-specific resources

✅ **Project Management**
- Table view with filtering and sorting
- Create projects within workspaces
- Status badges (Draft, Active, Archived)
- View project details with API spec listings
- API spec count per project

✅ **API Specifications**
- List all API specs with metadata
- Format badges (YAML/JSON)
- Status badges (Draft, Published, Deprecated)
- Version tracking
- View spec details (read-only)

✅ **Data Dictionaries**
- List all data dictionaries
- View dictionary metadata
- Entity count tracking
- Version and status management

### Phase 3 - API Editor
✅ **Monaco Editor Integration**
- Full-featured code editor
- Syntax highlighting for YAML/JSON
- Auto-completion for OpenAPI
- Real-time validation
- Line numbers and folding
- Dark theme

✅ **API Editor Page**
- Split view (editor + preview)
- Editor-only and preview-only modes
- Real-time validation feedback
- Error and warning display with line numbers
- Save and discard functionality
- Unsaved changes tracking

## Project Structure

```
src/
├── api/
│   ├── client.ts              # Axios instance with interceptors
│   └── services.ts            # API service functions
├── components/
│   ├── editor/
│   │   └── MonacoEditor.tsx   # Monaco Editor wrapper
│   └── layout/
│       └── MainLayout.tsx     # Main application layout
├── hooks/                     # Custom React hooks (future)
├── lib/
│   └── utils.ts              # Utility functions
├── pages/
│   ├── LoginPage.tsx
│   ├── RegisterPage.tsx
│   ├── WorkspacesPage.tsx
│   ├── WorkspaceDetailPage.tsx
│   ├── ProjectsPage.tsx
│   ├── ProjectDetailPage.tsx
│   ├── ApiSpecsPage.tsx
│   ├── ApiSpecDetailPage.tsx
│   ├── ApiSpecEditorPage.tsx
│   └── DataDictionariesPage.tsx
├── stores/
│   └── authStore.ts          # Zustand auth state
├── types/
│   └── index.ts              # TypeScript type definitions
├── App.tsx                   # Main app component with routing
└── main.tsx                  # Application entry point
```

## Getting Started

### Prerequisites

- Node.js 18+ and npm/yarn
- Backend API running (see ../Services/README.md)

### Installation

```bash
# Install dependencies
npm install

# Copy environment variables
cp .env.example .env.local

# Update .env.local with your API URL
# VITE_API_BASE_URL=http://localhost:5000
```

### Development

```bash
# Start development server
npm run dev

# The app will be available at http://localhost:5173
```

### Building for Production

```bash
# Build for production
npm run build

# Preview production build
npm run preview
```

### Linting

```bash
# Run ESLint
npm run lint
```

## Environment Variables

Create a `.env.local` file in the frontend directory:

```bash
# API Base URL
VITE_API_BASE_URL=http://localhost:5000
```

## API Integration

The frontend communicates with backend microservices through the API Gateway:

- **Auth Service** - `/api/auth/*` - Authentication and user management
- **Workspace Service** - `/api/workspaces/*` - Workspace CRUD
- **Project Service** - `/api/projects/*` - Project CRUD
- **API Spec Service** - `/api/apispecs/*` - API specification management
- **Data Dictionary Service** - `/api/datadictionaries/*` - Data model management

All requests include JWT token in Authorization header:
```
Authorization: Bearer <token>
```

## State Management

### Authentication State (Zustand)

```typescript
useAuthStore.getState()
  .user          // Current user object
  .token         // JWT token
  .isAuthenticated // Boolean
  .login()       // Login function
  .logout()      // Logout function
  .register()    // Register function
```

### Server State (TanStack Query)

All data fetching uses React Query with automatic:
- Caching
- Refetching
- Loading states
- Error handling
- Optimistic updates

## Routing

### Public Routes
- `/login` - Login page
- `/register` - Registration page

### Protected Routes
- `/workspaces` - Workspace list
- `/workspaces/:id` - Workspace detail
- `/projects` - Project list
- `/projects/:id` - Project detail
- `/specs` - API spec list
- `/specs/:id` - API spec detail (read-only)
- `/specs/:id/edit` - API spec editor (Phase 3)
- `/dictionaries` - Data dictionary list

## Monaco Editor Features

The API Editor (Phase 3) includes:

1. **Syntax Highlighting**
   - YAML and JSON support
   - OpenAPI-specific syntax

2. **Auto-completion**
   - OpenAPI schema suggestions
   - Context-aware completions

3. **Real-time Validation**
   - Syntax errors
   - Schema validation
   - Warning messages

4. **Editor Features**
   - Line numbers
   - Code folding
   - Find and replace
   - Multi-cursor editing
   - Minimap

5. **View Modes**
   - Split view (editor + preview)
   - Editor only
   - Preview only

## Styling

The app uses Tailwind CSS with custom configuration:

- **Colors**: Blue primary, gray neutrals
- **Fonts**: System font stack for performance
- **Components**: Utility-first approach
- **Responsive**: Mobile-first breakpoints

## Browser Support

- Chrome 90+
- Firefox 88+
- Safari 14+
- Edge 90+

## Performance Optimizations

- Code splitting with React.lazy (future)
- React Query caching
- Automatic layout in Monaco Editor
- Optimized re-renders with React.memo (future)

## Security

- JWT token stored in localStorage
- Automatic token refresh on 401
- CSRF protection via headers
- Input validation with TypeScript
- Protected routes

## Development Tips

### Adding a New Page

1. Create page component in `src/pages/`
2. Add route in `src/App.tsx`
3. Update navigation in `MainLayout.tsx` if needed
4. Add API service functions in `src/api/services.ts`
5. Define TypeScript types in `src/types/index.ts`

### Working with API

```typescript
// Query example
const { data, isLoading, error } = useQuery({
  queryKey: ['workspaces'],
  queryFn: () => workspaceApi.getAll(),
});

// Mutation example
const mutation = useMutation({
  mutationFn: workspaceApi.create,
  onSuccess: () => {
    queryClient.invalidateQueries({ queryKey: ['workspaces'] });
  },
});
```

## Troubleshooting

### CORS Errors
- Ensure backend API Gateway has CORS configured
- Check `VITE_API_BASE_URL` is correct

### Authentication Issues
- Clear localStorage and retry
- Check JWT token expiration
- Verify backend Auth Service is running

### Monaco Editor Not Loading
- Check network console for CDN errors
- Verify monaco-editor package is installed
- Clear browser cache

## Future Enhancements

- [ ] Real-time collaboration
- [ ] Diff viewer for version comparison
- [ ] Advanced search and filtering
- [ ] Export/import functionality
- [ ] Theme customization
- [ ] Offline support with Service Workers
- [ ] i18n (internationalization)

## License

Proprietary - Apivia Platform

## Support

For issues and questions, please contact the development team.
