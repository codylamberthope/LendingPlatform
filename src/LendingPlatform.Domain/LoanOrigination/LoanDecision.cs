namespace LendingPlatform.Domain.LoanOrigination;

public sealed class LoanDecision
{
    public bool IsApproved { get; }

    public IReadOnlyList<EligibilityDeclineReason> DeclineReasons { get; }

    private LoanDecision(bool isApproved, IReadOnlyList<EligibilityDeclineReason> declineReasons)
    {
        IsApproved = isApproved;
        DeclineReasons = declineReasons;
    }

    public static LoanDecision Approved() => new(true, Array.Empty<EligibilityDeclineReason>());

    public static LoanDecision Declined(IReadOnlyList<EligibilityDeclineReason> reasons)
    {
        if (reasons.Count == 0)
        {
            throw new ArgumentException("A declined decision must include at least one reason.", nameof(reasons));
        }

        return new LoanDecision(false, reasons.ToArray());
    }

    public static LoanDecision FromRecordedState(bool isApproved, IReadOnlyList<EligibilityDeclineReason> reasons)
    {
        return isApproved ? Approved() : Declined(reasons);
    }
}
