using SA.Operations.Domain.Enums;

namespace SA.Operations.Domain.Entities;

public static class ApplicationStateMachine
{
    private static readonly Dictionary<ApplicationStatus, ApplicationStatus[]> ValidTransitions = new()
    {
        [ApplicationStatus.Draft] = [ApplicationStatus.Pending, ApplicationStatus.Cancelled],
        [ApplicationStatus.Pending] = [ApplicationStatus.InReview, ApplicationStatus.Cancelled],
        [ApplicationStatus.InReview] = [ApplicationStatus.Approved, ApplicationStatus.Rejected, ApplicationStatus.Cancelled],
        // Approved → Settled is NOT here — only via settlement process
    };

    public static bool IsValidTransition(ApplicationStatus from, ApplicationStatus to)
    {
        return ValidTransitions.TryGetValue(from, out var allowed) && allowed.Contains(to);
    }
}
