using LendingPlatform.Domain.LoanOrigination;

namespace LendingPlatform.Application.LoanOrigination;

public sealed class RecordSecuredLoanApplicationResult
{
    public RecordSecuredLoanApplicationResult(LoanDecision decision, ApplicationBookStatistics bookStatistics)
    {
        Decision = decision;
        BookStatistics = bookStatistics;
    }

    public LoanDecision Decision { get; }

    public ApplicationBookStatistics BookStatistics { get; }
}
