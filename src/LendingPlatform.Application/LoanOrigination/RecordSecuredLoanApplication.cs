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

    public ApplicationBookStatistics CurrentBookStatistics() =>
        ApplicationBookStatistics.From(_recordedApplicationStore.GetAll());

    public RecordSecuredLoanApplicationResult Execute(ProposedSecuredLoan proposedApplication)
    {
        var decision = _loanEligibilityPolicy.Evaluate(proposedApplication);
        var recordedApplication = SecuredLoanApplication.Record(
            proposedApplication,
            decision,
            DateTimeOffset.UtcNow);

        _recordedApplicationStore.Append(recordedApplication);

        return new RecordSecuredLoanApplicationResult(decision, CurrentBookStatistics());
    }
}
