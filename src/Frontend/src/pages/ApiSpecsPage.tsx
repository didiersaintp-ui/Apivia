import { Link } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { apiSpecApi } from '../api/services';
import type { ApiSpec } from '../types';
import MainLayout from '../components/layout/MainLayout';
import { formatDate } from '../lib/utils';

export default function ApiSpecsPage() {
  const { data, isLoading } = useQuery({
    queryKey: ['apispecs'],
    queryFn: () => apiSpecApi.getAll({ page: 1, pageSize: 50 }),
  });

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
        <div className="flex items-center justify-between">
          <div>
            <h1 className="text-3xl font-bold text-gray-900">API Specifications</h1>
            <p className="mt-1 text-sm text-gray-600">Browse and manage your OpenAPI specifications</p>
          </div>
        </div>

        {isLoading && (
          <div className="text-center py-12">
            <div className="text-gray-600">Loading API specifications...</div>
          </div>
        )}

        {data && data.items.length === 0 && (
          <div className="text-center py-12 bg-white rounded-lg shadow">
            <div className="text-gray-600">
              No API specifications yet. Create your first spec in a project!
            </div>
          </div>
        )}

        {data && data.items.length > 0 && (
          <div className="bg-white shadow-sm rounded-lg overflow-hidden">
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
                {data.items.map((spec: ApiSpec) => (
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
          </div>
        )}
      </div>
    </MainLayout>
  );
}
