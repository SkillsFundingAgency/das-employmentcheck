using System;
using System.Windows.Input;
using MediatR;

namespace SFA.DAS.EmploymentCheck.Api.Commands
{
    public class PostRegisterCheckCommand : IRequest<PostRegisterCheckResponse>
    {
        public Guid CorrelationId { get; set; }
        public string CheckType { get; set; }
        public long Uln { get; set; }
        public int ApprenticeshipAccountId { get; set; }
        public long? ApprenticeshipId { get; set; }
        public DateTime MinDate { get; set; }
        public DateTime MaxDate { get; set; }
    }
}