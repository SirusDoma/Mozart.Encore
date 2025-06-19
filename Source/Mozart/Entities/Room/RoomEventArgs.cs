using Mozart.Messages;
using Mozart.Metadata.Room;

namespace Mozart.Entities;

public class RoomUserJoinedEventArgs : EventArgs
{
    public required int MemberId           { get; init; }

    public required Room.MemberSlot Member { get; init; }
}

public class RoomUserLeftEventArgs : EventArgs
{
    public required int MemberId           { get; init; }

    public required Room.MemberSlot Member { get; init; }

    public required int RoomMasterMemberId { get; init; }
}

public class RoomTitleChangedEventArgs : EventArgs
{
    public required string Title { get; init; }
}

public class RoomMusicChangedEventArgs : EventArgs
{
    public required int MusicId           { get; init; }
    public required GameSpeed Speed       { get; init; }
    public required Difficulty Difficulty { get; init; }
}

public class RoomArenaChangedEventArgs : EventArgs
{
    public required Arena Arena    { get; init; }
    public required int RandomSeed { get; init; }
}

public class RoomStateChangedEventArgs : EventArgs
{
    public required RoomState PreviousState { get; init; }
    public required RoomState CurrentState  { get; init; }
}

public class RoomUserReadyStateChangedEventArgs : EventArgs
{
    public required int MemberId           { get; init; }
    public required Room.MemberSlot Member { get; init; }
    public required bool Ready             { get; init; }
}

public class RoomUserTeamChangedEventArgs : EventArgs
{
    public required int MemberId           { get; init; }
    public required Room.MemberSlot Member { get; init; }
    public required RoomTeam Team          { get; init; }
}

public class RoomSlotChangedEventArgs : EventArgs
{
    public required int SlotId                    { get; init; }
    public required Room.ISlot PreviousSlot       { get; init; }
    public required Room.ISlot CurrentSlot        { get; init; }
    public required RoomSlotActionType ActionType { get; init; }
    public required int Capacity                  { get; init; }
    public required int UserCount                 { get; init; }
}