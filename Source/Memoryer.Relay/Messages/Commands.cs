namespace Memoryer.Relay;

public enum RelayCommand : ushort
{
    PeerConnected         = 0x001, // 1
    PeerConfirm           = 0x002, // 2
    PeerEndpointAssigned  = 0x003, // 3
    Ping                  = 0x004, // 4
    UpdateGameStats       = 0x005, // 5
    ComboBroken           = 0x006, // 6 (a.k.a CO2JamMissPack)
    ComboStarted          = 0x007, // 7 (a.k.a CO2JamCoolPack)
    CreateSession         = 0x00A, // 10
    DeleteSession         = 0x00B  // 11
}
