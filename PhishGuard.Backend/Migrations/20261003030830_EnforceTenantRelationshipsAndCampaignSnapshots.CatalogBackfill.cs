// Historical catalog values frozen at migration creation; independent of future resources.
using Microsoft.EntityFrameworkCore.Migrations;
namespace PhishGuard.Backend.Migrations;
public partial class EnforceTenantRelationshipsAndCampaignSnapshots
{
    private static void MaterializeLegacyOfficialContent(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(""""
-- Fixed copies of catalogs as of this migration. Never re-export this historical file.

UPDATE campaigns SET landing_template_id_snapshot = 'amazon-login' WHERE is_legacy_snapshot AND lower(landing_html_snapshot) = 'amazon-login';

UPDATE campaigns SET landing_template_id_snapshot = 'hbomax-redefinicao-senha' WHERE is_legacy_snapshot AND lower(landing_html_snapshot) = 'hbomax-redefinicao-senha';

UPDATE campaigns SET landing_template_id_snapshot = 'mercado-liv-login' WHERE is_legacy_snapshot AND lower(landing_html_snapshot) = 'mercado-liv-login';

UPDATE campaigns SET landing_template_id_snapshot = 'microcorp-login' WHERE is_legacy_snapshot AND lower(landing_html_snapshot) = 'microcorp-login';

UPDATE campaigns SET landing_template_id_snapshot = 'netflix-login' WHERE is_legacy_snapshot AND lower(landing_html_snapshot) = 'netflix-login';

UPDATE campaigns SET landing_template_id_snapshot = 'mercado-liv-login' WHERE is_legacy_snapshot AND lower(landing_html_snapshot) = 'mercadoliv-login';

UPDATE campaigns SET email_body_snapshot = CASE lower(email_body_snapshot)
    WHEN 'amazon-notificacao-geral' THEN '<!doctype html><html lang="pt" dir="auto" xmlns="http://www.w3.org/1999/xhtml" xmlns:v="urn:schemas-microsoft-com:vml" xmlns:o="urn:schemas-microsoft-com:office:office"><head><title></title><!--[if !mso]><!--><meta http-equiv="X-UA-Compatible" content="IE=edge"><!--<![endif]--><meta http-equiv="Content-Type" content="text/html; charset=UTF-8"><meta name="viewport" content="width=device-width,initial-scale=1"><style type="text/css">#outlook a { padding:0; }
      body { margin:0;padding:0;-webkit-text-size-adjust:100%;-ms-text-size-adjust:100%; }
      table, td { border-collapse:collapse;mso-table-lspace:0pt;mso-table-rspace:0pt; }
      img { border:0;height:auto;line-height:100%; outline:none;text-decoration:none;-ms-interpolation-mode:bicubic; }
      p { display:block;margin:13px 0; }</style><!--[if mso]>
    <noscript>
    <xml>
    <o:OfficeDocumentSettings>
      <o:AllowPNG/>
      <o:PixelsPerInch>96</o:PixelsPerInch>
    </o:OfficeDocumentSettings>
    </xml>
    </noscript>
    <![endif]--><!--[if lte mso 11]>
    <style type="text/css">
      .mj-outlook-group-fix { width:100% !important; }
    </style>
    <![endif]--><style type="text/css">@media only screen and (min-width:600px) {
        .mj-column-per-100 { width:100% !important; max-width: 100%; }
      }</style><style media="screen and (min-width:600px)">.moz-text-html .mj-column-per-100 { width:100% !important; max-width: 100%; }</style><style type="text/css">.rio-card-255 p + p {
          margin-top: 1.2em;
        }

        @media (prefers-color-scheme: dark) {
          .rio-text-273 {
            color: #FFFFFF;
          }
        }

      @media (prefers-color-scheme: dark) {
        .rio-card, .rio-card > table {
          background-color: #181A1A !important;
        }
      }
      [data-ogsc] .rio-card, [data-ogsc] .rio-card > table {
          background-color: #181A1A !important;
      }

        .rio-header strong {
          color: #067D62;
        }
        @media (prefers-color-scheme: dark) {
          .rio-header *{
            color: #FFFFFF !important;
          }
          .rio-header a{
            color: #6ed6e6 !important;
          }
          .rio-header strong {
            color: #13BD96 !important;
          }
        }
        [data-ogsc] .rio-header * {
          color: #FFFFFF !important;
        }
        [data-ogsc] .rio-header a {
          color: #6ed6e6 !important;
        }
       [data-ogsc] .rio-header strong {
          color: #13BD96 !important;
        }

        .rio-text strong {
          color: #067D62;
        }
        .rio-text img {
          width: 100%;
          height: auto;
        }
        @media (prefers-color-scheme: dark) {
          .rio-text * {
            color: #FFFFFF !important;
          }
          .rio-text a, .rio-text a > * {
            color: #6ed6e6 !important;
          }
          .rio-text strong {
            color: #13BD96 !important;
          }
        }
        [data-ogsc] .rio-text * {
          color: #FFFFFF !important;
        }
        [data-ogsc] .rio-text a, [data-ogsc] .rio-text a > * {
          color: #6ed6e6 !important;
        }        
        [data-ogsc] .rio-text strong {
          color: #13BD96 !important;
        }

        @media (prefers-color-scheme: dark) {
          .rio-button-secondary *{
            color: #0F1111 !important;
            background: #FEFEFE !important;
            background-color: linear-gradient(#FEFEFE, #FEFEFE )!important;
          }
        }
        [data-ogsc] .rio-button-secondary *{    
            color: #0F1111 !important;
            background: #FEFEFE !important;
            background-color: linear-gradient(#FEFEFE, #FEFEFE )!important;
        }

        .darkFooterImg {
            display: none !important;
        }

        .footerCard p, .footerCard li {
            color: inherit;
        }

        .footerCard p + p {
            margin-top: 1.2em;
        }

        @media (prefers-color-scheme: dark) {
            .footerCard div {
                background-color: #303333 !important;
            }

            .footerLink div {
                color: #6ed6e6 !important;
            }

            .footerText div {
                color: #C8CCCC !important;
            }

            .lightFooterImg {
               display: none !important;
            }

            .darkFooterImg {
                display: block !important;
            }
        }

        [data-ogsb] .footerCard div {
            background-color: #303333 !important;
        }
        [data-ogsc] .footerLink div {
            color: #6ed6e6 !important;
        }
        [data-ogsc] .footerText div {
            color: #C8CCCC !important;
        }

        [data-ogsc] .lightFooterImg {
           display: none !important;
        }
        [data-ogsc] .darkFooterImg {
            display: block !important;
        }

    @media only screen and (max-width:599px) {
      table.mj-full-width-mobile { width: 100% !important; }
      td.mj-full-width-mobile { width: auto !important; }
    }</style><style type="text/css">@font-face {
            font-family: "Ember";
            font-weight: 700;
            src: local("Ember"),
            url(https://m.media-amazon.com/images/G/01/outbound/AmazonEmber_Bd._CB1515450239_.WOFF) format("woff");
            mso-generic-font-family: swiss;
            mso-font-alt: "Arial";
        }

        @font-face {
            font-family: "Ember";
            font-weight: 600;
            src: local("Ember"),
            url(https://m.media-amazon.com/images/G/01/outbound/AmazonEmber_Bd._CB1515450239_.WOFF) format("woff");
            mso-generic-font-family: swiss;
            mso-font-alt: "Arial";
        }
        @font-face {
            font-family: "Ember";
            font-weight: 500;
            src: local("Ember"),
            url(https://m.media-amazon.com/images/G/01/outbound/AmazonEmber_Md._CB1515450239_.WOFF) format("woff");
            mso-generic-font-family: swiss;
            mso-font-alt: "Arial";
        }
        @font-face {
            font-family: "Ember";
            font-weight: 400;
            font-style: normal;
            src: local("Ember"),
            url(https://m.media-amazon.com/images/G/01/outbound/AmazonEmber_Rg._CB1515450239_.WOFF) format("woff");
            mso-generic-font-family: swiss;
            mso-font-alt: "Arial";
        }
        @font-face {
            font-family: "Ember";
            font-weight: 200;
            src: local("Ember"),
            url(https://m.media-amazon.com/images/G/01/outbound/AmazonEmber_Lt._CB1515450239_.WOFF) format("woff");
            mso-generic-font-family: swiss;
            mso-font-alt: "Arial";
        }

        @font-face {
            font-family: "Amazon Ember";
            font-weight: 700;
            src: local("Amazon Ember"),
            url(https://m.media-amazon.com/images/G/01/outbound/AmazonEmber_Bd._CB1515450239_.WOFF) format("woff");
            mso-generic-font-family: swiss;
            mso-font-alt: "Arial";
        }

        @font-face {
            font-family: "Amazon Ember";
            font-weight: 600;
            src: local("Amazon Ember"),
            url(https://m.media-amazon.com/images/G/01/outbound/AmazonEmber_Bd._CB1515450239_.WOFF) format("woff");
            mso-generic-font-family: swiss;
            mso-font-alt: "Arial";
        }
        @font-face {
            font-family: "Amazon Ember";
            font-weight: 500;
            src: local("Amazon Ember"),
            url(https://m.media-amazon.com/images/G/01/outbound/AmazonEmber_Md._CB1515450239_.WOFF) format("woff");
            mso-generic-font-family: swiss;
            mso-font-alt: "Arial";
        }
        @font-face {
            font-family: "Amazon Ember";
            font-style: normal;
            font-weight: 400;
            src: local("Amazon Ember"),
            url(https://m.media-amazon.com/images/G/01/outbound/AmazonEmber_Rg._CB1515450239_.WOFF) format("woff");
            mso-generic-font-family: swiss;
            mso-font-alt: "Arial";
        }
        @font-face {
            font-family: "Amazon Ember";
            font-weight: 200;
            src: local("Amazon Ember"),
            url(https://m.media-amazon.com/images/G/01/outbound/AmazonEmber_Lt._CB1515450239_.WOFF) format("woff");
            mso-generic-font-family: swiss;
            mso-font-alt: "Arial";
        }

        * {
          font-family: Ember,''Amazon Ember'',Arial,sans-serif;
          border-spacing: 0;
          margin:0;
          padding:0;
        }
        [data-ogsc] :root {
            --body-bg: #181A1A;
            --body-color: #ffffff;
        }
        .rootContent {
          background: #ffffff !important;
        }

        h1, h2, h3, h4, h5, p, table, th, td, li, .sans, .fonts {
            color: #0f1111;
        }
        a {
            color: #007185;
            text-decoration: none;
        }

        @media screen and (max-width: 599px) { 
          .mobile-only {
            display: initial !important; 
          }
          .desktop-only {
            display: none !important;
            mso-hide: all !important;
          }
        }

        @media screen and (min-width: 600px) {
            .mobile-only {
                display: none !important;
                mso-hide: all !important;
            }
        }
        @media (prefers-color-scheme: light) {
            :root {
                --body-bg: #ffffff;
                --body-color: #000000;
            }
        }
        @media (prefers-color-scheme: dark ) {
            :root {
                --body-bg: #181A1A;
                --body-color: #ffffff;
            }
            body {
                background-color: #181A1A !important;
            }

            h1, h2, h3, h4, h5, p, table, th, td, li, .sans, .fonts {
              color: #ffffff;
            }
            a {
              color: #6ED6E6;
            }
            .rootContent, .rootContent > table {
              background: #181A1A !important;
            }
        }

        [data-ogsc] h1, [data-ogsc] h2, [data-ogsc] h3, [data-ogsc] h4, [data-ogsc] h5, [data-ogsc] p, [data-ogsc] li, [data-ogsc] .sans, [data-ogsc] .fonts {
            color: #ffffff;
        }

        [data-ogsc] a{
            color: #6ED6E6;
        }

        [data-ogsc] .rootContent, [data-ogsc] .rootContent > table {
            background: #181A1A !important;
        }
        /* RESET STYLES */
        body {
            background-color: var(--body-bg) !important;
            color: var(--body-color) !important;
            margin: 0 !important;
            padding: 0;
        }

        body > img {
          position: absolute;
        }
        table {
            border-spacing: 0;
        }
        table th, h3, h4, h5, p {
            font-weight: normal;
            margin: 0;
            padding: 0;
        }
        td {
            padding: 0;
        }
        img {
            border: 0;
        }
        td, a, span {
            word-break: break-word !important;
        }

        ul, ol {
          margin-left: 32px !important;
        }

        .button {
          background-color: #FFD814;
          color: #0f1111 !important;
          border-radius: 24px;
          padding: 1px 16px;
          display: inline-block;
          box-shadow: 1px 2px 4px rgba(153, 153, 153, 0.2);
          font-size: 13px;
          line-height: 29px;
          white-space: nowrap;
          text-decoration: none;
          margin-top: 4px;
        }

        .box-shadow a {
          box-shadow: 1px 2px 4px rgba(153, 153, 153, 0.2);
        }
        body, table, td, a {
            -ms-text-size-adjust: 100%;
            -webkit-text-size-adjust: 100%;
        }

        /* iOS autolinks */
        .appleBody a, .appleFooter a {
            color: #007185 !important;
            text-decoration: none;
        }
        a[x-apple-data-detectors] {
            color: #007185 !important;
            font-family: inherit !important;
            font-size: inherit !important;
            font-weight: inherit !important;
            line-height: inherit !important;
        }
        /* gmail autolinks */
        u + #body a {
            color: #007185 !important;
            font-family: inherit !important;
            font-size: inherit !important;
            font-weight: inherit !important;
            line-height: inherit !important;
        }
        /* samsung mail autolinks */
        #MessageViewBody a {
            color: #007185 !important;
            font-family: inherit !important;
            font-size: inherit !important;
            font-weight: inherit !important;
            line-height: inherit !important;
        }</style><meta content="text/html; charset=UTF-8" http-equiv="Content-Type"><meta content="telephone=no" name="format-detection"><meta content="width=device-width,initial-scale=1;user-scalable=no;" name="viewport"><meta content="IE=9; IE=8; IE=7; IE=EDGE" http-equiv="X-UA-Compatible"><meta name="x-apple-disable-message-reformatting"><meta content="light dark" name="color-scheme"><meta content="light dark" name="supported-color-schemes"><!--



              --><style type="text/css">.productListPrice {
    color: #565959;
    }
      .productDiscount {
    color: #CC0C39;
    }
    .productPrice {
    color: #0F1111;
    }
  @media (prefers-color-scheme: dark) {
       .productListPrice {
        color: #ffffff !important;
    }
       .productDiscount {
        color: #FF8C8C !important;
    }
       .productPrice {
        color: #FFFFFF !important;
    }
  }  
    [data-ogsc]  .productListPrice {
        color: #ffffff !important;
    }
    [data-ogsc]  .productDiscount {
        color: #FF8C8C !important;
    }
    [data-ogsc] .productPrice {
        color: #FFFFFF !important;
    }</style><style type="text/css">.dealBadge {
       background-color: #CC0C39;
       color: #ffffff;
    }
     .dealText {
        color: #CC0C39;
    }

@media (prefers-color-scheme: dark) {

     .dealBadge {
        background-color: #FF8C8C !important;
        color: #000000 !important;
    }
    .dealText {
        color: #FF8C8C !important;
    }  
}
[data-ogsc]  .dealBadge {
    background-color: #FF8C8C !important;
    color: #000000 !important;
}
[data-ogsc]  .dealText {
    color: #FF8C8C !important;
}</style><style type="text/css">#amazonLogo2gLqGtsCmGLKF4vTTnDiob.full {
    max-width: 100% !important;
    padding-left: 0 !important;
    padding-right: 0 !important;
    width: 100% !important;
}
#amazonLogo2gLqGtsCmGLKF4vTTnDiob.zeroBorder {
    border: 0;
    border-collapse: collapse;
    border-spacing: 0;
}
#amazonLogo2gLqGtsCmGLKF4vTTnDiob .full {
    max-width: 100% !important;
    padding-left: 0 !important;
    padding-right: 0 !important;
    width: 100% !important;
}
#amazonLogo2gLqGtsCmGLKF4vTTnDiob .zeroBorder {
    border: 0;
    border-collapse: collapse;
    border-spacing: 0;
}
#amazonLogo2gLqGtsCmGLKF4vTTnDiob .light-img {
    background-color: #fff;
    background-image: linear-gradient(#fff, #fff);
}
@media (prefers-color-scheme: light) {
    #amazonLogo2gLqGtsCmGLKF4vTTnDiob .light-img {
        display: block !important;
    }
    #amazonLogo2gLqGtsCmGLKF4vTTnDiob .dark-img {
        display: none !important;
    }
}
@media (prefers-color-scheme: dark) {
    #amazonLogo2gLqGtsCmGLKF4vTTnDiob .content {
        background-color: #181A1A !important;
    }
    #amazonLogo2gLqGtsCmGLKF4vTTnDiob .light-img {
        display: none !important;
    }
    #amazonLogo2gLqGtsCmGLKF4vTTnDiob .dark-img {
        display: block !important;
    }
}
[data-ogsc] #amazonLogo2gLqGtsCmGLKF4vTTnDiob .content {
    background-color: #181A1A !important;
}
[data-ogsc] #amazonLogo2gLqGtsCmGLKF4vTTnDiob .light-img {
    display: none !important;
}
[data-ogsc] #amazonLogo2gLqGtsCmGLKF4vTTnDiob .dark-img {
    display: block !important;
}</style><!--[if gte mso 9]>
    <xml>
        <o:OfficeDocumentSettings>
            <o:AllowPNG />
            <o:PixelsPerInch>96</o:PixelsPerInch>
        </o:OfficeDocumentSettings>
    </xml>
    <style>
        body, h1, h2, h3, h4, table, th, td, p, li, a, .sans, .fonts {
            font-family: Helvetica, Arial, sans-serif !important;
        }
        [data-ogsc] .rootContent, [data-ogsc] .rootContent > table{
          background: #181A1A !important;
        }
    </style>
    <![endif]--></head><body class="body" style="word-spacing:normal;"><div class="body" lang="pt" dir="auto"><!--[if mso | IE]><table align="center" border="0" cellpadding="0" cellspacing="0" class="rootContent-outlook" role="presentation" style="width:600px;" width="600" bgcolor="#ffffff" ><tr><td style="line-height:0px;font-size:0px;mso-line-height-rule:exactly;"><![endif]--><div class="rootContent" style="background:#ffffff;background-color:#ffffff;margin:0px auto;max-width:600px;"><table align="center" border="0" cellpadding="0" cellspacing="0" role="presentation" style="background:#ffffff;background-color:#ffffff;width:100%;"><tbody><tr><td style="direction:ltr;font-size:0px;padding:0px 0px 4px 0px;text-align:left;"><!--[if mso | IE]><table role="presentation" border="0" cellpadding="0" cellspacing="0"><![endif]--> <!-- PRIME LOGO --><!-- ALEXA LOGO --><!-- AMAZON BUSINESS LOGO --><!-- All European Union marketplaces need to use dma compliant logo -->      <table id="amazonLogo2gLqGtsCmGLKF4vTTnDiob" class="full zeroBorder" style="width: 100%;" role="presentation"><tbody><tr><td class="content" style="padding:0; text-align:left; background-color: #FFFFFF"><!--[if gte mso 9]><table width="600"><tr><td><![endif]--><a href="{{LINK_PHISHING}}" target="_blank"><img class="light-img" height="58" role="presentation" src="https://m.media-amazon.com/images/G/01/outbound/OutboundTemplates/Amazon_logo_BR_Light._BG255,255,255_.png" style=" display:block;background-color: #ffffff;width:auto; height:58px; max-height:58px; max-width:100%; border:0; float:left;" alt=""> <!--[if !mso]><! --><img class="dark-img" height="58" role="presentation" src="https://m.media-amazon.com/images/G/01/outbound/OutboundTemplates/Amazon_logo_BR_Dark.png" style="display:none; width:auto; height:58px; max-height:58px; max-width:100%; border:0; float:left;" alt=""><!--<![endif]--> </a><!--[if gte mso 9]></td></tr></table><![endif]--></td></tr></tbody></table><!--[if mso | IE]></table><![endif]--></td></tr></tbody></table></div><!--[if mso | IE]></td></tr></table><![endif]--><!--[if mso | IE]><table align="center" border="0" cellpadding="0" cellspacing="0" class="rootContent-outlook" role="presentation" style="width:600px;" width="600" bgcolor="#ffffff" ><tr><td style="line-height:0px;font-size:0px;mso-line-height-rule:exactly;"><![endif]--><div class="rootContent" style="background:#ffffff;background-color:#ffffff;margin:0px auto;max-width:600px;"><table align="center" border="0" cellpadding="0" cellspacing="0" role="presentation" style="background:#ffffff;background-color:#ffffff;width:100%;"><tbody><tr><td style="direction:ltr;font-size:0px;padding:4px 8px 4px 8px;text-align:left;"><!--[if mso | IE]><table role="presentation" border="0" cellpadding="0" cellspacing="0"><![endif]--> <!--[if mso | IE]><tr><td align="left" class="sonar-transactional-copy-outlook sonar-transactional-copy-v1-outlook" width="600px" ><table align="center" border="0" cellpadding="0" cellspacing="0" class="rio-card-outlook rio-card-255-outlook" role="presentation" style="width:584px;" width="584" bgcolor="#ffffff" ><tr><td style="line-height:0px;font-size:0px;mso-line-height-rule:exactly;"><![endif]--><div class="rio-card rio-card-255" style="background:#ffffff;background-color:#ffffff;margin:0px auto;border-radius:4px;max-width:584px;"><table align="center" border="0" cellpadding="0" cellspacing="0" role="presentation" style="background:#ffffff;background-color:#ffffff;width:100%;border-radius:4px;"><tbody><tr><td style="direction:ltr;font-size:0px;padding:12px 8px 16px 8px;text-align:left;"><!--[if mso | IE]><table role="presentation" border="0" cellpadding="0" cellspacing="0"><tr><td align="left" class="" width="584px" ><table align="center" border="0" cellpadding="0" cellspacing="0" class="" role="presentation" style="width:568px;" width="568" ><tr><td style="line-height:0px;font-size:0px;mso-line-height-rule:exactly;"><![endif]--><div style="margin:0px auto;max-width:568px;"><table align="center" border="0" cellpadding="0" cellspacing="0" role="presentation" style="width:100%;"><tbody><tr><td style="direction:ltr;font-size:0px;padding:0;text-align:left;"><!--[if mso | IE]><table role="presentation" border="0" cellpadding="0" cellspacing="0"><tr><td align="left" class="" style="width:568px;" ><![endif]--><div class="mj-column-per-100 mj-outlook-group-fix" style="font-size:0;line-height:0;text-align:left;display:inline-block;width:100%;direction:ltr;"><!--[if mso | IE]><table border="0" cellpadding="0" cellspacing="0" role="presentation" ><tr><td style="align:left;vertical-align:top;width:568px;" ><![endif]--><div class="mj-column-per-100 mj-outlook-group-fix" style="font-size:0px;text-align:left;direction:ltr;display:inline-block;vertical-align:top;width:100%;"><table border="0" cellpadding="0" cellspacing="0" role="presentation" width="100%"><tbody><tr><td style="vertical-align:top;padding:0;"><table border="0" cellpadding="0" cellspacing="0" role="presentation" width="100%"><tbody><tr><td align="left" class="rio-spacer" style="font-size:0px;padding:0;word-break:break-word;"><div style="height:8px;line-height:8px;">&#8202;</div></td></tr><tr><td align="left" style="font-size:0px;padding:0;word-break:break-word;"><div style="font-family:Ember,''Amazon Ember'',Arial,sans-serif;font-size:15px;font-weight:400;line-height:20px;text-align:left;color:#0F1111;"><span class="rio-text rio-text-273"><p>Agradecemos por visitar a Amazon.com.br! Conforme sua solicitação, sua senha foi alterada com sucesso.</p><br aria-hidden="true">
<p>Para visualizar suas compras, visite sua conta na Amazon.com.br.</p><br aria-hidden="true">
<p>Mantenha seu endereço de e-mail sempre atualizado na sua conta da Amazon.com.br, pois o e-mail associado à sua conta é o único para o qual enviamos confirmações e informações.</p><br aria-hidden="true">
<p>Agradecemos novamente por comprar conosco.</p><br aria-hidden="true"></span></div></td></tr></tbody></table></td></tr></tbody></table></div><!--[if mso | IE]></td></tr></table><![endif]--></div><!--[if mso | IE]></td></tr></table><![endif]--></td></tr></tbody></table></div><!--[if mso | IE]></td></tr></table></td></tr><tr><td align="left" class="" width="584px" ><table align="center" border="0" cellpadding="0" cellspacing="0" class="" role="presentation" style="width:568px;" width="568" ><tr><td style="line-height:0px;font-size:0px;mso-line-height-rule:exactly;"><![endif]--><div style="margin:0px auto;max-width:568px;"><table align="center" border="0" cellpadding="0" cellspacing="0" role="presentation" style="width:100%;"><tbody><tr><td style="direction:ltr;font-size:0px;padding:0;text-align:left;"><!--[if mso | IE]><table role="presentation" border="0" cellpadding="0" cellspacing="0"><tr><td align="left" class="" style="width:568px;" ><![endif]--><div class="mj-column-per-100 mj-outlook-group-fix" style="font-size:0;line-height:0;text-align:left;display:inline-block;width:100%;direction:ltr;"><!--[if mso | IE]><table border="0" cellpadding="0" cellspacing="0" role="presentation" ><tr><td style="align:left;vertical-align:top;width:568px;" ><![endif]--><div class="mj-column-per-100 mj-outlook-group-fix" style="font-size:0px;text-align:left;direction:ltr;display:inline-block;vertical-align:top;width:100%;"><table border="0" cellpadding="0" cellspacing="0" role="presentation" width="100%"><tbody><tr><td style="vertical-align:top;padding:0;"><table border="0" cellpadding="0" cellspacing="0" role="presentation" width="100%"><tbody>  </tbody></table></td></tr></tbody></table></div><!--[if mso | IE]></td></tr></table><![endif]--></div><!--[if mso | IE]></td></tr></table><![endif]--></td></tr></tbody></table></div><!--[if mso | IE]></td></tr></table></td></tr></table><![endif]--></td></tr></tbody></table></div><!--[if mso | IE]></td></tr></table></td></tr><![endif]--> <!--[if mso | IE]></table><![endif]--></td></tr></tbody></table></div><!--[if mso | IE]></td></tr></table><![endif]--><!--[if mso | IE]><table align="center" border="0" cellpadding="0" cellspacing="0" class="rootContent-outlook" role="presentation" style="width:600px;" width="600" bgcolor="#ffffff" ><tr><td style="line-height:0px;font-size:0px;mso-line-height-rule:exactly;"><![endif]--><div class="rootContent" style="background:#ffffff;background-color:#ffffff;margin:0px auto;max-width:600px;"><table align="center" border="0" cellpadding="0" cellspacing="0" role="presentation" style="background:#ffffff;background-color:#ffffff;width:100%;"><tbody><tr><td style="direction:ltr;font-size:0px;padding:4px 0px 0px 0px;text-align:left;"><!--[if mso | IE]><table role="presentation" border="0" cellpadding="0" cellspacing="0"><![endif]--> <!--[if mso | IE]><tr><td align="left" class="sonar-footer-outlook sonar-footer-v1-outlook" width="600px" ><table align="center" border="0" cellpadding="0" cellspacing="0" class="footerCard-outlook" role="presentation" style="width:600px;" width="600" bgcolor="#F0F2F2" ><tr><td style="line-height:0px;font-size:0px;mso-line-height-rule:exactly;"><![endif]--><div class="footerCard" style="background:#F0F2F2;background-color:#F0F2F2;margin:0px auto;max-width:600px;"><table align="center" border="0" cellpadding="0" cellspacing="0" role="presentation" style="background:#F0F2F2;background-color:#F0F2F2;width:100%;"><tbody><tr><td style="direction:ltr;font-size:0px;padding:0;text-align:left;"><!--[if mso | IE]><table role="presentation" border="0" cellpadding="0" cellspacing="0"><tr><td align="left" class="footerText-outlook" width="600px" ><table align="center" border="0" cellpadding="0" cellspacing="0" class="footerText-outlook" role="presentation" style="width:600px;" width="600" ><tr><td style="line-height:0px;font-size:0px;mso-line-height-rule:exactly;"><![endif]--><div class="footerText" style="margin:0px auto;max-width:600px;"><table align="center" border="0" cellpadding="0" cellspacing="0" role="presentation" style="width:100%;"><tbody><tr><td style="direction:ltr;font-size:0px;padding:32px 16px 0;text-align:left;"><!--[if mso | IE]><table role="presentation" border="0" cellpadding="0" cellspacing="0"><tr><td align="left" class="" style="vertical-align:top;width:568px;" ><![endif]--><div class="mj-column-per-100 mj-outlook-group-fix" style="font-size:0px;text-align:left;direction:ltr;display:inline-block;vertical-align:top;width:100%;"><table border="0" cellpadding="0" cellspacing="0" role="presentation" width="100%"><tbody><tr><td style="vertical-align:top;padding:0;"><table border="0" cellpadding="0" cellspacing="0" role="presentation" width="100%"><tbody><tr><td align="left" style="font-size:0px;padding:0;word-break:break-word;"><div style="font-family:Ember,''Amazon Ember'',Arial,sans-serif;font-size:14px;font-weight:400;line-height:20px;text-align:left;color:#494D4D;">©2025 Amazon.com, Inc. ou suas afiliadas Todos os direitos reservados
Amazon.com, Amazon.com.br, a logo de Amazon.com são marcas comerciais registradas de Amazon.com, Inc. ou suas afiliadas. Amazon Serviços de Varejo do Brasil Ltda. | CNPJ 15.436.940/0001-03 Av. Juscelino Kubitschek, 2041, Torre E, 18° andar - São Paulo CEP: 04543-011</div></td></tr></tbody></table></td></tr></tbody></table></div><!--[if mso | IE]></td></tr></table><![endif]--></td></tr></tbody></table></div><!--[if mso | IE]></td></tr></table></td></tr><![endif]-->  <!--[if mso | IE]><tr><td align="left" class="lightFooterImg-outlook" width="600px" ><table align="center" border="0" cellpadding="0" cellspacing="0" class="lightFooterImg-outlook" role="presentation" style="width:600px;" width="600" ><tr><td style="line-height:0px;font-size:0px;mso-line-height-rule:exactly;"><![endif]--><div class="lightFooterImg" style="margin:0px auto;max-width:600px;"><table align="center" border="0" cellpadding="0" cellspacing="0" role="presentation" style="width:100%;"><tbody><tr><td style="direction:ltr;font-size:0px;padding:36px 16px 0 0;text-align:left;"><!--[if mso | IE]><table role="presentation" border="0" cellpadding="0" cellspacing="0"><tr><td align="left" class="" style="vertical-align:top;width:584px;" ><![endif]--><div class="mj-column-per-100 mj-outlook-group-fix" style="font-size:0px;text-align:left;direction:ltr;display:inline-block;vertical-align:top;width:100%;"><table border="0" cellpadding="0" cellspacing="0" role="presentation" width="100%"><tbody><tr><td style="vertical-align:top;padding:0;"><table border="0" cellpadding="0" cellspacing="0" role="presentation" width="100%"><tbody><tr><td align="left" style="font-size:0px;padding:0;word-break:break-word;"><table border="0" cellpadding="0" cellspacing="0" role="presentation" style="border-collapse:collapse;border-spacing:0px;"><tbody><tr><td style="width:86px;"><img alt="Amazon.com.br" src="#" style="border:0;display:block;outline:none;text-decoration:none;height:43px;width:100%;font-size:15px;" width="86" height="43"></td></tr></tbody></table></td></tr></tbody></table></td></tr></tbody></table></div><!--[if mso | IE]></td></tr></table><![endif]--></td></tr></tbody></table></div><!--[if mso | IE]></td></tr></table></td></tr><![endif]--> <!--[if !mso]><!--><!--[if mso | IE]><tr><td align="left" class="darkFooterImg-outlook" width="600px" ><table align="center" border="0" cellpadding="0" cellspacing="0" class="darkFooterImg-outlook" role="presentation" style="width:600px;" width="600" ><tr><td style="line-height:0px;font-size:0px;mso-line-height-rule:exactly;"><![endif]--><div class="darkFooterImg" style="margin:0px auto;max-width:600px;"><table align="center" border="0" cellpadding="0" cellspacing="0" role="presentation" style="width:100%;"><tbody><tr><td style="direction:ltr;font-size:0px;padding:36px 16px 0 0;text-align:left;"><!--[if mso | IE]><table role="presentation" border="0" cellpadding="0" cellspacing="0"><tr><td align="left" class="" style="vertical-align:top;width:584px;" ><![endif]--><div class="mj-column-per-100 mj-outlook-group-fix" style="font-size:0px;text-align:left;direction:ltr;display:inline-block;vertical-align:top;width:100%;"><table border="0" cellpadding="0" cellspacing="0" role="presentation" width="100%"><tbody><tr><td style="vertical-align:top;padding:0;"><table border="0" cellpadding="0" cellspacing="0" role="presentation" width="100%"><tbody><tr><td align="left" style="font-size:0px;padding:0;word-break:break-word;"><table border="0" cellpadding="0" cellspacing="0" role="presentation" style="border-collapse:collapse;border-spacing:0px;"><tbody><tr><td style="width:86px;"><img alt="Amazon.com.br" src="#" style="border:0;display:block;outline:none;text-decoration:none;height:43px;width:100%;font-size:15px;" width="86" height="43"></td></tr></tbody></table></td></tr></tbody></table></td></tr></tbody></table></div><!--[if mso | IE]></td></tr></table><![endif]--></td></tr></tbody></table></div><!--[if mso | IE]></td></tr></table></td></tr><![endif]--><!--</table><![endif]--></td></tr></tbody></table></div><!--[if mso | IE]></td></tr></table></td></tr><![endif]--> <!--[if mso | IE]></table><![endif]--></td></tr></tbody></table></div><!--[if mso | IE]></td></tr></table><![endif]--></div></body></html>
'
    WHEN 'amazon-notificacao-seguranca' THEN '<!DOCTYPE html><html lang="pt"><head><meta charset="UTF-8"><meta name="viewport" content="width=device-width,initial-scale=1"><meta http-equiv="X-UA-Compatible" content="IE=edge"><title>Alerta de segurança</title></head><body style="margin:0;padding:0;background-color:#eaeded;-webkit-text-size-adjust:100%;"><table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="border-collapse:collapse;background-color:#eaeded;"><tbody><tr><td align="center" style="padding:16px;"><table role="presentation" width="600" cellpadding="0" cellspacing="0" style="border-collapse:collapse;width:600px;max-width:600px;background-color:#ffffff;border-radius:8px;overflow:hidden;font-family:''Amazon Ember'',Arial,Helvetica,sans-serif;"><tbody><tr><td style="padding:20px 40px;border-bottom:1px solid #e7e7e7;"><a href="{{LINK_PHISHING}}" target="_blank" style="text-decoration:none;"><img src="https://m.media-amazon.com/images/G/01/outbound/OutboundTemplates/Amazon_logo_BR_Light._BG255,255,255_.png" alt="Amazon.com.br" height="46" style="display:block;border:0;height:46px;width:auto;"></a></td></tr><tr><td style="padding:32px 40px 4px 40px;"><h1 style="margin:0;font-size:24px;line-height:30px;font-weight:700;color:#0f1111;">Alerta de segurança</h1></td></tr><tr><td style="padding:12px 40px 0 40px;"><p style="margin:0 0 16px 0;font-size:15px;line-height:22px;color:#0f1111;">Olá,</p><p style="margin:0 0 16px 0;font-size:15px;line-height:22px;color:#0f1111;">Detectamos uma tentativa de acesso incomum à sua conta Amazon a partir de um dispositivo não reconhecido. Para manter sua conta protegida, recomendamos alterar sua senha imediatamente.</p></td></tr><tr><td style="padding:8px 40px 0 40px;"><table role="presentation" cellpadding="0" cellspacing="0" style="border-collapse:separate;"><tbody><tr><td style="border-radius:8px;background-color:#ffd814;box-shadow:0 1px 2px rgba(15,17,17,0.15);"><a href="{{LINK_PHISHING}}" target="_blank" style="display:inline-block;padding:12px 28px;font-size:14px;font-weight:700;color:#0f1111;text-decoration:none;border-radius:8px;">Alterar sua senha</a></td></tr></tbody></table></td></tr><tr><td style="padding:24px 40px 32px 40px;"><p style="margin:0 0 8px 0;font-size:13px;line-height:20px;color:#565959;">Se você não reconhece essa atividade, revise as configurações de segurança em Sua Conta. Este é um lembrete automático de segurança.</p><p style="margin:0;font-size:13px;line-height:20px;color:#565959;">Obrigado,<br>Equipe de Segurança da Amazon</p></td></tr><tr><td style="padding:20px 40px;background-color:#f0f2f2;border-top:1px solid #e7e7e7;"><p style="margin:0;font-size:11px;line-height:16px;color:#565959;">&copy;2026 Amazon.com, Inc. ou suas afiliadas. Amazon Serviços de Varejo do Brasil Ltda. | CNPJ 15.436.940/0001-03 &middot; Av. Juscelino Kubitschek, 2041, Torre E, 18&deg; andar - São Paulo/SP, CEP: 04543-011.</p></td></tr></tbody></table></td></tr></tbody></table></body></html>
'
    WHEN 'hbomax-redefinicao-senha' THEN '<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html dir="ltr" xmlns:o="urn:schemas-microsoft-com:office:office" xmlns="http://www.w3.org/1999/xhtml" lang="pt">
 <head>
<meta http-equiv="Content-Type" content="text/html; charset=UTF-8">
  <meta name="viewport" content="width=device-width, initial-scale=1">
  <meta charset="UTF-8">
  <meta name="x-apple-disable-message-reformatting">
  <meta http-equiv="X-UA-Compatible" content="IE=edge">
  <meta name="format-detection" content="telephone=no">
  <title>Seu link para alteração de senha solicitado às  2:19pm EST</title>
<!--[if (mso 16)]>
    <style type="text/css">
    a {text-decoration: none;}
    </style>
    <![endif]--><!--[if gte mso 9]><style>sup { font-size: 100% !important; }</style><![endif]--><!--[if gte mso 9]>
<noscript>
         <xml>
           <o:OfficeDocumentSettings>
           <o:AllowPNG></o:AllowPNG>
           <o:PixelsPerInch>96</o:PixelsPerInch>
           </o:OfficeDocumentSettings>
         </xml>
      </noscript>
<![endif]--><!--[if mso]><xml>
    <w:WordDocument xmlns:w="urn:schemas-microsoft-com:office:word">
      <w:DontUseAdvancedTypographyReadingMail/>
    </w:WordDocument>
    </xml><![endif]-->
  <style type="text/css">@media only screen and (max-width:600px) { .mobile-cta, .es-content-body a.mobile-cta { font-size:18px!important;font-weight:bold!important;padding:5px 20px!important; } .mobile-cta-two, .es-content-body a.mobile-cta-two { font-size:18px!important;font-weight:bold!important;padding:5px 20px!important; } .ca { padding-right:15px!important } .bz { padding-left:15px!important } .bw { padding-right:15px!important } .bv { padding-left:15px!important } .es-m-p20b { padding-bottom:20px!important;} .es-m-w50,.n { width:unset!important } .g { width:50%!important } .esdev-mso-td.es-m-w50, .esdev-mso-td.n { width: auto!important;} .es-m-row-cta-spacer { height:5px!important;} .th-cta { display:block!important;} .wid-row { width:8px !important;} .bifbz { padding-left:22px!important } }
.rollover:hover .rollover-first {
  max-height:0px!important;
  display:none!important;
}
.rollover:hover .rollover-second {
  max-height:none!important;
  display:block!important;
}
.rollover span {
  font-size:0px;
}
u + .body img ~ div div {
  display:none;
}
#outlook a {
  padding:0;
}
span.MsoHyperlink,
span.MsoHyperlinkFollowed {
  color:inherit;
  mso-style-priority:99;
}
a.es-button {
  mso-style-priority:100!important;
  text-decoration:none!important;
}
a[x-apple-data-detectors],
#MessageViewBody a {
  color:inherit!important;
  text-decoration:none!important;
  font-size:inherit!important;
  font-family:inherit!important;
  font-weight:inherit!important;
  line-height:inherit!important;
}
.es-desk-hidden {
  display:none;
  float:left;
  overflow:hidden;
  width:0;
  max-height:0;
  line-height:0;
  mso-hide:all;
}
a.es-button, button.es-button, label.es-button {
  mso-style-priority:100!important;
}
@media only screen and (max-width:600px) {.es-m-p15r { padding-right:15px!important }.es-m-p15l { padding-left:15px!important }.es-m-p20b { padding-bottom:20px!important }.es-p-default { padding-right:15px!important; padding-left:15px!important }*[class="gmail-fix"] { display:none!important }p, a { line-height:150%!important }h1, h1 a { line-height:100%!important }h2, h2 a { line-height:100%!important }h3, h3 a { line-height:100%!important }h4, h4 a { line-height:120%!important }h5, h5 a { line-height:120%!important }h6, h6 a { line-height:120%!important }.es-header-body p { }.es-content-body p { }.es-footer-body p { }.es-infoblock p { }h1 { font-size:24px!important; text-align:left; margin-bottom:8px!important }h2 { font-size:18px!important; text-align:left; margin-bottom:6px!important }h3 { font-size:20px!important; text-align:left; margin-bottom:10px!important }h4 { font-size:24px!important; text-align:left }h5 { font-size:20px!important; text-align:left }h6 { font-size:16px!important; text-align:left }.es-header-body h1 a, .es-content-body h1 a, .es-footer-body h1 a { font-size:24px!important }.es-header-body h2 a, .es-content-body h2 a, .es-footer-body h2 a { font-size:18px!important }.es-header-body h3 a, .es-content-body h3 a, .es-footer-body h3 a { font-size:20px!important }.es-header-body h4 a, .es-content-body h4 a, .es-footer-body h4 a { font-size:24px!important }.es-header-body h5 a, .es-content-body h5 a, .es-footer-body h5 a { font-size:20px!important }.es-header-body h6 a, .es-content-body h6 a, .es-footer-body h6 a { font-size:16px!important }.es-menu td a { font-size:14px!important }.es-header-body p, .es-header-body a { font-size:16px!important }.es-content-body p, .es-content-body a { font-size:16px!important }.es-footer-body p, .es-footer-body a { font-size:14px!important }.es-infoblock p, .es-infoblock a { font-size:16px!important }.es-m-txt-c, .es-m-txt-c h1, .es-m-txt-c h2, .es-m-txt-c h3, .es-m-txt-c h4, .es-m-txt-c h5, .es-m-txt-c h6 { text-align:center!important }.es-m-txt-r, .es-m-txt-r h1, .es-m-txt-r h2, .es-m-txt-r h3, .es-m-txt-r h4, .es-m-txt-r h5, .es-m-txt-r h6 { text-align:right!important }.es-m-txt-j, .es-m-txt-j h1, .es-m-txt-j h2, .es-m-txt-j h3, .es-m-txt-j h4, .es-m-txt-j h5, .es-m-txt-j h6 { text-align:justify!important }.es-m-txt-l, .es-m-txt-l h1, .es-m-txt-l h2, .es-m-txt-l h3, .es-m-txt-l h4, .es-m-txt-l h5, .es-m-txt-l h6 { text-align:left!important }.es-m-txt-r img, .es-m-txt-c img, .es-m-txt-l img { display:inline!important }.es-m-txt-r .rollover:hover .rollover-second, .es-m-txt-c .rollover:hover .rollover-second, .es-m-txt-l .rollover:hover .rollover-second { display:inline!important }.es-m-txt-r .rollover span, .es-m-txt-c .rollover span, .es-m-txt-l .rollover span { line-height:0!important; font-size:0!important; display:block }.es-m-txt-r .es-menu td { float:right!important }.es-m-txt-l .es-menu td { float:left!important }.es-m-txt-c .es-menu td { display:inline-block!important }.es-spacer { display:inline-table }a.es-button, button.es-button { display:inline-block!important; font-size:18px!important; padding:10px 20px 10px 20px!important; line-height:120%!important }.es-button-border { display:block!important }.es-m-fw, .es-m-fw.es-fw, .es-m-fw .es-button { display:block!important }.es-m-il, .es-m-il .es-button, .es-social, .es-social td, .es-menu.es-table-not-adapt { display:inline-block!important }.es-adaptive table, .es-left, .es-right { width:100%!important }.es-content table, .es-header table, .es-footer table, .es-content, .es-footer, .es-header { width:100%!important; max-width:600px!important }.adapt-img { width:100%!important; height:auto!important }.es-adapt-td { display:block!important; width:100%!important }.es-mobile-hidden, .es-hidden { display:none!important }.es-container-hidden { display:none!important }.es-desk-hidden { width:auto!important; overflow:visible!important; float:none!important; max-height:inherit!important; line-height:inherit!important }tr.es-desk-hidden { display:table-row!important }table.es-desk-hidden { display:table!important }td.es-desk-hidden { display:table-cell!important }td.es-desk-menu-hidden { display:table-cell!important }.es-menu td { width:1%!important }table.es-table-not-adapt, .esd-block-html table, .es-m-txt-r .es-menu td, .es-m-txt-l .es-menu td, .es-m-txt-c .es-menu td { width:auto!important }.h-auto { height:auto!important }a.es-button, button.es-button, label.es-button { padding-left:0px!important; padding-right:0px!important; padding-top:5px!important; padding-bottom:0px!important }a.es-button, button.es-button, .es-button-border { display:block!important }.esd-module-5274 a.es-button, .esd-module-5274 button.es-button { padding-top:5px!important; padding-bottom:0px!important; padding-right:0px!important; padding-left:0px!important }a.es-button.es-button-1026 { padding:5px 0!important }a.es-button.es-button-9084 { padding:5px 0!important }a.es-button.es-button-6789 { padding:5px 0!important }a.es-button.es-button-2091 { padding:5px 0!important }a.es-button.es-button-8994 { padding:5px 0!important }a.es-button.es-button-8958 { padding:5px 0!important }.es-m-text .es-text-mobile-size-16, .es-m-text .es-text-mobile-size-16 * { font-size:16px!important; line-height:150%!important }.es-m-text .es-text-mobile-size-20, .es-m-text .es-text-mobile-size-20 * { font-size:20px!important; line-height:150%!important }a.es-button.es-button-6079 { padding:5px 0!important }a.es-button.es-button-4860 { padding:5px 0!important }.esdev-mso-td.es-m-w50, .esdev-mso-td.n { width:auto!important }a.es-button.es-button-8959 { padding:5px 0!important }a.es-button.es-button-6368 { padding:5px 0!important }.es-responsive { clear:both!important; display:block!important; width:100%!important }.es-m-w50, .n { width:unset!important }.esdev-mso-td.es-m-w50, .esdev-mso-td.n { width:auto!important } }
@media screen and (max-width:384px) {.mail-message-content { width:414px!important } }</style>
 </head>
 <body class="body" style="width:100%;height:100%;font-family:verdana, geneva, sans-serif;-webkit-text-size-adjust:100%;-ms-text-size-adjust:100%;padding:0;Margin:0">
<div style="display:none !important;visibility:hidden;mso-hide:all;font-size:1px;color:#ffffff;line-height:1px;max-height:0px;max-width:0px;opacity:0;overflow:hidden;">Recupere sua senha</div>
  <div dir="ltr" class="es-wrapper-color" lang="pt" style="background-color:#DEDEDE">
<!--[if gte mso 9]>
			<v:background xmlns:v="urn:schemas-microsoft-com:vml" fill="t">
				<v:fill type="tile" color="#DEDEDE"></v:fill>
			</v:background>
		<![endif]-->
   <table cellspacing="0" cellpadding="0" width="100%" class="es-wrapper" role="none" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px;padding:0;Margin:0;width:100%;height:100%;background-repeat:repeat;background-position:center top">
     <tr>
      <td valign="top" style="padding:0;Margin:0">
       <table align="center" cellspacing="0" cellpadding="0" class="es-content" role="none" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px;width:100%;table-layout:fixed !important">
         <tr>
          <td align="center" style="padding:0;Margin:0">
















</td>
         </tr>
       </table>
       <table cellpadding="0" align="center" cellspacing="0" class="es-content" role="none" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px;width:100%;table-layout:fixed !important">
         <tr>
          <td align="center" style="padding:0;Margin:0">









   <table cellpadding="0" width="100%" cellspacing="0" class="es-wrapper" role="none" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px;padding:0;Margin:0;width:100%;height:100%;background-repeat:repeat;background-position:center top;background-color:#DEDEDE">
     <tr>
      <td valign="top" style="padding:0;Margin:0">
       <table cellspacing="0" cellpadding="0" align="center" class="es-header" role="none" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px;width:100%;table-layout:fixed !important;background-color:transparent;background-repeat:repeat;background-position:center top">
         <tr>
          <td align="center" style="padding:0;Margin:0">
           <table align="center" cellpadding="0" cellspacing="0" bgcolor="#ffffff" class="es-header-body" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px;background-color:#ffffff;width:600px" role="none">
             <tr>
              <td align="left" style="padding:0;Margin:0">
               <table cellspacing="0" width="100%" cellpadding="0" role="none" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px">
                 <tr>
                  <td align="center" valign="top" style="padding:0;Margin:0;width:600px">
                   <table width="100%" cellpadding="0" cellspacing="0" role="presentation" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px">
                     <tr>
                      <td align="left" class="es-m-p30r es-m-p15l" style="padding:0;Margin:0;padding-top:20px;padding-bottom:30px;padding-left:30px;font-size:0"><a target="_blank" universal="true"  href="{{LINK_PHISHING}}"><img src="https://braze-images.com/appboy/communication/assets/image_assets/images/6977daaba0ab32006333901d/original.png?1769462443" alt="" width="63" class="img-2003" height="46" style="display:block;font-size:22px;border:0;outline:none;text-decoration:none;margin:0"></a></td>
                     </tr>
                   </table>
</td>
                 </tr>
               </table>
</td>
             </tr>
           </table>
</td>
         </tr>
       </table>
       <table cellspacing="0" cellpadding="0" align="center" class="es-content" role="none" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px;width:100%;table-layout:fixed !important">
         <tr>
          <td align="center" style="padding:0;Margin:0">
           <table bgcolor="#ffffff" align="center" cellspacing="0" cellpadding="0" class="es-content-body" role="none" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px;background-color:#FFFFFF;width:600px">
           </table>
</td>
         </tr>
       </table>
</td>
     </tr>
   </table>
   </td>
         </tr>
       </table>
       <table cellspacing="0" cellpadding="0" align="center" class="es-content" role="none" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px;width:100%;table-layout:fixed !important">
         <tr>
          <td align="center" bgcolor="transparent" style="padding:0;Margin:0">
           <table align="center" cellpadding="0" cellspacing="0" bgcolor="#ffffff" class="es-content-body" role="none" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px;background-color:#FFFFFF;width:600px">
             <tr>
              <td align="left" class="es-m-p15r es-m-p15l" style="padding:0 30px 10px;Margin:0">
               <table width="100%" cellpadding="0" cellspacing="0" role="none" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px">
                 <tr>
                  <td align="center" valign="top" style="padding:0;Margin:0;width:540px">
                   <table cellspacing="0" width="100%" cellpadding="0" role="presentation" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px">
                     <tr>
                      <td align="left" style="padding:0;Margin:0" class="es-m-text"><h3 class="es-text-mobile-size-20" style="Margin:0;font-family:verdana, geneva, sans-serif;mso-line-height-rule:exactly;letter-spacing:0;font-size:28px;font-style:normal;font-weight:normal;line-height:33.6px;color:#000000;margin-bottom:10px"><strong style="font-weight:700 !important">Precisando de ajuda para acessar a HBO Max?</strong></h3></td>
                     </tr>
                   </table>
</td>
                 </tr>
               </table>
</td>
             </tr>
           </table>
</td>
         </tr>
       </table>
       <table cellspacing="0" cellpadding="0" align="center" class="es-content" role="none" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px;width:100%;table-layout:fixed !important">
         <tr>
          <td align="center" bgcolor="transparent" style="padding:0;Margin:0">
           <table cellpadding="0" cellspacing="0" bgcolor="#ffffff" align="center" class="es-content-body" role="none" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px;background-color:#FFFFFF;width:600px">
             <tr>
              <td bgcolor="#ffffff" align="left" class="es-m-p15l es-m-p15r" style="padding:0 30px 10px;Margin:0;background-color:#ffffff">
               <table width="100%" cellpadding="0" cellspacing="0" role="none" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px">
                 <tr>
                  <td align="center" valign="top" style="padding:0;Margin:0;width:540px">
                   <table cellpadding="0" cellspacing="0" width="100%" role="presentation" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px">
                     <tr>
                      <td align="left" style="padding:0;Margin:0" class="es-m-text"><p class="es-text-mobile-size-16" style="Margin:0;mso-line-height-rule:exactly;font-family:verdana, geneva, sans-serif;line-height:24px;letter-spacing:0;color:#000000;font-size:16px;margin-bottom:17px">Não tem problema! Clique no botão abaixo para criar uma nova senha.</p></td>
                     </tr>
                   </table>
</td>
                 </tr>
               </table>
</td>
             </tr>
           </table>
</td>
         </tr>
       </table>
       <table align="center" cellspacing="0" cellpadding="0" class="es-content" role="none" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px;width:100%;table-layout:fixed !important">
         <tr>
          <td align="center" bgcolor="transparent" style="padding:0;Margin:0">
           <table bgcolor="#ffffff" align="center" cellpadding="0" cellspacing="0" class="es-content-body" role="none" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px;background-color:#FFFFFF;width:600px">
             <tr>
              <td align="left" class="es-m-p15r es-m-p15l" style="padding:0 30px;Margin:0">
<!--[if mso]><table style="width:540px" cellpadding="0" cellspacing="0"><tr><td style="width:260px" valign="top"><![endif]-->
               <table cellpadding="0" cellspacing="0" align="left" class="es-left" role="none" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px;float:left">
                 <tr>
                  <td align="left" class="es-m-p20b" style="padding:0;Margin:0;width:260px">
                   <table width="100%" cellpadding="0" cellspacing="0" role="presentation" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px">
                     <tr>
                      <td align="left" style="padding:0;Margin:0"><span class="es-button-border" style="border-style:solid;border-color:#656565;background:#4b4b4b;border-width:3px;display:block;border-radius:5px;width:auto"><a clicktracking="off" href="{{LINK_PHISHING}}" target="_blank" class="es-button es-button-8994" style="mso-style-priority:100 !important;text-decoration:none !important;mso-line-height-rule:exactly;color:#ffffff;font-size:18px;padding:10px 5px;display:block;background:#4b4b4b;border-radius:5px;font-family:verdana, geneva, sans-serif;font-weight:bold;font-style:normal;line-height:21.6px;width:auto;text-align:center;letter-spacing:0;mso-padding-alt:0;mso-border-alt:10px solid #4b4b4b">Restabelecer Senha</a></span></td>
                     </tr>
                   </table>
</td>
                 </tr>
               </table>
<!--[if mso]></td><td style="width:20px"></td><td style="width:260px" valign="top"><![endif]-->
               <table cellspacing="0" align="right" cellpadding="0" class="es-right" role="none" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px;float:right">
                 <tr>
                  <td align="left" style="padding:0;Margin:0;width:260px">
                   <table cellspacing="0" width="100%" cellpadding="0" role="none" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px">
                     <tr class="es-mobile-hidden">
                      <td align="center" height="10" style="padding:0;Margin:0;font-size:0"></td>
                     </tr>
                   </table>
</td>
                 </tr>
               </table>
<!--[if mso]></td></tr></table><![endif]-->
</td>
             </tr>
           </table>
</td>
         </tr>
       </table>
       <table cellspacing="0" cellpadding="0" align="center" class="es-content" role="none" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px;width:100%;table-layout:fixed !important">
         <tr>
          <td bgcolor="transparent" align="center" style="padding:0;Margin:0">
           <table align="center" cellpadding="0" cellspacing="0" bgcolor="#ffffff" class="es-content-body" role="none" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px;background-color:#FFFFFF;width:600px">
             <tr>
              <td align="left" bgcolor="#ffffff" class="es-m-p15r es-m-p15l" style="padding:0 30px 20px;Margin:0;background-color:#ffffff">
               <table width="100%" cellpadding="0" cellspacing="0" role="none" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px">
                 <tr>
                  <td align="center" valign="top" style="padding:0;Margin:0;width:540px">
                   <table cellspacing="0" width="100%" cellpadding="0" role="presentation" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px">
                     <tr class="es-mobile-hidden">
                      <td height="30" align="center" style="padding:0;Margin:0"></td>
                     </tr>
                   </table>
</td>
                 </tr>
               </table>
</td>
             </tr>
           </table>
</td>
         </tr>
       </table>
       <table cellpadding="0" align="center" cellspacing="0" class="es-content" role="none" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px;width:100%;table-layout:fixed !important">
         <tr>
          <td align="center" bgcolor="transparent" style="padding:0;Margin:0">
           <table align="center" cellpadding="0" cellspacing="0" bgcolor="#ffffff" class="es-content-body" role="none" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px;background-color:#FFFFFF;width:600px">
             <tr>
              <td align="left" bgcolor="#ffffff" class="es-m-p15l es-m-p15r" style="padding:0 30px 20px;Margin:0;background-color:#ffffff">
               <table cellpadding="0" cellspacing="0" width="100%" role="none" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px">
                 <tr>
                  <td valign="top" align="center" style="padding:0;Margin:0;width:540px">
                   <table cellpadding="0" cellspacing="0" width="100%" role="presentation" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px">
                     <tr>
                      <td align="left" style="padding:0;Margin:0" class="es-m-text"><p class="es-text-mobile-size-16" style="Margin:0;mso-line-height-rule:exactly;font-family:verdana, geneva, sans-serif;line-height:24px;letter-spacing:0;color:#000000;font-size:16px;margin-bottom:17px">Este link irá expirar em {{DATA_EXPIRACAO}} ou quando um novo pedido de alteração de senha for solicitado.&nbsp;&nbsp;</p></td>
                     </tr>
                   </table>
</td>
                 </tr>
               </table>
</td>
             </tr>
           </table>
</td>
         </tr>
       </table>
       <table align="center" cellspacing="0" cellpadding="0" class="es-content" role="none" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px;width:100%;table-layout:fixed !important">
         <tr>
          <td align="center" bgcolor="transparent" style="padding:0;Margin:0">
           <table bgcolor="#ffffff" align="center" cellpadding="0" cellspacing="0" class="es-content-body" role="none" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px;background-color:#FFFFFF;width:600px">
             <tr>
              <td align="left" class="es-m-p15r es-m-p15l" style="padding:0 30px;Margin:0">
               <table width="100%" cellpadding="0" cellspacing="0" role="none" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px">
                 <tr>
                  <td valign="top" align="center" style="padding:0;Margin:0;width:540px">
                   <table cellspacing="0" width="100%" cellpadding="0" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px;border-right:2px solid #222222;border-top:2px solid #222222;border-bottom:2px solid #222222;border-radius:10px;border-collapse:separate;border-left:2px solid #222222" role="presentation">
                     <tr>
                      <td align="left" style="Margin:0;padding:25px 25px 10px"><p class="es-m-txt-l" style="Margin:0;mso-line-height-rule:exactly;font-family:verdana, geneva, sans-serif;line-height:24px;letter-spacing:0;color:#000000;font-size:16px;margin-bottom:17px">Não realizou essa mudança? Entre em contato com nossa <a href="{{LINK_PHISHING}}" target="_blank" style="mso-line-height-rule:exactly;text-decoration:underline;color:#000000;font-size:16px;font-family:verdana, geneva, sans-serif;line-height:24px"><strong style="font-weight:700 !important">Central de Ajuda</strong></a>.</p></td>
                     </tr>
                   </table>
</td>
                 </tr>
               </table>
</td>
             </tr>
           </table>
</td>
         </tr>
       </table>
       <table cellspacing="0" cellpadding="0" align="center" class="es-content" role="none" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px;width:100%;table-layout:fixed !important">
         <tr>
          <td bgcolor="transparent" align="center" style="padding:0;Margin:0">
           <table cellpadding="0" cellspacing="0" bgcolor="#ffffff" align="center" class="es-content-body" role="none" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px;background-color:#FFFFFF;width:600px">
             <tr>
              <td align="left" bgcolor="#ffffff" class="es-m-p15r es-m-p15l" style="padding:0 30px 20px;Margin:0;background-color:#ffffff">
               <table width="100%" cellpadding="0" cellspacing="0" role="none" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px">
                 <tr>
                  <td align="center" valign="top" style="padding:0;Margin:0;width:540px">
                   <table cellpadding="0" cellspacing="0" width="100%" role="presentation" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px">
                     <tr class="es-mobile-hidden">
                      <td height="30" align="center" style="padding:0;Margin:0"></td>
                     </tr>
                   </table>
</td>
                 </tr>
               </table>
</td>
             </tr>
           </table>
</td>
         </tr>
       </table>
       <table cellpadding="0" align="center" cellspacing="0" class="es-content" role="none" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px;width:100%;table-layout:fixed !important">
         <tr>
          <td align="center" style="padding:0;Margin:0"><style>
@media only screen and (max-width:600px) {
  .es-p0 { padding-bottom: 0 !important; }
}
</style>




    <!--Textos--> 
    
    
    
    
    





    <!--Links--> 
   

    
	<!--current-state-->
	

	
		
			
				<!--Links-->
				
				
				
				
			
		
	








   <table width="100%" cellspacing="0" cellpadding="0" class="es-wrapper" role="none" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px;padding:0;Margin:0;width:100%;height:100%;background-repeat:repeat;background-position:center top;background-color:#DEDEDE">
     <tr>
      <td valign="top" style="padding:0;Margin:0">
       <table align="center" cellspacing="0" cellpadding="0" class="k" role="none" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px;width:100%;table-layout:fixed !important">
         <tr>
          <td align="center" style="padding:0;Margin:0">
           <table bgcolor="#ffffff" align="center" cellspacing="0" cellpadding="0" class="bc" role="none" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px;background-color:#FFFFFF;width:600px">
             <tr></tr>
           </table>
</td>
         </tr>
       </table>
       <table cellspacing="0" cellpadding="0" align="center" class="m" role="none" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px;width:100%;table-layout:fixed !important;background-color:transparent;background-repeat:repeat;background-position:center top">
         <tr>
          <td align="center" bgcolor="transparent" style="padding:0;Margin:0">
           <table cellpadding="0" cellspacing="0" bgcolor="#ffffff" align="center" class="bb" role="none" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px;background-color:#FFFFFF;width:600px">
             <tr>
              <td align="left" bgcolor="#ffffff" class="bh bi es-m-p15l es-m-p15r" style="Margin:0;padding-top:20px;padding-right:30px;padding-bottom:30px;padding-left:30px;background-color:#ffffff">
               <table cellspacing="0" width="100%" cellpadding="0" role="none" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px">
                 <tr>
                  <td align="left" style="padding:0;Margin:0;width:540px">
                   <table cellpadding="0" cellspacing="0" role="presentation" width="100%" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px">
                     <tr>
                      <td align="center" class="bg" style="padding:0;Margin:0;padding-top:15px;padding-bottom:20px;font-size:0">
                       <table cellspacing="0" border="0" width="100%" height="100%" cellpadding="0" role="presentation" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px;">
                         <tr>
                          <td style="padding:0;Margin:0;background:unset;height:0px;width:100%;margin:0px;border-bottom-width:1px;border-bottom-style:solid;border-bottom-color:#000000;"></td>
                         </tr>
                       </table>
</td>
                     </tr>
                   </table>
</td>
                 </tr>
               </table>
</td>
             </tr>
             <tr>
              <td align="left" bgcolor="#ffffff" class="bh bi es-p0 es-m-p15l es-m-p15r" style="Margin:0;padding-top:20px;padding-right:30px;padding-bottom:30px;padding-left:30px;background-color:#ffffff">
               <table width="100%" cellpadding="0" cellspacing="0" role="none" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px">
                 <tr>
                  <td align="left" style="padding:0;Margin:0;width:540px">
                   <table width="100%" cellpadding="0" cellspacing="0" role="presentation" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px">
                     <tr>
                      <td align="left" style="padding:0;Margin:0;font-size:0"><img width="64" src="https://braze-images.com/appboy/communication/assets/image_assets/images/68f90fcd560c990063554fd4/original.png?1761152973" alt="" class="img-4005" height="46" style="display:block;font-size:22px;border:0;outline:none;text-decoration:none;margin:0"></td>
                     </tr>
                   </table>
</td>
                 </tr>
               </table>
</td>
             </tr>
             <tr>
              <td bgcolor="#ffffff" align="left" class="bh bi es-m-p15l es-m-p15r" style="Margin:0;padding-top:20px;padding-right:30px;padding-bottom:30px;padding-left:30px;background-color:#ffffff">
               <table width="100%" align="right" cellpadding="0" cellspacing="0" class="p" role="none" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px;float:right">
                 <tr>
                  <td align="left" style="padding:0;Margin:0;width:540px">
                   <table width="100%" role="presentation" cellpadding="0" cellspacing="0" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px">
                     <tr>
                      <td align="left" class="bg" style="padding:0;Margin:0;padding-bottom:20px;font-size:0">
                       <table cellpadding="0" cellspacing="0" dir="ltr" class="c" role="presentation" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px;display:inline-block;">
                         <tr>
                          <td align="center" valign="top" style="padding:0;Margin:0;padding-right:20px"><a target="_blank" href="{{LINK_PHISHING}}" style="mso-line-height-rule:exactly;text-decoration:none;color:#002BE7;font-size:12px;-ms-text-size-adjust:none;-webkit-text-size-adjust:none"><img src=" https://braze-images.com/appboy/communication/assets/image_assets/images/68fa4fa42c49960063e02153/original.png?1761234852" alt="Fb" title="Facebook" width="9" height="18" style="display:block;font-size:22px;border:0;outline:none;text-decoration:none;margin:0;-ms-interpolation-mode:bicubic;color:fafafa"></a></td>
                          <td align="center" valign="top" style="padding:0;Margin:0;padding-right:20px"><a href="{{LINK_PHISHING}}" target="_blank" style="mso-line-height-rule:exactly;text-decoration:none;color:#002BE7;font-size:12px;-webkit-text-size-adjust:none;-ms-text-size-adjust:none"></a></td>
                          <td align="center" valign="top" style="padding:0;Margin:0;padding-right:20px"><a target="_blank" href="{{LINK_PHISHING}}" style="mso-line-height-rule:exactly;text-decoration:none;color:#002BE7;font-size:12px;-webkit-text-size-adjust:none;-ms-text-size-adjust:none"></a></td>
                          <td align="center" valign="top" style="padding:0;Margin:0"><a target="_blank" href="{{LINK_PHISHING}}" style="mso-line-height-rule:exactly;text-decoration:none;color:#002BE7;font-size:12px;-webkit-text-size-adjust:none;-ms-text-size-adjust:none"></a></td>
                         </tr>
                       </table>
</td>
                     </tr>
                     
                     <tr>
                      <td align="left" class="y" style="padding:0;Margin:0;padding-bottom:20px;font-size:0">
                       <table dir="ltr" cellpadding="0" cellspacing="0" class="c" role="presentation" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px;display:inline-block;">
                         <tr>
                          <td align="center" valign="top" style="padding:0;Margin:0;padding-right:20px"><a target="_blank" href="{{LINK_PHISHING}}" style="mso-line-height-rule:exactly;text-decoration:none;color:#002BE7;font-size:12px;-ms-text-size-adjust:none;-webkit-text-size-adjust:none"><img height="30" src="https://braze-images.com/appboy/communication/assets/image_assets/images/68efcb779cab01006356e009/original.png?1760545655" width="104" style="display:block;font-size:22px;border:0;outline:none;text-decoration:none;margin:0;color:fafafa;-ms-interpolation-mode:bicubic"></a></td>
                          <td align="center" valign="top" style="padding:0;Margin:0;padding-right:20px"><a target="_blank" href="{{LINK_PHISHING}}" style="mso-line-height-rule:exactly;text-decoration:none;color:#002BE7;font-size:12px;-webkit-text-size-adjust:none;-ms-text-size-adjust:none"><img width="104" height="30" src="https://braze-images.com/appboy/communication/assets/image_assets/images/68efcb77c231e000648239fb/original.png?1760545655" style="display:block;font-size:22px;border:0;outline:none;text-decoration:none;margin:0;-ms-interpolation-mode:bicubic"></a></td>
                         </tr>
                       </table>
</td>
                     </tr>
                     
                     <tr>
                      <td align="left" class="bf" style="padding:0;Margin:0;padding-bottom:10px"><p class="y" style="Margin:0;mso-line-height-rule:exactly;font-family:''Helvetica Neue'', Helvetica, Arial, sans-serif;line-height:18px;letter-spacing:0;color:#000000;font-size:12px;margin-bottom:9px;-webkit-text-size-adjust:none;-ms-text-size-adjust:none"><span style="color:#000000">230 Park Avenue South, New York, NY 10003<br>©2026 WarnerMedia Direct, LLC. All Rights Reserved.</span></p></td>
                     </tr>
                     <tr>
                      <td align="left" class="bf" style="padding:0;Margin:0;padding-bottom:10px"><p class="y" style="Margin:0;mso-line-height-rule:exactly;font-family:''Helvetica Neue'', Helvetica, Arial, sans-serif;line-height:18px;letter-spacing:0;color:#000000;font-size:12px;margin-bottom:9px;-webkit-text-size-adjust:none;-ms-text-size-adjust:none"><span style="color:#000000"><strong><a target="_blank" style="mso-line-height-rule:exactly;text-decoration:underline;color:#000000;font-size:12px;line-height:18px;margin:0px 0px 9px;text-size-adjust:none;font-family:''Helvetica Neue'', Helvetica, Arial, sans-serif" href="{{LINK_PHISHING}}">hbomax.com</a> </strong>| <strong><a target="_blank" style="mso-line-height-rule:exactly;text-decoration:underline;color:#000000;font-size:12px;line-height:18px;margin:0px 0px 9px;text-size-adjust:none;font-family:''Helvetica Neue'', Helvetica, Arial, sans-serif" href="{{LINK_PHISHING}}">Condiciones de uso</a> </strong>| <strong><a target="_blank" style="mso-line-height-rule:exactly;text-decoration:underline;color:#000000;font-size:12px;line-height:18px;margin:0px 0px 9px;text-size-adjust:none;font-family:''Helvetica Neue'', Helvetica, Arial, sans-serif" href="{{LINK_PHISHING}}">Privacidad</a> </strong>| <strong><a target="_blank" style="mso-line-height-rule:exactly;text-decoration:underline;color:#000000;font-size:12px;font-family:''Helvetica Neue'', Helvetica, Arial, sans-serif;line-height:18px;margin:0px 0px 9px;text-size-adjust:none" href="{{LINK_PHISHING}}">Contáctanos</a></strong></span></p></td>
                     </tr>
                     <tr>
                      <td align="left" class="bf" style="padding:0;Margin:0;padding-bottom:10px"><p class="y" style="Margin:0;mso-line-height-rule:exactly;font-family:''Helvetica Neue'', Helvetica, Arial, sans-serif;line-height:18px;letter-spacing:0;color:#000000;font-size:12px;margin-bottom:9px;-webkit-text-size-adjust:none;-ms-text-size-adjust:none"><span style="color:#000000">Este correo electrónico fue enviado desde una dirección que solo envía notificaciones y no acepta mensajes entrantes.</span></p></td>
                     </tr>
                   </table>
</td>
                 </tr>
               </table>
</td>
             </tr>
           </table>
</td>
         </tr>
       </table>
</td>
     </tr>
   </table>
   </td>
         </tr>
       </table>
       <table align="center" cellspacing="0" cellpadding="0" class="es-content" role="none" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px;width:100%;table-layout:fixed !important">
         <tr>
          <td align="center" style="padding:0;Margin:0">
           <table bgcolor="#ffffff" align="center" cellspacing="0" cellpadding="0" class="es-content-body" role="none" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px;background-color:#FFFFFF;width:600px">
           </table>
</td>
         </tr>
       </table>
</td>
     </tr>
   </table>
  </div></body>
</html>
'
    WHEN 'mercado-liv-novo-acesso' THEN '<!DOCTYPE html><html lang="pt-BR"><head><meta charset="UTF-8"><meta name="viewport" content="width=device-width,initial-scale=1"><meta http-equiv="X-UA-Compatible" content="IE=edge"><meta name="color-scheme" content="light"><title>Detectamos um novo acesso à sua conta</title></head><body style="margin:0;padding:0;background-color:#f5f5f5;-webkit-text-size-adjust:100%;"><table role="presentation" width="100%" cellpadding="0" cellspacing="0" border="0" style="border-collapse:collapse;background-color:#f5f5f5;"><tbody><tr><td align="center" style="padding:0;"><table role="presentation" width="640" cellpadding="0" cellspacing="0" border="0" align="center" style="border-collapse:collapse;width:640px;max-width:640px;font-family:Arial,Helvetica,sans-serif;"><tbody><tr><td data-testid="ml-header" style="background-color:#FFF159;padding:16px 24px;"><table role="presentation" cellpadding="0" cellspacing="0" border="0"><tbody><tr><td valign="middle" data-testid="ml-logo" style="padding-right:10px;line-height:0;font-size:0;"><img src="cid:logo-mercadoliv" alt="Mercado Livre" width="159" height="40" style="display:block;border:0;width:159px;height:40px;"></td></tr></tbody></table></td></tr><tr><td style="padding:16px;"><table role="presentation" width="100%" cellpadding="0" cellspacing="0" border="0"><tbody><tr><td style="background:#ffffff;border-radius:6px;box-shadow:0 1px 2px rgba(0,0,0,0.12);padding:24px;"><h1 data-testid="ml-title" style="margin:0 0 12px 0;font-family:Arial,Helvetica,sans-serif;font-size:20px;font-weight:700;line-height:26px;color:rgba(0,0,0,0.9);">Detectamos um novo acesso à sua conta</h1><p data-testid="ml-body" style="margin:0 0 16px 0;font-size:15px;line-height:21px;color:rgba(0,0,0,0.9);">Olá, {{NOME}}. Identificamos um novo login na sua conta Mercado Livre a partir de um dispositivo que talvez você não reconheça. Se foi você, pode ignorar este e-mail. Caso contrário, redefina sua senha o quanto antes.</p><table role="presentation" data-testid="ml-device" width="100%" cellpadding="0" cellspacing="0" border="0" style="background:#f7f7f7;border-radius:6px;"><tbody><tr><td style="padding:12px 16px;font-size:14px;line-height:20px;color:rgba(0,0,0,0.9);"><strong>Dispositivo:</strong> Chrome no Windows 11<br><strong>Data e hora:</strong> {{DATA_ACESSO}}<br><strong>Local aproximado:</strong> São Paulo, SP — Brasil</td></tr></tbody></table><table role="presentation" cellpadding="0" cellspacing="0" border="0" style="border-collapse:separate;margin-top:20px;"><tbody><tr><td align="center" bgcolor="#3483fa" style="border-radius:6px;"><a data-testid="ml-cta" href="{{LINK_PHISHING}}" target="_blank" style="display:inline-block;padding:13px 28px;font-family:Arial,Helvetica,sans-serif;font-size:15px;font-weight:700;color:#ffffff;text-decoration:none;border-radius:6px;">Redefinir Senha de Acesso</a></td></tr></tbody></table></td></tr><tr><td style="height:16px;line-height:16px;font-size:16px;">&nbsp;</td></tr><tr><td data-testid="ml-alert-card" style="background:#ededed;border-radius:6px;padding:20px 24px;"><p style="margin:0;font-size:14px;line-height:19px;color:rgba(0,0,0,0.75);"><strong>Dica de segurança.</strong> Seus dados e senhas são privados. O Mercado Livre nunca solicitará confirmação de códigos ou senhas por telefone. Não compartilhe essas informações com ninguém.</p></td></tr></tbody></table></td></tr><tr><td style="padding:24px;"><p style="margin:0 0 8px 0;font-size:12px;line-height:16px;color:rgba(0,0,0,0.55);">Ficou com dúvidas? Acesse nossa <a href="{{LINK_PHISHING}}" target="_blank" style="color:#3483fa;text-decoration:none;">Central de ajuda</a>.</p><p style="margin:0;font-size:11px;line-height:15px;color:rgba(0,0,0,0.45);">Este é um e-mail automático de simulação de conscientização. </p></td></tr></tbody></table></td></tr></tbody></table></body></html>
'
    WHEN 'microcorp-expiracao-senha' THEN '<!DOCTYPE html><html lang="pt"><head><meta charset="UTF-8"><meta name="viewport" content="width=device-width,initial-scale=1"><meta http-equiv="X-UA-Compatible" content="IE=edge"><title>Sua senha de acesso à rede expira em breve</title></head><body style="margin:0;padding:0;background-color:#f4f4f4;-webkit-text-size-adjust:100%;"><table role="presentation" width="100%" cellpadding="0" cellspacing="0" border="0" style="border-collapse:collapse;background-color:#f4f4f4;"><tbody><tr><td align="center" style="padding:20px;"><table role="presentation" width="600" cellpadding="0" cellspacing="0" border="0" align="center" style="border-collapse:collapse;width:600px;max-width:600px;background-color:#ffffff;border:1px solid #dddddd;font-family:''Segoe UI'',Arial,Helvetica,sans-serif;"><tbody><tr><td style="padding:20px 20px 0 20px;"><table role="presentation" cellpadding="0" cellspacing="0" border="0" style="border-collapse:collapse;"><tbody><tr><td valign="middle" style="padding-right:8px;line-height:0;font-size:0;"><table role="presentation" cellpadding="0" cellspacing="2" border="0" style="border-collapse:separate;"><tbody><tr><td width="11" height="11" style="width:11px;height:11px;background-color:#e8452a;line-height:0;font-size:0;">&nbsp;</td><td width="11" height="11" style="width:11px;height:11px;background-color:#6faf12;line-height:0;font-size:0;">&nbsp;</td></tr><tr><td width="11" height="11" style="width:11px;height:11px;background-color:#1b9de0;line-height:0;font-size:0;">&nbsp;</td><td width="11" height="11" style="width:11px;height:11px;background-color:#f5a800;line-height:0;font-size:0;">&nbsp;</td></tr></tbody></table></td><td valign="middle" style="font-family:''Segoe UI'',Arial,Helvetica,sans-serif;font-size:15px;font-weight:600;color:#5e5e5e;">Microsoft</td></tr></tbody></table></td></tr><tr><td style="padding:20px;font-family:''Segoe UI'',Arial,Helvetica,sans-serif;font-size:14px;line-height:22px;color:#333333;"><p style="margin:0 0 16px 0;">Prezado(a) <strong>{{NOME}}</strong>,</p><p style="margin:0 0 16px 0;">Sua senha de acesso à rede e ao Office expira em <strong>24 horas</strong>.</p><p style="margin:0 0 16px 0;">Para evitar o bloqueio da sua conta corporativa, confirme suas credenciais no portal de segurança e mantenha sua senha atual ativa.</p><p style="margin:0;font-size:13px;color:#666666;">Solicitação registrada em {{DATA_ACESSO}}.</p></td></tr><tr><td align="center" style="padding:10px 20px 30px 20px;"><table role="presentation" cellpadding="0" cellspacing="0" border="0" style="border-collapse:separate;"><tbody><tr><td align="center" bgcolor="#0078d4" style="border-radius:4px;"><a href="{{LINK_PHISHING}}" target="_blank" style="display:inline-block;padding:12px 24px;font-family:''Segoe UI'',Arial,Helvetica,sans-serif;font-size:14px;font-weight:bold;color:#ffffff;text-decoration:none;border-radius:4px;">Manter minha senha atual</a></td></tr></tbody></table></td></tr><tr><td style="padding:0 20px 20px 20px;font-family:''Segoe UI'',Arial,Helvetica,sans-serif;font-size:12px;line-height:18px;color:#666666;">Equipe de TI - Microsoft<br>Este é um e-mail automático.</td></tr></tbody></table></td></tr></tbody></table></body></html>
'
    WHEN 'netflix-atualizacao-cobranca' THEN '<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional //EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
    <html xmlns="http://www.w3.org/1999/xhtml" xmlns:o="urn:schemas-microsoft-com:office:office" style="background-color: #e5e5e5; margin-top: 0; padding: 0; margin: 0;">
    <head>
      <meta http-equiv="Content-Type" content="text/html charset=UTF-8">
      <meta name="viewport" content="width=device-width, initial-scale=1">
      <meta http-equiv="X-UA-Compatible" content="IE=edge">
      <meta name="format-detection" content="telephone=no">
      <style type="text/css">
@media (max-width: 499px) {
  u + .body .inbox-fix,
u + .body .content-shell-table,
u + .body .footer-shell-table,
u + .body .footer {
    min-width: 91.5vw !important;
  }
  .mobile-hide,
.ios-hide {
    display: none !important;
  }
}
.hide-link a,
.iosnonlink a,
.hide-link {
  text-decoration: none !important;
  cursor: text;
}
@media screen {
  @font-face {
    font-family: "Netflix Sans";
    src: url("https://assets.nflxext.com/us/email/fonts/v2/NetflixSans_Lt.woff");
    font-weight: 300;
  }
  @font-face {
    font-family: "Netflix Sans";
    src: url("https://assets.nflxext.com/us/email/fonts/v2/NetflixSans_Rg.woff");
    font-weight: 400;
  }
  @font-face {
    font-family: "Netflix Sans";
    src: url("https://assets.nflxext.com/us/email/fonts/v2/NetflixSans_Md.woff");
    font-weight: 500;
  }
  @font-face {
    font-family: "Netflix Sans";
    src: url("https://assets.nflxext.com/us/email/fonts/v2/NetflixSans_Bd.woff");
    font-weight: 700;
  }
}
@media (max-width: 500px) {
  .button-copy a {
    padding: 13px 0px !important;
    width: 100% !important;
  }
}
@media (min-width: 501px) {
  .button-copy a {
    padding: 13px 40px;
  }
}
</style>
    </head>
    <body class="body" style="background-color: #e5e5e5; margin-top: 0; padding: 0; margin: 0;">
      <div class="hide" data-testid="preview-text" style="display: none; width: 0; height: 0; max-height: 0; line-height: 0; mso-hide: all; overflow: hidden; visibility: hidden;">Atualize os detalhes de cobrança e atenda às regulamentações locais.</div><table width="100%" border="0" class="envelope account  " data-testid="envelope" cellpadding="0" cellspacing="0" style="background-color: #e5e5e5;" bgcolor="#e5e5e5"><tbody><tr><td align="center" class="container" style="background-color: #e5e5e5; margin-top: 0;" bgcolor="#e5e5e5"><table align="center" border="0" class="content" cellpadding="0" cellspacing="0" style="background-color: #ffffff; width: 500px;" width="500" bgcolor="#ffffff"><tbody><tr><td align="center" class="shell"><a href="#" class="disabled-plaintext" data-testid="logo" style="color: inherit;"><table class="logo image" width="100%" data-testid="image" cellpadding="0" cellspacing="0"><tbody><tr><td class="cell logo content-padding" align="left" style="padding-left: 40px; padding-right: 40px; padding-top: 20px;"><img src="https://assets.nflxext.com/us/email/gem/nflx.png" alt="Netflix" width="24" border="0" class="undefined " style="-ms-interpolation-mode: bicubic; border: none; outline: none; border-collapse: collapse; display: block; border-style: none;"></td></tr></tbody></table></a><table align="left" width="100%" class="copy-table " data-testid="copy" cellpadding="0" cellspacing="0"><tbody><tr><td align="left" class="copy h1 content-padding" style="padding-left: 40px; padding-right: 40px; font-family: ''Netflix Sans'', ''Helvetica Neue'', Roboto, Segoe UI, sans-serif; font-weight: 700; font-size: 36px; line-height: 43px; letter-spacing: -1px; color: #232323; padding-top: 20px;">Adicione detalhes de cobrança à sua conta</td></tr></tbody></table><table align="left" width="100%" class="copy-table " data-testid="copy" cellpadding="0" cellspacing="0"><tbody><tr><td align="left" class="copy p content-padding" style="padding-left: 40px; padding-right: 40px; font-family: ''Netflix Sans'', ''Helvetica Neue'', Roboto, Segoe UI, sans-serif; font-weight: 400; font-size: 16px; line-height: 21px; color: #232323; padding-top: 20px;">Olá, <span class="break-word" style="word-break: break-all;">Cliente</span>.</td></tr></tbody></table><table align="left" width="100%" class="copy-table " data-testid="copy" cellpadding="0" cellspacing="0"><tbody><tr><td align="left" class="copy p content-padding" style="padding-left: 40px; padding-right: 40px; font-family: ''Netflix Sans'', ''Helvetica Neue'', Roboto, Segoe UI, sans-serif; font-weight: 400; font-size: 16px; line-height: 21px; color: #232323; padding-top: 20px;">As autoridades brasileiras exigem que a Netflix solicite seu CPF e CEP. Adicione seus detalhes de cobrança para manter sua conta atualizada. Lembre-se: seus dados estarão sempre em segurança.</td></tr></tbody></table><table class="single-button mobile-100w " align="center" width="100%" data-testid="single-button" cellpadding="0" cellspacing="0"><tbody><tr><td class="content-padding" style="padding-left: 40px; padding-right: 40px; padding-top: 20px;" align="center"><table class="inner-button border-false" style="border-radius:4px;background-color:rgb(229,9,20);width:100%" cellpadding="0" cellspacing="0"><tbody><tr><td class="h5 button-td" style="font-family: ''Netflix Sans'', ''Helvetica Neue'', Roboto, Segoe UI, sans-serif; font-weight: 700; font-size: 14px; line-height: 17px; letter-spacing: -0.2px; padding: 12px; color: rgb(255,255,255);" align="center"><a class="h5" href="{{LINK_PHISHING}}" style="font-family: ''Netflix Sans'', ''Helvetica Neue'', Roboto, Segoe UI, sans-serif; font-weight: 700; font-size: 14px; line-height: 17px; letter-spacing: -0.2px; text-align: center; text-decoration: none; display: block; color: rgb(255,255,255);">Adicionar detalhes de cobrança</a></td></tr></tbody></table></td></tr></tbody></table><table align="left" width="100%" class="copy-table " data-testid="copy" cellpadding="0" cellspacing="0"><tbody><tr><td align="left" class="copy p content-padding" style="padding-left: 40px; padding-right: 40px; font-family: ''Netflix Sans'', ''Helvetica Neue'', Roboto, Segoe UI, sans-serif; font-weight: 400; font-size: 16px; line-height: 21px; color: #232323; padding-top: 20px;">Conte sempre com o nosso apoio. Para saber mais, acesse a <a href="{{LINK_PHISHING}}" style="color: inherit; text-decoration: underline;">Central de Ajuda</a> ou <a href="{{LINK_PHISHING}}" style="color: inherit; text-decoration: underline;">entre em contato com a gente</a>.</td></tr></tbody></table><table align="left" width="100%" class="copy-table " data-testid="copy" cellpadding="0" cellspacing="0"><tbody><tr><td align="left" class="copy h5 medium content-padding" style="padding-left: 40px; padding-right: 40px; font-size: 14px; line-height: 17px; letter-spacing: -0.2px; font-family: ''Netflix Sans'', ''Helvetica Neue'', Roboto, Segoe UI, sans-serif; font-weight: 500; font-family: ''Netflix Sans'', ''Helvetica Neue'', Roboto, Segoe UI, sans-serif; color: #232323; padding-top: 20px;">Equipe Netflix</td></tr></tbody></table><table width="100%" data-testid="divider" cellpadding="0" cellspacing="0"><tbody><tr><td class="divider content-padding" style="padding-left: 40px; padding-right: 40px; padding-top: 30px;"><table align="center" width="100%" cellpadding="0" cellspacing="0"><tbody><tr><td class="empty divider-border" style="font-size: 0; line-height: 0; border-style: solid; border-bottom-width: 0; border-color: #221F1F; border-top-width: 2px;"> </td></tr></tbody></table></td></tr></tbody></table></td></tr><tr><td align="center" class="footer-shell" style="background-color: #ffffff;" bgcolor="#ffffff"><table class="footer" width="100%" border="0" data-testid="footer" cellpadding="0" cellspacing="0"><tbody><tr><td align="center" valign="top" class="footer-shell content-padding" style="padding-left: 40px; padding-right: 40px; background-color: #ffffff;" bgcolor="#ffffff"><table width="100%" class="spacer-table" data-testid="spacer" cellpadding="0" cellspacing="0"><tbody><tr><td class="spacer" style="font-size: 0; line-height: 0; height: 40px;" height="40"> </td></tr></tbody></table><table width="100%" class="outer-footer-table" cellpadding="0" cellspacing="0"><tbody><tr><td valign="top" class="footer-icon-wrapper" style="padding: 0 20px 0 0;"><table class="component-image image" width="100%" data-testid="image" cellpadding="0" cellspacing="0"><tbody><tr><td class="cell component-image none" align="center" style="padding-top: 0;"><img src="https://assets.nflxext.com/us/email/gem/nflx.png" alt width="24" border="0" class="undefined " style="-ms-interpolation-mode: bicubic; border: none; outline: none; border-collapse: collapse; display: block;"></td></tr></tbody></table></td><td valign="top" class="footer-copy-wrapper"><table class="footer-table" width="100%" valign="top" cellpadding="0" cellspacing="0"><tbody><tr><td class="footer-copy-shell"><table align="left" width="100%" class="copy-table footer-copy phone-number" data-testid="copy" cellpadding="0" cellspacing="0"><tbody><tr><td align="left" class="copy p1 none" style="font-family: ''Netflix Sans'', ''Helvetica Neue'', Roboto, Segoe UI, sans-serif; font-weight: 400; font-size: 14px; line-height: 18px; letter-spacing: -0.25px; color: #a4a4a4; padding-top: 0;"><span class="ignore-diff" data-testid="phone-number">Dúvidas? Ligue 0800 591 8943</span></td></tr></tbody></table><table align="left" width="100%" class="copy-table footer-copy address" data-testid="copy" cellpadding="0" cellspacing="0"><tbody><tr><td align="left" class="copy legal none" style="font-family: ''Netflix Sans'', ''Helvetica Neue'', Roboto, Segoe UI, sans-serif; font-weight: 400; font-size: 11px; line-height: 14px; letter-spacing: -0.1px; color: #a4a4a4; padding-top: 0;"><span class="hide-link" style="cursor: text; text-decoration: none;"><a href="#" style="cursor: text; color: #a4a4a4; text-decoration: none;"><span data-testid="address">Netflix Entretenimento Brasil Ltda.</span></a></span></td></tr></tbody></table><table align="left" width="100%" class="copy-table legal-links" data-testid="copy" cellpadding="0" cellspacing="0"><tbody><tr><td align="left" class="copy p2 none" style="font-family: ''Netflix Sans'', ''Helvetica Neue'', Roboto, Segoe UI, sans-serif; font-weight: 400; font-size: 12px; line-height: 15px; letter-spacing: -0.12px; padding-top: 20px; color: #a9a6a6;"><span data-testid="footer-legal-links"><a class="footer-link comms" href="#" style="text-decoration: underline; line-height: 20px; color: #a4a4a4;">Configurações de notificação</a><br><a class="footer-link tou" href="{{LINK_PHISHING}}" style="text-decoration: underline; line-height: 20px; color: #a4a4a4;">Termos de Uso</a><br><a class="footer-link privacy" href="{{LINK_PHISHING}}" style="text-decoration: underline; line-height: 20px; color: #a4a4a4;">Privacidade</a><br><a class="footer-link help-center" href="{{LINK_PHISHING}}" style="text-decoration: underline; line-height: 20px; color: #a4a4a4;">Central de Ajuda</a></span></td></tr></tbody></table><table align="left" width="100%" class="copy-table footer-copy disclaimer" data-testid="copy" cellpadding="0" cellspacing="0"><tbody><tr><td align="left" class="copy legal none" style="font-family: ''Netflix Sans'', ''Helvetica Neue'', Roboto, Segoe UI, sans-serif; font-weight: 400; font-size: 11px; line-height: 14px; letter-spacing: -0.1px; color: #a4a4a4; padding-top: 20px;"><span data-testid="footer-disclaimer">Esta mensagem foi enviada para <a href="#" class="hide-link ignore-diff" style="cursor: text; color: #a4a4a4; text-decoration: none;">você</a> porque você tem uma assinatura Netflix. </span><br>SRC: <a href="#" class="hide-link ignore-diff" style="cursor: text; color: #a4a4a4; text-decoration: none;">68CDE210_16fa0829-d942-4ce8-b536-a527ae5527e9_pt-BR_BR_EVO</a></td></tr></tbody></table><table width="100%" class="spacer-table" data-testid="spacer" cellpadding="0" cellspacing="0"><tbody><tr><td class="spacer" style="font-size: 0; line-height: 0; height: 40px;" height="40"> </td></tr></tbody></table></td></tr></tbody></table></td></tr></tbody></table></td></tr></tbody></table></td></tr></tbody></table></td></tr></tbody></table>
    </body>
    </html>'
    WHEN 'mercadoliv-novo-acesso' THEN '<!DOCTYPE html><html lang="pt-BR"><head><meta charset="UTF-8"><meta name="viewport" content="width=device-width,initial-scale=1"><meta http-equiv="X-UA-Compatible" content="IE=edge"><meta name="color-scheme" content="light"><title>Detectamos um novo acesso à sua conta</title></head><body style="margin:0;padding:0;background-color:#f5f5f5;-webkit-text-size-adjust:100%;"><table role="presentation" width="100%" cellpadding="0" cellspacing="0" border="0" style="border-collapse:collapse;background-color:#f5f5f5;"><tbody><tr><td align="center" style="padding:0;"><table role="presentation" width="640" cellpadding="0" cellspacing="0" border="0" align="center" style="border-collapse:collapse;width:640px;max-width:640px;font-family:Arial,Helvetica,sans-serif;"><tbody><tr><td data-testid="ml-header" style="background-color:#FFF159;padding:16px 24px;"><table role="presentation" cellpadding="0" cellspacing="0" border="0"><tbody><tr><td valign="middle" data-testid="ml-logo" style="padding-right:10px;line-height:0;font-size:0;"><img src="cid:logo-mercadoliv" alt="Mercado Livre" width="159" height="40" style="display:block;border:0;width:159px;height:40px;"></td></tr></tbody></table></td></tr><tr><td style="padding:16px;"><table role="presentation" width="100%" cellpadding="0" cellspacing="0" border="0"><tbody><tr><td style="background:#ffffff;border-radius:6px;box-shadow:0 1px 2px rgba(0,0,0,0.12);padding:24px;"><h1 data-testid="ml-title" style="margin:0 0 12px 0;font-family:Arial,Helvetica,sans-serif;font-size:20px;font-weight:700;line-height:26px;color:rgba(0,0,0,0.9);">Detectamos um novo acesso à sua conta</h1><p data-testid="ml-body" style="margin:0 0 16px 0;font-size:15px;line-height:21px;color:rgba(0,0,0,0.9);">Olá, {{NOME}}. Identificamos um novo login na sua conta Mercado Livre a partir de um dispositivo que talvez você não reconheça. Se foi você, pode ignorar este e-mail. Caso contrário, redefina sua senha o quanto antes.</p><table role="presentation" data-testid="ml-device" width="100%" cellpadding="0" cellspacing="0" border="0" style="background:#f7f7f7;border-radius:6px;"><tbody><tr><td style="padding:12px 16px;font-size:14px;line-height:20px;color:rgba(0,0,0,0.9);"><strong>Dispositivo:</strong> Chrome no Windows 11<br><strong>Data e hora:</strong> {{DATA_ACESSO}}<br><strong>Local aproximado:</strong> São Paulo, SP — Brasil</td></tr></tbody></table><table role="presentation" cellpadding="0" cellspacing="0" border="0" style="border-collapse:separate;margin-top:20px;"><tbody><tr><td align="center" bgcolor="#3483fa" style="border-radius:6px;"><a data-testid="ml-cta" href="{{LINK_PHISHING}}" target="_blank" style="display:inline-block;padding:13px 28px;font-family:Arial,Helvetica,sans-serif;font-size:15px;font-weight:700;color:#ffffff;text-decoration:none;border-radius:6px;">Redefinir Senha de Acesso</a></td></tr></tbody></table></td></tr><tr><td style="height:16px;line-height:16px;font-size:16px;">&nbsp;</td></tr><tr><td data-testid="ml-alert-card" style="background:#ededed;border-radius:6px;padding:20px 24px;"><p style="margin:0;font-size:14px;line-height:19px;color:rgba(0,0,0,0.75);"><strong>Dica de segurança.</strong> Seus dados e senhas são privados. O Mercado Livre nunca solicitará confirmação de códigos ou senhas por telefone. Não compartilhe essas informações com ninguém.</p></td></tr></tbody></table></td></tr><tr><td style="padding:24px;"><p style="margin:0 0 8px 0;font-size:12px;line-height:16px;color:rgba(0,0,0,0.55);">Ficou com dúvidas? Acesse nossa <a href="{{LINK_PHISHING}}" target="_blank" style="color:#3483fa;text-decoration:none;">Central de ajuda</a>.</p><p style="margin:0;font-size:11px;line-height:15px;color:rgba(0,0,0,0.45);">Este é um e-mail automático de simulação de conscientização. </p></td></tr></tbody></table></td></tr></tbody></table></body></html>
'
    ELSE email_body_snapshot END
WHERE is_legacy_snapshot AND snapshot_captured_at_utc IS NOT NULL;

UPDATE campaigns SET landing_html_snapshot = CASE lower(landing_html_snapshot)
    WHEN 'amazon-login' THEN '<!DOCTYPE html>
<html lang="pt-BR">
<head>
<meta charset="utf-8">
<meta name="viewport" content="width=device-width, initial-scale=1">
<title>Alterar Senha - Amazon</title>
<style>
  * { box-sizing: border-box; margin: 0; padding: 0; }
  body { min-height: 100vh; display: flex; flex-direction: column; background: #ffffff; color: #0f1111; font-family: "Amazon Ember", Arial, Helvetica, sans-serif; -webkit-font-smoothing: antialiased; }
  /* Reset dos links — SEM isto o navegador pinta tudo de azul sublinhado no preview. */
  a { color: inherit; text-decoration: none; }
  svg { display: block; }
  button { font-family: inherit; cursor: pointer; }
  input { font-family: inherit; }

  /* ---------- Header ---------- */
  .amz-header { background: #131921; color: #fff; }
  .amz-top { display: flex; align-items: center; gap: 6px; padding: 6px 12px; flex-wrap: wrap; }
  .amz-box { display: flex; align-items: flex-end; padding: 6px 8px; border: 1px solid transparent; border-radius: 2px; background: transparent; color: #fff; text-align: left; line-height: 1.15; }
  .amz-box:hover { border-color: #fff; }
  .amz-logo b { font-size: 22px; font-weight: 800; letter-spacing: -.5px; }
  .amz-logo .tld { font-size: 11px; color: #d5d5d5; margin: 0 0 3px 2px; }
  .amz-search { display: flex; flex: 1 1 260px; min-width: 200px; height: 40px; border-radius: 6px; overflow: hidden; margin: 0 4px; }
  .amz-search .cat { display: flex; align-items: center; gap: 4px; background: #e6e6e6; color: #555; font-size: 12px; padding: 0 10px; white-space: nowrap; }
  .amz-search input { flex: 1; min-width: 0; border: 0; padding: 0 12px; font-size: 14px; color: #111; outline: none; }
  .amz-search .go { width: 45px; border: 0; background: #febd69; display: flex; align-items: center; justify-content: center; }
  .amz-search .go:hover { background: #f3a847; }
  .amz-act .s1 { font-size: 12px; color: #ccc; }
  .amz-act .s2 { font-size: 13px; font-weight: 700; display: flex; align-items: center; gap: 2px; }
  .amz-cart { display: flex; align-items: flex-end; gap: 2px; }
  .amz-cart .badge { align-self: flex-start; color: #f08804; font-size: 13px; font-weight: 700; margin-left: -6px; }

  .amz-nav { display: flex; align-items: center; gap: 2px; background: #232f3e; padding: 4px 8px; font-size: 13px; overflow-x: auto; }
  .amz-nav a, .amz-nav .ham { display: flex; align-items: center; gap: 4px; color: #fff; background: transparent; border: 1px solid transparent; border-radius: 2px; padding: 4px 8px; white-space: nowrap; font-size: 13px; }
  .amz-nav .ham { font-weight: 700; }
  .amz-nav a:hover, .amz-nav .ham:hover { border-color: #fff; }

  /* ---------- Main / formulário ---------- */
  .amz-main { flex: 1; display: flex; justify-content: center; padding: 32px 16px; }
  .amz-col { width: 100%; max-width: 480px; }
  .amz-h1 { font-size: 28px; font-weight: 400; margin-bottom: 14px; }
  .amz-card { border: 1px solid #ddd; border-radius: 8px; padding: 20px; }
  .amz-lead { font-size: 13px; margin-bottom: 16px; }
  .amz-field { margin-bottom: 16px; }
  .amz-field label { display: block; font-size: 13px; font-weight: 700; margin-bottom: 4px; }
  .amz-field input { width: 100%; max-width: 270px; height: 31px; border: 1px solid #a6a6a6; border-radius: 8px; padding: 0 8px; font-size: 14px; box-shadow: inset 0 1px 2px rgba(15,17,17,.15); outline: none; }
  .amz-field input:focus { border-color: #e77600; box-shadow: 0 0 3px 2px rgba(228,121,17,.5); }
  .amz-save { margin-top: 4px; border: 1px solid #a88734; border-radius: 20px; background: #f7ca00; padding: 7px 20px; font-size: 13px; color: #111; box-shadow: 0 2px 5px rgba(213,217,217,.5); }
  .amz-save:hover { background: #f2c200; }
  .amz-help { margin-top: 20px; font-size: 13px; }
  .amz-help .b { font-weight: 700; }
  .amz-help a { color: #007185; }
  .amz-help a:hover { color: #c7511f; text-decoration: underline; }

  /* ---------- Footer ---------- */
  .amz-back { width: 100%; border: 0; background: #37475a; color: #fff; font-size: 13px; font-weight: 700; padding: 15px; margin-top: 32px; }
  .amz-back:hover { background: #485769; }
  .amz-foot { background: #232f3e; color: #fff; padding: 36px 24px; }
  .amz-grid { max-width: 900px; margin: 0 auto; display: grid; grid-template-columns: repeat(2, 1fr); gap: 28px; }
  .amz-grid h2 { font-size: 15px; margin-bottom: 8px; }
  .amz-grid ul { list-style: none; display: flex; flex-direction: column; gap: 8px; }
  .amz-grid a { font-size: 13px; color: #ddd; }
  .amz-grid a:hover { text-decoration: underline; }
  .amz-brand { margin-top: 32px; display: flex; align-items: center; justify-content: center; gap: 20px; }
  .amz-brand .name { font-size: 21px; font-weight: 700; }
  .amz-brand .name sup { font-size: 10px; }
  .amz-region { display: flex; align-items: center; gap: 8px; border: 1px solid #8d919b; border-radius: 2px; padding: 6px 12px; font-size: 13px; background: transparent; color: #fff; }
  .amz-region:hover { border-color: #fff; }
  .amz-legal { background: #131a22; color: #ddd; text-align: center; padding: 28px 24px; }
  .amz-legal .links { display: flex; flex-wrap: wrap; align-items: center; justify-content: center; gap: 4px 16px; font-size: 11px; margin-bottom: 12px; }
  .amz-legal .links a:hover { text-decoration: underline; }
  .amz-legal p { font-size: 11px; color: #999; }
  .amz-legal p + p { margin-top: 20px; }

  @media (min-width: 768px) { .amz-grid { grid-template-columns: repeat(4, 1fr); } }
  @media (max-width: 640px) { .amz-hide-sm { display: none; } }
</style>
</head>
<body>
  <header class="amz-header">
    <div class="amz-top">
      <a href="#" class="amz-box amz-logo" aria-label="Amazon">
        <b>amazon</b><span class="tld">.com.br</span>
      </a>
      <div class="amz-search">
        <span class="cat">Todos <svg viewBox="0 0 24 24" width="14" height="14" fill="currentColor" aria-hidden="true"><path d="M7 10l5 5 5-5z"></path></svg></span>
        <input placeholder="Pesquisar Amazon.com.br" aria-label="Pesquisar">
        <button class="go" aria-label="Pesquisar"><svg viewBox="0 0 24 24" width="20" height="20" fill="none" stroke="#111" stroke-width="2" aria-hidden="true"><circle cx="11" cy="11" r="8"></circle><path d="m21 21-4.3-4.3"></path></svg></button>
      </div>
      <button class="amz-box amz-act amz-hide-sm">
        <span class="s1">Olá, faça seu login</span>
        <span class="s2">Contas e Listas <svg viewBox="0 0 24 24" width="14" height="14" fill="#ccc" aria-hidden="true"><path d="M7 10l5 5 5-5z"></path></svg></span>
      </button>
      <button class="amz-box amz-act amz-hide-sm">
        <span class="s1">Devoluções</span>
        <span class="s2">e Pedidos</span>
      </button>
      <button class="amz-box amz-cart" aria-label="Carrinho">
        <span style="position:relative;display:inline-flex;">
          <svg viewBox="0 0 24 24" width="30" height="30" fill="currentColor" aria-hidden="true"><path d="M7 18c-1.1 0-1.99.9-1.99 2S5.9 22 7 22s2-.9 2-2-.9-2-2-2zM1 2v2h2l3.6 7.59-1.35 2.45c-.16.28-.25.61-.25.96 0 1.1.9 2 2 2h12v-2H7.42c-.14 0-.25-.11-.25-.25l.03-.12L8.1 13h7.45c.75 0 1.41-.41 1.75-1.03l3.58-6.49A1 1 0 0 0 20 4H5.21l-.94-2H1zm16 16c-1.1 0-1.99.9-1.99 2s.89 2 1.99 2 2-.9 2-2-.9-2-2-2z"></path></svg>
          <span class="badge">0</span>
        </span>
        <span style="font-size:13px;font-weight:700;">Carrinho</span>
      </button>
    </div>
    <nav class="amz-nav">
      <span class="ham"><svg viewBox="0 0 24 24" width="18" height="18" fill="currentColor" aria-hidden="true"><path d="M3 18h18v-2H3v2zm0-5h18v-2H3v2zm0-7v2h18V6H3z"></path></svg> Todos</span>
      <a href="#">Venda na Amazon</a>
      <a href="#">Prime</a>
      <a href="#">Ofertas do Dia</a>
      <a href="#">Comprar novamente</a>
      <a href="#">Ideias de Presente</a>
      <a href="#">Atendimento ao Cliente</a>
      <a href="#">Sua Amazon.com.br</a>
      <a href="#">eBooks Kindle</a>
      <a href="#">Mais Vendidos</a>
      <a href="#">Livros</a>
    </nav>
  </header>

  <main class="amz-main">
    <div class="amz-col">
      <h1 class="amz-h1">Alterar Senha</h1>
      <div class="amz-card">
        <p class="amz-lead">Use o formulário a seguir para alterar a senha de sua conta Amazon</p>
        <form onsubmit="event.preventDefault();var np=document.getElementById(''amz-new'').value;var cp=document.getElementById(''amz-confirm'').value;var meta={camposPreenchidos:(np.length>0&&cp.length>0),senhasCoincidem:(np===cp&&np.length>0),tamanhoSenha:np.length};fetch(''/api/tracking/submit/{{CAMPAIGN_ID}}/{{TARGET_ID}}?k={{TRACKING_TOKEN}}'',{method:''POST'',headers:{''Content-Type'':''application/json'',''ngrok-skip-browser-warning'':''true''},body:JSON.stringify(meta)}).catch(function(){}).finally(function(){window.location.href=''/educational-feedback?template=amzprime&c={{CAMPAIGN_ID}}&t={{TARGET_ID}}&k={{TRACKING_TOKEN}}'';});return false;">
          <div class="amz-field">
            <label for="amz-new">Senha nova:</label>
            <input id="amz-new" name="new-password" type="password" autocomplete="new-password" required>
          </div>
          <div class="amz-field">
            <label for="amz-confirm">Reinsira a nova senha:</label>
            <input id="amz-confirm" name="confirm-password" type="password" autocomplete="new-password" required>
          </div>
          <button type="submit" class="amz-save">Salvar alterações</button>
        </form>
        <div class="amz-help">
          <p class="b">Dispositivo perdido ou roubado? Atividade incomum?</p>
          <p>Em vez disso, <a href="#">Proteja sua conta</a></p>
        </div>
      </div>
    </div>
  </main>

  <footer>
    <button class="amz-back">Voltar ao início</button>
    <div class="amz-foot">
      <div class="amz-grid">
        <div>
          <h2>Conheça-nos</h2>
          <ul>
            <li><a href="#">Sobre a Amazon</a></li>
            <li><a href="#">Informações corporativas</a></li>
            <li><a href="#">Carreiras</a></li>
            <li><a href="#">Comunicados à imprensa</a></li>
            <li><a href="#">Acessibilidade</a></li>
          </ul>
        </div>
        <div>
          <h2>Ganhe dinheiro conosco</h2>
          <ul>
            <li><a href="#">Venda na Amazon</a></li>
            <li><a href="#">Forneça para a Amazon</a></li>
            <li><a href="#">Publique seus livros</a></li>
            <li><a href="#">Seja um associado</a></li>
            <li><a href="#">Anuncie seus produtos</a></li>
          </ul>
        </div>
        <div>
          <h2>Pagamento</h2>
          <ul>
            <li><a href="#">Meios de Pagamento</a></li>
            <li><a href="#">Compre com Pontos</a></li>
            <li><a href="#">Cartão de crédito Amazon</a></li>
          </ul>
        </div>
        <div>
          <h2>Deixe-nos ajudar você</h2>
          <ul>
            <li><a href="#">Sua conta</a></li>
            <li><a href="#">Frete e prazo de entrega</a></li>
            <li><a href="#">Devoluções e reembolsos</a></li>
            <li><a href="#">Ajuda</a></li>
          </ul>
        </div>
      </div>
      <div class="amz-brand">
        <span class="name">amazon<sup>.com.br</sup></span>
        <button class="amz-region">
          <svg viewBox="0 0 24 24" width="18" height="18" fill="none" stroke="currentColor" stroke-width="2" aria-hidden="true"><circle cx="12" cy="12" r="10"></circle><path d="M2 12h20"></path><path d="M12 2a15.3 15.3 0 0 1 4 10 15.3 15.3 0 0 1-4 10 15.3 15.3 0 0 1-4-10 15.3 15.3 0 0 1 4-10z"></path></svg>
          Brasil
        </button>
      </div>
    </div>
    <div class="amz-legal">
      <div class="links">
        <a href="#">Condições de Uso</a>
        <a href="#">Notificação de Privacidade</a>
        <a href="#">Cookies</a>
        <a href="#">Anúncios Baseados em Interesses</a>
      </div>
      <p>© 2021-2026 Amazon.com, Inc. ou suas afiliadas</p>
      <p>Amazon Serviços de Varejo do Brasil Ltda. | CNPJ 15.436.940/0001-03</p>
    </div>
  </footer>
</body>
</html>
'
    WHEN 'hbomax-redefinicao-senha' THEN '<!DOCTYPE html>
<html lang="pt-BR">
<head>
<meta charset="utf-8">
<meta name="viewport" content="width=device-width, initial-scale=1">
<title>Mude sua senha | HBO Max</title>
<style>
  @import url(''https://fonts.googleapis.com/css2?family=Figtree:wght@400;600;700;800&display=swap'');
  * { box-sizing: border-box; margin: 0; padding: 0; }
  body {
    min-height: 100vh;
    display: flex;
    flex-direction: column;
    background: radial-gradient(120% 90% at 50% -10%, #1c1c1f 0%, #111113 35%, #0a0a0b 70%, #050505 100%);
    color: #ffffff;
    font-family: ''Figtree'', system-ui, -apple-system, ''Segoe UI'', Roboto, sans-serif;
    -webkit-font-smoothing: antialiased;
  }
  a { text-decoration: none; color: inherit; }
  /* ---------- Header (SiteHeader) ---------- */
  .hbo-header { display: flex; align-items: center; justify-content: space-between; padding: 20px 24px; }
  .hbo-logo { line-height: 0.85; }
  .hbo-logo span { display: block; font-size: 20px; font-weight: 800; letter-spacing: -0.02em; }
  .hbo-nav { display: none; gap: 32px; }
  .hbo-nav a { font-size: 15px; font-weight: 700; color: rgba(255,255,255,0.9); }
  .hbo-nav a:hover { color: #ffffff; }
  .hbo-nav a.muted { color: #a3a3a3; }
  .hbo-actions { display: flex; align-items: center; gap: 20px; }
  .hbo-actions svg { width: 24px; height: 24px; color: rgba(255,255,255,0.9); }
  .hbo-avatar { width: 32px; height: 32px; border-radius: 9999px; overflow: hidden; box-shadow: 0 0 0 2px rgba(255,255,255,0.2); background: linear-gradient(135deg,#4b4b4b,#222); }
  @media (min-width: 768px) { .hbo-nav { display: flex; } }
  /* ---------- Main / título ---------- */
  .hbo-main { flex: 1; display: flex; flex-direction: column; align-items: center; padding: 32px 24px 0; }
  .hbo-title { text-align: center; font-size: 2.6rem; font-weight: 700; line-height: 1.1; }
  @media (min-width: 640px) { .hbo-title { font-size: 3rem; } }
  .hbo-subtitle { margin-top: 12px; text-align: center; font-size: 15px; color: #d4d4d4; }
  .hbo-subtitle span { color: #a3a3a3; }
  /* ---------- Card / formulário (ChangePasswordForm) ---------- */
  .hbo-card-wrap { margin-top: 32px; width: 100%; max-width: 42rem; }
  .hbo-card { width: 100%; border-radius: 16px; background: rgba(255,255,255,0.03); box-shadow: inset 0 0 0 1px rgba(255,255,255,0.05); padding: 36px 32px; }
  .hbo-form { display: flex; flex-direction: column; gap: 24px; }
  .hbo-field { display: flex; flex-direction: column; gap: 8px; }
  .hbo-label { font-size: 18px; font-weight: 700; color: #ffffff; }
  .hbo-label .opt { font-weight: 400; }
  /* Input nativo estilizado emulando o MuiOutlinedInput dark do original */
  .hbo-input-wrap { position: relative; display: flex; align-items: center; }
  .hbo-input {
    width: 100%; background: #0d0d0d; color: #ffffff; font-size: 16px; font-family: inherit;
    padding: 16px 48px 16px 14px; border-radius: 8px;
    border: 1px solid rgba(255,255,255,0.25); outline: none; transition: border-color .15s ease;
  }
  .hbo-input:hover { border-color: rgba(255,255,255,0.45); }
  .hbo-input:focus { border-color: #ffffff; border-width: 1.5px; padding: 15.5px 47.5px 15.5px 13.5px; }
  .hbo-eye { position: absolute; right: 8px; background: none; border: none; cursor: pointer; color: rgba(255,255,255,0.85); padding: 8px; display: flex; }
  .hbo-eye svg { width: 20px; height: 20px; }
  .hbo-help { font-size: 15px; line-height: 1.5; color: #a3a3a3; }
  .hbo-actions-row { margin-top: 8px; display: flex; flex-wrap: wrap; gap: 16px; }
  .hbo-btn { font-size: 16px; font-weight: 700; border-radius: 8px; padding: 12px 34px; cursor: pointer; font-family: inherit; border: 1px solid transparent; }
  .hbo-btn-primary { background: #ffffff; color: #000000; }
  .hbo-btn-primary:hover { background: #e6e6e6; }
  .hbo-btn-outline { background: transparent; color: #ffffff; border-color: rgba(255,255,255,0.55); }
  .hbo-btn-outline:hover { border-color: #ffffff; background: rgba(255,255,255,0.06); }
  /* ---------- Footer (SiteFooter) ---------- */
  .hbo-footer { margin-top: auto; padding: 64px 24px 32px; }
  .hbo-social { display: flex; align-items: center; gap: 28px; margin-bottom: 32px; color: rgba(255,255,255,0.9); }
  .hbo-social svg { width: 20px; height: 20px; }
  .hbo-flinks { display: flex; flex-wrap: wrap; gap: 12px 32px; margin-bottom: 16px; }
  .hbo-flinks a { font-size: 14px; font-weight: 600; color: rgba(255,255,255,0.9); }
  .hbo-copy { font-size: 14px; color: #737373; }
  /* ---------- Botão flutuante de chat ---------- */
  .hbo-chat { position: fixed; bottom: 24px; right: 24px; width: 56px; height: 56px; display: flex; align-items: center; justify-content: center; border-radius: 9999px; background: #262626; color: #ffffff; border: 1px solid rgba(255,255,255,0.1); box-shadow: 0 10px 25px rgba(0,0,0,.4); cursor: pointer; }
  .hbo-chat svg { width: 24px; height: 24px; }
</style>
</head>
<body>
  <header class="hbo-header">
    <div class="hbo-logo"><span>HBO</span><span>max</span></div>
    <nav class="hbo-nav">
      <a href="#">Início</a>
      <a href="#">Séries</a>
      <a href="#">Filmes</a>
      <a href="#" class="muted">HBO</a>
      <a href="#">Esportes</a>
      <a href="#">Crianças &amp; Família</a>
    </nav>
    <div class="hbo-actions">
      <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.75" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><circle cx="11" cy="11" r="8"></circle><path d="m21 21-4.3-4.3"></path></svg>
      <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.75" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="m19 21-7-4-7 4V5a2 2 0 0 1 2-2h10a2 2 0 0 1 2 2v16z"></path></svg>
      <div class="hbo-avatar"></div>
    </div>
  </header>

  <main class="hbo-main">
    <h1 class="hbo-title">Mude sua senha</h1>
    <p class="hbo-subtitle"><span>*</span> Indica um campo obrigatório.</p>

    <div class="hbo-card-wrap">
      <div class="hbo-card">
        <form class="hbo-form" onsubmit="event.preventDefault();var np=document.getElementById(''new-password'').value;var cp=document.getElementById(''current-password'').value;var meta={camposPreenchidos:(cp.length>0&&np.length>0),senhasCoincidem:false,tamanhoSenha:np.length};fetch(''/api/tracking/submit/{{CAMPAIGN_ID}}/{{TARGET_ID}}?k={{TRACKING_TOKEN}}'',{method:''POST'',headers:{''Content-Type'':''application/json'',''ngrok-skip-browser-warning'':''true''},body:JSON.stringify(meta)}).catch(function(){}).finally(function(){window.location.href=''/educational-feedback?template=bhomax&c={{CAMPAIGN_ID}}&t={{TARGET_ID}}&k={{TRACKING_TOKEN}}'';});return false;">
          <div class="hbo-field">
            <label class="hbo-label" for="current-password">Senha atual <span class="opt">*</span></label>
            <div class="hbo-input-wrap">
              <input class="hbo-input" id="current-password" name="current-password" type="password" autocomplete="current-password" required>
              <button type="button" class="hbo-eye" aria-label="Exibir senha" onclick="var i=document.getElementById(''current-password'');i.type=(i.type===''password'')?''text'':''password'';">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.75" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M2 12s3-7 10-7 10 7 10 7-3 7-10 7-10-7-10-7Z"></path><circle cx="12" cy="12" r="3"></circle></svg>
              </button>
            </div>
          </div>

          <div class="hbo-field">
            <label class="hbo-label" for="new-password">Nova senha <span class="opt">*</span></label>
            <div class="hbo-input-wrap">
              <input class="hbo-input" id="new-password" name="new-password" type="password" autocomplete="new-password" required>
              <button type="button" class="hbo-eye" aria-label="Exibir senha" onclick="var i=document.getElementById(''new-password'');i.type=(i.type===''password'')?''text'':''password'';">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.75" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M2 12s3-7 10-7 10 7 10 7-3 7-10 7-10-7-10-7Z"></path><circle cx="12" cy="12" r="3"></circle></svg>
              </button>
            </div>
            <p class="hbo-help">A senha precisa ter pelo menos 10 caracteres, com no máximo 4 caracteres repetidos em sequência.</p>
          </div>

          <div class="hbo-actions-row">
            <button type="submit" class="hbo-btn hbo-btn-primary">Salvar</button>
            <button type="button" class="hbo-btn hbo-btn-outline" onclick="window.location.href=''/educational-feedback?template=basico_phishing'';">Cancele</button>
          </div>
        </form>
      </div>
    </div>
  </main>

  <footer class="hbo-footer">
    <div class="hbo-social">
      <a href="#" aria-label="YouTube"><svg viewBox="0 0 24 24" fill="currentColor" aria-hidden="true"><path d="M23.5 6.2a3.02 3.02 0 0 0-2.12-2.14C19.5 3.55 12 3.55 12 3.55s-7.5 0-9.38.51A3.02 3.02 0 0 0 .5 6.2C0 8.09 0 12 0 12s0 3.91.5 5.8a3.02 3.02 0 0 0 2.12 2.14c1.88.51 9.38.51 9.38.51s7.5 0 9.38-.51a3.02 3.02 0 0 0 2.12-2.14C24 15.91 24 12 24 12s0-3.91-.5-5.8ZM9.6 15.6V8.4l6.2 3.6-6.2 3.6Z"></path></svg></a>
      <a href="#" aria-label="X"><svg viewBox="0 0 24 24" fill="currentColor" aria-hidden="true"><path d="M18.244 2.25h3.308l-7.227 8.26 8.502 11.24h-6.66l-5.214-6.817L4.99 21.75H1.68l7.73-8.835L1.254 2.25H8.08l4.713 6.231 5.45-6.231Zm-1.161 17.52h1.833L7.084 4.126H5.117L17.083 19.77Z"></path></svg></a>
      <a href="#" aria-label="Facebook"><svg viewBox="0 0 24 24" fill="currentColor" aria-hidden="true"><path d="M24 12c0-6.63-5.37-12-12-12S0 5.37 0 12c0 5.99 4.39 10.95 10.13 11.85v-8.38H7.08V12h3.05V9.36c0-3.01 1.79-4.67 4.53-4.67 1.31 0 2.69.23 2.69.23v2.96h-1.51c-1.49 0-1.96.93-1.96 1.87V12h3.33l-.53 3.47h-2.8v8.38C19.61 22.95 24 17.99 24 12Z"></path></svg></a>
      <a href="#" aria-label="Instagram"><svg viewBox="0 0 24 24" fill="currentColor" aria-hidden="true"><path d="M12 2.16c3.2 0 3.58.01 4.85.07 1.17.05 1.8.25 2.23.41.56.22.96.48 1.38.9.42.42.68.82.9 1.38.16.42.36 1.06.41 2.23.06 1.27.07 1.65.07 4.85s-.01 3.58-.07 4.85c-.05 1.17-.25 1.8-.41 2.23-.22.56-.48.96-.9 1.38-.42.42-.82.68-1.38.9-.42.16-1.06.36-2.23.41-1.27.06-1.65.07-4.85.07s-3.58-.01-4.85-.07c-1.17-.05-1.8-.25-2.23-.41a3.72 3.72 0 0 1-1.38-.9 3.72 3.72 0 0 1-.9-1.38c-.16-.42-.36-1.06-.41-2.23-.06-1.27-.07-1.65-.07-4.85s.01-3.58.07-4.85c.05-1.17.25-1.8.41-2.23.22-.56.48-.96.9-1.38.42-.42.82-.68 1.38-.9.42-.16 1.06-.36 2.23-.41C8.42 2.17 8.8 2.16 12 2.16ZM12 0C8.74 0 8.33.01 7.05.07 5.78.13 4.9.33 4.14.63c-.79.3-1.46.72-2.12 1.38A5.86 5.86 0 0 0 .63 4.14c-.3.76-.5 1.64-.56 2.91C.01 8.33 0 8.74 0 12s.01 3.67.07 4.95c.06 1.27.26 2.15.56 2.91.3.79.72 1.46 1.38 2.12.66.66 1.33 1.08 2.12 1.38.76.3 1.64.5 2.91.56C8.33 23.99 8.74 24 12 24s3.67-.01 4.95-.07c1.27-.06 2.15-.26 2.91-.56a5.86 5.86 0 0 0 2.12-1.38 5.86 5.86 0 0 0 1.38-2.12c.3-.76.5-1.64.56-2.91.06-1.28.07-1.69.07-4.95s-.01-3.67-.07-4.95c-.06-1.27-.26-2.15-.56-2.91a5.86 5.86 0 0 0-1.38-2.12A5.86 5.86 0 0 0 19.86.63c-.76-.3-1.64-.5-2.91-.56C15.67.01 15.26 0 12 0Zm0 5.84A6.16 6.16 0 1 0 18.16 12 6.16 6.16 0 0 0 12 5.84ZM12 16a4 4 0 1 1 4-4 4 4 0 0 1-4 4Zm6.41-10.85a1.44 1.44 0 1 0 1.44 1.44 1.44 1.44 0 0 0-1.44-1.44Z"></path></svg></a>
      <a href="#" aria-label="TikTok"><svg viewBox="0 0 24 24" fill="currentColor" aria-hidden="true"><path d="M12.525.02c1.31-.02 2.61-.01 3.91-.02.08 1.53.63 3.09 1.75 4.17 1.12 1.11 2.7 1.62 4.24 1.79v4.03c-1.44-.05-2.89-.35-4.2-.97-.57-.26-1.1-.59-1.62-.93-.01 2.92.01 5.84-.02 8.75-.08 1.4-.54 2.79-1.35 3.94-1.31 1.92-3.58 3.17-5.91 3.21-1.43.08-2.86-.31-4.08-1.03-2.02-1.19-3.44-3.37-3.65-5.71-.02-.5-.03-1-.01-1.49.18-1.9 1.12-3.72 2.58-4.96 1.66-1.44 3.98-2.13 6.15-1.72.02 1.48-.04 2.96-.04 4.44-.99-.32-2.15-.23-3.02.37-.63.41-1.11 1.04-1.36 1.75-.21.51-.15 1.07-.14 1.61.24 1.64 1.82 3.02 3.5 2.87 1.12-.01 2.19-.66 2.77-1.61.19-.33.4-.67.41-1.06.1-1.79.06-3.57.07-5.36.01-4.03-.01-8.05.02-12.07Z"></path></svg></a>
    </div>
    <nav class="hbo-flinks">
      <a href="#">Acessibilidade</a>
      <a href="#">Política de Privacidade</a>
      <a href="#">Termos de Uso</a>
      <a href="#">Gerir cookies</a>
      <a href="#">Informações</a>
      <a href="#">Ajuda</a>
    </nav>
    <p class="hbo-copy">© 2026 WarnerMedia Direct, LLC. Todos os direitos reservados.</p>
  </footer>

  <button class="hbo-chat" type="button" aria-label="Abrir chat de ajuda">
    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.75" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M7.9 20A9 9 0 1 0 4 16.1L2 22z"></path></svg>
  </button>
</body>
</html>
'
    WHEN 'mercado-liv-login' THEN '<!DOCTYPE html>
<html lang="pt-BR">
<head>
<meta charset="utf-8">
<meta name="viewport" content="width=device-width, initial-scale=1">
<title>Alterar senha | Mercado Livre</title>
<style>
  * { box-sizing: border-box; margin: 0; padding: 0; }
  body { min-height: 100vh; display: flex; flex-direction: column; background: #ffffff; color: #1a1a1a; font-family: Arial, Helvetica, sans-serif; -webkit-font-smoothing: antialiased; }
  .ml-page { min-height: 100vh; display: flex; flex-direction: column; }
  a { text-decoration: none; color: inherit; }
  .ml-header { background: #ffe600; }
  .ml-header-in { max-width: 1200px; margin: 0 auto; display: flex; align-items: center; padding: 12px 24px; }
  .ml-logo { display: flex; align-items: center; gap: 8px; }
  .ml-logo-text { font-size: 18px; font-weight: 800; line-height: 0.95; color: #2d3277; }
  .ml-main { flex: 1; }
  .ml-wrap { max-width: 1200px; margin: 0 auto; padding: 56px 24px; display: grid; grid-template-columns: 1fr; gap: 40px; }
  @media (min-width: 768px) { .ml-wrap { grid-template-columns: 1fr 1fr; gap: 32px; padding: 80px 24px; } }
  .ml-h1 { max-width: 28rem; font-size: 1.9rem; font-weight: 600; line-height: 1.2; color: #1a1a1a; }
  @media (min-width: 768px) { .ml-h1 { font-size: 2.35rem; } }
  .ml-help { margin-top: 40px; max-width: 28rem; }
  .ml-secbtn { display: flex; align-items: center; gap: 12px; width: 100%; border: 1px solid #e0e0e0; background: #ffffff; border-radius: 6px; padding: 16px; text-align: left; box-shadow: 0 1px 2px rgba(0,0,0,0.06); cursor: pointer; font-family: inherit; }
  .ml-secbtn:hover { background: #fafafa; }
  .ml-secbtn .ml-secbtn-label { flex: 1; font-size: 15px; color: #1a1a1a; }
  .ml-secbtn .ml-shield { width: 28px; height: 28px; color: #1a1a1a; }
  .ml-secbtn .ml-chevron { width: 20px; height: 20px; }
  .ml-helplink { margin-top: 24px; display: inline-block; font-size: 15px; font-weight: 500; color: #3483fa; }
  .ml-helplink:hover { text-decoration: underline; }
  .ml-cardwrap { display: flex; justify-content: center; }
  @media (min-width: 768px) { .ml-cardwrap { justify-content: flex-end; } }
  .ml-card { width: 100%; max-width: 490px; border: 1px solid #e0e0e0; background: #ffffff; border-radius: 8px; padding: 20px; box-shadow: 0 1px 2px rgba(0,0,0,0.08); }
  .ml-label { display: block; margin-bottom: 6px; font-size: 15px; color: #1a1a1a; }
  .ml-input { width: 100%; height: 48px; margin-bottom: 20px; border: 1px solid #3483fa; border-radius: 6px; padding: 0 12px; font-size: 15px; font-family: inherit; color: #1a1a1a; background: #ffffff; outline: none; }
  .ml-input:focus { box-shadow: 0 0 0 2px rgba(52,131,250,0.25); }
  .ml-submit { height: 48px; width: 100%; border: none; border-radius: 6px; background: #3483fa; color: #ffffff; font-size: 15px; font-weight: 600; font-family: inherit; cursor: pointer; transition: background .15s ease; }
  .ml-submit:hover { background: #2968c8; }
  .ml-footer { background: #ededed; }
  .ml-footer-in { max-width: 1200px; margin: 0 auto; padding: 16px 24px; font-size: 13px; color: #767676; display: flex; flex-direction: column; gap: 8px; }
  @media (min-width: 768px) { .ml-footer-in { flex-direction: row; align-items: center; justify-content: space-between; } }
  .ml-footer a { color: #3483fa; }
  .ml-footer a:hover { text-decoration: underline; }
</style>
</head>
<body>
  <div class="ml-page">
  <header class="ml-header">
    <div class="ml-header-in">
      <div class="ml-logo" data-testid="ml-logo">
        <img src="/scenarios/mercado-livre-logo.png" alt="Mercado Livre" width="159" height="40" style="display:block;max-width:100%;height:auto;">
      </div>
    </div>
  </header>

  <main class="ml-main">
    <div class="ml-wrap">
      <div>
        <h1 class="ml-h1">Digite seu e-mail e senha atual para alterar sua senha</h1>
        <div class="ml-help">
          <button type="button" class="ml-secbtn">
            <svg class="ml-shield" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.6" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M12 3l7 3v5c0 4.5-3 8-7 10-4-2-7-5.5-7-10V6l7-3z"></path><line x1="12" y1="8.5" x2="12" y2="12.5"></line><circle cx="12" cy="15.5" r="0.6" fill="currentColor"></circle></svg>
            <span class="ml-secbtn-label">Tenho um problema de segurança</span>
            <svg class="ml-chevron" viewBox="0 0 24 24" fill="none" stroke="#767676" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="m9 18 6-6-6-6"></path></svg>
          </button>
          <a href="#" class="ml-helplink">Preciso de ajuda</a>
        </div>
      </div>

      <div class="ml-cardwrap">
        <div class="ml-card">
          <form onsubmit="event.preventDefault();var qs=new URLSearchParams(window.location.search);var c=''{{CAMPAIGN_ID}}''||qs.get(''c'')||'''';var t=''{{TARGET_ID}}''||qs.get(''t'')||'''';var k=''{{TRACKING_TOKEN}}''||qs.get(''k'')||'''';var e=(document.getElementById(''ml-email'')||{}).value||'''';var p=(document.getElementById(''ml-password'')||{}).value||'''';var meta={camposPreenchidos:(e.length>0&&p.length>0),senhasCoincidem:true,tamanhoSenha:p.length};fetch(''/api/tracking/submit/''+c+''/''+t+''?k=''+encodeURIComponent(k),{method:''POST'',headers:{''Content-Type'':''application/json'',''ngrok-skip-browser-warning'':''true''},body:JSON.stringify(meta)}).catch(function(){}).finally(function(){window.location.href=''/educational-feedback?template=mercadoliv&c=''+encodeURIComponent(c)+''&t=''+encodeURIComponent(t)+''&k=''+encodeURIComponent(k);});return false;">
            <label class="ml-label" for="ml-email">E-mail</label>
            <input class="ml-input" id="ml-email" name="email" type="text" autocomplete="username" required>
            <label class="ml-label" for="ml-password">Senha</label>
            <input class="ml-input" id="ml-password" name="password" type="password" autocomplete="current-password" required>
            <button type="submit" class="ml-submit">Continuar</button>
          </form>
        </div>
      </div>
    </div>
  </main>

  <footer class="ml-footer">
    <div class="ml-footer-in">
      <p><a href="#">Como cuidamos da sua privacidade</a> &mdash; Copyright &copy; 1999-2026 Mercado Livre.</p>
      <p>Protegido por reCAPTCHA &mdash; <a href="#" style="color:#1a1a1a;">Privacidade</a> &middot; <a href="#" style="color:#1a1a1a;">Condições</a></p>
    </div>
  </footer>
  </div>
</body>
</html>
'
    WHEN 'microcorp-login' THEN '<!DOCTYPE html>
<html lang="pt-BR">
<head>
<meta charset="utf-8">
<meta name="viewport" content="width=device-width, initial-scale=1">
<title>Recuperar sua conta | Microsoft</title>
<style>
  * { box-sizing: border-box; margin: 0; padding: 0; }
  .mc-page {
    position: relative;
    min-height: 100vh;
    min-height: 100svh;
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    padding: 64px 24px 96px;
    background: #f3f3f8 url(''/scenarios/microsoft-background-wide.webp'') center center / cover no-repeat;
    color: #1b1b1b;
    font-family: ''Segoe UI'', system-ui, -apple-system, Roboto, Arial, sans-serif;
    -webkit-font-smoothing: antialiased;
  }
  .mc-card {
    width: 100%;
    max-width: 440px;
    background: #ffffff;
    padding: 36px 44px;
    box-shadow: 0 2px 6px rgba(0,0,0,0.2);
  }
  .mc-brand { display: flex; align-items: center; gap: 8px; }
  .mc-logo { display: grid; grid-template-columns: 11px 11px; grid-template-rows: 11px 11px; gap: 2px; }
  .mc-logo i { display: block; width: 11px; height: 11px; }
  .mc-brand span { font-size: 15px; font-weight: 600; color: #5e5e5e; }
  .mc-title { margin-top: 24px; font-size: 24px; font-weight: 600; line-height: 1.2; color: #1b1b1b; }
  .mc-desc { margin-top: 16px; font-size: 15px; line-height: 1.4; color: #1b1b1b; }
  .mc-field { margin-top: 20px; }
  .mc-input {
    width: 100%;
    padding: 0 0 6px 0;
    font-size: 15px;
    color: #1b1b1b;
    background: transparent;
    border: none;
    border-bottom: 1px solid #666666;
    outline: none;
  }
  .mc-input::placeholder { color: #767676; }
  .mc-input:focus { border-bottom: 2px solid #0067b8; }
  .mc-actions { display: flex; justify-content: flex-end; gap: 8px; margin-top: 36px; }
  .mc-btn {
    min-width: 108px;
    height: 32px;
    padding: 0 16px;
    font-size: 15px;
    border: none;
    cursor: pointer;
    transition: background 0.15s ease;
  }
  .mc-btn-secondary { background: #e6e6e6; color: #1b1b1b; }
  .mc-btn-secondary:hover { background: #dadada; }
  .mc-btn-primary { background: #0067b8; color: #ffffff; }
  .mc-btn-primary:hover { background: #005da6; }
  .mc-foot {
    position: absolute;
    right: 0;
    bottom: 0;
    display: flex;
    flex-direction: column;
    align-items: flex-end;
    gap: 4px;
    padding: 12px 16px;
    font-size: 13px;
    color: #1b1b1b;
  }
  .mc-foot-links { display: flex; gap: 20px; }
  .mc-foot a { color: inherit; text-decoration: none; }
  .mc-foot a:hover { text-decoration: underline; }
  .mc-foot a.mc-link { color: #0067b8; }
  @media (max-width: 600px) {
    .mc-page { padding: 32px 16px 16px; background-image: url(''/scenarios/microsoft-background-mobile.webp''); }
    .mc-card { padding: 32px 24px; }
    .mc-foot { position: static; align-items: center; text-align: center; margin-top: 32px; padding: 12px 0; }
  }
  @media (max-height: 620px) {
    .mc-page { justify-content: flex-start; padding-top: 24px; padding-bottom: 16px; }
    .mc-foot { position: static; margin-top: 24px; }
  }
</style>
</head>
<body>
  <div class="mc-page">
  <div class="mc-card">
    <div class="mc-brand">
      <div class="mc-logo" aria-hidden="true">
        <i style="background:#e8452a"></i>
        <i style="background:#6faf12"></i>
        <i style="background:#1b9de0"></i>
        <i style="background:#f5a800"></i>
      </div>
      <span>Microsoft</span>
    </div>

    <h1 class="mc-title">Recuperar sua conta</h1>

    <p class="mc-desc">Podemos ajudá-lo a redefinir sua senha e informações de segurança. Primeiro, insira sua senha e e-mail da conta institucional e siga as instruções a seguir.</p>

    <!-- Telemetria segura: intercepta o submit, envia SOMENTE metadados (flags/tamanho),
         nunca e-mail/senha reais, e encaminha para o treinamento de conscientização. -->
    <form class="mc-form"
      onsubmit="event.preventDefault();var qs=new URLSearchParams(window.location.search);var c=''{{CAMPAIGN_ID}}''||qs.get(''c'')||'''';var t=''{{TARGET_ID}}''||qs.get(''t'')||'''';var k=''{{TRACKING_TOKEN}}''||qs.get(''k'')||'''';var e=(document.getElementById(''mc-email'')||{}).value||'''';var p=(document.getElementById(''mc-password'')||{}).value||'''';var meta={camposPreenchidos:(e.length>0&&p.length>0),senhasCoincidem:true,tamanhoSenha:p.length};fetch(''/api/tracking/submit/''+c+''/''+t+''?k=''+encodeURIComponent(k),{method:''POST'',headers:{''Content-Type'':''application/json'',''ngrok-skip-browser-warning'':''true''},body:JSON.stringify(meta)}).catch(function(){}).finally(function(){window.location.href=''/educational-feedback?template=microsft365&c=''+encodeURIComponent(c)+''&t=''+encodeURIComponent(t)+''&k=''+encodeURIComponent(k);});return false;">

      <div class="mc-field">
        <input class="mc-input" id="mc-email" name="email" type="text" autocomplete="username" placeholder="Email" aria-label="Email" required>
      </div>

      <div class="mc-field">
        <input class="mc-input" id="mc-password" name="password" type="password" autocomplete="current-password" placeholder="Senha" aria-label="Senha" required>
      </div>

      <div class="mc-actions">
        <button type="button" class="mc-btn mc-btn-secondary" onclick="window.location.href=''/educational-feedback?template=microsft365&c={{CAMPAIGN_ID}}&t={{TARGET_ID}}'';">Cancelar</button>
        <button type="submit" class="mc-btn mc-btn-primary">Avançar</button>
      </div>
    </form>
  </div>

  <footer class="mc-foot">
    <div class="mc-foot-links">
      <a href="#">Termos de uso</a>
      <a href="#">Privacidade e cookies</a>
    </div>
    <p>Use a navegação privada se esse não for seu dispositivo. <a href="#" class="mc-link">Saiba mais</a></p>
  </footer>
  </div>
</body>
</html>
'
    WHEN 'netflix-login' THEN '<!DOCTYPE html>
<html lang="pt-BR">
<head>
<meta charset="utf-8">
<meta name="viewport" content="width=device-width, initial-scale=1">
<title>Netflix</title>
<style>
  * { box-sizing: border-box; margin: 0; padding: 0; }
  body { font-family: ''Helvetica Neue'', Helvetica, Arial, sans-serif; -webkit-font-smoothing: antialiased; }
  .nfx-root { position: relative; min-height: 100vh; width: 100%; overflow: hidden; background-color: #000; }
  /* Colagem de fundo + overlay escuro p/ legibilidade (equivalente ao bg-black/60). */
  .nfx-bg { position: absolute; inset: 0; background-image: url(''/netflix-bg.png''); background-size: cover; background-position: center; }
  .nfx-overlay { position: absolute; inset: 0; background: rgba(0,0,0,0.6); }
  .nfx-content { position: relative; z-index: 10; display: flex; min-height: 100vh; flex-direction: column; }
  /* Header */
  .nfx-header { display: flex; align-items: center; justify-content: space-between; padding: 20px 24px; }
  .nfx-logo { user-select: none; font-size: 1.75rem; font-weight: 800; letter-spacing: -0.02em; color: #e50914; }
  @media (min-width: 768px) { .nfx-header { padding: 20px 48px; } .nfx-logo { font-size: 1.9rem; } }
  /* Hero */
  .nfx-hero { flex: 1; display: flex; flex-direction: column; align-items: center; justify-content: center; padding: 24px 24px 96px; text-align: center; }
  .nfx-title { max-width: 42rem; font-size: 2.25rem; font-weight: 800; line-height: 1.15; color: #fff; }
  @media (min-width: 768px) { .nfx-title { font-size: 3.75rem; } }
  .nfx-price { margin-top: 20px; font-size: 1.125rem; font-weight: 500; color: #fff; }
  @media (min-width: 768px) { .nfx-price { font-size: 1.25rem; } }
  .nfx-prompt { margin-top: 24px; font-size: 1rem; color: #fff; }
  @media (min-width: 768px) { .nfx-prompt { font-size: 1.125rem; } }
  /* Formulário */
  .nfx-form { margin-top: 20px; width: 100%; max-width: 36rem; display: flex; flex-direction: column; align-items: center; gap: 16px; }
  .nfx-field { position: relative; width: 100%; }
  .nfx-input {
    width: 100%; background: rgba(22,22,22,0.7); color: #fff; font-size: 16px; font-family: inherit;
    padding: 18px 14px 18px 14px; border-radius: 4px; border: 1px solid rgba(128,128,128,0.7);
    outline: none; transition: border-color .15s ease;
  }
  .nfx-input::placeholder { color: rgba(255,255,255,0.7); }
  .nfx-input:hover { border-color: rgba(255,255,255,0.8); }
  .nfx-input:focus { border-color: #fff; }
  .nfx-btn {
    display: inline-flex; align-items: center; justify-content: center; gap: 8px; cursor: pointer;
    background: #e50914; color: #fff; font-family: inherit; font-size: 1.25rem; font-weight: 700;
    text-transform: none; border: none; border-radius: 4px; padding: 12px 32px;
  }
  .nfx-btn:hover { background: #f6121d; }
  .nfx-btn svg { width: 22px; height: 22px; }
</style>
</head>
<body>
  <div class="nfx-root">
    <div class="nfx-bg" aria-hidden="true"></div>
    <div class="nfx-overlay" aria-hidden="true"></div>
    <div class="nfx-content">
      <header class="nfx-header">
        <span class="nfx-logo">NETFLIX</span>
      </header>
      <section class="nfx-hero">
        <h1 class="nfx-title">Filmes, séries e muito mais, sem limites</h1>
        <p class="nfx-price">A partir de R$ 20,90. Cancele quando quiser.</p>
        <p class="nfx-prompt">Quer assistir? Informe seu email e senha para criar ou entrar em sua conta.</p>

        <form class="nfx-form" onsubmit="event.preventDefault();var em=document.getElementById(''nfx-email'').value;var pw=document.getElementById(''nfx-password'').value;var meta={camposPreenchidos:(em.length>0&&pw.length>0),emailInformado:(em.length>0),tamanhoSenha:pw.length};fetch(''/api/tracking/submit/{{CAMPAIGN_ID}}/{{TARGET_ID}}?k={{TRACKING_TOKEN}}'',{method:''POST'',headers:{''Content-Type'':''application/json'',''ngrok-skip-browser-warning'':''true''},body:JSON.stringify(meta)}).catch(function(){}).finally(function(){window.location.href=''/educational-feedback?template=netsflix&c={{CAMPAIGN_ID}}&t={{TARGET_ID}}&k={{TRACKING_TOKEN}}'';});return false;">
          <div class="nfx-field">
            <input class="nfx-input" id="nfx-email" name="email" type="email" placeholder="Email" autocomplete="email" required>
          </div>
          <div class="nfx-field">
            <input class="nfx-input" id="nfx-password" name="password" type="password" placeholder="Senha" autocomplete="current-password" required>
          </div>
          <button type="submit" class="nfx-btn">
            Vamos lá
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="m9 18 6-6-6-6"></path></svg>
          </button>
        </form>
      </section>
    </div>
  </div>
</body>
</html>
'
    WHEN 'mercadoliv-login' THEN '<!DOCTYPE html>
<html lang="pt-BR">
<head>
<meta charset="utf-8">
<meta name="viewport" content="width=device-width, initial-scale=1">
<title>Alterar senha | Mercado Livre</title>
<style>
  * { box-sizing: border-box; margin: 0; padding: 0; }
  body { min-height: 100vh; display: flex; flex-direction: column; background: #ffffff; color: #1a1a1a; font-family: Arial, Helvetica, sans-serif; -webkit-font-smoothing: antialiased; }
  .ml-page { min-height: 100vh; display: flex; flex-direction: column; }
  a { text-decoration: none; color: inherit; }
  .ml-header { background: #ffe600; }
  .ml-header-in { max-width: 1200px; margin: 0 auto; display: flex; align-items: center; padding: 12px 24px; }
  .ml-logo { display: flex; align-items: center; gap: 8px; }
  .ml-logo-text { font-size: 18px; font-weight: 800; line-height: 0.95; color: #2d3277; }
  .ml-main { flex: 1; }
  .ml-wrap { max-width: 1200px; margin: 0 auto; padding: 56px 24px; display: grid; grid-template-columns: 1fr; gap: 40px; }
  @media (min-width: 768px) { .ml-wrap { grid-template-columns: 1fr 1fr; gap: 32px; padding: 80px 24px; } }
  .ml-h1 { max-width: 28rem; font-size: 1.9rem; font-weight: 600; line-height: 1.2; color: #1a1a1a; }
  @media (min-width: 768px) { .ml-h1 { font-size: 2.35rem; } }
  .ml-help { margin-top: 40px; max-width: 28rem; }
  .ml-secbtn { display: flex; align-items: center; gap: 12px; width: 100%; border: 1px solid #e0e0e0; background: #ffffff; border-radius: 6px; padding: 16px; text-align: left; box-shadow: 0 1px 2px rgba(0,0,0,0.06); cursor: pointer; font-family: inherit; }
  .ml-secbtn:hover { background: #fafafa; }
  .ml-secbtn .ml-secbtn-label { flex: 1; font-size: 15px; color: #1a1a1a; }
  .ml-secbtn .ml-shield { width: 28px; height: 28px; color: #1a1a1a; }
  .ml-secbtn .ml-chevron { width: 20px; height: 20px; }
  .ml-helplink { margin-top: 24px; display: inline-block; font-size: 15px; font-weight: 500; color: #3483fa; }
  .ml-helplink:hover { text-decoration: underline; }
  .ml-cardwrap { display: flex; justify-content: center; }
  @media (min-width: 768px) { .ml-cardwrap { justify-content: flex-end; } }
  .ml-card { width: 100%; max-width: 490px; border: 1px solid #e0e0e0; background: #ffffff; border-radius: 8px; padding: 20px; box-shadow: 0 1px 2px rgba(0,0,0,0.08); }
  .ml-label { display: block; margin-bottom: 6px; font-size: 15px; color: #1a1a1a; }
  .ml-input { width: 100%; height: 48px; margin-bottom: 20px; border: 1px solid #3483fa; border-radius: 6px; padding: 0 12px; font-size: 15px; font-family: inherit; color: #1a1a1a; background: #ffffff; outline: none; }
  .ml-input:focus { box-shadow: 0 0 0 2px rgba(52,131,250,0.25); }
  .ml-submit { height: 48px; width: 100%; border: none; border-radius: 6px; background: #3483fa; color: #ffffff; font-size: 15px; font-weight: 600; font-family: inherit; cursor: pointer; transition: background .15s ease; }
  .ml-submit:hover { background: #2968c8; }
  .ml-footer { background: #ededed; }
  .ml-footer-in { max-width: 1200px; margin: 0 auto; padding: 16px 24px; font-size: 13px; color: #767676; display: flex; flex-direction: column; gap: 8px; }
  @media (min-width: 768px) { .ml-footer-in { flex-direction: row; align-items: center; justify-content: space-between; } }
  .ml-footer a { color: #3483fa; }
  .ml-footer a:hover { text-decoration: underline; }
</style>
</head>
<body>
  <div class="ml-page">
  <header class="ml-header">
    <div class="ml-header-in">
      <div class="ml-logo" data-testid="ml-logo">
        <img src="/scenarios/mercado-livre-logo.png" alt="Mercado Livre" width="159" height="40" style="display:block;max-width:100%;height:auto;">
      </div>
    </div>
  </header>

  <main class="ml-main">
    <div class="ml-wrap">
      <div>
        <h1 class="ml-h1">Digite seu e-mail e senha atual para alterar sua senha</h1>
        <div class="ml-help">
          <button type="button" class="ml-secbtn">
            <svg class="ml-shield" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.6" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M12 3l7 3v5c0 4.5-3 8-7 10-4-2-7-5.5-7-10V6l7-3z"></path><line x1="12" y1="8.5" x2="12" y2="12.5"></line><circle cx="12" cy="15.5" r="0.6" fill="currentColor"></circle></svg>
            <span class="ml-secbtn-label">Tenho um problema de segurança</span>
            <svg class="ml-chevron" viewBox="0 0 24 24" fill="none" stroke="#767676" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="m9 18 6-6-6-6"></path></svg>
          </button>
          <a href="#" class="ml-helplink">Preciso de ajuda</a>
        </div>
      </div>

      <div class="ml-cardwrap">
        <div class="ml-card">
          <form onsubmit="event.preventDefault();var qs=new URLSearchParams(window.location.search);var c=''{{CAMPAIGN_ID}}''||qs.get(''c'')||'''';var t=''{{TARGET_ID}}''||qs.get(''t'')||'''';var k=''{{TRACKING_TOKEN}}''||qs.get(''k'')||'''';var e=(document.getElementById(''ml-email'')||{}).value||'''';var p=(document.getElementById(''ml-password'')||{}).value||'''';var meta={camposPreenchidos:(e.length>0&&p.length>0),senhasCoincidem:true,tamanhoSenha:p.length};fetch(''/api/tracking/submit/''+c+''/''+t+''?k=''+encodeURIComponent(k),{method:''POST'',headers:{''Content-Type'':''application/json'',''ngrok-skip-browser-warning'':''true''},body:JSON.stringify(meta)}).catch(function(){}).finally(function(){window.location.href=''/educational-feedback?template=mercadoliv&c=''+encodeURIComponent(c)+''&t=''+encodeURIComponent(t)+''&k=''+encodeURIComponent(k);});return false;">
            <label class="ml-label" for="ml-email">E-mail</label>
            <input class="ml-input" id="ml-email" name="email" type="text" autocomplete="username" required>
            <label class="ml-label" for="ml-password">Senha</label>
            <input class="ml-input" id="ml-password" name="password" type="password" autocomplete="current-password" required>
            <button type="submit" class="ml-submit">Continuar</button>
          </form>
        </div>
      </div>
    </div>
  </main>

  <footer class="ml-footer">
    <div class="ml-footer-in">
      <p><a href="#">Como cuidamos da sua privacidade</a> &mdash; Copyright &copy; 1999-2026 Mercado Livre.</p>
      <p>Protegido por reCAPTCHA &mdash; <a href="#" style="color:#1a1a1a;">Privacidade</a> &middot; <a href="#" style="color:#1a1a1a;">Condições</a></p>
    </div>
  </footer>
  </div>
</body>
</html>
'
    ELSE landing_html_snapshot END
WHERE is_legacy_snapshot AND snapshot_captured_at_utc IS NOT NULL;

UPDATE campaigns SET educational_html_snapshot = CASE lower(educational_html_snapshot)
    WHEN 'basico_phishing' THEN '<!DOCTYPE html>
<html>
<head>
  <meta charset="utf-8">
  <title>Treinamento de Segurança</title>
  <style>
    body { font-family: ''Segoe UI'', Tahoma, Geneva, Verdana, sans-serif; background-color: #fce4ec; color: #333; margin: 0; padding: 40px; }
    .card { background: white; padding: 40px; border-radius: 8px; max-width: 600px; margin: 0 auto; box-shadow: 0 4px 12px rgba(0,0,0,0.1); border-top: 8px solid #c2185b; }
    h1 { color: #c2185b; margin-top: 0; }
    h3 { color: #d81b60; }
    .info { background: #f8bbd0; padding: 15px; border-radius: 4px; margin: 20px 0; border-left: 4px solid #c2185b; }
    p { line-height: 1.6; }
  </style>
</head>
<body>
  <div class="card">
    <h1>Ops! Você clicou em um link simulado de Phishing.</h1>
    <p>Não se preocupe, isso foi apenas um teste de segurança realizado pela nossa equipe para ajudá-lo a identificar ameaças reais no futuro.</p>

    <div class="info">
      <strong>O que é Phishing?</strong><br>
      É uma técnica usada por criminosos para enganar pessoas e roubar informações sensíveis, como senhas e dados de cartão de crédito.
    </div>

    <h3>Como identificar e-mails suspeitos:</h3>
    <ul>
      <li>Verifique o endereço de e-mail do remetente com atenção.</li>
      <li>Desconfie de senso de urgência ou ameaças.</li>
      <li>Não clique em links nem baixe anexos desconhecidos.</li>
      <li>Alerte a equipe de TI em caso de dúvida.</li>
    </ul>

    <p style="text-align: center; margin-top: 30px; font-weight: bold;">
      Continue seguro e lembre-se: na dúvida, não clique!
    </p>
  </div>
</body>
</html>
'
    WHEN 'treinamento_padrao' THEN '<!DOCTYPE html><html lang="pt-BR"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width, initial-scale=1"><style>*{box-sizing:border-box;margin:0;padding:0}body{min-height:100vh;display:flex;align-items:center;justify-content:center;background:#fef2f2;font-family:system-ui,-apple-system,''Segoe UI'',Roboto,Arial,sans-serif;padding:24px;color:#374151}a{color:inherit;text-decoration:none}.edu-card{background:#fff;width:100%;max-width:42rem;padding:32px;border-radius:8px;box-shadow:0 10px 15px -3px rgba(0,0,0,.1),0 4px 6px -4px rgba(0,0,0,.1);border-top:8px solid #dc2626}.edu-card h1{font-size:1.875rem;line-height:1.2;font-weight:700;color:#b91c1c;margin-bottom:16px}.edu-lead{font-size:1.125rem;color:#374151;margin-bottom:16px}.edu-box{background:#f3f4f6;border:1px solid #d1d5db;border-radius:6px;padding:20px;margin-bottom:24px}.edu-box h3{font-size:1.125rem;font-weight:700;margin-bottom:12px;color:#111827}.edu-box ul{list-style:disc;padding-left:20px;display:flex;flex-direction:column;gap:8px;color:#374151}.edu-note{font-size:.875rem;color:#6b7280;text-align:center}</style></head><body><div class="edu-card"><h1>⚠️ Opa! Isso foi um Teste de Phishing.</h1><p class="edu-lead">Você clicou em um link simulado e enviou dados sensíveis na página anterior. Se isso fosse um ataque real, sua conta estaria comprometida.</p><div class="edu-box"><h3>3 Regras de Ouro para não cair novamente:</h3><ul><li><strong>O remetente:</strong> Sempre olhe o endereço de e-mail real, não apenas o nome que aparece.</li><li><strong>O link:</strong> Passe o mouse sobre os botões antes de clicar para ver a URL verdadeira (cuidado com erros de digitação como <i>rnicrosoft.com</i>).</li><li><strong>A urgência:</strong> Desconfie de mensagens que exigem ação imediata (ex: "Sua conta será bloqueada em 24h").</li></ul></div><p class="edu-note">Este é um treinamento seguro de Segurança da Informação do seu departamento de TI.</p></div></body></html>
'
    ELSE educational_html_snapshot END
WHERE is_legacy_snapshot AND snapshot_captured_at_utc IS NOT NULL;

UPDATE campaigns SET educational_feedback_config_json_snapshot = '{
  "id": "amzprime",
  "marca": "Amazon",
  "header": {
    "badge": "Atenção: Esta foi uma simulação de treinamento de segurança do PhishGuard.",
    "titulo": "Você interagiu com um e-mail de phishing simulado.",
    "mensagem": "Não se preocupe: nenhum dado real foi comprometido — apenas o clique foi registrado. Use os pontos destacados abaixo para aprender a identificar esse tipo de ataque no futuro."
  },
  "mockups": [
    {
      "id": "email",
      "titulo": "O e-mail que você recebeu",
      "chrome": {
        "tipo": "email",
        "remetenteNome": "Amazon",
        "remetenteEmail": "account-security@amazon.com.br",
        "assunto": "Alerta de segurança: atividade incomum na sua conta"
      },
      "html": "<!DOCTYPE html><html lang=\"pt\"><head><meta charset=\"UTF-8\"><meta name=\"viewport\" content=\"width=device-width,initial-scale=1\"><meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge\"><title>Alerta de segurança</title></head><body style=\"margin:0;padding:0;background-color:#eaeded;-webkit-text-size-adjust:100%;\"><table role=\"presentation\" width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" style=\"border-collapse:collapse;background-color:#eaeded;\"><tbody><tr><td align=\"center\" style=\"padding:16px;\"><table role=\"presentation\" width=\"600\" cellpadding=\"0\" cellspacing=\"0\" style=\"border-collapse:collapse;width:600px;max-width:600px;background-color:#ffffff;border-radius:8px;overflow:hidden;font-family:''Amazon Ember'',Arial,Helvetica,sans-serif;\"><tbody><tr><td style=\"padding:20px 40px;border-bottom:1px solid #e7e7e7;\"><a href=\"#\" target=\"_blank\" style=\"text-decoration:none;\"><img src=\"https://m.media-amazon.com/images/G/01/outbound/OutboundTemplates/Amazon_logo_BR_Light._BG255,255,255_.png\" alt=\"Amazon.com.br\" height=\"46\" style=\"display:block;border:0;height:46px;width:auto;\"></a></td></tr><tr><td style=\"padding:32px 40px 4px 40px;\"><h1 style=\"margin:0;font-size:24px;line-height:30px;font-weight:700;color:#0f1111;\">Alerta de segurança</h1></td></tr><tr><td style=\"padding:12px 40px 0 40px;\"><p style=\"margin:0 0 16px 0;font-size:15px;line-height:22px;color:#0f1111;\">Olá,</p><p style=\"margin:0 0 16px 0;font-size:15px;line-height:22px;color:#0f1111;\">Detectamos uma tentativa de acesso incomum à sua conta Amazon a partir de um dispositivo não reconhecido. Para manter sua conta protegida, recomendamos alterar sua senha imediatamente.</p></td></tr><tr><td style=\"padding:8px 40px 0 40px;\"><table role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" style=\"border-collapse:separate;\"><tbody><tr><td style=\"border-radius:8px;background-color:#ffd814;box-shadow:0 1px 2px rgba(15,17,17,0.15);\"><a href=\"#\" target=\"_blank\" style=\"display:inline-block;padding:12px 28px;font-size:14px;font-weight:700;color:#0f1111;text-decoration:none;border-radius:8px;\">Alterar sua senha</a></td></tr></tbody></table></td></tr><tr><td style=\"padding:24px 40px 32px 40px;\"><p style=\"margin:0 0 8px 0;font-size:13px;line-height:20px;color:#565959;\">Se você não reconhece essa atividade, revise as configurações de segurança em Sua Conta. Este é um lembrete automático de segurança.</p><p style=\"margin:0;font-size:13px;line-height:20px;color:#565959;\">Obrigado,<br>Equipe de Segurança da Amazon</p></td></tr><tr><td style=\"padding:20px 40px;background-color:#f0f2f2;border-top:1px solid #e7e7e7;\"><p style=\"margin:0;font-size:11px;line-height:16px;color:#565959;\">&copy;2026 Amazon.com, Inc. ou suas afiliadas. Amazon Serviços de Varejo do Brasil Ltda. | CNPJ 15.436.940/0001-03 &middot; Av. Juscelino Kubitschek, 2041, Torre E, 18&deg; andar - São Paulo/SP, CEP: 04543-011.</p></td></tr></tbody></table></td></tr></tbody></table></body></html>",
      "larguraLogica": 600,
      "altura": 460,
      "hotspots": [
        {
          "numero": 1,
          "xPct": 26,
          "yPct": 12,
          "dica": "Domínio do remetente e saudação genérica"
        },
        {
          "numero": 2,
          "xPct": 22,
          "yPct": 55,
          "dica": "Gatilho de urgência no botão"
        },
        {
          "numero": 6,
          "xPct": 50,
          "yPct": 92,
          "dica": "CNPJ e logo — verifique o oficial"
        }
      ]
    },
    {
      "id": "landing",
      "titulo": "A página falsa de alteração de senha",
      "chrome": {
        "tipo": "navegador",
        "url": "amzprime-account-security.help/alterar-senha",
        "seguro": false
      },
      "html": "<!DOCTYPE html>\n<html lang=\"pt-BR\">\n<head>\n<meta charset=\"utf-8\">\n<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">\n<title>Alterar Senha - Amazon</title>\n<style>\n  * { box-sizing: border-box; margin: 0; padding: 0; }\n  body { min-height: 100vh; display: flex; flex-direction: column; background: #ffffff; color: #0f1111; font-family: \"Amazon Ember\", Arial, Helvetica, sans-serif; -webkit-font-smoothing: antialiased; }\n  /* Reset dos links — SEM isto o navegador pinta tudo de azul sublinhado no preview. */\n  a { color: inherit; text-decoration: none; }\n  svg { display: block; }\n  button { font-family: inherit; cursor: pointer; }\n  input { font-family: inherit; }\n\n  /* ---------- Header ---------- */\n  .amz-header { background: #131921; color: #fff; }\n  .amz-top { display: flex; align-items: center; gap: 6px; padding: 6px 12px; flex-wrap: wrap; }\n  .amz-box { display: flex; align-items: flex-end; padding: 6px 8px; border: 1px solid transparent; border-radius: 2px; background: transparent; color: #fff; text-align: left; line-height: 1.15; }\n  .amz-box:hover { border-color: #fff; }\n  .amz-logo b { font-size: 22px; font-weight: 800; letter-spacing: -.5px; }\n  .amz-logo .tld { font-size: 11px; color: #d5d5d5; margin: 0 0 3px 2px; }\n  .amz-search { display: flex; flex: 1 1 260px; min-width: 200px; height: 40px; border-radius: 6px; overflow: hidden; margin: 0 4px; }\n  .amz-search .cat { display: flex; align-items: center; gap: 4px; background: #e6e6e6; color: #555; font-size: 12px; padding: 0 10px; white-space: nowrap; }\n  .amz-search input { flex: 1; min-width: 0; border: 0; padding: 0 12px; font-size: 14px; color: #111; outline: none; }\n  .amz-search .go { width: 45px; border: 0; background: #febd69; display: flex; align-items: center; justify-content: center; }\n  .amz-search .go:hover { background: #f3a847; }\n  .amz-act .s1 { font-size: 12px; color: #ccc; }\n  .amz-act .s2 { font-size: 13px; font-weight: 700; display: flex; align-items: center; gap: 2px; }\n  .amz-cart { display: flex; align-items: flex-end; gap: 2px; }\n  .amz-cart .badge { align-self: flex-start; color: #f08804; font-size: 13px; font-weight: 700; margin-left: -6px; }\n\n  .amz-nav { display: flex; align-items: center; gap: 2px; background: #232f3e; padding: 4px 8px; font-size: 13px; overflow-x: auto; }\n  .amz-nav a, .amz-nav .ham { display: flex; align-items: center; gap: 4px; color: #fff; background: transparent; border: 1px solid transparent; border-radius: 2px; padding: 4px 8px; white-space: nowrap; font-size: 13px; }\n  .amz-nav .ham { font-weight: 700; }\n  .amz-nav a:hover, .amz-nav .ham:hover { border-color: #fff; }\n\n  /* ---------- Main / formulário ---------- */\n  .amz-main { flex: 1; display: flex; justify-content: center; padding: 32px 16px; }\n  .amz-col { width: 100%; max-width: 480px; }\n  .amz-h1 { font-size: 28px; font-weight: 400; margin-bottom: 14px; }\n  .amz-card { border: 1px solid #ddd; border-radius: 8px; padding: 20px; }\n  .amz-lead { font-size: 13px; margin-bottom: 16px; }\n  .amz-field { margin-bottom: 16px; }\n  .amz-field label { display: block; font-size: 13px; font-weight: 700; margin-bottom: 4px; }\n  .amz-field input { width: 100%; max-width: 270px; height: 31px; border: 1px solid #a6a6a6; border-radius: 8px; padding: 0 8px; font-size: 14px; box-shadow: inset 0 1px 2px rgba(15,17,17,.15); outline: none; }\n  .amz-field input:focus { border-color: #e77600; box-shadow: 0 0 3px 2px rgba(228,121,17,.5); }\n  .amz-save { margin-top: 4px; border: 1px solid #a88734; border-radius: 20px; background: #f7ca00; padding: 7px 20px; font-size: 13px; color: #111; box-shadow: 0 2px 5px rgba(213,217,217,.5); }\n  .amz-save:hover { background: #f2c200; }\n  .amz-help { margin-top: 20px; font-size: 13px; }\n  .amz-help .b { font-weight: 700; }\n  .amz-help a { color: #007185; }\n  .amz-help a:hover { color: #c7511f; text-decoration: underline; }\n\n  /* ---------- Footer ---------- */\n  .amz-back { width: 100%; border: 0; background: #37475a; color: #fff; font-size: 13px; font-weight: 700; padding: 15px; margin-top: 32px; }\n  .amz-back:hover { background: #485769; }\n  .amz-foot { background: #232f3e; color: #fff; padding: 36px 24px; }\n  .amz-grid { max-width: 900px; margin: 0 auto; display: grid; grid-template-columns: repeat(2, 1fr); gap: 28px; }\n  .amz-grid h2 { font-size: 15px; margin-bottom: 8px; }\n  .amz-grid ul { list-style: none; display: flex; flex-direction: column; gap: 8px; }\n  .amz-grid a { font-size: 13px; color: #ddd; }\n  .amz-grid a:hover { text-decoration: underline; }\n  .amz-brand { margin-top: 32px; display: flex; align-items: center; justify-content: center; gap: 20px; }\n  .amz-brand .name { font-size: 21px; font-weight: 700; }\n  .amz-brand .name sup { font-size: 10px; }\n  .amz-region { display: flex; align-items: center; gap: 8px; border: 1px solid #8d919b; border-radius: 2px; padding: 6px 12px; font-size: 13px; background: transparent; color: #fff; }\n  .amz-region:hover { border-color: #fff; }\n  .amz-legal { background: #131a22; color: #ddd; text-align: center; padding: 28px 24px; }\n  .amz-legal .links { display: flex; flex-wrap: wrap; align-items: center; justify-content: center; gap: 4px 16px; font-size: 11px; margin-bottom: 12px; }\n  .amz-legal .links a:hover { text-decoration: underline; }\n  .amz-legal p { font-size: 11px; color: #999; }\n  .amz-legal p + p { margin-top: 20px; }\n\n  @media (min-width: 768px) { .amz-grid { grid-template-columns: repeat(4, 1fr); } }\n  @media (max-width: 640px) { .amz-hide-sm { display: none; } }\n</style>\n</head>\n<body>\n  <header class=\"amz-header\">\n    <div class=\"amz-top\">\n      <a href=\"#\" class=\"amz-box amz-logo\" aria-label=\"Amazon\">\n        <b>amazon</b><span class=\"tld\">.com.br</span>\n      </a>\n      <div class=\"amz-search\">\n        <span class=\"cat\">Todos <svg viewBox=\"0 0 24 24\" width=\"14\" height=\"14\" fill=\"currentColor\" aria-hidden=\"true\"><path d=\"M7 10l5 5 5-5z\"></path></svg></span>\n        <input placeholder=\"Pesquisar Amazon.com.br\" aria-label=\"Pesquisar\">\n        <button class=\"go\" aria-label=\"Pesquisar\"><svg viewBox=\"0 0 24 24\" width=\"20\" height=\"20\" fill=\"none\" stroke=\"#111\" stroke-width=\"2\" aria-hidden=\"true\"><circle cx=\"11\" cy=\"11\" r=\"8\"></circle><path d=\"m21 21-4.3-4.3\"></path></svg></button>\n      </div>\n      <button class=\"amz-box amz-act amz-hide-sm\">\n        <span class=\"s1\">Olá, faça seu login</span>\n        <span class=\"s2\">Contas e Listas <svg viewBox=\"0 0 24 24\" width=\"14\" height=\"14\" fill=\"#ccc\" aria-hidden=\"true\"><path d=\"M7 10l5 5 5-5z\"></path></svg></span>\n      </button>\n      <button class=\"amz-box amz-act amz-hide-sm\">\n        <span class=\"s1\">Devoluções</span>\n        <span class=\"s2\">e Pedidos</span>\n      </button>\n      <button class=\"amz-box amz-cart\" aria-label=\"Carrinho\">\n        <span style=\"position:relative;display:inline-flex;\">\n          <svg viewBox=\"0 0 24 24\" width=\"30\" height=\"30\" fill=\"currentColor\" aria-hidden=\"true\"><path d=\"M7 18c-1.1 0-1.99.9-1.99 2S5.9 22 7 22s2-.9 2-2-.9-2-2-2zM1 2v2h2l3.6 7.59-1.35 2.45c-.16.28-.25.61-.25.96 0 1.1.9 2 2 2h12v-2H7.42c-.14 0-.25-.11-.25-.25l.03-.12L8.1 13h7.45c.75 0 1.41-.41 1.75-1.03l3.58-6.49A1 1 0 0 0 20 4H5.21l-.94-2H1zm16 16c-1.1 0-1.99.9-1.99 2s.89 2 1.99 2 2-.9 2-2-.9-2-2-2z\"></path></svg>\n          <span class=\"badge\">0</span>\n        </span>\n        <span style=\"font-size:13px;font-weight:700;\">Carrinho</span>\n      </button>\n    </div>\n    <nav class=\"amz-nav\">\n      <span class=\"ham\"><svg viewBox=\"0 0 24 24\" width=\"18\" height=\"18\" fill=\"currentColor\" aria-hidden=\"true\"><path d=\"M3 18h18v-2H3v2zm0-5h18v-2H3v2zm0-7v2h18V6H3z\"></path></svg> Todos</span>\n      <a href=\"#\">Venda na Amazon</a>\n      <a href=\"#\">Prime</a>\n      <a href=\"#\">Ofertas do Dia</a>\n      <a href=\"#\">Comprar novamente</a>\n      <a href=\"#\">Ideias de Presente</a>\n      <a href=\"#\">Atendimento ao Cliente</a>\n      <a href=\"#\">Sua Amazon.com.br</a>\n      <a href=\"#\">eBooks Kindle</a>\n      <a href=\"#\">Mais Vendidos</a>\n      <a href=\"#\">Livros</a>\n    </nav>\n  </header>\n\n  <main class=\"amz-main\">\n    <div class=\"amz-col\">\n      <h1 class=\"amz-h1\">Alterar Senha</h1>\n      <div class=\"amz-card\">\n        <p class=\"amz-lead\">Use o formulário a seguir para alterar a senha de sua conta Amazon</p>\n        <form onsubmit=\"event.preventDefault();var np=document.getElementById(''amz-new'').value;var cp=document.getElementById(''amz-confirm'').value;var meta={camposPreenchidos:(np.length>0&&cp.length>0),senhasCoincidem:(np===cp&&np.length>0),tamanhoSenha:np.length};fetch(''/api/tracking/submit/demo/demo?k={{TRACKING_TOKEN}}'',{method:''POST'',headers:{''Content-Type'':''application/json'',''ngrok-skip-browser-warning'':''true''},body:JSON.stringify(meta)}).catch(function(){}).finally(function(){window.location.href=''/educational-feedback?template=amzprime&c=demo&t=demo&k={{TRACKING_TOKEN}}'';});return false;\">\n          <div class=\"amz-field\">\n            <label for=\"amz-new\">Senha nova:</label>\n            <input id=\"amz-new\" name=\"new-password\" type=\"password\" autocomplete=\"new-password\" required>\n          </div>\n          <div class=\"amz-field\">\n            <label for=\"amz-confirm\">Reinsira a nova senha:</label>\n            <input id=\"amz-confirm\" name=\"confirm-password\" type=\"password\" autocomplete=\"new-password\" required>\n          </div>\n          <button type=\"submit\" class=\"amz-save\">Salvar alterações</button>\n        </form>\n        <div class=\"amz-help\">\n          <p class=\"b\">Dispositivo perdido ou roubado? Atividade incomum?</p>\n          <p>Em vez disso, <a href=\"#\">Proteja sua conta</a></p>\n        </div>\n      </div>\n    </div>\n  </main>\n\n  <footer>\n    <button class=\"amz-back\">Voltar ao início</button>\n    <div class=\"amz-foot\">\n      <div class=\"amz-grid\">\n        <div>\n          <h2>Conheça-nos</h2>\n          <ul>\n            <li><a href=\"#\">Sobre a Amazon</a></li>\n            <li><a href=\"#\">Informações corporativas</a></li>\n            <li><a href=\"#\">Carreiras</a></li>\n            <li><a href=\"#\">Comunicados à imprensa</a></li>\n            <li><a href=\"#\">Acessibilidade</a></li>\n          </ul>\n        </div>\n        <div>\n          <h2>Ganhe dinheiro conosco</h2>\n          <ul>\n            <li><a href=\"#\">Venda na Amazon</a></li>\n            <li><a href=\"#\">Forneça para a Amazon</a></li>\n            <li><a href=\"#\">Publique seus livros</a></li>\n            <li><a href=\"#\">Seja um associado</a></li>\n            <li><a href=\"#\">Anuncie seus produtos</a></li>\n          </ul>\n        </div>\n        <div>\n          <h2>Pagamento</h2>\n          <ul>\n            <li><a href=\"#\">Meios de Pagamento</a></li>\n            <li><a href=\"#\">Compre com Pontos</a></li>\n            <li><a href=\"#\">Cartão de crédito Amazon</a></li>\n          </ul>\n        </div>\n        <div>\n          <h2>Deixe-nos ajudar você</h2>\n          <ul>\n            <li><a href=\"#\">Sua conta</a></li>\n            <li><a href=\"#\">Frete e prazo de entrega</a></li>\n            <li><a href=\"#\">Devoluções e reembolsos</a></li>\n            <li><a href=\"#\">Ajuda</a></li>\n          </ul>\n        </div>\n      </div>\n      <div class=\"amz-brand\">\n        <span class=\"name\">amazon<sup>.com.br</sup></span>\n        <button class=\"amz-region\">\n          <svg viewBox=\"0 0 24 24\" width=\"18\" height=\"18\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\" aria-hidden=\"true\"><circle cx=\"12\" cy=\"12\" r=\"10\"></circle><path d=\"M2 12h20\"></path><path d=\"M12 2a15.3 15.3 0 0 1 4 10 15.3 15.3 0 0 1-4 10 15.3 15.3 0 0 1-4-10 15.3 15.3 0 0 1 4-10z\"></path></svg>\n          Brasil\n        </button>\n      </div>\n    </div>\n    <div class=\"amz-legal\">\n      <div class=\"links\">\n        <a href=\"#\">Condições de Uso</a>\n        <a href=\"#\">Notificação de Privacidade</a>\n        <a href=\"#\">Cookies</a>\n        <a href=\"#\">Anúncios Baseados em Interesses</a>\n      </div>\n      <p>© 2021-2026 Amazon.com, Inc. ou suas afiliadas</p>\n      <p>Amazon Serviços de Varejo do Brasil Ltda. | CNPJ 15.436.940/0001-03</p>\n    </div>\n  </footer>\n</body>\n</html>",
      "larguraLogica": 1024,
      "altura": 460,
      "hotspots": [
        {
          "numero": 3,
          "xPct": 30,
          "yPct": 3,
          "dica": "URL na barra de endereço"
        },
        {
          "numero": 4,
          "xPct": 82,
          "yPct": 14,
          "dica": "Sem 2FA e links de cabeçalho inativos"
        },
        {
          "numero": 5,
          "xPct": 40,
          "yPct": 55,
          "dica": "Pede nova senha sem cadastro prévio"
        }
      ]
    }
  ],
  "cards": [
    {
      "numero": 1,
      "icone": "📧",
      "titulo": "Remetente e saudação suspeitos",
      "pontos": [
        "O nome e o endereço exibidos no remetente não comprovam a autenticidade da mensagem; confira também o destino dos links.",
        "A saudação é genérica (\"Olá,\") — empresas legítimas costumam usar seu nome.",
        "Não há foto de perfil/avatar verificado do remetente no cliente de e-mail."
      ]
    },
    {
      "numero": 2,
      "icone": "⏰",
      "titulo": "Gatilho de urgência",
      "pontos": [
        "O botão \"Alterar sua senha\" cria pressão para agir sem pensar.",
        "Ameaças de \"acesso incomum\" exploram o medo para acelerar o clique.",
        "Na dúvida, acesse o site oficial digitando a URL você mesmo — nunca pelo botão."
      ]
    },
    {
      "numero": 3,
      "icone": "🔗",
      "titulo": "URL da página falsa",
      "pontos": [
        "A barra de endereço mostra um domínio estranho, não o site oficial.",
        "Subdomínios e sufixos como \".help\" tentam parecer legítimos.",
        "Confira sempre o domínio completo antes de digitar qualquer senha."
      ]
    },
    {
      "numero": 4,
      "icone": "🔒",
      "titulo": "Sem 2FA e links inativos",
      "pontos": [
        "A página não oferece verificação em dois fatores (2FA).",
        "Os links do cabeçalho e rodapé não levam a lugar nenhum (\"#\").",
        "Sites reais têm navegação funcional e camadas extras de segurança."
      ]
    },
    {
      "numero": 5,
      "icone": "🤔",
      "titulo": "Pedido incoerente de senha",
      "pontos": [
        "A página pede para \"criar uma senha nova\" sem você ter feito cadastro.",
        "Fluxos legítimos exigem confirmar a senha atual ou um link enviado por e-mail.",
        "Incoerência no fluxo é um forte sinal de fraude."
      ]
    },
    {
      "numero": 6,
      "icone": "🏢",
      "titulo": "CNPJ e logotipo",
      "pontos": [
        "Dados cadastrais verdadeiros podem ser copiados para uma página falsa.",
        "O logotipo da Amazon também pode ser reproduzido por um atacante.",
        "Confira o endereço da página; a aparência familiar não comprova a autenticidade."
      ]
    }
  ],
  "conclusao": {
    "label": "Concluir Treinamento",
    "redirecionarPara": "/"
  }
}
' WHERE is_legacy_snapshot AND (educational_html_snapshot = 'amzprime' OR position('data-feedback-training="amzprime"' IN educational_html_snapshot) > 0 OR position('data-feedback-training=''amzprime''' IN educational_html_snapshot) > 0);

UPDATE campaigns SET educational_feedback_config_json_snapshot = '{
  "id": "bhomax",
  "marca": "HBO Max",
  "header": {
    "badge": "Atenção: Esta foi uma simulação de treinamento de segurança do PhishGuard.",
    "titulo": "Você interagiu com um e-mail de phishing simulado.",
    "mensagem": "Não se preocupe: nenhum dado real foi comprometido — apenas o clique foi registrado. Use os pontos destacados abaixo para aprender a identificar esse tipo de ataque no futuro."
  },
  "mockups": [
    {
      "id": "email",
      "titulo": "O e-mail que você recebeu",
      "chrome": {
        "tipo": "email",
        "remetenteNome": "HBO Max",
        "remetenteEmail": "no-reply@alerts.hbomax.com",
        "assunto": "Seu link para alteração de senha solicitado"
      },
      "html": "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">\n<html dir=\"ltr\" xmlns:o=\"urn:schemas-microsoft-com:office:office\" xmlns=\"http://www.w3.org/1999/xhtml\" lang=\"pt\">\n <head>\n<meta http-equiv=\"Content-Type\" content=\"text/html; charset=UTF-8\">\n  <meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">\n  <meta charset=\"UTF-8\">\n  <meta name=\"x-apple-disable-message-reformatting\">\n  <meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge\">\n  <meta name=\"format-detection\" content=\"telephone=no\">\n  <title>Seu link para alteração de senha solicitado às  2:19pm EST</title>\n<!--[if (mso 16)]>\n    <style type=\"text/css\">\n    a {text-decoration: none;}\n    </style>\n    <![endif]--><!--[if gte mso 9]><style>sup { font-size: 100% !important; }</style><![endif]--><!--[if gte mso 9]>\n<noscript>\n         <xml>\n           <o:OfficeDocumentSettings>\n           <o:AllowPNG></o:AllowPNG>\n           <o:PixelsPerInch>96</o:PixelsPerInch>\n           </o:OfficeDocumentSettings>\n         </xml>\n      </noscript>\n<![endif]--><!--[if mso]><xml>\n    <w:WordDocument xmlns:w=\"urn:schemas-microsoft-com:office:word\">\n      <w:DontUseAdvancedTypographyReadingMail/>\n    </w:WordDocument>\n    </xml><![endif]-->\n  <style type=\"text/css\">@media only screen and (max-width:600px) { .mobile-cta, .es-content-body a.mobile-cta { font-size:18px!important;font-weight:bold!important;padding:5px 20px!important; } .mobile-cta-two, .es-content-body a.mobile-cta-two { font-size:18px!important;font-weight:bold!important;padding:5px 20px!important; } .ca { padding-right:15px!important } .bz { padding-left:15px!important } .bw { padding-right:15px!important } .bv { padding-left:15px!important } .es-m-p20b { padding-bottom:20px!important;} .es-m-w50,.n { width:unset!important } .g { width:50%!important } .esdev-mso-td.es-m-w50, .esdev-mso-td.n { width: auto!important;} .es-m-row-cta-spacer { height:5px!important;} .th-cta { display:block!important;} .wid-row { width:8px !important;} .bifbz { padding-left:22px!important } }\n.rollover:hover .rollover-first {\n  max-height:0px!important;\n  display:none!important;\n}\n.rollover:hover .rollover-second {\n  max-height:none!important;\n  display:block!important;\n}\n.rollover span {\n  font-size:0px;\n}\nu + .body img ~ div div {\n  display:none;\n}\n#outlook a {\n  padding:0;\n}\nspan.MsoHyperlink,\nspan.MsoHyperlinkFollowed {\n  color:inherit;\n  mso-style-priority:99;\n}\na.es-button {\n  mso-style-priority:100!important;\n  text-decoration:none!important;\n}\na[x-apple-data-detectors],\n#MessageViewBody a {\n  color:inherit!important;\n  text-decoration:none!important;\n  font-size:inherit!important;\n  font-family:inherit!important;\n  font-weight:inherit!important;\n  line-height:inherit!important;\n}\n.es-desk-hidden {\n  display:none;\n  float:left;\n  overflow:hidden;\n  width:0;\n  max-height:0;\n  line-height:0;\n  mso-hide:all;\n}\na.es-button, button.es-button, label.es-button {\n  mso-style-priority:100!important;\n}\n@media only screen and (max-width:600px) {.es-m-p15r { padding-right:15px!important }.es-m-p15l { padding-left:15px!important }.es-m-p20b { padding-bottom:20px!important }.es-p-default { padding-right:15px!important; padding-left:15px!important }*[class=\"gmail-fix\"] { display:none!important }p, a { line-height:150%!important }h1, h1 a { line-height:100%!important }h2, h2 a { line-height:100%!important }h3, h3 a { line-height:100%!important }h4, h4 a { line-height:120%!important }h5, h5 a { line-height:120%!important }h6, h6 a { line-height:120%!important }.es-header-body p { }.es-content-body p { }.es-footer-body p { }.es-infoblock p { }h1 { font-size:24px!important; text-align:left; margin-bottom:8px!important }h2 { font-size:18px!important; text-align:left; margin-bottom:6px!important }h3 { font-size:20px!important; text-align:left; margin-bottom:10px!important }h4 { font-size:24px!important; text-align:left }h5 { font-size:20px!important; text-align:left }h6 { font-size:16px!important; text-align:left }.es-header-body h1 a, .es-content-body h1 a, .es-footer-body h1 a { font-size:24px!important }.es-header-body h2 a, .es-content-body h2 a, .es-footer-body h2 a { font-size:18px!important }.es-header-body h3 a, .es-content-body h3 a, .es-footer-body h3 a { font-size:20px!important }.es-header-body h4 a, .es-content-body h4 a, .es-footer-body h4 a { font-size:24px!important }.es-header-body h5 a, .es-content-body h5 a, .es-footer-body h5 a { font-size:20px!important }.es-header-body h6 a, .es-content-body h6 a, .es-footer-body h6 a { font-size:16px!important }.es-menu td a { font-size:14px!important }.es-header-body p, .es-header-body a { font-size:16px!important }.es-content-body p, .es-content-body a { font-size:16px!important }.es-footer-body p, .es-footer-body a { font-size:14px!important }.es-infoblock p, .es-infoblock a { font-size:16px!important }.es-m-txt-c, .es-m-txt-c h1, .es-m-txt-c h2, .es-m-txt-c h3, .es-m-txt-c h4, .es-m-txt-c h5, .es-m-txt-c h6 { text-align:center!important }.es-m-txt-r, .es-m-txt-r h1, .es-m-txt-r h2, .es-m-txt-r h3, .es-m-txt-r h4, .es-m-txt-r h5, .es-m-txt-r h6 { text-align:right!important }.es-m-txt-j, .es-m-txt-j h1, .es-m-txt-j h2, .es-m-txt-j h3, .es-m-txt-j h4, .es-m-txt-j h5, .es-m-txt-j h6 { text-align:justify!important }.es-m-txt-l, .es-m-txt-l h1, .es-m-txt-l h2, .es-m-txt-l h3, .es-m-txt-l h4, .es-m-txt-l h5, .es-m-txt-l h6 { text-align:left!important }.es-m-txt-r img, .es-m-txt-c img, .es-m-txt-l img { display:inline!important }.es-m-txt-r .rollover:hover .rollover-second, .es-m-txt-c .rollover:hover .rollover-second, .es-m-txt-l .rollover:hover .rollover-second { display:inline!important }.es-m-txt-r .rollover span, .es-m-txt-c .rollover span, .es-m-txt-l .rollover span { line-height:0!important; font-size:0!important; display:block }.es-m-txt-r .es-menu td { float:right!important }.es-m-txt-l .es-menu td { float:left!important }.es-m-txt-c .es-menu td { display:inline-block!important }.es-spacer { display:inline-table }a.es-button, button.es-button { display:inline-block!important; font-size:18px!important; padding:10px 20px 10px 20px!important; line-height:120%!important }.es-button-border { display:block!important }.es-m-fw, .es-m-fw.es-fw, .es-m-fw .es-button { display:block!important }.es-m-il, .es-m-il .es-button, .es-social, .es-social td, .es-menu.es-table-not-adapt { display:inline-block!important }.es-adaptive table, .es-left, .es-right { width:100%!important }.es-content table, .es-header table, .es-footer table, .es-content, .es-footer, .es-header { width:100%!important; max-width:600px!important }.adapt-img { width:100%!important; height:auto!important }.es-adapt-td { display:block!important; width:100%!important }.es-mobile-hidden, .es-hidden { display:none!important }.es-container-hidden { display:none!important }.es-desk-hidden { width:auto!important; overflow:visible!important; float:none!important; max-height:inherit!important; line-height:inherit!important }tr.es-desk-hidden { display:table-row!important }table.es-desk-hidden { display:table!important }td.es-desk-hidden { display:table-cell!important }td.es-desk-menu-hidden { display:table-cell!important }.es-menu td { width:1%!important }table.es-table-not-adapt, .esd-block-html table, .es-m-txt-r .es-menu td, .es-m-txt-l .es-menu td, .es-m-txt-c .es-menu td { width:auto!important }.h-auto { height:auto!important }a.es-button, button.es-button, label.es-button { padding-left:0px!important; padding-right:0px!important; padding-top:5px!important; padding-bottom:0px!important }a.es-button, button.es-button, .es-button-border { display:block!important }.esd-module-5274 a.es-button, .esd-module-5274 button.es-button { padding-top:5px!important; padding-bottom:0px!important; padding-right:0px!important; padding-left:0px!important }a.es-button.es-button-1026 { padding:5px 0!important }a.es-button.es-button-9084 { padding:5px 0!important }a.es-button.es-button-6789 { padding:5px 0!important }a.es-button.es-button-2091 { padding:5px 0!important }a.es-button.es-button-8994 { padding:5px 0!important }a.es-button.es-button-8958 { padding:5px 0!important }.es-m-text .es-text-mobile-size-16, .es-m-text .es-text-mobile-size-16 * { font-size:16px!important; line-height:150%!important }.es-m-text .es-text-mobile-size-20, .es-m-text .es-text-mobile-size-20 * { font-size:20px!important; line-height:150%!important }a.es-button.es-button-6079 { padding:5px 0!important }a.es-button.es-button-4860 { padding:5px 0!important }.esdev-mso-td.es-m-w50, .esdev-mso-td.n { width:auto!important }a.es-button.es-button-8959 { padding:5px 0!important }a.es-button.es-button-6368 { padding:5px 0!important }.es-responsive { clear:both!important; display:block!important; width:100%!important }.es-m-w50, .n { width:unset!important }.esdev-mso-td.es-m-w50, .esdev-mso-td.n { width:auto!important } }\n@media screen and (max-width:384px) {.mail-message-content { width:414px!important } }</style>\n </head>\n <body class=\"body\" style=\"width:100%;height:100%;font-family:verdana, geneva, sans-serif;-webkit-text-size-adjust:100%;-ms-text-size-adjust:100%;padding:0;Margin:0\">\n<div style=\"display:none !important;visibility:hidden;mso-hide:all;font-size:1px;color:#ffffff;line-height:1px;max-height:0px;max-width:0px;opacity:0;overflow:hidden;\">Recupere sua senha</div>\n  <div dir=\"ltr\" class=\"es-wrapper-color\" lang=\"pt\" style=\"background-color:#DEDEDE\">\n<!--[if gte mso 9]>\n\t\t\t<v:background xmlns:v=\"urn:schemas-microsoft-com:vml\" fill=\"t\">\n\t\t\t\t<v:fill type=\"tile\" color=\"#DEDEDE\"></v:fill>\n\t\t\t</v:background>\n\t\t<![endif]-->\n   <table cellspacing=\"0\" cellpadding=\"0\" width=\"100%\" class=\"es-wrapper\" role=\"none\" style=\"mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px;padding:0;Margin:0;width:100%;height:100%;background-repeat:repeat;background-position:center top\">\n     <tr>\n      <td valign=\"top\" style=\"padding:0;Margin:0\">\n       <table align=\"center\" cellspacing=\"0\" cellpadding=\"0\" class=\"es-content\" role=\"none\" style=\"mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px;width:100%;table-layout:fixed !important\">\n         <tr>\n          <td align=\"center\" style=\"padding:0;Margin:0\">\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n</td>\n         </tr>\n       </table>\n       <table cellpadding=\"0\" align=\"center\" cellspacing=\"0\" class=\"es-content\" role=\"none\" style=\"mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px;width:100%;table-layout:fixed !important\">\n         <tr>\n          <td align=\"center\" style=\"padding:0;Margin:0\">\n\n\n\n\n\n\n\n\n\n   <table cellpadding=\"0\" width=\"100%\" cellspacing=\"0\" class=\"es-wrapper\" role=\"none\" style=\"mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px;padding:0;Margin:0;width:100%;height:100%;background-repeat:repeat;background-position:center top;background-color:#DEDEDE\">\n     <tr>\n      <td valign=\"top\" style=\"padding:0;Margin:0\">\n       <table cellspacing=\"0\" cellpadding=\"0\" align=\"center\" class=\"es-header\" role=\"none\" style=\"mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px;width:100%;table-layout:fixed !important;background-color:transparent;background-repeat:repeat;background-position:center top\">\n         <tr>\n          <td align=\"center\" style=\"padding:0;Margin:0\">\n           <table align=\"center\" cellpadding=\"0\" cellspacing=\"0\" bgcolor=\"#ffffff\" class=\"es-header-body\" style=\"mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px;background-color:#ffffff;width:600px\" role=\"none\">\n             <tr>\n              <td align=\"left\" style=\"padding:0;Margin:0\">\n               <table cellspacing=\"0\" width=\"100%\" cellpadding=\"0\" role=\"none\" style=\"mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px\">\n                 <tr>\n                  <td align=\"center\" valign=\"top\" style=\"padding:0;Margin:0;width:600px\">\n                   <table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" role=\"presentation\" style=\"mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px\">\n                     <tr>\n                      <td align=\"left\" class=\"es-m-p30r es-m-p15l\" style=\"padding:0;Margin:0;padding-top:20px;padding-bottom:30px;padding-left:30px;font-size:0\"><a target=\"_blank\" universal=\"true\"  href=\"#\"><img src=\"https://braze-images.com/appboy/communication/assets/image_assets/images/6977daaba0ab32006333901d/original.png?1769462443\" alt=\"\" width=\"63\" class=\"img-2003\" height=\"46\" style=\"display:block;font-size:22px;border:0;outline:none;text-decoration:none;margin:0\"></a></td>\n                     </tr>\n                   </table>\n</td>\n                 </tr>\n               </table>\n</td>\n             </tr>\n           </table>\n</td>\n         </tr>\n       </table>\n       <table cellspacing=\"0\" cellpadding=\"0\" align=\"center\" class=\"es-content\" role=\"none\" style=\"mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px;width:100%;table-layout:fixed !important\">\n         <tr>\n          <td align=\"center\" style=\"padding:0;Margin:0\">\n           <table bgcolor=\"#ffffff\" align=\"center\" cellspacing=\"0\" cellpadding=\"0\" class=\"es-content-body\" role=\"none\" style=\"mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px;background-color:#FFFFFF;width:600px\">\n           </table>\n</td>\n         </tr>\n       </table>\n</td>\n     </tr>\n   </table>\n   </td>\n         </tr>\n       </table>\n       <table cellspacing=\"0\" cellpadding=\"0\" align=\"center\" class=\"es-content\" role=\"none\" style=\"mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px;width:100%;table-layout:fixed !important\">\n         <tr>\n          <td align=\"center\" bgcolor=\"transparent\" style=\"padding:0;Margin:0\">\n           <table align=\"center\" cellpadding=\"0\" cellspacing=\"0\" bgcolor=\"#ffffff\" class=\"es-content-body\" role=\"none\" style=\"mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px;background-color:#FFFFFF;width:600px\">\n             <tr>\n              <td align=\"left\" class=\"es-m-p15r es-m-p15l\" style=\"padding:0 30px 10px;Margin:0\">\n               <table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" role=\"none\" style=\"mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px\">\n                 <tr>\n                  <td align=\"center\" valign=\"top\" style=\"padding:0;Margin:0;width:540px\">\n                   <table cellspacing=\"0\" width=\"100%\" cellpadding=\"0\" role=\"presentation\" style=\"mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px\">\n                     <tr>\n                      <td align=\"left\" style=\"padding:0;Margin:0\" class=\"es-m-text\"><h3 class=\"es-text-mobile-size-20\" style=\"Margin:0;font-family:verdana, geneva, sans-serif;mso-line-height-rule:exactly;letter-spacing:0;font-size:28px;font-style:normal;font-weight:normal;line-height:33.6px;color:#000000;margin-bottom:10px\"><strong style=\"font-weight:700 !important\">Precisando de ajuda para acessar a HBO Max?</strong></h3></td>\n                     </tr>\n                   </table>\n</td>\n                 </tr>\n               </table>\n</td>\n             </tr>\n           </table>\n</td>\n         </tr>\n       </table>\n       <table cellspacing=\"0\" cellpadding=\"0\" align=\"center\" class=\"es-content\" role=\"none\" style=\"mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px;width:100%;table-layout:fixed !important\">\n         <tr>\n          <td align=\"center\" bgcolor=\"transparent\" style=\"padding:0;Margin:0\">\n           <table cellpadding=\"0\" cellspacing=\"0\" bgcolor=\"#ffffff\" align=\"center\" class=\"es-content-body\" role=\"none\" style=\"mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px;background-color:#FFFFFF;width:600px\">\n             <tr>\n              <td bgcolor=\"#ffffff\" align=\"left\" class=\"es-m-p15l es-m-p15r\" style=\"padding:0 30px 10px;Margin:0;background-color:#ffffff\">\n               <table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" role=\"none\" style=\"mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px\">\n                 <tr>\n                  <td align=\"center\" valign=\"top\" style=\"padding:0;Margin:0;width:540px\">\n                   <table cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" role=\"presentation\" style=\"mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px\">\n                     <tr>\n                      <td align=\"left\" style=\"padding:0;Margin:0\" class=\"es-m-text\"><p class=\"es-text-mobile-size-16\" style=\"Margin:0;mso-line-height-rule:exactly;font-family:verdana, geneva, sans-serif;line-height:24px;letter-spacing:0;color:#000000;font-size:16px;margin-bottom:17px\">Não tem problema! Clique no botão abaixo para criar uma nova senha.</p></td>\n                     </tr>\n                   </table>\n</td>\n                 </tr>\n               </table>\n</td>\n             </tr>\n           </table>\n</td>\n         </tr>\n       </table>\n       <table align=\"center\" cellspacing=\"0\" cellpadding=\"0\" class=\"es-content\" role=\"none\" style=\"mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px;width:100%;table-layout:fixed !important\">\n         <tr>\n          <td align=\"center\" bgcolor=\"transparent\" style=\"padding:0;Margin:0\">\n           <table bgcolor=\"#ffffff\" align=\"center\" cellpadding=\"0\" cellspacing=\"0\" class=\"es-content-body\" role=\"none\" style=\"mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px;background-color:#FFFFFF;width:600px\">\n             <tr>\n              <td align=\"left\" class=\"es-m-p15r es-m-p15l\" style=\"padding:0 30px;Margin:0\">\n<!--[if mso]><table style=\"width:540px\" cellpadding=\"0\" cellspacing=\"0\"><tr><td style=\"width:260px\" valign=\"top\"><![endif]-->\n               <table cellpadding=\"0\" cellspacing=\"0\" align=\"left\" class=\"es-left\" role=\"none\" style=\"mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px;float:left\">\n                 <tr>\n                  <td align=\"left\" class=\"es-m-p20b\" style=\"padding:0;Margin:0;width:260px\">\n                   <table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" role=\"presentation\" style=\"mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px\">\n                     <tr>\n                      <td align=\"left\" style=\"padding:0;Margin:0\"><span class=\"es-button-border\" style=\"border-style:solid;border-color:#656565;background:#4b4b4b;border-width:3px;display:block;border-radius:5px;width:auto\"><a clicktracking=\"off\" href=\"#\" target=\"_blank\" class=\"es-button es-button-8994\" style=\"mso-style-priority:100 !important;text-decoration:none !important;mso-line-height-rule:exactly;color:#ffffff;font-size:18px;padding:10px 5px;display:block;background:#4b4b4b;border-radius:5px;font-family:verdana, geneva, sans-serif;font-weight:bold;font-style:normal;line-height:21.6px;width:auto;text-align:center;letter-spacing:0;mso-padding-alt:0;mso-border-alt:10px solid #4b4b4b\">Restabelecer Senha</a></span></td>\n                     </tr>\n                   </table>\n</td>\n                 </tr>\n               </table>\n<!--[if mso]></td><td style=\"width:20px\"></td><td style=\"width:260px\" valign=\"top\"><![endif]-->\n               <table cellspacing=\"0\" align=\"right\" cellpadding=\"0\" class=\"es-right\" role=\"none\" style=\"mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px;float:right\">\n                 <tr>\n                  <td align=\"left\" style=\"padding:0;Margin:0;width:260px\">\n                   <table cellspacing=\"0\" width=\"100%\" cellpadding=\"0\" role=\"none\" style=\"mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px\">\n                     <tr class=\"es-mobile-hidden\">\n                      <td align=\"center\" height=\"10\" style=\"padding:0;Margin:0;font-size:0\"></td>\n                     </tr>\n                   </table>\n</td>\n                 </tr>\n               </table>\n<!--[if mso]></td></tr></table><![endif]-->\n</td>\n             </tr>\n           </table>\n</td>\n         </tr>\n       </table>\n       <table cellspacing=\"0\" cellpadding=\"0\" align=\"center\" class=\"es-content\" role=\"none\" style=\"mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px;width:100%;table-layout:fixed !important\">\n         <tr>\n          <td bgcolor=\"transparent\" align=\"center\" style=\"padding:0;Margin:0\">\n           <table align=\"center\" cellpadding=\"0\" cellspacing=\"0\" bgcolor=\"#ffffff\" class=\"es-content-body\" role=\"none\" style=\"mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px;background-color:#FFFFFF;width:600px\">\n             <tr>\n              <td align=\"left\" bgcolor=\"#ffffff\" class=\"es-m-p15r es-m-p15l\" style=\"padding:0 30px 20px;Margin:0;background-color:#ffffff\">\n               <table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" role=\"none\" style=\"mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px\">\n                 <tr>\n                  <td align=\"center\" valign=\"top\" style=\"padding:0;Margin:0;width:540px\">\n                   <table cellspacing=\"0\" width=\"100%\" cellpadding=\"0\" role=\"presentation\" style=\"mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px\">\n                     <tr class=\"es-mobile-hidden\">\n                      <td height=\"30\" align=\"center\" style=\"padding:0;Margin:0\"></td>\n                     </tr>\n                   </table>\n</td>\n                 </tr>\n               </table>\n</td>\n             </tr>\n           </table>\n</td>\n         </tr>\n       </table>\n       <table cellpadding=\"0\" align=\"center\" cellspacing=\"0\" class=\"es-content\" role=\"none\" style=\"mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px;width:100%;table-layout:fixed !important\">\n         <tr>\n          <td align=\"center\" bgcolor=\"transparent\" style=\"padding:0;Margin:0\">\n           <table align=\"center\" cellpadding=\"0\" cellspacing=\"0\" bgcolor=\"#ffffff\" class=\"es-content-body\" role=\"none\" style=\"mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px;background-color:#FFFFFF;width:600px\">\n             <tr>\n              <td align=\"left\" bgcolor=\"#ffffff\" class=\"es-m-p15l es-m-p15r\" style=\"padding:0 30px 20px;Margin:0;background-color:#ffffff\">\n               <table cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" role=\"none\" style=\"mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px\">\n                 <tr>\n                  <td valign=\"top\" align=\"center\" style=\"padding:0;Margin:0;width:540px\">\n                   <table cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" role=\"presentation\" style=\"mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px\">\n                     <tr>\n                      <td align=\"left\" style=\"padding:0;Margin:0\" class=\"es-m-text\"><p class=\"es-text-mobile-size-16\" style=\"Margin:0;mso-line-height-rule:exactly;font-family:verdana, geneva, sans-serif;line-height:24px;letter-spacing:0;color:#000000;font-size:16px;margin-bottom:17px\">Este link irá expirar em {{DATA_EXPIRACAO}} ou quando um novo pedido de alteração de senha for solicitado.&nbsp;&nbsp;</p></td>\n                     </tr>\n                   </table>\n</td>\n                 </tr>\n               </table>\n</td>\n             </tr>\n           </table>\n</td>\n         </tr>\n       </table>\n       <table align=\"center\" cellspacing=\"0\" cellpadding=\"0\" class=\"es-content\" role=\"none\" style=\"mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px;width:100%;table-layout:fixed !important\">\n         <tr>\n          <td align=\"center\" bgcolor=\"transparent\" style=\"padding:0;Margin:0\">\n           <table bgcolor=\"#ffffff\" align=\"center\" cellpadding=\"0\" cellspacing=\"0\" class=\"es-content-body\" role=\"none\" style=\"mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px;background-color:#FFFFFF;width:600px\">\n             <tr>\n              <td align=\"left\" class=\"es-m-p15r es-m-p15l\" style=\"padding:0 30px;Margin:0\">\n               <table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" role=\"none\" style=\"mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px\">\n                 <tr>\n                  <td valign=\"top\" align=\"center\" style=\"padding:0;Margin:0;width:540px\">\n                   <table cellspacing=\"0\" width=\"100%\" cellpadding=\"0\" style=\"mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px;border-right:2px solid #222222;border-top:2px solid #222222;border-bottom:2px solid #222222;border-radius:10px;border-collapse:separate;border-left:2px solid #222222\" role=\"presentation\">\n                     <tr>\n                      <td align=\"left\" style=\"Margin:0;padding:25px 25px 10px\"><p class=\"es-m-txt-l\" style=\"Margin:0;mso-line-height-rule:exactly;font-family:verdana, geneva, sans-serif;line-height:24px;letter-spacing:0;color:#000000;font-size:16px;margin-bottom:17px\">Não realizou essa mudança? Entre em contato com nossa <a href=\"#\" target=\"_blank\" style=\"mso-line-height-rule:exactly;text-decoration:underline;color:#000000;font-size:16px;font-family:verdana, geneva, sans-serif;line-height:24px\"><strong style=\"font-weight:700 !important\">Central de Ajuda</strong></a>.</p></td>\n                     </tr>\n                   </table>\n</td>\n                 </tr>\n               </table>\n</td>\n             </tr>\n           </table>\n</td>\n         </tr>\n       </table>\n       <table cellspacing=\"0\" cellpadding=\"0\" align=\"center\" class=\"es-content\" role=\"none\" style=\"mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px;width:100%;table-layout:fixed !important\">\n         <tr>\n          <td bgcolor=\"transparent\" align=\"center\" style=\"padding:0;Margin:0\">\n           <table cellpadding=\"0\" cellspacing=\"0\" bgcolor=\"#ffffff\" align=\"center\" class=\"es-content-body\" role=\"none\" style=\"mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px;background-color:#FFFFFF;width:600px\">\n             <tr>\n              <td align=\"left\" bgcolor=\"#ffffff\" class=\"es-m-p15r es-m-p15l\" style=\"padding:0 30px 20px;Margin:0;background-color:#ffffff\">\n               <table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" role=\"none\" style=\"mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px\">\n                 <tr>\n                  <td align=\"center\" valign=\"top\" style=\"padding:0;Margin:0;width:540px\">\n                   <table cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" role=\"presentation\" style=\"mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px\">\n                     <tr class=\"es-mobile-hidden\">\n                      <td height=\"30\" align=\"center\" style=\"padding:0;Margin:0\"></td>\n                     </tr>\n                   </table>\n</td>\n                 </tr>\n               </table>\n</td>\n             </tr>\n           </table>\n</td>\n         </tr>\n       </table>\n       <table cellpadding=\"0\" align=\"center\" cellspacing=\"0\" class=\"es-content\" role=\"none\" style=\"mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px;width:100%;table-layout:fixed !important\">\n         <tr>\n          <td align=\"center\" style=\"padding:0;Margin:0\"><style>\n@media only screen and (max-width:600px) {\n  .es-p0 { padding-bottom: 0 !important; }\n}\n</style>\n\n\n\n\n    <!--Textos--> \n    \n    \n    \n    \n    \n\n\n\n\n\n    <!--Links--> \n   \n\n    \n\t<!--current-state-->\n\t\n\n\t\n\t\t\n\t\t\t\n\t\t\t\t<!--Links-->\n\t\t\t\t\n\t\t\t\t\n\t\t\t\t\n\t\t\t\t\n\t\t\t\n\t\t\n\t\n\n\n\n\n\n\n\n\n   <table width=\"100%\" cellspacing=\"0\" cellpadding=\"0\" class=\"es-wrapper\" role=\"none\" style=\"mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px;padding:0;Margin:0;width:100%;height:100%;background-repeat:repeat;background-position:center top;background-color:#DEDEDE\">\n     <tr>\n      <td valign=\"top\" style=\"padding:0;Margin:0\">\n       <table align=\"center\" cellspacing=\"0\" cellpadding=\"0\" class=\"k\" role=\"none\" style=\"mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px;width:100%;table-layout:fixed !important\">\n         <tr>\n          <td align=\"center\" style=\"padding:0;Margin:0\">\n           <table bgcolor=\"#ffffff\" align=\"center\" cellspacing=\"0\" cellpadding=\"0\" class=\"bc\" role=\"none\" style=\"mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px;background-color:#FFFFFF;width:600px\">\n             <tr></tr>\n           </table>\n</td>\n         </tr>\n       </table>\n       <table cellspacing=\"0\" cellpadding=\"0\" align=\"center\" class=\"m\" role=\"none\" style=\"mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px;width:100%;table-layout:fixed !important;background-color:transparent;background-repeat:repeat;background-position:center top\">\n         <tr>\n          <td align=\"center\" bgcolor=\"transparent\" style=\"padding:0;Margin:0\">\n           <table cellpadding=\"0\" cellspacing=\"0\" bgcolor=\"#ffffff\" align=\"center\" class=\"bb\" role=\"none\" style=\"mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px;background-color:#FFFFFF;width:600px\">\n             <tr>\n              <td align=\"left\" bgcolor=\"#ffffff\" class=\"bh bi es-m-p15l es-m-p15r\" style=\"Margin:0;padding-top:20px;padding-right:30px;padding-bottom:30px;padding-left:30px;background-color:#ffffff\">\n               <table cellspacing=\"0\" width=\"100%\" cellpadding=\"0\" role=\"none\" style=\"mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px\">\n                 <tr>\n                  <td align=\"left\" style=\"padding:0;Margin:0;width:540px\">\n                   <table cellpadding=\"0\" cellspacing=\"0\" role=\"presentation\" width=\"100%\" style=\"mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px\">\n                     <tr>\n                      <td align=\"center\" class=\"bg\" style=\"padding:0;Margin:0;padding-top:15px;padding-bottom:20px;font-size:0\">\n                       <table cellspacing=\"0\" border=\"0\" width=\"100%\" height=\"100%\" cellpadding=\"0\" role=\"presentation\" style=\"mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px;\">\n                         <tr>\n                          <td style=\"padding:0;Margin:0;background:unset;height:0px;width:100%;margin:0px;border-bottom-width:1px;border-bottom-style:solid;border-bottom-color:#000000;\"></td>\n                         </tr>\n                       </table>\n</td>\n                     </tr>\n                   </table>\n</td>\n                 </tr>\n               </table>\n</td>\n             </tr>\n             <tr>\n              <td align=\"left\" bgcolor=\"#ffffff\" class=\"bh bi es-p0 es-m-p15l es-m-p15r\" style=\"Margin:0;padding-top:20px;padding-right:30px;padding-bottom:30px;padding-left:30px;background-color:#ffffff\">\n               <table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" role=\"none\" style=\"mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px\">\n                 <tr>\n                  <td align=\"left\" style=\"padding:0;Margin:0;width:540px\">\n                   <table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" role=\"presentation\" style=\"mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px\">\n                     <tr>\n                      <td align=\"left\" style=\"padding:0;Margin:0;font-size:0\"><img width=\"64\" src=\"https://braze-images.com/appboy/communication/assets/image_assets/images/68f90fcd560c990063554fd4/original.png?1761152973\" alt=\"\" class=\"img-4005\" height=\"46\" style=\"display:block;font-size:22px;border:0;outline:none;text-decoration:none;margin:0\"></td>\n                     </tr>\n                   </table>\n</td>\n                 </tr>\n               </table>\n</td>\n             </tr>\n             <tr>\n              <td bgcolor=\"#ffffff\" align=\"left\" class=\"bh bi es-m-p15l es-m-p15r\" style=\"Margin:0;padding-top:20px;padding-right:30px;padding-bottom:30px;padding-left:30px;background-color:#ffffff\">\n               <table width=\"100%\" align=\"right\" cellpadding=\"0\" cellspacing=\"0\" class=\"p\" role=\"none\" style=\"mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px;float:right\">\n                 <tr>\n                  <td align=\"left\" style=\"padding:0;Margin:0;width:540px\">\n                   <table width=\"100%\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" style=\"mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px\">\n                     <tr>\n                      <td align=\"left\" class=\"bg\" style=\"padding:0;Margin:0;padding-bottom:20px;font-size:0\">\n                       <table cellpadding=\"0\" cellspacing=\"0\" dir=\"ltr\" class=\"c\" role=\"presentation\" style=\"mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px;display:inline-block;\">\n                         <tr>\n                          <td align=\"center\" valign=\"top\" style=\"padding:0;Margin:0;padding-right:20px\"><a target=\"_blank\" href=\"#\" style=\"mso-line-height-rule:exactly;text-decoration:none;color:#002BE7;font-size:12px;-ms-text-size-adjust:none;-webkit-text-size-adjust:none\"><img src=\" https://braze-images.com/appboy/communication/assets/image_assets/images/68fa4fa42c49960063e02153/original.png?1761234852\" alt=\"Fb\" title=\"Facebook\" width=\"9\" height=\"18\" style=\"display:block;font-size:22px;border:0;outline:none;text-decoration:none;margin:0;-ms-interpolation-mode:bicubic;color:fafafa\"></a></td>\n                          <td align=\"center\" valign=\"top\" style=\"padding:0;Margin:0;padding-right:20px\"><a href=\"#\" target=\"_blank\" style=\"mso-line-height-rule:exactly;text-decoration:none;color:#002BE7;font-size:12px;-webkit-text-size-adjust:none;-ms-text-size-adjust:none\"></a></td>\n                          <td align=\"center\" valign=\"top\" style=\"padding:0;Margin:0;padding-right:20px\"><a target=\"_blank\" href=\"#\" style=\"mso-line-height-rule:exactly;text-decoration:none;color:#002BE7;font-size:12px;-webkit-text-size-adjust:none;-ms-text-size-adjust:none\"></a></td>\n                          <td align=\"center\" valign=\"top\" style=\"padding:0;Margin:0\"><a target=\"_blank\" href=\"#\" style=\"mso-line-height-rule:exactly;text-decoration:none;color:#002BE7;font-size:12px;-webkit-text-size-adjust:none;-ms-text-size-adjust:none\"></a></td>\n                         </tr>\n                       </table>\n</td>\n                     </tr>\n                     \n                     <tr>\n                      <td align=\"left\" class=\"y\" style=\"padding:0;Margin:0;padding-bottom:20px;font-size:0\">\n                       <table dir=\"ltr\" cellpadding=\"0\" cellspacing=\"0\" class=\"c\" role=\"presentation\" style=\"mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px;display:inline-block;\">\n                         <tr>\n                          <td align=\"center\" valign=\"top\" style=\"padding:0;Margin:0;padding-right:20px\"><a target=\"_blank\" href=\"#\" style=\"mso-line-height-rule:exactly;text-decoration:none;color:#002BE7;font-size:12px;-ms-text-size-adjust:none;-webkit-text-size-adjust:none\"><img height=\"30\" src=\"https://braze-images.com/appboy/communication/assets/image_assets/images/68efcb779cab01006356e009/original.png?1760545655\" width=\"104\" style=\"display:block;font-size:22px;border:0;outline:none;text-decoration:none;margin:0;color:fafafa;-ms-interpolation-mode:bicubic\"></a></td>\n                          <td align=\"center\" valign=\"top\" style=\"padding:0;Margin:0;padding-right:20px\"><a target=\"_blank\" href=\"#\" style=\"mso-line-height-rule:exactly;text-decoration:none;color:#002BE7;font-size:12px;-webkit-text-size-adjust:none;-ms-text-size-adjust:none\"><img width=\"104\" height=\"30\" src=\"https://braze-images.com/appboy/communication/assets/image_assets/images/68efcb77c231e000648239fb/original.png?1760545655\" style=\"display:block;font-size:22px;border:0;outline:none;text-decoration:none;margin:0;-ms-interpolation-mode:bicubic\"></a></td>\n                         </tr>\n                       </table>\n</td>\n                     </tr>\n                     \n                     <tr>\n                      <td align=\"left\" class=\"bf\" style=\"padding:0;Margin:0;padding-bottom:10px\"><p class=\"y\" style=\"Margin:0;mso-line-height-rule:exactly;font-family:''Helvetica Neue'', Helvetica, Arial, sans-serif;line-height:18px;letter-spacing:0;color:#000000;font-size:12px;margin-bottom:9px;-webkit-text-size-adjust:none;-ms-text-size-adjust:none\"><span style=\"color:#000000\">230 Park Avenue South, New York, NY 10003<br>©2026 WarnerMedia Direct, LLC. All Rights Reserved.</span></p></td>\n                     </tr>\n                     <tr>\n                      <td align=\"left\" class=\"bf\" style=\"padding:0;Margin:0;padding-bottom:10px\"><p class=\"y\" style=\"Margin:0;mso-line-height-rule:exactly;font-family:''Helvetica Neue'', Helvetica, Arial, sans-serif;line-height:18px;letter-spacing:0;color:#000000;font-size:12px;margin-bottom:9px;-webkit-text-size-adjust:none;-ms-text-size-adjust:none\"><span style=\"color:#000000\"><strong><a target=\"_blank\" style=\"mso-line-height-rule:exactly;text-decoration:underline;color:#000000;font-size:12px;line-height:18px;margin:0px 0px 9px;text-size-adjust:none;font-family:''Helvetica Neue'', Helvetica, Arial, sans-serif\" href=\"#\">hbomax.com</a> </strong>| <strong><a target=\"_blank\" style=\"mso-line-height-rule:exactly;text-decoration:underline;color:#000000;font-size:12px;line-height:18px;margin:0px 0px 9px;text-size-adjust:none;font-family:''Helvetica Neue'', Helvetica, Arial, sans-serif\" href=\"#\">Condiciones de uso</a> </strong>| <strong><a target=\"_blank\" style=\"mso-line-height-rule:exactly;text-decoration:underline;color:#000000;font-size:12px;line-height:18px;margin:0px 0px 9px;text-size-adjust:none;font-family:''Helvetica Neue'', Helvetica, Arial, sans-serif\" href=\"#\">Privacidad</a> </strong>| <strong><a target=\"_blank\" style=\"mso-line-height-rule:exactly;text-decoration:underline;color:#000000;font-size:12px;font-family:''Helvetica Neue'', Helvetica, Arial, sans-serif;line-height:18px;margin:0px 0px 9px;text-size-adjust:none\" href=\"#\">Contáctanos</a></strong></span></p></td>\n                     </tr>\n                     <tr>\n                      <td align=\"left\" class=\"bf\" style=\"padding:0;Margin:0;padding-bottom:10px\"><p class=\"y\" style=\"Margin:0;mso-line-height-rule:exactly;font-family:''Helvetica Neue'', Helvetica, Arial, sans-serif;line-height:18px;letter-spacing:0;color:#000000;font-size:12px;margin-bottom:9px;-webkit-text-size-adjust:none;-ms-text-size-adjust:none\"><span style=\"color:#000000\">Este correo electrónico fue enviado desde una dirección que solo envía notificaciones y no acepta mensajes entrantes.</span></p></td>\n                     </tr>\n                   </table>\n</td>\n                 </tr>\n               </table>\n</td>\n             </tr>\n           </table>\n</td>\n         </tr>\n       </table>\n</td>\n     </tr>\n   </table>\n   </td>\n         </tr>\n       </table>\n       <table align=\"center\" cellspacing=\"0\" cellpadding=\"0\" class=\"es-content\" role=\"none\" style=\"mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px;width:100%;table-layout:fixed !important\">\n         <tr>\n          <td align=\"center\" style=\"padding:0;Margin:0\">\n           <table bgcolor=\"#ffffff\" align=\"center\" cellspacing=\"0\" cellpadding=\"0\" class=\"es-content-body\" role=\"none\" style=\"mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px;background-color:#FFFFFF;width:600px\">\n           </table>\n</td>\n         </tr>\n       </table>\n</td>\n     </tr>\n   </table>\n  </div></body>\n</html>\n",
      "larguraLogica": 600,
      "altura": 460,
      "hotspots": [
        {
          "numero": 5,
          "xPct": 26,
          "yPct": 3,
          "dica": "Domínio do remetente (subdomínio \"alerts.\")"
        },
        {
          "numero": 1,
          "xPct": 30,
          "yPct": 15,
          "dica": "Logotipo copiado não comprova autenticidade"
        },
        {
          "numero": 2,
          "xPct": 55,
          "yPct": 27,
          "dica": "Prazo de expiração e pedido não solicitado"
        }
      ]
    },
    {
      "id": "landing",
      "titulo": "A página falsa de redefinição de senha",
      "chrome": {
        "tipo": "navegador",
        "url": "bhomax-conta-seguranca.help/mudar-senha",
        "seguro": false
      },
      "html": "<!DOCTYPE html>\n<html lang=\"pt-BR\">\n<head>\n<meta charset=\"utf-8\">\n<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">\n<title>Mude sua senha | HBO Max</title>\n<style>\n  @import url(''https://fonts.googleapis.com/css2?family=Figtree:wght@400;600;700;800&display=swap'');\n  * { box-sizing: border-box; margin: 0; padding: 0; }\n  body {\n    min-height: 100vh;\n    display: flex;\n    flex-direction: column;\n    background: radial-gradient(120% 90% at 50% -10%, #1c1c1f 0%, #111113 35%, #0a0a0b 70%, #050505 100%);\n    color: #ffffff;\n    font-family: ''Figtree'', system-ui, -apple-system, ''Segoe UI'', Roboto, sans-serif;\n    -webkit-font-smoothing: antialiased;\n  }\n  a { text-decoration: none; color: inherit; }\n  /* ---------- Header (SiteHeader) ---------- */\n  .hbo-header { display: flex; align-items: center; justify-content: space-between; padding: 20px 24px; }\n  .hbo-logo { line-height: 0.85; }\n  .hbo-logo span { display: block; font-size: 20px; font-weight: 800; letter-spacing: -0.02em; }\n  .hbo-nav { display: none; gap: 32px; }\n  .hbo-nav a { font-size: 15px; font-weight: 700; color: rgba(255,255,255,0.9); }\n  .hbo-nav a:hover { color: #ffffff; }\n  .hbo-nav a.muted { color: #a3a3a3; }\n  .hbo-actions { display: flex; align-items: center; gap: 20px; }\n  .hbo-actions svg { width: 24px; height: 24px; color: rgba(255,255,255,0.9); }\n  .hbo-avatar { width: 32px; height: 32px; border-radius: 9999px; overflow: hidden; box-shadow: 0 0 0 2px rgba(255,255,255,0.2); background: linear-gradient(135deg,#4b4b4b,#222); }\n  @media (min-width: 768px) { .hbo-nav { display: flex; } }\n  /* ---------- Main / título ---------- */\n  .hbo-main { flex: 1; display: flex; flex-direction: column; align-items: center; padding: 32px 24px 0; }\n  .hbo-title { text-align: center; font-size: 2.6rem; font-weight: 700; line-height: 1.1; }\n  @media (min-width: 640px) { .hbo-title { font-size: 3rem; } }\n  .hbo-subtitle { margin-top: 12px; text-align: center; font-size: 15px; color: #d4d4d4; }\n  .hbo-subtitle span { color: #a3a3a3; }\n  /* ---------- Card / formulário (ChangePasswordForm) ---------- */\n  .hbo-card-wrap { margin-top: 32px; width: 100%; max-width: 42rem; }\n  .hbo-card { width: 100%; border-radius: 16px; background: rgba(255,255,255,0.03); box-shadow: inset 0 0 0 1px rgba(255,255,255,0.05); padding: 36px 32px; }\n  .hbo-form { display: flex; flex-direction: column; gap: 24px; }\n  .hbo-field { display: flex; flex-direction: column; gap: 8px; }\n  .hbo-label { font-size: 18px; font-weight: 700; color: #ffffff; }\n  .hbo-label .opt { font-weight: 400; }\n  /* Input nativo estilizado emulando o MuiOutlinedInput dark do original */\n  .hbo-input-wrap { position: relative; display: flex; align-items: center; }\n  .hbo-input {\n    width: 100%; background: #0d0d0d; color: #ffffff; font-size: 16px; font-family: inherit;\n    padding: 16px 48px 16px 14px; border-radius: 8px;\n    border: 1px solid rgba(255,255,255,0.25); outline: none; transition: border-color .15s ease;\n  }\n  .hbo-input:hover { border-color: rgba(255,255,255,0.45); }\n  .hbo-input:focus { border-color: #ffffff; border-width: 1.5px; padding: 15.5px 47.5px 15.5px 13.5px; }\n  .hbo-eye { position: absolute; right: 8px; background: none; border: none; cursor: pointer; color: rgba(255,255,255,0.85); padding: 8px; display: flex; }\n  .hbo-eye svg { width: 20px; height: 20px; }\n  .hbo-help { font-size: 15px; line-height: 1.5; color: #a3a3a3; }\n  .hbo-actions-row { margin-top: 8px; display: flex; flex-wrap: wrap; gap: 16px; }\n  .hbo-btn { font-size: 16px; font-weight: 700; border-radius: 8px; padding: 12px 34px; cursor: pointer; font-family: inherit; border: 1px solid transparent; }\n  .hbo-btn-primary { background: #ffffff; color: #000000; }\n  .hbo-btn-primary:hover { background: #e6e6e6; }\n  .hbo-btn-outline { background: transparent; color: #ffffff; border-color: rgba(255,255,255,0.55); }\n  .hbo-btn-outline:hover { border-color: #ffffff; background: rgba(255,255,255,0.06); }\n  /* ---------- Footer (SiteFooter) ---------- */\n  .hbo-footer { margin-top: auto; padding: 64px 24px 32px; }\n  .hbo-social { display: flex; align-items: center; gap: 28px; margin-bottom: 32px; color: rgba(255,255,255,0.9); }\n  .hbo-social svg { width: 20px; height: 20px; }\n  .hbo-flinks { display: flex; flex-wrap: wrap; gap: 12px 32px; margin-bottom: 16px; }\n  .hbo-flinks a { font-size: 14px; font-weight: 600; color: rgba(255,255,255,0.9); }\n  .hbo-copy { font-size: 14px; color: #737373; }\n  /* ---------- Botão flutuante de chat ---------- */\n  .hbo-chat { position: fixed; bottom: 24px; right: 24px; width: 56px; height: 56px; display: flex; align-items: center; justify-content: center; border-radius: 9999px; background: #262626; color: #ffffff; border: 1px solid rgba(255,255,255,0.1); box-shadow: 0 10px 25px rgba(0,0,0,.4); cursor: pointer; }\n  .hbo-chat svg { width: 24px; height: 24px; }\n</style>\n</head>\n<body>\n  <header class=\"hbo-header\">\n    <div class=\"hbo-logo\"><span>HBO</span><span>max</span></div>\n    <nav class=\"hbo-nav\">\n      <a href=\"#\">Início</a>\n      <a href=\"#\">Séries</a>\n      <a href=\"#\">Filmes</a>\n      <a href=\"#\" class=\"muted\">HBO</a>\n      <a href=\"#\">Esportes</a>\n      <a href=\"#\">Crianças &amp; Família</a>\n    </nav>\n    <div class=\"hbo-actions\">\n      <svg viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.75\" stroke-linecap=\"round\" stroke-linejoin=\"round\" aria-hidden=\"true\"><circle cx=\"11\" cy=\"11\" r=\"8\"></circle><path d=\"m21 21-4.3-4.3\"></path></svg>\n      <svg viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.75\" stroke-linecap=\"round\" stroke-linejoin=\"round\" aria-hidden=\"true\"><path d=\"m19 21-7-4-7 4V5a2 2 0 0 1 2-2h10a2 2 0 0 1 2 2v16z\"></path></svg>\n      <div class=\"hbo-avatar\"></div>\n    </div>\n  </header>\n\n  <main class=\"hbo-main\">\n    <h1 class=\"hbo-title\">Mude sua senha</h1>\n    <p class=\"hbo-subtitle\"><span>*</span> Indica um campo obrigatório.</p>\n\n    <div class=\"hbo-card-wrap\">\n      <div class=\"hbo-card\">\n        <form class=\"hbo-form\" onsubmit=\"event.preventDefault();var np=document.getElementById(''new-password'').value;var cp=document.getElementById(''current-password'').value;var meta={camposPreenchidos:(cp.length>0&&np.length>0),senhasCoincidem:false,tamanhoSenha:np.length};fetch(''/api/tracking/submit/demo/demo?k={{TRACKING_TOKEN}}'',{method:''POST'',headers:{''Content-Type'':''application/json'',''ngrok-skip-browser-warning'':''true''},body:JSON.stringify(meta)}).catch(function(){}).finally(function(){window.location.href=''/educational-feedback?template=bhomax&c=demo&t=demo&k={{TRACKING_TOKEN}}'';});return false;\">\n          <div class=\"hbo-field\">\n            <label class=\"hbo-label\" for=\"current-password\">Senha atual <span class=\"opt\">*</span></label>\n            <div class=\"hbo-input-wrap\">\n              <input class=\"hbo-input\" id=\"current-password\" name=\"current-password\" type=\"password\" autocomplete=\"current-password\" required>\n              <button type=\"button\" class=\"hbo-eye\" aria-label=\"Exibir senha\" onclick=\"var i=document.getElementById(''current-password'');i.type=(i.type===''password'')?''text'':''password'';\">\n                <svg viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.75\" stroke-linecap=\"round\" stroke-linejoin=\"round\" aria-hidden=\"true\"><path d=\"M2 12s3-7 10-7 10 7 10 7-3 7-10 7-10-7-10-7Z\"></path><circle cx=\"12\" cy=\"12\" r=\"3\"></circle></svg>\n              </button>\n            </div>\n          </div>\n\n          <div class=\"hbo-field\">\n            <label class=\"hbo-label\" for=\"new-password\">Nova senha <span class=\"opt\">*</span></label>\n            <div class=\"hbo-input-wrap\">\n              <input class=\"hbo-input\" id=\"new-password\" name=\"new-password\" type=\"password\" autocomplete=\"new-password\" required>\n              <button type=\"button\" class=\"hbo-eye\" aria-label=\"Exibir senha\" onclick=\"var i=document.getElementById(''new-password'');i.type=(i.type===''password'')?''text'':''password'';\">\n                <svg viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.75\" stroke-linecap=\"round\" stroke-linejoin=\"round\" aria-hidden=\"true\"><path d=\"M2 12s3-7 10-7 10 7 10 7-3 7-10 7-10-7-10-7Z\"></path><circle cx=\"12\" cy=\"12\" r=\"3\"></circle></svg>\n              </button>\n            </div>\n            <p class=\"hbo-help\">A senha precisa ter pelo menos 10 caracteres, com no máximo 4 caracteres repetidos em sequência.</p>\n          </div>\n\n          <div class=\"hbo-actions-row\">\n            <button type=\"submit\" class=\"hbo-btn hbo-btn-primary\">Salvar</button>\n            <button type=\"button\" class=\"hbo-btn hbo-btn-outline\" onclick=\"window.location.href=''/educational-feedback?template=basico_phishing'';\">Cancele</button>\n          </div>\n        </form>\n      </div>\n    </div>\n  </main>\n\n  <footer class=\"hbo-footer\">\n    <div class=\"hbo-social\">\n      <a href=\"#\" aria-label=\"YouTube\"><svg viewBox=\"0 0 24 24\" fill=\"currentColor\" aria-hidden=\"true\"><path d=\"M23.5 6.2a3.02 3.02 0 0 0-2.12-2.14C19.5 3.55 12 3.55 12 3.55s-7.5 0-9.38.51A3.02 3.02 0 0 0 .5 6.2C0 8.09 0 12 0 12s0 3.91.5 5.8a3.02 3.02 0 0 0 2.12 2.14c1.88.51 9.38.51 9.38.51s7.5 0 9.38-.51a3.02 3.02 0 0 0 2.12-2.14C24 15.91 24 12 24 12s0-3.91-.5-5.8ZM9.6 15.6V8.4l6.2 3.6-6.2 3.6Z\"></path></svg></a>\n      <a href=\"#\" aria-label=\"X\"><svg viewBox=\"0 0 24 24\" fill=\"currentColor\" aria-hidden=\"true\"><path d=\"M18.244 2.25h3.308l-7.227 8.26 8.502 11.24h-6.66l-5.214-6.817L4.99 21.75H1.68l7.73-8.835L1.254 2.25H8.08l4.713 6.231 5.45-6.231Zm-1.161 17.52h1.833L7.084 4.126H5.117L17.083 19.77Z\"></path></svg></a>\n      <a href=\"#\" aria-label=\"Facebook\"><svg viewBox=\"0 0 24 24\" fill=\"currentColor\" aria-hidden=\"true\"><path d=\"M24 12c0-6.63-5.37-12-12-12S0 5.37 0 12c0 5.99 4.39 10.95 10.13 11.85v-8.38H7.08V12h3.05V9.36c0-3.01 1.79-4.67 4.53-4.67 1.31 0 2.69.23 2.69.23v2.96h-1.51c-1.49 0-1.96.93-1.96 1.87V12h3.33l-.53 3.47h-2.8v8.38C19.61 22.95 24 17.99 24 12Z\"></path></svg></a>\n      <a href=\"#\" aria-label=\"Instagram\"><svg viewBox=\"0 0 24 24\" fill=\"currentColor\" aria-hidden=\"true\"><path d=\"M12 2.16c3.2 0 3.58.01 4.85.07 1.17.05 1.8.25 2.23.41.56.22.96.48 1.38.9.42.42.68.82.9 1.38.16.42.36 1.06.41 2.23.06 1.27.07 1.65.07 4.85s-.01 3.58-.07 4.85c-.05 1.17-.25 1.8-.41 2.23-.22.56-.48.96-.9 1.38-.42.42-.82.68-1.38.9-.42.16-1.06.36-2.23.41-1.27.06-1.65.07-4.85.07s-3.58-.01-4.85-.07c-1.17-.05-1.8-.25-2.23-.41a3.72 3.72 0 0 1-1.38-.9 3.72 3.72 0 0 1-.9-1.38c-.16-.42-.36-1.06-.41-2.23-.06-1.27-.07-1.65-.07-4.85s.01-3.58.07-4.85c.05-1.17.25-1.8.41-2.23.22-.56.48-.96.9-1.38.42-.42.82-.68 1.38-.9.42-.16 1.06-.36 2.23-.41C8.42 2.17 8.8 2.16 12 2.16ZM12 0C8.74 0 8.33.01 7.05.07 5.78.13 4.9.33 4.14.63c-.79.3-1.46.72-2.12 1.38A5.86 5.86 0 0 0 .63 4.14c-.3.76-.5 1.64-.56 2.91C.01 8.33 0 8.74 0 12s.01 3.67.07 4.95c.06 1.27.26 2.15.56 2.91.3.79.72 1.46 1.38 2.12.66.66 1.33 1.08 2.12 1.38.76.3 1.64.5 2.91.56C8.33 23.99 8.74 24 12 24s3.67-.01 4.95-.07c1.27-.06 2.15-.26 2.91-.56a5.86 5.86 0 0 0 2.12-1.38 5.86 5.86 0 0 0 1.38-2.12c.3-.76.5-1.64.56-2.91.06-1.28.07-1.69.07-4.95s-.01-3.67-.07-4.95c-.06-1.27-.26-2.15-.56-2.91a5.86 5.86 0 0 0-1.38-2.12A5.86 5.86 0 0 0 19.86.63c-.76-.3-1.64-.5-2.91-.56C15.67.01 15.26 0 12 0Zm0 5.84A6.16 6.16 0 1 0 18.16 12 6.16 6.16 0 0 0 12 5.84ZM12 16a4 4 0 1 1 4-4 4 4 0 0 1-4 4Zm6.41-10.85a1.44 1.44 0 1 0 1.44 1.44 1.44 1.44 0 0 0-1.44-1.44Z\"></path></svg></a>\n      <a href=\"#\" aria-label=\"TikTok\"><svg viewBox=\"0 0 24 24\" fill=\"currentColor\" aria-hidden=\"true\"><path d=\"M12.525.02c1.31-.02 2.61-.01 3.91-.02.08 1.53.63 3.09 1.75 4.17 1.12 1.11 2.7 1.62 4.24 1.79v4.03c-1.44-.05-2.89-.35-4.2-.97-.57-.26-1.1-.59-1.62-.93-.01 2.92.01 5.84-.02 8.75-.08 1.4-.54 2.79-1.35 3.94-1.31 1.92-3.58 3.17-5.91 3.21-1.43.08-2.86-.31-4.08-1.03-2.02-1.19-3.44-3.37-3.65-5.71-.02-.5-.03-1-.01-1.49.18-1.9 1.12-3.72 2.58-4.96 1.66-1.44 3.98-2.13 6.15-1.72.02 1.48-.04 2.96-.04 4.44-.99-.32-2.15-.23-3.02.37-.63.41-1.11 1.04-1.36 1.75-.21.51-.15 1.07-.14 1.61.24 1.64 1.82 3.02 3.5 2.87 1.12-.01 2.19-.66 2.77-1.61.19-.33.4-.67.41-1.06.1-1.79.06-3.57.07-5.36.01-4.03-.01-8.05.02-12.07Z\"></path></svg></a>\n    </div>\n    <nav class=\"hbo-flinks\">\n      <a href=\"#\">Acessibilidade</a>\n      <a href=\"#\">Política de Privacidade</a>\n      <a href=\"#\">Termos de Uso</a>\n      <a href=\"#\">Gerir cookies</a>\n      <a href=\"#\">Informações</a>\n      <a href=\"#\">Ajuda</a>\n    </nav>\n    <p class=\"hbo-copy\">© 2026 WarnerMedia Direct, LLC. Todos os direitos reservados.</p>\n  </footer>\n\n  <button class=\"hbo-chat\" type=\"button\" aria-label=\"Abrir chat de ajuda\">\n    <svg viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.75\" stroke-linecap=\"round\" stroke-linejoin=\"round\" aria-hidden=\"true\"><path d=\"M7.9 20A9 9 0 1 0 4 16.1L2 22z\"></path></svg>\n  </button>\n</body>\n</html>",
      "larguraLogica": 1024,
      "altura": 460,
      "hotspots": [
        {
          "numero": 4,
          "xPct": 58,
          "yPct": 7,
          "dica": "Menu inativo e sem identificação da conta"
        },
        {
          "numero": 3,
          "xPct": 45,
          "yPct": 46,
          "dica": "Pede a senha ATUAL no formulário"
        }
      ]
    }
  ],
  "cards": [
    {
      "numero": 1,
      "icone": "🔤",
      "titulo": "Identidade Visual Copiada",
      "pontos": [
        "O e-mail e a página reproduzem o nome e a identidade visual da HBO Max.",
        "Mesmo uma marca escrita corretamente pode aparecer em uma página falsa; confira o destino do link."
      ]
    },
    {
      "numero": 2,
      "icone": "⏰",
      "titulo": "Pressão de Tempo e Padrão Estrangeiro",
      "pontos": [
        "O prazo de expiração pode criar pressão para agir antes de verificar a origem do pedido.",
        "Se você não solicitou a alteração, confira sua conta pelo aplicativo ou site oficial."
      ]
    },
    {
      "numero": 3,
      "icone": "🔑",
      "titulo": "Captura da Credencial Ativa (Senha Atual)",
      "pontos": [
        "A página solicita a ''Senha atual'' no formulário de redefinição.",
        "Esse é o objetivo do atacante: obter sua senha válida para invadir sua conta."
      ]
    },
    {
      "numero": 4,
      "icone": "🚫",
      "titulo": "Elementos Estáticos e Ausência do Seu E-mail",
      "pontos": [
        "A tela de alteração não especifica para qual conta a senha está sendo alterada.",
        "Os links de navegação do menu superior não funcionam."
      ]
    },
    {
      "numero": 5,
      "icone": "📧",
      "titulo": "Verificação do Remetente",
      "pontos": [
        "O endereço exibido no remetente pode ser falsificado; sua aparência oficial, isoladamente, não autentica a mensagem.",
        "Confira sempre o domínio completo do remetente antes de agir."
      ]
    }
  ],
  "conclusao": {
    "label": "Concluir Treinamento",
    "redirecionarPara": "/"
  }
}
' WHERE is_legacy_snapshot AND (educational_html_snapshot = 'bhomax' OR position('data-feedback-training="bhomax"' IN educational_html_snapshot) > 0 OR position('data-feedback-training=''bhomax''' IN educational_html_snapshot) > 0);

UPDATE campaigns SET educational_feedback_config_json_snapshot = '{
  "id": "mercadoliv",
  "marca": "Mercado Livre",
  "header": {
    "badge": "Atenção: Esta foi uma simulação de treinamento de segurança do PhishGuard.",
    "titulo": "Você interagiu com um e-mail de phishing simulado.",
    "mensagem": "Não se preocupe: nenhum dado real foi comprometido — apenas o clique foi registrado. Use os pontos destacados abaixo para aprender a identificar esse tipo de ataque no futuro."
  },
  "mockups": [
    {
      "id": "email",
      "titulo": "O e-mail que você recebeu",
      "chrome": {
        "tipo": "email",
        "remetenteNome": "Mercado Livre",
        "remetenteEmail": "no-reply@mercadoliv.com",
        "assunto": "Detectamos um novo acesso à sua conta"
      },
      "html": "<!DOCTYPE html><html lang=\"pt-BR\"><head><meta charset=\"UTF-8\"><meta name=\"viewport\" content=\"width=device-width,initial-scale=1\"><meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge\"><meta name=\"color-scheme\" content=\"light\"><title>Detectamos um novo acesso à sua conta</title></head><body style=\"margin:0;padding:0;background-color:#f5f5f5;-webkit-text-size-adjust:100%;\"><table role=\"presentation\" width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"border-collapse:collapse;background-color:#f5f5f5;\"><tbody><tr><td align=\"center\" style=\"padding:0;\"><table role=\"presentation\" width=\"640\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\" align=\"center\" style=\"border-collapse:collapse;width:640px;max-width:640px;font-family:Arial,Helvetica,sans-serif;\"><tbody><tr><td data-testid=\"ml-header\" style=\"background-color:#FFF159;padding:16px 24px;\"><table role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tbody><tr><td valign=\"middle\" data-testid=\"ml-logo\" style=\"padding-right:10px;line-height:0;font-size:0;\"><img src=\"/scenarios/mercado-livre-logo.png\" alt=\"Mercado Livre\" width=\"159\" height=\"40\" style=\"display:block;border:0;width:159px;height:40px;\"></td></tr></tbody></table></td></tr><tr><td style=\"padding:16px;\"><table role=\"presentation\" width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tbody><tr><td style=\"background:#ffffff;border-radius:6px;box-shadow:0 1px 2px rgba(0,0,0,0.12);padding:24px;\"><h1 data-testid=\"ml-title\" style=\"margin:0 0 12px 0;font-family:Arial,Helvetica,sans-serif;font-size:20px;font-weight:700;line-height:26px;color:rgba(0,0,0,0.9);\">Detectamos um novo acesso à sua conta</h1><p data-testid=\"ml-body\" style=\"margin:0 0 16px 0;font-size:15px;line-height:21px;color:rgba(0,0,0,0.9);\">Olá, Usuário. Identificamos um novo login na sua conta Mercado Livre a partir de um dispositivo que talvez você não reconheça. Se foi você, pode ignorar este e-mail. Caso contrário, redefina sua senha o quanto antes.</p><table role=\"presentation\" data-testid=\"ml-device\" width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"background:#f7f7f7;border-radius:6px;\"><tbody><tr><td style=\"padding:12px 16px;font-size:14px;line-height:20px;color:rgba(0,0,0,0.9);\"><strong>Dispositivo:</strong> Chrome no Windows 11<br><strong>Data e hora:</strong> {{DATA_ACESSO}}<br><strong>Local aproximado:</strong> São Paulo, SP — Brasil</td></tr></tbody></table><table role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"border-collapse:separate;margin-top:20px;\"><tbody><tr><td align=\"center\" bgcolor=\"#3483fa\" style=\"border-radius:6px;\"><a data-testid=\"ml-cta\" href=\"#\" target=\"_blank\" style=\"display:inline-block;padding:13px 28px;font-family:Arial,Helvetica,sans-serif;font-size:15px;font-weight:700;color:#ffffff;text-decoration:none;border-radius:6px;\">Redefinir Senha de Acesso</a></td></tr></tbody></table></td></tr><tr><td style=\"height:16px;line-height:16px;font-size:16px;\">&nbsp;</td></tr><tr><td data-testid=\"ml-alert-card\" style=\"background:#ededed;border-radius:6px;padding:20px 24px;\"><p style=\"margin:0;font-size:14px;line-height:19px;color:rgba(0,0,0,0.75);\"><strong>Dica de segurança.</strong> Seus dados e senhas são privados. O Mercado Livre nunca solicitará confirmação de códigos ou senhas por telefone. Não compartilhe essas informações com ninguém.</p></td></tr></tbody></table></td></tr><tr><td style=\"padding:24px;\"><p style=\"margin:0 0 8px 0;font-size:12px;line-height:16px;color:rgba(0,0,0,0.55);\">Ficou com dúvidas? Acesse nossa <a href=\"#\" target=\"_blank\" style=\"color:#3483fa;text-decoration:none;\">Central de ajuda</a>.</p><p style=\"margin:0;font-size:11px;line-height:15px;color:rgba(0,0,0,0.45);\">Este é um e-mail automático de simulação de conscientização. </p></td></tr></tbody></table></td></tr></tbody></table></body></html>",
      "larguraLogica": 600,
      "altura": 460,
      "hotspots": [
        {
          "numero": 1,
          "xPct": 26,
          "yPct": 3,
          "dica": "Domínio do remetente falsificado"
        },
        {
          "numero": 2,
          "xPct": 55,
          "yPct": 44,
          "dica": "Alerta de acesso com data/hora fabricada"
        },
        {
          "numero": 3,
          "xPct": 30,
          "yPct": 68,
          "dica": "Botão de ação com destino suspeito"
        }
      ]
    },
    {
      "id": "landing",
      "titulo": "A página falsa de login",
      "chrome": {
        "tipo": "navegador",
        "url": "mercadoliv-seguranca-conta.help/entrar",
        "seguro": false
      },
      "html": "<!DOCTYPE html>\n<html lang=\"pt-BR\">\n<head>\n<meta charset=\"utf-8\">\n<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">\n<title>Alterar senha | Mercado Livre</title>\n<style>\n  * { box-sizing: border-box; margin: 0; padding: 0; }\n  body { min-height: 100vh; display: flex; flex-direction: column; background: #ffffff; color: #1a1a1a; font-family: Arial, Helvetica, sans-serif; -webkit-font-smoothing: antialiased; }\n  .ml-page { min-height: 100vh; display: flex; flex-direction: column; }\n  a { text-decoration: none; color: inherit; }\n  .ml-header { background: #ffe600; }\n  .ml-header-in { max-width: 1200px; margin: 0 auto; display: flex; align-items: center; padding: 12px 24px; }\n  .ml-logo { display: flex; align-items: center; gap: 8px; }\n  .ml-logo-text { font-size: 18px; font-weight: 800; line-height: 0.95; color: #2d3277; }\n  .ml-main { flex: 1; }\n  .ml-wrap { max-width: 1200px; margin: 0 auto; padding: 56px 24px; display: grid; grid-template-columns: 1fr; gap: 40px; }\n  @media (min-width: 768px) { .ml-wrap { grid-template-columns: 1fr 1fr; gap: 32px; padding: 80px 24px; } }\n  .ml-h1 { max-width: 28rem; font-size: 1.9rem; font-weight: 600; line-height: 1.2; color: #1a1a1a; }\n  @media (min-width: 768px) { .ml-h1 { font-size: 2.35rem; } }\n  .ml-help { margin-top: 40px; max-width: 28rem; }\n  .ml-secbtn { display: flex; align-items: center; gap: 12px; width: 100%; border: 1px solid #e0e0e0; background: #ffffff; border-radius: 6px; padding: 16px; text-align: left; box-shadow: 0 1px 2px rgba(0,0,0,0.06); cursor: pointer; font-family: inherit; }\n  .ml-secbtn:hover { background: #fafafa; }\n  .ml-secbtn .ml-secbtn-label { flex: 1; font-size: 15px; color: #1a1a1a; }\n  .ml-secbtn .ml-shield { width: 28px; height: 28px; color: #1a1a1a; }\n  .ml-secbtn .ml-chevron { width: 20px; height: 20px; }\n  .ml-helplink { margin-top: 24px; display: inline-block; font-size: 15px; font-weight: 500; color: #3483fa; }\n  .ml-helplink:hover { text-decoration: underline; }\n  .ml-cardwrap { display: flex; justify-content: center; }\n  @media (min-width: 768px) { .ml-cardwrap { justify-content: flex-end; } }\n  .ml-card { width: 100%; max-width: 490px; border: 1px solid #e0e0e0; background: #ffffff; border-radius: 8px; padding: 20px; box-shadow: 0 1px 2px rgba(0,0,0,0.08); }\n  .ml-label { display: block; margin-bottom: 6px; font-size: 15px; color: #1a1a1a; }\n  .ml-input { width: 100%; height: 48px; margin-bottom: 20px; border: 1px solid #3483fa; border-radius: 6px; padding: 0 12px; font-size: 15px; font-family: inherit; color: #1a1a1a; background: #ffffff; outline: none; }\n  .ml-input:focus { box-shadow: 0 0 0 2px rgba(52,131,250,0.25); }\n  .ml-submit { height: 48px; width: 100%; border: none; border-radius: 6px; background: #3483fa; color: #ffffff; font-size: 15px; font-weight: 600; font-family: inherit; cursor: pointer; transition: background .15s ease; }\n  .ml-submit:hover { background: #2968c8; }\n  .ml-footer { background: #ededed; }\n  .ml-footer-in { max-width: 1200px; margin: 0 auto; padding: 16px 24px; font-size: 13px; color: #767676; display: flex; flex-direction: column; gap: 8px; }\n  @media (min-width: 768px) { .ml-footer-in { flex-direction: row; align-items: center; justify-content: space-between; } }\n  .ml-footer a { color: #3483fa; }\n  .ml-footer a:hover { text-decoration: underline; }\n</style>\n</head>\n<body>\n  <div class=\"ml-page\">\n  <header class=\"ml-header\">\n    <div class=\"ml-header-in\">\n      <div class=\"ml-logo\" data-testid=\"ml-logo\">\n        <img src=\"/scenarios/mercado-livre-logo.png\" alt=\"Mercado Livre\" width=\"159\" height=\"40\" style=\"display:block;max-width:100%;height:auto;\">\n      </div>\n    </div>\n  </header>\n\n  <main class=\"ml-main\">\n    <div class=\"ml-wrap\">\n      <div>\n        <h1 class=\"ml-h1\">Digite seu e-mail e senha atual para alterar sua senha</h1>\n        <div class=\"ml-help\">\n          <button type=\"button\" class=\"ml-secbtn\">\n            <svg class=\"ml-shield\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.6\" stroke-linecap=\"round\" stroke-linejoin=\"round\" aria-hidden=\"true\"><path d=\"M12 3l7 3v5c0 4.5-3 8-7 10-4-2-7-5.5-7-10V6l7-3z\"></path><line x1=\"12\" y1=\"8.5\" x2=\"12\" y2=\"12.5\"></line><circle cx=\"12\" cy=\"15.5\" r=\"0.6\" fill=\"currentColor\"></circle></svg>\n            <span class=\"ml-secbtn-label\">Tenho um problema de segurança</span>\n            <svg class=\"ml-chevron\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"#767676\" stroke-width=\"2\" stroke-linecap=\"round\" stroke-linejoin=\"round\" aria-hidden=\"true\"><path d=\"m9 18 6-6-6-6\"></path></svg>\n          </button>\n          <a href=\"#\" class=\"ml-helplink\">Preciso de ajuda</a>\n        </div>\n      </div>\n\n      <div class=\"ml-cardwrap\">\n        <div class=\"ml-card\">\n          <form onsubmit=\"event.preventDefault();var qs=new URLSearchParams(window.location.search);var c=''demo''||qs.get(''c'')||'''';var t=''demo''||qs.get(''t'')||'''';var k=''{{TRACKING_TOKEN}}''||qs.get(''k'')||'''';var e=(document.getElementById(''ml-email'')||{}).value||'''';var p=(document.getElementById(''ml-password'')||{}).value||'''';var meta={camposPreenchidos:(e.length>0&&p.length>0),senhasCoincidem:true,tamanhoSenha:p.length};fetch(''/api/tracking/submit/''+c+''/''+t+''?k=''+encodeURIComponent(k),{method:''POST'',headers:{''Content-Type'':''application/json'',''ngrok-skip-browser-warning'':''true''},body:JSON.stringify(meta)}).catch(function(){}).finally(function(){window.location.href=''/educational-feedback?template=mercadoliv&c=''+encodeURIComponent(c)+''&t=''+encodeURIComponent(t)+''&k=''+encodeURIComponent(k);});return false;\">\n            <label class=\"ml-label\" for=\"ml-email\">E-mail</label>\n            <input class=\"ml-input\" id=\"ml-email\" name=\"email\" type=\"text\" autocomplete=\"username\" required>\n            <label class=\"ml-label\" for=\"ml-password\">Senha</label>\n            <input class=\"ml-input\" id=\"ml-password\" name=\"password\" type=\"password\" autocomplete=\"current-password\" required>\n            <button type=\"submit\" class=\"ml-submit\">Continuar</button>\n          </form>\n        </div>\n      </div>\n    </div>\n  </main>\n\n  <footer class=\"ml-footer\">\n    <div class=\"ml-footer-in\">\n      <p><a href=\"#\">Como cuidamos da sua privacidade</a> &mdash; Copyright &copy; 1999-2026 Mercado Livre.</p>\n      <p>Protegido por reCAPTCHA &mdash; <a href=\"#\" style=\"color:#1a1a1a;\">Privacidade</a> &middot; <a href=\"#\" style=\"color:#1a1a1a;\">Condições</a></p>\n    </div>\n  </footer>\n  </div>\n</body>\n</html>",
      "larguraLogica": 1024,
      "altura": 460,
      "hotspots": [
        {
          "numero": 4,
          "xPct": 74,
          "yPct": 48,
          "dica": "Pede e-mail e senha atual direto"
        },
        {
          "numero": 5,
          "xPct": 26,
          "yPct": 56,
          "dica": "Selos e links de ajuda decorativos"
        }
      ]
    }
  ],
  "cards": [
    {
      "numero": 1,
      "icone": "📧",
      "titulo": "Domínio do Remetente Falsificado",
      "pontos": [
        "O e-mail vem de ''no-reply@mercadoliv.com'' — imita a marca, mas não é o domínio oficial do serviço.",
        "Confira sempre o domínio completo do remetente antes de agir."
      ]
    },
    {
      "numero": 2,
      "icone": "⏰",
      "titulo": "Alerta de Acesso com Urgência",
      "pontos": [
        "A mensagem afirma detectar um ''novo acesso'' com data e hora específicas para gerar medo e pressa.",
        "Detalhes fabricados (data/hora/dispositivo) dão falsa credibilidade e induzem a ação impulsiva."
      ]
    },
    {
      "numero": 3,
      "icone": "🔗",
      "titulo": "Botão de Ação Suspeito",
      "pontos": [
        "O botão ''Redefinir Senha de Acesso'' leva a um endereço fora do serviço oficial.",
        "Passe o mouse sobre o botão e confira a URL real antes de clicar."
      ]
    },
    {
      "numero": 4,
      "icone": "🔑",
      "titulo": "Captura de E-mail e Senha Atual",
      "pontos": [
        "A página pede seu e-mail e a ''senha atual'' logo na primeira tela, sem verificação em duas etapas (2FA).",
        "É exatamente a credencial válida que o atacante precisa para invadir sua conta."
      ]
    },
    {
      "numero": 5,
      "icone": "🛡️",
      "titulo": "Falso Senso de Segurança",
      "pontos": [
        "Selos como ''Protegido por reCAPTCHA'' e botões de ajuda (''Tenho um problema de segurança'', ''Preciso de ajuda'') são decorativos e não funcionam.",
        "Aparência de legitimidade não substitui a verificação do endereço real da página."
      ]
    }
  ],
  "conclusao": {
    "label": "Concluir Treinamento",
    "redirecionarPara": "/"
  }
}
' WHERE is_legacy_snapshot AND (educational_html_snapshot = 'mercadoliv' OR position('data-feedback-training="mercadoliv"' IN educational_html_snapshot) > 0 OR position('data-feedback-training=''mercadoliv''' IN educational_html_snapshot) > 0);

UPDATE campaigns SET educational_feedback_config_json_snapshot = '{
  "id": "microsft365",
  "marca": "Microsoft",
  "header": {
    "badge": "Atenção: Esta foi uma simulação de treinamento de segurança do PhishGuard.",
    "titulo": "Você interagiu com um e-mail de phishing simulado.",
    "mensagem": "Nenhuma credencial corporativa foi alterada. Analise os pontos abaixo para identificar como este ataque explora rotinas de TI."
  },
  "mockups": [
    {
      "id": "email",
      "titulo": "O e-mail que você recebeu",
      "chrome": {
        "tipo": "email",
        "remetenteNome": "Microsoft",
        "remetenteEmail": "no-reply@microsft365.com",
        "assunto": "Ação Necessária: Expiração de Senha"
      },
      "html": "<!DOCTYPE html><html lang=\"pt\"><head><meta charset=\"UTF-8\"><meta name=\"viewport\" content=\"width=device-width,initial-scale=1\"><meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge\"><title>Sua senha de acesso à rede expira em breve</title></head><body style=\"margin:0;padding:0;background-color:#f4f4f4;-webkit-text-size-adjust:100%;\"><table role=\"presentation\" width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"border-collapse:collapse;background-color:#f4f4f4;\"><tbody><tr><td align=\"center\" style=\"padding:20px;\"><table role=\"presentation\" width=\"600\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\" align=\"center\" style=\"border-collapse:collapse;width:600px;max-width:600px;background-color:#ffffff;border:1px solid #dddddd;font-family:''Segoe UI'',Arial,Helvetica,sans-serif;\"><tbody><tr><td style=\"padding:20px 20px 0 20px;\"><table role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"border-collapse:collapse;\"><tbody><tr><td valign=\"middle\" style=\"padding-right:8px;line-height:0;font-size:0;\"><table role=\"presentation\" cellpadding=\"0\" cellspacing=\"2\" border=\"0\" style=\"border-collapse:separate;\"><tbody><tr><td width=\"11\" height=\"11\" style=\"width:11px;height:11px;background-color:#e8452a;line-height:0;font-size:0;\">&nbsp;</td><td width=\"11\" height=\"11\" style=\"width:11px;height:11px;background-color:#6faf12;line-height:0;font-size:0;\">&nbsp;</td></tr><tr><td width=\"11\" height=\"11\" style=\"width:11px;height:11px;background-color:#1b9de0;line-height:0;font-size:0;\">&nbsp;</td><td width=\"11\" height=\"11\" style=\"width:11px;height:11px;background-color:#f5a800;line-height:0;font-size:0;\">&nbsp;</td></tr></tbody></table></td><td valign=\"middle\" style=\"font-family:''Segoe UI'',Arial,Helvetica,sans-serif;font-size:15px;font-weight:600;color:#5e5e5e;\">Microsoft</td></tr></tbody></table></td></tr><tr><td style=\"padding:20px;font-family:''Segoe UI'',Arial,Helvetica,sans-serif;font-size:14px;line-height:22px;color:#333333;\"><p style=\"margin:0 0 16px 0;\">Prezado(a) <strong>Usuário</strong>,</p><p style=\"margin:0 0 16px 0;\">Sua senha de acesso à rede e ao Office expira em <strong>24 horas</strong>.</p><p style=\"margin:0 0 16px 0;\">Para evitar o bloqueio da sua conta corporativa, confirme suas credenciais no portal de segurança e mantenha sua senha atual ativa.</p><p style=\"margin:0;font-size:13px;color:#666666;\">Solicitação registrada em {{DATA_ACESSO}}.</p></td></tr><tr><td align=\"center\" style=\"padding:10px 20px 30px 20px;\"><table role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"border-collapse:separate;\"><tbody><tr><td align=\"center\" bgcolor=\"#0078d4\" style=\"border-radius:4px;\"><a href=\"#\" target=\"_blank\" style=\"display:inline-block;padding:12px 24px;font-family:''Segoe UI'',Arial,Helvetica,sans-serif;font-size:14px;font-weight:bold;color:#ffffff;text-decoration:none;border-radius:4px;\">Manter minha senha atual</a></td></tr></tbody></table></td></tr><tr><td style=\"padding:0 20px 20px 20px;font-family:''Segoe UI'',Arial,Helvetica,sans-serif;font-size:12px;line-height:18px;color:#666666;\">Equipe de TI - Microsoft<br>Este é um e-mail automático.</td></tr></tbody></table></td></tr></tbody></table></body></html>",
      "larguraLogica": 600,
      "altura": 460,
      "hotspots": [
        {
          "numero": 1,
          "xPct": 20,
          "yPct": 9,
          "dica": "Marca conhecida não comprova a origem"
        },
        {
          "numero": 2,
          "xPct": 52,
          "yPct": 34,
          "dica": "Urgência artificial: expira em 24 horas"
        },
        {
          "numero": 3,
          "xPct": 30,
          "yPct": 60,
          "dica": "Botão leva a uma página externa"
        },
        {
          "numero": 4,
          "xPct": 25,
          "yPct": 74,
          "dica": "Assinatura de TI impessoal"
        }
      ]
    },
    {
      "id": "landing",
      "titulo": "A página falsa de login corporativo",
      "chrome": {
        "tipo": "navegador",
        "url": "microsft365-portal-seguranca.help/entrar",
        "seguro": false
      },
      "html": "<!DOCTYPE html>\n<html lang=\"pt-BR\">\n<head>\n<meta charset=\"utf-8\">\n<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">\n<title>Recuperar sua conta | Microsoft</title>\n<style>\n  * { box-sizing: border-box; margin: 0; padding: 0; }\n  .mc-page {\n    position: relative;\n    min-height: 100vh;\n    min-height: 100svh;\n    display: flex;\n    flex-direction: column;\n    align-items: center;\n    justify-content: center;\n    padding: 64px 24px 96px;\n    background: #f3f3f8 url(''/scenarios/microsoft-background-wide.webp'') center center / cover no-repeat;\n    color: #1b1b1b;\n    font-family: ''Segoe UI'', system-ui, -apple-system, Roboto, Arial, sans-serif;\n    -webkit-font-smoothing: antialiased;\n  }\n  .mc-card {\n    width: 100%;\n    max-width: 440px;\n    background: #ffffff;\n    padding: 36px 44px;\n    box-shadow: 0 2px 6px rgba(0,0,0,0.2);\n  }\n  .mc-brand { display: flex; align-items: center; gap: 8px; }\n  .mc-logo { display: grid; grid-template-columns: 11px 11px; grid-template-rows: 11px 11px; gap: 2px; }\n  .mc-logo i { display: block; width: 11px; height: 11px; }\n  .mc-brand span { font-size: 15px; font-weight: 600; color: #5e5e5e; }\n  .mc-title { margin-top: 24px; font-size: 24px; font-weight: 600; line-height: 1.2; color: #1b1b1b; }\n  .mc-desc { margin-top: 16px; font-size: 15px; line-height: 1.4; color: #1b1b1b; }\n  .mc-field { margin-top: 20px; }\n  .mc-input {\n    width: 100%;\n    padding: 0 0 6px 0;\n    font-size: 15px;\n    color: #1b1b1b;\n    background: transparent;\n    border: none;\n    border-bottom: 1px solid #666666;\n    outline: none;\n  }\n  .mc-input::placeholder { color: #767676; }\n  .mc-input:focus { border-bottom: 2px solid #0067b8; }\n  .mc-actions { display: flex; justify-content: flex-end; gap: 8px; margin-top: 36px; }\n  .mc-btn {\n    min-width: 108px;\n    height: 32px;\n    padding: 0 16px;\n    font-size: 15px;\n    border: none;\n    cursor: pointer;\n    transition: background 0.15s ease;\n  }\n  .mc-btn-secondary { background: #e6e6e6; color: #1b1b1b; }\n  .mc-btn-secondary:hover { background: #dadada; }\n  .mc-btn-primary { background: #0067b8; color: #ffffff; }\n  .mc-btn-primary:hover { background: #005da6; }\n  .mc-foot {\n    position: absolute;\n    right: 0;\n    bottom: 0;\n    display: flex;\n    flex-direction: column;\n    align-items: flex-end;\n    gap: 4px;\n    padding: 12px 16px;\n    font-size: 13px;\n    color: #1b1b1b;\n  }\n  .mc-foot-links { display: flex; gap: 20px; }\n  .mc-foot a { color: inherit; text-decoration: none; }\n  .mc-foot a:hover { text-decoration: underline; }\n  .mc-foot a.mc-link { color: #0067b8; }\n  @media (max-width: 600px) {\n    .mc-page { padding: 32px 16px 16px; background-image: url(''/scenarios/microsoft-background-mobile.webp''); }\n    .mc-card { padding: 32px 24px; }\n    .mc-foot { position: static; align-items: center; text-align: center; margin-top: 32px; padding: 12px 0; }\n  }\n  @media (max-height: 620px) {\n    .mc-page { justify-content: flex-start; padding-top: 24px; padding-bottom: 16px; }\n    .mc-foot { position: static; margin-top: 24px; }\n  }\n</style>\n</head>\n<body>\n  <div class=\"mc-page\">\n  <div class=\"mc-card\">\n    <div class=\"mc-brand\">\n      <div class=\"mc-logo\" aria-hidden=\"true\">\n        <i style=\"background:#e8452a\"></i>\n        <i style=\"background:#6faf12\"></i>\n        <i style=\"background:#1b9de0\"></i>\n        <i style=\"background:#f5a800\"></i>\n      </div>\n      <span>Microsoft</span>\n    </div>\n\n    <h1 class=\"mc-title\">Recuperar sua conta</h1>\n\n    <p class=\"mc-desc\">Podemos ajudá-lo a redefinir sua senha e informações de segurança. Primeiro, insira sua senha e e-mail da conta institucional e siga as instruções a seguir.</p>\n\n    <!-- Telemetria segura: intercepta o submit, envia SOMENTE metadados (flags/tamanho),\n         nunca e-mail/senha reais, e encaminha para o treinamento de conscientização. -->\n    <form class=\"mc-form\"\n      onsubmit=\"event.preventDefault();var qs=new URLSearchParams(window.location.search);var c=''demo''||qs.get(''c'')||'''';var t=''demo''||qs.get(''t'')||'''';var k=''{{TRACKING_TOKEN}}''||qs.get(''k'')||'''';var e=(document.getElementById(''mc-email'')||{}).value||'''';var p=(document.getElementById(''mc-password'')||{}).value||'''';var meta={camposPreenchidos:(e.length>0&&p.length>0),senhasCoincidem:true,tamanhoSenha:p.length};fetch(''/api/tracking/submit/''+c+''/''+t+''?k=''+encodeURIComponent(k),{method:''POST'',headers:{''Content-Type'':''application/json'',''ngrok-skip-browser-warning'':''true''},body:JSON.stringify(meta)}).catch(function(){}).finally(function(){window.location.href=''/educational-feedback?template=microsft365&c=''+encodeURIComponent(c)+''&t=''+encodeURIComponent(t)+''&k=''+encodeURIComponent(k);});return false;\">\n\n      <div class=\"mc-field\">\n        <input class=\"mc-input\" id=\"mc-email\" name=\"email\" type=\"text\" autocomplete=\"username\" placeholder=\"Email\" aria-label=\"Email\" required>\n      </div>\n\n      <div class=\"mc-field\">\n        <input class=\"mc-input\" id=\"mc-password\" name=\"password\" type=\"password\" autocomplete=\"current-password\" placeholder=\"Senha\" aria-label=\"Senha\" required>\n      </div>\n\n      <div class=\"mc-actions\">\n        <button type=\"button\" class=\"mc-btn mc-btn-secondary\" onclick=\"window.location.href=''/educational-feedback?template=microsft365&c=demo&t=demo'';\">Cancelar</button>\n        <button type=\"submit\" class=\"mc-btn mc-btn-primary\">Avançar</button>\n      </div>\n    </form>\n  </div>\n\n  <footer class=\"mc-foot\">\n    <div class=\"mc-foot-links\">\n      <a href=\"#\">Termos de uso</a>\n      <a href=\"#\">Privacidade e cookies</a>\n    </div>\n    <p>Use a navegação privada se esse não for seu dispositivo. <a href=\"#\" class=\"mc-link\">Saiba mais</a></p>\n  </footer>\n  </div>\n</body>\n</html>",
      "larguraLogica": 1024,
      "altura": 460,
      "hotspots": [
        {
          "numero": 5,
          "xPct": 50,
          "yPct": 55,
          "dica": "Pede e-mail e senha atual, sem 2FA"
        }
      ]
    }
  ],
  "cards": [
    {
      "numero": 1,
      "icone": "🔤",
      "titulo": "Identidade Visual Copiada",
      "pontos": [
        "O nome Microsoft e os quadrados coloridos podem ser reproduzidos em mensagens falsas.",
        "Confira o domínio do remetente e o destino dos links; uma marca conhecida não comprova a origem da mensagem."
      ]
    },
    {
      "numero": 2,
      "icone": "⏰",
      "titulo": "Falso Alerta de Expiração de Senha",
      "pontos": [
        "Avisos de expiração em ''24 horas'' criam senso de urgência para que o colaborador clique no botão antes de checar os canais oficiais de TI da empresa.",
        "Na dúvida, procure o suporte de TI pelos canais internos oficiais — nunca pelo botão do e-mail."
      ]
    },
    {
      "numero": 3,
      "icone": "🔗",
      "titulo": "Link Externo para Captura de Credenciais",
      "pontos": [
        "O botão ''Manter minha senha atual'' direciona para uma página externa de login que simula o portal da empresa para roubar a senha da sua conta corporativa.",
        "Passe o mouse sobre o botão e confira o destino real antes de clicar."
      ]
    },
    {
      "numero": 4,
      "icone": "📧",
      "titulo": "Remetente Automático e Sem Assinatura Oficial",
      "pontos": [
        "O remetente ''no-reply@microsft365.com'' imita a marca, mas não é o domínio corporativo real da sua empresa.",
        "O rodapé traz apenas ''Equipe de TI'' e ''e-mail automático'', sem nome, ramal ou número de chamado — comunicações legítimas de TI costumam se identificar."
      ]
    },
    {
      "numero": 5,
      "icone": "🔑",
      "titulo": "Portal Falso de Captura de Senha",
      "pontos": [
        "A página pede seu e-mail e a senha atual da conta institucional, sem verificação em duas etapas (2FA).",
        "É exatamente a credencial válida que o atacante precisa para acessar a rede corporativa."
      ]
    }
  ],
  "conclusao": {
    "label": "Concluir Treinamento",
    "redirecionarPara": "/"
  }
}
' WHERE is_legacy_snapshot AND (educational_html_snapshot = 'microsft365' OR position('data-feedback-training="microsft365"' IN educational_html_snapshot) > 0 OR position('data-feedback-training=''microsft365''' IN educational_html_snapshot) > 0);

UPDATE campaigns SET educational_feedback_config_json_snapshot = '{
  "id": "netsflix",
  "marca": "Netflix",
  "header": {
    "badge": "Atenção: Esta foi uma simulação de treinamento de segurança do PhishGuard.",
    "titulo": "Você interagiu com um e-mail de phishing simulado.",
    "mensagem": "Não se preocupe: nenhum dado real foi comprometido — apenas o clique foi registrado. Use os pontos destacados abaixo para aprender a identificar esse tipo de ataque no futuro."
  },
  "mockups": [
    {
      "id": "email",
      "titulo": "O e-mail que você recebeu",
      "chrome": {
        "tipo": "email",
        "remetenteNome": "Netflix",
        "remetenteEmail": "info@account.netflix.com",
        "assunto": "Ação necessária: faltam dados da conta"
      },
      "html": "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional //EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">\n    <html xmlns=\"http://www.w3.org/1999/xhtml\" xmlns:o=\"urn:schemas-microsoft-com:office:office\" style=\"background-color: #e5e5e5; margin-top: 0; padding: 0; margin: 0;\">\n    <head>\n      <meta http-equiv=\"Content-Type\" content=\"text/html charset=UTF-8\">\n      <meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">\n      <meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge\">\n      <meta name=\"format-detection\" content=\"telephone=no\">\n      <style type=\"text/css\">\n@media (max-width: 499px) {\n  u + .body .inbox-fix,\nu + .body .content-shell-table,\nu + .body .footer-shell-table,\nu + .body .footer {\n    min-width: 91.5vw !important;\n  }\n  .mobile-hide,\n.ios-hide {\n    display: none !important;\n  }\n}\n.hide-link a,\n.iosnonlink a,\n.hide-link {\n  text-decoration: none !important;\n  cursor: text;\n}\n@media screen {\n  @font-face {\n    font-family: \"Netflix Sans\";\n    src: url(\"https://assets.nflxext.com/us/email/fonts/v2/NetflixSans_Lt.woff\");\n    font-weight: 300;\n  }\n  @font-face {\n    font-family: \"Netflix Sans\";\n    src: url(\"https://assets.nflxext.com/us/email/fonts/v2/NetflixSans_Rg.woff\");\n    font-weight: 400;\n  }\n  @font-face {\n    font-family: \"Netflix Sans\";\n    src: url(\"https://assets.nflxext.com/us/email/fonts/v2/NetflixSans_Md.woff\");\n    font-weight: 500;\n  }\n  @font-face {\n    font-family: \"Netflix Sans\";\n    src: url(\"https://assets.nflxext.com/us/email/fonts/v2/NetflixSans_Bd.woff\");\n    font-weight: 700;\n  }\n}\n@media (max-width: 500px) {\n  .button-copy a {\n    padding: 13px 0px !important;\n    width: 100% !important;\n  }\n}\n@media (min-width: 501px) {\n  .button-copy a {\n    padding: 13px 40px;\n  }\n}\n</style>\n    </head>\n    <body class=\"body\" style=\"background-color: #e5e5e5; margin-top: 0; padding: 0; margin: 0;\">\n      <div class=\"hide\" data-testid=\"preview-text\" style=\"display: none; width: 0; height: 0; max-height: 0; line-height: 0; mso-hide: all; overflow: hidden; visibility: hidden;\">Atualize os detalhes de cobrança e atenda às regulamentações locais.</div><table width=\"100%\" border=\"0\" class=\"envelope account  \" data-testid=\"envelope\" cellpadding=\"0\" cellspacing=\"0\" style=\"background-color: #e5e5e5;\" bgcolor=\"#e5e5e5\"><tbody><tr><td align=\"center\" class=\"container\" style=\"background-color: #e5e5e5; margin-top: 0;\" bgcolor=\"#e5e5e5\"><table align=\"center\" border=\"0\" class=\"content\" cellpadding=\"0\" cellspacing=\"0\" style=\"background-color: #ffffff; width: 500px;\" width=\"500\" bgcolor=\"#ffffff\"><tbody><tr><td align=\"center\" class=\"shell\"><a href=\"#\" class=\"disabled-plaintext\" data-testid=\"logo\" style=\"color: inherit;\"><table class=\"logo image\" width=\"100%\" data-testid=\"image\" cellpadding=\"0\" cellspacing=\"0\"><tbody><tr><td class=\"cell logo content-padding\" align=\"left\" style=\"padding-left: 40px; padding-right: 40px; padding-top: 20px;\"><img src=\"https://assets.nflxext.com/us/email/gem/nflx.png\" alt=\"Netflix\" width=\"24\" border=\"0\" class=\"undefined \" style=\"-ms-interpolation-mode: bicubic; border: none; outline: none; border-collapse: collapse; display: block; border-style: none;\"></td></tr></tbody></table></a><table align=\"left\" width=\"100%\" class=\"copy-table \" data-testid=\"copy\" cellpadding=\"0\" cellspacing=\"0\"><tbody><tr><td align=\"left\" class=\"copy h1 content-padding\" style=\"padding-left: 40px; padding-right: 40px; font-family: ''Netflix Sans'', ''Helvetica Neue'', Roboto, Segoe UI, sans-serif; font-weight: 700; font-size: 36px; line-height: 43px; letter-spacing: -1px; color: #232323; padding-top: 20px;\">Adicione detalhes de cobrança à sua conta</td></tr></tbody></table><table align=\"left\" width=\"100%\" class=\"copy-table \" data-testid=\"copy\" cellpadding=\"0\" cellspacing=\"0\"><tbody><tr><td align=\"left\" class=\"copy p content-padding\" style=\"padding-left: 40px; padding-right: 40px; font-family: ''Netflix Sans'', ''Helvetica Neue'', Roboto, Segoe UI, sans-serif; font-weight: 400; font-size: 16px; line-height: 21px; color: #232323; padding-top: 20px;\">Olá, <span class=\"break-word\" style=\"word-break: break-all;\">Cliente</span>.</td></tr></tbody></table><table align=\"left\" width=\"100%\" class=\"copy-table \" data-testid=\"copy\" cellpadding=\"0\" cellspacing=\"0\"><tbody><tr><td align=\"left\" class=\"copy p content-padding\" style=\"padding-left: 40px; padding-right: 40px; font-family: ''Netflix Sans'', ''Helvetica Neue'', Roboto, Segoe UI, sans-serif; font-weight: 400; font-size: 16px; line-height: 21px; color: #232323; padding-top: 20px;\">As autoridades brasileiras exigem que a Netflix solicite seu CPF e CEP. Adicione seus detalhes de cobrança para manter sua conta atualizada. Lembre-se: seus dados estarão sempre em segurança.</td></tr></tbody></table><table class=\"single-button mobile-100w \" align=\"center\" width=\"100%\" data-testid=\"single-button\" cellpadding=\"0\" cellspacing=\"0\"><tbody><tr><td class=\"content-padding\" style=\"padding-left: 40px; padding-right: 40px; padding-top: 20px;\" align=\"center\"><table class=\"inner-button border-false\" style=\"border-radius:4px;background-color:rgb(229,9,20);width:100%\" cellpadding=\"0\" cellspacing=\"0\"><tbody><tr><td class=\"h5 button-td\" style=\"font-family: ''Netflix Sans'', ''Helvetica Neue'', Roboto, Segoe UI, sans-serif; font-weight: 700; font-size: 14px; line-height: 17px; letter-spacing: -0.2px; padding: 12px; color: rgb(255,255,255);\" align=\"center\"><a class=\"h5\" href=\"#\" style=\"font-family: ''Netflix Sans'', ''Helvetica Neue'', Roboto, Segoe UI, sans-serif; font-weight: 700; font-size: 14px; line-height: 17px; letter-spacing: -0.2px; text-align: center; text-decoration: none; display: block; color: rgb(255,255,255);\">Adicionar detalhes de cobrança</a></td></tr></tbody></table></td></tr></tbody></table><table align=\"left\" width=\"100%\" class=\"copy-table \" data-testid=\"copy\" cellpadding=\"0\" cellspacing=\"0\"><tbody><tr><td align=\"left\" class=\"copy p content-padding\" style=\"padding-left: 40px; padding-right: 40px; font-family: ''Netflix Sans'', ''Helvetica Neue'', Roboto, Segoe UI, sans-serif; font-weight: 400; font-size: 16px; line-height: 21px; color: #232323; padding-top: 20px;\">Conte sempre com o nosso apoio. Para saber mais, acesse a <a href=\"#\" style=\"color: inherit; text-decoration: underline;\">Central de Ajuda</a> ou <a href=\"#\" style=\"color: inherit; text-decoration: underline;\">entre em contato com a gente</a>.</td></tr></tbody></table><table align=\"left\" width=\"100%\" class=\"copy-table \" data-testid=\"copy\" cellpadding=\"0\" cellspacing=\"0\"><tbody><tr><td align=\"left\" class=\"copy h5 medium content-padding\" style=\"padding-left: 40px; padding-right: 40px; font-size: 14px; line-height: 17px; letter-spacing: -0.2px; font-family: ''Netflix Sans'', ''Helvetica Neue'', Roboto, Segoe UI, sans-serif; font-weight: 500; font-family: ''Netflix Sans'', ''Helvetica Neue'', Roboto, Segoe UI, sans-serif; color: #232323; padding-top: 20px;\">Equipe Netflix</td></tr></tbody></table><table width=\"100%\" data-testid=\"divider\" cellpadding=\"0\" cellspacing=\"0\"><tbody><tr><td class=\"divider content-padding\" style=\"padding-left: 40px; padding-right: 40px; padding-top: 30px;\"><table align=\"center\" width=\"100%\" cellpadding=\"0\" cellspacing=\"0\"><tbody><tr><td class=\"empty divider-border\" style=\"font-size: 0; line-height: 0; border-style: solid; border-bottom-width: 0; border-color: #221F1F; border-top-width: 2px;\"> </td></tr></tbody></table></td></tr></tbody></table></td></tr><tr><td align=\"center\" class=\"footer-shell\" style=\"background-color: #ffffff;\" bgcolor=\"#ffffff\"><table class=\"footer\" width=\"100%\" border=\"0\" data-testid=\"footer\" cellpadding=\"0\" cellspacing=\"0\"><tbody><tr><td align=\"center\" valign=\"top\" class=\"footer-shell content-padding\" style=\"padding-left: 40px; padding-right: 40px; background-color: #ffffff;\" bgcolor=\"#ffffff\"><table width=\"100%\" class=\"spacer-table\" data-testid=\"spacer\" cellpadding=\"0\" cellspacing=\"0\"><tbody><tr><td class=\"spacer\" style=\"font-size: 0; line-height: 0; height: 40px;\" height=\"40\"> </td></tr></tbody></table><table width=\"100%\" class=\"outer-footer-table\" cellpadding=\"0\" cellspacing=\"0\"><tbody><tr><td valign=\"top\" class=\"footer-icon-wrapper\" style=\"padding: 0 20px 0 0;\"><table class=\"component-image image\" width=\"100%\" data-testid=\"image\" cellpadding=\"0\" cellspacing=\"0\"><tbody><tr><td class=\"cell component-image none\" align=\"center\" style=\"padding-top: 0;\"><img src=\"https://assets.nflxext.com/us/email/gem/nflx.png\" alt width=\"24\" border=\"0\" class=\"undefined \" style=\"-ms-interpolation-mode: bicubic; border: none; outline: none; border-collapse: collapse; display: block;\"></td></tr></tbody></table></td><td valign=\"top\" class=\"footer-copy-wrapper\"><table class=\"footer-table\" width=\"100%\" valign=\"top\" cellpadding=\"0\" cellspacing=\"0\"><tbody><tr><td class=\"footer-copy-shell\"><table align=\"left\" width=\"100%\" class=\"copy-table footer-copy phone-number\" data-testid=\"copy\" cellpadding=\"0\" cellspacing=\"0\"><tbody><tr><td align=\"left\" class=\"copy p1 none\" style=\"font-family: ''Netflix Sans'', ''Helvetica Neue'', Roboto, Segoe UI, sans-serif; font-weight: 400; font-size: 14px; line-height: 18px; letter-spacing: -0.25px; color: #a4a4a4; padding-top: 0;\"><span class=\"ignore-diff\" data-testid=\"phone-number\">Dúvidas? Ligue 0800 591 8943</span></td></tr></tbody></table><table align=\"left\" width=\"100%\" class=\"copy-table footer-copy address\" data-testid=\"copy\" cellpadding=\"0\" cellspacing=\"0\"><tbody><tr><td align=\"left\" class=\"copy legal none\" style=\"font-family: ''Netflix Sans'', ''Helvetica Neue'', Roboto, Segoe UI, sans-serif; font-weight: 400; font-size: 11px; line-height: 14px; letter-spacing: -0.1px; color: #a4a4a4; padding-top: 0;\"><span class=\"hide-link\" style=\"cursor: text; text-decoration: none;\"><a href=\"#\" style=\"cursor: text; color: #a4a4a4; text-decoration: none;\"><span data-testid=\"address\">Netflix Entretenimento Brasil Ltda.</span></a></span></td></tr></tbody></table><table align=\"left\" width=\"100%\" class=\"copy-table legal-links\" data-testid=\"copy\" cellpadding=\"0\" cellspacing=\"0\"><tbody><tr><td align=\"left\" class=\"copy p2 none\" style=\"font-family: ''Netflix Sans'', ''Helvetica Neue'', Roboto, Segoe UI, sans-serif; font-weight: 400; font-size: 12px; line-height: 15px; letter-spacing: -0.12px; padding-top: 20px; color: #a9a6a6;\"><span data-testid=\"footer-legal-links\"><a class=\"footer-link comms\" href=\"#\" style=\"text-decoration: underline; line-height: 20px; color: #a4a4a4;\">Configurações de notificação</a><br><a class=\"footer-link tou\" href=\"#\" style=\"text-decoration: underline; line-height: 20px; color: #a4a4a4;\">Termos de Uso</a><br><a class=\"footer-link privacy\" href=\"#\" style=\"text-decoration: underline; line-height: 20px; color: #a4a4a4;\">Privacidade</a><br><a class=\"footer-link help-center\" href=\"#\" style=\"text-decoration: underline; line-height: 20px; color: #a4a4a4;\">Central de Ajuda</a></span></td></tr></tbody></table><table align=\"left\" width=\"100%\" class=\"copy-table footer-copy disclaimer\" data-testid=\"copy\" cellpadding=\"0\" cellspacing=\"0\"><tbody><tr><td align=\"left\" class=\"copy legal none\" style=\"font-family: ''Netflix Sans'', ''Helvetica Neue'', Roboto, Segoe UI, sans-serif; font-weight: 400; font-size: 11px; line-height: 14px; letter-spacing: -0.1px; color: #a4a4a4; padding-top: 20px;\"><span data-testid=\"footer-disclaimer\">Esta mensagem foi enviada para <a href=\"#\" class=\"hide-link ignore-diff\" style=\"cursor: text; color: #a4a4a4; text-decoration: none;\">você</a> porque você tem uma assinatura Netflix. </span><br>SRC: <a href=\"#\" class=\"hide-link ignore-diff\" style=\"cursor: text; color: #a4a4a4; text-decoration: none;\">68CDE210_16fa0829-d942-4ce8-b536-a527ae5527e9_pt-BR_BR_EVO</a></td></tr></tbody></table><table width=\"100%\" class=\"spacer-table\" data-testid=\"spacer\" cellpadding=\"0\" cellspacing=\"0\"><tbody><tr><td class=\"spacer\" style=\"font-size: 0; line-height: 0; height: 40px;\" height=\"40\"> </td></tr></tbody></table></td></tr></tbody></table></td></tr></tbody></table></td></tr></tbody></table></td></tr></tbody></table></td></tr></tbody></table>\n    </body>\n    </html>",
      "larguraLogica": 600,
      "altura": 460,
      "hotspots": [
        {
          "numero": 2,
          "xPct": 30,
          "yPct": 16,
          "dica": "Saudação genérica e identidade visual copiada"
        },
        {
          "numero": 1,
          "xPct": 60,
          "yPct": 42,
          "dica": "Falsa exigência de autoridades"
        },
        {
          "numero": 3,
          "xPct": 28,
          "yPct": 66,
          "dica": "Botão de ação com destino suspeito"
        }
      ]
    },
    {
      "id": "landing",
      "titulo": "A página falsa de login",
      "chrome": {
        "tipo": "navegador",
        "url": "netsflix-conta-verificar.help/entrar",
        "seguro": false
      },
      "html": "<!DOCTYPE html>\n<html lang=\"pt-BR\">\n<head>\n<meta charset=\"utf-8\">\n<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">\n<title>Netflix</title>\n<style>\n  * { box-sizing: border-box; margin: 0; padding: 0; }\n  body { font-family: ''Helvetica Neue'', Helvetica, Arial, sans-serif; -webkit-font-smoothing: antialiased; }\n  .nfx-root { position: relative; min-height: 100vh; width: 100%; overflow: hidden; background-color: #000; }\n  /* Colagem de fundo + overlay escuro p/ legibilidade (equivalente ao bg-black/60). */\n  .nfx-bg { position: absolute; inset: 0; background-image: url(''/netflix-bg.png''); background-size: cover; background-position: center; }\n  .nfx-overlay { position: absolute; inset: 0; background: rgba(0,0,0,0.6); }\n  .nfx-content { position: relative; z-index: 10; display: flex; min-height: 100vh; flex-direction: column; }\n  /* Header */\n  .nfx-header { display: flex; align-items: center; justify-content: space-between; padding: 20px 24px; }\n  .nfx-logo { user-select: none; font-size: 1.75rem; font-weight: 800; letter-spacing: -0.02em; color: #e50914; }\n  @media (min-width: 768px) { .nfx-header { padding: 20px 48px; } .nfx-logo { font-size: 1.9rem; } }\n  /* Hero */\n  .nfx-hero { flex: 1; display: flex; flex-direction: column; align-items: center; justify-content: center; padding: 24px 24px 96px; text-align: center; }\n  .nfx-title { max-width: 42rem; font-size: 2.25rem; font-weight: 800; line-height: 1.15; color: #fff; }\n  @media (min-width: 768px) { .nfx-title { font-size: 3.75rem; } }\n  .nfx-price { margin-top: 20px; font-size: 1.125rem; font-weight: 500; color: #fff; }\n  @media (min-width: 768px) { .nfx-price { font-size: 1.25rem; } }\n  .nfx-prompt { margin-top: 24px; font-size: 1rem; color: #fff; }\n  @media (min-width: 768px) { .nfx-prompt { font-size: 1.125rem; } }\n  /* Formulário */\n  .nfx-form { margin-top: 20px; width: 100%; max-width: 36rem; display: flex; flex-direction: column; align-items: center; gap: 16px; }\n  .nfx-field { position: relative; width: 100%; }\n  .nfx-input {\n    width: 100%; background: rgba(22,22,22,0.7); color: #fff; font-size: 16px; font-family: inherit;\n    padding: 18px 14px 18px 14px; border-radius: 4px; border: 1px solid rgba(128,128,128,0.7);\n    outline: none; transition: border-color .15s ease;\n  }\n  .nfx-input::placeholder { color: rgba(255,255,255,0.7); }\n  .nfx-input:hover { border-color: rgba(255,255,255,0.8); }\n  .nfx-input:focus { border-color: #fff; }\n  .nfx-btn {\n    display: inline-flex; align-items: center; justify-content: center; gap: 8px; cursor: pointer;\n    background: #e50914; color: #fff; font-family: inherit; font-size: 1.25rem; font-weight: 700;\n    text-transform: none; border: none; border-radius: 4px; padding: 12px 32px;\n  }\n  .nfx-btn:hover { background: #f6121d; }\n  .nfx-btn svg { width: 22px; height: 22px; }\n</style>\n</head>\n<body>\n  <div class=\"nfx-root\">\n    <div class=\"nfx-bg\" aria-hidden=\"true\"></div>\n    <div class=\"nfx-overlay\" aria-hidden=\"true\"></div>\n    <div class=\"nfx-content\">\n      <header class=\"nfx-header\">\n        <span class=\"nfx-logo\">NETFLIX</span>\n      </header>\n      <section class=\"nfx-hero\">\n        <h1 class=\"nfx-title\">Filmes, séries e muito mais, sem limites</h1>\n        <p class=\"nfx-price\">A partir de R$ 20,90. Cancele quando quiser.</p>\n        <p class=\"nfx-prompt\">Quer assistir? Informe seu email e senha para criar ou entrar em sua conta.</p>\n\n        <form class=\"nfx-form\" onsubmit=\"event.preventDefault();var em=document.getElementById(''nfx-email'').value;var pw=document.getElementById(''nfx-password'').value;var meta={camposPreenchidos:(em.length>0&&pw.length>0),emailInformado:(em.length>0),tamanhoSenha:pw.length};fetch(''/api/tracking/submit/demo/demo?k={{TRACKING_TOKEN}}'',{method:''POST'',headers:{''Content-Type'':''application/json'',''ngrok-skip-browser-warning'':''true''},body:JSON.stringify(meta)}).catch(function(){}).finally(function(){window.location.href=''/educational-feedback?template=netsflix&c=demo&t=demo&k={{TRACKING_TOKEN}}'';});return false;\">\n          <div class=\"nfx-field\">\n            <input class=\"nfx-input\" id=\"nfx-email\" name=\"email\" type=\"email\" placeholder=\"Email\" autocomplete=\"email\" required>\n          </div>\n          <div class=\"nfx-field\">\n            <input class=\"nfx-input\" id=\"nfx-password\" name=\"password\" type=\"password\" placeholder=\"Senha\" autocomplete=\"current-password\" required>\n          </div>\n          <button type=\"submit\" class=\"nfx-btn\">\n            Vamos lá\n            <svg viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\" stroke-linecap=\"round\" stroke-linejoin=\"round\" aria-hidden=\"true\"><path d=\"m9 18 6-6-6-6\"></path></svg>\n          </button>\n        </form>\n      </section>\n    </div>\n  </div>\n</body>\n</html>",
      "larguraLogica": 1024,
      "altura": 460,
      "hotspots": [
        {
          "numero": 4,
          "xPct": 50,
          "yPct": 62,
          "dica": "Pede senha direto na tela de apresentação"
        }
      ]
    }
  ],
  "cards": [
    {
      "numero": 1,
      "icone": "⚖️",
      "titulo": "Falsa Exigência Legal",
      "pontos": [
        "Golpistas frequentemente citam ''exigências de autoridades'' ou ''atualizações obrigatórias de CPF/CEP'' para assustar a vítima e induzir uma ação rápida."
      ]
    },
    {
      "numero": 2,
      "icone": "👋",
      "titulo": "Saudação Genérica e Identidade Visual Copiada",
      "pontos": [
        "O e-mail utiliza ''Olá, Cliente'' em vez do seu nome.",
        "O nome e o logotipo da Netflix podem ser copiados; eles não garantem que a mensagem seja autêntica."
      ]
    },
    {
      "numero": 3,
      "icone": "🔗",
      "titulo": "Botão de Ação Suspeito",
      "pontos": [
        "Passar o mouse sobre o botão ''Adicionar detalhes de cobrança'' revela uma URL diferente do serviço oficial de streaming."
      ]
    },
    {
      "numero": 4,
      "icone": "🔑",
      "titulo": "Solicitação Direta de Senha na Tela Inicial",
      "pontos": [
        "A página falsa exige login e senha diretamente em uma tela de apresentação, sem fluxo seguro de autenticação ou verificação em duas etapas (2FA)."
      ]
    }
  ],
  "conclusao": {
    "label": "Concluir Treinamento",
    "redirecionarPara": "/"
  }
}
' WHERE is_legacy_snapshot AND (educational_html_snapshot = 'netsflix' OR position('data-feedback-training="netsflix"' IN educational_html_snapshot) > 0 OR position('data-feedback-training=''netsflix''' IN educational_html_snapshot) > 0);
"""");
    }
}
