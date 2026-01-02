using QuestPDF.Fluent;
namespace EventManager.WebUI.ViewComponents
{

    using HtmlAgilityPack;
    using QuestPDF.Fluent;
    using QuestPDF.Helpers;
    using QuestPDF.Infrastructure;

    namespace EventManager.WebUI.ViewComponents
    {
        public class HtmlToQuestPdfConverter
        {
            public Document ConvertHtmlToDocument(string html, bool isA6 = false)
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                return Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        // Set page size based on parameter
                        if (isA6)
                        {
                            page.Size(PageSizes.A6);
                            page.Margin(0.5f, Unit.Centimetre); // Smaller margins for A6
                        }
                        else
                        {
                            page.Size(PageSizes.A4);
                            page.Margin(2, Unit.Centimetre);
                        }

                        page.DefaultTextStyle(style =>
                            isA6
                            ? style.FontSize(8) // Smaller font for A6
                            : style.FontSize(11)
                        );

                        page.Content().Column(column =>
                        {
                            ProcessHtmlNode(column, doc.DocumentNode, isA6);
                        });
                    });
                });
            }

            private void ProcessHtmlNode(ColumnDescriptor column, HtmlNode node, bool isA6, int indentLevel = 0)
            {
                // Skip script and style tags
                if (node.Name.ToLower() is "script" or "style")
                    return;

                // Handle text nodes
                if (node.NodeType == HtmlNodeType.Text)
                {
                    var text = node.InnerText.Trim();
                    if (!string.IsNullOrEmpty(text))
                    {
                        column.Item().Text(text);
                    }
                    return;
                }

                // Calculate spacing based on page size
                var spacing = isA6 ? 2 : 5;
                var indent = isA6 ? 8 * indentLevel : 15 * indentLevel;

                switch (node.Name.ToLower())
                {
                    case "h1":
                        column.Item()
                            .PaddingBottom(spacing)
                            .Text(node.InnerText.Trim())
                            .FontSize(isA6 ? 12 : 24)
                            .Bold();
                        break;

                    case "h2":
                        column.Item()
                            .PaddingBottom(spacing)
                            .Text(node.InnerText.Trim())
                            .FontSize(isA6 ? 10 : 20)
                            .Bold();
                        break;

                    case "h3":
                        column.Item()
                            .PaddingBottom(spacing)
                            .Text(node.InnerText.Trim())
                            .FontSize(isA6 ? 9 : 18)
                            .Bold();
                        break;

                    case "h4":
                    case "h5":
                    case "h6":
                        column.Item()
                            .PaddingBottom(spacing)
                            .Text(node.InnerText.Trim())
                            .FontSize(isA6 ? 8 : 16)
                            .Bold();
                        break;

                    case "p":
                        column.Item()
                            .PaddingBottom(spacing)
                            .PaddingLeft(indent)
                            .Text(node.InnerText.Trim());
                        break;

                    case "br":
                        column.Item().PaddingBottom(spacing / 2f);
                        break;

                    case "hr":
                        column.Item()
                            .PaddingVertical(spacing)
                            .LineHorizontal(0.5f);
                        break;

                    case "ul":
                    case "ol":
                        column.Item()
                            .PaddingVertical(spacing / 2f)
                            .PaddingLeft(indent)
                            .Column(listColumn =>
                            {
                                var items = node.SelectNodes("li") ?? new HtmlNodeCollection(node);
                                for (int i = 0; i < items.Count; i++)
                                {
                                    var item = items[i];
                                    var prefix = node.Name.ToLower() == "ol"
                                        ? $"{i + 1}."
                                        : isA6 ? "•" : "•"; // Using bullet for both, adjust as needed

                                    listColumn.Item()
                                        .PaddingBottom(1)
                                        .Text($"{prefix} {item.InnerText.Trim()}");
                                }
                            });
                        break;

                    case "div":
                    case "section":
                    case "article":
                    case "main":
                        // Create nested column for block elements
                        column.Item().Column(nestedColumn =>
                        {
                            foreach (var child in node.ChildNodes)
                            {
                                ProcessHtmlNode(nestedColumn, child, isA6, indentLevel);
                            }
                        });
                        break;

                    case "span":
                    case "strong":
                    case "b":
                        // Inline elements - combine with previous text if possible
                        foreach (var child in node.ChildNodes)
                        {
                            ProcessHtmlNode(column, child, isA6, indentLevel);
                        }
                        break;

                    case "em":
                    case "i":
                        column.Item().Text(node.InnerText.Trim()).Italic();
                        break;

                    case "a":
                        column.Item().Text($"[{node.InnerText.Trim()}]").Underline();
                        break;

                    case "code":
                        column.Item()
                            .Background(Colors.Grey.Lighten3)
                            .Padding(2)
                            .Text(node.InnerText.Trim())
                            .FontFamily("Courier");
                        break;

                    case "pre":
                        column.Item()
                            .Background(Colors.Grey.Lighten3)
                            .Border(1)
                            .BorderColor(Colors.Grey.Medium)
                            .Padding(5)
                            .Text(node.InnerText.Trim())
                            .FontFamily("Courier")
                            .FontSize(isA6 ? 6 : 10);
                        break;

                    case "blockquote":
                        column.Item()
                            .PaddingLeft(15)
                            .BorderLeft(2)
                            .BorderColor(Colors.Grey.Medium)
                            .PaddingVertical(3)
                            .Text(node.InnerText.Trim())
                            .Italic();
                        break;

                    case "table":
                        ProcessTable(column, node, isA6);
                        break;

                    default:
                        // Process child nodes for unknown elements
                        foreach (var child in node.ChildNodes)
                        {
                            ProcessHtmlNode(column, child, isA6, indentLevel);
                        }
                        break;
                }
            }

            private void ProcessTable(ColumnDescriptor column, HtmlNode tableNode, bool isA6)
            {
                column.Item().Table(table =>
                {
                    // Find all rows
                    var rows = tableNode.SelectNodes(".//tr") ?? new HtmlNodeCollection(tableNode);

                    if (rows.Count == 0)
                        return;

                    // Determine number of columns from first row
                    var firstRowCells = rows[0].SelectNodes(".//th|.//td") ?? new HtmlNodeCollection(rows[0]);
                    int columnCount = firstRowCells.Count;

                    // Define columns with relative widths
                    table.ColumnsDefinition(columns =>
                    {
                        for (int i = 0; i < columnCount; i++)
                        {
                            columns.RelativeColumn();
                        }
                    });

                    // Add header if first row has th elements
                    bool hasHeader = rows[0].SelectNodes(".//th")?.Count > 0;

                    if (hasHeader)
                    {
                        table.Header(header =>
                        {
                            var headerCells = rows[0].SelectNodes(".//th");
                            foreach (var cell in headerCells)
                            {
                                header.Cell()
                                    .Background(Colors.Grey.Lighten3)
                                    .Border(0.5f)
                                    .Padding(isA6 ? 2 : 5)
                                    .Text(cell.InnerText.Trim())
                                    .FontSize(isA6 ? 7 : 10)
                                    .Bold();
                            }
                        });
                    }

                    // Add data rows
                    int startRow = hasHeader ? 1 : 0;
                    for (int rowIndex = startRow; rowIndex < rows.Count; rowIndex++)
                    {
                        var cells = rows[rowIndex].SelectNodes(".//td") ?? new HtmlNodeCollection(rows[rowIndex]);

                        foreach (var cell in cells)
                        {
                            table.Cell()
                                .Border(0.5f)
                                .Padding(isA6 ? 2 : 5)
                                .Text(cell.InnerText.Trim())
                                .FontSize(isA6 ? 7 : 10);
                        }
                    }
                });
            }

            // Helper method to generate PDF bytes directly
            public byte[] ConvertHtmlToPdfBytes(string html, bool isA6 = false)
            {
                var document = ConvertHtmlToDocument(html, isA6);
                return document.GeneratePdf();
            }

            // Helper method to generate PDF and save to file
            public void ConvertHtmlToPdfFile(string html, string outputPath, bool isA6 = false)
            {
                var document = ConvertHtmlToDocument(html, isA6);
                document.GeneratePdf(outputPath);
            }

            // Method optimized for ticket/receipt printing (A6)
            public byte[] GenerateTicket(string htmlContent)
            {
                return ConvertHtmlToPdfBytes(htmlContent, true);
            }

            // Method for multi-page A6 documents
            //public Document ConvertHtmlToMultiPageDocument(string html, bool isA6 = false)
            //{
            //    var doc = new HtmlDocument();
            //    doc.LoadHtml(html);

            //    return Document.Create(container =>
            //    {
            //        var currentColumn = container.Page(page =>
            //        {
            //            if (isA6)
            //            {
            //                page.Size(PageSizes.A6);
            //                page.Margin(0.5f, Unit.Centimetre);
            //                page.DefaultTextStyle(style => style.FontSize(8));
            //            }
            //            else
            //            {
            //                page.Size(PageSizes.A4);
            //                page.Margin(2, Unit.Centimetre);
            //                page.DefaultTextStyle(style => style.FontSize(11));
            //            }

            //            // Add page numbers for multi-page
            //            page.Footer()
            //                .AlignCenter()
            //                .Text(text =>
            //                {
            //                    text.Span("Page ");
            //                    text.CurrentPageNumber();
            //                    text.Span(" of ");
            //                    text.TotalPages();
            //                })
            //                .FontSize(isA6 ? 6 : 9);

            //            return page.Content().Column();
            //        });

            //        ProcessHtmlNode(currentColumn, doc.DocumentNode, isA6);
            //    });
            //}
        }
    }


    //public class HtmlToQuestPdfConverter
    //{
        //public QuestPDF.Fluent.Document ConvertHtmlToDocument(string html)
        //{
        //    QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
        //    var doc = new HtmlDocument();
        //    doc.LoadHtml(html);

        //    return QuestPDF.Fluent.Document.Create(container =>
        //    {
        //        container.Page(page =>
        //        {
        //            page.Content().Column(column =>
        //            {
        //                foreach (var node in doc.DocumentNode.ChildNodes)
        //                {
        //                    ProcessHtmlNode(column, node);
        //                }
        //            });
        //        });
        //    });
        //}

        //private void ProcessHtmlNode(ColumnDescriptor column, HtmlNode node)
        //{
        //    switch (node.Name.ToLower())
        //    {
        //        case "h1":
        //            column.Item().Text(node.InnerText).FontSize(24).Bold();
        //            break;
        //        case "p":
        //            column.Item().PaddingBottom(10).Text(node.InnerText);
        //            break;
        //        case "ul":
        //            column.Item().PaddingVertical(5).Column(listColumn =>
        //            {
        //                foreach (var li in node.SelectNodes("li"))
        //                {
        //                    listColumn.Item().Text($"• {li.InnerText}");
        //                }
        //            });
        //            break;
        //    }
        //}
    }
