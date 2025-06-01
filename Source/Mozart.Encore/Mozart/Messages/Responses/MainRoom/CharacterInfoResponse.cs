using Encore.Messaging;

namespace Mozart;

public class CharacterInfoResponse : IMessage
{
    public static Enum Command => ResponseCommand.GetCharacterInfo;

    // Suspended?
    [MessageField<MessageFieldCodec<int>>(order: 0)]
    public bool DisableInventory { get; init; }

    [MessageField(order: 1)]
    public required CharacterInfo CharacterInfo { get; init; }

}