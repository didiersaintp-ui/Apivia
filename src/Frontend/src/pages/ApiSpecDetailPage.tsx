import { useParams, Link } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { apiSpecApi } from '../api/services';
import MainLayout from '../components/layout/MainLayout';
import { formatDateTime } from '../lib/utils';

export default function ApiSpecDetailPage() {
  const { id } = useParams<{ id: string }>();

  const { data: spec, isLoading } = useQuery({
    queryKey: ['apispec', id],
    queryFn: () => apiSpecApi.getById(id!),
    enabled: !!id,
  });

  if (isLoading) {
    return (
      <MainLayout>
        <div className="text-center py-12">
          <div className="text-gray-600">Loading API specification...</div>
        </div>
      </MainLayout>
    );
  }

  if (!spec) {
    return (
      <MainLayout>
        <div className="text-center py-12">
          <div className="text-red-600">API Specification not found</div>
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
            <Link to="/specs" className="hover:text-gray-700">
              API Specifications
            </Link>
            <span>/</span>
            <span className="text-gray-900">{spec.name}</span>
          </div>
          <div className="flex items-center justify-between">
            <div className="flex-1">
              <div className="flex items-center space-x-3">
                <h1 className="text-3xl font-bold text-gray-900">{spec.name}</h1>
                <span
                  className={`inline-flex px-3 py-1 text-sm font-semibold rounded-full ${getStatusBadge(
                    spec.status
                  )}`}
                >
                  {spec.status}
                </span>
                <span
                  className={`inline-flex px-3 py-1 text-sm font-semibold rounded-full ${getFormatBadge(
                    spec.format
                  )}`}
                >
                  {spec.format}
                </span>
              </div>
              {spec.description && (
                <p className="mt-1 text-gray-600">{spec.description}</p>
              )}
              <div className="mt-2 flex items-center space-x-4 text-sm text-gray-500">
                <span>Version {spec.version}</span>
                <span>•</span>
                <span>Updated {formatDateTime(spec.updatedAt || spec.createdAt)}</span>
              </div>
            </div>
            <Link
              to={`/specs/${spec.id}/edit`}
              className="px-4 py-2 text-sm font-medium text-white bg-blue-600 rounded-md hover:bg-blue-700"
            >
              Edit Specification
            </Link>
          </div>
        </div>

        {/* Specification Content */}
        <div className="bg-white shadow-sm rounded-lg overflow-hidden">
          <div className="px-6 py-4 border-b border-gray-200 bg-gray-50">
            <h2 className="text-lg font-semibold text-gray-900">Specification Content</h2>
          </div>
          <div className="p-6">
            <pre className="bg-gray-50 p-4 rounded-lg overflow-x-auto text-sm font-mono">
              <code className="text-gray-800">{spec.content}</code>
            </pre>
          </div>
        </div>

        {/* Metadata */}
        <div className="bg-white shadow-sm rounded-lg overflow-hidden">
          <div className="px-6 py-4 border-b border-gray-200 bg-gray-50">
            <h2 className="text-lg font-semibold text-gray-900">Metadata</h2>
          </div>
          <div className="p-6">
            <dl className="grid grid-cols-1 md:grid-cols-2 gap-6">
              <div>
                <dt className="text-sm font-medium text-gray-500">ID</dt>
                <dd className="mt-1 text-sm text-gray-900 font-mono">{spec.id}</dd>
              </div>
              <div>
                <dt className="text-sm font-medium text-gray-500">Project ID</dt>
                <dd className="mt-1 text-sm text-gray-900 font-mono">{spec.projectId}</dd>
              </div>
              <div>
                <dt className="text-sm font-medium text-gray-500">Name</dt>
                <dd className="mt-1 text-sm text-gray-900">{spec.name}</dd>
              </div>
              <div>
                <dt className="text-sm font-medium text-gray-500">Version</dt>
                <dd className="mt-1 text-sm text-gray-900">{spec.version}</dd>
              </div>
              <div>
                <dt className="text-sm font-medium text-gray-500">Format</dt>
                <dd className="mt-1 text-sm text-gray-900">{spec.format}</dd>
              </div>
              <div>
                <dt className="text-sm font-medium text-gray-500">Status</dt>
                <dd className="mt-1 text-sm text-gray-900">{spec.status}</dd>
              </div>
              <div>
                <dt className="text-sm font-medium text-gray-500">Created At</dt>
                <dd className="mt-1 text-sm text-gray-900">{formatDateTime(spec.createdAt)}</dd>
              </div>
              {spec.updatedAt && (
                <div>
                  <dt className="text-sm font-medium text-gray-500">Updated At</dt>
                  <dd className="mt-1 text-sm text-gray-900">
                    {formatDateTime(spec.updatedAt)}
                  </dd>
                </div>
              )}
            </dl>
          </div>
        </div>
      </div>
    </MainLayout>
  );
}
