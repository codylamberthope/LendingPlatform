# AI log

Key prompts, iterations, and corrections used while building this solution.

## Prompts

1. Convert `Blackfinch Engineering Candidate Tests - Backend 3.pdf` to markdown for analysis.
2. Analyse the requirements for shortfalls in the business rules and suggest how to fix them.
3. Create a plan to implement the task. Adhere to best practices and strict domain-driven guidelines. Naming must explain what the object or process actually does. Ask as many clarifying questions as possible.
4. Implement the attached DDD secured-lending console plan. Do not edit the plan file. Complete every to-do.

## Notable analysis (prompt 2)

The brief’s sub-£1m LTV clauses overlap (`LTV < 60%`, `< 80%`, `< 90%`). A 50% LTV application matches all three. If those predicates are AND-ed, every loan under 90% LTV needs credit ≥ 900 and the 750 / 800 floors never approve anyone.

Other gaps called out: LTV undefined when asset value ≤ 0; “loans written” vs all applications; mean LTV population; invalid input vs business decline; rounding at band edges; empty-set mean; the £1 million product cliff; applicants vs applications.

**Correction applied in code:** exclusive first-match bands, not conjunction of overlapping clauses. Test `Overlap_trap_fifty_percent_ltv_with_score_760_is_approved_on_the_small_loan_ladder` guards that reading.

## Planning answers (prompt 3)

Locked by the author before implementation:

- C# / .NET 10, four layers plus domain tests
- Exclusive if/else LTV bands
- Invalid input retries and is not counted
- Written value = approved principal only
- Mean LTV = unweighted mean of every recorded application; n/a if none
- Interactive loop; JSON file persistence
- Collect every applicable decline reason
- `decimal` money; raw LTV ratio (no extra rounding)
- Intention-revealing names (`LoanEligibilityPolicy`, `RecordSecuredLoanApplication`, `ApplicationBookStatistics`)
- README, AI log, assumptions, architecture docs

## Iterations while implementing (prompt 4)

- .NET 10 `dotnet new sln` produced `LendingPlatform.slnx` rather than a classic `.sln`. Kept the SDK default.
- Infrastructure needed a direct project reference to Domain as well as Application, so JSON reconstitution can call `SecuredLoanApplication.Reconstitute` (SDK project references are not compile-time transitive).
- `PoundSterlingAmount` forbids `<= 0` for user-entered money, but book totals can be £0 when nothing has been approved. `Zero` is a dedicated instance for that statistic, not creatable through `TryCreate`.
- Large-loan applications that fail several rules append amount, LTV, and credit reasons in that order. Small-loan LTV ≥ 90% records only the LTV maximum reason, because no credit floor would approve it.
- Decline reasons persist as code + explanation DTOs so a restart reprints the same wording without re-running the policy.

## Output questioned

- Whether amount-out-of-range applications should still receive LTV/credit reasons: yes, per “collect all reasons”.
- Whether reconstituting from JSON should trust the stored LTV field: LTV is recomputed from loan and asset on load so the aggregate cannot drift from its amounts.
