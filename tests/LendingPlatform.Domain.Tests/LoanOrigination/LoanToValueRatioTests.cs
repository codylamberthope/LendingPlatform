using LendingPlatform.Domain.LoanOrigination;

namespace LendingPlatform.Domain.Tests.LoanOrigination;

public sealed class LoanToValueRatioTests
{
    [Fact]
    public void From_divides_loan_amount_by_asset_value_without_rounding()
    {
        var ratio = LoanToValueRatio.From(
            PoundSterlingAmount.FromPounds(200_000m),
            PoundSterlingAmount.FromPounds(400_000m));

        Assert.Equal(0.5m, ratio.Ratio);
        Assert.True(ratio.IsBelow(0.60m));
        Assert.True(ratio.IsAtMost(0.50m));
        Assert.True(ratio.IsAtLeast(0.50m));
        Assert.False(ratio.IsAtLeast(0.60m));
    }

    [Fact]
    public void From_keeps_a_repeating_decimal_ratio()
    {
        var ratio = LoanToValueRatio.From(
            PoundSterlingAmount.FromPounds(100_000m),
            PoundSterlingAmount.FromPounds(300_000m));

        Assert.Equal(100_000m / 300_000m, ratio.Ratio);
    }

    [Fact]
    public void FromRecordedRatio_preserves_the_stored_ratio_without_recomputing()
    {
        var ratio = LoanToValueRatio.FromRecordedRatio(0.42m);

        Assert.Equal(0.42m, ratio.Ratio);
    }
}
