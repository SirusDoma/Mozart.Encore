using System.Security.Cryptography;

namespace Encore.Services;

public interface IAuthSessionTokenGenerator
{
    string Generate();
}

public sealed class GuidAuthSessionTokenGenerator : IAuthSessionTokenGenerator
{
    public string Generate() => Guid.NewGuid().ToString().ToUpperInvariant();
}

public sealed class CompactGuidAuthSessionTokenGenerator : IAuthSessionTokenGenerator
{
    public string Generate() => Guid.NewGuid().ToString("N").ToUpperInvariant();
}

public sealed class NumericAuthSessionTokenGenerator : IAuthSessionTokenGenerator
{
    public string Generate()
    {
        return RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6") +
               RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
    }
}
