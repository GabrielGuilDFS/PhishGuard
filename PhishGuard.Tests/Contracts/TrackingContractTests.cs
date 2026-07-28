using PhishGuard.Backend.Contracts;
using Xunit;

namespace PhishGuard.Tests.Contracts;

// Espelha PhishGuard.Backend/Contracts/. Valida o builder canônico da URL educacional
// — o mesmo formato do módulo TS src/shared/trackingContract.ts (Passo 12).
public class TrackingContractTests
{
    [Fact]
    public void EducationalFeedbackUrl_ComCampanhaEAlvo_UsaParametrosCanonicosCT()
    {
        var url = TrackingContract.EducationalFeedbackUrl(null, "camp-1", "tgt-2");

        Assert.Equal("/educational-feedback?c=camp-1&t=tgt-2", url);
        Assert.DoesNotContain("campaign=", url); // regressão §1.3d
    }

    [Fact]
    public void EducationalFeedbackUrl_ComTemplate_IncluiOsTresParametros()
    {
        var url = TrackingContract.EducationalFeedbackUrl("amzprime", "c1", "t1");

        Assert.Equal("/educational-feedback?template=amzprime&c=c1&t=t1", url);
    }

    [Fact]
    public void EducationalFeedbackUrl_SemNenhumParametro_RetornaSoORoteiro()
    {
        Assert.Equal("/educational-feedback", TrackingContract.EducationalFeedbackUrl(null, null, null));
    }

    [Fact]
    public void EducationalFeedbackUrl_OmiteCamposVazios()
    {
        // Sem alvo: só template e campanha entram na query.
        var url = TrackingContract.EducationalFeedbackUrl("x", "c1", "");
        Assert.Equal("/educational-feedback?template=x&c=c1", url);
    }
}
