using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Api.Middleware;

namespace SFA.DAS.EmploymentCheck.Api.UnitTests.Middleware
{
    public class WhenRemovingServerHeaders
    {
        [TestCase(200)]
        [TestCase(302)]
        [TestCase(400)]
        [TestCase(401)]
        [TestCase(403)]
        [TestCase(404)]
        [TestCase(500)]
        public async Task Then_Headers_Added_Downstream_Are_Removed_Without_Changing_The_Response(int statusCode)
        {
            using var host = await new HostBuilder()
                .ConfigureWebHost(webBuilder => webBuilder
                    .UseTestServer()
                    .Configure(app =>
                    {
                        app.UseMiddleware<RemoveServerHeadersMiddleware>();
                        app.Run(async context =>
                        {
                            context.Response.StatusCode = statusCode;
                            context.Response.ContentType = "text/plain";
                            context.Response.Headers["X-Content-Type-Options"] = "nosniff";
                            context.Response.Headers["Location"] = "/next";
                            context.Response.Headers["WWW-Authenticate"] = "Bearer";
                            context.Response.OnStarting(() =>
                            {
                                context.Response.Headers["Server"] = "Microsoft-IIS/10.0";
                                context.Response.Headers["X-Powered-By"] = "ASP.NET";
                                return Task.CompletedTask;
                            });

                            await context.Response.WriteAsync("Response body");
                        });
                    }))
                .StartAsync();
            using var client = host.GetTestClient();

            using var response = await client.GetAsync("/");

            response.Headers.Contains("Server").Should().BeFalse();
            response.Headers.Contains("X-Powered-By").Should().BeFalse();
            response.StatusCode.Should().Be((HttpStatusCode)statusCode);
            response.Content.Headers.ContentType.MediaType.Should().Be("text/plain");
            response.Headers.GetValues("X-Content-Type-Options").Should().Equal("nosniff");
            response.Headers.Location.ToString().Should().Be("/next");
            response.Headers.WwwAuthenticate.Should().ContainSingle(value => value.Scheme == "Bearer");
            (await response.Content.ReadAsStringAsync()).Should().Be("Response body");
        }

        [Test]
        public async Task Then_A_Response_Without_Server_Headers_Is_Unchanged()
        {
            using var host = await new HostBuilder()
                .ConfigureWebHost(webBuilder => webBuilder
                    .UseTestServer()
                    .Configure(app =>
                    {
                        app.UseMiddleware<RemoveServerHeadersMiddleware>();
                        app.Run(context =>
                        {
                            context.Response.StatusCode = StatusCodes.Status204NoContent;
                            return Task.CompletedTask;
                        });
                    }))
                .StartAsync();
            using var client = host.GetTestClient();

            using var response = await client.GetAsync("/");

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            response.Headers.Contains("Server").Should().BeFalse();
            response.Headers.Contains("X-Powered-By").Should().BeFalse();
            (await response.Content.ReadAsStringAsync()).Should().BeEmpty();
        }
    }
}
