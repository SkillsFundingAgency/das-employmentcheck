using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace SFA.DAS.EmploymentCheck.Functions.TestHelpers.AzureDurableFunctions
{
    public sealed class DummyHttpRequest : HttpRequest
    {
        private readonly string _content;
        private Stream _stream;

        public DummyHttpRequest() : this("") { }

        public DummyHttpRequest(string content)
        {
            HttpContext = new DummyHttpContext(this);
            _content = content;
        }
        public override Task<IFormCollection> ReadFormAsync(CancellationToken cancellationToken = new CancellationToken()) =>
            Task.FromResult((IFormCollection)new FormCollection(new Dictionary<string, StringValues>()));

        public override HttpContext HttpContext { get; }
        public override string Method { get; set; } = "Get";
        public override string Scheme { get; set; } = "http";
        public override bool IsHttps { get; set; }
        public override HostString Host { get; set; } = new HostString("dummy");
        public override PathString PathBase { get; set; }
        public override PathString Path { get; set; }
        public override QueryString QueryString { get; set; }
        public override IQueryCollection Query { get; set; } = new QueryCollection();
        public override string Protocol { get; set; }
        public override IHeaderDictionary Headers { get; } = new HeaderDictionary();
        public override IRequestCookieCollection Cookies { get; set; }
        public override long? ContentLength { get; set; }
        public override string ContentType { get; set; }

        public override Stream Body
        {
            get => _stream ??= new MemoryStream(Encoding.UTF8.GetBytes(_content));
            set => _stream = value;
        }

        public override bool HasFormContentType => false;
        public override IFormCollection Form { get; set; }
    }

}
