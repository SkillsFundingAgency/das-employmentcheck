using SFA.DAS.EmploymentCheck.Application.ApiClients;

namespace SFA.DAS.EmploymentCheck.Application.Services.EmployerAccount
{
    public class GetAccountPayeSchemesRequest : IGetApiRequest
    {
        private readonly string _hashedAccountId;

        public GetAccountPayeSchemesRequest(string hashedAccountId)
        {
            _hashedAccountId = hashedAccountId;
        }

        public string GetUrl => $"api/accounts/{_hashedAccountId}/payeschemes";
    }
}