using Mozart.Metadata.Items;

namespace Mozart.Data.Entities;

public class GiftBox(User user, List<GiftItem> items, List<GiftMusic> musics)
{
    public IReadOnlyList<GiftItem> Items   => items;

    public IReadOnlyList<GiftMusic> Musics => musics;

    public void SendItem(User sender, ItemData item)
    {
        items.Add(new GiftItem
        {
            UserId         = user.Id,
            ItemId         = item.Id,
            SenderId       = sender.Id,
            SenderNickname = sender.Nickname,
            SendDate       = DateTime.UtcNow
        });
    }

    public void SendMusic(User sender, int musicId)
    {
        musics.Add(new GiftMusic
        {
            UserId         = user.Id,
            MusicId        = musicId,
            SenderId       = sender.Id,
            SenderNickname = sender.Nickname,
            SendDate       = DateTime.UtcNow,
        });
    }

    public bool ClaimItem(int giftId, ItemData item)
    {
        bool claimed = items.RemoveAll(i => i.Id == giftId && i.ItemId == item.Id) > 0;
        bool inserted = false;

        if (claimed)
        {
            var inventory = user.Inventory;
            for (int i = 0; i < inventory.Capacity; i++)
            {
                if (inventory[i].Id != 0)
                    continue;

                int count = 0;
                if (item.ItemKind == ItemKind.AttributiveItem)
                    count = item.Quantity;

                inventory[i] = new Inventory.BagItem
                {
                    Id    = (short)item.Id,
                    Count = count
                };

                inserted = true;
                break;
            }
        }

        return claimed && inserted;
    }

    public bool ClaimMusic(int giftId, int musicId)
    {
        bool claimed = musics.RemoveAll(i => i.Id == giftId && i.MusicId == musicId) > 0;
        bool inserted = false;

        if (claimed)
        {
            if (user.AcquiredMusicList.All(m => m.MusicId != musicId))
            {
                user.AcquiredMusicList.Add(new AcquiredMusic
                {
                    UserId  = user.Id,
                    MusicId = musicId
                });

                inserted = true;
            }
        }

        return claimed && inserted;
    }
}
