namespace Mozart.Metadata.Items;

public class ItemSpecialAttribute
{
    public Gender Gender { get; set; }
}

public class ItemData
{
    public int Id                             { get; set; }
    public ItemPart ItemPart                  { get; set; }
    public ItemKind ItemKind                  { get; set; }
    public Planet Origin                      { get; set; }
    public Gender Gender                      { get; set; }
    public bool IsNew                         { get; set; }
    public short Quantity                     { get; set; }
    public GameModifier GameModifier          { get; set; }
    public GameModifierType GameModifierType  { get; set; }
    public ItemPriceInfo Price                { get; set; }
    public string Name                        { get; set; } = string.Empty;
    public string Description                 { get; set; } = string.Empty;

    public ItemSpecialAttribute? Special      { get; set; }
    public List<ItemRenderFrame> RenderFrames { get; set; }

    public ItemData()
    {
        RenderFrames = new List<ItemRenderFrame>();
        Price        = new ItemPriceInfo();
    }
}
