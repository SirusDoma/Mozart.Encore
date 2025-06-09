using System.Collections;
using Mozart.Metadata.Items;

namespace Mozart.Persistence.Entities;

public class Inventory
{
    public int UserId { get; init; }

    public short Equip1  { get; set; }
    public short Equip2  { get; set; }
    public short Equip3  { get; set; }
    public short Equip4  { get; set; }
    public short Equip5  { get; set; }
    public short Equip6  { get; set; }
    public short Equip7  { get; set; }
    public short Equip8  { get; set; }
    public short Equip9  { get; set; }
    public short Equip10 { get; set; }
    public short Equip11 { get; set; }
    public short Equip12 { get; set; }
    public short Equip13 { get; set; }
    public short Equip14 { get; set; }
    public short Equip15 { get; set; }
    public short Equip16 { get; set; }

    public short Bag1  { get; set; }
    public short Bag2  { get; set; }
    public short Bag3  { get; set; }
    public short Bag4  { get; set; }
    public short Bag5  { get; set; }
    public short Bag6  { get; set; }
    public short Bag7  { get; set; }
    public short Bag8  { get; set; }
    public short Bag9  { get; set; }
    public short Bag10 { get; set; }
    public short Bag11 { get; set; }
    public short Bag12 { get; set; }
    public short Bag13 { get; set; }
    public short Bag14 { get; set; }
    public short Bag15 { get; set; }
    public short Bag16 { get; set; }
    public short Bag17 { get; set; }
    public short Bag18 { get; set; }
    public short Bag19 { get; set; }
    public short Bag20 { get; set; }
    public short Bag21 { get; set; }
    public short Bag22 { get; set; }
    public short Bag23 { get; set; }
    public short Bag24 { get; set; }
    public short Bag25 { get; set; }
    public short Bag26 { get; set; }
    public short Bag27 { get; set; }
    public short Bag28 { get; set; }
    public short Bag29 { get; set; }
    public short Bag30 { get; set; }

    public short GetBagItemId(int slot)
    {
        return slot switch
        {
            1  => Bag1,   2 => Bag2,   3 => Bag3,   4 => Bag4,   5 => Bag5,
            6  => Bag6,   7 => Bag7,   8 => Bag8,   9 => Bag9,  10 => Bag10,
            11 => Bag11, 12 => Bag12, 13 => Bag13, 14 => Bag14, 15 => Bag15,
            16 => Bag16, 17 => Bag17, 18 => Bag18, 19 => Bag19, 20 => Bag20,
            21 => Bag21, 22 => Bag22, 23 => Bag23, 24 => Bag24, 25 => Bag25,
            26 => Bag26, 27 => Bag27, 28 => Bag28, 29 => Bag29, 30 => Bag30,
            _ => throw new ArgumentOutOfRangeException(nameof(slot))
        };
    }

    public void SetBagItemId(int slot, short itemId)
    {
        switch (slot)
        {
            case  1: Bag1  = itemId; break;
            case  2: Bag2  = itemId; break;
            case  3: Bag3  = itemId; break;
            case  4: Bag4  = itemId; break;
            case  5: Bag5  = itemId; break;
            case  6: Bag6  = itemId; break;
            case  7: Bag7  = itemId; break;
            case  8: Bag8  = itemId; break;
            case  9: Bag9  = itemId; break;
            case 10: Bag10 = itemId; break;
            case 11: Bag11 = itemId; break;
            case 12: Bag12 = itemId; break;
            case 13: Bag13 = itemId; break;
            case 14: Bag14 = itemId; break;
            case 15: Bag15 = itemId; break;
            case 16: Bag16 = itemId; break;
            case 17: Bag17 = itemId; break;
            case 18: Bag18 = itemId; break;
            case 19: Bag19 = itemId; break;
            case 20: Bag20 = itemId; break;
            case 21: Bag21 = itemId; break;
            case 22: Bag22 = itemId; break;
            case 23: Bag23 = itemId; break;
            case 24: Bag24 = itemId; break;
            case 25: Bag25 = itemId; break;
            case 26: Bag26 = itemId; break;
            case 27: Bag27 = itemId; break;
            case 28: Bag28 = itemId; break;
            case 29: Bag29 = itemId; break;
            case 30: Bag30 = itemId; break;
            default: throw new ArgumentOutOfRangeException(nameof(slot), "Index must be between 1 and 30");
        }
    }

