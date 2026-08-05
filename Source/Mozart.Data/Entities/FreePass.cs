namespace Mozart.Data.Entities;

public class FreePass(FreePassType type, DateTime expiryDate)
{
    public FreePassType Type { get; } = type;

    public DateTime ExpiryDate { get; } = expiryDate;
}
