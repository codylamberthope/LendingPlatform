# Engineering Candidate Technical Tests — Backend

**July 2025**

---

## Overview

Build a console application (preferably in C#) that simulates a basic Lending Platform. This task is designed to evaluate your:

- problem-solving approach
- use of clean code practices
- ability to model domain logic
- how effectively you work with AI tools

Use AI assistance (ChatGPT, Claude, Copilot, etc.) as part of your process. As part of your submission, include a brief **AI log**: the key prompts you used and any notable iterations or corrections. Copy/paste is fine.

The test is **timeboxed to 1 hour** and is not expected to be production-ready. Focus on demonstrating your approach, and feel free to include notes about any assumptions, improvements, or considerations you'd make for a production version.

---

## Requirements

### Inputs (User)

- Loan amount (in GBP)
- Asset value the loan is secured against
- Applicant's credit score (1–999)

### Outputs (System)

- **Loan decision:** Whether the applicant is successful or declined
- Total number of applicants (grouped by success status)
- Total value of loans written to date
- Mean average Loan to Value (LTV) across all applications

> **Loan to Value (LTV)** is the loan amount expressed as a percentage of the asset's value it is secured against.

---

## Business Rules

### General Limits

- Decline if loan is **< £100,000** or **> £1.5 million**

### If loan is ≥ £1 million

- LTV must be **60% or less**
- Credit score must be **≥ 950**

### If loan is < £1 million

| LTV condition | Credit score requirement |
|---------------|--------------------------|
| LTV < 60%     | Credit score must be ≥ 750 |
| LTV < 80%     | Credit score must be ≥ 800 |
| LTV < 90%     | Credit score must be ≥ 900 |
| LTV ≥ 90%     | Decline |

---

## Evaluation Criteria

- Correctness of business logic
- Clarity and maintainability of code
- Separation of concerns and modularity
- Effective use of AI, quality of prompting, iteration, and critical review of AI output
- Ability to explain your reasoning, assumptions, and trade-offs

---

## Submission

- Share a **public Git repository** containing your solution.
- Ensure all instructions for running the application are included.
- Include your AI log, key prompts, iterations, and any output you questioned or corrected.

---

## Thank You
