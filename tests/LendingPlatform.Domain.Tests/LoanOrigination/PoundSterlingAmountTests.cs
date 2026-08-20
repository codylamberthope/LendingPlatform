using LendingPlatform.Domain.LoanOrigination;

namespace LendingPlatform.Domain.Tests.LoanOrigination;

public sealed class PoundSterlingAmountTests
{
    [Fact]
    public void TryCreate_rejects_zero_and_negative_amounts()
    {
        Assert.False(PoundSterlingAmount.TryCreate(0m, out _, out var zeroError));
        Assert.Equal("Amount must be greater than zero.", zeroError);

        Assert.False(PoundSterlingAmount.TryCreate(-1m, out _, out var negativeError));
        Assert.Equal("Amount must be greater than zero.", negativeError);
    }

    [Fact]
    public void FromPounds_accepts_a_positive_amount()
    {
        var amount = PoundSterlingAmount.FromPounds(100_000.01m);

        Assert.Equal(100_000.01m, amount.Pounds);
        Assert.True(amount.IsAtLeast(PoundSterlingAmount.FromPounds(100_000m)));
        Assert.True(amount.IsAbove(PoundSterlingAmount.FromPounds(100_000m)));
        Assert.True(PoundSterlingAmount.FromPounds(99_999.99m).IsBelow(amount));
    }

    [Fact]
    public void Sum_of_no_amounts_is_zero()
    {
        Assert.Equal(PoundSterlingAmount.Zero, PoundSterlingAmount.Sum(Array.Empty<PoundSterlingAmount>()));
    }
}
