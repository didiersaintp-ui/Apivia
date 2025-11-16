import { useState } from 'react';
import { useParams, Link } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { workspaceApi, projectApi } from '../api/services';
import type { CreateProjectRequest } from '../types';
import MainLayout from '../components/layout/MainLayout';
import { formatDate } from '../lib/utils';

export default function WorkspaceDetailPage() {
  const { id } = useParams<{ id: string }>();
  const queryClient = useQueryClient();
  const [isCreateDialogOpen, setIsCreateDialogOpen] = useState(false);
  const [formData, setFormData] = useState<CreateProjectRequest>({
    workspaceId: id || '',
    name: '',
    description: '',
  });

  const { data: workspace, isLoading: workspaceLoading } = useQuery({
    queryKey: ['workspace', id],
    queryFn: () => workspaceApi.getById(id!),
    enabled: !!id,
  });

  const { data: projects } = useQuery({
    queryKey: ['projects', id],
    queryFn: () => projectApi.getAll({ workspaceId: id, page: 1, pageSize: 50 }),
    enabled: !!id,
  });

  const createMutation = useMutation({
    mutationFn: projectApi.create,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['projects', id] });
      setIsCreateDialogOpen(false);
      setFormData({ workspaceId: id || '', name: '', description: '' });
    },
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    createMutation.mutate(formData);
  };

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    setFormData({
      ...formData,
      [e.target.name]: e.target.value,
    });
  };

  if (workspaceLoading) {
    return (
      <MainLayout>
        <div className="text-center py-12">
          <div className="text-gray-600">Loading workspace...</div>
        </div>
      </MainLayout>
    );
  }

  if (!workspace) {
    return (
      <MainLayout>
        <div className="text-center py-12">
          <div className="text-red-600">Workspace not found</div>
        </div>
      </MainLayout>
    );
  }

  const getStatusBadge = (status: string) => {
    const colors = {
      Draft: 'bg-gray-100 text-gray-800',
      Active: 'bg-green-100 text-green-800',
      Archived: 'bg-yellow-100 text-yellow-800',
    };
    return colors[status as keyof typeof colors] || colors.Draft;
  };

  return (
    <MainLayout>
      <div className="space-y-6">
        {/* Header */}
        <div>
          <div className="flex items-center space-x-2 text-sm text-gray-500 mb-2">
            <Link to="/workspaces" className="hover:text-gray-700">
              Workspaces
            </Link>
            <span>/</span>
            <span className="text-gray-900">{workspace.name}</span>
          </div>
          <div className="flex items-center justify-between">
            <div>
              <h1 className="text-3xl font-bold text-gray-900">{workspace.name}</h1>
              {workspace.description && (
                <p className="mt-1 text-gray-600">{workspace.description}</p>
              )}
              <div className="mt-2 flex items-center space-x-4 text-sm text-gray-500">
                <span>{workspace.memberCount} members</span>
                <span>•</span>
                <span>Created {formatDate(workspace.createdAt)}</span>
              </div>
            </div>
            <button
              onClick={() => setIsCreateDialogOpen(true)}
              className="px-4 py-2 text-sm font-medium text-white bg-blue-600 rounded-md hover:bg-blue-700"
            >
              + New Project
            </button>
          </div>
        </div>

        {/* Projects */}
        <div className="bg-white shadow-sm rounded-lg p-6">
          <h2 className="text-xl font-semibold text-gray-900 mb-4">Projects</h2>

          {projects && projects.items.length === 0 && (
            <div className="text-center py-8 text-gray-500">
              No projects yet. Create your first project!
            </div>
          )}

          {projects && projects.items.length > 0 && (
            <div className="space-y-4">
              {projects.items.map((project) => (
                <Link
                  key={project.id}
                  to={`/projects/${project.id}`}
                  className="block border border-gray-200 rounded-lg p-4 hover:border-blue-500 hover:shadow-sm transition-all"
                >
                  <div className="flex items-start justify-between">
                    <div className="flex-1">
                      <div className="flex items-center space-x-3">
                        <h3 className="font-semibold text-gray-900">{project.name}</h3>
                        <span
                          className={`inline-flex px-2 py-1 text-xs font-semibold rounded-full ${getStatusBadge(
                            project.status
                          )}`}
                        >
                          {project.status}
                        </span>
                      </div>
                      {project.description && (
                        <p className="mt-1 text-sm text-gray-600">{project.description}</p>
                      )}
                      <div className="mt-2 flex items-center space-x-4 text-sm text-gray-500">
                        <span>{project.apiSpecCount} API Specs</span>
                        <span>•</span>
                        <span>Updated {formatDate(project.updatedAt || project.createdAt)}</span>
                      </div>
                    </div>
                    <div className="text-gray-400">→</div>
                  </div>
                </Link>
              ))}
            </div>
          )}
        </div>

        {/* Create Project Dialog */}
        {isCreateDialogOpen && (
          <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
            <div className="bg-white rounded-lg p-6 max-w-md w-full mx-4">
              <h2 className="text-xl font-semibold text-gray-900 mb-4">
                Create New Project
              </h2>

              <form onSubmit={handleSubmit} className="space-y-4">
                <div>
                  <label htmlFor="name" className="block text-sm font-medium text-gray-700">
                    Name
                  </label>
                  <input
                    id="name"
                    name="name"
                    type="text"
                    required
                    className="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-blue-500 focus:border-blue-500"
                    value={formData.name}
                    onChange={handleChange}
                  />
                </div>

                <div>
                  <label htmlFor="description" className="block text-sm font-medium text-gray-700">
                    Description (optional)
                  </label>
                  <textarea
                    id="description"
                    name="description"
                    rows={3}
                    className="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-blue-500 focus:border-blue-500"
                    value={formData.description}
                    onChange={handleChange}
                  />
                </div>

                <div className="flex justify-end space-x-3">
                  <button
                    type="button"
                    onClick={() => setIsCreateDialogOpen(false)}
                    className="px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-md hover:bg-gray-50"
                  >
                    Cancel
                  </button>
                  <button
                    type="submit"
                    disabled={createMutation.isPending}
                    className="px-4 py-2 text-sm font-medium text-white bg-blue-600 rounded-md hover:bg-blue-700 disabled:opacity-50"
                  >
                    {createMutation.isPending ? 'Creating...' : 'Create'}
                  </button>
                </div>
              </form>
            </div>
          </div>
        )}
      </div>
    </MainLayout>
  );
}
