using Encore.Data.Entities;
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

    public class UserInfo : SubMessage
    {
        [MessageField(order: 0)]
        public int Id { get; init; }

        [StringMessageField(order: 1)]
        public required string Username { get; init; }

        [MessageField(order: 2)]
        public int GemStar { get; init; }

        [MessageField(order: 3)]
        public FreePassType FreePass { get; init; } = FreePassType.None;

        [StringMessageField(order: 4)]
        public required string Nickname { get; init; }

        [MessageField(order: 5)]
        public int Unknown1 { get; init; }

        [MessageField(order: 6)]
        public int Unknown2 { get; init; }
    }

    [MessageField(order: 0)]
    public AuthSessionResult Result { get; init; }

    [MessageField(order: 1)]
    public UserInfo? Info { get; init; }
}
