using System.Diagnostics.CodeAnalysis;

namespace Mozart.Messages;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public enum Difficulty : byte
{
    EX = 0,
    NX = 1,
    HX = 2,
    RX = 3
}