export interface ListItem {
  code: string;
  description: string;
}

export interface Parameter {
  id: string;
  group: string;
  key: string;
  value: string;
  description: string;
  type: 'text' | 'number' | 'boolean' | 'select' | 'list' | 'html';
  options?: string[];
  listItems?: ListItem[];
  parentKey?: string;
  icon?: string;
  updatedAt: string;
}

export interface ParameterGroup {
  id: string;
  label: string;
  icon: string;
}

export const defaultParameterGroups: ParameterGroup[] = [
  { id: 'general', label: 'Generales', icon: 'Settings' },
  { id: 'security', label: 'Seguridad', icon: 'Shield' },
  { id: 'notifications', label: 'Email', icon: 'Mail' },
  { id: 'whatsapp', label: 'WhatsApp', icon: 'MessageSquare' },
  { id: 'sms', label: 'SMS', icon: 'Smartphone' },
  { id: 'templates', label: 'Plantillas', icon: 'FileText' },
  { id: 'workflow', label: 'Workflow', icon: 'GitBranch' },
  { id: 'integrations', label: 'Integraciones', icon: 'Plug' },
  { id: 'masks', label: 'Máscaras', icon: 'Regex' },
  { id: 'entity_types', label: 'Tipos de Entidades', icon: 'Building2' },
  { id: 'channels', label: 'Canales', icon: 'Globe' },
  { id: 'provinces', label: 'Provincias', icon: 'MapPin' },
  { id: 'cities', label: 'Ciudades', icon: 'MapPinned' },
  { id: 'card_networks', label: 'Red de tarjetas', icon: 'CreditCard' },
  { id: 'card_levels', label: 'Niveles de tarjetas', icon: 'CreditCard' },
  { id: 'insurance_coverages', label: 'Coberturas de seguros', icon: 'ShieldCheck' },
  { id: 'countries', label: 'Países', icon: 'Globe2' },
  { id: 'currencies', label: 'Monedas', icon: 'Banknote' },
  { id: 'phone_codes', label: 'Caract. telefónica', icon: 'Phone' },
];

/** @deprecated Use defaultParameterGroups instead */
export const parameterGroups = defaultParameterGroups;

