using FluentAssertions;
using SA.Operations.Domain.Entities;
using SA.Operations.Domain.Enums;
using Xunit;

namespace SA.Operations.Tests.Unit.Domain;

public sealed class ApplicationStateMachineTests
{
    [Theory(DisplayName = "TP_OPS_SM_01-07: Valid transitions return true")]
    [InlineData(ApplicationStatus.Draft, ApplicationStatus.Pending, "TP_OPS_SM_01")]
    [InlineData(ApplicationStatus.Draft, ApplicationStatus.Cancelled, "TP_OPS_SM_02")]
    [InlineData(ApplicationStatus.Pending, ApplicationStatus.InReview, "TP_OPS_SM_03")]
    [InlineData(ApplicationStatus.Pending, ApplicationStatus.Cancelled, "TP_OPS_SM_04")]
    [InlineData(ApplicationStatus.InReview, ApplicationStatus.Approved, "TP_OPS_SM_05")]
    [InlineData(ApplicationStatus.InReview, ApplicationStatus.Rejected, "TP_OPS_SM_06")]
    [InlineData(ApplicationStatus.InReview, ApplicationStatus.Cancelled, "TP_OPS_SM_07")]
    public void Valid_Transitions_Return_True(ApplicationStatus from, ApplicationStatus to, string testId)
    {
        // Act
        var result = ApplicationStateMachine.IsValidTransition(from, to);

        // Assert
        result.Should().BeTrue($"{testId}: {from} -> {to} should be valid");
    }

    [Theory(DisplayName = "TP_OPS_SM_08-10: Invalid transitions return false")]
    [InlineData(ApplicationStatus.Draft, ApplicationStatus.Approved, "TP_OPS_SM_08")]
    [InlineData(ApplicationStatus.Approved, ApplicationStatus.Settled, "TP_OPS_SM_09")]
    [InlineData(ApplicationStatus.Rejected, ApplicationStatus.Draft, "TP_OPS_SM_10")]
    public void Invalid_Transitions_Return_False(ApplicationStatus from, ApplicationStatus to, string testId)
    {
        // Act
        var result = ApplicationStateMachine.IsValidTransition(from, to);

        // Assert
        result.Should().BeFalse($"{testId}: {from} -> {to} should be invalid");
    }

    [Fact(DisplayName = "TP_OPS_SM_Extra: Cancelled is a terminal state")]
    public void Cancelled_Is_Terminal_State()
    {
        // Act
        var result = ApplicationStateMachine.IsValidTransition(ApplicationStatus.Cancelled, ApplicationStatus.Draft);

        // Assert
        result.Should().BeFalse("Cancelled -> Draft should be invalid (terminal state)");
    }
}