    public short GetEquipmentItemId(ItemType type)
    {
        return type switch
        {
            ItemType.Instrument         => Equip1,
            ItemType.Hair               => Equip2,
            ItemType.Earring            => Equip3,
            ItemType.Gloves             => Equip4,
            ItemType.Accessories        => Equip5,
            ItemType.Top                => Equip6,
            ItemType.Pants              => Equip7,
            ItemType.Glasses            => Equip8,
            ItemType.Necklace           => Equip9,
            ItemType.ClothesAccessories => Equip10,
            ItemType.Shoes              => Equip11,
            ItemType.Face               => Equip12,
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };
    }

    public void SetEquipmentItemId(ItemType type, short itemId)
    {
        switch (type)
        {
            case ItemType.Instrument         : Equip1  = itemId; break;
            case ItemType.Hair               : Equip2  = itemId; break;
            case ItemType.Earring            : Equip3  = itemId; break;
            case ItemType.Gloves             : Equip4  = itemId; break;
            case ItemType.Accessories        : Equip5  = itemId; break;
            case ItemType.Top                : Equip6  = itemId; break;
            case ItemType.Pants              : Equip7  = itemId; break;
            case ItemType.Glasses            : Equip8  = itemId; break;
            case ItemType.Necklace           : Equip9  = itemId; break;
            case ItemType.ClothesAccessories : Equip10 = itemId; break;
            case ItemType.Shoes              : Equip11 = itemId; break;
            case ItemType.Face               : Equip12 = itemId; break;
            default: throw new ArgumentOutOfRangeException(nameof(type), "Index must be between 1 and 30");
        }
    }
}

public class InventoryItems(Inventory items) : IReadOnlyList<short>, IEnumerator
{
    private int _pointer = -1;

    private class InventoryEnumerator : IEnumerator<short>
    {
        private readonly InventoryItems _items;

        public InventoryEnumerator(InventoryItems items)
        {
            _items = items;
        }

        public bool MoveNext()
        {
            return ((IEnumerator)_items).MoveNext();
        }

        public void Reset()
        {
            ((IEnumerator)_items).Reset();
        }

        public short Current => _items[_items._pointer];

        object? IEnumerator.Current => _items[_items._pointer];

        public void Dispose()
        {
        }
    }

    public short this[int index]
    {
        get => items.GetBagItemId(index + 1);
        set => items.SetBagItemId(index + 1, value);
    }

    public IEnumerator<short> GetEnumerator()
    {
        return new InventoryEnumerator(this);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public int Capacity => 30;

    public int Count
    {
        get
        {
            int count = 0;
            for (int i = 0; i < Capacity; i ++)
            {
                if (items.GetBagItemId(i + 1) != 0)
                    count++;
            }

            return count;
        }
    }

    public int FindSlot(int id)
    {
        for (int i = 0; i < Capacity; i ++)
        {
            if (items.GetBagItemId(i + 1) == id)
                return i;
        }

        return -1;
    }

    bool IEnumerator.MoveNext()
    {
        if (_pointer >= Capacity - 1)
            return false;

        _pointer++;
        return true;
    }

    void IEnumerator.Reset()
    {
        _pointer = -1;
    }

    object? IEnumerator.Current => this[_pointer <= 0 ? 0 : _pointer];
}

public class EquipmentItems(Inventory items) : IReadOnlyDictionary<ItemType, short>
{
    public bool ContainsKey(ItemType key)
    {
        return items.GetEquipmentItemId(key) != 0;
    }

    public bool TryGetValue(ItemType key, out short value)
    {
        value = this[key];
        return value != 0;
    }

    public short this[ItemType type]
    {
        get => items.GetEquipmentItemId(type);
        set => items.SetEquipmentItemId(type, value);
    }

    public IEnumerable<ItemType> Keys => Enum.GetValues<ItemType>();

    public IEnumerable<short> Values => Keys.Select(type => this[type]).ToList();

    public IEnumerator<KeyValuePair<ItemType, short>> GetEnumerator()
    {
        return Keys.ToDictionary(k => k, k => this[k]).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public int Count => Keys.Count(type => this[type] != 0);
}
