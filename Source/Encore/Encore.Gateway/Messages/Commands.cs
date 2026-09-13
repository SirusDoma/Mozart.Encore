namespace Encore;

// O2JamServer -> O2JamGManager
public enum ServerCommand : ushort
{
    ChannelRegister        = 0x0000, // 0
    ChannelLogin           = 0x0004, // 4
    GrantSession           = 0x0006, // 6
    TerminateSession       = 0x0008, // 8
    MusicList              = 0x000A, // 10
    Announce               = 0x000B, // 11
    ChannelLoginCancel     = 0x0014, // 20
    SyncGem                = 0x0015, // 21
    AlbumScoreUpdate       = 0x0016, // 22
    FirstPlayBonus         = 0x0017, // 23
    BugleMessage           = 0x0019, // 25
    UserStatsUpdate        = 0x012C, // 300
    SyncItemPurchase       = 0x012F, // 303
    SellItem               = 0x0131, // 305
    EquipItem              = 0x0132, // 306
    NoOp                   = 0x0133, // 307
    SyncMusicPurchase      = 0x0135, // 309
    SyncPoint              = 0x0137, // 311
    StartPayment           = 0x0139, // 313
    GetGiftMessageList     = 0x013B, // 315
    ReadGiftMessage        = 0x013D, // 317
    ClaimGift              = 0x013E, // 318
    ConsumeAttributiveItem = 0x0140, // 320
    SyncFreePass           = 0x0142, // 322
}

// O2JamGManager -> O2JamServer / O2JamGateWay
public enum GameManagerCommand : ushort
{
    ChannelRegister     = 0x0001, // 1
    ChannelLogin        = 0x0005, // 5
    GrantSession        = 0x0007, // 7
    TerminateSession    = 0x0009, // 9
    Announcement        = 0x000B, // 11
    BonusPresentItem    = 0x0018, // 24
    BugleMessageResult  = 0x001A, // 26
    ChannelState        = 0x0064, // 100
    ChannelList         = 0x00C9, // 201
    ChannelCreated      = 0x00CA, // 202
    ChannelRemoved      = 0x00CB, // 203
    UserRoute           = 0x00CD, // 205
    BugleMessage        = 0x00CE, // 206
    SyncItemPurchase    = 0x0130, // 304
    SyncMusicPurchase   = 0x0136, // 310
    SyncPoint           = 0x0138, // 312
    StartPayment        = 0x013A, // 314
    GetGiftMessageList  = 0x013C, // 316
    ClaimGift           = 0x013F, // 319
    AttributiveItemList = 0x0141, // 321
    SyncFreePass        = 0x0143, // 323
}

// O2JamGateWay -> O2JamGManager / O2JamServer
public enum GatewayCommand : ushort
{
    GatewayRegister = 0x00C8, // 200
    UserRoute       = 0x00CC, // 204
    CreateSession   = 0x03EE, // 1006
}
