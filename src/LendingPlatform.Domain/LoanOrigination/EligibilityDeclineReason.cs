namespace LendingPlatform.Domain.LoanOrigination;

public sealed record EligibilityDeclineReason(string Code, string Explanation)
{
    public static EligibilityDeclineReason LoanAmountBelowProductMinimum(
        PoundSterlingAmount loanAmount,
        PoundSterlingAmount minimumLendableAmount)
    {
        return new(
            nameof(LoanAmountBelowProductMinimum),
            $"Loan amount {loanAmount} is below the product minimum of {minimumLendableAmount}.");
    }

    public static EligibilityDeclineReason LoanAmountAboveProductMaximum(
        PoundSterlingAmount loanAmount,
        PoundSterlingAmount maximumLendableAmount)
    {
        return new(
            nameof(LoanAmountAboveProductMaximum),
            $"Loan amount {loanAmount} is above the product maximum of {maximumLendableAmount}.");
    }

    public static EligibilityDeclineReason LoanToValueExceedsLargeLoanMaximum(
        LoanToValueRatio loanToValue,
        decimal maximumRatio,
        PoundSterlingAmount largeLoanAmountThreshold)
    {
        return new(
            nameof(LoanToValueExceedsLargeLoanMaximum),
            $"Loan to value {loanToValue} exceeds the {maximumRatio:P0} maximum for loans of {largeLoanAmountThreshold} or more.");
    }

    public static EligibilityDeclineReason CreditScoreBelowLargeLoanMinimum(
        CreditScore applicantCreditScore,
        int requiredMinimum,
        PoundSterlingAmount largeLoanAmountThreshold)
    {
        return new(
            nameof(CreditScoreBelowLargeLoanMinimum),
            $"Credit score {applicantCreditScore} is below the minimum of {requiredMinimum} for loans of {largeLoanAmountThreshold} or more.");
    }

    public static EligibilityDeclineReason LoanToValueAtOrAboveSmallLoanMaximum(
        LoanToValueRatio loanToValue,
        decimal maximumRatio,
        PoundSterlingAmount largeLoanAmountThreshold)
    {
        return new(
            nameof(LoanToValueAtOrAboveSmallLoanMaximum),
            $"Loan to value {loanToValue} is at or above the {maximumRatio:P0} maximum for loans under {largeLoanAmountThreshold}.");
    }

    public static EligibilityDeclineReason CreditScoreBelowRequiredForLoanToValueBand(
        CreditScore applicantCreditScore,
        int requiredMinimum,
        string loanToValueBandDescription)
    {
        return new(
            nameof(CreditScoreBelowRequiredForLoanToValueBand),
            $"Credit score {applicantCreditScore} is below the minimum of {requiredMinimum} for {loanToValueBandDescription}.");
    }
}
