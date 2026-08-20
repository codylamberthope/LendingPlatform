# Architecture

## Bounded context

**LoanOrigination** is the only bounded context. It decides whether a single secured-loan application is eligible under the product rules, then records that decision in an application book.

There is no separate pricing, servicing, or identity context. Production would typically introduce a `SecuredLoanApplicationRecorded` domain event for those consumers; this exercise has no event bus.

## Layers

Dependencies point inward. The domain has no project references.

```
ConsoleApp  -->  Application  -->  Domain
     |                ^
     |                |
     +-->  Infrastructure
              |
              +--> Domain
```

| Layer | Responsibility |
|-------|----------------|
| Domain | Value objects, `SecuredLoanApplication` aggregate, `LoanEligibilityPolicy`, book statistics |
| Application | `RecordSecuredLoanApplication` use case; `IRecordedApplicationStore` port |
| Infrastructure | `JsonFileRecordedApplicationStore` — DTO mapping, load/save JSON |
| ConsoleApp | Parse user input into value objects, print decisions, compose the object graph in `Program.cs` |

Invalid strings never enter the domain. The console retries until a value object can be constructed, or the user quits.

## Tactical model

- **`PoundSterlingAmount`**, **`CreditScore`**, **`LoanToValueRatio`** — invariants in factory methods.
- **`LoanDecision`** / **`EligibilityDeclineReason`** — an approval, or a declined outcome with every failed product check.
- **`SecuredLoanApplication`** — aggregate root. Created only after the policy has produced a decision (`Record`). Reconstituted from the store without re-running eligibility (`Reconstitute`).
- **`LoanEligibilityPolicy`** — domain policy. Named product terms, exclusive LTV bands, collect-all failures.
- **`ApplicationBookStatistics`** — derived read model over the recorded book (approved/declined counts, approved principal written, unweighted mean LTV).

## Recording an application

1. Console constructs `PoundSterlingAmount` and `CreditScore`.
2. `RecordSecuredLoanApplication` asks `LoanEligibilityPolicy.Evaluate`.
3. `SecuredLoanApplication.Record` stamps the decision and current UTC time.
4. The store appends and writes `data/recorded-applications.json`.
5. Statistics are recalculated from the full book and returned to the console.

`Program.cs` is the composition root: policy, JSON store, use case, console host. There is no DI container or mediator.
