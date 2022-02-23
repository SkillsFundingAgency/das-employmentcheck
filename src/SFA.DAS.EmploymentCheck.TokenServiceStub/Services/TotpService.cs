using System;
using Simonbu11.Otp;
using Simonbu11.Otp.Totp;

namespace SFA.DAS.EmploymentCheck.TokenServiceStub.Services
{
    public class TotpService : ITotpService
    {
        public string Generate(string secret)
        {
            var generator = new UnpaddedHmacSha512TotpGenerator(new TotpGeneratorSettings
            {
                SharedSecret = OtpSharedSecret.FromBase32String(secret)
            });
            return generator.Generate(DateTime.UtcNow);
        }

        private sealed class UnpaddedHmacSha512TotpGenerator : HmacSha512TotpGenerator
        {
            public UnpaddedHmacSha512TotpGenerator(TotpGeneratorSettings settings) : base(settings)
            {
            }

            protected override byte[] ConvertSecretToHashKey(OtpSharedSecret sharedSecret)
            {
                return sharedSecret.Data;
            }
        }
    }
}