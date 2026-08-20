using System.Globalization;

namespace LendingPlatform.Domain.LoanOrigination;

/// <summary>
/// Loan amount divided by the security asset value. 0.60 is 60% LTV.
/// </summary>
public readonly record struct LoanToValueRatio
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

    public static LoanToValueRatio FromRecordedRatio(decimal ratio) => new(ratio);

    public bool IsBelow(decimal threshold) => Ratio < threshold;

    public bool IsAtLeast(decimal threshold) => Ratio >= threshold;

    public bool IsAtMost(decimal threshold) => Ratio <= threshold;

    public override string ToString() => Ratio.ToString("P2", CultureInfo.GetCultureInfo("en-GB"));
}
