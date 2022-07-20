using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.EmploymentCheck.Api.Configuration
{
    [ExcludeFromCodeCoverage]
    public class AddVersionHeaderParameter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            operation.Parameters ??= new List<OpenApiParameter>();

            operation.Parameters.Add(
                new OpenApiParameter
            {
                Name = "X-Version",
                In = ParameterLocation.Header,
                Required = true,
            });
        }
    }
}
