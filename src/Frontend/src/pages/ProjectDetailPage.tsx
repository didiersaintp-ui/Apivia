import { useParams, Link } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { projectApi, apiSpecApi } from '../api/services';
import type { ApiSpec } from '../types';
import MainLayout from '../components/layout/MainLayout';
import { formatDate } from '../lib/utils';

export default function ProjectDetailPage() {
  const { id } = useParams<{ id: string }>();

  const { data: project, isLoading: projectLoading } = useQuery({
    queryKey: ['project', id],
    queryFn: () => projectApi.getById(id!),
    enabled: !!id,
  });

  const { data: specs } = useQuery({
    queryKey: ['apispecs', id],
    queryFn: () => apiSpecApi.getAll({ projectId: id, page: 1, pageSize: 50 }),
    enabled: !!id,
  });

  if (projectLoading) {
    return (
      <MainLayout>
        <div className="text-center py-12">
          <div className="text-gray-600">Loading project...</div>
        </div>
      </MainLayout>
    );
  }

  if (!project) {
    return (
      <MainLayout>
        <div className="text-center py-12">
          <div className="text-red-600">Project not found</div>
        </div>
      </MainLayout>
    );
  }

  const getStatusBadge = (status: string) => {
    const colors = {
      Draft: 'bg-gray-100 text-gray-800',
      Published: 'bg-green-100 text-green-800',
      Deprecated: 'bg-red-100 text-red-800',
    };
    return colors[status as keyof typeof colors] || colors.Draft;
  };

  const getFormatBadge = (format: string) => {
    const colors = {
      YAML: 'bg-purple-100 text-purple-800',
      JSON: 'bg-blue-100 text-blue-800',
    };
    return colors[format as keyof typeof colors] || colors.YAML;
  };

  return (
    <MainLayout>
      <div className="space-y-6">
        {/* Header */}
        <div>
          <div className="flex items-center space-x-2 text-sm text-gray-500 mb-2">
            <Link to="/projects" className="hover:text-gray-700">
              Projects
            </Link>
            <span>/</span>
            <span className="text-gray-900">{project.name}</span>
          </div>
          <div className="flex items-center justify-between">
            <div>
              <div className="flex items-center space-x-3">
                <h1 className="text-3xl font-bold text-gray-900">{project.name}</h1>
                <span
                  className={`inline-flex px-3 py-1 text-sm font-semibold rounded-full ${
                    project.status === 'Active'
                      ? 'bg-green-100 text-green-800'
                      : project.status === 'Archived'
                      ? 'bg-yellow-100 text-yellow-800'
                      : 'bg-gray-100 text-gray-800'
                  }`}
                >
                  {project.status}
                </span>
              </div>
              {project.description && (
                <p className="mt-1 text-gray-600">{project.description}</p>
              )}
              <div className="mt-2 flex items-center space-x-4 text-sm text-gray-500">
                <span>{project.apiSpecCount} API Specifications</span>
                <span>•</span>
                <span>Created {formatDate(project.createdAt)}</span>
              </div>
            </div>
          </div>
        </div>

        {/* API Specifications */}
        <div className="bg-white shadow-sm rounded-lg overflow-hidden">
          <div className="px-6 py-4 border-b border-gray-200">
            <h2 className="text-xl font-semibold text-gray-900">API Specifications</h2>
          </div>

          {specs && specs.items.length === 0 && (
            <div className="text-center py-12 text-gray-500">
              No API specifications yet. Create your first spec!
            </div>
          )}

          {specs && specs.items.length > 0 && (
            <table className="min-w-full divide-y divide-gray-200">
              <thead className="bg-gray-50">
                <tr>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Name
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Version
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Format
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Status
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Updated
                  </th>
                  <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Actions
                  </th>
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-gray-200">
                {specs.items.map((spec: ApiSpec) => (
                  <tr key={spec.id} className="hover:bg-gray-50">
                    <td className="px-6 py-4">
                      <div>
                        <div className="font-medium text-gray-900">{spec.name}</div>
                        {spec.description && (
                          <div className="text-sm text-gray-500 line-clamp-1">
                            {spec.description}
                          </div>
                        )}
                      </div>
                    </td>
                    <td className="px-6 py-4 text-sm text-gray-500">{spec.version}</td>
                    <td className="px-6 py-4">
                      <span
                        className={`inline-flex px-2 py-1 text-xs font-semibold rounded-full ${getFormatBadge(
                          spec.format
                        )}`}
                      >
                        {spec.format}
                      </span>
                    </td>
                    <td className="px-6 py-4">
                      <span
                        className={`inline-flex px-2 py-1 text-xs font-semibold rounded-full ${getStatusBadge(
                          spec.status
                        )}`}
                      >
                        {spec.status}
                      </span>
                    </td>
                    <td className="px-6 py-4 text-sm text-gray-500">
                      {formatDate(spec.updatedAt || spec.createdAt)}
                    </td>
                    <td className="px-6 py-4 text-right text-sm font-medium space-x-3">
                      <Link
                        to={`/specs/${spec.id}`}
                        className="text-blue-600 hover:text-blue-900"
                      >
                        View
                      </Link>
                      <Link
                        to={`/specs/${spec.id}/edit`}
                        className="text-green-600 hover:text-green-900"
                      >
                        Edit
                      </Link>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </div>
      </div>
    </MainLayout>
  );
}
