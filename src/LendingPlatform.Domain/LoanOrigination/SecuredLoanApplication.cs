namespace LendingPlatform.Domain.LoanOrigination;

/// <summary>
/// A recorded application for a loan secured against an asset. Eligibility has already been decided.
/// </summary>
public sealed class SecuredLoanApplication
{
    public SecuredLoanApplicationId Id { get; }

    public PoundSterlingAmount LoanAmount { get; }

    public PoundSterlingAmount SecurityAssetValue { get; }

    public CreditScore ApplicantCreditScore { get; }

    public LoanToValueRatio EvaluatedLoanToValue { get; }

    public LoanDecision Decision { get; }

    public DateTimeOffset RecordedAt { get; }

    private SecuredLoanApplication(
        SecuredLoanApplicationId id,
        PoundSterlingAmount loanAmount,
        PoundSterlingAmount securityAssetValue,
        CreditScore applicantCreditScore,
        LoanToValueRatio evaluatedLoanToValue,
        LoanDecision decision,
        DateTimeOffset recordedAt)
    {
        Id = id;
        LoanAmount = loanAmount;
        SecurityAssetValue = securityAssetValue;
        ApplicantCreditScore = applicantCreditScore;
        EvaluatedLoanToValue = evaluatedLoanToValue;
        Decision = decision;
        RecordedAt = recordedAt;
    }

    public static SecuredLoanApplication Record(
        PoundSterlingAmount loanAmount,
        PoundSterlingAmount securityAssetValue,
        CreditScore applicantCreditScore,
        LoanDecision decision,
        DateTimeOffset recordedAt)
    {
        return new SecuredLoanApplication(
            SecuredLoanApplicationId.New(),
            loanAmount,
            securityAssetValue,
            applicantCreditScore,
            LoanToValueRatio.From(loanAmount, securityAssetValue),
            decision,
            recordedAt);
    }

    public static SecuredLoanApplication Reconstitute(
        SecuredLoanApplicationId id,
        PoundSterlingAmount loanAmount,
        PoundSterlingAmount securityAssetValue,
        CreditScore applicantCreditScore,
        LoanDecision decision,
        DateTimeOffset recordedAt)
    {
        return new SecuredLoanApplication(
            id,
            loanAmount,
            securityAssetValue,
            applicantCreditScore,
            LoanToValueRatio.From(loanAmount, securityAssetValue),
            decision,
            recordedAt);
    }
}
