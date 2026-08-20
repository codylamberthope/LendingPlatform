using LendingPlatform.Application.LoanOrigination;
using LendingPlatform.ConsoleApp;
using LendingPlatform.Domain.LoanOrigination;
using LendingPlatform.Infrastructure.LoanOrigination;

var dataDirectory = Path.Combine(Directory.GetCurrentDirectory(), "data");
var storePath = Path.Combine(dataDirectory, "recorded-applications.json");

try
{
    var store = new JsonFileRecordedApplicationStore(storePath);
    var policy = new LoanEligibilityPolicy();
    var recordApplication = new RecordSecuredLoanApplication(policy, store);
    var host = new LoanOriginationConsoleHost(recordApplication, store, Console.In, Console.Out);
    host.Run();
}
catch (InvalidOperationException exception)
{
    Console.Error.WriteLine(exception.Message);
    return 1;
}

return 0;
