using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Commands
{
    public class CommandService : ICommandService
    {
        private readonly HttpClient _client;
        private const string AllowedTypesPrefix = "SFA.DAS.EmploymentCheck.Commands.Types.";

        public CommandService(HttpClient client)
        {
            _client = client;
        }

        public async Task Dispatch<T>(T command) where T : DomainCommand
        {
            var commandType = command.GetType();
            EnsureIsValidType(commandType);

            var commandText = JsonConvert.SerializeObject(command, new JsonSerializerSettings {ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
            var response = await _client.PostAsJsonAsync($"commands/{commandType.FullName?.Replace(AllowedTypesPrefix, "")}", commandText);
            response.EnsureSuccessStatusCode();
        }

        private static void EnsureIsValidType(Type commandType)
        {
            if (commandType.FullName == null || !commandType.FullName.StartsWith(AllowedTypesPrefix))
            {
                throw new ArgumentException($"Invalid command type {commandType.FullName}");
            }
        }
    }
}
