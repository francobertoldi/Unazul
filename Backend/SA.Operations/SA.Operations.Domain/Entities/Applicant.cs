using SA.Operations.Domain.Enums;

namespace SA.Operations.Domain.Entities;

public sealed class Applicant
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public DocumentType DocumentType { get; private set; }
    public string DocumentNumber { get; private set; } = string.Empty;
    public DateOnly? BirthDate { get; private set; }
    public Gender? Gender { get; private set; }
    public string? Occupation { get; private set; }

    // Navigation properties
    public ICollection<ApplicantContact> Contacts { get; private set; } = [];
    public ICollection<ApplicantAddress> Addresses { get; private set; } = [];

    private Applicant() { }

    public static Applicant Create(
        Guid tenantId,
        string firstName,
        string lastName,
        DocumentType docType,
        string docNumber,
        DateOnly? birthDate,
        Gender? gender,
        string? occupation)
    {
        return new Applicant
        {
            Id = Guid.CreateVersion7(),
            TenantId = tenantId,
            FirstName = firstName,
            LastName = lastName,
            DocumentType = docType,
            DocumentNumber = docNumber,
            BirthDate = birthDate,
            Gender = gender,
            Occupation = occupation
        };
    }

    public void Update(
        string firstName,
        string lastName,
        DateOnly? birthDate,
        Gender? gender,
        string? occupation)
    {
        FirstName = firstName;
        LastName = lastName;
        BirthDate = birthDate;
        Gender = gender;
        Occupation = occupation;
    }
}
