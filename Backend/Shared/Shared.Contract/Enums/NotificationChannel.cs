using System.Runtime.Serialization;

namespace Shared.Contract.Enums;

public enum NotificationChannel
{
    [EnumMember(Value = "email")]
    Email,

    [EnumMember(Value = "sms")]
    Sms,

    [EnumMember(Value = "whatsapp")]
    WhatsApp
}
