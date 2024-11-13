
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace Application.Helper
{
    public static class ConfigureEmail
    {
		public readonly static IConfiguration config;
		private readonly static string base64Image = "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";

        private static readonly string image = $"data:image/png;base64,{base64Image}";
        public static async Task SendWelcomeEmailAsync(string email, string firstName, string password)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Easydoc", "pfe_test123@outlook.com"));
            message.To.Add(new MailboxAddress(firstName, email));
            message.Subject = "Easydoc Account Creation";

            message.Body = new TextPart("html")
            {
                Text = ConfigureEmail.getBody(password)
                //$"Welcome to Digital Work! We're thrilled to have you on board. This is your password so that you can access your account: {password}"
            };

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync("outlook.office365.com", 587, false);
                await client.AuthenticateAsync("pfe_test123@outlook.com", "frbqkgntqwgdcmgq");
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
        }

        public static string getBody(string password)
        {
            return $@"<!DOCTYPE html><html lang=""en"" xmlns:o=""urn:schemas-microsoft-com:office:office"" xmlns:v=""urn:schemas-microsoft-com:vml"">
					<head>
					<title></title>
					<meta content=""text/html; charset=utf-8"" http-equiv=""Content-Type""/>
					<meta content=""width=device-width, initial-scale=1.0"" name=""viewport""/><!--[if mso]><xml><o:OfficeDocumentSettings><o:PixelsPerInch>96</o:PixelsPerInch><o:AllowPNG/></o:OfficeDocumentSettings></xml><![endif]--><!--[if !mso]><!--><!--<![endif]-->
					<style>* {{
						box-sizing: border-box;
					}}

					body {{
						margin: 0;
						padding: 0;
				}}

					a[x-apple-data-detectors] {{
						color: inherit !important;
						text-decoration: inherit !important;
					}}

					#MessageViewBody a {{
						color: inherit;
						text-decoration: none;
					}}

					p {{
						line-height: inherit
				}}

					.desktop_hide,
					.desktop_hide table {{
						mso-hide: all;
						display: none;
						max-height: 0px;
						overflow: hidden;
				}}

					.image_block img+div {{
						display: none;
					}}

					sup,
					sub {{
						line-height: 0;
						font-size: 75%;
				}}

					@media (max-width:670px) {{
						.desktop_hide table.icons-inner {{
							display: inline-block !important;
						}}

						.icons-inner {{
							text-align: center;
						}}

						.icons-inner td {{
							margin: 0 auto;
						}}

						.mobile_hide {{
							display: none;
					}}

						.row-content {{
							width: 100% !important;
						}}

						.stack .column {{
							width: 100%;
							display: block;
						}}
						.mobile_hide {{
							min-height: 0;
							max-height: 0;
							max-width: 0;
							overflow: hidden;
							font-size: 0px;
					}}

						.desktop_hide,
						.desktop_hide table {{
							display: table !important;
							max-height: none !important;
						}}

						.row-1 .column-1 .block-3.paragraph_block td.pad>div,
						.row-1 .column-1 .block-4.paragraph_block td.pad>div {{
							font-size: 36px !important;
					}}

						.row-1 .column-1 .block-2.spacer_block {{
							height: 10px !important;
					}}

						.row-1 .column-1 .block-6.paragraph_block td.pad>div {{
							font-size: 14px !important;
					}}

						.row-1 .column-1 .block-9.spacer_block {{
							height: 20px !important;
					}}

						.row-1 .column-1 .block-8.button_block a,
						.row-1 .column-1 .block-8.button_block div,
						.row-1 .column-1 .block-8.button_block span {{
							line-height: 28px !important;
						}}

						.row-1 .column-1 .block-8.button_block .alignment a,
						.row-1 .column-1 .block-8.button_block .alignment div {{
							width: 50% !important;
						}}
					}}
						</style><!--[if mso ]><style>sup, sub {{ font-size: 100% !important; }} sup {{ mso-text-raise:10% }} sub {{ mso-text-raise:-10% }}</style> <![endif]--><!--[if true]><style>.forceBgColor{{background-color: white !important}}</style><![endif]-->
					</head>
						<body class=""body forceBgColor"" style=""background-color: transparent; margin: 0; padding: 0; -webkit-text-size-adjust: none; text-size-adjust: none;"">
						<table border=""0"" cellpadding=""0"" cellspacing=""0"" class=""nl-container"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt; background-color: transparent; background-size: auto; background-image: none; background-position: top left; background-repeat: no-repeat;"" width=""100%"">
						<tbody>
						<tr>
						<td>
						<table align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" class=""row row-1"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt; background-size: auto;"" width=""100%"">
						<tbody>
						<tr>
						<td>
						<table align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" class=""row-content stack"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt; background-color: #007c86; background-image: url('images/BEE_May20_MarketingAgency_Onboarding_v01_copy.jpg'); background-repeat: no-repeat; background-size: cover; border-radius: 0; color: #000000; width: 650px; margin: 0 auto;"" width=""650"">
						<tbody>
						<tr>
						<td class=""column column-1"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt; font-weight: 400; text-align: left; vertical-align: top; border-top: 0px; border-right: 0px; border-bottom: 0px; border-left: 0px;"" width=""100%"">
						<table border=""0"" cellpadding=""0"" cellspacing=""0"" class=""image_block block-1"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt;"" width=""100%"">
						<tr>
						<td class=""pad"" style=""width:100%;padding-right:0px;padding-left:0px;"">
						<div align=""center"" class=""alignment"" style=""line-height:10px"">
						<div style=""max-width: 65px;""><img height=""auto"" src='{image}' style=""display: block; height: auto; border: 0; width: 100%;"" width=""65""/></div>
						</div>
						</td>
						</tr>
						</table>
						<div class=""spacer_block block-2"" style=""height:20px;line-height:20px;font-size:1px;""> </div>
						<table border=""0"" cellpadding=""0"" cellspacing=""0"" class=""paragraph_block block-3"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt; word-break: break-word;"" width=""100%"">
						<tr>
						<td class=""pad"">
						<div style=""color:#ffffff;font-family:'Helvetica Neue', Helvetica, Arial, sans-serif;font-size:60px;font-weight:700;letter-spacing:-1px;line-height:120%;text-align:center;mso-line-height-alt:72px;"">
						<p style=""margin: 0; word-break: break-word;"">Welcome to </p>
						</div>
						</td>
						</tr>
						</table>
						<table border=""0"" cellpadding=""0"" cellspacing=""0"" class=""paragraph_block block-4"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt; word-break: break-word;"" width=""100%"">
						<tr>
						<td class=""pad"">
						<div style=""color:#ffffff;font-family:TimesNewRoman, 'Times New Roman', Times, Beskerville, Georgia, serif;font-size:60px;font-weight:400;letter-spacing:-1px;line-height:120%;text-align:center;mso-line-height-alt:72px;"">
						<p style=""margin: 0;""><strong>Easy</strong>doc</p>
						</div>
						</td>
						</tr>
						</table>
						<div class=""spacer_block block-5"" style=""height:10px;line-height:10px;font-size:1px;""> </div>
						<table border=""0"" cellpadding=""0"" cellspacing=""0"" class=""paragraph_block block-6"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt; word-break: break-word;"" width=""100%"">
						<tr>
						<td class=""pad"" style=""padding-left:60px;padding-right:60px;padding-top:10px;"">
						<div style=""color:#ffffff;direction:ltr;font-family:'Helvetica Neue', Helvetica, Arial, sans-serif;font-size:16px;font-weight:400;letter-spacing:0px;line-height:120%;text-align:center;mso-line-height-alt:19.2px;"">
                    <p style=""margin: 0;"">We're thrilled to have you on board. This is your password so that you can access your account</p>

						</div>
						</td>
						</tr>
						</table>
						<div class=""spacer_block block-7"" style=""height:20px;line-height:20px;font-size:1px;""> </div>
						<table border=""0"" cellpadding=""10"" cellspacing=""0"" class=""button_block block-8"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt;"" width=""100%"">
						<tr>
						<td class=""pad"">
						<div align=""center"" class=""alignment""><!--[if mso]>
						<v:roundrect xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:w=""urn:schemas-microsoft-com:office:word"" href=""http://www.example.com"" style=""height:39px;width:189px;v-text-anchor:middle;"" arcsize=""26%"" strokeweight=""0.75pt"" strokecolor=""#222222"" fillcolor=""#ddd988"">
						<w:anchorlock/>
						<v:textbox inset=""0px,0px,0px,0px"">
						<center dir=""false"" style=""color:#222222;font-family:Arial, sans-serif;font-size:14px"">
						<![endif]--><a href=""http://www.example.com"" style=""background-color:#ddd988;border-bottom:1px solid #222222;border-left:1px solid #222222;border-radius:10px;border-right:1px solid #222222;border-top:1px solid #222222;color:#222222;display:block;font-family:'Helvetica Neue', Helvetica, Arial, sans-serif;font-size:14px;font-weight:400;mso-border-alt:none;padding-bottom:5px;padding-top:5px;text-align:center;text-decoration:none;width:30%;word-break:keep-all;"" target=""_blank""><span style=""word-break: break-word; padding-left: 15px; padding-right: 15px; font-size: 14px; display: inline-block; letter-spacing: 1px;"">
                         <span style=""word-break: break-word; line-height: 28px;""> <strong>${password}</strong></span></span></a><!--[if mso]></center></v:textbox></v:roundrect><![endif]--></div>
						</td>
						</tr>
						</table>
						<div class=""spacer_block block-9"" style=""height:40px;line-height:40px;font-size:1px;""> </div>
						</td>
						</tr>
						</tbody>
						</table>
						</td>
						</tr>
						</tbody>
						</table>
						<table align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" class=""row row-2"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt;"" width=""100%"">
						<tbody>
						<tr>
						<td>
						<table align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" class=""row-content stack"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt; background-color: #f6f5f1; border-radius: 0; color: #000000; width: 650px; margin: 0 auto;"" width=""650"">
						<tbody>
						<tr>
						<td class=""column column-1"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt; font-weight: 400; text-align: left; vertical-align: top; border-top: 0px; border-right: 0px; border-bottom: 0px; border-left: 0px;"" width=""100%"">
						<table border=""0"" cellpadding=""0"" cellspacing=""0"" class=""empty_block block-1"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt;"" width=""100%"">
						<tr>
						<td class=""pad"">
						<div></div>
						</td>
						</tr>
						</table>
						</td>
						</tr>
						</tbody>
						</table>
						</td>
						</tr>
						</tbody>
						</table>
						<table align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" class=""row row-3"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt; background-color: #ffffff;"" width=""100%"">
						<tbody>
						<tr>
						<td>
						<table align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" class=""row-content stack"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt; background-color: #ffffff; color: #000000; width: 650px; margin: 0 auto;"" width=""650"">
						<tbody>
						<tr>
						<td class=""column column-1"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt; font-weight: 400; text-align: left; padding-bottom: 5px; padding-top: 5px; vertical-align: top; border-top: 0px; border-right: 0px; border-bottom: 0px; border-left: 0px;"" width=""100%"">
						<table border=""0"" cellpadding=""0"" cellspacing=""0"" class=""icons_block block-1"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt; text-align: center; line-height: 0;"" width=""100%"">
						<tr>
						<td class=""pad"" style=""vertical-align: middle; color: #1e0e4b; font-family: 'Inter', sans-serif; font-size: 15px; padding-bottom: 5px; padding-top: 5px; text-align: center;""><!--[if vml]><table align=""center"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""display:inline-block;padding-left:0px;padding-right:0px;mso-table-lspace: 0pt;mso-table-rspace: 0pt;""><![endif]-->
						<!--[if !vml]><!-->
						
						</td>
						</tr>
						</table>
						</td>
						</tr>
						</tbody>
						</table>
						</td>
						</tr>
						</tbody>
						</table>
						</td>
						</tr>
						</tbody>
						</table><!-- End -->
						</body>
						</html>
						";
				}

			}
}
