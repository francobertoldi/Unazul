using Mediator;
using SA.Operations.Application.Events;
using SA.Operations.Application.Interfaces;
using SA.Operations.DataAccess.Interface.Repositories;
using SA.Operations.Domain.Entities;
using SA.Operations.Domain.Enums;
using Shared.Contract.Exceptions;

namespace SA.Operations.Application.Commands.Applications;

public sealed class UpdateApplicationCommandHandler(
    IApplicationRepository applicationRepository,
    IApplicantRepository applicantRepository,
    IContactRepository contactRepository,
    IAddressRepository addressRepository,
    IBeneficiaryRepository beneficiaryRepository,
    ICatalogServiceClient catalogClient,
    IIntegrationEventPublisher eventPublisher) : ICommandHandler<UpdateApplicationCommand, UpdateApplicationResult>
{
    public async ValueTask<UpdateApplicationResult> Handle(UpdateApplicationCommand command, CancellationToken ct)
    {
        var app = await applicationRepository.GetByIdAsync(command.ApplicationId, ct);
        if (app is null || app.TenantId != command.TenantId)
            throw new NotFoundException("OPS_APPLICATION_NOT_FOUND", "Solicitud no encontrada.");

        if (app.Status != ApplicationStatus.Draft)
            throw new ValidationException("OPS_ONLY_DRAFT_EDITABLE", "Solo se pueden editar solicitudes en borrador.");

        // Validate product if changed
        string? productName = null;
        string? planName = null;
        if (command.ProductId.HasValue && command.PlanId.HasValue)
        {
            var catalogResult = await catalogClient.ValidateProductAndPlanAsync(command.ProductId.Value, command.PlanId.Value, ct);
            if (catalogResult is null)
                throw new NotFoundException("OPS_PRODUCT_PLAN_NOT_FOUND", "Producto o plan no encontrado.");
            if (!catalogResult.IsActive)
                throw new ValidationException("OPS_PRODUCT_PLAN_INACTIVE", "El plan de producto no esta activo.");
            productName = catalogResult.ProductName;
            planName = catalogResult.PlanName;
        }

        // Update application fields
        app.UpdateDraft(command.EntityId, command.ProductId, command.PlanId, productName, planName, command.UpdatedBy);
        applicationRepository.Update(app);
        await applicationRepository.SaveChangesAsync(ct);

        // Update applicant if data provided
        var applicant = await applicantRepository.GetByIdAsync(app.ApplicantId, ct);
        if (applicant is not null && command.FirstName is not null)
        {
            Gender? gender = null;
            if (!string.IsNullOrWhiteSpace(command.Gender) && Enum.TryParse<Gender>(command.Gender, true, out var g))
                gender = g;

            applicant.Update(
                command.FirstName,
                command.LastName ?? applicant.LastName,
                command.BirthDate ?? applicant.BirthDate,
                gender ?? applicant.Gender,
                command.Occupation ?? applicant.Occupation);
            applicantRepository.Update(applicant);
            await applicantRepository.SaveChangesAsync(ct);
        }

        // Replace contacts if provided
        if (command.Contacts is not null && applicant is not null)
        {
            // Delete existing then add new
            var existing = await contactRepository.GetByApplicantIdAsync(applicant.Id, ct);
            foreach (var c in existing)
                contactRepository.Delete(c);
            await contactRepository.SaveChangesAsync(ct);

            foreach (var c in command.Contacts)
            {
                if (!Enum.TryParse<ContactType>(c.Type, true, out var contactType))
                    throw new ValidationException("OPS_INVALID_CONTACT_TYPE", "Tipo de contacto invalido.");
                await contactRepository.AddAsync(
                    ApplicantContact.Create(applicant.Id, command.TenantId, contactType, c.Email, c.PhoneCode, c.Phone), ct);
            }

            await contactRepository.SaveChangesAsync(ct);
        }

        // Replace addresses if provided
        if (command.Addresses is not null && applicant is not null)
        {
            var existing = await addressRepository.GetByApplicantIdAsync(applicant.Id, ct);
            foreach (var a in existing)
                addressRepository.Delete(a);
            await addressRepository.SaveChangesAsync(ct);

            foreach (var a in command.Addresses)
            {
                if (!Enum.TryParse<AddressType>(a.Type, true, out var addrType))
                    throw new ValidationException("OPS_INVALID_ADDRESS_TYPE", "Tipo de direccion invalido.");
                await addressRepository.AddAsync(
                    ApplicantAddress.Create(applicant.Id, command.TenantId, addrType, a.Street, a.Number, a.Floor, a.Apartment, a.City, a.Province, a.PostalCode, a.Latitude, a.Longitude), ct);
            }

            await addressRepository.SaveChangesAsync(ct);
        }

        // Replace beneficiaries if provided
        if (command.Beneficiaries is not null)
        {
            var existing = await beneficiaryRepository.GetByApplicationIdAsync(app.Id, ct);
            foreach (var b in existing)
                beneficiaryRepository.Delete(b);
            await beneficiaryRepository.SaveChangesAsync(ct);

            foreach (var b in command.Beneficiaries)
            {
                await beneficiaryRepository.AddAsync(
                    Beneficiary.Create(app.Id, command.TenantId, b.FirstName, b.LastName, b.Relationship, b.Percentage), ct);
            }

            await beneficiaryRepository.SaveChangesAsync(ct);
        }

        // Publish event
        await eventPublisher.PublishAsync(new ApplicationStatusChangedEvent(
            app.Id,
            app.Status.ToString(),
            app.Status.ToString(),
            command.UpdatedBy,
            command.TenantId,
            DateTimeOffset.UtcNow,
            Guid.CreateVersion7()), ct);

        return new UpdateApplicationResult(app.Id, app.Code, app.Status.ToString());
    }
}
