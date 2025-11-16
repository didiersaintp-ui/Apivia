import { Link } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { dataDictionaryApi } from '../api/services';
import type { DataDictionary } from '../types';
import MainLayout from '../components/layout/MainLayout';
import { formatDate } from '../lib/utils';

export default function DataDictionariesPage() {
  const { data, isLoading } = useQuery({
    queryKey: ['dictionaries'],
    queryFn: () => dataDictionaryApi.getAll({ page: 1, pageSize: 50 }),
  });

  const getStatusBadge = (status: string) => {
    const colors = {
      Draft: 'bg-gray-100 text-gray-800',
      Published: 'bg-green-100 text-green-800',
      Archived: 'bg-yellow-100 text-yellow-800',
    };
    return colors[status as keyof typeof colors] || colors.Draft;
  };

  return (
    <MainLayout>
      <div className="space-y-6">
        <div className="flex items-center justify-between">
          <div>
            <h1 className="text-3xl font-bold text-gray-900">Data Dictionaries</h1>
            <p className="mt-1 text-sm text-gray-600">
              Manage your data models and entity definitions
            </p>
          </div>
        </div>

        {isLoading && (
          <div className="text-center py-12">
            <div className="text-gray-600">Loading data dictionaries...</div>
          </div>
        )}

        {data && data.items.length === 0 && (
          <div className="text-center py-12 bg-white rounded-lg shadow">
            <div className="text-gray-600">
              No data dictionaries yet. Create your first dictionary in a workspace!
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
                    Status
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Entities
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Created
                  </th>
                  <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Actions
                  </th>
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-gray-200">
                {data.items.map((dictionary: DataDictionary) => (
                  <tr key={dictionary.id} className="hover:bg-gray-50">
                    <td className="px-6 py-4">
                      <div>
                        <div className="font-medium text-gray-900">{dictionary.name}</div>
                        {dictionary.description && (
                          <div className="text-sm text-gray-500 line-clamp-1">
                            {dictionary.description}
                          </div>
                        )}
                      </div>
                    </td>
                    <td className="px-6 py-4 text-sm text-gray-500">{dictionary.version}</td>
                    <td className="px-6 py-4">
                      <span
                        className={`inline-flex px-2 py-1 text-xs font-semibold rounded-full ${getStatusBadge(
                          dictionary.status
                        )}`}
                      >
                        {dictionary.status}
                      </span>
                    </td>
                    <td className="px-6 py-4 text-sm text-gray-500">
                      {dictionary.entityCount}
                    </td>
                    <td className="px-6 py-4 text-sm text-gray-500">
                      {formatDate(dictionary.createdAt)}
                    </td>
                    <td className="px-6 py-4 text-right text-sm font-medium">
                      <Link
                        to={`/dictionaries/${dictionary.id}`}
                        className="text-blue-600 hover:text-blue-900"
                      >
                        View
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
