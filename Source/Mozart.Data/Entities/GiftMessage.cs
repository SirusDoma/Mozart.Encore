using Mozart.Metadata;

namespace Mozart.Data.Entities;

public class GiftMessage(UserMessage message)
{
    public int Id => message.Id;

    public GiftType GiftType => message.GiftType;

    public string SenderNickname => message.SenderNickname;

    public string Title => message.Title;

    public string Content => message.Content;

    public DateTime WriteDate => message.WriteDate;

    public bool IsRead => message.IsRead;

    public void MarkAsRead()
    {
        message.IsRead = true;
        message.ReadDate = DateTime.UtcNow;
    }
}
