using System.Text.Json;
using LendingPlatform.Application.LoanOrigination;
using LendingPlatform.Domain.LoanOrigination;

namespace LendingPlatform.Infrastructure.LoanOrigination;

/// <summary>
/// Stores the application book as JSON so running totals survive process restarts.
/// </summary>
public sealed class JsonFileRecordedApplicationStore : IRecordedApplicationStore
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly string _filePath;
    private readonly List<SecuredLoanApplication> _recordedApplications;

    public JsonFileRecordedApplicationStore(string filePath)
    {
        _filePath = filePath;
        _recordedApplications = LoadFromFile(filePath);
    }

    public IReadOnlyList<SecuredLoanApplication> GetAll() => _recordedApplications.AsReadOnly();

    public void Add(SecuredLoanApplication application)
    {
        _recordedApplications.Add(application);
    }

    public void Save()
    {
        var directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var payload = _recordedApplications.Select(ToDto).ToList();
        var json = JsonSerializer.Serialize(payload, SerializerOptions);
        File.WriteAllText(_filePath, json);
    }

    private static List<SecuredLoanApplication> LoadFromFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return new List<SecuredLoanApplication>();
        }

        string json;
        try
        {
            json = File.ReadAllText(filePath);
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException(
                $"The application book at '{filePath}' could not be read.",
                exception);
        }

        if (string.IsNullOrWhiteSpace(json))
        {
            return new List<SecuredLoanApplication>();
        }

        List<RecordedApplicationFileDto>? dtos;
        try
        {
            dtos = JsonSerializer.Deserialize<List<RecordedApplicationFileDto>>(json, SerializerOptions);
        }
        catch (JsonException exception)
        {
            throw new InvalidOperationException(
                $"The application book at '{filePath}' is not valid JSON and will not be overwritten.",
                exception);
        }

        if (dtos is null)
        {
            return new List<SecuredLoanApplication>();
        }

        try
        {
            return dtos.Select(ToDomain).ToList();
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException(
                $"The application book at '{filePath}' contains records that cannot be reconstituted.",
                exception);
        }
    }

    private static RecordedApplicationFileDto ToDto(SecuredLoanApplication application)
    {
        return new RecordedApplicationFileDto
        {
            Id = application.Id.Value,
            LoanAmountPounds = application.LoanAmount.Pounds,
            SecurityAssetValuePounds = application.SecurityAssetValue.Pounds,
            ApplicantCreditScore = application.ApplicantCreditScore.Value,
            EvaluatedLoanToValueRatio = application.EvaluatedLoanToValue.Ratio,
            IsApproved = application.Decision.IsApproved,
            DeclineReasons = application.Decision.DeclineReasons
                .Select(reason => new DeclineReasonFileDto
                {
                    Code = reason.Code,
                    Explanation = reason.Explanation
                })
                .ToList(),
            RecordedAt = application.RecordedAt
        };
    }

    private static SecuredLoanApplication ToDomain(RecordedApplicationFileDto dto)
    {
        var declineReasons = (dto.DeclineReasons ?? new List<DeclineReasonFileDto>())
            .Select(reason => new EligibilityDeclineReason(reason.Code, reason.Explanation))
            .ToList();

        return SecuredLoanApplication.Reconstitute(
            new SecuredLoanApplicationId(dto.Id),
            PoundSterlingAmount.FromPounds(dto.LoanAmountPounds),
            PoundSterlingAmount.FromPounds(dto.SecurityAssetValuePounds),
            CreditScore.From(dto.ApplicantCreditScore),
            LoanDecision.FromRecordedState(dto.IsApproved, declineReasons),
            dto.RecordedAt);
    }
}
