namespace SFA.DAS.EmploymentCheck.Functions.Application.Enums
{
    enum RequestTypeEnum : byte
    {
        Unknown = 0,
        EmploymentCheck = 1,
        NinoLookup =2,
        PayeSchemLookup = 3,
        EmployedLookup = 4
    }
}
