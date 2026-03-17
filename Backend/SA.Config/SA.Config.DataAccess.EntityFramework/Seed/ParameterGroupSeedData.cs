namespace SA.Config.DataAccess.EntityFramework.Seed;

public static class ParameterGroupSeedData
{
    public static object[] GetSeedObjects() =>
    [
        // General (sort 1-5)
        new { Id = new Guid("019573a0-0001-7000-0000-000000000001"), Code = "empresa_info", Name = "Informacion de Empresa", Category = "General", Icon = "mdi-office-building", SortOrder = 1 },
        new { Id = new Guid("019573a0-0001-7000-0000-000000000002"), Code = "monedas", Name = "Monedas", Category = "General", Icon = "mdi-currency-usd", SortOrder = 2 },
        new { Id = new Guid("019573a0-0001-7000-0000-000000000003"), Code = "paises", Name = "Paises", Category = "General", Icon = "mdi-earth", SortOrder = 3 },
        new { Id = new Guid("019573a0-0001-7000-0000-000000000004"), Code = "provincias", Name = "Provincias", Category = "General", Icon = "mdi-map", SortOrder = 4 },
        new { Id = new Guid("019573a0-0001-7000-0000-000000000005"), Code = "ciudades", Name = "Ciudades", Category = "General", Icon = "mdi-city", SortOrder = 5 },

        // Tecnico (sort 1-3)
        new { Id = new Guid("019573a0-0002-7000-0000-000000000001"), Code = "seguridad", Name = "Seguridad", Category = "Tecnico", Icon = "mdi-shield-lock", SortOrder = 1 },
        new { Id = new Guid("019573a0-0002-7000-0000-000000000002"), Code = "integraciones", Name = "Integraciones", Category = "Tecnico", Icon = "mdi-puzzle", SortOrder = 2 },
        new { Id = new Guid("019573a0-0002-7000-0000-000000000003"), Code = "rendimiento", Name = "Rendimiento", Category = "Tecnico", Icon = "mdi-speedometer", SortOrder = 3 },

        // Notificaciones (sort 1-3)
        new { Id = new Guid("019573a0-0003-7000-0000-000000000001"), Code = "email_config", Name = "Configuracion de Email", Category = "Notificaciones", Icon = "mdi-email", SortOrder = 1 },
        new { Id = new Guid("019573a0-0003-7000-0000-000000000002"), Code = "sms_config", Name = "Configuracion de SMS", Category = "Notificaciones", Icon = "mdi-message-text", SortOrder = 2 },
        new { Id = new Guid("019573a0-0003-7000-0000-000000000003"), Code = "whatsapp_config", Name = "Configuracion de WhatsApp", Category = "Notificaciones", Icon = "mdi-whatsapp", SortOrder = 3 },

        // Datos (sort 1-3)
        new { Id = new Guid("019573a0-0004-7000-0000-000000000001"), Code = "tipos_documento", Name = "Tipos de Documento", Category = "Datos", Icon = "mdi-file-document", SortOrder = 1 },
        new { Id = new Guid("019573a0-0004-7000-0000-000000000002"), Code = "estados_civiles", Name = "Estados Civiles", Category = "Datos", Icon = "mdi-account-heart", SortOrder = 2 },
        new { Id = new Guid("019573a0-0004-7000-0000-000000000003"), Code = "ocupaciones", Name = "Ocupaciones", Category = "Datos", Icon = "mdi-briefcase", SortOrder = 3 },
    ];
}
