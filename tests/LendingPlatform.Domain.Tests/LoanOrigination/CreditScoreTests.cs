using LendingPlatform.Domain.LoanOrigination;

namespace LendingPlatform.Domain.Tests.LoanOrigination;

public sealed class CreditScoreTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(1000)]
    [InlineData(-5)]
    public void TryCreate_rejects_scores_outside_one_to_nine_hundred_and_ninety_nine(int value)
    {
        Assert.False(CreditScore.TryCreate(value, out var score, out var error));
        Assert.Null(score);
        Assert.Contains("1 to 999", error, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(750)]
    [InlineData(999)]
    public void From_accepts_boundary_and_typical_scores(int value)
    {
        var score = CreditScore.From(value);

        Assert.Equal(value, score.Value);
        Assert.True(score.MeetsMinimum(value));
        Assert.False(score.MeetsMinimum(value + 1));
    }
}
