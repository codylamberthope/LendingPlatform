using LendingPlatform.Domain.LoanOrigination;

namespace LendingPlatform.Domain.Tests.LoanOrigination;

public sealed class ApplicationBookStatisticsTests
{
    [Fact]
    public void Empty_book_has_zero_counts_zero_written_and_no_mean_ltv()
    {
        var statistics = ApplicationBookStatistics.From(Array.Empty<SecuredLoanApplication>());

        Assert.Equal(0, statistics.ApprovedApplicantCount);
        Assert.Equal(0, statistics.DeclinedApplicantCount);
        Assert.Equal(PoundSterlingAmount.Zero, statistics.TotalValueOfLoansWritten);
        Assert.Null(statistics.MeanLoanToValueAcrossAllApplications);
    }

    [Fact]
    public void Written_value_sums_approved_principal_only_and_mean_ltv_includes_declines()
    {
        var approved = Record(200_000m, 400_000m, approved: true);
        var declined = Record(300_000m, 300_000m, approved: false);

        var statistics = ApplicationBookStatistics.From(new[] { approved, declined });

        Assert.Equal(1, statistics.ApprovedApplicantCount);
        Assert.Equal(1, statistics.DeclinedApplicantCount);
        Assert.Equal(200_000m, statistics.TotalValueOfLoansWritten.Pounds);
        Assert.Equal(0.75m, statistics.MeanLoanToValueAcrossAllApplications);
    }

    private static SecuredLoanApplication Record(decimal loanPounds, decimal assetPounds, bool approved)
    {
        var loan = PoundSterlingAmount.FromPounds(loanPounds);
        var asset = PoundSterlingAmount.FromPounds(assetPounds);
        var decision = approved
            ? LoanDecision.Approved()
            : LoanDecision.Declined(
                new[]
                {
                    EligibilityDeclineReason.LoanToValueAtOrAboveSmallLoanMaximum(
                        LoanToValueRatio.From(loan, asset),
                        0.90m)
                });

        return SecuredLoanApplication.Record(
            loan,
            asset,
            CreditScore.From(750),
            decision,
            DateTimeOffset.UtcNow);
    }
}
