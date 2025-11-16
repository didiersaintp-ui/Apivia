import { useRef } from 'react';
import Editor, { OnMount } from '@monaco-editor/react';
import * as monaco from 'monaco-editor';

interface MonacoEditorProps {
  value: string;
  onChange: (value: string) => void;
  language: 'yaml' | 'json';
  readOnly?: boolean;
  height?: string;
  onValidate?: (markers: monaco.editor.IMarker[]) => void;
}

export default function MonacoEditor({
  value,
  onChange,
  language,
  readOnly = false,
  height = '600px',
  onValidate,
}: MonacoEditorProps) {
  const editorRef = useRef<monaco.editor.IStandaloneCodeEditor | null>(null);

  const handleEditorDidMount: OnMount = (editor, monaco) => {
    editorRef.current = editor;

    // Configure editor options
    editor.updateOptions({
      minimap: { enabled: true },
      fontSize: 14,
      lineNumbers: 'on',
      scrollBeyondLastLine: false,
      wordWrap: 'on',
      wrappingIndent: 'indent',
      automaticLayout: true,
      tabSize: 2,
      insertSpaces: true,
      folding: true,
      readOnly,
    });

    // Configure YAML/JSON language features
    if (language === 'yaml') {
      monaco.languages.yaml?.yamlDefaults.setDiagnosticsOptions({
        validate: true,
        enableSchemaRequest: true,
        hover: true,
        completion: true,
        format: true,
        schemas: [
          {
            uri: 'https://raw.githubusercontent.com/OAI/OpenAPI-Specification/main/schemas/v3.1/schema.json',
            fileMatch: ['*'],
          },
        ],
      });
    } else if (language === 'json') {
      monaco.languages.json.jsonDefaults.setDiagnosticsOptions({
        validate: true,
        schemas: [
          {
            uri: 'https://raw.githubusercontent.com/OAI/OpenAPI-Specification/main/schemas/v3.1/schema.json',
            fileMatch: ['*'],
          },
        ],
      });
    }

    // Set up validation handler
    if (onValidate) {
      const model = editor.getModel();
      if (model) {
        // Listen to model content changes
        const disposable = monaco.editor.onDidChangeMarkers((uris) => {
          const editorUri = model.uri;
          if (uris.find((uri) => uri.toString() === editorUri.toString())) {
            const markers = monaco.editor.getModelMarkers({ resource: editorUri });
            onValidate(markers);
          }
        });

        // Clean up on unmount
        return () => {
          disposable.dispose();
        };
      }
    }
  };

  const handleEditorChange = (value: string | undefined) => {
    if (value !== undefined) {
      onChange(value);
    }
  };

  return (
    <Editor
      height={height}
      language={language}
      value={value}
      onChange={handleEditorChange}
      onMount={handleEditorDidMount}
      theme="vs-dark"
      options={{
        readOnly,
        automaticLayout: true,
      }}
    />
  );
}
