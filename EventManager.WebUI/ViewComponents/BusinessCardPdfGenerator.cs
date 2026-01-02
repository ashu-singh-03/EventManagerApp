using PuppeteerSharp;
using PuppeteerSharp.Media;

namespace EventManager.WebUI.ViewComponents
{
    public class BusinessCardPdfGenerator
    {
        private readonly string _participantName;
        private readonly string _company;
        private readonly string _qrCodeBase64;

        public BusinessCardPdfGenerator(string participantName, string company, string qrCodeBase64)
        {
            _participantName = participantName;
            _company = company;
            _qrCodeBase64 = qrCodeBase64;
        }

        public async Task<byte[]> GeneratePdfAsync()
        {
            var htmlTemplate = @"
        <!DOCTYPE html>
        <html lang='en'>
        <head>
            <meta charset='UTF-8'>
            <style>
                * { margin: 0; padding: 0; box-sizing: border-box; }
                body { background: white; }
                .pdf-page {
                    width: 105mm;
                    height: 148mm;
                    background: white;
                    position: relative;
                }
                .business-card {
                    width: 63.5mm;
                    height: 50.8mm;
                    background: linear-gradient(135deg, white 0%, white 100%);
                    border-radius: 8px;
                    padding: 4mm;
                    position: absolute;
                    bottom: 15mm;
                    left: 50%;
                    transform: translateX(-50%);
                    display: flex;
                    color: #161515;
                    box-shadow: 0 4px 12px rgba(0,0,0,0.2);
                }
                .card-left { flex: 1; padding-right: 3mm; border-right: 1px solid #ddd; }
                .card-right { width: 20mm; padding-left: 3mm; }
                .name { font-size: 5mm; font-weight: bold; margin-bottom: 2mm; }
                .company { font-size: 3.5mm; margin-bottom: 2mm; }
                .country { font-size: 3mm; opacity: 0.9; }
                .qr-code { width: 20mm; height: 20mm; }
            </style>
        </head>
        <body>
            <div class='pdf-page'>
                <div class='business-card'>
                    <div class='card-left'>
                        <div class='name'>" + _participantName + @"</div>
                        <div class='company'>" + _company + @"</div>
                        <div class='country'>United Kingdom</div>
                    </div>
                    <div class='card-right'>
                        <img src='" + _qrCodeBase64 + @"' class='qr-code'/>
                    </div>
                </div>
            </div>
        </body>
        </html>";

            // Launch browser
            using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true,
                DefaultViewport = new ViewPortOptions
                {
                    Width = 1050,  // 105mm at 10x scale
                    Height = 1480  // 148mm at 10x scale
                },
                Args = new[] { "--no-sandbox" }
            });

            using var page = await browser.NewPageAsync();
            await page.SetContentAsync(htmlTemplate);

            // Wait for images to load
            await page.WaitForSelectorAsync(".qr-code");

            // Generate PDF with exact A6 dimensions
            return await page.PdfDataAsync(new PdfOptions
            {
                Width = "105mm",
                Height = "148mm",
                PrintBackground = true,
                MarginOptions = new MarginOptions
                {
                    Top = "0mm",
                    Right = "0mm",
                    Bottom = "0mm",
                    Left = "0mm"
                }
            });
        }
    }
}
