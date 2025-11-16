import { useState, useEffect } from 'react';
import { useParams, Link } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { apiSpecApi } from '../api/services';
import MonacoEditor from '../components/editor/MonacoEditor';
import MainLayout from '../components/layout/MainLayout';
import type * as monaco from 'monaco-editor';

export default function ApiSpecEditorPage() {
  const { id } = useParams<{ id: string }>();
  const queryClient = useQueryClient();

  const [content, setContent] = useState('');
  const [hasChanges, setHasChanges] = useState(false);
  const [validationErrors, setValidationErrors] = useState<monaco.editor.IMarker[]>([]);
  const [isSaving, setIsSaving] = useState(false);
  const [saveError, setSaveError] = useState<string | null>(null);
  const [viewMode, setViewMode] = useState<'split' | 'editor' | 'preview'>('split');

  const { data: spec, isLoading } = useQuery({
    queryKey: ['apispec', id],
    queryFn: () => apiSpecApi.getById(id!),
    enabled: !!id,
  });

  const updateMutation = useMutation({
    mutationFn: (data: { content: string }) =>
      apiSpecApi.update(id!, { content: data.content }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['apispec', id] });
      setHasChanges(false);
      setIsSaving(false);
      setSaveError(null);
    },
    onError: (error: any) => {
      setIsSaving(false);
      setSaveError(error.response?.data?.detail || 'Failed to save specification');
    },
  });

  const validateMutation = useMutation({
    mutationFn: () => apiSpecApi.validate(id!),
  });

  useEffect(() => {
    if (spec) {
      setContent(spec.content);
    }
  }, [spec]);

  const handleContentChange = (newContent: string) => {
    setContent(newContent);
    setHasChanges(newContent !== spec?.content);
  };

  const handleValidate = (markers: monaco.editor.IMarker[]) => {
    setValidationErrors(markers);
  };

  const handleSave = async () => {
    if (!hasChanges) return;

    setIsSaving(true);
    setSaveError(null);
    updateMutation.mutate({ content });
  };

  const handleValidateSpec = () => {
    validateMutation.mutate();
  };

  const handleDiscard = () => {
    if (spec) {
      setContent(spec.content);
      setHasChanges(false);
    }
  };

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

  const errorCount = validationErrors.filter((m) => m.severity === 8).length; // Error = 8
  const warningCount = validationErrors.filter((m) => m.severity === 4).length; // Warning = 4

  return (
    <MainLayout>
      <div className="space-y-4">
        {/* Header */}
        <div>
          <div className="flex items-center space-x-2 text-sm text-gray-500 mb-2">
            <Link to="/specs" className="hover:text-gray-700">
              API Specifications
            </Link>
            <span>/</span>
            <Link to={`/specs/${spec.id}`} className="hover:text-gray-700">
              {spec.name}
            </Link>
            <span>/</span>
            <span className="text-gray-900">Edit</span>
          </div>
          <div className="flex items-center justify-between">
            <div>
              <h1 className="text-2xl font-bold text-gray-900">{spec.name}</h1>
              <p className="text-sm text-gray-600">
                {spec.format} • Version {spec.version}
              </p>
            </div>
            <div className="flex items-center space-x-3">
              {hasChanges && (
                <span className="text-sm text-yellow-600 font-medium">Unsaved changes</span>
              )}
              <button
                onClick={handleDiscard}
                disabled={!hasChanges}
                className="px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-md hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
              >
                Discard
              </button>
              <button
                onClick={handleSave}
                disabled={!hasChanges || isSaving}
                className="px-4 py-2 text-sm font-medium text-white bg-blue-600 rounded-md hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed"
              >
                {isSaving ? 'Saving...' : 'Save'}
              </button>
            </div>
          </div>
        </div>

        {/* Toolbar */}
        <div className="bg-white shadow-sm rounded-lg p-4">
          <div className="flex items-center justify-between">
            <div className="flex items-center space-x-4">
              {/* View Mode Selector */}
              <div className="flex items-center space-x-2">
                <button
                  onClick={() => setViewMode('split')}
                  className={`px-3 py-1 text-sm font-medium rounded-md ${
                    viewMode === 'split'
                      ? 'bg-blue-100 text-blue-700'
                      : 'text-gray-700 hover:bg-gray-100'
                  }`}
                >
                  Split View
                </button>
                <button
                  onClick={() => setViewMode('editor')}
                  className={`px-3 py-1 text-sm font-medium rounded-md ${
                    viewMode === 'editor'
                      ? 'bg-blue-100 text-blue-700'
                      : 'text-gray-700 hover:bg-gray-100'
                  }`}
                >
                  Editor Only
                </button>
                <button
                  onClick={() => setViewMode('preview')}
                  className={`px-3 py-1 text-sm font-medium rounded-md ${
                    viewMode === 'preview'
                      ? 'bg-blue-100 text-blue-700'
                      : 'text-gray-700 hover:bg-gray-100'
                  }`}
                >
                  Preview Only
                </button>
              </div>

              {/* Validation Status */}
              <div className="flex items-center space-x-2 text-sm">
                {errorCount > 0 && (
                  <span className="text-red-600 font-medium">{errorCount} errors</span>
                )}
                {warningCount > 0 && (
                  <span className="text-yellow-600 font-medium">{warningCount} warnings</span>
                )}
                {errorCount === 0 && warningCount === 0 && (
                  <span className="text-green-600 font-medium">No issues</span>
                )}
              </div>
            </div>

            <div className="flex items-center space-x-2">
              <button
                onClick={handleValidateSpec}
                disabled={validateMutation.isPending}
                className="px-3 py-1 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-md hover:bg-gray-50 disabled:opacity-50"
              >
                {validateMutation.isPending ? 'Validating...' : 'Validate'}
              </button>
            </div>
          </div>

          {saveError && (
            <div className="mt-3 rounded-md bg-red-50 p-3">
              <div className="text-sm text-red-800">{saveError}</div>
            </div>
          )}

          {validateMutation.isSuccess && (
            <div className="mt-3 rounded-md bg-green-50 p-3">
              <div className="text-sm text-green-800">
                Validation successful! {validateMutation.data.errors?.length || 0} errors,{' '}
                {validateMutation.data.warnings?.length || 0} warnings found.
              </div>
            </div>
          )}
        </div>

        {/* Editor/Preview Area */}
        <div className="bg-white shadow-sm rounded-lg overflow-hidden">
          <div
            className={`grid ${
              viewMode === 'split' ? 'grid-cols-2' : 'grid-cols-1'
            } divide-x divide-gray-200`}
          >
            {/* Editor */}
            {(viewMode === 'split' || viewMode === 'editor') && (
              <div className="h-[calc(100vh-320px)]">
                <div className="px-4 py-3 bg-gray-50 border-b border-gray-200">
                  <h3 className="text-sm font-semibold text-gray-900">Editor</h3>
                </div>
                <div className="h-[calc(100%-45px)]">
                  <MonacoEditor
                    value={content}
                    onChange={handleContentChange}
                    language={spec.format.toLowerCase() as 'yaml' | 'json'}
                    onValidate={handleValidate}
                    height="100%"
                  />
                </div>
              </div>
            )}

            {/* Preview */}
            {(viewMode === 'split' || viewMode === 'preview') && (
              <div className="h-[calc(100vh-320px)]">
                <div className="px-4 py-3 bg-gray-50 border-b border-gray-200">
                  <h3 className="text-sm font-semibold text-gray-900">Preview</h3>
                </div>
                <div className="h-[calc(100%-45px)] overflow-auto p-4">
                  <pre className="text-sm font-mono text-gray-800 whitespace-pre-wrap">
                    {content}
                  </pre>
                </div>
              </div>
            )}
          </div>
        </div>

        {/* Validation Results */}
        {validationErrors.length > 0 && (
          <div className="bg-white shadow-sm rounded-lg overflow-hidden">
            <div className="px-6 py-4 border-b border-gray-200 bg-gray-50">
              <h3 className="text-lg font-semibold text-gray-900">Validation Issues</h3>
            </div>
            <div className="divide-y divide-gray-200">
              {validationErrors.map((marker, index) => (
                <div key={index} className="px-6 py-4">
                  <div className="flex items-start space-x-3">
                    <span
                      className={`inline-flex px-2 py-1 text-xs font-semibold rounded ${
                        marker.severity === 8
                          ? 'bg-red-100 text-red-800'
                          : marker.severity === 4
                          ? 'bg-yellow-100 text-yellow-800'
                          : 'bg-blue-100 text-blue-800'
                      }`}
                    >
                      {marker.severity === 8 ? 'Error' : marker.severity === 4 ? 'Warning' : 'Info'}
                    </span>
                    <div className="flex-1">
                      <p className="text-sm text-gray-900">{marker.message}</p>
                      <p className="mt-1 text-xs text-gray-500">
                        Line {marker.startLineNumber}, Column {marker.startColumn}
                      </p>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          </div>
        )}
      </div>
    </MainLayout>
  );
}
