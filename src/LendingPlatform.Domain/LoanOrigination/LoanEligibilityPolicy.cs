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

    public LoanDecision Evaluate(
        PoundSterlingAmount loanAmount,
        PoundSterlingAmount securityAssetValue,
        CreditScore applicantCreditScore)
    {
        var declineReasons = new List<EligibilityDeclineReason>();
        var loanToValue = LoanToValueRatio.From(loanAmount, securityAssetValue);

        CollectAmountLimitFailures(loanAmount, declineReasons);

        if (loanAmount.IsAtLeast(LargeLoanAmountThreshold))
        {
            CollectLargeLoanFailures(loanToValue, applicantCreditScore, declineReasons);
        }
        else
        {
            CollectSmallLoanFailures(loanToValue, applicantCreditScore, declineReasons);
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
        LoanToValueRatio loanToValue,
        CreditScore applicantCreditScore,
        List<EligibilityDeclineReason> declineReasons)
    {
        if (!loanToValue.IsAtMost(MaximumLoanToValueForLargeLoan))
        {
            declineReasons.Add(
                EligibilityDeclineReason.LoanToValueExceedsLargeLoanMaximum(
                    loanToValue,
                    MaximumLoanToValueForLargeLoan));
        }

        if (!applicantCreditScore.MeetsMinimum(MinimumCreditScoreForLargeLoan))
        {
            declineReasons.Add(
                EligibilityDeclineReason.CreditScoreBelowLargeLoanMinimum(
                    applicantCreditScore,
                    MinimumCreditScoreForLargeLoan));
        }
    }

    private static void CollectSmallLoanFailures(
        LoanToValueRatio loanToValue,
        CreditScore applicantCreditScore,
        List<EligibilityDeclineReason> declineReasons)
    {
        if (loanToValue.IsBelow(LowerLoanToValueBandUpperBound))
        {
            RequireCreditScoreForBand(
                applicantCreditScore,
                MinimumCreditScoreForLowerLoanToValueBand,
                "loan to value below 60%",
                declineReasons);
            return;
        }

        if (loanToValue.IsBelow(MidLoanToValueBandUpperBound))
        {
            RequireCreditScoreForBand(
                applicantCreditScore,
                MinimumCreditScoreForMidLoanToValueBand,
                "loan to value of at least 60% and below 80%",
                declineReasons);
            return;
        }

        if (loanToValue.IsBelow(HighLoanToValueBandUpperBound))
        {
            RequireCreditScoreForBand(
                applicantCreditScore,
                MinimumCreditScoreForHighLoanToValueBand,
                "loan to value of at least 80% and below 90%",
                declineReasons);
            return;
        }

        declineReasons.Add(
            EligibilityDeclineReason.LoanToValueAtOrAboveSmallLoanMaximum(
                loanToValue,
                HighLoanToValueBandUpperBound));
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
}
