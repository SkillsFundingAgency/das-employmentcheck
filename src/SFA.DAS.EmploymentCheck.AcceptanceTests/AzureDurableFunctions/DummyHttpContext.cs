using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;

namespace SFA.DAS.EmploymentCheck.AcceptanceTests.AzureDurableFunctions
{
    public class DummyHttpContext : HttpContext
    {
        public DummyHttpContext(HttpRequest request)
        {
            Request = request;
        }

        public override void Abort()
        {
        }

        public override IFeatureCollection Features { get; } = new FeatureCollection();
        public override HttpRequest Request { get; }
        public override HttpResponse Response { get; } = null;
        public override ConnectionInfo Connection { get; } = null;
        public override WebSocketManager WebSockets { get; } = null;
        public override ClaimsPrincipal User { get; set; }
        public override IDictionary<object, object> Items { get; set; } = new Dictionary<object, object>();
        public override IServiceProvider RequestServices { get; set; }
        public override CancellationToken RequestAborted { get; set; }
        public override string TraceIdentifier { get; set; }
        public override ISession Session { get; set; }
    }
}
