using SA.Operations.Api.ViewModels.Applicants;
using SA.Operations.Domain.Entities;

namespace SA.Operations.Api.Mappers.Applicants;

public static class ApplicantMapper
{
    public static ApplicantResponse ToResponse(
        Applicant applicant,
        IReadOnlyList<ApplicantContact> contacts,
        IReadOnlyList<ApplicantAddress> addresses,
        int applicationCount)
    {
        return new ApplicantResponse(
            applicant.Id,
            applicant.FirstName,
            applicant.LastName,
            applicant.DocumentType,
            applicant.DocumentNumber,
            applicant.BirthDate,
            applicant.Gender,
            applicant.Occupation,
            contacts.Select(ToContactResponse).ToList(),
            addresses.Select(ToAddressResponse).ToList(),
            applicationCount);
    }

    public static ContactResponse ToContactResponse(ApplicantContact contact)
    {
        return new ContactResponse(
            contact.Id,
            contact.Type,
            contact.Email,
            contact.PhoneCode,
            contact.Phone);
    }

    public static AddressResponse ToAddressResponse(ApplicantAddress address)
    {
        return new AddressResponse(
            address.Id,
            address.Type,
            address.Street,
            address.Number,
            address.Floor,
            address.Apartment,
            address.City,
            address.Province,
            address.PostalCode,
            address.Latitude,
            address.Longitude);
    }
}
