using LendingPlatform.Domain.LoanOrigination;

namespace LendingPlatform.Application.LoanOrigination;

/// <summary>
/// Evaluates eligibility, records the application in the book, and returns the updated statistics.
/// </summary>
public sealed class RecordSecuredLoanApplication
{
    private readonly LoanEligibilityPolicy _loanEligibilityPolicy;
    private readonly IRecordedApplicationStore _recordedApplicationStore;

    public RecordSecuredLoanApplication(
        LoanEligibilityPolicy loanEligibilityPolicy,
        IRecordedApplicationStore recordedApplicationStore)
    {
        _loanEligibilityPolicy = loanEligibilityPolicy;
        _recordedApplicationStore = recordedApplicationStore;
    }

    public RecordSecuredLoanApplicationResult Execute(
        PoundSterlingAmount loanAmount,
        PoundSterlingAmount securityAssetValue,
        CreditScore applicantCreditScore)
    {
        var decision = _loanEligibilityPolicy.Evaluate(loanAmount, securityAssetValue, applicantCreditScore);
        var recordedApplication = SecuredLoanApplication.Record(
            loanAmount,
            securityAssetValue,
            applicantCreditScore,
            decision,
            DateTimeOffset.UtcNow);

        _recordedApplicationStore.Add(recordedApplication);
        _recordedApplicationStore.Save();

        return new RecordSecuredLoanApplicationResult(
            decision,
            ApplicationBookStatistics.From(_recordedApplicationStore.GetAll()));
    }
}
