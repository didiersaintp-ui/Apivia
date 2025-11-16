import { apiClient } from './client';
import type {
  LoginRequest,
  RegisterRequest,
  AuthResponse,
  Workspace,
  CreateWorkspaceRequest,
  Project,
  CreateProjectRequest,
  ApiSpec,
  CreateApiSpecRequest,
  PagedResponse,
  QueryParams,
  DataDictionary,
  DataEntity,
} from '../types';

// Auth API
export const authApi = {
  login: async (data: LoginRequest): Promise<AuthResponse> => {
    const response = await apiClient.post('/api/auth/login', data);
    return response.data;
  },

  register: async (data: RegisterRequest): Promise<AuthResponse> => {
    const response = await apiClient.post('/api/auth/register', data);
    return response.data;
  },

  refreshToken: async (refreshToken: string): Promise<AuthResponse> => {
    const response = await apiClient.post('/api/auth/refresh', { refreshToken });
    return response.data;
  },
};

// Workspace API
export const workspaceApi = {
  getAll: async (params?: QueryParams): Promise<PagedResponse<Workspace>> => {
    const response = await apiClient.get('/api/workspaces', { params });
    return response.data;
  },

  getById: async (id: string): Promise<Workspace> => {
    const response = await apiClient.get(`/api/workspaces/${id}`);
    return response.data;
  },

  create: async (data: CreateWorkspaceRequest): Promise<Workspace> => {
    const response = await apiClient.post('/api/workspaces', data);
    return response.data;
  },

  update: async (id: string, data: Partial<CreateWorkspaceRequest>): Promise<Workspace> => {
    const response = await apiClient.put(`/api/workspaces/${id}`, data);
    return response.data;
  },

  delete: async (id: string): Promise<void> => {
    await apiClient.delete(`/api/workspaces/${id}`);
  },
};

// Project API
export const projectApi = {
  getAll: async (params?: QueryParams & { workspaceId?: string }): Promise<PagedResponse<Project>> => {
    const response = await apiClient.get('/api/projects', { params });
    return response.data;
  },

  getById: async (id: string): Promise<Project> => {
    const response = await apiClient.get(`/api/projects/${id}`);
    return response.data;
  },

  create: async (data: CreateProjectRequest): Promise<Project> => {
    const response = await apiClient.post('/api/projects', data);
    return response.data;
  },

  update: async (id: string, data: Partial<CreateProjectRequest>): Promise<Project> => {
    const response = await apiClient.put(`/api/projects/${id}`, data);
    return response.data;
  },

  delete: async (id: string): Promise<void> => {
    await apiClient.delete(`/api/projects/${id}`);
  },
};

// API Spec API
export const apiSpecApi = {
  getAll: async (params?: QueryParams & { projectId?: string }): Promise<PagedResponse<ApiSpec>> => {
    const response = await apiClient.get('/api/apispecs', { params });
    return response.data;
  },

  getById: async (id: string): Promise<ApiSpec> => {
    const response = await apiClient.get(`/api/apispecs/${id}`);
    return response.data;
  },

  create: async (data: CreateApiSpecRequest): Promise<ApiSpec> => {
    const response = await apiClient.post('/api/apispecs', data);
    return response.data;
  },

  update: async (id: string, data: Partial<CreateApiSpecRequest>): Promise<ApiSpec> => {
    const response = await apiClient.put(`/api/apispecs/${id}`, data);
    return response.data;
  },

  delete: async (id: string): Promise<void> => {
    await apiClient.delete(`/api/apispecs/${id}`);
  },

  validate: async (id: string): Promise<any> => {
    const response = await apiClient.post(`/api/apispecs/${id}/validate`);
    return response.data;
  },
};

// Data Dictionary API
export const dataDictionaryApi = {
  getAll: async (params?: QueryParams & { workspaceId?: string }): Promise<PagedResponse<DataDictionary>> => {
    const response = await apiClient.get('/api/datadictionaries', { params });
    return response.data;
  },

  getById: async (id: string): Promise<DataDictionary> => {
    const response = await apiClient.get(`/api/datadictionaries/${id}`);
    return response.data;
  },

  getEntities: async (dictionaryId: string): Promise<PagedResponse<DataEntity>> => {
    const response = await apiClient.get(`/api/datadictionaries/${dictionaryId}/entities`);
    return response.data;
  },
};
