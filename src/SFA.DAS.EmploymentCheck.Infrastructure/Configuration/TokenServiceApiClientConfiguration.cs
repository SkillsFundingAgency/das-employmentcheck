using System.Security.Cryptography.X509Certificates;
using SFA.DAS.TokenService.Api.Client;

namespace SFA.DAS.EmploymentCheck.Infrastructure.Configuration
{
    public class TokenServiceApiClientConfiguration : ITokenServiceApiClientConfiguration
    {
        public string ApiBaseUrl { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string IdentifierUri { get; set; }
        public string Tenant { get; set; }
        public string CertificateThumbprint { get; set; }
        public X509Certificate TokenCertificate
        {
            get
            {
                var store = new X509Store(StoreLocation.LocalMachine);
                store.Open(OpenFlags.ReadOnly);
                try
                {
                    var thumbprint = CertificateThumbprint;

                    if (string.IsNullOrWhiteSpace(thumbprint))
                    {
                        return null;
                    }

                    var certificates = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false);

                    return certificates[0];
                }
                finally
                {
                    store.Close();
                }
            }
            set { }
        }
    }
}
