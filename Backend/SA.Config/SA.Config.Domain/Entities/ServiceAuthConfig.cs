namespace SA.Config.Domain.Entities;

public sealed class ServiceAuthConfig
{
    public Guid Id { get; private set; }
    public Guid ServiceId { get; private set; }
    public string Key { get; private set; } = string.Empty;
    public string ValueEncrypted { get; private set; } = string.Empty;

    private ServiceAuthConfig() { }

    public static ServiceAuthConfig Create(
        Guid serviceId,
        string key,
        string valueEncrypted)
    {
        return new ServiceAuthConfig
        {
            Id = Guid.CreateVersion7(),
            ServiceId = serviceId,
            Key = key,
            ValueEncrypted = valueEncrypted
        };
    }
}
