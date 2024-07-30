using SFA.DAS.EmploymentCheck.Application.ApiClients;

namespace SFA.DAS.EmploymentCheck.Application.Services.EmployerAccount
{
    public class GetAccountPayeSchemesRequest : IGetApiRequest
    {
        private readonly long _accountId;

        public GetAccountPayeSchemesRequest(long accountId)
        {
            _accountId = accountId;
        }

        public string GetUrl => $"api/accounts/{_accountId}/payeschemes";
    }
}