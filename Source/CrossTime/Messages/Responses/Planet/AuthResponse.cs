using Encore.Messaging;

namespace CrossTime.Messages.Responses;

public enum AuthSessionResult : uint
{
    Success           = 0x00000000,
    DatabaseError     = 0xFFFFFF9B, // -101
    DuplicateSessions = 0xFFFFFFFB, // -5
    Banned            = 0xFFFFFFFC, // -4
    InvalidPassword   = 0xFFFFFFFD, // -3
    InvalidUsername   = 0xFFFFFFFE, // -2
    InvalidParameter  = 0xFFFFFFFF, // -1
}

public class AuthResponse : IMessage
{
    public static Enum Command => ResponseCommand.Authorize;

    [MessageField(order: 0)]
    public AuthSessionResult Result { get; init; }

    [MessageField(order: 1)]
    public int Id { get; set; }

    [StringMessageField(order: 2)]
    public string Username { get; init; } = "\0";

    [MessageField(order: 3)]
    public int GemStar { get; set; }

    [MessageField(order: 4)]
    public int MembershipType { get; set; }

    [StringMessageField(order: 5)]
    public string Nickname { get; init; } = "\0";

    [MessageField(order: 6)]
    public int Unknown1 { get; set; }

    [MessageField(order: 7)]
    public int Unknown2 { get; set; }
}
