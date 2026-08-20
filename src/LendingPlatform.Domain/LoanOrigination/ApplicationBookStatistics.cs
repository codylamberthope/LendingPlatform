namespace LendingPlatform.Domain.LoanOrigination;

/// <summary>
/// Running totals over the recorded application book.
/// Mean LTV is the unweighted average of each application's ratio; null when the book is empty.
/// Total written is approved principal only.
/// </summary>
public sealed class ApplicationBookStatistics
{
    public int ApprovedApplicantCount { get; }

    public int DeclinedApplicantCount { get; }

    public PoundSterlingAmount TotalValueOfLoansWritten { get; }

    public decimal? MeanLoanToValueAcrossAllApplications { get; }

    private ApplicationBookStatistics(
        int approvedApplicantCount,
        int declinedApplicantCount,
        PoundSterlingAmount totalValueOfLoansWritten,
        decimal? meanLoanToValueAcrossAllApplications)
    {
        ApprovedApplicantCount = approvedApplicantCount;
        DeclinedApplicantCount = declinedApplicantCount;
        TotalValueOfLoansWritten = totalValueOfLoansWritten;
        MeanLoanToValueAcrossAllApplications = meanLoanToValueAcrossAllApplications;
    }

    public static ApplicationBookStatistics Empty { get; } = new(0, 0, PoundSterlingAmount.Zero, null);

    public static ApplicationBookStatistics From(IReadOnlyList<SecuredLoanApplication> recordedApplications)
    {
        if (recordedApplications.Count == 0)
        {
            return Empty;
        }

        var approved = recordedApplications.Where(application => application.Decision.IsApproved).ToList();
        var totalWritten = PoundSterlingAmount.Sum(approved.Select(application => application.LoanAmount));
        var meanLoanToValue = recordedApplications.Average(application => application.EvaluatedLoanToValue.Ratio);

        return new ApplicationBookStatistics(
            approved.Count,
            recordedApplications.Count - approved.Count,
            totalWritten,
            meanLoanToValue);
    }
}
