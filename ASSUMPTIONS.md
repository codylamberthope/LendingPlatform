# Assumptions

Interpretations used where the brief is silent or overlapping. These are also the production notes a 1-hour submission would call out.

## Eligibility

- Sub-£1 million LTV rules are **exclusive bands**, evaluated in listed order (first match wins):
  - LTV &lt; 60% → credit ≥ 750
  - 60% ≤ LTV &lt; 80% → credit ≥ 800
  - 80% ≤ LTV &lt; 90% → credit ≥ 900
  - LTV ≥ 90% → decline (LTV only; credit is not also failed)
- AND-ing the overlapping `&lt; 60% / &lt; 80% / &lt; 90%` clauses was rejected: that reading makes the 750 and 800 floors unreachable.
- Product amount limits are inclusive: £100,000 is eligible; £1,500,000 is eligible and uses the large-loan ladder. £99,999.99 and £1,500,000.01 fail the amount limits.
- £1,000,000 uses the large-loan ladder (LTV ≤ 60% and credit ≥ 950). £999,999 uses the small-loan ladder. That £1 cliff is treated as specified, not smoothed.
- Large-loan LTV is **at most** 60% (60% is allowed). Small-loan “below 60%” is a strict less-than, so exactly 60% on a small loan is the 800-score band.
- Every applicable rule is evaluated. An amount outside the product range still runs the LTV/credit ladder for that amount’s tier, so the applicant can see multiple reasons.

## Input versus decline

- Zero or negative money, non-numeric text, blank fields, and credit scores outside 1–999 are **validation errors**. The field is re-prompted. They are not declines and are excluded from the book.
- Loan amount and asset value must be greater than zero so LTV is defined (no division by zero).

## Money and LTV

- Money is `decimal` pounds. There is no forced rounding to pence.
- LTV is the raw ratio `loan / asset` and is compared to `0.60`, `0.80`, and `0.90` with no extra rounding.

## Book metrics

- Each valid submission is one application. There is no applicant identity, so counts are not unique people.
- **Total value of loans written** is the sum of loan amounts for **approved** applications only.
- **Mean LTV** is the unweighted arithmetic mean of each recorded application’s LTV ratio, including declines. It is `n/a` when the book is empty.
- The book is the JSON file at `data/recorded-applications.json`. It is loaded on startup and saved after every recorded application. A corrupt file fails the process; it is not wiped.

## Session

- The console loops until `quit`, `exit`, or `q` (case-insensitive) at any prompt.
- No interest rate, term, fees, affordability, or asset type. Those belong in a production origination platform.
