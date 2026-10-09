using OrderService.Infrastructure.Auth;

namespace OrderService.Api.IntegrationTests;

public class Pbkdf2PasswordHasherTests
{
    private readonly Pbkdf2PasswordHasher hasher = new();

    [Fact]
    public void Verify_CorrectPassword_ReturnsTrue()
    {
        var hash = hasher.Hash("s3cret");

        Assert.True(hasher.Verify(hash, "s3cret"));
    }

    [Fact]
    public void Verify_WrongPassword_ReturnsFalse()
    {
        var hash = hasher.Hash("s3cret");

        Assert.False(hasher.Verify(hash, "other"));
    }

    [Fact]
    public void Hash_SamePasswordTwice_ProducesDifferentHashes()
    {
        Assert.NotEqual(hasher.Hash("s3cret"), hasher.Hash("s3cret"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("garbage")]
    [InlineData("v1.abc.def.ghi")]
    [InlineData("v2.1000.AAAA.AAAA")]
    public void Verify_MalformedHash_ReturnsFalse(string malformed)
    {
        Assert.False(hasher.Verify(malformed, "s3cret"));
    }

    [Theory]
    [InlineData("v1.600000.v/1H6x41FXYgiPv5Z5WXWQ==.abEgtNOuCopuMywr7rZs8javnZMEYJFhmLW7QGkiiV4=", "admin123")]
    [InlineData("v1.600000.Wth+s2v7HS1WrnlrNZN8Rg==.zxUtdxcycvnbOfhqX9BGTeoav9Q4rX7DyrCC2w8wL0M=", "customer123")]
    [InlineData("v1.600000.nmEmoJon3mBxxOuPr8boFg==.6bAC2WWeOxYuuQcnBasJkWrYkq1yW+UrwWBpW0z0+Ko=", "customer123")]
    public void Verify_SeededHashes_MatchDocumentedPasswords(string seededHash, string password)
    {
        Assert.True(hasher.Verify(seededHash, password));
    }
}
