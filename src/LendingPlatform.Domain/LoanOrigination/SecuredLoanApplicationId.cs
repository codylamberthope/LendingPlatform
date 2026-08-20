namespace LendingPlatform.Domain.LoanOrigination;

public readonly record struct SecuredLoanApplicationId(Guid Value)
{
    public static SecuredLoanApplicationId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}
