using HtmlAgilityPack;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace EventManager.WebUI.ViewComponents
{
    public class HtmlToQuestPdfConverter
    {
        public byte[] ConvertHtmlToPdfBytes(string html, bool isA6 = true)
        {
            try
            {
                // Parse HTML
                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(html);

                // Extract data
                var name = ExtractText(htmlDoc, "//div[@class='name']");
                var company = ExtractText(htmlDoc, "//div[@class='company']");
                var country = ExtractText(htmlDoc, "//div[@class='country']") ?? "United Kingdom";

                // Extract QR code
                var qrImage = ExtractQrCode(htmlDoc);

                // Create PDF - SIMPLIFIED VERSION THAT WORKS
                return CreateWorkingBusinessCardPdf(name, company, country, qrImage);
            }
            catch (Exception ex)
            {
                // Fallback to simple card
                return CreateSimpleBusinessCardPdf("Error", ex.Message, "Failed to generate");
            }
        }

        private string ExtractText(HtmlDocument doc, string xpath)
        {
            try
            {
                var node = doc.DocumentNode.SelectSingleNode(xpath);
                return node?.InnerText?.Trim() ?? "";
            }
            catch
            {
                return "";
            }
        }

        private byte[] ExtractQrCode(HtmlDocument doc)
        {
            try
            {
                var imgNode = doc.DocumentNode.SelectSingleNode("//img[@class='qr-placeholder']");
                if (imgNode != null)
                {
                    var src = imgNode.GetAttributeValue("src", "");
                    if (src.StartsWith("data:image/png;base64,"))
                    {
                        var base64String = src.Substring("data:image/png;base64,".Length);
                        return Convert.FromBase64String(base64String);
                    }
                }
            }
            catch
            {
                // Ignore extraction errors
            }
            return null;
        }

        // WORKING VERSION - Simple and reliable
        private byte[] CreateWorkingBusinessCardPdf(string name, string company, string country, byte[] qrImage)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    // A6 size
                    page.Size(PageSizes.A6);
                    page.Margin(15); // Page margin

                    page.Content().Element(content =>
                    {
                        content.AlignCenter().AlignMiddle().Element(card =>
                        {
                            // Simple card with proper dimensions
                            card.Background(Colors.White)
                                .Border(1).BorderColor(Colors.Grey.Lighten2)
                                .Padding(10) // Reduced padding
                                .Column(col =>
                                {
                                    // Name at top
                                    col.Item().Text(name)
                                        .FontSize(12).Bold().AlignCenter().FontColor(Colors.Black);

                                    // Divider
                                    col.Item().PaddingVertical(5).LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten2);

                                    // Content in a row
                                    col.Item().Row(row =>
                                    {
                                        // Text column - takes 60%
                                        row.RelativeItem(6).Column(textCol =>
                                        {
                                            if (!string.IsNullOrEmpty(company))
                                            {
                                                textCol.Item().Text(company)
                                                    .FontSize(9).FontColor(Colors.Black);
                                                textCol.Item().PaddingTop(2);
                                            }

                                            textCol.Item().Text(country)
                                                .FontSize(8).FontColor(Colors.Black);
                                        });

                                        // QR column - takes 40% (MORE SPACE!)
                                        row.RelativeItem(4).AlignCenter().AlignMiddle()
                                            .Element(qrCol =>
                                            {
                                                // Smaller QR that fits (40 points)
                                                var qrSize = 40f;

                                                if (qrImage != null)
                                                {
                                                    qrCol.Width(qrSize).Height(qrSize).Image(qrImage);
                                                }
                                                else
                                                {
                                                    qrCol.Width(qrSize).Height(qrSize)
                                                        .Background(Colors.Grey.Lighten2)
                                                        .AlignCenter().AlignMiddle()
                                                        .Text("QR")
                                                        .FontSize(10).Bold().FontColor(Colors.Grey.Medium);
                                                }
                                            });
                                    });
                                });
                        });
                    });
                });
            });

            return document.GeneratePdf();
        }

        // Even simpler version - guaranteed to work
        public byte[] CreateSimpleBusinessCardPdf(string name, string company, string country, byte[] qrImage = null)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A6);
                    page.Margin(10);

                    page.Content().Column(col =>
                    {
                        // Name
                        col.Item().Text(name)
                            .FontSize(14).Bold().AlignCenter();

                        // QR code (centered)
                        col.Item().PaddingTop(10).AlignCenter().Element(qr =>
                        {
                            var qrSize = 60f;
                            if (qrImage != null)
                            {
                                qr.Width(qrSize).Height(qrSize).Image(qrImage);
                            }
                            else
                            {
                                qr.Width(qrSize).Height(qrSize)
                                    .Background(Colors.Grey.Lighten2)
                                    .AlignCenter().AlignMiddle()
                                    .Text("QR")
                                    .FontSize(12).Bold();
                            }
                        });

                        // Company (if exists)
                        if (!string.IsNullOrEmpty(company))
                        {
                            col.Item().PaddingTop(10).Text(company)
                                .FontSize(10).AlignCenter();
                        }

                        // Country
                        col.Item().PaddingTop(5).Text(country)
                            .FontSize(9).AlignCenter();
                    });
                });
            });

            return document.GeneratePdf();
        }

        // Alternative: Vertical layout that always works
        public byte[] CreateVerticalBusinessCardPdf(string name, string company, string country, byte[] qrImage)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A6);
                    page.Margin(10);

                    page.Content().Element(content =>
                    {
                        content.AlignCenter().AlignMiddle().Element(card =>
                        {
                            card.Background(Colors.White)
                                .Border(1).BorderColor(Colors.Grey.Lighten2)
                                .Padding(8)
                                .Column(col =>
                                {
                                    // QR code at top
                                    col.Item().AlignCenter().Element(qr =>
                                    {
                                        var qrSize = 50f;
                                        if (qrImage != null)
                                        {
                                            qr.Width(qrSize).Height(qrSize).Image(qrImage);
                                        }
                                    });

                                    // Name below QR
                                    col.Item().PaddingTop(5).Text(name)
                                        .FontSize(11).Bold().AlignCenter();

                                    // Company
                                    if (!string.IsNullOrEmpty(company))
                                    {
                                        col.Item().PaddingTop(2).Text(company)
                                            .FontSize(9).AlignCenter();
                                    }

                                    // Country
                                    col.Item().PaddingTop(2).Text(country)
                                        .FontSize(8).AlignCenter();
                                });
                        });
                    });
                });
            });

            return document.GeneratePdf();
        }

        private byte[] CreateErrorPdf(string message, bool isA6)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(isA6 ? PageSizes.A6 : PageSizes.A4);
                    page.Margin(20);

                    page.Content().Column(col =>
                    {
                        col.Item().Text("Error")
                            .FontSize(12).Bold().FontColor(Colors.Red.Medium);

                        col.Item().PaddingTop(10).Text(message)
                            .FontSize(9);
                    });
                });
            });

            return document.GeneratePdf();
        }
    }
}