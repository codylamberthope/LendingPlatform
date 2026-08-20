using LendingPlatform.Domain.LoanOrigination;

namespace LendingPlatform.Application.LoanOrigination;

public sealed record RecordSecuredLoanApplicationResult(
    LoanDecision Decision,
    ApplicationBookStatistics BookStatistics);
