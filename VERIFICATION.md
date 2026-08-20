# Manual verification

Enter these at the three prompts: **loan amount**, **asset value**, **credit score**. Start from a clean book if you want totals to match (`delete data/recorded-applications.json` first).

```bash
dotnet run --project src/LendingPlatform.ConsoleApp
```

## Validation (re-prompt, not recorded)

| Prompt | Input | Expect |
|---|---|---|
| Any | blank | `Please enter a value.` |
| Loan or asset | `abc` | numeric amount error |
| Loan or asset | `0` or `-1` | `Amount must be greater than zero.` |
| Credit | `0` or `1000` | credit score 1–999 error |
| Any | `q` / `quit` / `exit` | process stops |

## Product amount limits

| Loan | Asset | Score | LTV | Expect |
|---|---|---|---|---|
| `100000` | `200000` | `750` | 50% | **Successful** (inclusive minimum) |
| `99999.99` | `199999.98` | `750` | 50% | **Declined** — below product minimum only |
| `1500000` | `2500000` | `950` | 60% | **Successful** (inclusive maximum, large-loan ladder) |
| `1500000.01` | `2500000.02` | `950` | 60% | **Declined** — above product maximum only |

## Small loan (under £1m) — exclusive LTV bands

| Loan | Asset | Score | Band | Expect |
|---|---|---|---|---|
| `200000` | `400000` | `760` | 50% | **Successful** (overlap trap: 750 floor is reachable) |
| `599999` | `1000000` | `750` | just under 60% | **Successful** |
| `599999` | `1000000` | `749` | just under 60% | **Declined** — credit too low for below-60% band |
| `600000` | `1000000` | `799` | exactly 60% | **Declined** — needs 800 (not 750) |
| `600000` | `1000000` | `800` | exactly 60% | **Successful** |
| `799999` | `1000000` | `800` | just under 80% | **Successful** |
| `800000` | `1000000` | `899` | exactly 80% | **Declined** — needs 900 |
| `800000` | `1000000` | `900` | exactly 80% | **Successful** |
| `899999` | `1000000` | `900` | just under 90% | **Successful** |
| `900000` | `1000000` | `999` | exactly 90% | **Declined** — LTV only, credit is ignored |
| `250000` | `250000` | `400` | 100% | **Declined** — LTV only (one reason) |

## Large loan (£1m+) and the £1 cliff

| Loan | Asset | Score | LTV | Expect |
|---|---|---|---|---|
| `999999` | `1999998` | `750` | 50% | **Successful** (still small-loan ladder) |
| `1000000` | `2000000` | `750` | 50% | **Declined** — credit below 950 only |
| `1200000` | `2000000` | `950` | 60% | **Successful** (60% is allowed on large loans) |
| `1200000` | `2000000` | `949` | 60% | **Declined** — credit below 950 |
| `1200001` | `2000000` | `950` | just over 60% | **Declined** — LTV above large-loan maximum |
| `2000000` | `2000000` | `400` | 100% | **Declined** — three reasons: amount, LTV, credit |

## Book totals (run in this order on an empty book)

1. `200000` / `400000` / `760` → Successful. Written **£200,000.00**, mean LTV **50.00%**, approved 1 / declined 0.
2. `300000` / `300000` / `400` → Declined. Written still **£200,000.00** (approved principal only), mean LTV **75.00%** (includes the decline), approved 1 / declined 1.

Restart the app after that: the same totals should load from `data/recorded-applications.json`.
