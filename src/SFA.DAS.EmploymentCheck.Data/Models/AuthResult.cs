﻿using System.Text.Json.Serialization;

namespace SFA.DAS.EmploymentCheck.Data.Models
{
    public class AuthResult
    {
        [JsonPropertyName("token_type")]
        public string TokenType { get; set; }
        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }
        [JsonPropertyName("ext_expires_in")]
        public int ExtExpiresIn { get; set; }
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }
    }
}