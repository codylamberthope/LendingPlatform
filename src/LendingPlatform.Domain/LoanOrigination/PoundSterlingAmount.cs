using System.Globalization;

namespace LendingPlatform.Domain.LoanOrigination;

/// <summary>
/// A positive amount of pound sterling. Zero is reserved for empty statistical totals.
/// </summary>
public readonly record struct PoundSterlingAmount
{
    private static readonly CultureInfo UnitedKingdom = CultureInfo.GetCultureInfo("en-GB");

    public static PoundSterlingAmount Zero { get; } = new(0m);

    public decimal Pounds { get; }

    private PoundSterlingAmount(decimal pounds)
    {
        Pounds = pounds;
    }

    public static bool TryCreate(decimal pounds, out PoundSterlingAmount amount, out string error)
    {
        if (pounds <= 0m)
        {
            amount = default;
            error = "Amount must be greater than zero.";
            return false;
        }

        amount = new PoundSterlingAmount(pounds);
        error = string.Empty;
        return true;
    }

    public static PoundSterlingAmount FromPounds(decimal pounds)
    {
        if (!TryCreate(pounds, out var amount, out var error))
        {
            throw new ArgumentOutOfRangeException(nameof(pounds), pounds, error);
        }

        return amount;
    }

    public bool IsBelow(PoundSterlingAmount other) => Pounds < other.Pounds;

    public bool IsAbove(PoundSterlingAmount other) => Pounds > other.Pounds;

    public bool IsAtLeast(PoundSterlingAmount other) => Pounds >= other.Pounds;

    public static PoundSterlingAmount Sum(IEnumerable<PoundSterlingAmount> amounts)
    {
        var total = 0m;
        foreach (var amount in amounts)
        {
            total += amount.Pounds;
        }

        return total == 0m ? Zero : new PoundSterlingAmount(total);
    }

    public override string ToString() => Pounds.ToString("C", UnitedKingdom);
}
