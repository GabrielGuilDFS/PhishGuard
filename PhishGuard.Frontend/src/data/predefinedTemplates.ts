// Catálogo estático de templates predefinidos do PhishGuard.
// Entradas com isPredefinido=true são iscas de referência mantidas pela plataforma,
// agrupadas por categoria. O placeholder {{LINK_PHISHING}} é substituído pela URL
// de rastreamento da campanha no momento do disparo.

export interface TemplatePredefinido {
  id: string;
  nome: string;
  assunto: string;
  remetenteNome: string;
  remetenteEmail: string;
  isPredefinido: true;
  categoria: string;
  corpoHtml: string;
}

const catalogoIscasOficiais: TemplatePredefinido[] = [
  {
    id: 'hbomax-redefinicao-senha',
    nome: 'HBO Max - Redefinição de Senha',
    assunto: 'Seu link para alteração de senha solicitado',
    remetenteNome: 'HBO Max',
    remetenteEmail: 'no-reply@alerts.hbomax.com',
    isPredefinido: true,
    categoria: 'Streaming',
    corpoHtml: `<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html dir="ltr" xmlns:o="urn:schemas-microsoft-com:office:office" xmlns="http://www.w3.org/1999/xhtml" lang="pt">
 <head>
<meta http-equiv="Content-Type" content="text/html; charset=UTF-8">
  <meta name="viewport" content="width=device-width, initial-scale=1">
  <meta charset="UTF-8">
  <meta name="x-apple-disable-message-reformatting">
  <meta http-equiv="X-UA-Compatible" content="IE=edge">
  <meta name="format-detection" content="telephone=no">
  <title>Seu link para alteração de senha solicitado às  8:28pm EST</title>
  <style type="text/css">@media only screen and (max-width:600px) { .mobile-cta, .es-content-body a.mobile-cta { font-size:18px!important;font-weight:bold!important;padding:5px 20px!important; } }
u + .body img ~ div div {
  display:none;
}
#outlook a {
  padding:0;
}
a.es-button, button.es-button, label.es-button {
  mso-style-priority:100!important;
}
</style>
 </head>
 <body class="body" style="width:100%;height:100%;font-family:verdana, geneva, sans-serif;-webkit-text-size-adjust:100%;-ms-text-size-adjust:100%;padding:0;Margin:0">
<div style="display:none !important;visibility:hidden;mso-hide:all;font-size:1px;color:#ffffff;line-height:1px;max-height:0px;max-width:0px;opacity:0;overflow:hidden;">Recupere sua senha</div>
  <div dir="ltr" class="es-wrapper-color" lang="pt" style="background-color:#DEDEDE">
   <table cellspacing="0" cellpadding="0" width="100%" class="es-wrapper" role="none" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px;padding:0;Margin:0;width:100%;height:100%;background-repeat:repeat;background-position:center top">
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
                      <td align="left" class="es-m-p30r es-m-p15l" style="padding:0;Margin:0;padding-top:20px;padding-bottom:30px;padding-left:30px;font-size:0"><a target="_blank" universal="true"  href="#"><img src="https://braze-images.com/appboy/communication/assets/image_assets/images/6977daaba0ab32006333901d/original.png?1769462443" alt="" width="63" class="img-2003" height="46" style="display:block;font-size:22px;border:0;outline:none;text-decoration:none;margin:0"></a></td>
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
           <table bgcolor="#ffffff" align="center" cellspacing="0" cellpadding="0" class="es-content-body" role="none" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px;background-color:#FFFFFF;width:600px">
             <tr>
              <td align="left" class="es-m-p30r es-m-p15l" style="padding:0 30px 10px;Margin:0">
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
             <tr>
              <td align="left" class="es-m-p15r es-m-p15l" style="padding:0 30px;Margin:0">
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
</td>
             </tr>
             <tr>
              <td align="left" bgcolor="#ffffff" class="es-m-p15r es-m-p15l" style="padding:0 30px 20px;Margin:0;background-color:#ffffff">
               <table width="100%" cellpadding="0" cellspacing="0" role="none" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px">
                 <tr>
                  <td align="center" valign="top" style="padding:0;Margin:0;width:540px">
                   <table cellspacing="0" width="100%" cellpadding="0" role="presentation" style="mso-table-lspace:0pt;mso-table-rspace:0pt;border-spacing:0px">
                     <tr>
                      <td align="left" style="padding:0;Margin:0" class="es-m-text"><p class="es-text-mobile-size-16" style="Margin:0;mso-line-height-rule:exactly;font-family:verdana, geneva, sans-serif;line-height:24px;letter-spacing:0;color:#000000;font-size:16px;margin-bottom:17px">Este link irá expirar em Jun 25, 2026 no 08:28pm EST ou quando um novo pedido de alteração de senha for solicitado.&nbsp;&nbsp;</p></td>
                     </tr>
                   </table>
</td>
                 </tr>
               </table>
</td>
             </tr>
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
             <tr>
              <td align="left" class="bf" style="padding:0;Margin:0;padding-bottom:10px"><p class="y" style="Margin:0;mso-line-height-rule:exactly;font-family:'Helvetica Neue', Helvetica, Arial, sans-serif;line-height:18px;letter-spacing:0;color:#000000;font-size:12px;margin-bottom:9px;-webkit-text-size-adjust:none;-ms-text-size-adjust:none"><span style="color:#000000">230 Park Avenue South, New York, NY 10003<br>©2026 WarnerMedia Direct, LLC. All Rights Reserved.</span></p></td>
             </tr>
             <tr>
              <td align="left" class="bf" style="padding:0;Margin:0;padding-bottom:10px"><p class="y" style="Margin:0;mso-line-height-rule:exactly;font-family:'Helvetica Neue', Helvetica, Arial, sans-serif;line-height:18px;letter-spacing:0;color:#000000;font-size:12px;margin-bottom:9px;-webkit-text-size-adjust:none;-ms-text-size-adjust:none"><span style="color:#000000">Este correo electrónico fue enviado desde una dirección que solo envía notificaciones y no acepta mensajes entrantes.</span></p></td>
             </tr>
           </table>
</td>
         </tr>
       </table>
</td>
     </tr>
   </table>
  </div></body>
</html>`,
  },
  {
    id: 'netflix-atualizacao-cobranca',
    nome: 'Netflix - Atualização de Detalhes de Cobrança',
    assunto: 'Ação necessária: faltam dados da conta',
    remetenteNome: 'Netflix',
    remetenteEmail: 'info@account.netflix.com',
    isPredefinido: true,
    categoria: 'Streaming',
    corpoHtml: `<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional //EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
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
      <div class="hide" data-testid="preview-text" style="display: none; width: 0; height: 0; max-height: 0; line-height: 0; mso-hide: all; overflow: hidden; visibility: hidden;">Atualize os detalhes de cobrança e atenda às regulamentações locais.</div><table width="100%" border="0" class="envelope account  " data-testid="envelope" cellpadding="0" cellspacing="0" style="background-color: #e5e5e5;" bgcolor="#e5e5e5"><tbody><tr><td align="center" class="container" style="background-color: #e5e5e5; margin-top: 0;" bgcolor="#e5e5e5"><table align="center" border="0" class="content" cellpadding="0" cellspacing="0" style="background-color: #ffffff; width: 500px;" width="500" bgcolor="#ffffff"><tbody><tr><td align="center" class="shell"><a href="#" class="disabled-plaintext" data-testid="logo" style="color: inherit;"><table class="logo image" width="100%" data-testid="image" cellpadding="0" cellspacing="0"><tbody><tr><td class="cell logo content-padding" align="left" style="padding-left: 40px; padding-right: 40px; padding-top: 20px;"><img src="https://assets.nflxext.com/us/email/gem/nflx.png" alt="Netflix" width="24" border="0" class="undefined " style="-ms-interpolation-mode: bicubic; border: none; outline: none; border-collapse: collapse; display: block; border-style: none;"></td></tr></tbody></table></a><table align="left" width="100%" class="copy-table " data-testid="copy" cellpadding="0" cellspacing="0"><tbody><tr><td align="left" class="copy h1 content-padding" style="padding-left: 40px; padding-right: 40px; font-family: 'Netflix Sans', 'Helvetica Neue', Roboto, Segoe UI, sans-serif; font-weight: 700; font-size: 36px; line-height: 43px; letter-spacing: -1px; color: #232323; padding-top: 20px;">Adicione detalhes de cobrança à sua conta</td></tr></tbody></table><table align="left" width="100%" class="copy-table " data-testid="copy" cellpadding="0" cellspacing="0"><tbody><tr><td align="left" class="copy p content-padding" style="padding-left: 40px; padding-right: 40px; font-family: 'Netflix Sans', 'Helvetica Neue', Roboto, Segoe UI, sans-serif; font-weight: 400; font-size: 16px; line-height: 21px; color: #232323; padding-top: 20px;">Olá, <span class="break-word" style="word-break: break-all;">Cliente</span>.</td></tr></tbody></table><table align="left" width="100%" class="copy-table " data-testid="copy" cellpadding="0" cellspacing="0"><tbody><tr><td align="left" class="copy p content-padding" style="padding-left: 40px; padding-right: 40px; font-family: 'Netflix Sans', 'Helvetica Neue', Roboto, Segoe UI, sans-serif; font-weight: 400; font-size: 16px; line-height: 21px; color: #232323; padding-top: 20px;">As autoridades brasileiras exigem que a Netflix solicite seu CPF e CEP. Adicione seus detalhes de cobrança para manter sua conta atualizada. Lembre-se: seus dados estarão sempre em segurança.</td></tr></tbody></table><table class="single-button mobile-100w " align="center" width="100%" data-testid="single-button" cellpadding="0" cellspacing="0"><tbody><tr><td class="content-padding" style="padding-left: 40px; padding-right: 40px; padding-top: 20px;" align="center"><table class="inner-button border-false" style="border-radius:4px;background-color:rgb(229,9,20);width:100%" cellpadding="0" cellspacing="0"><tbody><tr><td class="h5 button-td" style="font-family: 'Netflix Sans', 'Helvetica Neue', Roboto, Segoe UI, sans-serif; font-weight: 700; font-size: 14px; line-height: 17px; letter-spacing: -0.2px; padding: 12px; color: rgb(255,255,255);" align="center"><a class="h5" href="{{LINK_PHISHING}}" style="font-family: 'Netflix Sans', 'Helvetica Neue', Roboto, Segoe UI, sans-serif; font-weight: 700; font-size: 14px; line-height: 17px; letter-spacing: -0.2px; text-align: center; text-decoration: none; display: block; color: rgb(255,255,255);">Adicionar detalhes de cobrança</a></td></tr></tbody></table></td></tr></tbody></table><table align="left" width="100%" class="copy-table " data-testid="copy" cellpadding="0" cellspacing="0"><tbody><tr><td align="left" class="copy p content-padding" style="padding-left: 40px; padding-right: 40px; font-family: 'Netflix Sans', 'Helvetica Neue', Roboto, Segoe UI, sans-serif; font-weight: 400; font-size: 16px; line-height: 21px; color: #232323; padding-top: 20px;">Conte sempre com o nosso apoio. Para saber mais, acesse a <a href="{{LINK_PHISHING}}" style="color: inherit; text-decoration: underline;">Central de Ajuda</a> ou <a href="{{LINK_PHISHING}}" style="color: inherit; text-decoration: underline;">entre em contato com a gente</a>.</td></tr></tbody></table><table align="left" width="100%" class="copy-table " data-testid="copy" cellpadding="0" cellspacing="0"><tbody><tr><td align="left" class="copy h5 medium content-padding" style="padding-left: 40px; padding-right: 40px; font-size: 14px; line-height: 17px; letter-spacing: -0.2px; font-family: 'Netflix Sans', 'Helvetica Neue', Roboto, Segoe UI, sans-serif; font-weight: 500; font-family: 'Netflix Sans', 'Helvetica Neue', Roboto, Segoe UI, sans-serif; color: #232323; padding-top: 20px;">Equipe Netflix</td></tr></tbody></table><table width="100%" data-testid="divider" cellpadding="0" cellspacing="0"><tbody><tr><td class="divider content-padding" style="padding-left: 40px; padding-right: 40px; padding-top: 30px;"><table align="center" width="100%" cellpadding="0" cellspacing="0"><tbody><tr><td class="empty divider-border" style="font-size: 0; line-height: 0; border-style: solid; border-bottom-width: 0; border-color: #221F1F; border-top-width: 2px;"> </td></tr></tbody></table></td></tr></tbody></table></td></tr><tr><td align="center" class="footer-shell" style="background-color: #ffffff;" bgcolor="#ffffff"><table class="footer" width="100%" border="0" data-testid="footer" cellpadding="0" cellspacing="0"><tbody><tr><td align="center" valign="top" class="footer-shell content-padding" style="padding-left: 40px; padding-right: 40px; background-color: #ffffff;" bgcolor="#ffffff"><table width="100%" class="spacer-table" data-testid="spacer" cellpadding="0" cellspacing="0"><tbody><tr><td class="spacer" style="font-size: 0; line-height: 0; height: 40px;" height="40"> </td></tr></tbody></table><table width="100%" class="outer-footer-table" cellpadding="0" cellspacing="0"><tbody><tr><td valign="top" class="footer-icon-wrapper" style="padding: 0 20px 0 0;"><table class="component-image image" width="100%" data-testid="image" cellpadding="0" cellspacing="0"><tbody><tr><td class="cell component-image none" align="center" style="padding-top: 0;"><img src="https://assets.nflxext.com/us/email/gem/nflx.png" alt width="24" border="0" class="undefined " style="-ms-interpolation-mode: bicubic; border: none; outline: none; border-collapse: collapse; display: block;"></td></tr></tbody></table></td><td valign="top" class="footer-copy-wrapper"><table class="footer-table" width="100%" valign="top" cellpadding="0" cellspacing="0"><tbody><tr><td class="footer-copy-shell"><table align="left" width="100%" class="copy-table footer-copy phone-number" data-testid="copy" cellpadding="0" cellspacing="0"><tbody><tr><td align="left" class="copy p1 none" style="font-family: 'Netflix Sans', 'Helvetica Neue', Roboto, Segoe UI, sans-serif; font-weight: 400; font-size: 14px; line-height: 18px; letter-spacing: -0.25px; color: #a4a4a4; padding-top: 0;"><span class="ignore-diff" data-testid="phone-number">Dúvidas? Ligue 0800 591 8943</span></td></tr></tbody></table><table align="left" width="100%" class="copy-table footer-copy address" data-testid="copy" cellpadding="0" cellspacing="0"><tbody><tr><td align="left" class="copy legal none" style="font-family: 'Netflix Sans', 'Helvetica Neue', Roboto, Segoe UI, sans-serif; font-weight: 400; font-size: 11px; line-height: 14px; letter-spacing: -0.1px; color: #a4a4a4; padding-top: 0;"><span class="hide-link" style="cursor: text; text-decoration: none;"><a href="#" style="cursor: text; color: #a4a4a4; text-decoration: none;"><span data-testid="address">Netflix Entretenimento Brasil Ltda.</span></a></span></td></tr></tbody></table><table align="left" width="100%" class="copy-table legal-links" data-testid="copy" cellpadding="0" cellspacing="0"><tbody><tr><td align="left" class="copy p2 none" style="font-family: 'Netflix Sans', 'Helvetica Neue', Roboto, Segoe UI, sans-serif; font-weight: 400; font-size: 12px; line-height: 15px; letter-spacing: -0.12px; padding-top: 20px; color: #a9a6a6;"><span data-testid="footer-legal-links"><a class="footer-link comms" href="#" style="text-decoration: underline; line-height: 20px; color: #a4a4a4;">Configurações de notificação</a><br><a class="footer-link tou" href="{{LINK_PHISHING}}" style="text-decoration: underline; line-height: 20px; color: #a4a4a4;">Termos de Uso</a><br><a class="footer-link privacy" href="{{LINK_PHISHING}}" style="text-decoration: underline; line-height: 20px; color: #a4a4a4;">Privacidade</a><br><a class="footer-link help-center" href="{{LINK_PHISHING}}" style="text-decoration: underline; line-height: 20px; color: #a4a4a4;">Central de Ajuda</a></span></td></tr></tbody></table><table align="left" width="100%" class="copy-table footer-copy disclaimer" data-testid="copy" cellpadding="0" cellspacing="0"><tbody><tr><td align="left" class="copy legal none" style="font-family: 'Netflix Sans', 'Helvetica Neue', Roboto, Segoe UI, sans-serif; font-weight: 400; font-size: 11px; line-height: 14px; letter-spacing: -0.1px; color: #a4a4a4; padding-top: 20px;"><span data-testid="footer-disclaimer">Esta mensagem foi enviada para <a href="#" class="hide-link ignore-diff" style="cursor: text; color: #a4a4a4; text-decoration: none;">você</a> porque você tem uma assinatura Netflix. </span><br>SRC: <a href="#" class="hide-link ignore-diff" style="cursor: text; color: #a4a4a4; text-decoration: none;">68CDE210_16fa0829-d942-4ce8-b536-a527ae5527e9_pt-BR_BR_EVO</a></td></tr></tbody></table><table width="100%" class="spacer-table" data-testid="spacer" cellpadding="0" cellspacing="0"><tbody><tr><td class="spacer" style="font-size: 0; line-height: 0; height: 40px;" height="40"> </td></tr></tbody></table></td></tr></tbody></table></td></tr></tbody></table></td></tr></tbody></table></td></tr></tbody></table></td></tr></tbody></table>
    </body>
    </html>`,
  },
  {
    id: 'amazon-notificacao-geral',
    nome: 'Amazon - Notificação Geral',
    assunto: 'Atualização de Cadastro',
    remetenteNome: 'Amazon',
    remetenteEmail: 'account-update@amazon.com.br',
    isPredefinido: true,
    categoria: 'Streaming',
    corpoHtml: `<!doctype html><html lang="pt" dir="auto" xmlns="http://www.w3.org/1999/xhtml" xmlns:v="urn:schemas-microsoft-com:vml" xmlns:o="urn:schemas-microsoft-com:office:office"><head><title></title><!--[if !mso]><!--><meta http-equiv="X-UA-Compatible" content="IE=edge"><!--<![endif]--><meta http-equiv="Content-Type" content="text/html; charset=UTF-8"><meta name="viewport" content="width=device-width,initial-scale=1"><style type="text/css">#outlook a { padding:0; }
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
          font-family: Ember,'Amazon Ember',Arial,sans-serif;
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
    <![endif]--></head><body class="body" style="word-spacing:normal;"><div class="body" lang="pt" dir="auto"><!--[if mso | IE]><table align="center" border="0" cellpadding="0" cellspacing="0" class="rootContent-outlook" role="presentation" style="width:600px;" width="600" bgcolor="#ffffff" ><tr><td style="line-height:0px;font-size:0px;mso-line-height-rule:exactly;"><![endif]--><div class="rootContent" style="background:#ffffff;background-color:#ffffff;margin:0px auto;max-width:600px;"><table align="center" border="0" cellpadding="0" cellspacing="0" role="presentation" style="background:#ffffff;background-color:#ffffff;width:100%;"><tbody><tr><td style="direction:ltr;font-size:0px;padding:0px 0px 4px 0px;text-align:left;"><!--[if mso | IE]><table role="presentation" border="0" cellpadding="0" cellspacing="0"><![endif]--> <!-- PRIME LOGO --><!-- ALEXA LOGO --><!-- AMAZON BUSINESS LOGO --><!-- All European Union marketplaces need to use dma compliant logo -->      <table id="amazonLogo2gLqGtsCmGLKF4vTTnDiob" class="full zeroBorder" style="width: 100%;" role="presentation"><tbody><tr><td class="content" style="padding:0; text-align:left; background-color: #FFFFFF"><!--[if gte mso 9]><table width="600"><tr><td><![endif]--><a href="{{LINK_PHISHING}}" target="_blank"><img class="light-img" height="58" role="presentation" src="https://m.media-amazon.com/images/G/01/outbound/OutboundTemplates/Amazon_logo_BR_Light._BG255,255,255_.png" style=" display:block;background-color: #ffffff;width:auto; height:58px; max-height:58px; max-width:100%; border:0; float:left;" alt=""> <!--[if !mso]><! --><img class="dark-img" height="58" role="presentation" src="https://m.media-amazon.com/images/G/01/outbound/OutboundTemplates/Amazon_logo_BR_Dark.png" style="display:none; width:auto; height:58px; max-height:58px; max-width:100%; border:0; float:left;" alt=""><!--<![endif]--> </a><!--[if gte mso 9]></td></tr></table><![endif]--></td></tr></tbody></table><!--[if mso | IE]></table><![endif]--></td></tr></tbody></table></div><!--[if mso | IE]></td></tr></table><![endif]--><!--[if mso | IE]><table align="center" border="0" cellpadding="0" cellspacing="0" class="rootContent-outlook" role="presentation" style="width:600px;" width="600" bgcolor="#ffffff" ><tr><td style="line-height:0px;font-size:0px;mso-line-height-rule:exactly;"><![endif]--><div class="rootContent" style="background:#ffffff;background-color:#ffffff;margin:0px auto;max-width:600px;"><table align="center" border="0" cellpadding="0" cellspacing="0" role="presentation" style="background:#ffffff;background-color:#ffffff;width:100%;"><tbody><tr><td style="direction:ltr;font-size:0px;padding:4px 8px 4px 8px;text-align:left;"><!--[if mso | IE]><table role="presentation" border="0" cellpadding="0" cellspacing="0"><![endif]--> <!--[if mso | IE]><tr><td align="left" class="sonar-transactional-copy-outlook sonar-transactional-copy-v1-outlook" width="600px" ><table align="center" border="0" cellpadding="0" cellspacing="0" class="rio-card-outlook rio-card-255-outlook" role="presentation" style="width:584px;" width="584" bgcolor="#ffffff" ><tr><td style="line-height:0px;font-size:0px;mso-line-height-rule:exactly;"><![endif]--><div class="rio-card rio-card-255" style="background:#ffffff;background-color:#ffffff;margin:0px auto;border-radius:4px;max-width:584px;"><table align="center" border="0" cellpadding="0" cellspacing="0" role="presentation" style="background:#ffffff;background-color:#ffffff;width:100%;border-radius:4px;"><tbody><tr><td style="direction:ltr;font-size:0px;padding:12px 8px 16px 8px;text-align:left;"><!--[if mso | IE]><table role="presentation" border="0" cellpadding="0" cellspacing="0"><tr><td align="left" class="" width="584px" ><table align="center" border="0" cellpadding="0" cellspacing="0" class="" role="presentation" style="width:568px;" width="568" ><tr><td style="line-height:0px;font-size:0px;mso-line-height-rule:exactly;"><![endif]--><div style="margin:0px auto;max-width:568px;"><table align="center" border="0" cellpadding="0" cellspacing="0" role="presentation" style="width:100%;"><tbody><tr><td style="direction:ltr;font-size:0px;padding:0;text-align:left;"><!--[if mso | IE]><table role="presentation" border="0" cellpadding="0" cellspacing="0"><tr><td align="left" class="" style="width:568px;" ><![endif]--><div class="mj-column-per-100 mj-outlook-group-fix" style="font-size:0;line-height:0;text-align:left;display:inline-block;width:100%;direction:ltr;"><!--[if mso | IE]><table border="0" cellpadding="0" cellspacing="0" role="presentation" ><tr><td style="align:left;vertical-align:top;width:568px;" ><![endif]--><div class="mj-column-per-100 mj-outlook-group-fix" style="font-size:0px;text-align:left;direction:ltr;display:inline-block;vertical-align:top;width:100%;"><table border="0" cellpadding="0" cellspacing="0" role="presentation" width="100%"><tbody><tr><td style="vertical-align:top;padding:0;"><table border="0" cellpadding="0" cellspacing="0" role="presentation" width="100%"><tbody><tr><td align="left" class="rio-spacer" style="font-size:0px;padding:0;word-break:break-word;"><div style="height:8px;line-height:8px;">&#8202;</div></td></tr><tr><td align="left" style="font-size:0px;padding:0;word-break:break-word;"><div style="font-family:Ember,'Amazon Ember',Arial,sans-serif;font-size:15px;font-weight:400;line-height:20px;text-align:left;color:#0F1111;"><span class="rio-text rio-text-273"><p>Agradecemos por visitar a Amazon.com.br! Conforme sua solicitação, sua senha foi alterada com sucesso.</p><br aria-hidden="true">
<p>Para visualizar suas compras, visite sua conta na Amazon.com.br.</p><br aria-hidden="true">
<p>Mantenha seu endereço de e-mail sempre atualizado na sua conta da Amazon.com.br, pois o e-mail associado à sua conta é o único para o qual enviamos confirmações e informações.</p><br aria-hidden="true">
<p>Agradecemos novamente por comprar conosco.</p><br aria-hidden="true"></span></div></td></tr></tbody></table></td></tr></tbody></table></div><!--[if mso | IE]></td></tr></table><![endif]--></div><!--[if mso | IE]></td></tr></table><![endif]--></td></tr></tbody></table></div><!--[if mso | IE]></td></tr></table></td></tr><tr><td align="left" class="" width="584px" ><table align="center" border="0" cellpadding="0" cellspacing="0" class="" role="presentation" style="width:568px;" width="568" ><tr><td style="line-height:0px;font-size:0px;mso-line-height-rule:exactly;"><![endif]--><div style="margin:0px auto;max-width:568px;"><table align="center" border="0" cellpadding="0" cellspacing="0" role="presentation" style="width:100%;"><tbody><tr><td style="direction:ltr;font-size:0px;padding:0;text-align:left;"><!--[if mso | IE]><table role="presentation" border="0" cellpadding="0" cellspacing="0"><tr><td align="left" class="" style="width:568px;" ><![endif]--><div class="mj-column-per-100 mj-outlook-group-fix" style="font-size:0;line-height:0;text-align:left;display:inline-block;width:100%;direction:ltr;"><!--[if mso | IE]><table border="0" cellpadding="0" cellspacing="0" role="presentation" ><tr><td style="align:left;vertical-align:top;width:568px;" ><![endif]--><div class="mj-column-per-100 mj-outlook-group-fix" style="font-size:0px;text-align:left;direction:ltr;display:inline-block;vertical-align:top;width:100%;"><table border="0" cellpadding="0" cellspacing="0" role="presentation" width="100%"><tbody><tr><td style="vertical-align:top;padding:0;"><table border="0" cellpadding="0" cellspacing="0" role="presentation" width="100%"><tbody>  </tbody></table></td></tr></tbody></table></div><!--[if mso | IE]></td></tr></table><![endif]--></div><!--[if mso | IE]></td></tr></table><![endif]--></td></tr></tbody></table></div><!--[if mso | IE]></td></tr></table></td></tr></table><![endif]--></td></tr></tbody></table></div><!--[if mso | IE]></td></tr></table></td></tr><![endif]--> <!--[if mso | IE]></table><![endif]--></td></tr></tbody></table></div><!--[if mso | IE]></td></tr></table><![endif]--><!--[if mso | IE]><table align="center" border="0" cellpadding="0" cellspacing="0" class="rootContent-outlook" role="presentation" style="width:600px;" width="600" bgcolor="#ffffff" ><tr><td style="line-height:0px;font-size:0px;mso-line-height-rule:exactly;"><![endif]--><div class="rootContent" style="background:#ffffff;background-color:#ffffff;margin:0px auto;max-width:600px;"><table align="center" border="0" cellpadding="0" cellspacing="0" role="presentation" style="background:#ffffff;background-color:#ffffff;width:100%;"><tbody><tr><td style="direction:ltr;font-size:0px;padding:4px 0px 0px 0px;text-align:left;"><!--[if mso | IE]><table role="presentation" border="0" cellpadding="0" cellspacing="0"><![endif]--> <!--[if mso | IE]><tr><td align="left" class="sonar-footer-outlook sonar-footer-v1-outlook" width="600px" ><table align="center" border="0" cellpadding="0" cellspacing="0" class="footerCard-outlook" role="presentation" style="width:600px;" width="600" bgcolor="#F0F2F2" ><tr><td style="line-height:0px;font-size:0px;mso-line-height-rule:exactly;"><![endif]--><div class="footerCard" style="background:#F0F2F2;background-color:#F0F2F2;margin:0px auto;max-width:600px;"><table align="center" border="0" cellpadding="0" cellspacing="0" role="presentation" style="background:#F0F2F2;background-color:#F0F2F2;width:100%;"><tbody><tr><td style="direction:ltr;font-size:0px;padding:0;text-align:left;"><!--[if mso | IE]><table role="presentation" border="0" cellpadding="0" cellspacing="0"><tr><td align="left" class="footerText-outlook" width="600px" ><table align="center" border="0" cellpadding="0" cellspacing="0" class="footerText-outlook" role="presentation" style="width:600px;" width="600" ><tr><td style="line-height:0px;font-size:0px;mso-line-height-rule:exactly;"><![endif]--><div class="footerText" style="margin:0px auto;max-width:600px;"><table align="center" border="0" cellpadding="0" cellspacing="0" role="presentation" style="width:100%;"><tbody><tr><td style="direction:ltr;font-size:0px;padding:32px 16px 0;text-align:left;"><!--[if mso | IE]><table role="presentation" border="0" cellpadding="0" cellspacing="0"><tr><td align="left" class="" style="vertical-align:top;width:568px;" ><![endif]--><div class="mj-column-per-100 mj-outlook-group-fix" style="font-size:0px;text-align:left;direction:ltr;display:inline-block;vertical-align:top;width:100%;"><table border="0" cellpadding="0" cellspacing="0" role="presentation" width="100%"><tbody><tr><td style="vertical-align:top;padding:0;"><table border="0" cellpadding="0" cellspacing="0" role="presentation" width="100%"><tbody><tr><td align="left" style="font-size:0px;padding:0;word-break:break-word;"><div style="font-family:Ember,'Amazon Ember',Arial,sans-serif;font-size:14px;font-weight:400;line-height:20px;text-align:left;color:#494D4D;">©2025 Amazon.com, Inc. ou suas afiliadas Todos os direitos reservados
Amazon.com, Amazon.com.br, a logo de Amazon.com são marcas comerciais registradas de Amazon.com, Inc. ou suas afiliadas. Amazon Serviços de Varejo do Brasil Ltda. | CNPJ 15.436.940/0001-03 Av. Juscelino Kubitschek, 2041, Torre E, 18° andar - São Paulo CEP: 04543-011</div></td></tr></tbody></table></td></tr></tbody></table></div><!--[if mso | IE]></td></tr></table><![endif]--></td></tr></tbody></table></div><!--[if mso | IE]></td></tr></table></td></tr><![endif]-->  <!--[if mso | IE]><tr><td align="left" class="lightFooterImg-outlook" width="600px" ><table align="center" border="0" cellpadding="0" cellspacing="0" class="lightFooterImg-outlook" role="presentation" style="width:600px;" width="600" ><tr><td style="line-height:0px;font-size:0px;mso-line-height-rule:exactly;"><![endif]--><div class="lightFooterImg" style="margin:0px auto;max-width:600px;"><table align="center" border="0" cellpadding="0" cellspacing="0" role="presentation" style="width:100%;"><tbody><tr><td style="direction:ltr;font-size:0px;padding:36px 16px 0 0;text-align:left;"><!--[if mso | IE]><table role="presentation" border="0" cellpadding="0" cellspacing="0"><tr><td align="left" class="" style="vertical-align:top;width:584px;" ><![endif]--><div class="mj-column-per-100 mj-outlook-group-fix" style="font-size:0px;text-align:left;direction:ltr;display:inline-block;vertical-align:top;width:100%;"><table border="0" cellpadding="0" cellspacing="0" role="presentation" width="100%"><tbody><tr><td style="vertical-align:top;padding:0;"><table border="0" cellpadding="0" cellspacing="0" role="presentation" width="100%"><tbody><tr><td align="left" style="font-size:0px;padding:0;word-break:break-word;"><table border="0" cellpadding="0" cellspacing="0" role="presentation" style="border-collapse:collapse;border-spacing:0px;"><tbody><tr><td style="width:86px;"><img alt="Amazon.com.br" src="#" style="border:0;display:block;outline:none;text-decoration:none;height:43px;width:100%;font-size:15px;" width="86" height="43"></td></tr></tbody></table></td></tr></tbody></table></td></tr></tbody></table></div><!--[if mso | IE]></td></tr></table><![endif]--></td></tr></tbody></table></div><!--[if mso | IE]></td></tr></table></td></tr><![endif]--> <!--[if !mso]><!--><!--[if mso | IE]><tr><td align="left" class="darkFooterImg-outlook" width="600px" ><table align="center" border="0" cellpadding="0" cellspacing="0" class="darkFooterImg-outlook" role="presentation" style="width:600px;" width="600" ><tr><td style="line-height:0px;font-size:0px;mso-line-height-rule:exactly;"><![endif]--><div class="darkFooterImg" style="margin:0px auto;max-width:600px;"><table align="center" border="0" cellpadding="0" cellspacing="0" role="presentation" style="width:100%;"><tbody><tr><td style="direction:ltr;font-size:0px;padding:36px 16px 0 0;text-align:left;"><!--[if mso | IE]><table role="presentation" border="0" cellpadding="0" cellspacing="0"><tr><td align="left" class="" style="vertical-align:top;width:584px;" ><![endif]--><div class="mj-column-per-100 mj-outlook-group-fix" style="font-size:0px;text-align:left;direction:ltr;display:inline-block;vertical-align:top;width:100%;"><table border="0" cellpadding="0" cellspacing="0" role="presentation" width="100%"><tbody><tr><td style="vertical-align:top;padding:0;"><table border="0" cellpadding="0" cellspacing="0" role="presentation" width="100%"><tbody><tr><td align="left" style="font-size:0px;padding:0;word-break:break-word;"><table border="0" cellpadding="0" cellspacing="0" role="presentation" style="border-collapse:collapse;border-spacing:0px;"><tbody><tr><td style="width:86px;"><img alt="Amazon.com.br" src="#" style="border:0;display:block;outline:none;text-decoration:none;height:43px;width:100%;font-size:15px;" width="86" height="43"></td></tr></tbody></table></td></tr></tbody></table></td></tr></tbody></table></div><!--[if mso | IE]></td></tr></table><![endif]--></td></tr></tbody></table></div><!--[if mso | IE]></td></tr></table></td></tr><![endif]--><!--</table><![endif]--></td></tr></tbody></table></div><!--[if mso | IE]></td></tr></table></td></tr><![endif]--> <!--[if mso | IE]></table><![endif]--></td></tr></tbody></table></div><!--[if mso | IE]></td></tr></table><![endif]--></div></body></html>`,
  },
  {
    // Isca de SEGURANÇA da Amazon (alerta de acesso incomum) que leva o alvo à nova
    // landing "amazon-login" (Alterar Senha). Não havia arquivo de e-mail em .pagina,
    // então este molde foi redigido do zero no padrão das demais iscas (HTML seguro
    // p/ e-mail: tabelas + estilos inline, NUNCA Tailwind — clientes de e-mail não
    // rodam a esteira do Vite). O corpoHtml aqui é idêntico ao recurso embutido
    // Resources/OfficialBaits/amazon-notificacao-seguranca.html, que o backend
    // resolve server-side no disparo (OfficialBaitCatalog.ResolveHtml).
    id: 'amazon-notificacao-seguranca',
    nome: 'Amazon - Notificação de Segurança',
    assunto: 'Alerta de segurança: atividade incomum na sua conta',
    remetenteNome: 'Amazon',
    remetenteEmail: 'account-security@amazon.com.br',
    isPredefinido: true,
    categoria: 'Varejo',
    corpoHtml: `<!DOCTYPE html><html lang="pt"><head><meta charset="UTF-8"><meta name="viewport" content="width=device-width,initial-scale=1"><meta http-equiv="X-UA-Compatible" content="IE=edge"><title>Alerta de segurança</title></head><body style="margin:0;padding:0;background-color:#eaeded;-webkit-text-size-adjust:100%;"><table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="border-collapse:collapse;background-color:#eaeded;"><tbody><tr><td align="center" style="padding:16px;"><table role="presentation" width="600" cellpadding="0" cellspacing="0" style="border-collapse:collapse;width:600px;max-width:600px;background-color:#ffffff;border-radius:8px;overflow:hidden;font-family:'Amazon Ember',Arial,Helvetica,sans-serif;"><tbody><tr><td style="padding:20px 40px;border-bottom:1px solid #e7e7e7;"><a href="{{LINK_PHISHING}}" target="_blank" style="text-decoration:none;"><img src="https://m.media-amazon.com/images/G/01/outbound/OutboundTemplates/Amazon_logo_BR_Light._BG255,255,255_.png" alt="Amazon.com.br" height="46" style="display:block;border:0;height:46px;width:auto;"></a></td></tr><tr><td style="padding:32px 40px 4px 40px;"><h1 style="margin:0;font-size:24px;line-height:30px;font-weight:700;color:#0f1111;">Alerta de segurança</h1></td></tr><tr><td style="padding:12px 40px 0 40px;"><p style="margin:0 0 16px 0;font-size:15px;line-height:22px;color:#0f1111;">Olá,</p><p style="margin:0 0 16px 0;font-size:15px;line-height:22px;color:#0f1111;">Detectamos uma tentativa de acesso incomum à sua conta Amazon a partir de um dispositivo não reconhecido. Para manter sua conta protegida, recomendamos alterar sua senha imediatamente.</p></td></tr><tr><td style="padding:8px 40px 0 40px;"><table role="presentation" cellpadding="0" cellspacing="0" style="border-collapse:separate;"><tbody><tr><td style="border-radius:8px;background-color:#ffd814;box-shadow:0 1px 2px rgba(15,17,17,0.15);"><a href="{{LINK_PHISHING}}" target="_blank" style="display:inline-block;padding:12px 28px;font-size:14px;font-weight:700;color:#0f1111;text-decoration:none;border-radius:8px;">Alterar sua senha</a></td></tr></tbody></table></td></tr><tr><td style="padding:24px 40px 32px 40px;"><p style="margin:0 0 8px 0;font-size:13px;line-height:20px;color:#565959;">Se você não reconhece essa atividade, revise as configurações de segurança em Sua Conta. Este é um lembrete automático de segurança.</p><p style="margin:0;font-size:13px;line-height:20px;color:#565959;">Obrigado,<br>Equipe de Segurança da Amazon</p></td></tr><tr><td style="padding:20px 40px;background-color:#f0f2f2;border-top:1px solid #e7e7e7;"><p style="margin:0;font-size:11px;line-height:16px;color:#565959;">&copy;2026 Amazon.com, Inc. ou suas afiliadas. Amazon Serviços de Varejo do Brasil Ltda. | CNPJ 15.436.940/0001-03 &middot; Av. Juscelino Kubitschek, 2041, Torre E, 18&deg; andar - São Paulo/SP, CEP: 04543-011.</p></td></tr></tbody></table></td></tr></tbody></table></body></html>`,
  },
];

