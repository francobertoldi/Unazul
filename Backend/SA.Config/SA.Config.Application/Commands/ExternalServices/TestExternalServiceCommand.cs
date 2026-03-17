using Mediator;
using SA.Config.Application.Dtos.ExternalServices;

namespace SA.Config.Application.Commands.ExternalServices;

public readonly record struct TestExternalServiceCommand(Guid Id) : ICommand<TestResultDto>;
