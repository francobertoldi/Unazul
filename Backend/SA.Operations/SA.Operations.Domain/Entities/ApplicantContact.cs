using SA.Operations.Domain.Enums;

namespace SA.Operations.Domain.Entities;

public sealed class ApplicantContact
{
    public Guid Id { get; private set; }
    public Guid ApplicantId { get; private set; }
    public Guid TenantId { get; private set; }
    public ContactType Type { get; private set; }
    public string? Email { get; private set; }
    public string? PhoneCode { get; private set; }
    public string? Phone { get; private set; }

    private ApplicantContact() { }

    public static ApplicantContact Create(
        Guid applicantId,
        Guid tenantId,
        ContactType type,
        string? email,
        string? phoneCode,
        string? phone)
    {
        return new ApplicantContact
        {
            Id = Guid.CreateVersion7(),
            ApplicantId = applicantId,
            TenantId = tenantId,
            Type = type,
            Email = email,
            PhoneCode = phoneCode,
            Phone = phone
        };
    }

    public void Update(
        ContactType type,
        string? email,
        string? phoneCode,
        string? phone)
    {
        Type = type;
        Email = email;
        PhoneCode = phoneCode;
        Phone = phone;
    }
}
