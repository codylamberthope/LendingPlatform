using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using LendingPlatform.Application.LoanOrigination;
using LendingPlatform.Domain.LoanOrigination;

namespace LendingPlatform.ConsoleApp;

/// <summary>
/// Interactive adapter: prompts for a valid application, records it, and prints the decision and book totals.
/// </summary>
public sealed class LoanOriginationConsoleHost
{
    private static readonly CultureInfo UnitedKingdom = CultureInfo.GetCultureInfo("en-GB");

    private readonly RecordSecuredLoanApplication _recordSecuredLoanApplication;
    private readonly TextReader _input;
    private readonly TextWriter _output;

    public LoanOriginationConsoleHost(
        RecordSecuredLoanApplication recordSecuredLoanApplication,
        TextReader input,
        TextWriter output)
    {
        _recordSecuredLoanApplication = recordSecuredLoanApplication;
        _input = input;
        _output = output;
    }

    public void Run()
    {
        _output.WriteLine("Secured lending console");
        _output.WriteLine("Enter quit, exit, or q at any prompt to stop.");
        _output.WriteLine();
        WriteBookStatistics(_recordSecuredLoanApplication.CurrentBookStatistics());
        _output.WriteLine();

        while (true)
        {
            if (!TryReadPoundSterlingAmount("Loan amount in GBP", out var loanAmount))
            {
                return;
            }

            if (!TryReadPoundSterlingAmount("Asset value the loan is secured against in GBP", out var securityAssetValue))
            {
                return;
            }

            if (!TryReadCreditScore(out var applicantCreditScore))
            {
                return;
            }

            var proposedApplication = ProposedSecuredLoan.From(
                loanAmount.Value,
                securityAssetValue.Value,
                applicantCreditScore.Value);
            var result = _recordSecuredLoanApplication.Execute(proposedApplication);
            WriteDecision(result.Decision);
            WriteBookStatistics(result.BookStatistics);
            _output.WriteLine();
        }
    }

    private bool TryReadPoundSterlingAmount(string prompt, [NotNullWhen(true)] out PoundSterlingAmount? amount)
    {
        amount = null;
        while (true)
        {
            if (!TryReadRequiredLine(prompt, out var line))
            {
                return false;
            }

            var cleaned = line.Replace("£", string.Empty, StringComparison.Ordinal).Trim();
            if (!decimal.TryParse(cleaned, NumberStyles.Number, UnitedKingdom, out var pounds))
            {
                _output.WriteLine("Enter a numeric pound amount, for example 250000 or 250,000.00.");
                continue;
            }

            if (!PoundSterlingAmount.TryCreate(pounds, out var parsedAmount, out var error))
            {
                _output.WriteLine(error);
                continue;
            }

            amount = parsedAmount;
            return true;
        }
    }

    private bool TryReadCreditScore([NotNullWhen(true)] out CreditScore? creditScore)
    {
        creditScore = null;
        while (true)
        {
            if (!TryReadRequiredLine("Applicant credit score (1-999)", out var line))
            {
                return false;
            }

            if (!int.TryParse(line, NumberStyles.Integer, UnitedKingdom, out var value))
            {
                _output.WriteLine("Enter a whole number from 1 to 999.");
                continue;
            }

            if (!CreditScore.TryCreate(value, out var parsedScore, out var error))
            {
                _output.WriteLine(error);
                continue;
            }

            creditScore = parsedScore;
            return true;
        }
    }

    private bool TryReadRequiredLine(string prompt, out string value)
    {
        while (true)
        {
            _output.Write($"{prompt}: ");
            var line = _input.ReadLine();
            if (IsQuitCommand(line))
            {
                value = string.Empty;
                return false;
            }

            if (string.IsNullOrWhiteSpace(line))
            {
                _output.WriteLine("Please enter a value.");
                continue;
            }

            value = line.Trim();
            return true;
        }
    }

    private void WriteDecision(LoanDecision decision)
    {
        _output.WriteLine();
        if (decision.IsApproved)
        {
            _output.WriteLine("Loan decision: Successful");
            return;
        }

        _output.WriteLine("Loan decision: Declined");
        foreach (var reason in decision.DeclineReasons)
        {
            _output.WriteLine($" - {reason.Explanation}");
        }
    }

    private void WriteBookStatistics(ApplicationBookStatistics statistics)
    {
        var meanLoanToValue = statistics.MeanLoanToValueAcrossAllApplications is { } ratio
            ? ratio.ToString("P2", UnitedKingdom)
            : "n/a";

        var totalApplications = statistics.ApprovedApplicantCount + statistics.DeclinedApplicantCount;
        _output.WriteLine(totalApplications == 0 ? "No applications recorded yet." : "Application book");
        _output.WriteLine($"Applicants approved: {statistics.ApprovedApplicantCount}");
        _output.WriteLine($"Applicants declined: {statistics.DeclinedApplicantCount}");
        _output.WriteLine($"Total value of loans written: {statistics.TotalValueOfLoansWritten}");
        _output.WriteLine($"Mean LTV across all applications: {meanLoanToValue}");
    }

    private static bool IsQuitCommand(string? line)
    {
        if (string.IsNullOrWhiteSpace(line))
        {
            return false;
        }

        var command = line.Trim();
        return command.Equals("quit", StringComparison.OrdinalIgnoreCase)
            || command.Equals("exit", StringComparison.OrdinalIgnoreCase)
            || command.Equals("q", StringComparison.OrdinalIgnoreCase);
    }
}
