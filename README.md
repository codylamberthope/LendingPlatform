# Secured lending console

Console application that decides whether a secured loan application is successful, then keeps a running book of applications. Built as a layered DDD solution for the Blackfinch backend engineering exercise.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)

## Run

From the repository root:

```bash
dotnet run --project src/LendingPlatform.ConsoleApp
```

The host prompts for:

1. Loan amount in GBP
2. Asset value the loan is secured against in GBP
3. Applicant credit score (1–999)

After each valid application it prints the decision (and every decline reason, when declined) plus running book totals. Enter `quit`, `exit`, or `q` at any prompt to stop.

Invalid values (blank, non-numeric, zero/negative money, credit score outside 1–999) are rejected and that field is prompted again. They are not recorded and are not counted.

## Persistence

Recorded applications are stored at `data/recorded-applications.json` relative to the working directory (normally the repo root when using the command above). Totals survive restarts. If the file exists but cannot be read or reconstituted, the process exits with an error and does not overwrite the book.

## Tests

```bash
dotnet test
```

Domain unit tests cover value-object invariants, exclusive LTV bands, product amount endpoints, the £1 million rule cliff, the overlapping-clause trap, collect-all decline reasons, and application-book statistics.

## Solution layout

| Project | Role |
|---------|------|
| `src/LendingPlatform.Domain` | Loan origination model and eligibility policy |
| `src/LendingPlatform.Application` | Record-application use case and store port |
| `src/LendingPlatform.Infrastructure` | JSON file store |
| `src/LendingPlatform.ConsoleApp` | Prompts, parsing, composition root |
| `tests/LendingPlatform.Domain.Tests` | xUnit tests for the domain |

See [ARCHITECTURE.md](ARCHITECTURE.md) and [ASSUMPTIONS.md](ASSUMPTIONS.md) for design and rule interpretations. [AI-LOG.md](AI-LOG.md) records the prompts and iterations used to produce this solution.
