import { useEffect } from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { useAuthStore } from './stores/authStore';

// Pages
import LoginPage from './pages/LoginPage';
import RegisterPage from './pages/RegisterPage';
import WorkspacesPage from './pages/WorkspacesPage';
import WorkspaceDetailPage from './pages/WorkspaceDetailPage';
import ProjectsPage from './pages/ProjectsPage';
import ProjectDetailPage from './pages/ProjectDetailPage';
import ApiSpecsPage from './pages/ApiSpecsPage';
import ApiSpecDetailPage from './pages/ApiSpecDetailPage';
import ApiSpecEditorPage from './pages/ApiSpecEditorPage';
import DataDictionariesPage from './pages/DataDictionariesPage';

// Create query client
const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      retry: 1,
      refetchOnWindowFocus: false,
    },
  },
});

// Protected Route component
function ProtectedRoute({ children }: { children: React.ReactNode }) {
  const isAuthenticated = useAuthStore((state) => state.isAuthenticated);

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  return <>{children}</>;
}

function App() {
  const initializeAuth = useAuthStore((state) => state.initializeAuth);

  useEffect(() => {
    initializeAuth();
  }, [initializeAuth]);

  return (
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        <Routes>
          {/* Public routes */}
          <Route path="/login" element={<LoginPage />} />
          <Route path="/register" element={<RegisterPage />} />

          {/* Protected routes - Workspaces */}
          <Route
            path="/workspaces"
            element={
              <ProtectedRoute>
                <WorkspacesPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/workspaces/:id"
            element={
              <ProtectedRoute>
                <WorkspaceDetailPage />
              </ProtectedRoute>
            }
          />

          {/* Protected routes - Projects */}
          <Route
            path="/projects"
            element={
              <ProtectedRoute>
                <ProjectsPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/projects/:id"
            element={
              <ProtectedRoute>
                <ProjectDetailPage />
              </ProtectedRoute>
            }
          />

          {/* Protected routes - API Specs */}
          <Route
            path="/specs"
            element={
              <ProtectedRoute>
                <ApiSpecsPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/specs/:id"
            element={
              <ProtectedRoute>
                <ApiSpecDetailPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/specs/:id/edit"
            element={
              <ProtectedRoute>
                <ApiSpecEditorPage />
              </ProtectedRoute>
            }
          />

          {/* Protected routes - Data Dictionaries */}
          <Route
            path="/dictionaries"
            element={
              <ProtectedRoute>
                <DataDictionariesPage />
              </ProtectedRoute>
            }
          />

          {/* Default redirect */}
          <Route path="/" element={<Navigate to="/workspaces" replace />} />
          <Route path="*" element={<Navigate to="/workspaces" replace />} />
        </Routes>
      </BrowserRouter>
    </QueryClientProvider>
  );
}

export default App;
