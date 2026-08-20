namespace LendingPlatform.Domain.LoanOrigination;

/// <summary>
/// The facts of a secured-loan application before it is decided and recorded.
/// Loan to value is derived once from the loan amount and security asset value.
/// </summary>
public sealed class ProposedSecuredLoan
{
    public PoundSterlingAmount LoanAmount { get; }

    public PoundSterlingAmount SecurityAssetValue { get; }

    public CreditScore ApplicantCreditScore { get; }

    public LoanToValueRatio LoanToValue { get; }

    private ProposedSecuredLoan(
        PoundSterlingAmount loanAmount,
        PoundSterlingAmount securityAssetValue,
        CreditScore applicantCreditScore,
        LoanToValueRatio loanToValue)
    {
        LoanAmount = loanAmount;
        SecurityAssetValue = securityAssetValue;
        ApplicantCreditScore = applicantCreditScore;
        LoanToValue = loanToValue;
    }

    public static ProposedSecuredLoan From(
        PoundSterlingAmount loanAmount,
        PoundSterlingAmount securityAssetValue,
        CreditScore applicantCreditScore)
    {
        return new ProposedSecuredLoan(
            loanAmount,
            securityAssetValue,
            applicantCreditScore,
            LoanToValueRatio.From(loanAmount, securityAssetValue));
    }
}
