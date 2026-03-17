using Mediator;
using SA.Operations.Application.Events;
using SA.Operations.Application.Interfaces;
using SA.Operations.DataAccess.Interface.Repositories;
using SA.Operations.Domain.Entities;
using SA.Operations.Domain.Enums;
using Shared.Contract.Exceptions;

namespace SA.Operations.Application.Commands.Applications;

public sealed class CreateApplicationCommandHandler(
    IApplicationRepository applicationRepository,
    IApplicantRepository applicantRepository,
    IContactRepository contactRepository,
    IAddressRepository addressRepository,
    IBeneficiaryRepository beneficiaryRepository,
    ITraceEventRepository traceEventRepository,
    ICatalogServiceClient catalogClient,
    IIntegrationEventPublisher eventPublisher) : ICommandHandler<CreateApplicationCommand, CreateApplicationResult>
{
    public async ValueTask<CreateApplicationResult> Handle(CreateApplicationCommand command, CancellationToken ct)
    {
        // Validate product & plan via Catalog service
        var catalogResult = await catalogClient.ValidateProductAndPlanAsync(command.ProductId, command.PlanId, ct);
        if (catalogResult is null)
            throw new NotFoundException("OPS_PRODUCT_PLAN_NOT_FOUND", "Producto o plan no encontrado.");
        if (!catalogResult.IsActive)
            throw new ValidationException("OPS_PRODUCT_PLAN_INACTIVE", "El plan de producto no esta activo.");

        // Parse enums
        if (!Enum.TryParse<DocumentType>(command.DocumentType, true, out var docType))
            throw new ValidationException("OPS_INVALID_DOCUMENT_TYPE", "Tipo de documento invalido.");

        Gender? gender = null;
        if (!string.IsNullOrWhiteSpace(command.Gender) && Enum.TryParse<Gender>(command.Gender, true, out var g))
            gender = g;

        // Upsert applicant
        var applicant = await applicantRepository.GetByDocumentAsync(command.TenantId, docType, command.DocumentNumber, ct);
        if (applicant is null)
        {
            applicant = Applicant.Create(
                command.TenantId,
                command.FirstName,
                command.LastName,
                docType,
                command.DocumentNumber,
                command.BirthDate,
                gender,
                command.Occupation);
            await applicantRepository.AddAsync(applicant, ct);
        }
        else
        {
            applicant.Update(command.FirstName, command.LastName, command.BirthDate, gender, command.Occupation);
            applicantRepository.Update(applicant);
        }

        await applicantRepository.SaveChangesAsync(ct);

        // Sync contacts
        if (command.Contacts is { Length: > 0 })
        {
            var contacts = command.Contacts.Select(c =>
            {
                if (!Enum.TryParse<ContactType>(c.Type, true, out var contactType))
                    throw new ValidationException("OPS_INVALID_CONTACT_TYPE", "Tipo de contacto invalido.");
                return ApplicantContact.Create(applicant.Id, command.TenantId, contactType, c.Email, c.PhoneCode, c.Phone);
            }).ToList();

            foreach (var contact in contacts)
                await contactRepository.AddAsync(contact, ct);
            await contactRepository.SaveChangesAsync(ct);
        }

        // Sync addresses
        if (command.Addresses is { Length: > 0 })
        {
            var addresses = command.Addresses.Select(a =>
            {
                if (!Enum.TryParse<AddressType>(a.Type, true, out var addrType))
                    throw new ValidationException("OPS_INVALID_ADDRESS_TYPE", "Tipo de direccion invalido.");
                return ApplicantAddress.Create(applicant.Id, command.TenantId, addrType, a.Street, a.Number, a.Floor, a.Apartment, a.City, a.Province, a.PostalCode, a.Latitude, a.Longitude);
            }).ToList();

            foreach (var address in addresses)
                await addressRepository.AddAsync(address, ct);
            await addressRepository.SaveChangesAsync(ct);
        }

        // Generate code SOL-YYYY-NNN
        var year = DateTime.UtcNow.Year;
        var sequence = await applicationRepository.GetNextSequenceAsync(command.TenantId, ct);
        var code = $"SOL-{year}-{sequence:D3}";

        // Create application
        var application = Domain.Entities.Application.Create(
            command.TenantId,
            command.EntityId,
            applicant.Id,
            code,
            command.ProductId,
            command.PlanId,
            catalogResult.ProductName,
            catalogResult.PlanName,
            command.CreatedBy);

        await applicationRepository.AddAsync(application, ct);
        await applicationRepository.SaveChangesAsync(ct);

        // Insert beneficiaries
        if (command.Beneficiaries is { Length: > 0 })
        {
            foreach (var b in command.Beneficiaries)
            {
                var beneficiary = Beneficiary.Create(application.Id, command.TenantId, b.FirstName, b.LastName, b.Relationship, b.Percentage);
                await beneficiaryRepository.AddAsync(beneficiary, ct);
            }

            await beneficiaryRepository.SaveChangesAsync(ct);
        }

        // Insert trace event
        var traceEvent = TraceEvent.Create(
            application.Id,
            command.TenantId,
            ApplicationStatus.Draft.ToString(),
            "created",
            command.CreatedBy,
            command.CreatedByName);
        await traceEventRepository.AddAsync(traceEvent, ct);
        await traceEventRepository.SaveChangesAsync(ct);

        // Publish integration event
        await eventPublisher.PublishAsync(new ApplicationCreatedEvent(
            application.Id,
            code,
            command.TenantId,
            command.EntityId,
            command.ProductId,
            DateTimeOffset.UtcNow,
            Guid.CreateVersion7()), ct);

        return new CreateApplicationResult(application.Id, code, ApplicationStatus.Draft.ToString());
    }
}
