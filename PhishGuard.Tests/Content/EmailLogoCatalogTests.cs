using System.Linq;
using PhishGuard.Backend.Content;
using Xunit;

namespace PhishGuard.Tests.Content;

// Regressão do embutimento das logos PNG anexadas via CID no disparo. O corpo da
// isca bho MAX referencia "cid:logo-bhomax" e o CampaignDispatchService anexa o PNG
// resolvido pelo EmailLogoCatalog. Se o recurso não estiver embutido/mapeado, o
// e-mail sai com a imagem quebrada — regressão silenciosa.
public class EmailLogoCatalogTests
{
    private static readonly byte[] PngSignature = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };

    [Theory]
    [InlineData("logo-bhomax")]
    [InlineData("logo-mercadoliv")]
    public void TryGetLogo_ComTokenConhecido_RetornaPngEmbutido(string token)
    {
        var ok = EmailLogoCatalog.TryGetLogo(token, out var fileName, out var bytes);

        Assert.True(ok, $"O token '{token}' deveria resolver para um PNG embutido.");
        Assert.EndsWith(".png", fileName);
        Assert.NotEmpty(bytes);
        Assert.True(bytes.Take(PngSignature.Length).SequenceEqual(PngSignature),
            "Os bytes deveriam começar com a assinatura de um arquivo PNG.");
    }

    [Fact]
    public void TryGetLogo_ComTokenDesconhecido_RetornaFalse()
    {
        var ok = EmailLogoCatalog.TryGetLogo("logo-inexistente", out _, out var bytes);

        Assert.False(ok);
        Assert.Empty(bytes);
    }

    [Fact]
    public void Tokens_ExpoeOsLogosConhecidos()
    {
        Assert.Contains("logo-bhomax", EmailLogoCatalog.Tokens);
        Assert.Contains("logo-mercadoliv", EmailLogoCatalog.Tokens);
    }
}
