export type AuthType = 'none' | 'api_key' | 'bearer_token' | 'basic_auth' | 'oauth2' | 'custom_header';
export type ServiceType = 'rest_api' | 'mcp' | 'graphql' | 'soap' | 'webhook';
export type ServiceStatus = 'active' | 'inactive' | 'error';

export interface ServiceAuthConfig {
  type: AuthType;
  // API Key
  apiKeyHeader?: string;
  apiKeyValue?: string;
  apiKeyLocation?: 'header' | 'query';
  // Bearer
  bearerToken?: string;
  // Basic Auth
  basicUser?: string;
  basicPassword?: string;
  // OAuth2
  oauth2ClientId?: string;
  oauth2ClientSecret?: string;
  oauth2TokenUrl?: string;
  oauth2Scopes?: string;
  oauth2GrantType?: 'client_credentials' | 'authorization_code';
  // Custom Header
  customHeaders?: { key: string; value: string }[];
}

export interface ExternalService {
  id: string;
  name: string;
  description: string;
  type: ServiceType;
  baseUrl: string;
  status: ServiceStatus;
  auth: ServiceAuthConfig;
  timeout: number;
  retries: number;
  createdAt: string;
  updatedAt: string;
  lastTestedAt?: string;
  lastTestResult?: 'success' | 'failure';
}

export const authTypeLabels: Record<AuthType, string> = {
  none: 'Sin autenticación',
  api_key: 'API Key',
  bearer_token: 'Bearer Token',
  basic_auth: 'Basic Auth (Usuario/Contraseña)',
  oauth2: 'OAuth 2.0',
  custom_header: 'Headers personalizados',
};

export const serviceTypeLabels: Record<ServiceType, string> = {
  rest_api: 'REST API',
  mcp: 'MCP (Model Context Protocol)',
  graphql: 'GraphQL',
  soap: 'SOAP',
  webhook: 'Webhook',
};

export const mockServices: ExternalService[] = [
  {
    id: 'svc1', name: 'Servicio de Scoring', description: 'API de scoring crediticio para evaluación de riesgo',
    type: 'rest_api', baseUrl: 'https://api.scoring.example.com/v2', status: 'active',
    auth: { type: 'api_key', apiKeyHeader: 'X-API-Key', apiKeyValue: '••••••••••sk_live_abc123', apiKeyLocation: 'header' },
    timeout: 30, retries: 3, createdAt: '2025-01-15', updatedAt: '2025-03-01', lastTestedAt: '2025-03-07', lastTestResult: 'success',
  },
  {
    id: 'svc2', name: 'Validación de Identidad', description: 'Servicio de verificación de documentos e identidad',
    type: 'rest_api', baseUrl: 'https://identity.verify.example.com/api', status: 'active',
    auth: { type: 'bearer_token', bearerToken: '••••••••••eyJhbGciOiJIUzI1NiJ9' },
    timeout: 15, retries: 2, createdAt: '2025-01-20', updatedAt: '2025-02-28', lastTestedAt: '2025-03-06', lastTestResult: 'success',
  },
  {
    id: 'svc3', name: 'Asistente IA', description: 'MCP server para asistencia inteligente en procesos',
    type: 'mcp', baseUrl: 'https://mcp.ai-assistant.example.com', status: 'active',
    auth: { type: 'oauth2', oauth2ClientId: 'client_abc123', oauth2ClientSecret: '••••••••••secret', oauth2TokenUrl: 'https://auth.example.com/oauth/token', oauth2Scopes: 'read write', oauth2GrantType: 'client_credentials' },
    timeout: 60, retries: 1, createdAt: '2025-02-01', updatedAt: '2025-03-05', lastTestedAt: '2025-03-05', lastTestResult: 'success',
  },
  {
    id: 'svc4', name: 'Notificaciones Push', description: 'Servicio de envío de notificaciones push a dispositivos móviles',
    type: 'rest_api', baseUrl: 'https://push.notifications.example.com/v1', status: 'inactive',
    auth: { type: 'basic_auth', basicUser: 'admin_push', basicPassword: '••••••••••' },
    timeout: 10, retries: 3, createdAt: '2025-02-10', updatedAt: '2025-02-20',
  },
  {
    id: 'svc5', name: 'Regulatorio BCRA', description: 'API de reportes regulatorios al Banco Central',
    type: 'soap', baseUrl: 'https://ws.bcra.gov.ar/services/reporting', status: 'error',
    auth: { type: 'custom_header', customHeaders: [{ key: 'X-Entity-Code', value: 'ENT-001' }, { key: 'X-Auth-Token', value: '••••••••••token_xyz' }] },
    timeout: 45, retries: 5, createdAt: '2025-01-05', updatedAt: '2025-03-07', lastTestedAt: '2025-03-07', lastTestResult: 'failure',
  },
  {
    id: 'svc6', name: 'Análisis de Documentos', description: 'MCP para extracción y análisis de documentos con IA',
    type: 'mcp', baseUrl: 'https://mcp.docanalyzer.example.com/stream', status: 'active',
    auth: { type: 'api_key', apiKeyHeader: 'Authorization', apiKeyValue: '••••••••••key_doc_456', apiKeyLocation: 'header' },
    timeout: 120, retries: 1, createdAt: '2025-02-15', updatedAt: '2025-03-04', lastTestedAt: '2025-03-04', lastTestResult: 'success',
  },
];
