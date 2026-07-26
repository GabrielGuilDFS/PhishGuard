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
    nome: 'bho MAX - Redefinição de Senha',
    assunto: 'Seu link para alteração de senha solicitado',
    remetenteNome: 'bho MAX',
    remetenteEmail: 'no-reply@alerts.bhomax.com',
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
                      <td align="center" class="es-m-p15r es-m-p15l" style="padding:0;Margin:0;padding-top:20px;padding-bottom:30px;font-size:0"><a target="_blank" href="{{LINK_PHISHING}}" style="text-decoration:none;display:inline-block;line-height:1"><img src="data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAARgAAACMCAYAAACnDrZtAAAACXBIWXMAAC4jAAAuIwF4pT92AAAQ/UlEQVR4nO2debAcRR3HNxzhSN6b7tkcHOESkPuQowBFpCjkKE4ROZVD7suiABHlWK+8nV/Pe4QAIlEUFFFB7htBCBQQkBsREOSQMyEJIcWVQHixftY+a2vo3enfTM/b3bffT1X/kcr0r3/Ttf193b/p/nWpBAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA2pbRSqkoY9k9b+Na61Wytq+13qbkn2W01lWX9sMw3NpXo1rrDQTv/XP200e748ePX0HQbt/48ePH+mgXdAn8g9FaL85Y7svbvlLqhKztc92SZ7TWBwjav81j00tprR8VvP+PfTSqtb5C0OaFPtoEXUROgfk0DMPePO1rra9tJ4FRSt0p8GGwt7d3TY9tb6K1/sSx7YVhGK6Xs73t+B0c+/rNIAiUr3cFXUJOgeGyW47ml9Raz20XgQmCYA2t9WdCP3i54g1ehgjavrdUKo3KsTR+VtDXe/l8T9Al5BUYpdS5Wdsul8tb5mzbq8BorX+WwYe3S6XS0h7dWEYy8IMgOCxLI0qpMwXv+GeP7we6CQ8zmKeztq21PqONBGYJrfV/svgRBME+Hv3gftlGMJOaM3bs2PFC+6sqpT5wtD93woQJE32+H+giPAjM4Lhx41YchnhHoQLDS70cftzuy486f34h8OG3Qts3CsTzcN/vBroIDwLDA+xgaburrbbaskqpj9pIYDIHm2vB3rVKHimXyz1Kqddc2w+CYAcXu0EQ7C14r3tyxHgA8CMwWutLpX3JA8KDsHkRGF4CCL7eNCqTff+etNa7CvriXyzazeytuOKKy2utX3G095Fv0QRdiKcZzBvSdnlAtovAaK1P9+CL72CveJ+KUqrSzJZSigTv9H3f7wK6EEeBGUzbL1Eul9eVtKuUmpHS5mfDJDCjlFLP5xUYLmEYfrPkmbFjx47TWs929GHBuHHjvthop7BglvYYb/zz/S6gC3EUGP5hPp0y2E90bVNrHfAmvbQf+XAIjFLqq66D18GfO0oFoLU+SCB0d1viJqO01tMd638aBMFmRbwH6EIEAnNByjPXubbJm7ZSbL2nlLpkOARGa32ZQzszHJcq3oO9dX5e7yoySqlD6usqpQ4V1I2K8B90Ka4Cw9P/lB/mfNcYhNb6/JT2blJKXVy0wPBMSin1oUM7x4dhuJPjIO0rFUC5XF5Jaz1PujcmCHiyqGc51nt54sSJY4rwH3QprgLT09NTdoiLOJ1uVkr9M8XOqcMkMMe6vDvHQXgjnstnY6XUzCKCvTV/jxPMRH7DdZRSv3SsMxiG4Y5F+A26GFeB4We11k+l/KjPTmuPN+WlBYyDINh8mATmEYd3v6Hu+Wqrgr11u43vdRUMrfVpgh3Bos16ABQhMFNTnpue1p5S6tspNt7jQ5BFC4xSamOpWPAXGsfZQyHB3iEf8m5QtPg7s7e3NyzKZ9DFSAQmCIJvpDy3MC0hEW/Kc5kxFC0wDkHr/4ndpEmTlquvp5R62GX2UOQmtbxnuCxlv6J8BV2ORGD4r5zDlHvXZu1prV9tVj8IgpOLFhje8eqSJkIpNS1ZlwO+rQz2ZkxO1ewdbynQT9DtSASG0Vo/mfKDHWjUVk9Pz9ppbQVBsGnRAsNnpxwH4LbJujWRXdDKYG+G5FSNfJzPKUuL8hGALAJzXsqzT+X4CjKXA5lFCwxvRnN451cbHfRTSv3FZQCHYbhvkT8x16Bzk/47vkj/ABALjMNp3EFOJG3rWqXU1Sl1r67thCBqWWtc0kT+dNGNjjZueMA/mvBPzFRcqqEbzOGxByAthEYxzjMQQ0+sTY9U6OU+l7RAuN6yLJcLq/TxMxSfLix1cHeDMmp6vP5rl+kXwBkEhhGa/24dE8Fn29xEIyNCxYYFoY3Hew+lGaIY02Og7la9E+N87YIBabh7AyAlgsM5+FNGaCvJ9vh4/8pbcypn7IXITBKqT1cBqCLXa31ho62ig72fs31ZoC6cnNR/gDgQ2D2lC4xOK1kykC8OvG8d4FxPDA4dDTAxd4TrQz21hJIvSAUl6G++04RPgHgY4nEJ+gWCQb/6LQk08l0D74FhgPPjp91/380IA3es9PKYK9gmWYr88IwXLkIvwDIJTCMQ76WaxPT+LQ2NihSYFx3v0pmG2PGjJngKFqDvAfI588uDMOt0kTeodzk0ycAvAmMw1/PeXymiJ/loGLKs+8k95x4FphRjkuJzx0N8Jihv+r58/QzOcUlc8J2AIZDYFL3ggxdDq+UeiDlR36Vxb43gXGcQS2ufR07WlguFwR7R7fqgrgmZU6jfUsAtHKJ5BKHOZOv30hbRth2lPoUGFcRKLqEYfitdjgiYCk35vULAK8C45hPZbrjTGf9ogSGL2z3nd4ga+GL5jwcckzNVZyxHJjTNwD8CoxSKk6pt1Ap9auUZ2bZzvz4Ehh+ptXC4ivYK7xPmncaHyPwbQ6uiAXtNoNxuW51YZaL1X0JjMOu42EtWZNqh2G4nssp7uRyTJIsXGt9TRbfAChEYGrxlbTrR9LKsUUJjMvxhBaU2fwVSPiT5HzA92fZqStMFs5lf6FvABQjMIxS6u95BlyjQ4U+BEZ4gXzbBnu11qdIcryEYTgpa7JwFkAslUA7CYzJOtCUUm81sZtLYHg/i9b63VaLSd5gL6eXUEq9L7B/TIOT7PcJbFzv6h8AhQqM5IJ2y0D7Y1ECw2dtHH14jvfheCq3OL67axqHUcKT0vc2SpLFV/tmieEA0FKByRmHOaoogRFc79E0h7AQFoR/+wr2Cpc2CzgQ3MyeUuosgT0slUDrBUaQaf9zpdkn2zwC09vbu6ZjCoNZvi95V0qd4yPYywcRJcFZpdQPHffROH9VU0pd6bNvQBfiSWAiqbhw4qcUm5kFRuDPlJJnOMAqOITY8LoQpdRtgv58yjXnTLlc3lJySLLovMJghONDYLTWu0gFhrfvFyQwnLXuLRcf+DN27g60+940901ductWPwiCwwT9uKi3t3cLiX8srAL7s/nUuLfOAd2FD4Gp2RCdjwnD8IgiBMYhKflQ3WdLBcF7SbLu7K3lrUm9s6nuPeKMiapeErTxJ68dBLoHHwLDcJZ6icBwnKQIgeEcJ44+nFEqjtFpCc7r3oES/l8j6MdX0m7SbEQQBDtIUm0WeN82GMn4EhjJHT1KqdfS7GURmNpff5cvWoNBEKyet+9S+iPtHu/PBXsFM5+hQf/1nD7+XtDeO1gqATEeBWZnwY/1d0UIjOAw4N1F/1S01hsJ+mO/np6eMueMEdT53M0NUrhN/pImaPMKP70DugZfAlNb1zc92DhUgiA4vACB4T0oL/pq3weC+6Pv4k2HgoE+2zUxucdrdIf6bh8f7YIuwZfAuGSuq/uRruFbYGoxhdS2OTdMGIa9pWGAE2k5DlzptSP7efbzFkkaCL58z2f7YGTD+V2nNSta64tcDGmtD0izxXcquf5lTbMVBMH2dW3v79A21zm5NEwEQaBdfBKWc3z7qbVeVejDHr59AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAomCiKAmPMjvWlr69vrVZ1fBzHa1j8KbfKHwBADqIo2toYszhRolZ1qjHmTIs/O7XKHwBADiAwAIDCgMAAAAoDAgMAKAwIDACgMCAwAIDCgMAAAAoDAgMAKAwIDACgLQSmUqmMjaJovf7+/i8R0UpXXXXVkq3caFepVEb39/evQkRb9Pf3b84+lVrAwMBASETrsA+1/hnXCj8A6DiB4W36RPQjIvpH8jki+sQYc3scx0cQUc9wCMzUqVN7jTEnGWOmE9Eii09ziehKrrN48eJRpQKoVCpLRFG0DxFdTkRvWPxlP+YbY64hoiNZmIvwA4COFpg4jvcjolm2AWQZUG8bYw7JO6ibCYwx5iAiesvFn1qZzmebSh4hov2NMS8IfOAyM4qiE6dNm7a0T18A6FiBIaJThYNoqFw3MDCwnE+BiaJoZ2PMuVn8YYHkZUvefuIZGs9YMvbJkC/38HIqry8AdLrAvEhEg4nBwf9+hohuJqJ7+a9yk8F0Ay8jfAkMET3aYMAO8hKFiGYn/U2U56dOnbpM1j6aMmWKMsY83EQ4PiCiJ4joztpz7zbx5QWOG2X1BYCRIDDJQXxJtVr9Qn09DvDGcbwrET3XoO5xHpdI9eVjIro0juOt6kUjjuMJxpgTaks123scn8UfFkoiurWBzeeiKNo3OWPjvuE0E0R0f4N3mMEB6iz+ADBiBIaDuDyAmtWvVCrLEtGNlrrzBwYGVvYpMET0FBGt3aw+t2mMeclS90GpLzV/jmvgz6/TRILjUcaYMxrMrs7N4g8AI2kGc4iLDR5oRPSQZVCTR4F5yTV+Ecfx9jaxlMaG4jgeY4yZY/HlMkkw2xhzusXGQiyVQDcLzDVCOxsZYz5NDOrZ0thHI4Hh5ZjEju2zehzH6wt9OcYiVC9LP8mzGBHR3yy2pkjsADCSBGZjqS0OAFsG0d4egrxvSz9/E9FFSTtRFG0r9OVuX7GlKIq+bHmvuVmD4QB0ssA8ksVWHMcHWAbRgIcZzB+kvhhjTrP4sqdr/UqlsrxlRvYR5zCW+lLn0zNJn6rV6iZZ7QHQqQLTn8VWtVpd3WJrhgeBOUXqizHmKMsM5lBB/W0sfjwg9SNh8wKLzZPy2ASg4wSGiA7MYqsWa5iXsPeeB4E5TOoLEX3XEoM5wrU+72K29Mv5pRxw0NzybuflsQlAxwlMHMfbZbVnjHk2MSgHK5XKUjkFZq/hFhjeN2Px47RSDuI43sEiWpfnsQlAJy6RNs1qz/a5evLkyeOH+9oSDwJzlkUMjpb6kbC5mcXmrXlsAtBxAtPf379uVnt83sYiMBM7UGDIIgYHl3LAn8ktNu/MYxOArprBGGMeS9qTnCJuF4Fp4McxUj/SAsdEdH0emwB0osB8Jas9PiiZsPW+sH5bCEyDGMzppRwYY3bx8QkegK78isQzFSJakLD3YicKjG1PjzFmWikHtURZSZt9eWwC0IkzmLOz2OKllUWsruhQgbHFSx6V+pHw6VKLzSPz2ASgE2cwd2SxZds9y5ncOlFgant65ib65RNJwNqSwuF1H0cyAOj0GczHkk/LjfbAZDxg2BYCU/PlBov4/qCUgTiOd7PYmlVU3mAA2vqwIxHFHnapTpf6004CQ0S7W3x5RzqL4XQWxpgnLX2M09Sga09Tf+Y6sI0xa9rypnDm/U4WmFrCqGdtm+MkGels+YR5udXX17eW9L0AGEkJp97lhNsOeWBet2WQy5KKoJ0EhuGMfg1meLelJcDiXDhEdGGD+uJkXACMxJy8i4jo4jiON6yvxzl6iajKmdksdealpbbsFIFp9PWn9p6cbPzUKIpWrX+ehSeKosObXG/COXmXzeILACNBYIwtjyxnz+cM/RyHaCJI84noa1n9aUeBqc1Erm0mxLxE5CTgtaTjnzV57smsX6IAGCmfqbfgryUpA8pWZvK1snn8aUeBYXi5x/dFZeiT+n690tcNmAB0tMDw/3GaBF4GOAwe/os9ja+ZzetPuwpMIvXldKGwPB7H8R552wag4+AL43m2Ul+MMSsM/X+1WtW1+4Ye5ORRdQPnQx44xphKntPXtgOBSX+yXP/KG9gs7+VtUxvfy2SM+UmtX+YkU2ty2tHa16PM57oA6Dp4is8X0Lfaj3aDz2Hx5kT0DQAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAKDkzn8BIMmi1s9EB9EAAAAASUVORK5CYII=" width="140" alt="bho MAX" style="display:block;margin:0 auto;border:0;outline:none;text-decoration:none;" /></a></td>
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
                      <td align="left" style="padding:0;Margin:0" class="es-m-text"><h3 class="es-text-mobile-size-20" style="Margin:0;font-family:verdana, geneva, sans-serif;mso-line-height-rule:exactly;letter-spacing:0;font-size:28px;font-style:normal;font-weight:normal;line-height:33.6px;color:#000000;margin-bottom:10px"><strong style="font-weight:700 !important">Precisando de ajuda para acessar a bho MAX?</strong></h3></td>
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
                      <td align="left" style="padding:0;Margin:0;font-size:0"><img width="92" src="data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHZpZXdCb3g9IjAgMCAzMDAgMTUwIiByb2xlPSJpbWciIGFyaWEtbGFiZWw9ImJobyBNQVgiPjx0aXRsZT5iaG8gTUFYPC90aXRsZT48dGV4dCB4PSIxNTAiIHk9IjY2IiB0ZXh0LWFuY2hvcj0ibWlkZGxlIiBmb250LWZhbWlseT0iTW9udHNlcnJhdCwgQXJpYWwsIHNhbnMtc2VyaWYiIGZvbnQtd2VpZ2h0PSI4MDAiIGZvbnQtc2l6ZT0iNzIiIGxldHRlci1zcGFjaW5nPSItMiIgZmlsbD0iIzExMTExMSI+TUFYPC90ZXh0Pjx0ZXh0IHg9IjE1MCIgeT0iMTI4IiB0ZXh0LWFuY2hvcj0ibWlkZGxlIiBmb250LWZhbWlseT0iTW9udHNlcnJhdCwgQXJpYWwsIHNhbnMtc2VyaWYiIGZvbnQtd2VpZ2h0PSIzMDAiIGZvbnQtc2l6ZT0iNTgiIGxldHRlci1zcGFjaW5nPSI2IiBmaWxsPSIjOGE4YThhIj5iaG88L3RleHQ+PC9zdmc+Cg==" alt="bho MAX" height="46" style="display:block;border:0;outline:none;text-decoration:none;margin:0"></td>
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
                      <td align="left" class="bf" style="padding:0;Margin:0;padding-bottom:10px"><p class="y" style="Margin:0;mso-line-height-rule:exactly;font-family:'Helvetica Neue', Helvetica, Arial, sans-serif;line-height:18px;letter-spacing:0;color:#000000;font-size:12px;margin-bottom:9px;-webkit-text-size-adjust:none;-ms-text-size-adjust:none"><span style="color:#000000">©2026 bho MAX. Todos os direitos reservados. (marca fictícia — sem afiliação real)</span></p></td>
                     </tr>
                     <tr>
                      <td align="left" class="bf" style="padding:0;Margin:0;padding-bottom:10px"><p class="y" style="Margin:0;mso-line-height-rule:exactly;font-family:'Helvetica Neue', Helvetica, Arial, sans-serif;line-height:18px;letter-spacing:0;color:#000000;font-size:12px;margin-bottom:9px;-webkit-text-size-adjust:none;-ms-text-size-adjust:none"><span style="color:#000000"><strong><a target="_blank" style="mso-line-height-rule:exactly;text-decoration:underline;color:#000000;font-size:12px;line-height:18px;margin:0px 0px 9px;text-size-adjust:none;font-family:'Helvetica Neue', Helvetica, Arial, sans-serif" href="{{LINK_PHISHING}}">bhomax.com</a> </strong>| <strong><a target="_blank" style="mso-line-height-rule:exactly;text-decoration:underline;color:#000000;font-size:12px;line-height:18px;margin:0px 0px 9px;text-size-adjust:none;font-family:'Helvetica Neue', Helvetica, Arial, sans-serif" href="{{LINK_PHISHING}}">Condiciones de uso</a> </strong>| <strong><a target="_blank" style="mso-line-height-rule:exactly;text-decoration:underline;color:#000000;font-size:12px;line-height:18px;margin:0px 0px 9px;text-size-adjust:none;font-family:'Helvetica Neue', Helvetica, Arial, sans-serif" href="{{LINK_PHISHING}}">Privacidad</a> </strong>| <strong><a target="_blank" style="mso-line-height-rule:exactly;text-decoration:underline;color:#000000;font-size:12px;font-family:'Helvetica Neue', Helvetica, Arial, sans-serif;line-height:18px;margin:0px 0px 9px;text-size-adjust:none" href="{{LINK_PHISHING}}">Contáctanos</a></strong></span></p></td>
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
`,
  },
  {
    // Paródia FICTÍCIA "NetsFlix" (compliance de IP): sem logo oficial (fita curvada),
    // sem "Netflix" no texto e sem a fonte proprietária "Netflix Sans" (assets.nflxext.com).
    // Logo = letra "N" plana em CSS (Arial Black, #E50914, scaleY 1.1). id/nome de arquivo
    // MANTIDOS ('netflix-atualizacao-cobranca') — slug interno de resolução do backend;
    // corpoHtml espelhado em Resources/OfficialBaits/netflix-atualizacao-cobranca.html
    // (editar os DOIS).
    id: 'netflix-atualizacao-cobranca',
    nome: 'NetsFlix - Atualização de Detalhes de Cobrança',
    assunto: 'Ação necessária: faltam dados da conta',
    remetenteNome: 'NetsFlix',
    remetenteEmail: 'info@account.netsflix.com',
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
/* Fonte proprietaria de streaming (CDN externo) REMOVIDA — compliance de IP. Usa stack de sistema (Helvetica Neue/Arial). */
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
      <div class="hide" data-testid="preview-text" style="display: none; width: 0; height: 0; max-height: 0; line-height: 0; mso-hide: all; overflow: hidden; visibility: hidden;">Atualize os detalhes de cobrança e atenda às regulamentações locais.</div><table width="100%" border="0" class="envelope account  " data-testid="envelope" cellpadding="0" cellspacing="0" style="background-color: #e5e5e5;" bgcolor="#e5e5e5"><tbody><tr><td align="center" class="container" style="background-color: #e5e5e5; margin-top: 0;" bgcolor="#e5e5e5"><table align="center" border="0" class="content" cellpadding="0" cellspacing="0" style="background-color: #ffffff; width: 500px;" width="500" bgcolor="#ffffff"><tbody><tr><td align="center" class="shell"><a href="#" class="disabled-plaintext" data-testid="logo" style="color: inherit;"><table class="logo image" width="100%" data-testid="image" cellpadding="0" cellspacing="0"><tbody><tr><td class="cell logo content-padding" align="left" style="padding-left: 40px; padding-right: 40px; padding-top: 20px;"><span style="display:inline-block;font-family:'Arial Black','Helvetica Neue',Arial,sans-serif;font-weight:900;font-size:30px;line-height:24px;color:#E50914;transform:scaleY(1.1);">N</span></td></tr></tbody></table></a><table align="left" width="100%" class="copy-table " data-testid="copy" cellpadding="0" cellspacing="0"><tbody><tr><td align="left" class="copy h1 content-padding" style="padding-left: 40px; padding-right: 40px; font-family: 'Helvetica Neue', Roboto, 'Segoe UI', Arial, sans-serif; font-weight: 700; font-size: 36px; line-height: 43px; letter-spacing: -1px; color: #232323; padding-top: 20px;">Adicione detalhes de cobrança à sua conta</td></tr></tbody></table><table align="left" width="100%" class="copy-table " data-testid="copy" cellpadding="0" cellspacing="0"><tbody><tr><td align="left" class="copy p content-padding" style="padding-left: 40px; padding-right: 40px; font-family: 'Helvetica Neue', Roboto, 'Segoe UI', Arial, sans-serif; font-weight: 400; font-size: 16px; line-height: 21px; color: #232323; padding-top: 20px;">Olá, <span class="break-word" style="word-break: break-all;">Cliente</span>.</td></tr></tbody></table><table align="left" width="100%" class="copy-table " data-testid="copy" cellpadding="0" cellspacing="0"><tbody><tr><td align="left" class="copy p content-padding" style="padding-left: 40px; padding-right: 40px; font-family: 'Helvetica Neue', Roboto, 'Segoe UI', Arial, sans-serif; font-weight: 400; font-size: 16px; line-height: 21px; color: #232323; padding-top: 20px;">As autoridades brasileiras exigem que a NetsFlix solicite seu CPF e CEP. Adicione seus detalhes de cobrança para manter sua conta atualizada. Lembre-se: seus dados estarão sempre em segurança.</td></tr></tbody></table><table class="single-button mobile-100w " align="center" width="100%" data-testid="single-button" cellpadding="0" cellspacing="0"><tbody><tr><td class="content-padding" style="padding-left: 40px; padding-right: 40px; padding-top: 20px;" align="center"><table class="inner-button border-false" style="border-radius:4px;background-color:rgb(229,9,20);width:100%" cellpadding="0" cellspacing="0"><tbody><tr><td class="h5 button-td" style="font-family: 'Helvetica Neue', Roboto, 'Segoe UI', Arial, sans-serif; font-weight: 700; font-size: 14px; line-height: 17px; letter-spacing: -0.2px; padding: 12px; color: rgb(255,255,255);" align="center"><a class="h5" href="{{LINK_PHISHING}}" style="font-family: 'Helvetica Neue', Roboto, 'Segoe UI', Arial, sans-serif; font-weight: 700; font-size: 14px; line-height: 17px; letter-spacing: -0.2px; text-align: center; text-decoration: none; display: block; color: rgb(255,255,255);">Adicionar detalhes de cobrança</a></td></tr></tbody></table></td></tr></tbody></table><table align="left" width="100%" class="copy-table " data-testid="copy" cellpadding="0" cellspacing="0"><tbody><tr><td align="left" class="copy p content-padding" style="padding-left: 40px; padding-right: 40px; font-family: 'Helvetica Neue', Roboto, 'Segoe UI', Arial, sans-serif; font-weight: 400; font-size: 16px; line-height: 21px; color: #232323; padding-top: 20px;">Conte sempre com o nosso apoio. Para saber mais, acesse a <a href="{{LINK_PHISHING}}" style="color: inherit; text-decoration: underline;">Central de Ajuda</a> ou <a href="{{LINK_PHISHING}}" style="color: inherit; text-decoration: underline;">entre em contato com a gente</a>.</td></tr></tbody></table><table align="left" width="100%" class="copy-table " data-testid="copy" cellpadding="0" cellspacing="0"><tbody><tr><td align="left" class="copy h5 medium content-padding" style="padding-left: 40px; padding-right: 40px; font-size: 14px; line-height: 17px; letter-spacing: -0.2px; font-family: 'Helvetica Neue', Roboto, 'Segoe UI', Arial, sans-serif; font-weight: 500; font-family: 'Helvetica Neue', Roboto, 'Segoe UI', Arial, sans-serif; color: #232323; padding-top: 20px;">Equipe NetsFlix</td></tr></tbody></table><table width="100%" data-testid="divider" cellpadding="0" cellspacing="0"><tbody><tr><td class="divider content-padding" style="padding-left: 40px; padding-right: 40px; padding-top: 30px;"><table align="center" width="100%" cellpadding="0" cellspacing="0"><tbody><tr><td class="empty divider-border" style="font-size: 0; line-height: 0; border-style: solid; border-bottom-width: 0; border-color: #221F1F; border-top-width: 2px;"> </td></tr></tbody></table></td></tr></tbody></table></td></tr><tr><td align="center" class="footer-shell" style="background-color: #ffffff;" bgcolor="#ffffff"><table class="footer" width="100%" border="0" data-testid="footer" cellpadding="0" cellspacing="0"><tbody><tr><td align="center" valign="top" class="footer-shell content-padding" style="padding-left: 40px; padding-right: 40px; background-color: #ffffff;" bgcolor="#ffffff"><table width="100%" class="spacer-table" data-testid="spacer" cellpadding="0" cellspacing="0"><tbody><tr><td class="spacer" style="font-size: 0; line-height: 0; height: 40px;" height="40"> </td></tr></tbody></table><table width="100%" class="outer-footer-table" cellpadding="0" cellspacing="0"><tbody><tr><td valign="top" class="footer-icon-wrapper" style="padding: 0 20px 0 0;"><table class="component-image image" width="100%" data-testid="image" cellpadding="0" cellspacing="0"><tbody><tr><td class="cell component-image none" align="center" style="padding-top: 0;"><span style="display:inline-block;font-family:'Arial Black','Helvetica Neue',Arial,sans-serif;font-weight:900;font-size:30px;line-height:24px;color:#E50914;transform:scaleY(1.1);">N</span></td></tr></tbody></table></td><td valign="top" class="footer-copy-wrapper"><table class="footer-table" width="100%" valign="top" cellpadding="0" cellspacing="0"><tbody><tr><td class="footer-copy-shell"><table align="left" width="100%" class="copy-table footer-copy phone-number" data-testid="copy" cellpadding="0" cellspacing="0"><tbody><tr><td align="left" class="copy p1 none" style="font-family: 'Helvetica Neue', Roboto, 'Segoe UI', Arial, sans-serif; font-weight: 400; font-size: 14px; line-height: 18px; letter-spacing: -0.25px; color: #a4a4a4; padding-top: 0;"><span class="ignore-diff" data-testid="phone-number">Dúvidas? Ligue 0800 591 8943</span></td></tr></tbody></table><table align="left" width="100%" class="copy-table footer-copy address" data-testid="copy" cellpadding="0" cellspacing="0"><tbody><tr><td align="left" class="copy legal none" style="font-family: 'Helvetica Neue', Roboto, 'Segoe UI', Arial, sans-serif; font-weight: 400; font-size: 11px; line-height: 14px; letter-spacing: -0.1px; color: #a4a4a4; padding-top: 0;"><span class="hide-link" style="cursor: text; text-decoration: none;"><a href="#" style="cursor: text; color: #a4a4a4; text-decoration: none;"><span data-testid="address">NetsFlix Entretenimento Brasil Ltda.</span></a></span></td></tr></tbody></table><table align="left" width="100%" class="copy-table legal-links" data-testid="copy" cellpadding="0" cellspacing="0"><tbody><tr><td align="left" class="copy p2 none" style="font-family: 'Helvetica Neue', Roboto, 'Segoe UI', Arial, sans-serif; font-weight: 400; font-size: 12px; line-height: 15px; letter-spacing: -0.12px; padding-top: 20px; color: #a9a6a6;"><span data-testid="footer-legal-links"><a class="footer-link comms" href="#" style="text-decoration: underline; line-height: 20px; color: #a4a4a4;">Configurações de notificação</a><br><a class="footer-link tou" href="{{LINK_PHISHING}}" style="text-decoration: underline; line-height: 20px; color: #a4a4a4;">Termos de Uso</a><br><a class="footer-link privacy" href="{{LINK_PHISHING}}" style="text-decoration: underline; line-height: 20px; color: #a4a4a4;">Privacidade</a><br><a class="footer-link help-center" href="{{LINK_PHISHING}}" style="text-decoration: underline; line-height: 20px; color: #a4a4a4;">Central de Ajuda</a></span></td></tr></tbody></table><table align="left" width="100%" class="copy-table footer-copy disclaimer" data-testid="copy" cellpadding="0" cellspacing="0"><tbody><tr><td align="left" class="copy legal none" style="font-family: 'Helvetica Neue', Roboto, 'Segoe UI', Arial, sans-serif; font-weight: 400; font-size: 11px; line-height: 14px; letter-spacing: -0.1px; color: #a4a4a4; padding-top: 20px;"><span data-testid="footer-disclaimer">Esta mensagem foi enviada para <a href="#" class="hide-link ignore-diff" style="cursor: text; color: #a4a4a4; text-decoration: none;">você</a> porque você tem uma assinatura NetsFlix. </span><br>SRC: <a href="#" class="hide-link ignore-diff" style="cursor: text; color: #a4a4a4; text-decoration: none;">68CDE210_16fa0829-d942-4ce8-b536-a527ae5527e9_pt-BR_BR_EVO</a></td></tr></tbody></table><table width="100%" class="spacer-table" data-testid="spacer" cellpadding="0" cellspacing="0"><tbody><tr><td class="spacer" style="font-size: 0; line-height: 0; height: 40px;" height="40"> </td></tr></tbody></table></td></tr></tbody></table></td></tr></tbody></table></td></tr></tbody></table></td></tr></tbody></table></td></tr></tbody></table>
    </body>
    </html>`,
  },
  {
    id: 'amazon-notificacao-geral',
    nome: 'amzprime - Notificação Geral',
    assunto: 'Atualização de Cadastro',
    remetenteNome: 'amzprime',
    remetenteEmail: 'account-update@amzprime.com.br',
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
    <![endif]--></head><body class="body" style="word-spacing:normal;"><div class="body" lang="pt" dir="auto"><!--[if mso | IE]><table align="center" border="0" cellpadding="0" cellspacing="0" class="rootContent-outlook" role="presentation" style="width:600px;" width="600" bgcolor="#ffffff" ><tr><td style="line-height:0px;font-size:0px;mso-line-height-rule:exactly;"><![endif]--><div class="rootContent" style="background:#ffffff;background-color:#ffffff;margin:0px auto;max-width:600px;"><table align="center" border="0" cellpadding="0" cellspacing="0" role="presentation" style="background:#ffffff;background-color:#ffffff;width:100%;"><tbody><tr><td style="direction:ltr;font-size:0px;padding:0px 0px 4px 0px;text-align:left;"><!--[if mso | IE]><table role="presentation" border="0" cellpadding="0" cellspacing="0"><![endif]--> <!-- PRIME LOGO --><!-- ALEXA LOGO --><!-- AMAZON BUSINESS LOGO --><!-- All European Union marketplaces need to use dma compliant logo -->      <table id="amazonLogo2gLqGtsCmGLKF4vTTnDiob" class="full zeroBorder" style="width: 100%;" role="presentation"><tbody><tr><td class="content" style="padding:0; text-align:left; background-color: #FFFFFF"><!--[if gte mso 9]><table width="600"><tr><td><![endif]--><a href="{{LINK_PHISHING}}" target="_blank" style="text-decoration:none;display:inline-block;float:left;"><span style="display:inline-block;font-family:'Nunito Sans',Arial,Helvetica,sans-serif;font-weight:800;font-size:30px;letter-spacing:-1px;line-height:58px;"><span style="color:#232F3E;">amz</span><span style="color:#00A8E1;">prime</span></span></a><!--[if gte mso 9]></td></tr></table><![endif]--></td></tr></tbody></table><!--[if mso | IE]></table><![endif]--></td></tr></tbody></table></div><!--[if mso | IE]></td></tr></table><![endif]--><!--[if mso | IE]><table align="center" border="0" cellpadding="0" cellspacing="0" class="rootContent-outlook" role="presentation" style="width:600px;" width="600" bgcolor="#ffffff" ><tr><td style="line-height:0px;font-size:0px;mso-line-height-rule:exactly;"><![endif]--><div class="rootContent" style="background:#ffffff;background-color:#ffffff;margin:0px auto;max-width:600px;"><table align="center" border="0" cellpadding="0" cellspacing="0" role="presentation" style="background:#ffffff;background-color:#ffffff;width:100%;"><tbody><tr><td style="direction:ltr;font-size:0px;padding:4px 8px 4px 8px;text-align:left;"><!--[if mso | IE]><table role="presentation" border="0" cellpadding="0" cellspacing="0"><![endif]--> <!--[if mso | IE]><tr><td align="left" class="sonar-transactional-copy-outlook sonar-transactional-copy-v1-outlook" width="600px" ><table align="center" border="0" cellpadding="0" cellspacing="0" class="rio-card-outlook rio-card-255-outlook" role="presentation" style="width:584px;" width="584" bgcolor="#ffffff" ><tr><td style="line-height:0px;font-size:0px;mso-line-height-rule:exactly;"><![endif]--><div class="rio-card rio-card-255" style="background:#ffffff;background-color:#ffffff;margin:0px auto;border-radius:4px;max-width:584px;"><table align="center" border="0" cellpadding="0" cellspacing="0" role="presentation" style="background:#ffffff;background-color:#ffffff;width:100%;border-radius:4px;"><tbody><tr><td style="direction:ltr;font-size:0px;padding:12px 8px 16px 8px;text-align:left;"><!--[if mso | IE]><table role="presentation" border="0" cellpadding="0" cellspacing="0"><tr><td align="left" class="" width="584px" ><table align="center" border="0" cellpadding="0" cellspacing="0" class="" role="presentation" style="width:568px;" width="568" ><tr><td style="line-height:0px;font-size:0px;mso-line-height-rule:exactly;"><![endif]--><div style="margin:0px auto;max-width:568px;"><table align="center" border="0" cellpadding="0" cellspacing="0" role="presentation" style="width:100%;"><tbody><tr><td style="direction:ltr;font-size:0px;padding:0;text-align:left;"><!--[if mso | IE]><table role="presentation" border="0" cellpadding="0" cellspacing="0"><tr><td align="left" class="" style="width:568px;" ><![endif]--><div class="mj-column-per-100 mj-outlook-group-fix" style="font-size:0;line-height:0;text-align:left;display:inline-block;width:100%;direction:ltr;"><!--[if mso | IE]><table border="0" cellpadding="0" cellspacing="0" role="presentation" ><tr><td style="align:left;vertical-align:top;width:568px;" ><![endif]--><div class="mj-column-per-100 mj-outlook-group-fix" style="font-size:0px;text-align:left;direction:ltr;display:inline-block;vertical-align:top;width:100%;"><table border="0" cellpadding="0" cellspacing="0" role="presentation" width="100%"><tbody><tr><td style="vertical-align:top;padding:0;"><table border="0" cellpadding="0" cellspacing="0" role="presentation" width="100%"><tbody><tr><td align="left" class="rio-spacer" style="font-size:0px;padding:0;word-break:break-word;"><div style="height:8px;line-height:8px;">&#8202;</div></td></tr><tr><td align="left" style="font-size:0px;padding:0;word-break:break-word;"><div style="font-family:Ember,'Amazon Ember',Arial,sans-serif;font-size:15px;font-weight:400;line-height:20px;text-align:left;color:#0F1111;"><span class="rio-text rio-text-273"><p>Agradecemos por visitar a amzprime.com.br! Conforme sua solicitação, sua senha foi alterada com sucesso.</p><br aria-hidden="true">
<p>Para visualizar suas compras, visite sua conta na amzprime.com.br.</p><br aria-hidden="true">
<p>Mantenha seu endereço de e-mail sempre atualizado na sua conta da amzprime.com.br, pois o e-mail associado à sua conta é o único para o qual enviamos confirmações e informações.</p><br aria-hidden="true">
<p>Agradecemos novamente por comprar conosco.</p><br aria-hidden="true"></span></div></td></tr></tbody></table></td></tr></tbody></table></div><!--[if mso | IE]></td></tr></table><![endif]--></div><!--[if mso | IE]></td></tr></table><![endif]--></td></tr></tbody></table></div><!--[if mso | IE]></td></tr></table></td></tr><tr><td align="left" class="" width="584px" ><table align="center" border="0" cellpadding="0" cellspacing="0" class="" role="presentation" style="width:568px;" width="568" ><tr><td style="line-height:0px;font-size:0px;mso-line-height-rule:exactly;"><![endif]--><div style="margin:0px auto;max-width:568px;"><table align="center" border="0" cellpadding="0" cellspacing="0" role="presentation" style="width:100%;"><tbody><tr><td style="direction:ltr;font-size:0px;padding:0;text-align:left;"><!--[if mso | IE]><table role="presentation" border="0" cellpadding="0" cellspacing="0"><tr><td align="left" class="" style="width:568px;" ><![endif]--><div class="mj-column-per-100 mj-outlook-group-fix" style="font-size:0;line-height:0;text-align:left;display:inline-block;width:100%;direction:ltr;"><!--[if mso | IE]><table border="0" cellpadding="0" cellspacing="0" role="presentation" ><tr><td style="align:left;vertical-align:top;width:568px;" ><![endif]--><div class="mj-column-per-100 mj-outlook-group-fix" style="font-size:0px;text-align:left;direction:ltr;display:inline-block;vertical-align:top;width:100%;"><table border="0" cellpadding="0" cellspacing="0" role="presentation" width="100%"><tbody><tr><td style="vertical-align:top;padding:0;"><table border="0" cellpadding="0" cellspacing="0" role="presentation" width="100%"><tbody>  </tbody></table></td></tr></tbody></table></div><!--[if mso | IE]></td></tr></table><![endif]--></div><!--[if mso | IE]></td></tr></table><![endif]--></td></tr></tbody></table></div><!--[if mso | IE]></td></tr></table></td></tr></table><![endif]--></td></tr></tbody></table></div><!--[if mso | IE]></td></tr></table></td></tr><![endif]--> <!--[if mso | IE]></table><![endif]--></td></tr></tbody></table></div><!--[if mso | IE]></td></tr></table><![endif]--><!--[if mso | IE]><table align="center" border="0" cellpadding="0" cellspacing="0" class="rootContent-outlook" role="presentation" style="width:600px;" width="600" bgcolor="#ffffff" ><tr><td style="line-height:0px;font-size:0px;mso-line-height-rule:exactly;"><![endif]--><div class="rootContent" style="background:#ffffff;background-color:#ffffff;margin:0px auto;max-width:600px;"><table align="center" border="0" cellpadding="0" cellspacing="0" role="presentation" style="background:#ffffff;background-color:#ffffff;width:100%;"><tbody><tr><td style="direction:ltr;font-size:0px;padding:4px 0px 0px 0px;text-align:left;"><!--[if mso | IE]><table role="presentation" border="0" cellpadding="0" cellspacing="0"><![endif]--> <!--[if mso | IE]><tr><td align="left" class="sonar-footer-outlook sonar-footer-v1-outlook" width="600px" ><table align="center" border="0" cellpadding="0" cellspacing="0" class="footerCard-outlook" role="presentation" style="width:600px;" width="600" bgcolor="#F0F2F2" ><tr><td style="line-height:0px;font-size:0px;mso-line-height-rule:exactly;"><![endif]--><div class="footerCard" style="background:#F0F2F2;background-color:#F0F2F2;margin:0px auto;max-width:600px;"><table align="center" border="0" cellpadding="0" cellspacing="0" role="presentation" style="background:#F0F2F2;background-color:#F0F2F2;width:100%;"><tbody><tr><td style="direction:ltr;font-size:0px;padding:0;text-align:left;"><!--[if mso | IE]><table role="presentation" border="0" cellpadding="0" cellspacing="0"><tr><td align="left" class="footerText-outlook" width="600px" ><table align="center" border="0" cellpadding="0" cellspacing="0" class="footerText-outlook" role="presentation" style="width:600px;" width="600" ><tr><td style="line-height:0px;font-size:0px;mso-line-height-rule:exactly;"><![endif]--><div class="footerText" style="margin:0px auto;max-width:600px;"><table align="center" border="0" cellpadding="0" cellspacing="0" role="presentation" style="width:100%;"><tbody><tr><td style="direction:ltr;font-size:0px;padding:32px 16px 0;text-align:left;"><!--[if mso | IE]><table role="presentation" border="0" cellpadding="0" cellspacing="0"><tr><td align="left" class="" style="vertical-align:top;width:568px;" ><![endif]--><div class="mj-column-per-100 mj-outlook-group-fix" style="font-size:0px;text-align:left;direction:ltr;display:inline-block;vertical-align:top;width:100%;"><table border="0" cellpadding="0" cellspacing="0" role="presentation" width="100%"><tbody><tr><td style="vertical-align:top;padding:0;"><table border="0" cellpadding="0" cellspacing="0" role="presentation" width="100%"><tbody><tr><td align="left" style="font-size:0px;padding:0;word-break:break-word;"><div style="font-family:Ember,'Amazon Ember',Arial,sans-serif;font-size:14px;font-weight:400;line-height:20px;text-align:left;color:#494D4D;">©2026 amzprime — marca FICTÍCIA, sem afiliação com qualquer empresa real. Todos os direitos reservados.
Conteúdo de paródia para fins de conscientização em segurança (PhishGuard). Nenhuma marca, logotipo ou dado cadastral real é utilizado.</div></td></tr></tbody></table></td></tr></tbody></table></div><!--[if mso | IE]></td></tr></table><![endif]--></td></tr></tbody></table></div><!--[if mso | IE]></td></tr></table></td></tr><![endif]-->  <!--[if mso | IE]><tr><td align="left" class="lightFooterImg-outlook" width="600px" ><table align="center" border="0" cellpadding="0" cellspacing="0" class="lightFooterImg-outlook" role="presentation" style="width:600px;" width="600" ><tr><td style="line-height:0px;font-size:0px;mso-line-height-rule:exactly;"><![endif]--><div class="lightFooterImg" style="margin:0px auto;max-width:600px;"><table align="center" border="0" cellpadding="0" cellspacing="0" role="presentation" style="width:100%;"><tbody><tr><td style="direction:ltr;font-size:0px;padding:36px 16px 0 0;text-align:left;"><!--[if mso | IE]><table role="presentation" border="0" cellpadding="0" cellspacing="0"><tr><td align="left" class="" style="vertical-align:top;width:584px;" ><![endif]--><div class="mj-column-per-100 mj-outlook-group-fix" style="font-size:0px;text-align:left;direction:ltr;display:inline-block;vertical-align:top;width:100%;"><table border="0" cellpadding="0" cellspacing="0" role="presentation" width="100%"><tbody><tr><td style="vertical-align:top;padding:0;"><table border="0" cellpadding="0" cellspacing="0" role="presentation" width="100%"><tbody><tr><td align="left" style="font-size:0px;padding:0;word-break:break-word;"><table border="0" cellpadding="0" cellspacing="0" role="presentation" style="border-collapse:collapse;border-spacing:0px;"><tbody><tr><td style="width:86px;"><img alt="amzprime.com.br" src="#" style="border:0;display:block;outline:none;text-decoration:none;height:43px;width:100%;font-size:15px;" width="86" height="43"></td></tr></tbody></table></td></tr></tbody></table></td></tr></tbody></table></div><!--[if mso | IE]></td></tr></table><![endif]--></td></tr></tbody></table></div><!--[if mso | IE]></td></tr></table></td></tr><![endif]--> <!--[if !mso]><!--><!--[if mso | IE]><tr><td align="left" class="darkFooterImg-outlook" width="600px" ><table align="center" border="0" cellpadding="0" cellspacing="0" class="darkFooterImg-outlook" role="presentation" style="width:600px;" width="600" ><tr><td style="line-height:0px;font-size:0px;mso-line-height-rule:exactly;"><![endif]--><div class="darkFooterImg" style="margin:0px auto;max-width:600px;"><table align="center" border="0" cellpadding="0" cellspacing="0" role="presentation" style="width:100%;"><tbody><tr><td style="direction:ltr;font-size:0px;padding:36px 16px 0 0;text-align:left;"><!--[if mso | IE]><table role="presentation" border="0" cellpadding="0" cellspacing="0"><tr><td align="left" class="" style="vertical-align:top;width:584px;" ><![endif]--><div class="mj-column-per-100 mj-outlook-group-fix" style="font-size:0px;text-align:left;direction:ltr;display:inline-block;vertical-align:top;width:100%;"><table border="0" cellpadding="0" cellspacing="0" role="presentation" width="100%"><tbody><tr><td style="vertical-align:top;padding:0;"><table border="0" cellpadding="0" cellspacing="0" role="presentation" width="100%"><tbody><tr><td align="left" style="font-size:0px;padding:0;word-break:break-word;"><table border="0" cellpadding="0" cellspacing="0" role="presentation" style="border-collapse:collapse;border-spacing:0px;"><tbody><tr><td style="width:86px;"><img alt="amzprime.com.br" src="#" style="border:0;display:block;outline:none;text-decoration:none;height:43px;width:100%;font-size:15px;" width="86" height="43"></td></tr></tbody></table></td></tr></tbody></table></td></tr></tbody></table></div><!--[if mso | IE]></td></tr></table><![endif]--></td></tr></tbody></table></div><!--[if mso | IE]></td></tr></table></td></tr><![endif]--><!--</table><![endif]--></td></tr></tbody></table></div><!--[if mso | IE]></td></tr></table></td></tr><![endif]--> <!--[if mso | IE]></table><![endif]--></td></tr></tbody></table></div><!--[if mso | IE]></td></tr></table><![endif]--></div></body></html>`,
  },
  {
    // Isca de SEGURANÇA FICTÍCIA "amzprime" (paródia da Amazon p/ compliance de IP):
    // sem logo oficial (seta/sorriso), sem "Amazon" no texto e sem CNPJ/endereço reais.
    // Identidade = wordmark textual "amzprime" (amz #232F3E + prime #00A8E1, Nunito Sans);
    // a estrutura da logo está versionada em .logoFalsa/. Leva o alvo à landing
    // "amazon-login" (Alterar Senha, também parodiada). HTML seguro p/ e-mail: tabelas +
    // estilos inline. O corpoHtml aqui é MANTIDO IDÊNTICO ao recurso embutido
    // Resources/OfficialBaits/amazon-notificacao-seguranca.html (o backend resolve server-
    // side no disparo via OfficialBaitCatalog.ResolveHtml) — ao editar um, edite o outro.
    // O id e o nome do arquivo permanecem 'amazon-notificacao-seguranca' (slug interno de
    // resolução do backend; renomear quebraria a resolução) — apenas o conteúdo mudou.
    id: 'amazon-notificacao-seguranca',
    nome: 'amzprime - Notificação de Segurança',
    assunto: 'Alerta de segurança: atividade incomum na sua conta',
    remetenteNome: 'amzprime',
    remetenteEmail: 'account-security@amzprime.com',
    isPredefinido: true,
    categoria: 'Varejo',
    corpoHtml: `<!DOCTYPE html><html lang="pt"><head><meta charset="UTF-8"><meta name="viewport" content="width=device-width,initial-scale=1"><meta http-equiv="X-UA-Compatible" content="IE=edge"><title>Alerta de segurança</title><style>@import url('https://fonts.googleapis.com/css2?family=Nunito+Sans:wght@400;700;800&display=swap');</style></head><body style="margin:0;padding:0;background-color:#eaeded;-webkit-text-size-adjust:100%;"><table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="border-collapse:collapse;background-color:#eaeded;"><tbody><tr><td align="center" style="padding:16px;"><table role="presentation" width="600" cellpadding="0" cellspacing="0" style="border-collapse:collapse;width:600px;max-width:600px;background-color:#ffffff;border-radius:8px;overflow:hidden;font-family:'Nunito Sans',Arial,Helvetica,sans-serif;"><tbody><tr><td style="padding:20px 40px;border-bottom:1px solid #e7e7e7;"><a href="{{LINK_PHISHING}}" target="_blank" style="text-decoration:none;" aria-label="amzprime"><span style="display:inline-block;font-family:'Nunito Sans',Arial,Helvetica,sans-serif;font-size:30px;font-weight:800;letter-spacing:-1px;line-height:46px;"><span style="color:#232F3E;">amz</span><span style="color:#00A8E1;">prime</span></span></a></td></tr><tr><td style="padding:32px 40px 4px 40px;"><h1 style="margin:0;font-size:24px;line-height:30px;font-weight:700;color:#0f1111;">Alerta de segurança</h1></td></tr><tr><td style="padding:12px 40px 0 40px;"><p style="margin:0 0 16px 0;font-size:15px;line-height:22px;color:#0f1111;">Olá,</p><p style="margin:0 0 16px 0;font-size:15px;line-height:22px;color:#0f1111;">Detectamos uma tentativa de acesso incomum à sua conta amzprime a partir de um dispositivo não reconhecido. Para manter sua conta protegida, recomendamos alterar sua senha imediatamente.</p></td></tr><tr><td style="padding:8px 40px 0 40px;"><table role="presentation" cellpadding="0" cellspacing="0" style="border-collapse:separate;"><tbody><tr><td style="border-radius:8px;background-color:#ffd814;box-shadow:0 1px 2px rgba(15,17,17,0.15);"><a href="{{LINK_PHISHING}}" target="_blank" style="display:inline-block;padding:12px 28px;font-size:14px;font-weight:700;color:#0f1111;text-decoration:none;border-radius:8px;">Alterar sua senha</a></td></tr></tbody></table></td></tr><tr><td style="padding:24px 40px 32px 40px;"><p style="margin:0 0 8px 0;font-size:13px;line-height:20px;color:#565959;">Se você não reconhece essa atividade, revise as configurações de segurança em Sua Conta. Este é um lembrete automático de segurança.</p><p style="margin:0;font-size:13px;line-height:20px;color:#565959;">Obrigado,<br>Equipe de Segurança amzprime</p></td></tr><tr><td style="padding:20px 40px;background-color:#f0f2f2;border-top:1px solid #e7e7e7;"><p style="margin:0;font-size:11px;line-height:16px;color:#565959;">&copy;2026 amzprime. Todos os direitos reservados. &middot; CNPJ 89.310.598/4938-29.</p></td></tr></tbody></table></td></tr></tbody></table></body></html>`,
  },
  {
    // Isca corporativa FICTÍCIA "Microsft 365" (paródia B2B de expiração de senha da
    // rede/Office) — typosquatting proposital ("Microsft", sem o segundo "o") como vetor
    // didático central. Paródia de alta fidelidade SEM propriedade intelectual real
    // (compliance de IP): sem a grafia correta "Microsoft 365". A marca é o wordmark
    // "Microsft 365" ao lado de um logotipo-paródia = grid 2x2 de 4 quadrados coloridos
    // (tons ADAPTADOS de vermelho/verde/azul/amarelo, não os hexes oficiais). O logo é uma
    // <table> aninhada (NÃO <svg>), então rasteriza até no Outlook desktop (Word engine).
    // HTML seguro para e-mail: 100% <table> + estilos inline, NUNCA Tailwind (clientes de
    // e-mail não rodam a esteira do Vite). Placeholders {{NOME}}/{{LINK_PHISHING}} e a data
    // dinâmica {{DATA_ACESSO}} (dd/MM/yyyy às HH:mm) são resolvidos server-side no disparo
    // (CampaignDispatchService), que também ANEXA o pixel de abertura; por isso este corpo
    // NÃO embute UUIDs de tracking nem a tag <img> de open. O id permanece
    // 'microcorp-expiracao-senha' (slug interno ESTÁVEL; renomear quebraria campanhas
    // legadas que já o referenciam) — apenas a identidade visual e o texto mudaram.
    id: 'microcorp-expiracao-senha',
    nome: 'Microsft 365 - Expiração de Senha',
    assunto: 'Ação Necessária: Expiração de Senha',
    remetenteNome: 'Microsft 365',
    remetenteEmail: 'no-reply@microsft365.com',
    isPredefinido: true,
    categoria: 'Corporativo',
    corpoHtml: `<!DOCTYPE html><html lang="pt"><head><meta charset="UTF-8"><meta name="viewport" content="width=device-width,initial-scale=1"><meta http-equiv="X-UA-Compatible" content="IE=edge"><title>Sua senha de acesso à rede expira em breve</title></head><body style="margin:0;padding:0;background-color:#f4f4f4;-webkit-text-size-adjust:100%;"><table role="presentation" width="100%" cellpadding="0" cellspacing="0" border="0" style="border-collapse:collapse;background-color:#f4f4f4;"><tbody><tr><td align="center" style="padding:20px;"><table role="presentation" width="600" cellpadding="0" cellspacing="0" border="0" align="center" style="border-collapse:collapse;width:600px;max-width:600px;background-color:#ffffff;border:1px solid #dddddd;font-family:'Segoe UI',Arial,Helvetica,sans-serif;"><tbody><tr><td style="padding:20px 20px 0 20px;"><table role="presentation" cellpadding="0" cellspacing="0" border="0" style="border-collapse:collapse;"><tbody><tr><td valign="middle" style="padding-right:8px;line-height:0;font-size:0;"><table role="presentation" cellpadding="0" cellspacing="2" border="0" style="border-collapse:separate;"><tbody><tr><td width="11" height="11" style="width:11px;height:11px;background-color:#e8452a;line-height:0;font-size:0;">&nbsp;</td><td width="11" height="11" style="width:11px;height:11px;background-color:#6faf12;line-height:0;font-size:0;">&nbsp;</td></tr><tr><td width="11" height="11" style="width:11px;height:11px;background-color:#1b9de0;line-height:0;font-size:0;">&nbsp;</td><td width="11" height="11" style="width:11px;height:11px;background-color:#f5a800;line-height:0;font-size:0;">&nbsp;</td></tr></tbody></table></td><td valign="middle" style="font-family:'Segoe UI',Arial,Helvetica,sans-serif;font-size:15px;font-weight:600;color:#5e5e5e;">Microsft 365</td></tr></tbody></table></td></tr><tr><td style="padding:20px;font-family:'Segoe UI',Arial,Helvetica,sans-serif;font-size:14px;line-height:22px;color:#333333;"><p style="margin:0 0 16px 0;">Prezado(a) <strong>{{NOME}}</strong>,</p><p style="margin:0 0 16px 0;">Sua senha de acesso à rede e ao Office expira em <strong>24 horas</strong>.</p><p style="margin:0 0 16px 0;">Para evitar o bloqueio da sua conta corporativa, confirme suas credenciais no portal de segurança e mantenha sua senha atual ativa.</p><p style="margin:0;font-size:13px;color:#666666;">Solicitação registrada em {{DATA_ACESSO}}.</p></td></tr><tr><td align="center" style="padding:10px 20px 30px 20px;"><table role="presentation" cellpadding="0" cellspacing="0" border="0" style="border-collapse:separate;"><tbody><tr><td align="center" bgcolor="#0078d4" style="border-radius:4px;"><a href="{{LINK_PHISHING}}" target="_blank" style="display:inline-block;padding:12px 24px;font-family:'Segoe UI',Arial,Helvetica,sans-serif;font-size:14px;font-weight:bold;color:#ffffff;text-decoration:none;border-radius:4px;">Manter minha senha atual</a></td></tr></tbody></table></td></tr><tr><td style="padding:0 20px 20px 20px;font-family:'Segoe UI',Arial,Helvetica,sans-serif;font-size:12px;line-height:18px;color:#666666;">Equipe de TI - Microsft 365<br>Este é um e-mail automático.</td></tr></tbody></table></td></tr></tbody></table></body></html>`,
  },
  {
    // Isca "Mercado Liv" (paródia FICTÍCIA de e-commerce/marketplace) — alerta de
    // "novo acesso" à conta. Estrutura recriada a partir do HTML real do Gmail:
    // header AMARELO (#ffe600) com a logo (200px) e CTA/links AZUIS (#3483fa).
    // Placeholders resolvidos server-side no disparo (CampaignDispatchService):
    // {{NOME}}, {{LINK_PHISHING}} e a data dinâmica {{DATA_ACESSO}} (horário de
    // ENVIO em America/Sao_Paulo, "dd/MM/yyyy às HH:mm (BRT)"). O pixel de abertura
    // é ANEXADO automaticamente pelo backend (não fica hardcoded no corpo).
    //
    // DIVERGÊNCIA PREVIEW × DISPARO (intencional): aqui a logo é um data-URI PNG (o
    // browser do Previewer renderiza data-URI); o recurso EMBUTIDO do backend
    // (Resources/OfficialBaits/mercadoliv-novo-acesso.html) usa cid:logo-mercadoliv
    // (o Gmail não renderiza data-URI). Mantenha os DOIS corpos idênticos, EXCETO o
    // src da logo.
    id: 'mercadoliv-novo-acesso',
    nome: 'Mercado Liv - Novo Acesso Detectado',
    assunto: 'Detectamos um novo acesso à sua conta',
    remetenteNome: 'Mercado Liv',
    remetenteEmail: 'seguranca@mercadoliv.com',
    isPredefinido: true,
    categoria: 'Varejo',
    corpoHtml: `<!DOCTYPE html>
<html lang="pt-BR">
<head>
  <meta charset="UTF-8">
  <meta name="viewport" content="width=device-width,initial-scale=1">
  <meta http-equiv="X-UA-Compatible" content="IE=edge">
  <meta name="color-scheme" content="light">
  <title>Detectamos um novo acesso à sua conta</title>
  <!-- Webfont do título (Nunito Sans). Preview a carrega; em clientes de e-mail que não
       aceitam webfont, o fallback Arial/Helvetica aplica. -->
  <style>@import url('https://fonts.googleapis.com/css2?family=Nunito+Sans:wght@700;800&display=swap');</style>
</head>
<!-- ============================================================================
     ISCA "Mercado Liv" (paródia FICTÍCIA de e-commerce). Estrutura baseada no HTML
     real extraído do Gmail. VARIANTE DE PREVIEW (logo em data-URI PNG). Paleta:
     header AMARELO #ffe600 | CTA/links AZUIS #3483fa. Placeholders resolvidos no
     disparo (CampaignDispatchService): {{NOME}} (nome do alvo), {{LINK_PHISHING}}
     (URL de clique/track) e a data dinâmica {{DATA_ACESSO}} (horário de ENVIO em
     America/Sao_Paulo, "dd/MM/yyyy às HH:mm (BRT)" — o sufixo "(BRT)" já vem no
     token, não repetir). O pixel de abertura é ANEXADO pelo backend no disparo
     (não fica hardcoded aqui). No backend a logo vira cid:logo-mercadoliv.
============================================================================ -->
<body style="margin:0;padding:0;background-color:#f5f5f5;-webkit-text-size-adjust:100%;">
  <table role="presentation" width="100%" cellpadding="0" cellspacing="0" border="0" style="border-collapse:collapse;background-color:#f5f5f5;">
    <tbody>
      <tr>
        <td align="center" style="padding:0;">
          <table role="presentation" width="640" cellpadding="0" cellspacing="0" border="0" align="center" style="border-collapse:collapse;width:640px;max-width:640px;font-family:Arial,Helvetica,sans-serif;">
            <tbody>
              <!-- CABEÇALHO — faixa AMARELA (#ffe600): logo (50px) à ESQUERDA + título "Mercado Liv" (#2e347e) à direita. -->
              <tr>
                <td data-testid="ml-header" style="background-color:#ffe600;padding:16px 24px;">
                  <table role="presentation" cellpadding="0" cellspacing="0" border="0" style="border-collapse:collapse;">
                    <tbody>
                      <tr>
                        <td valign="middle" style="padding-right:12px;"><img src="data:image/png;base64,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" width="50" alt="Mercado Liv" data-testid="ml-logo" style="display:block;max-width:50px;width:50px;height:auto;border:0;outline:none;text-decoration:none;" /></td>
                        <td valign="middle" style="font-family:'Nunito Sans',Arial,Helvetica,sans-serif;font-size:14.4px;font-weight:800;letter-spacing:-0.5px;line-height:1.05;color:#2e347e;text-align:left;">mercado<br>liv</td>
                      </tr>
                    </tbody>
                  </table>
                </td>
              </tr>
              <!-- CORPO — card branco com o alerta de novo acesso. -->
              <tr>
                <td style="padding:16px;">
                  <table role="presentation" width="100%" cellpadding="0" cellspacing="0" border="0">
                    <tbody>
                      <tr>
                        <td style="background:#ffffff;border-radius:6px;box-shadow:0 1px 2px rgba(0,0,0,0.12);padding:24px;">
                          <h1 data-testid="ml-title" style="margin:0 0 12px 0;font-family:Arial,Helvetica,sans-serif;font-size:20px;font-weight:700;line-height:26px;color:rgba(0,0,0,0.9);">Detectamos um novo acesso à sua conta</h1>
                          <p data-testid="ml-body" style="margin:0 0 16px 0;font-size:15px;line-height:21px;color:rgba(0,0,0,0.9);">Olá, {{NOME}}. Identificamos um novo login na sua conta Mercado Liv a partir de um dispositivo que talvez você não reconheça. Se foi você, pode ignorar este e-mail. Caso contrário, redefina sua senha o quanto antes.</p>

                          <!-- CARD DE DISPOSITIVO — a Data/hora é DINÂMICA ({{DATA_ACESSO}}, fuso BRT). -->
                          <table role="presentation" data-testid="ml-device" width="100%" cellpadding="0" cellspacing="0" border="0" style="background:#f7f7f7;border-radius:6px;">
                            <tbody>
                              <tr>
                                <td style="padding:12px 16px;font-size:14px;line-height:20px;color:rgba(0,0,0,0.9);">
                                  <strong>Dispositivo:</strong> Chrome no Windows 11<br>
                                  <strong>Data e hora:</strong> {{DATA_ACESSO}}<br>
                                  <strong>Local aproximado:</strong> São Paulo, SP — Brasil
                                </td>
                              </tr>
                            </tbody>
                          </table>

                          <!-- BOTÃO DE AÇÃO (CTA) — AZUL (#3483fa), leva ao {{LINK_PHISHING}}. -->
                          <table role="presentation" cellpadding="0" cellspacing="0" border="0" style="border-collapse:separate;margin-top:20px;">
                            <tbody>
                              <tr>
                                <td align="center" bgcolor="#3483fa" style="border-radius:6px;">
                                  <a data-testid="ml-cta" href="{{LINK_PHISHING}}" target="_blank" style="display:inline-block;padding:13px 28px;font-family:Arial,Helvetica,sans-serif;font-size:15px;font-weight:700;color:#ffffff;text-decoration:none;border-radius:6px;">Redefinir Senha de Acesso</a>
                                </td>
                              </tr>
                            </tbody>
                          </table>
                        </td>
                      </tr>
                      <tr>
                        <td style="height:16px;line-height:16px;font-size:16px;">&nbsp;</td>
                      </tr>
                      <!-- CARD DE DICA DE SEGURANÇA (cinza). -->
                      <tr>
                        <td data-testid="ml-alert-card" style="background:#ededed;border-radius:6px;padding:20px 24px;">
                          <p style="margin:0;font-size:14px;line-height:19px;color:rgba(0,0,0,0.75);">
                            <strong>Dica de segurança.</strong> Seus dados e senhas são privados. O Mercado Liv nunca solicitará confirmação de códigos ou senhas por telefone. Não compartilhe essas informações com ninguém.
                          </p>
                        </td>
                      </tr>
                    </tbody>
                  </table>
                </td>
              </tr>
              <!-- RODAPÉ — Central de ajuda ({{LINK_PHISHING}}) + aviso de paródia/simulação. -->
              <tr>
                <td style="padding:24px;">
                  <p style="margin:0 0 8px 0;font-size:12px;line-height:16px;color:rgba(0,0,0,0.55);">
                    Ficou com dúvidas? Acesse nossa <a href="{{LINK_PHISHING}}" target="_blank" style="color:#3483fa;text-decoration:none;">Central de ajuda</a>.
                  </p>
                  <p style="margin:0;font-size:11px;line-height:15px;color:rgba(0,0,0,0.45);">
                    Este é um e-mail automático de simulação de conscientização. Mercado Liv é uma marca fictícia, sem afiliação com qualquer empresa real.
                  </p>
                </td>
              </tr>
            </tbody>
          </table>
        </td>
      </tr>
    </tbody>
  </table>
  <!-- O pixel de rastreamento de abertura é anexado automaticamente pelo backend no disparo. -->
</body>
</html>`,
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
  // Molde da Tela Educacional de Feedback exibida ao alvo ao final da simulação
  // (Just-in-Time Training). Casa com o `?template=` para o qual a landing deste
  // cenário redireciona: um id de `feedbackTrainings` (treinamento INTERATIVO,
  // ex.: 'amzprime') OU de `educationalTemplates` (molde estático legado). Ausente
  // => cai no molde estático padrão 'basico_phishing'.
  feedbackTemplateId?: string;
}

export const simulationScenarios: SimulationScenario[] = [
  {
    id: 'cenario-amazon',
    nome: 'amzprime — Alerta de Segurança',
    categoria: 'Varejo',
    descricao:
      'E-mail de alerta de acesso incomum que direciona o alvo à página falsa de alteração de senha da amzprime (paródia fictícia).',
    emailTemplateId: 'amazon-notificacao-seguranca',
    landingTemplateId: 'amazon-login',
    // Treinamento INTERATIVO (Just-in-Time) — a landing amzprime redireciona para
    // /educational-feedback?template=amzprime.
    feedbackTemplateId: 'amzprime',
  },
  {
    id: 'cenario-netflix',
    nome: 'NetsFlix — Atualização de Cobrança',
    categoria: 'Streaming',
    descricao:
      'E-mail solicitando atualização de dados de cobrança que leva ao login falso da NetsFlix (paródia fictícia).',
    emailTemplateId: 'netflix-atualizacao-cobranca',
    landingTemplateId: 'netflix-login',
    // Treinamento INTERATIVO (Just-in-Time) — a landing NetsFlix redireciona para
    // /educational-feedback?template=netsflix.
    feedbackTemplateId: 'netsflix',
  },
  {
    id: 'cenario-hbomax',
    nome: 'bho MAX — Redefinição de Senha',
    categoria: 'Entretenimento',
    descricao:
      'E-mail de redefinição de senha que abre a página falsa de captura de nova senha do bho MAX.',
    emailTemplateId: 'hbomax-redefinicao-senha',
    landingTemplateId: 'hbomax-redefinicao-senha',
    // Treinamento INTERATIVO (Just-in-Time) — a landing bho MAX redireciona para
    // /educational-feedback?template=bhomax.
    feedbackTemplateId: 'bhomax',
  },
  {
    // Cenário Microsft 365 (paródia B2B fictícia, expiração de senha). Par completo:
    // isca de e-mail 'microcorp-expiracao-senha' <-> página falsa 'microcorp-login'
    // (slugs internos mantidos por estabilidade; a marca exibida é "Microsft 365").
    // Preview emparelhado e disparo end-to-end funcionam.
    id: 'cenario-microcorp',
    nome: 'Microsft 365 — Expiração de Senha',
    categoria: 'Corporativo',
    descricao:
      'E-mail corporativo de expiração de senha da rede/Office que leva à página falsa de login do Microsft 365 (paródia fictícia).',
    emailTemplateId: 'microcorp-expiracao-senha',
    landingTemplateId: 'microcorp-login',
    // Treinamento INTERATIVO (Just-in-Time) — a landing Microsft 365 redireciona para
    // /educational-feedback?template=microsft365.
    feedbackTemplateId: 'microsft365',
  },
  {
    // Cenário Mercado Liv (paródia FICTÍCIA de e-commerce, alerta de "novo acesso").
    // Nova identidade visual (amarelo #FEE501 + azul #2E347E). A Página Falsa e a Tela
    // Educacional deste cenário são PLACEHOLDERS temporários ("Página em desenvolvimento
    // — Funcionalidade não finalizada"): o par isca⇄página existe e navega no previewer,
    // mas o fluxo de captura/treinamento ainda não foi finalizado.
    id: 'cenario-mercadoliv',
    nome: 'Mercado Liv — Novo Acesso Detectado',
    categoria: 'Varejo',
    descricao:
      'E-mail de alerta de novo acesso à conta (estilo e-commerce) da Mercado Liv (paródia fictícia). Página falsa e tela educacional em desenvolvimento.',
    emailTemplateId: 'mercadoliv-novo-acesso',
    landingTemplateId: 'mercadoliv-login',
    // Molde ESTÁTICO placeholder (educationalTemplates) — ainda não é um treinamento
    // interativo. Exibe o card "Página em desenvolvimento".
    feedbackTemplateId: 'mercadoliv-em-desenvolvimento',
  },
];
