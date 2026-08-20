namespace LendingPlatform.Domain.LoanOrigination;

/// <summary>
/// Product rules for a secured loan. Every applicable failure is collected; evaluation does not stop at the first.
/// Sub-£1m LTV bands are exclusive and evaluated in listed order.
/// </summary>
public sealed class LoanEligibilityPolicy
{
    public static PoundSterlingAmount MinimumLendableAmount { get; } = PoundSterlingAmount.FromPounds(100_000m);

    public static PoundSterlingAmount MaximumLendableAmount { get; } = PoundSterlingAmount.FromPounds(1_500_000m);

    public static PoundSterlingAmount LargeLoanAmountThreshold { get; } = PoundSterlingAmount.FromPounds(1_000_000m);

    public const decimal MaximumLoanToValueForLargeLoan = 0.60m;

    public const decimal LowerLoanToValueBandUpperBound = 0.60m;

    public const decimal MidLoanToValueBandUpperBound = 0.80m;

    public const decimal HighLoanToValueBandUpperBound = 0.90m;

    public const int MinimumCreditScoreForLowerLoanToValueBand = 750;

    public const int MinimumCreditScoreForMidLoanToValueBand = 800;

    public const int MinimumCreditScoreForHighLoanToValueBand = 900;

    public const int MinimumCreditScoreForLargeLoan = 950;

    private static readonly SmallLoanCreditBand[] SmallLoanCreditBands =
    [
        new(
            LowerLoanToValueBandUpperBound,
            MinimumCreditScoreForLowerLoanToValueBand,
            $"loan to value below {AsPercent(LowerLoanToValueBandUpperBound)}"),
        new(
            MidLoanToValueBandUpperBound,
            MinimumCreditScoreForMidLoanToValueBand,
            $"loan to value of at least {AsPercent(LowerLoanToValueBandUpperBound)} and below {AsPercent(MidLoanToValueBandUpperBound)}"),
        new(
            HighLoanToValueBandUpperBound,
            MinimumCreditScoreForHighLoanToValueBand,
            $"loan to value of at least {AsPercent(MidLoanToValueBandUpperBound)} and below {AsPercent(HighLoanToValueBandUpperBound)}")
    ];

    public LoanDecision Evaluate(ProposedSecuredLoan proposedApplication)
    {
        var declineReasons = new List<EligibilityDeclineReason>();

        CollectAmountLimitFailures(proposedApplication.LoanAmount, declineReasons);

        if (proposedApplication.LoanAmount.IsAtLeast(LargeLoanAmountThreshold))
        {
            CollectLargeLoanFailures(proposedApplication, declineReasons);
        }
        else
        {
            CollectSmallLoanFailures(proposedApplication, declineReasons);
        }

        return declineReasons.Count == 0
            ? LoanDecision.Approved()
            : LoanDecision.Declined(declineReasons);
    }

    private static void CollectAmountLimitFailures(
        PoundSterlingAmount loanAmount,
        List<EligibilityDeclineReason> declineReasons)
    {
        if (loanAmount.IsBelow(MinimumLendableAmount))
        {
            declineReasons.Add(
                EligibilityDeclineReason.LoanAmountBelowProductMinimum(loanAmount, MinimumLendableAmount));
        }

        if (loanAmount.IsAbove(MaximumLendableAmount))
        {
            declineReasons.Add(
                EligibilityDeclineReason.LoanAmountAboveProductMaximum(loanAmount, MaximumLendableAmount));
        }
    }

    private static void CollectLargeLoanFailures(
        ProposedSecuredLoan proposedApplication,
        List<EligibilityDeclineReason> declineReasons)
    {
        if (!proposedApplication.LoanToValue.IsAtMost(MaximumLoanToValueForLargeLoan))
        {
            declineReasons.Add(
                EligibilityDeclineReason.LoanToValueExceedsLargeLoanMaximum(
                    proposedApplication.LoanToValue,
                    MaximumLoanToValueForLargeLoan,
                    LargeLoanAmountThreshold));
        }

        if (!proposedApplication.ApplicantCreditScore.MeetsMinimum(MinimumCreditScoreForLargeLoan))
        {
            declineReasons.Add(
                EligibilityDeclineReason.CreditScoreBelowLargeLoanMinimum(
                    proposedApplication.ApplicantCreditScore,
                    MinimumCreditScoreForLargeLoan,
                    LargeLoanAmountThreshold));
        }
    }

    private static void CollectSmallLoanFailures(
        ProposedSecuredLoan proposedApplication,
        List<EligibilityDeclineReason> declineReasons)
    {
        foreach (var band in SmallLoanCreditBands)
        {
            if (!proposedApplication.LoanToValue.IsBelow(band.ExclusiveUpperBound))
            {
                continue;
            }

            RequireCreditScoreForBand(
                proposedApplication.ApplicantCreditScore,
                band.MinimumCreditScore,
                band.Description,
                declineReasons);
            return;
        }

        declineReasons.Add(
            EligibilityDeclineReason.LoanToValueAtOrAboveSmallLoanMaximum(
                proposedApplication.LoanToValue,
                HighLoanToValueBandUpperBound,
                LargeLoanAmountThreshold));
    }

    private static void RequireCreditScoreForBand(
        CreditScore applicantCreditScore,
        int requiredMinimum,
        string loanToValueBandDescription,
        List<EligibilityDeclineReason> declineReasons)
    {
        if (!applicantCreditScore.MeetsMinimum(requiredMinimum))
        {
            declineReasons.Add(
                EligibilityDeclineReason.CreditScoreBelowRequiredForLoanToValueBand(
                    applicantCreditScore,
                    requiredMinimum,
                    loanToValueBandDescription));
        }
    }

    private static string AsPercent(decimal ratio) => $"{ratio * 100m:0}%";

    private readonly record struct SmallLoanCreditBand(
        decimal ExclusiveUpperBound,
        int MinimumCreditScore,
        string Description);
}
