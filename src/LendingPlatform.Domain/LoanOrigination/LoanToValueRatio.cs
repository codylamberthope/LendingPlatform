using System.Globalization;

namespace LendingPlatform.Domain.LoanOrigination;

/// <summary>
/// Loan amount divided by the security asset value. 0.60 is 60% LTV.
/// </summary>
public sealed class LoanToValueRatio : IEquatable<LoanToValueRatio>
{
    public decimal Ratio { get; }

    private LoanToValueRatio(decimal ratio)
    {
        Ratio = ratio;
    }

    public static LoanToValueRatio From(PoundSterlingAmount loanAmount, PoundSterlingAmount securityAssetValue)
    {
        return new LoanToValueRatio(loanAmount.Pounds / securityAssetValue.Pounds);
    }

    public bool IsBelow(decimal threshold) => Ratio < threshold;

    public bool IsAtLeast(decimal threshold) => Ratio >= threshold;

    public bool IsAtMost(decimal threshold) => Ratio <= threshold;

    public bool Equals(LoanToValueRatio? other) => other is not null && Ratio == other.Ratio;

    public override bool Equals(object? obj) => obj is LoanToValueRatio other && Equals(other);

    public override int GetHashCode() => Ratio.GetHashCode();

    public override string ToString() => Ratio.ToString("P2", CultureInfo.GetCultureInfo("en-GB"));
}