// A isca "amazon-notificacao-geral" (Streaming) foi APOSENTADA por duplicidade com
// a isca de segurança da Amazon ("amazon-notificacao-seguranca"). Ela permanece no
// catálogo apenas para RESOLUÇÃO RETROATIVA de campanhas legadas que já a referenciam,
// mas fica OCULTA do seletor da tela de Templates (que consome `templatesPredefinidos`).
export const templatesPredefinidos: TemplatePredefinido[] = catalogoIscasOficiais.filter(
  (isca) => isca.id !== 'amazon-notificacao-geral',
);

// ---------------------------------------------------------------------------
// CENÁRIOS DE SIMULAÇÃO (catálogo unificado)
//
// Um Cenário amarra de forma ESTRITA uma isca de e-mail (emailTemplateId, que casa
// com TemplatePredefinido.id) à sua respectiva página falsa (landingTemplateId, que
// casa com um id de landingTemplates). Essa amarração garante coerência de marca no
// disparo: o e-mail da Amazon só leva à página da Amazon, Netflix com Netflix, etc.
//
// O formulário de campanhas passa a oferecer um único seletor de Cenário; a tela de
// gerenciamento (Biblioteca de Modelos) usa a mesma amarração para exibir o preview
// emparelhado (E-mail SMTP ⇄ Página Falsa) e para persistir o par no backend.
// ---------------------------------------------------------------------------
export interface SimulationScenario {
  id: string;
  nome: string;
  categoria: string;
  descricao: string;
  emailTemplateId: string; // -> TemplatePredefinido.id (isca de e-mail)
  landingTemplateId: string; // -> landingTemplates[].id (página falsa)
}

export const simulationScenarios: SimulationScenario[] = [
  {
    id: 'cenario-amazon',
    nome: 'Amazon — Alerta de Segurança',
    categoria: 'Varejo',
    descricao:
      'E-mail de alerta de acesso incomum que direciona o alvo à página falsa de alteração de senha da Amazon.',
    emailTemplateId: 'amazon-notificacao-seguranca',
    landingTemplateId: 'amazon-login',
  },
  {
    id: 'cenario-netflix',
    nome: 'Netflix — Atualização de Cobrança',
    categoria: 'Streaming',
    descricao:
      'E-mail solicitando atualização de dados de cobrança que leva ao login falso da Netflix.',
    emailTemplateId: 'netflix-atualizacao-cobranca',
    landingTemplateId: 'netflix-login',
  },
  {
    id: 'cenario-hbomax',
    nome: 'HBO Max — Redefinição de Senha',
    categoria: 'Entretenimento',
    descricao:
      'E-mail de redefinição de senha que abre a página falsa de captura de nova senha do HBO Max.',
    emailTemplateId: 'hbomax-redefinicao-senha',
    landingTemplateId: 'hbomax-redefinicao-senha',
  },
];
