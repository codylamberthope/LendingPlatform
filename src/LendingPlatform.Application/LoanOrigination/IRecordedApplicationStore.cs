using LendingPlatform.Domain.LoanOrigination;

namespace LendingPlatform.Application.LoanOrigination;

/// <summary>
/// Port for the durable book of recorded secured-loan applications.
/// </summary>
public interface IRecordedApplicationStore
{
    IReadOnlyList<SecuredLoanApplication> GetAll();

    void Add(SecuredLoanApplication application);

    void Save();
}
