namespace LendingPlatform.Infrastructure.LoanOrigination;

internal sealed class RecordedApplicationFileDto
{
    public Guid Id { get; set; }

    public decimal LoanAmountPounds { get; set; }

    public decimal SecurityAssetValuePounds { get; set; }

    public int ApplicantCreditScore { get; set; }

    public decimal EvaluatedLoanToValueRatio { get; set; }

    public bool IsApproved { get; set; }

    public List<DeclineReasonFileDto> DeclineReasons { get; set; } = new();

    public DateTimeOffset RecordedAt { get; set; }
}

internal sealed class DeclineReasonFileDto
{
    public string Code { get; set; } = string.Empty;

    public string Explanation { get; set; } = string.Empty;
}
