using LendingPlatform.Domain.LoanOrigination;

namespace LendingPlatform.Domain.Tests.LoanOrigination;

public sealed class LoanEligibilityPolicyTests
{
    private readonly LoanEligibilityPolicy _policy = new();

    [Fact]
    public void Product_minimum_is_inclusive_at_one_hundred_thousand_pounds()
    {
        var decision = Evaluate(100_000m, 200_000m, 750);

        Assert.True(decision.IsApproved);
        Assert.Empty(decision.DeclineReasons);
    }

    [Fact]
    public void Loan_just_below_product_minimum_is_declined_for_amount_only_when_other_rules_pass()
    {
        var decision = Evaluate(99_999.99m, 199_999.98m, 750);

        Assert.False(decision.IsApproved);
        Assert.Single(decision.DeclineReasons);
        Assert.Equal(nameof(EligibilityDeclineReason.LoanAmountBelowProductMinimum), decision.DeclineReasons[0].Code);
    }

    [Fact]
    public void Product_maximum_is_inclusive_at_one_point_five_million_and_uses_the_large_loan_ladder()
    {
        var decision = Evaluate(1_500_000m, 2_500_000m, 950);

        Assert.True(decision.IsApproved);
    }

    [Fact]
    public void Loan_just_above_product_maximum_fails_the_amount_limit()
    {
        var decision = Evaluate(1_500_000.01m, 2_500_000.02m, 950);

        Assert.False(decision.IsApproved);
        Assert.Contains(
            decision.DeclineReasons,
            reason => reason.Code == nameof(EligibilityDeclineReason.LoanAmountAboveProductMaximum));
        Assert.DoesNotContain(
            decision.DeclineReasons,
            reason => reason.Code == nameof(EligibilityDeclineReason.LoanToValueExceedsLargeLoanMaximum));
        Assert.DoesNotContain(
            decision.DeclineReasons,
            reason => reason.Code == nameof(EligibilityDeclineReason.CreditScoreBelowLargeLoanMinimum));
    }

    [Fact]
    public void Overlap_trap_fifty_percent_ltv_with_score_760_is_approved_on_the_small_loan_ladder()
    {
        var decision = Evaluate(200_000m, 400_000m, 760);

        Assert.True(decision.IsApproved);
    }

    [Theory]
    [MemberData(nameof(ExclusiveLtvBandCases))]
    public void Exclusive_ltv_bands_use_first_matching_threshold(
        decimal loanPounds,
        decimal assetPounds,
        int creditScore,
        bool expectedApproval)
    {
        var decision = Evaluate(loanPounds, assetPounds, creditScore);

        Assert.Equal(expectedApproval, decision.IsApproved);
        if (!expectedApproval && loanPounds / assetPounds >= 0.90m)
        {
            Assert.Contains(
                decision.DeclineReasons,
                reason => reason.Code == nameof(EligibilityDeclineReason.LoanToValueAtOrAboveSmallLoanMaximum));
        }
    }

    public static IEnumerable<object[]> ExclusiveLtvBandCases()
    {
        yield return new object[] { 599_999m, 1_000_000m, 750, true };
        yield return new object[] { 600_000m, 1_000_000m, 799, false };
        yield return new object[] { 600_000m, 1_000_000m, 800, true };
        yield return new object[] { 799_999m, 1_000_000m, 800, true };
        yield return new object[] { 800_000m, 1_000_000m, 899, false };
        yield return new object[] { 800_000m, 1_000_000m, 900, true };
        yield return new object[] { 899_999m, 1_000_000m, 900, true };
        yield return new object[] { 900_000m, 1_000_000m, 999, false };
    }

    [Fact]
    public void Exactly_sixty_percent_ltv_on_a_small_loan_requires_credit_of_800()
    {
        var tooLow = Evaluate(600_000m, 1_000_000m, 799);
        var enough = Evaluate(600_000m, 1_000_000m, 800);

        Assert.False(tooLow.IsApproved);
        Assert.Equal(
            nameof(EligibilityDeclineReason.CreditScoreBelowRequiredForLoanToValueBand),
            tooLow.DeclineReasons[0].Code);
        Assert.True(enough.IsApproved);
    }

    [Fact]
    public void Exactly_sixty_percent_ltv_on_a_large_loan_is_allowed_when_credit_is_950()
    {
        var decision = Evaluate(1_200_000m, 2_000_000m, 950);

        Assert.True(decision.IsApproved);
    }

    [Fact]
    public void One_pound_cliff_at_one_million_switches_to_the_large_loan_credit_floor()
    {
        var justBelow = Evaluate(999_999m, 1_999_998m, 750);
        var atThreshold = Evaluate(1_000_000m, 2_000_000m, 750);

        Assert.True(justBelow.IsApproved);
        Assert.False(atThreshold.IsApproved);
        Assert.Single(atThreshold.DeclineReasons);
        Assert.Equal(
            nameof(EligibilityDeclineReason.CreditScoreBelowLargeLoanMinimum),
            atThreshold.DeclineReasons[0].Code);
    }

    [Fact]
    public void Out_of_range_large_loan_collects_amount_ltv_and_credit_failures_together()
    {
        var decision = Evaluate(2_000_000m, 2_000_000m, 400);

        Assert.False(decision.IsApproved);
        Assert.Equal(3, decision.DeclineReasons.Count);
        Assert.Equal(nameof(EligibilityDeclineReason.LoanAmountAboveProductMaximum), decision.DeclineReasons[0].Code);
        Assert.Equal(nameof(EligibilityDeclineReason.LoanToValueExceedsLargeLoanMaximum), decision.DeclineReasons[1].Code);
        Assert.Equal(nameof(EligibilityDeclineReason.CreditScoreBelowLargeLoanMinimum), decision.DeclineReasons[2].Code);
    }

    [Fact]
    public void Small_loan_at_or_above_ninety_percent_ltv_does_not_also_require_a_credit_score()
    {
        var decision = Evaluate(250_000m, 250_000m, 400);

        Assert.False(decision.IsApproved);
        Assert.Single(decision.DeclineReasons);
        Assert.Equal(
            nameof(EligibilityDeclineReason.LoanToValueAtOrAboveSmallLoanMaximum),
            decision.DeclineReasons[0].Code);
    }

    private LoanDecision Evaluate(decimal loanPounds, decimal assetPounds, int creditScore)
    {
        return _policy.Evaluate(
            PoundSterlingAmount.FromPounds(loanPounds),
            PoundSterlingAmount.FromPounds(assetPounds),
            CreditScore.From(creditScore));
    }
}
