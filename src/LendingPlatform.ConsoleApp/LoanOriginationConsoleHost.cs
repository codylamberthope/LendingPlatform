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
    private readonly IRecordedApplicationStore _recordedApplicationStore;
    private readonly TextReader _input;
    private readonly TextWriter _output;

    public LoanOriginationConsoleHost(
        RecordSecuredLoanApplication recordSecuredLoanApplication,
        IRecordedApplicationStore recordedApplicationStore,
        TextReader input,
        TextWriter output)
    {
        _recordSecuredLoanApplication = recordSecuredLoanApplication;
        _recordedApplicationStore = recordedApplicationStore;
        _input = input;
        _output = output;
    }

    public void Run()
    {
        _output.WriteLine("Secured lending console");
        _output.WriteLine("Enter quit, exit, or q at any prompt to stop.");
        _output.WriteLine();
        WriteBookStatistics(ApplicationBookStatistics.From(_recordedApplicationStore.GetAll()));
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

            var result = _recordSecuredLoanApplication.Execute(loanAmount, securityAssetValue, applicantCreditScore);
            WriteDecision(result.Decision);
            WriteBookStatistics(result.BookStatistics);
            _output.WriteLine();
        }
    }

    private bool TryReadPoundSterlingAmount(string prompt, out PoundSterlingAmount amount)
    {
        while (true)
        {
            _output.Write($"{prompt}: ");
            var line = _input.ReadLine();
            if (IsQuitCommand(line))
            {
                amount = PoundSterlingAmount.Zero;
                return false;
            }

            if (string.IsNullOrWhiteSpace(line))
            {
                _output.WriteLine("Please enter a value.");
                continue;
            }

            var cleaned = line.Trim().Replace("£", string.Empty, StringComparison.Ordinal).Trim();
            if (!decimal.TryParse(cleaned, NumberStyles.Number, UnitedKingdom, out var pounds))
            {
                _output.WriteLine("Enter a numeric pound amount, for example 250000 or 250,000.00.");
                continue;
            }

            if (!PoundSterlingAmount.TryCreate(pounds, out amount, out var error))
            {
                _output.WriteLine(error);
                continue;
            }

            return true;
        }
    }

    private bool TryReadCreditScore(out CreditScore creditScore)
    {
        while (true)
        {
            _output.Write("Applicant credit score (1-999): ");
            var line = _input.ReadLine();
            if (IsQuitCommand(line))
            {
                creditScore = CreditScore.From(1);
                return false;
            }

            if (string.IsNullOrWhiteSpace(line))
            {
                _output.WriteLine("Please enter a value.");
                continue;
            }

            if (!int.TryParse(line.Trim(), NumberStyles.Integer, UnitedKingdom, out var value))
            {
                _output.WriteLine("Enter a whole number from 1 to 999.");
                continue;
            }

            if (!CreditScore.TryCreate(value, out var parsedScore, out var error) || parsedScore is null)
            {
                _output.WriteLine(error);
                continue;
            }

            creditScore = parsedScore;
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
        var totalApplications = statistics.ApprovedApplicantCount + statistics.DeclinedApplicantCount;
        if (totalApplications == 0)
        {
            _output.WriteLine("No applications recorded yet.");
            _output.WriteLine("Applicants approved: 0");
            _output.WriteLine("Applicants declined: 0");
            _output.WriteLine("Total value of loans written: £0.00");
            _output.WriteLine("Mean LTV across all applications: n/a");
            return;
        }

        var meanLoanToValue = statistics.MeanLoanToValueAcrossAllApplications is { } ratio
            ? ratio.ToString("P2", UnitedKingdom)
            : "n/a";

        _output.WriteLine("Application book");
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