export const mockParameters: Parameter[] = [
  // General
  { id: 'param1', group: 'general', key: 'app.name', value: 'BackOffice Multi-Entidad', description: 'Nombre de la aplicación', type: 'text', updatedAt: '2025-03-01' },
  { id: 'param2', group: 'general', key: 'app.default_language', value: 'es', description: 'Idioma por defecto del sistema', type: 'select', options: ['es', 'en', 'pt'], updatedAt: '2025-03-01' },
  { id: 'param3', group: 'general', key: 'app.items_per_page', value: '10', description: 'Cantidad de registros por página en grillas', type: 'number', updatedAt: '2025-02-15' },
  { id: 'param4', group: 'general', key: 'app.timezone', value: 'America/Argentina/Buenos_Aires', description: 'Zona horaria del sistema', type: 'text', updatedAt: '2025-01-10' },
  { id: 'param5', group: 'general', key: 'app.currency', value: 'ARS', description: 'Moneda por defecto', type: 'select', options: ['ARS', 'USD', 'EUR', 'BRL'], updatedAt: '2025-01-10' },

  // Seguridad
  { id: 'param6', group: 'security', key: 'security.max_login_attempts', value: '5', description: 'Máximo de intentos de login antes de bloquear', type: 'number', updatedAt: '2025-03-01' },
  { id: 'param7', group: 'security', key: 'security.session_timeout_min', value: '30', description: 'Tiempo de expiración de sesión en minutos', type: 'number', updatedAt: '2025-03-01' },
  { id: 'param8', group: 'security', key: 'security.password_min_length', value: '8', description: 'Longitud mínima de contraseña', type: 'number', updatedAt: '2025-02-20' },
  { id: 'param9', group: 'security', key: 'security.require_2fa', value: 'false', description: 'Requerir autenticación de dos factores', type: 'boolean', updatedAt: '2025-02-20' },
  { id: 'param10', group: 'security', key: 'security.password_expiry_days', value: '90', description: 'Días para expiración de contraseña', type: 'number', updatedAt: '2025-01-15' },

  // Notificaciones
  { id: 'param11', group: 'notifications', key: 'notifications.email_enabled', value: 'true', description: 'Habilitar notificaciones por email', type: 'boolean', updatedAt: '2025-03-05' },
  { id: 'param12', group: 'notifications', key: 'notifications.smtp_host', value: 'smtp.example.com', description: 'Servidor SMTP para envío de emails', type: 'text', updatedAt: '2025-03-05' },
  { id: 'param13', group: 'notifications', key: 'notifications.smtp_port', value: '587', description: 'Puerto del servidor SMTP', type: 'number', updatedAt: '2025-03-05' },
  { id: 'param14', group: 'notifications', key: 'notifications.from_email', value: 'noreply@backoffice.com', description: 'Dirección de email remitente', type: 'text', updatedAt: '2025-03-05' },
  { id: 'param_smtp01', group: 'notifications', key: 'notifications.smtp_username', value: 'user@example.com', description: 'Usuario de autenticación SMTP', type: 'text', updatedAt: '2025-03-10' },
  { id: 'param_smtp02', group: 'notifications', key: 'notifications.smtp_password', value: '********', description: 'Contraseña de autenticación SMTP', type: 'text', updatedAt: '2025-03-10' },
  { id: 'param_smtp03', group: 'notifications', key: 'notifications.smtp_auth_method', value: 'LOGIN', description: 'Método de autenticación SMTP', type: 'select', options: ['PLAIN', 'LOGIN', 'CRAM-MD5', 'XOAUTH2'], updatedAt: '2025-03-10' },
  { id: 'param_smtp04', group: 'notifications', key: 'notifications.smtp_ssl_enabled', value: 'true', description: 'Habilitar conexión SSL/TLS', type: 'boolean', updatedAt: '2025-03-10' },
  { id: 'param_smtp05', group: 'notifications', key: 'notifications.smtp_starttls', value: 'true', description: 'Usar STARTTLS para actualizar conexión a TLS', type: 'boolean', updatedAt: '2025-03-10' },
  { id: 'param_smtp06', group: 'notifications', key: 'notifications.smtp_ssl_verify', value: 'true', description: 'Verificar certificado SSL del servidor', type: 'boolean', updatedAt: '2025-03-10' },
  { id: 'param_smtp07', group: 'notifications', key: 'notifications.from_name', value: 'BackOffice Sistema', description: 'Nombre del remitente en emails', type: 'text', updatedAt: '2025-03-10' },
  { id: 'param_smtp08', group: 'notifications', key: 'notifications.reply_to', value: 'soporte@backoffice.com', description: 'Dirección de respuesta (Reply-To)', type: 'text', updatedAt: '2025-03-10' },

  // WhatsApp Business
  { id: 'param_wa01', group: 'whatsapp', key: 'whatsapp.enabled', value: 'false', description: 'Habilitar integración con WhatsApp Business', type: 'boolean', updatedAt: '2025-03-15' },
  { id: 'param_wa02', group: 'whatsapp', key: 'whatsapp.api_version', value: 'v21.0', description: 'Versión de la API de Meta/WhatsApp', type: 'text', updatedAt: '2025-03-15' },
  { id: 'param_wa03', group: 'whatsapp', key: 'whatsapp.phone_number_id', value: '', description: 'ID del número de teléfono registrado en WhatsApp Business', type: 'text', updatedAt: '2025-03-15' },
  { id: 'param_wa04', group: 'whatsapp', key: 'whatsapp.business_account_id', value: '', description: 'ID de la cuenta de WhatsApp Business (WABA ID)', type: 'text', updatedAt: '2025-03-15' },
  { id: 'param_wa05', group: 'whatsapp', key: 'whatsapp.access_token', value: '********', description: 'Token de acceso permanente de la API de Meta', type: 'text', updatedAt: '2025-03-15' },
  { id: 'param_wa06', group: 'whatsapp', key: 'whatsapp.app_id', value: '', description: 'ID de la aplicación en Meta for Developers', type: 'text', updatedAt: '2025-03-15' },
  { id: 'param_wa07', group: 'whatsapp', key: 'whatsapp.app_secret', value: '********', description: 'Secreto de la aplicación en Meta for Developers', type: 'text', updatedAt: '2025-03-15' },
  { id: 'param_wa08', group: 'whatsapp', key: 'whatsapp.webhook_verify_token', value: '********', description: 'Token de verificación para el webhook de WhatsApp', type: 'text', updatedAt: '2025-03-15' },
  { id: 'param_wa09', group: 'whatsapp', key: 'whatsapp.webhook_url', value: '', description: 'URL del webhook para recibir mensajes entrantes', type: 'text', updatedAt: '2025-03-15' },
  { id: 'param_wa10', group: 'whatsapp', key: 'whatsapp.default_language', value: 'es_AR', description: 'Idioma por defecto para plantillas de mensajes', type: 'text', updatedAt: '2025-03-15' },
  { id: 'param_wa11', group: 'whatsapp', key: 'whatsapp.business_display_name', value: '', description: 'Nombre visible del negocio en WhatsApp', type: 'text', updatedAt: '2025-03-15' },
  { id: 'param_wa12', group: 'whatsapp', key: 'whatsapp.message_ttl_hours', value: '24', description: 'Tiempo máximo (horas) para responder mensajes de sesión', type: 'number', updatedAt: '2025-03-15' },

  // SMS
  { id: 'param_sms01', group: 'sms', key: 'sms.enabled', value: 'false', description: 'Habilitar envío de SMS', type: 'boolean', updatedAt: '2025-03-15' },
  { id: 'param_sms02', group: 'sms', key: 'sms.provider', value: 'twilio', description: 'Proveedor de SMS', type: 'select', options: ['twilio', 'sms_masivos', 'vonage', 'infobip'], updatedAt: '2025-03-15' },
  { id: 'param_sms03', group: 'sms', key: 'sms.account_sid', value: '', description: 'Account SID (Twilio) o ID de cuenta del proveedor', type: 'text', updatedAt: '2025-03-15' },
  { id: 'param_sms04', group: 'sms', key: 'sms.auth_token', value: '********', description: 'Auth Token o API Key del proveedor', type: 'text', updatedAt: '2025-03-15' },
  { id: 'param_sms05', group: 'sms', key: 'sms.from_number', value: '', description: 'Número remitente (formato E.164, ej: +5491112345678)', type: 'text', updatedAt: '2025-03-15' },
  { id: 'param_sms06', group: 'sms', key: 'sms.sender_id', value: '', description: 'Sender ID alfanumérico (alternativa al número, ej: MiEmpresa)', type: 'text', updatedAt: '2025-03-15' },
  { id: 'param_sms07', group: 'sms', key: 'sms.api_url', value: 'https://api.twilio.com', description: 'URL base de la API del proveedor', type: 'text', updatedAt: '2025-03-15' },
  { id: 'param_sms08', group: 'sms', key: 'sms.webhook_url', value: '', description: 'URL del webhook para reportes de entrega (delivery reports)', type: 'text', updatedAt: '2025-03-15' },
  { id: 'param_sms09', group: 'sms', key: 'sms.max_length', value: '160', description: 'Longitud máxima por segmento SMS', type: 'number', updatedAt: '2025-03-15' },
  { id: 'param_sms10', group: 'sms', key: 'sms.default_country_code', value: '+54', description: 'Código de país por defecto para números sin prefijo', type: 'text', updatedAt: '2025-03-15' },
  { id: 'param_sms11', group: 'sms', key: 'sms.retry_attempts', value: '3', description: 'Reintentos en caso de fallo de envío', type: 'number', updatedAt: '2025-03-15' },
  { id: 'param_sms12', group: 'sms', key: 'sms.rate_limit_per_second', value: '10', description: 'Límite de envíos por segundo', type: 'number', updatedAt: '2025-03-15' },

  // Plantillas de notificación
  { id: 'param_tpl01', group: 'templates', key: 'templates.bienvenida.titulo', value: 'Bienvenido a {{organizacion.nombre}}', description: 'Título de la plantilla de bienvenida', type: 'text', updatedAt: '2025-03-15' },
  { id: 'param_tpl01f', group: 'templates', key: 'templates.bienvenida.formato', value: 'html', description: 'Formato de la plantilla de bienvenida', type: 'select', options: ['texto', 'html'], updatedAt: '2025-03-15' },
  { id: 'param_tpl02', group: 'templates', key: 'templates.bienvenida.contenido', value: '<h2>¡Hola {{solicitante.nombre}} {{solicitante.apellido}}!</h2><p>Bienvenido a <strong>{{organizacion.nombre}}</strong>. Su solicitud <em>{{solicitud.codigo}}</em> ha sido recibida correctamente.</p><p>Nos pondremos en contacto a la brevedad.</p><p>Saludos,<br/>{{entidad.nombre}}</p>', description: 'Contenido HTML de la plantilla de bienvenida', type: 'html', updatedAt: '2025-03-15' },
  { id: 'param_tpl03', group: 'templates', key: 'templates.aprobacion.titulo', value: 'Solicitud {{solicitud.codigo}} aprobada', description: 'Título de la plantilla de aprobación', type: 'text', updatedAt: '2025-03-15' },
  { id: 'param_tpl03f', group: 'templates', key: 'templates.aprobacion.formato', value: 'html', description: 'Formato de la plantilla de aprobación', type: 'select', options: ['texto', 'html'], updatedAt: '2025-03-15' },
  { id: 'param_tpl04', group: 'templates', key: 'templates.aprobacion.contenido', value: '<h2>Estimado/a {{solicitante.nombre}}</h2><p>Nos complace informarle que su solicitud <strong>{{solicitud.codigo}}</strong> para el producto <em>{{producto.nombre}}</em> ha sido <span style="color:green;font-weight:bold">APROBADA</span>.</p><p>Monto: {{plan.precio}} {{plan.moneda}}</p><p>Entidad: {{entidad.nombre}}<br/>Sucursal: {{sucursal.nombre}}</p>', description: 'Contenido HTML de la plantilla de aprobación', type: 'html', updatedAt: '2025-03-15' },
  { id: 'param_tpl05', group: 'templates', key: 'templates.rechazo.titulo', value: 'Solicitud {{solicitud.codigo}} rechazada', description: 'Título de la plantilla de rechazo', type: 'text', updatedAt: '2025-03-15' },
  { id: 'param_tpl05f', group: 'templates', key: 'templates.rechazo.formato', value: 'html', description: 'Formato de la plantilla de rechazo', type: 'select', options: ['texto', 'html'], updatedAt: '2025-03-15' },
  { id: 'param_tpl06', group: 'templates', key: 'templates.rechazo.contenido', value: '<h2>Estimado/a {{solicitante.nombre}} {{solicitante.apellido}}</h2><p>Lamentamos informarle que su solicitud <strong>{{solicitud.codigo}}</strong> no ha podido ser aprobada en esta oportunidad.</p><p>Para más información, contacte a {{entidad.nombre}}.</p>', description: 'Contenido HTML de la plantilla de rechazo', type: 'html', updatedAt: '2025-03-15' },
  { id: 'param_tpl07', group: 'templates', key: 'templates.reset_password.titulo', value: 'Restablecer contraseña', description: 'Título de la plantilla de reset de contraseña', type: 'text', updatedAt: '2025-03-15' },
  { id: 'param_tpl07f', group: 'templates', key: 'templates.reset_password.formato', value: 'html', description: 'Formato de la plantilla de reset de contraseña', type: 'select', options: ['texto', 'html'], updatedAt: '2025-03-15' },
  { id: 'param_tpl08', group: 'templates', key: 'templates.reset_password.contenido', value: '<h2>Hola {{usuario.nombre}}</h2><p>Recibimos una solicitud para restablecer su contraseña.</p><p>Si usted no realizó esta solicitud, ignore este mensaje.</p><p>Usuario: {{usuario.email}}<br/>Fecha: {{sistema.fecha}}</p>', description: 'Contenido HTML de la plantilla de reset de contraseña', type: 'html', updatedAt: '2025-03-15' },

  // Workflow
  { id: 'param15', group: 'workflow', key: 'workflow.auto_assign', value: 'true', description: 'Asignar automáticamente solicitudes a operadores', type: 'boolean', updatedAt: '2025-02-28' },
  { id: 'param16', group: 'workflow', key: 'workflow.sla_warning_pct', value: '80', description: 'Porcentaje de SLA para generar alerta', type: 'number', updatedAt: '2025-02-28' },
  { id: 'param17', group: 'workflow', key: 'workflow.max_reassignments', value: '3', description: 'Máximo de reasignaciones permitidas', type: 'number', updatedAt: '2025-02-10' },

  // Integraciones
  { id: 'param18', group: 'integrations', key: 'integrations.api_rate_limit', value: '1000', description: 'Límite de llamadas API por minuto', type: 'number', updatedAt: '2025-03-01' },
  { id: 'param19', group: 'integrations', key: 'integrations.webhook_enabled', value: 'true', description: 'Habilitar webhooks de eventos', type: 'boolean', updatedAt: '2025-03-01' },
  { id: 'param20', group: 'integrations', key: 'integrations.webhook_url', value: 'https://hooks.example.com/events', description: 'URL del webhook principal', type: 'text', updatedAt: '2025-02-25' },
  { id: 'param21', group: 'integrations', key: 'integrations.api_version', value: 'v2', description: 'Versión de API expuesta', type: 'select', options: ['v1', 'v2', 'v3'], updatedAt: '2025-02-25' },

  // Máscaras
  { id: 'param22', group: 'masks', key: 'mask.cuit', value: '^\\d{2}-\\d{8}-\\d{1}$', description: 'Formato CUIT argentino (XX-XXXXXXXX-X)', type: 'text', updatedAt: '2025-03-01' },
  { id: 'param23', group: 'masks', key: 'mask.dni', value: '^\\d{7,8}$', description: 'Documento Nacional de Identidad (7 u 8 dígitos)', type: 'text', updatedAt: '2025-03-01' },
  { id: 'param24', group: 'masks', key: 'mask.phone_ar', value: '^\\+54\\s?\\d{2,4}\\s?\\d{4}-?\\d{4}$', description: 'Teléfono argentino (+54 XX XXXX-XXXX)', type: 'text', updatedAt: '2025-03-01' },
  { id: 'param25', group: 'masks', key: 'mask.email', value: '^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\\.[a-zA-Z]{2,}$', description: 'Dirección de correo electrónico', type: 'text', updatedAt: '2025-03-01' },
  { id: 'param26', group: 'masks', key: 'mask.cbu', value: '^\\d{22}$', description: 'CBU bancario argentino (22 dígitos)', type: 'text', updatedAt: '2025-02-28' },
  { id: 'param27', group: 'masks', key: 'mask.alias_cbu', value: '^[a-zA-Z0-9.\\-]{6,20}$', description: 'Alias CBU (alfanumérico con puntos y guiones, 6-20 caracteres)', type: 'text', updatedAt: '2025-02-28' },
  { id: 'param28', group: 'masks', key: 'mask.postal_code_ar', value: '^[A-Z]\\d{4}[A-Z]{3}$', description: 'Código postal argentino CPA (X0000XXX)', type: 'text', updatedAt: '2025-02-28' },
  { id: 'param29', group: 'masks', key: 'mask.currency_amount', value: '^\\d{1,12}(\\.\\d{1,2})?$', description: 'Monto monetario (hasta 12 enteros, 2 decimales)', type: 'text', updatedAt: '2025-02-28' },
  { id: 'param30', group: 'masks', key: 'mask.date_ddmmyyyy', value: '^(0[1-9]|[12]\\d|3[01])/(0[1-9]|1[0-2])/\\d{4}$', description: 'Fecha formato DD/MM/YYYY', type: 'text', updatedAt: '2025-02-25' },
  { id: 'param31', group: 'masks', key: 'mask.passport', value: '^[A-Z]{3}\\d{6}$', description: 'Pasaporte argentino (3 letras + 6 dígitos)', type: 'text', updatedAt: '2025-02-25' },
  { id: 'param32', group: 'masks', key: 'mask.ip_address', value: '^((25[0-5]|2[0-4]\\d|[01]?\\d\\d?)\\.){3}(25[0-5]|2[0-4]\\d|[01]?\\d\\d?)$', description: 'Dirección IPv4', type: 'text', updatedAt: '2025-02-20' },
  { id: 'param33', group: 'masks', key: 'mask.license_plate_ar', value: '^[A-Z]{2}\\d{3}[A-Z]{2}$|^[A-Z]{3}\\d{3}$', description: 'Patente argentina (nuevo o viejo formato)', type: 'text', updatedAt: '2025-02-20' },
  { id: 'param34', group: 'masks', key: 'mask.username', value: '^[a-zA-Z0-9._-]{3,30}$', description: 'Nombre de usuario (3-30 caracteres: letras, números, puntos, guiones)', type: 'text', updatedAt: '2025-03-08' },
  { id: 'param35', group: 'masks', key: 'mask.password', value: '^(?=.*[A-Z])(?=.*[a-z])(?=.*\\d)(?=.*[@$!%*?&])[A-Za-z\\d@$!%*?&]{8,50}$', description: 'Contraseña segura (8-50 caracteres, mayúscula, minúscula, número y carácter especial)', type: 'text', updatedAt: '2025-03-08' },

  // Tipos de Entidades
  { id: 'param36', group: 'entity_types', key: 'entity_type.bank', value: 'bank', description: 'Banco', type: 'text', updatedAt: '2025-03-08' },
  { id: 'param37', group: 'entity_types', key: 'entity_type.insurance', value: 'insurance', description: 'Aseguradora', type: 'text', updatedAt: '2025-03-08' },
  { id: 'param38', group: 'entity_types', key: 'entity_type.fintech', value: 'fintech', description: 'Fintech', type: 'text', updatedAt: '2025-03-08' },
  { id: 'param39', group: 'entity_types', key: 'entity_type.cooperative', value: 'cooperative', description: 'Cooperativa', type: 'text', updatedAt: '2025-03-08' },
  { id: 'param40', group: 'entity_types', key: 'entity_type.mutual', value: 'mutual', description: 'Mutual', type: 'text', updatedAt: '2025-03-08' },
  { id: 'param41', group: 'entity_types', key: 'entity_type.sgr', value: 'sgr', description: 'Sociedad de Garantía Recíproca', type: 'text', updatedAt: '2025-03-08' },

  // Canales
  { id: 'param42', group: 'channels', key: 'channel.web', value: 'web', description: 'Canal Web', type: 'text', updatedAt: '2025-03-08' },
  { id: 'param43', group: 'channels', key: 'channel.mobile', value: 'mobile', description: 'Canal Mobile', type: 'text', updatedAt: '2025-03-08' },
  { id: 'param44', group: 'channels', key: 'channel.api', value: 'api', description: 'Canal API', type: 'text', updatedAt: '2025-03-08' },
  { id: 'param45', group: 'channels', key: 'channel.presencial', value: 'presencial', description: 'Canal Presencial', type: 'text', updatedAt: '2025-03-08' },
  { id: 'param46', group: 'channels', key: 'channel.whatsapp', value: 'whatsapp', description: 'Canal WhatsApp', type: 'text', updatedAt: '2025-03-08' },
  { id: 'param47', group: 'channels', key: 'channel.email', value: 'email', description: 'Canal Email', type: 'text', updatedAt: '2025-03-08' },

  // Provincias de Argentina (vinculadas a países via parentKey)
  { id: 'param_prov01', group: 'provinces', key: 'province.buenos_aires', value: 'Buenos Aires', description: 'Provincia de Buenos Aires', type: 'text', parentKey: 'country.argentina', updatedAt: '2025-03-08' },
  { id: 'param_prov02', group: 'provinces', key: 'province.caba', value: 'CABA', description: 'Ciudad Autónoma de Buenos Aires', type: 'text', parentKey: 'country.argentina', updatedAt: '2025-03-08' },
  { id: 'param_prov03', group: 'provinces', key: 'province.catamarca', value: 'Catamarca', description: 'Provincia de Catamarca', type: 'text', parentKey: 'country.argentina', updatedAt: '2025-03-08' },
  { id: 'param_prov04', group: 'provinces', key: 'province.chaco', value: 'Chaco', description: 'Provincia del Chaco', type: 'text', parentKey: 'country.argentina', updatedAt: '2025-03-08' },
  { id: 'param_prov05', group: 'provinces', key: 'province.chubut', value: 'Chubut', description: 'Provincia del Chubut', type: 'text', parentKey: 'country.argentina', updatedAt: '2025-03-08' },
  { id: 'param_prov06', group: 'provinces', key: 'province.cordoba', value: 'Córdoba', description: 'Provincia de Córdoba', type: 'text', parentKey: 'country.argentina', updatedAt: '2025-03-08' },
  { id: 'param_prov07', group: 'provinces', key: 'province.corrientes', value: 'Corrientes', description: 'Provincia de Corrientes', type: 'text', parentKey: 'country.argentina', updatedAt: '2025-03-08' },
  { id: 'param_prov08', group: 'provinces', key: 'province.entre_rios', value: 'Entre Ríos', description: 'Provincia de Entre Ríos', type: 'text', parentKey: 'country.argentina', updatedAt: '2025-03-08' },
  { id: 'param_prov09', group: 'provinces', key: 'province.formosa', value: 'Formosa', description: 'Provincia de Formosa', type: 'text', parentKey: 'country.argentina', updatedAt: '2025-03-08' },
  { id: 'param_prov10', group: 'provinces', key: 'province.jujuy', value: 'Jujuy', description: 'Provincia de Jujuy', type: 'text', parentKey: 'country.argentina', updatedAt: '2025-03-08' },
  { id: 'param_prov11', group: 'provinces', key: 'province.la_pampa', value: 'La Pampa', description: 'Provincia de La Pampa', type: 'text', parentKey: 'country.argentina', updatedAt: '2025-03-08' },
  { id: 'param_prov12', group: 'provinces', key: 'province.la_rioja', value: 'La Rioja', description: 'Provincia de La Rioja', type: 'text', parentKey: 'country.argentina', updatedAt: '2025-03-08' },
  { id: 'param_prov13', group: 'provinces', key: 'province.mendoza', value: 'Mendoza', description: 'Provincia de Mendoza', type: 'text', parentKey: 'country.argentina', updatedAt: '2025-03-08' },
  { id: 'param_prov14', group: 'provinces', key: 'province.misiones', value: 'Misiones', description: 'Provincia de Misiones', type: 'text', parentKey: 'country.argentina', updatedAt: '2025-03-08' },
  { id: 'param_prov15', group: 'provinces', key: 'province.neuquen', value: 'Neuquén', description: 'Provincia del Neuquén', type: 'text', parentKey: 'country.argentina', updatedAt: '2025-03-08' },
  { id: 'param_prov16', group: 'provinces', key: 'province.rio_negro', value: 'Río Negro', description: 'Provincia de Río Negro', type: 'text', parentKey: 'country.argentina', updatedAt: '2025-03-08' },
  { id: 'param_prov17', group: 'provinces', key: 'province.salta', value: 'Salta', description: 'Provincia de Salta', type: 'text', parentKey: 'country.argentina', updatedAt: '2025-03-08' },
  { id: 'param_prov18', group: 'provinces', key: 'province.san_juan', value: 'San Juan', description: 'Provincia de San Juan', type: 'text', parentKey: 'country.argentina', updatedAt: '2025-03-08' },
  { id: 'param_prov19', group: 'provinces', key: 'province.san_luis', value: 'San Luis', description: 'Provincia de San Luis', type: 'text', parentKey: 'country.argentina', updatedAt: '2025-03-08' },
  { id: 'param_prov20', group: 'provinces', key: 'province.santa_cruz', value: 'Santa Cruz', description: 'Provincia de Santa Cruz', type: 'text', parentKey: 'country.argentina', updatedAt: '2025-03-08' },
  { id: 'param_prov21', group: 'provinces', key: 'province.santa_fe', value: 'Santa Fe', description: 'Provincia de Santa Fe', type: 'text', parentKey: 'country.argentina', updatedAt: '2025-03-08' },
  { id: 'param_prov22', group: 'provinces', key: 'province.santiago_del_estero', value: 'Santiago del Estero', description: 'Provincia de Santiago del Estero', type: 'text', parentKey: 'country.argentina', updatedAt: '2025-03-08' },
  { id: 'param_prov23', group: 'provinces', key: 'province.tierra_del_fuego', value: 'Tierra del Fuego', description: 'Provincia de Tierra del Fuego', type: 'text', parentKey: 'country.argentina', updatedAt: '2025-03-08' },
  { id: 'param_prov24', group: 'provinces', key: 'province.tucuman', value: 'Tucumán', description: 'Provincia de Tucumán', type: 'text', parentKey: 'country.argentina', updatedAt: '2025-03-08' },

  // Ciudades de Argentina (vinculadas a provincias via parentKey)
  // Buenos Aires
  { id: 'city_ba01', group: 'cities', key: 'city.la_plata', value: 'La Plata', description: 'Capital de Buenos Aires', type: 'text', parentKey: 'province.buenos_aires', updatedAt: '2025-03-08' },
  { id: 'city_ba02', group: 'cities', key: 'city.mar_del_plata', value: 'Mar del Plata', description: 'Ciudad de Buenos Aires', type: 'text', parentKey: 'province.buenos_aires', updatedAt: '2025-03-08' },
  { id: 'city_ba03', group: 'cities', key: 'city.bahia_blanca', value: 'Bahía Blanca', description: 'Ciudad de Buenos Aires', type: 'text', parentKey: 'province.buenos_aires', updatedAt: '2025-03-08' },
  { id: 'city_ba04', group: 'cities', key: 'city.tandil', value: 'Tandil', description: 'Ciudad de Buenos Aires', type: 'text', parentKey: 'province.buenos_aires', updatedAt: '2025-03-08' },
  { id: 'city_ba05', group: 'cities', key: 'city.lomas_de_zamora', value: 'Lomas de Zamora', description: 'Ciudad de Buenos Aires', type: 'text', parentKey: 'province.buenos_aires', updatedAt: '2025-03-08' },
  { id: 'city_ba06', group: 'cities', key: 'city.quilmes', value: 'Quilmes', description: 'Ciudad de Buenos Aires', type: 'text', parentKey: 'province.buenos_aires', updatedAt: '2025-03-08' },
  { id: 'city_ba07', group: 'cities', key: 'city.san_isidro', value: 'San Isidro', description: 'Ciudad de Buenos Aires', type: 'text', parentKey: 'province.buenos_aires', updatedAt: '2025-03-08' },
  // CABA
  { id: 'city_caba01', group: 'cities', key: 'city.caba', value: 'Ciudad Autónoma de Buenos Aires', description: 'CABA', type: 'text', parentKey: 'province.caba', updatedAt: '2025-03-08' },
  // Catamarca
  { id: 'city_cat01', group: 'cities', key: 'city.sfv_catamarca', value: 'San Fernando del Valle de Catamarca', description: 'Capital de Catamarca', type: 'text', parentKey: 'province.catamarca', updatedAt: '2025-03-08' },
  { id: 'city_cat02', group: 'cities', key: 'city.belen', value: 'Belén', description: 'Ciudad de Catamarca', type: 'text', parentKey: 'province.catamarca', updatedAt: '2025-03-08' },
  // Chaco
  { id: 'city_cha01', group: 'cities', key: 'city.resistencia', value: 'Resistencia', description: 'Capital del Chaco', type: 'text', parentKey: 'province.chaco', updatedAt: '2025-03-08' },
  { id: 'city_cha02', group: 'cities', key: 'city.presidencia_roque_saenz_pena', value: 'Presidencia Roque Sáenz Peña', description: 'Ciudad del Chaco', type: 'text', parentKey: 'province.chaco', updatedAt: '2025-03-08' },
  // Chubut
  { id: 'city_chu01', group: 'cities', key: 'city.rawson', value: 'Rawson', description: 'Capital del Chubut', type: 'text', parentKey: 'province.chubut', updatedAt: '2025-03-08' },
  { id: 'city_chu02', group: 'cities', key: 'city.comodoro_rivadavia', value: 'Comodoro Rivadavia', description: 'Ciudad del Chubut', type: 'text', parentKey: 'province.chubut', updatedAt: '2025-03-08' },
  { id: 'city_chu03', group: 'cities', key: 'city.trelew', value: 'Trelew', description: 'Ciudad del Chubut', type: 'text', parentKey: 'province.chubut', updatedAt: '2025-03-08' },
  // Córdoba
  { id: 'city_cor01', group: 'cities', key: 'city.cordoba', value: 'Córdoba', description: 'Capital de Córdoba', type: 'text', parentKey: 'province.cordoba', updatedAt: '2025-03-08' },
  { id: 'city_cor02', group: 'cities', key: 'city.rio_cuarto', value: 'Río Cuarto', description: 'Ciudad de Córdoba', type: 'text', parentKey: 'province.cordoba', updatedAt: '2025-03-08' },
  { id: 'city_cor03', group: 'cities', key: 'city.villa_maria', value: 'Villa María', description: 'Ciudad de Córdoba', type: 'text', parentKey: 'province.cordoba', updatedAt: '2025-03-08' },
  { id: 'city_cor04', group: 'cities', key: 'city.carlos_paz', value: 'Villa Carlos Paz', description: 'Ciudad de Córdoba', type: 'text', parentKey: 'province.cordoba', updatedAt: '2025-03-08' },
  // Corrientes
  { id: 'city_corr01', group: 'cities', key: 'city.corrientes', value: 'Corrientes', description: 'Capital de Corrientes', type: 'text', parentKey: 'province.corrientes', updatedAt: '2025-03-08' },
  { id: 'city_corr02', group: 'cities', key: 'city.goya', value: 'Goya', description: 'Ciudad de Corrientes', type: 'text', parentKey: 'province.corrientes', updatedAt: '2025-03-08' },
  // Entre Ríos
  { id: 'city_er01', group: 'cities', key: 'city.parana', value: 'Paraná', description: 'Capital de Entre Ríos', type: 'text', parentKey: 'province.entre_rios', updatedAt: '2025-03-08' },
  { id: 'city_er02', group: 'cities', key: 'city.concordia', value: 'Concordia', description: 'Ciudad de Entre Ríos', type: 'text', parentKey: 'province.entre_rios', updatedAt: '2025-03-08' },
  { id: 'city_er03', group: 'cities', key: 'city.gualeguaychu', value: 'Gualeguaychú', description: 'Ciudad de Entre Ríos', type: 'text', parentKey: 'province.entre_rios', updatedAt: '2025-03-08' },
  // Formosa
  { id: 'city_for01', group: 'cities', key: 'city.formosa', value: 'Formosa', description: 'Capital de Formosa', type: 'text', parentKey: 'province.formosa', updatedAt: '2025-03-08' },
  // Jujuy
  { id: 'city_juj01', group: 'cities', key: 'city.san_salvador_jujuy', value: 'San Salvador de Jujuy', description: 'Capital de Jujuy', type: 'text', parentKey: 'province.jujuy', updatedAt: '2025-03-08' },
  { id: 'city_juj02', group: 'cities', key: 'city.san_pedro_jujuy', value: 'San Pedro de Jujuy', description: 'Ciudad de Jujuy', type: 'text', parentKey: 'province.jujuy', updatedAt: '2025-03-08' },
  // La Pampa
  { id: 'city_lp01', group: 'cities', key: 'city.santa_rosa', value: 'Santa Rosa', description: 'Capital de La Pampa', type: 'text', parentKey: 'province.la_pampa', updatedAt: '2025-03-08' },
  { id: 'city_lp02', group: 'cities', key: 'city.general_pico', value: 'General Pico', description: 'Ciudad de La Pampa', type: 'text', parentKey: 'province.la_pampa', updatedAt: '2025-03-08' },
  // La Rioja
  { id: 'city_lr01', group: 'cities', key: 'city.la_rioja', value: 'La Rioja', description: 'Capital de La Rioja', type: 'text', parentKey: 'province.la_rioja', updatedAt: '2025-03-08' },
  { id: 'city_lr02', group: 'cities', key: 'city.chilecito', value: 'Chilecito', description: 'Ciudad de La Rioja', type: 'text', parentKey: 'province.la_rioja', updatedAt: '2025-03-08' },
  // Mendoza
  { id: 'city_mza01', group: 'cities', key: 'city.mendoza', value: 'Mendoza', description: 'Capital de Mendoza', type: 'text', parentKey: 'province.mendoza', updatedAt: '2025-03-08' },
  { id: 'city_mza02', group: 'cities', key: 'city.san_rafael', value: 'San Rafael', description: 'Ciudad de Mendoza', type: 'text', parentKey: 'province.mendoza', updatedAt: '2025-03-08' },
  { id: 'city_mza03', group: 'cities', key: 'city.godoy_cruz', value: 'Godoy Cruz', description: 'Ciudad de Mendoza', type: 'text', parentKey: 'province.mendoza', updatedAt: '2025-03-08' },
  // Misiones
  { id: 'city_mis01', group: 'cities', key: 'city.posadas', value: 'Posadas', description: 'Capital de Misiones', type: 'text', parentKey: 'province.misiones', updatedAt: '2025-03-08' },
  { id: 'city_mis02', group: 'cities', key: 'city.obera', value: 'Oberá', description: 'Ciudad de Misiones', type: 'text', parentKey: 'province.misiones', updatedAt: '2025-03-08' },
  // Neuquén
  { id: 'city_nqn01', group: 'cities', key: 'city.neuquen', value: 'Neuquén', description: 'Capital del Neuquén', type: 'text', parentKey: 'province.neuquen', updatedAt: '2025-03-08' },
  { id: 'city_nqn02', group: 'cities', key: 'city.san_martin_andes', value: 'San Martín de los Andes', description: 'Ciudad del Neuquén', type: 'text', parentKey: 'province.neuquen', updatedAt: '2025-03-08' },
  // Río Negro
  { id: 'city_rn01', group: 'cities', key: 'city.viedma', value: 'Viedma', description: 'Capital de Río Negro', type: 'text', parentKey: 'province.rio_negro', updatedAt: '2025-03-08' },
  { id: 'city_rn02', group: 'cities', key: 'city.bariloche', value: 'San Carlos de Bariloche', description: 'Ciudad de Río Negro', type: 'text', parentKey: 'province.rio_negro', updatedAt: '2025-03-08' },
  { id: 'city_rn03', group: 'cities', key: 'city.cipolletti', value: 'Cipolletti', description: 'Ciudad de Río Negro', type: 'text', parentKey: 'province.rio_negro', updatedAt: '2025-03-08' },
  // Salta
  { id: 'city_sal01', group: 'cities', key: 'city.salta', value: 'Salta', description: 'Capital de Salta', type: 'text', parentKey: 'province.salta', updatedAt: '2025-03-08' },
  { id: 'city_sal02', group: 'cities', key: 'city.oran', value: 'San Ramón de la Nueva Orán', description: 'Ciudad de Salta', type: 'text', parentKey: 'province.salta', updatedAt: '2025-03-08' },
  // San Juan
  { id: 'city_sj01', group: 'cities', key: 'city.san_juan', value: 'San Juan', description: 'Capital de San Juan', type: 'text', parentKey: 'province.san_juan', updatedAt: '2025-03-08' },
  // San Luis
  { id: 'city_sl01', group: 'cities', key: 'city.san_luis', value: 'San Luis', description: 'Capital de San Luis', type: 'text', parentKey: 'province.san_luis', updatedAt: '2025-03-08' },
  { id: 'city_sl02', group: 'cities', key: 'city.villa_mercedes', value: 'Villa Mercedes', description: 'Ciudad de San Luis', type: 'text', parentKey: 'province.san_luis', updatedAt: '2025-03-08' },
  // Santa Cruz
  { id: 'city_sc01', group: 'cities', key: 'city.rio_gallegos', value: 'Río Gallegos', description: 'Capital de Santa Cruz', type: 'text', parentKey: 'province.santa_cruz', updatedAt: '2025-03-08' },
  { id: 'city_sc02', group: 'cities', key: 'city.caleta_olivia', value: 'Caleta Olivia', description: 'Ciudad de Santa Cruz', type: 'text', parentKey: 'province.santa_cruz', updatedAt: '2025-03-08' },
  // Santa Fe
  { id: 'city_sf01', group: 'cities', key: 'city.santa_fe', value: 'Santa Fe', description: 'Capital de Santa Fe', type: 'text', parentKey: 'province.santa_fe', updatedAt: '2025-03-08' },
  { id: 'city_sf02', group: 'cities', key: 'city.rosario', value: 'Rosario', description: 'Ciudad de Santa Fe', type: 'text', parentKey: 'province.santa_fe', updatedAt: '2025-03-08' },
  { id: 'city_sf03', group: 'cities', key: 'city.rafaela', value: 'Rafaela', description: 'Ciudad de Santa Fe', type: 'text', parentKey: 'province.santa_fe', updatedAt: '2025-03-08' },
  { id: 'city_sf04', group: 'cities', key: 'city.venado_tuerto', value: 'Venado Tuerto', description: 'Ciudad de Santa Fe', type: 'text', parentKey: 'province.santa_fe', updatedAt: '2025-03-08' },
  // Santiago del Estero
  { id: 'city_se01', group: 'cities', key: 'city.santiago_del_estero', value: 'Santiago del Estero', description: 'Capital de Santiago del Estero', type: 'text', parentKey: 'province.santiago_del_estero', updatedAt: '2025-03-08' },
  { id: 'city_se02', group: 'cities', key: 'city.la_banda', value: 'La Banda', description: 'Ciudad de Santiago del Estero', type: 'text', parentKey: 'province.santiago_del_estero', updatedAt: '2025-03-08' },
  // Tierra del Fuego
  { id: 'city_tf01', group: 'cities', key: 'city.ushuaia', value: 'Ushuaia', description: 'Capital de Tierra del Fuego', type: 'text', parentKey: 'province.tierra_del_fuego', updatedAt: '2025-03-08' },
  { id: 'city_tf02', group: 'cities', key: 'city.rio_grande', value: 'Río Grande', description: 'Ciudad de Tierra del Fuego', type: 'text', parentKey: 'province.tierra_del_fuego', updatedAt: '2025-03-08' },
  // Tucumán
  { id: 'city_tuc01', group: 'cities', key: 'city.san_miguel_tucuman', value: 'San Miguel de Tucumán', description: 'Capital de Tucumán', type: 'text', parentKey: 'province.tucuman', updatedAt: '2025-03-08' },
  { id: 'city_tuc02', group: 'cities', key: 'city.yerba_buena', value: 'Yerba Buena', description: 'Ciudad de Tucumán', type: 'text', parentKey: 'province.tucuman', updatedAt: '2025-03-08' },
  { id: 'city_tuc03', group: 'cities', key: 'city.concepcion_tucuman', value: 'Concepción', description: 'Ciudad de Tucumán', type: 'text', parentKey: 'province.tucuman', updatedAt: '2025-03-08' },

  // Red de Tarjetas
  { id: 'cnet_01', group: 'card_networks', key: 'card_network.visa', value: 'Visa', description: 'Red de tarjetas Visa', type: 'text', updatedAt: '2025-03-09' },
  { id: 'cnet_02', group: 'card_networks', key: 'card_network.mastercard', value: 'Mastercard', description: 'Red de tarjetas Mastercard', type: 'text', updatedAt: '2025-03-09' },
  { id: 'cnet_03', group: 'card_networks', key: 'card_network.amex', value: 'Amex', description: 'Red de tarjetas American Express', type: 'text', updatedAt: '2025-03-09' },
  { id: 'cnet_04', group: 'card_networks', key: 'card_network.cabal', value: 'Cabal', description: 'Red de tarjetas Cabal', type: 'text', updatedAt: '2025-03-09' },
  { id: 'cnet_05', group: 'card_networks', key: 'card_network.naranja', value: 'Naranja', description: 'Red de tarjetas Naranja', type: 'text', updatedAt: '2025-03-09' },

  // Niveles de Tarjetas
  { id: 'clvl_01', group: 'card_levels', key: 'card_level.classic', value: 'classic', description: 'Clásica', type: 'text', updatedAt: '2025-03-09' },
  { id: 'clvl_02', group: 'card_levels', key: 'card_level.gold', value: 'gold', description: 'Gold', type: 'text', updatedAt: '2025-03-09' },
  { id: 'clvl_03', group: 'card_levels', key: 'card_level.platinum', value: 'platinum', description: 'Platinum', type: 'text', updatedAt: '2025-03-09' },
  { id: 'clvl_04', group: 'card_levels', key: 'card_level.black', value: 'black', description: 'Black', type: 'text', updatedAt: '2025-03-09' },
  { id: 'clvl_05', group: 'card_levels', key: 'card_level.signature', value: 'signature', description: 'Signature', type: 'text', updatedAt: '2025-03-09' },
  { id: 'clvl_06', group: 'card_levels', key: 'card_level.infinite', value: 'infinite', description: 'Infinite', type: 'text', updatedAt: '2025-03-09' },

  // Coberturas de Seguros
  { id: 'inscov_01', group: 'insurance_coverages', key: 'ins_cov.vida', value: 'vida', description: 'Cobertura de Vida', type: 'text', updatedAt: '2025-03-09' },
  { id: 'inscov_02', group: 'insurance_coverages', key: 'ins_cov.incendio', value: 'incendio', description: 'Cobertura contra Incendio', type: 'text', updatedAt: '2025-03-09' },
  { id: 'inscov_03', group: 'insurance_coverages', key: 'ins_cov.robo', value: 'robo', description: 'Cobertura contra Robo', type: 'text', updatedAt: '2025-03-09' },
  { id: 'inscov_04', group: 'insurance_coverages', key: 'ins_cov.responsabilidad_civil', value: 'responsabilidad_civil', description: 'Responsabilidad Civil', type: 'text', updatedAt: '2025-03-09' },
  { id: 'inscov_05', group: 'insurance_coverages', key: 'ins_cov.accidentes_personales', value: 'accidentes_personales', description: 'Accidentes Personales', type: 'text', updatedAt: '2025-03-09' },
  { id: 'inscov_06', group: 'insurance_coverages', key: 'ins_cov.todo_riesgo', value: 'todo_riesgo', description: 'Todo Riesgo', type: 'text', updatedAt: '2025-03-09' },
  { id: 'inscov_07', group: 'insurance_coverages', key: 'ins_cov.terceros_completo', value: 'terceros_completo', description: 'Terceros Completo', type: 'text', updatedAt: '2025-03-09' },
  { id: 'inscov_08', group: 'insurance_coverages', key: 'ins_cov.granizo', value: 'granizo', description: 'Cobertura contra Granizo', type: 'text', updatedAt: '2025-03-09' },

  // Países
  { id: 'country_01', group: 'countries', key: 'country.argentina', value: 'Argentina', description: 'República Argentina', type: 'text', updatedAt: '2025-03-08' },
  { id: 'country_02', group: 'countries', key: 'country.brasil', value: 'Brasil', description: 'República Federativa del Brasil', type: 'text', updatedAt: '2025-03-08' },
  { id: 'country_03', group: 'countries', key: 'country.chile', value: 'Chile', description: 'República de Chile', type: 'text', updatedAt: '2025-03-08' },
  { id: 'country_04', group: 'countries', key: 'country.colombia', value: 'Colombia', description: 'República de Colombia', type: 'text', updatedAt: '2025-03-08' },
  { id: 'country_05', group: 'countries', key: 'country.ecuador', value: 'Ecuador', description: 'República del Ecuador', type: 'text', updatedAt: '2025-03-08' },
  { id: 'country_06', group: 'countries', key: 'country.mexico', value: 'México', description: 'Estados Unidos Mexicanos', type: 'text', updatedAt: '2025-03-08' },
  { id: 'country_07', group: 'countries', key: 'country.paraguay', value: 'Paraguay', description: 'República del Paraguay', type: 'text', updatedAt: '2025-03-08' },
  { id: 'country_08', group: 'countries', key: 'country.peru', value: 'Perú', description: 'República del Perú', type: 'text', updatedAt: '2025-03-08' },
  { id: 'country_09', group: 'countries', key: 'country.uruguay', value: 'Uruguay', description: 'República Oriental del Uruguay', type: 'text', updatedAt: '2025-03-08' },
  { id: 'country_10', group: 'countries', key: 'country.venezuela', value: 'Venezuela', description: 'República Bolivariana de Venezuela', type: 'text', updatedAt: '2025-03-08' },
  { id: 'country_11', group: 'countries', key: 'country.bolivia', value: 'Bolivia', description: 'Estado Plurinacional de Bolivia', type: 'text', updatedAt: '2025-03-08' },
  { id: 'country_12', group: 'countries', key: 'country.panama', value: 'Panamá', description: 'República de Panamá', type: 'text', updatedAt: '2025-03-08' },
  { id: 'country_13', group: 'countries', key: 'country.costa_rica', value: 'Costa Rica', description: 'República de Costa Rica', type: 'text', updatedAt: '2025-03-08' },
  { id: 'country_14', group: 'countries', key: 'country.estados_unidos', value: 'Estados Unidos', description: 'Estados Unidos de América', type: 'text', updatedAt: '2025-03-08' },
  { id: 'country_15', group: 'countries', key: 'country.espana', value: 'España', description: 'Reino de España', type: 'text', updatedAt: '2025-03-08' },

  // Monedas (ISO 4217)
  { id: 'currency_01', group: 'currencies', key: 'currency.ARS', value: 'ARS', description: 'Peso Argentino', type: 'text', updatedAt: '2025-03-08' },
  { id: 'currency_02', group: 'currencies', key: 'currency.USD', value: 'USD', description: 'Dólar Estadounidense', type: 'text', updatedAt: '2025-03-08' },
  { id: 'currency_03', group: 'currencies', key: 'currency.EUR', value: 'EUR', description: 'Euro', type: 'text', updatedAt: '2025-03-08' },
  { id: 'currency_04', group: 'currencies', key: 'currency.BRL', value: 'BRL', description: 'Real Brasileño', type: 'text', updatedAt: '2025-03-08' },
  { id: 'currency_05', group: 'currencies', key: 'currency.CLP', value: 'CLP', description: 'Peso Chileno', type: 'text', updatedAt: '2025-03-08' },
  { id: 'currency_06', group: 'currencies', key: 'currency.COP', value: 'COP', description: 'Peso Colombiano', type: 'text', updatedAt: '2025-03-08' },
  { id: 'currency_07', group: 'currencies', key: 'currency.MXN', value: 'MXN', description: 'Peso Mexicano', type: 'text', updatedAt: '2025-03-08' },
  { id: 'currency_08', group: 'currencies', key: 'currency.PEN', value: 'PEN', description: 'Sol Peruano', type: 'text', updatedAt: '2025-03-08' },
  { id: 'currency_09', group: 'currencies', key: 'currency.UYU', value: 'UYU', description: 'Peso Uruguayo', type: 'text', updatedAt: '2025-03-08' },
  { id: 'currency_10', group: 'currencies', key: 'currency.PYG', value: 'PYG', description: 'Guaraní Paraguayo', type: 'text', updatedAt: '2025-03-08' },
  { id: 'currency_11', group: 'currencies', key: 'currency.BOB', value: 'BOB', description: 'Boliviano', type: 'text', updatedAt: '2025-03-08' },
  { id: 'currency_12', group: 'currencies', key: 'currency.VES', value: 'VES', description: 'Bolívar Digital Venezolano', type: 'text', updatedAt: '2025-03-08' },
  { id: 'currency_13', group: 'currencies', key: 'currency.PAB', value: 'PAB', description: 'Balboa Panameño', type: 'text', updatedAt: '2025-03-08' },
  { id: 'currency_14', group: 'currencies', key: 'currency.CRC', value: 'CRC', description: 'Colón Costarricense', type: 'text', updatedAt: '2025-03-08' },
  { id: 'currency_15', group: 'currencies', key: 'currency.GBP', value: 'GBP', description: 'Libra Esterlina', type: 'text', updatedAt: '2025-03-08' },
  { id: 'currency_16', group: 'currencies', key: 'currency.JPY', value: 'JPY', description: 'Yen Japonés', type: 'text', updatedAt: '2025-03-08' },
  { id: 'currency_17', group: 'currencies', key: 'currency.CHF', value: 'CHF', description: 'Franco Suizo', type: 'text', updatedAt: '2025-03-08' },
  { id: 'currency_18', group: 'currencies', key: 'currency.CNY', value: 'CNY', description: 'Yuan Chino', type: 'text', updatedAt: '2025-03-08' },

  // Característica telefónica (códigos de país)
  { id: 'phone_01', group: 'phone_codes', key: 'phone.ar', value: '+54', description: 'Argentina', icon: '🇦🇷', type: 'text', parentKey: 'country.argentina', updatedAt: '2025-03-08' },
  { id: 'phone_02', group: 'phone_codes', key: 'phone.br', value: '+55', description: 'Brasil', icon: '🇧🇷', type: 'text', parentKey: 'country.brasil', updatedAt: '2025-03-08' },
  { id: 'phone_03', group: 'phone_codes', key: 'phone.cl', value: '+56', description: 'Chile', icon: '🇨🇱', type: 'text', parentKey: 'country.chile', updatedAt: '2025-03-08' },
  { id: 'phone_04', group: 'phone_codes', key: 'phone.co', value: '+57', description: 'Colombia', icon: '🇨🇴', type: 'text', parentKey: 'country.colombia', updatedAt: '2025-03-08' },
  { id: 'phone_05', group: 'phone_codes', key: 'phone.ec', value: '+593', description: 'Ecuador', icon: '🇪🇨', type: 'text', parentKey: 'country.ecuador', updatedAt: '2025-03-08' },
  { id: 'phone_06', group: 'phone_codes', key: 'phone.mx', value: '+52', description: 'México', icon: '🇲🇽', type: 'text', parentKey: 'country.mexico', updatedAt: '2025-03-08' },
  { id: 'phone_07', group: 'phone_codes', key: 'phone.py', value: '+595', description: 'Paraguay', icon: '🇵🇾', type: 'text', parentKey: 'country.paraguay', updatedAt: '2025-03-08' },
  { id: 'phone_08', group: 'phone_codes', key: 'phone.pe', value: '+51', description: 'Perú', icon: '🇵🇪', type: 'text', parentKey: 'country.peru', updatedAt: '2025-03-08' },
  { id: 'phone_09', group: 'phone_codes', key: 'phone.uy', value: '+598', description: 'Uruguay', icon: '🇺🇾', type: 'text', parentKey: 'country.uruguay', updatedAt: '2025-03-08' },
  { id: 'phone_10', group: 'phone_codes', key: 'phone.ve', value: '+58', description: 'Venezuela', icon: '🇻🇪', type: 'text', parentKey: 'country.venezuela', updatedAt: '2025-03-08' },
  { id: 'phone_11', group: 'phone_codes', key: 'phone.bo', value: '+591', description: 'Bolivia', icon: '🇧🇴', type: 'text', parentKey: 'country.bolivia', updatedAt: '2025-03-08' },
  { id: 'phone_12', group: 'phone_codes', key: 'phone.pa', value: '+507', description: 'Panamá', icon: '🇵🇦', type: 'text', parentKey: 'country.panama', updatedAt: '2025-03-08' },
  { id: 'phone_13', group: 'phone_codes', key: 'phone.cr', value: '+506', description: 'Costa Rica', icon: '🇨🇷', type: 'text', parentKey: 'country.costa_rica', updatedAt: '2025-03-08' },
  { id: 'phone_14', group: 'phone_codes', key: 'phone.us', value: '+1', description: 'Estados Unidos', icon: '🇺🇸', type: 'text', parentKey: 'country.estados_unidos', updatedAt: '2025-03-08' },
  { id: 'phone_15', group: 'phone_codes', key: 'phone.es', value: '+34', description: 'España', icon: '🇪🇸', type: 'text', parentKey: 'country.espana', updatedAt: '2025-03-08' },
  { id: 'phone_16', group: 'phone_codes', key: 'phone.ca', value: '+1', description: 'Canadá', icon: '🇨🇦', type: 'text', updatedAt: '2025-03-08' },
  { id: 'phone_17', group: 'phone_codes', key: 'phone.gb', value: '+44', description: 'Reino Unido', icon: '🇬🇧', type: 'text', updatedAt: '2025-03-08' },
  { id: 'phone_18', group: 'phone_codes', key: 'phone.fr', value: '+33', description: 'Francia', icon: '🇫🇷', type: 'text', updatedAt: '2025-03-08' },
  { id: 'phone_19', group: 'phone_codes', key: 'phone.de', value: '+49', description: 'Alemania', icon: '🇩🇪', type: 'text', updatedAt: '2025-03-08' },
  { id: 'phone_20', group: 'phone_codes', key: 'phone.it', value: '+39', description: 'Italia', icon: '🇮🇹', type: 'text', updatedAt: '2025-03-08' },
  { id: 'phone_21', group: 'phone_codes', key: 'phone.pt', value: '+351', description: 'Portugal', icon: '🇵🇹', type: 'text', updatedAt: '2025-03-08' },
  { id: 'phone_22', group: 'phone_codes', key: 'phone.nl', value: '+31', description: 'Países Bajos', icon: '🇳🇱', type: 'text', updatedAt: '2025-03-08' },
  { id: 'phone_23', group: 'phone_codes', key: 'phone.be', value: '+32', description: 'Bélgica', icon: '🇧🇪', type: 'text', updatedAt: '2025-03-08' },
  { id: 'phone_24', group: 'phone_codes', key: 'phone.ch', value: '+41', description: 'Suiza', icon: '🇨🇭', type: 'text', updatedAt: '2025-03-08' },
  { id: 'phone_25', group: 'phone_codes', key: 'phone.at', value: '+43', description: 'Austria', icon: '🇦🇹', type: 'text', updatedAt: '2025-03-08' },
  { id: 'phone_26', group: 'phone_codes', key: 'phone.se', value: '+46', description: 'Suecia', icon: '🇸🇪', type: 'text', updatedAt: '2025-03-08' },
  { id: 'phone_27', group: 'phone_codes', key: 'phone.no', value: '+47', description: 'Noruega', icon: '🇳🇴', type: 'text', updatedAt: '2025-03-08' },
  { id: 'phone_28', group: 'phone_codes', key: 'phone.dk', value: '+45', description: 'Dinamarca', icon: '🇩🇰', type: 'text', updatedAt: '2025-03-08' },
  { id: 'phone_29', group: 'phone_codes', key: 'phone.fi', value: '+358', description: 'Finlandia', icon: '🇫🇮', type: 'text', updatedAt: '2025-03-08' },
  { id: 'phone_30', group: 'phone_codes', key: 'phone.ie', value: '+353', description: 'Irlanda', icon: '🇮🇪', type: 'text', updatedAt: '2025-03-08' },
  { id: 'phone_31', group: 'phone_codes', key: 'phone.pl', value: '+48', description: 'Polonia', icon: '🇵🇱', type: 'text', updatedAt: '2025-03-08' },
  { id: 'phone_32', group: 'phone_codes', key: 'phone.cz', value: '+420', description: 'Rep. Checa', icon: '🇨🇿', type: 'text', updatedAt: '2025-03-08' },
  { id: 'phone_33', group: 'phone_codes', key: 'phone.ro', value: '+40', description: 'Rumania', icon: '🇷🇴', type: 'text', updatedAt: '2025-03-08' },
  { id: 'phone_34', group: 'phone_codes', key: 'phone.hu', value: '+36', description: 'Hungría', icon: '🇭🇺', type: 'text', updatedAt: '2025-03-08' },
  { id: 'phone_35', group: 'phone_codes', key: 'phone.gr', value: '+30', description: 'Grecia', icon: '🇬🇷', type: 'text', updatedAt: '2025-03-08' },
  { id: 'phone_36', group: 'phone_codes', key: 'phone.tr', value: '+90', description: 'Turquía', icon: '🇹🇷', type: 'text', updatedAt: '2025-03-08' },
  { id: 'phone_37', group: 'phone_codes', key: 'phone.ru', value: '+7', description: 'Rusia', icon: '🇷🇺', type: 'text', updatedAt: '2025-03-08' },
  { id: 'phone_38', group: 'phone_codes', key: 'phone.ua', value: '+380', description: 'Ucrania', icon: '🇺🇦', type: 'text', updatedAt: '2025-03-08' },
  { id: 'phone_39', group: 'phone_codes', key: 'phone.il', value: '+972', description: 'Israel', icon: '🇮🇱', type: 'text', updatedAt: '2025-03-08' },
  { id: 'phone_40', group: 'phone_codes', key: 'phone.ae', value: '+971', description: 'EAU', icon: '🇦🇪', type: 'text', updatedAt: '2025-03-08' },
  { id: 'phone_41', group: 'phone_codes', key: 'phone.sa', value: '+966', description: 'Arabia Saudita', icon: '🇸🇦', type: 'text', updatedAt: '2025-03-08' },
  { id: 'phone_42', group: 'phone_codes', key: 'phone.eg', value: '+20', description: 'Egipto', icon: '🇪🇬', type: 'text', updatedAt: '2025-03-08' },
  { id: 'phone_43', group: 'phone_codes', key: 'phone.za', value: '+27', description: 'Sudáfrica', icon: '🇿🇦', type: 'text', updatedAt: '2025-03-08' },
  { id: 'phone_44', group: 'phone_codes', key: 'phone.ng', value: '+234', description: 'Nigeria', icon: '🇳🇬', type: 'text', updatedAt: '2025-03-08' },
  { id: 'phone_45', group: 'phone_codes', key: 'phone.ke', value: '+254', description: 'Kenia', icon: '🇰🇪', type: 'text', updatedAt: '2025-03-08' },
  { id: 'phone_46', group: 'phone_codes', key: 'phone.ma', value: '+212', description: 'Marruecos', icon: '🇲🇦', type: 'text', updatedAt: '2025-03-08' },
  { id: 'phone_47', group: 'phone_codes', key: 'phone.cn', value: '+86', description: 'China', icon: '🇨🇳', type: 'text', updatedAt: '2025-03-08' },
  { id: 'phone_48', group: 'phone_codes', key: 'phone.jp', value: '+81', description: 'Japón', icon: '🇯🇵', type: 'text', updatedAt: '2025-03-08' },
  { id: 'phone_49', group: 'phone_codes', key: 'phone.kr', value: '+82', description: 'Corea del Sur', icon: '🇰🇷', type: 'text', updatedAt: '2025-03-08' },
  { id: 'phone_50', group: 'phone_codes', key: 'phone.in', value: '+91', description: 'India', icon: '🇮🇳', type: 'text', updatedAt: '2025-03-08' },
  { id: 'phone_51', group: 'phone_codes', key: 'phone.pk', value: '+92', description: 'Pakistán', icon: '🇵🇰', type: 'text', updatedAt: '2025-03-08' },
  { id: 'phone_52', group: 'phone_codes', key: 'phone.bd', value: '+880', description: 'Bangladesh', icon: '🇧🇩', type: 'text', updatedAt: '2025-03-08' },
  { id: 'phone_53', group: 'phone_codes', key: 'phone.id', value: '+62', description: 'Indonesia', icon: '🇮🇩', type: 'text', updatedAt: '2025-03-08' },
  { id: 'phone_54', group: 'phone_codes', key: 'phone.my', value: '+60', description: 'Malasia', icon: '🇲🇾', type: 'text', updatedAt: '2025-03-08' },
  { id: 'phone_55', group: 'phone_codes', key: 'phone.th', value: '+66', description: 'Tailandia', icon: '🇹🇭', type: 'text', updatedAt: '2025-03-08' },
  { id: 'phone_56', group: 'phone_codes', key: 'phone.vn', value: '+84', description: 'Vietnam', icon: '🇻🇳', type: 'text', updatedAt: '2025-03-08' },
  { id: 'phone_57', group: 'phone_codes', key: 'phone.ph', value: '+63', description: 'Filipinas', icon: '🇵🇭', type: 'text', updatedAt: '2025-03-08' },
  { id: 'phone_58', group: 'phone_codes', key: 'phone.sg', value: '+65', description: 'Singapur', icon: '🇸🇬', type: 'text', updatedAt: '2025-03-08' },
  { id: 'phone_59', group: 'phone_codes', key: 'phone.au', value: '+61', description: 'Australia', icon: '🇦🇺', type: 'text', updatedAt: '2025-03-08' },
  { id: 'phone_60', group: 'phone_codes', key: 'phone.nz', value: '+64', description: 'Nueva Zelanda', icon: '🇳🇿', type: 'text', updatedAt: '2025-03-08' },
  { id: 'phone_61', group: 'phone_codes', key: 'phone.gt', value: '+502', description: 'Guatemala', icon: '🇬🇹', type: 'text', updatedAt: '2025-03-08' },
  { id: 'phone_62', group: 'phone_codes', key: 'phone.hn', value: '+504', description: 'Honduras', icon: '🇭🇳', type: 'text', updatedAt: '2025-03-08' },
  { id: 'phone_63', group: 'phone_codes', key: 'phone.sv', value: '+503', description: 'El Salvador', icon: '🇸🇻', type: 'text', updatedAt: '2025-03-08' },
  { id: 'phone_64', group: 'phone_codes', key: 'phone.ni', value: '+505', description: 'Nicaragua', icon: '🇳🇮', type: 'text', updatedAt: '2025-03-08' },
  { id: 'phone_65', group: 'phone_codes', key: 'phone.cu', value: '+53', description: 'Cuba', icon: '🇨🇺', type: 'text', updatedAt: '2025-03-08' },
  { id: 'phone_66', group: 'phone_codes', key: 'phone.do', value: '+1-809', description: 'Rep. Dominicana', icon: '🇩🇴', type: 'text', updatedAt: '2025-03-08' },
  { id: 'phone_67', group: 'phone_codes', key: 'phone.pr', value: '+1-787', description: 'Puerto Rico', icon: '🇵🇷', type: 'text', updatedAt: '2025-03-08' },
  { id: 'phone_68', group: 'phone_codes', key: 'phone.jm', value: '+1-876', description: 'Jamaica', icon: '🇯🇲', type: 'text', updatedAt: '2025-03-08' },
  { id: 'phone_69', group: 'phone_codes', key: 'phone.tt', value: '+1-868', description: 'Trinidad y Tobago', icon: '🇹🇹', type: 'text', updatedAt: '2025-03-08' },
  { id: 'phone_70', group: 'phone_codes', key: 'phone.ht', value: '+509', description: 'Haití', icon: '🇭🇹', type: 'text', updatedAt: '2025-03-08' },
];
