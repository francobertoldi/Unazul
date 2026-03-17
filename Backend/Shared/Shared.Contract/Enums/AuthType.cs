using System.Runtime.Serialization;

namespace Shared.Contract.Enums;

public enum AuthType
{
    [EnumMember(Value = "none")]
    None,

    [EnumMember(Value = "api_key")]
    ApiKey,

    [EnumMember(Value = "bearer_token")]
    BearerToken,

    [EnumMember(Value = "basic_auth")]
    BasicAuth,

    [EnumMember(Value = "oauth2")]
    OAuth2,

    [EnumMember(Value = "custom_header")]
    CustomHeader
}
