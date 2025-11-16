// Auth types
export interface User {
  id: string;
  email: string;
  fullName: string;
  role: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  confirmPassword: string;
  fullName: string;
}

export interface AuthResponse {
  token: string;
  refreshToken: string;
  user: User;
  expiresAt: string;
}

// Workspace types
export interface Workspace {
  id: string;
  name: string;
  description?: string;
  createdAt: string;
  updatedAt?: string;
  memberCount: number;
  projectCount: number;
}

export interface CreateWorkspaceRequest {
  name: string;
  description?: string;
}

// Project types
export interface Project {
  id: string;
  workspaceId: string;
  name: string;
  description?: string;
  status: 'Draft' | 'Active' | 'Archived';
  createdAt: string;
  updatedAt?: string;
  apiSpecCount: number;
}

export interface CreateProjectRequest {
  workspaceId: string;
  name: string;
  description?: string;
}

// API Spec types
export interface ApiSpec {
  id: string;
  projectId: string;
  name: string;
  version: string;
  description?: string;
  format: 'YAML' | 'JSON';
  status: 'Draft' | 'Published' | 'Deprecated';
  content: string;
  createdAt: string;
  updatedAt?: string;
}

export interface CreateApiSpecRequest {
  projectId: string;
  name: string;
  version: string;
  description?: string;
  format: 'YAML' | 'JSON';
  content: string;
}

// Data Dictionary types
export interface DataDictionary {
  id: string;
  workspaceId: string;
  name: string;
  description?: string;
  version: string;
  status: 'Draft' | 'Published' | 'Archived';
  entityCount: number;
  createdAt: string;
}

export interface DataEntity {
  id: string;
  dictionaryId: string;
  name: string;
  description?: string;
  tableName?: string;
  schemaName?: string;
  attributeCount: number;
}

// Pagination
export interface PagedResponse<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

export interface QueryParams {
  page?: number;
  pageSize?: number;
  search?: string;
}
