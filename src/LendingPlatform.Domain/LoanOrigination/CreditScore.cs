namespace LendingPlatform.Domain.LoanOrigination;

public readonly record struct CreditScore
{
    public const int MinimumInclusive = 1;
    public const int MaximumInclusive = 999;

    public int Value { get; }

    private CreditScore(int value)
    {
        Value = value;
    }

    public static bool TryCreate(int value, out CreditScore score, out string error)
    {
        if (value < MinimumInclusive || value > MaximumInclusive)
        {
            score = default;
            error = $"Credit score must be an integer from {MinimumInclusive} to {MaximumInclusive}.";
            return false;
        }

        score = new CreditScore(value);
        error = string.Empty;
        return true;
    }

    public static CreditScore From(int value)
    {
        if (!TryCreate(value, out var score, out var error))
        {
            throw new ArgumentOutOfRangeException(nameof(value), value, error);
        }

        return score;
    }

    public bool MeetsMinimum(int requiredMinimum) => Value >= requiredMinimum;

    public override string ToString() => Value.ToString();
}
