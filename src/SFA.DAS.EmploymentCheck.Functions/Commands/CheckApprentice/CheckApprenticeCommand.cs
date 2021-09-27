﻿using MediatR;
using SFA.DAS.EmploymentCheck.Functions.Dtos;

namespace SFA.DAS.EmploymentCheck.Functions.Commands.CheckApprentice
{
    public class CheckApprenticeCommand : IRequest
    {
        public CheckApprenticeCommand(ApprenticeToVerifyDto apprentice)
        {
            Apprentice = apprentice;
        }

        public ApprenticeToVerifyDto Apprentice { get; }
    }
}
