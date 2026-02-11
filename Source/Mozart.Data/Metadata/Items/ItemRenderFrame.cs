namespace Mozart.Metadata.Items;

public class ItemRenderFrame
{
    public ItemRenderPart ItemRenderPart { get; set; }
    public Instrument Instrument         { get; set; }
    public Gender Gender                 { get; set; }
    public required string Reference     { get; set; }
}
