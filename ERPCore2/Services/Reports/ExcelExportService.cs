using ClosedXML.Excel;
using ERPCore2.Models.Reports;
using ERPCore2.Services.Reports.Interfaces;

namespace ERPCore2.Services.Reports
{
    /// <summary>
    /// Excel 匯出服務實作
    /// 使用 ClosedXML 將 FormattedDocument 轉換為 Excel 檔案
    /// </summary>
    public class ExcelExportService : IExcelExportService
    {
        /// <summary>
        /// 將 FormattedDocument 匯出為 Excel 檔案
        /// </summary>
        public byte[] ExportToExcel(FormattedDocument document)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add(SanitizeSheetName(document.DocumentName));

            int currentRow = 1;

            // 處理頁首元素
            foreach (var element in document.HeaderElements)
            {
                currentRow = RenderElement(worksheet, element, currentRow);
            }

            // 處理主要內容元素
            foreach (var element in document.Elements)
            {
                currentRow = RenderElement(worksheet, element, currentRow);
            }

            // 處理頁尾元素
            foreach (var element in document.FooterElements)
            {
                currentRow = RenderElement(worksheet, element, currentRow);
            }

            // 自動調整欄寬
            worksheet.Columns().AdjustToContents(minWidth: 5, maxWidth: 50);

            // 輸出為 byte[]
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        /// <summary>
        /// 將 FormattedDocument 匯出為 Excel 檔案（非同步版本）
        /// </summary>
        public Task<byte[]> ExportToExcelAsync(FormattedDocument document)
        {
            return Task.Run(() => ExportToExcel(document));
        }

        /// <summary>
        /// 檢查服務是否可用
        /// </summary>
        public bool IsSupported() => true;

        #region 元素渲染方法

        /// <summary>
        /// 渲染單一元素到工作表
        /// </summary>
        private int RenderElement(IXLWorksheet worksheet, PageElement element, int startRow)
        {
            return element switch
            {
                TextElement text => RenderTextElement(worksheet, text, startRow),
                TableElement table => RenderTableElement(worksheet, table, startRow),
                KeyValueRowElement kvRow => RenderKeyValueRowElement(worksheet, kvRow, startRow),
                ThreeColumnHeaderElement header => RenderThreeColumnHeaderElement(worksheet, header, startRow),
                ReportHeaderBlockElement headerBlock => RenderReportHeaderBlockElement(worksheet, headerBlock, startRow),
                TwoColumnSectionElement twoCol => RenderTwoColumnSectionElement(worksheet, twoCol, startRow),
                LineElement => RenderLineElement(worksheet, startRow),
                SpacingElement => startRow + 1, // 空白列
                SignatureSectionElement sig => RenderSignatureSectionElement(worksheet, sig, startRow),
                PageBreakElement => RenderPageBreakElement(worksheet, startRow),
                ImageElement => startRow + 1, // 圖片暫不支援，跳過
                _ => startRow
            };
        }

        /// <summary>
        /// 渲染文字元素
        /// </summary>
        private int RenderTextElement(IXLWorksheet worksheet, TextElement element, int row)
        {
            var cell = worksheet.Cell(row, 1);
            cell.Value = element.Text;

            // 設定樣式
            cell.Style.Font.FontSize = element.FontSize;
            cell.Style.Font.Bold = element.Bold;
            cell.Style.Font.Italic = element.Italic;

            // 設定對齊方式
            cell.Style.Alignment.Horizontal = element.Alignment switch
            {
                TextAlignment.Left => XLAlignmentHorizontalValues.Left,
                TextAlignment.Center => XLAlignmentHorizontalValues.Center,
                TextAlignment.Right => XLAlignmentHorizontalValues.Right,
                _ => XLAlignmentHorizontalValues.Left
            };

            // 合併儲存格以模擬全寬文字
            worksheet.Range(row, 1, row, 10).Merge();

            return row + 1;
        }

        /// <summary>
        /// 渲染表格元素
        /// </summary>
        private int RenderTableElement(IXLWorksheet worksheet, TableElement table, int startRow)
        {
            int row = startRow;
            int colCount = table.Columns.Count;

            if (colCount == 0) return startRow;

            // 渲染表頭
            for (int col = 0; col < colCount; col++)
            {
                var cell = worksheet.Cell(row, col + 1);
                cell.Value = table.Columns[col].Header;
                cell.Style.Font.Bold = true;
                cell.Style.Alignment.Horizontal = GetXLAlignment(table.Columns[col].Alignment);

                if (table.ShowHeaderBackground)
                {
                    cell.Style.Fill.BackgroundColor = XLColor.LightGray;
                }

                if (table.ShowBorder)
                {
                    cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                }
            }
            row++;

            // 渲染資料列
            foreach (var dataRow in table.Rows)
            {
                for (int col = 0; col < Math.Min(dataRow.Cells.Count, colCount); col++)
                {
                    var cell = worksheet.Cell(row, col + 1);
                    var cellValue = dataRow.Cells[col];

                    // 嘗試解析為數字
                    if (decimal.TryParse(cellValue.Replace(",", ""), out decimal numericValue))
                    {
                        cell.Value = numericValue;
                        cell.Style.NumberFormat.Format = "#,##0.##";
                    }
                    else
                    {
                        cell.Value = cellValue;
                    }

                    cell.Style.Alignment.Horizontal = GetXLAlignment(table.Columns[col].Alignment);

                    if (table.ShowBorder)
                    {
                        cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    }
                }
                row++;
            }

            return row;
        }

        /// <summary>
        /// 渲染鍵值對行
        /// </summary>
        private int RenderKeyValueRowElement(IXLWorksheet worksheet, KeyValueRowElement element, int row)
        {
            int col = 1;
            foreach (var (key, value) in element.Pairs)
            {
                var keyCell = worksheet.Cell(row, col);
                keyCell.Value = $"{key}：";
                keyCell.Style.Font.Bold = true;
                col++;

                var valueCell = worksheet.Cell(row, col);
                valueCell.Value = value;
                col++;
            }

            return row + 1;
        }

        /// <summary>
        /// 渲染三欄標頭
        /// </summary>
        private int RenderThreeColumnHeaderElement(IXLWorksheet worksheet, ThreeColumnHeaderElement element, int row)
        {
            // 左側
            var leftCell = worksheet.Cell(row, 1);
            leftCell.Value = element.LeftText;
            leftCell.Style.Font.FontSize = element.SideFontSize;
            leftCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            worksheet.Range(row, 1, row, 3).Merge();

            // 中間
            var centerCell = worksheet.Cell(row, 4);
            centerCell.Value = element.CenterText;
            centerCell.Style.Font.FontSize = element.CenterFontSize;
            centerCell.Style.Font.Bold = element.CenterBold;
            centerCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Range(row, 4, row, 7).Merge();

            // 右側
            var rightCell = worksheet.Cell(row, 8);
            rightCell.Value = element.RightText;
            rightCell.Style.Font.FontSize = element.SideFontSize;
            rightCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            worksheet.Range(row, 8, row, 10).Merge();

            return row + 1;
        }

        /// <summary>
        /// 渲染報表標頭區塊
        /// </summary>
        private int RenderReportHeaderBlockElement(IXLWorksheet worksheet, ReportHeaderBlockElement element, int startRow)
        {
            int row = startRow;
            int maxLines = Math.Max(element.CenterLines.Count, element.RightLines.Count);

            for (int i = 0; i < maxLines; i++)
            {
                // 中間標題
                if (i < element.CenterLines.Count)
                {
                    var (text, fontSize, bold) = element.CenterLines[i];
                    var centerCell = worksheet.Cell(row, 4);
                    centerCell.Value = text;
                    centerCell.Style.Font.FontSize = fontSize;
                    centerCell.Style.Font.Bold = bold;
                    centerCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Range(row, 4, row, 7).Merge();
                }

                // 右側資訊
                if (i < element.RightLines.Count)
                {
                    var rightCell = worksheet.Cell(row, 8);
                    rightCell.Value = element.RightLines[i];
                    rightCell.Style.Font.FontSize = element.RightFontSize;
                    rightCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    worksheet.Range(row, 8, row, 10).Merge();
                }

                row++;
            }

            return row;
        }

        /// <summary>
        /// 渲染左右並排區塊
        /// </summary>
        private int RenderTwoColumnSectionElement(IXLWorksheet worksheet, TwoColumnSectionElement element, int startRow)
        {
            int row = startRow;

            // 左側標題
            if (!string.IsNullOrEmpty(element.LeftTitle))
            {
                var leftTitleCell = worksheet.Cell(row, 1);
                leftTitleCell.Value = element.LeftTitle;
                leftTitleCell.Style.Font.Bold = true;
                worksheet.Range(row, 1, row, 5).Merge();

                // 右側標題
                if (!string.IsNullOrEmpty(element.RightTitle))
                {
                    var rightTitleCell = worksheet.Cell(row, 6);
                    rightTitleCell.Value = element.RightTitle;
                    rightTitleCell.Style.Font.Bold = true;
                    worksheet.Range(row, 6, row, 10).Merge();
                }
                row++;
            }

            // 內容
            int maxLines = Math.Max(element.LeftContent.Count, element.RightContent.Count);
            for (int i = 0; i < maxLines; i++)
            {
                if (i < element.LeftContent.Count)
                {
                    var leftCell = worksheet.Cell(row, 1);
                    leftCell.Value = element.LeftContent[i];
                    worksheet.Range(row, 1, row, 5).Merge();

                    if (element.LeftHasBorder)
                    {
                        worksheet.Range(row, 1, row, 5).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    }
                }

                if (i < element.RightContent.Count)
                {
                    var rightCell = worksheet.Cell(row, 6);
                    rightCell.Value = element.RightContent[i];
                    worksheet.Range(row, 6, row, 10).Merge();

                    if (element.RightHasBorder)
                    {
                        worksheet.Range(row, 6, row, 10).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    }
                }

                row++;
            }

            return row;
        }

        /// <summary>
        /// 渲染線條元素（使用底線模擬）
        /// </summary>
        private int RenderLineElement(IXLWorksheet worksheet, int row)
        {
            var range = worksheet.Range(row, 1, row, 10);
            range.Merge();
            range.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            return row + 1;
        }

        /// <summary>
        /// 渲染簽名區
        /// </summary>
        private int RenderSignatureSectionElement(IXLWorksheet worksheet, SignatureSectionElement element, int startRow)
        {
            int row = startRow + 1; // 空一行

            int labelCount = element.Labels.Count;
            if (labelCount == 0) return row;

            // 計算每個簽名區的欄位寬度
            int colsPerLabel = 10 / labelCount;
            int col = 1;

            foreach (var label in element.Labels)
            {
                var cell = worksheet.Cell(row, col);
                cell.Value = $"{label}：＿＿＿＿＿＿＿＿";
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                
                int endCol = Math.Min(col + colsPerLabel - 1, 10);
                worksheet.Range(row, col, row, endCol).Merge();
                col = endCol + 1;
            }

            return row + 2; // 簽名區後空一行
        }

        /// <summary>
        /// 渲染分頁符號
        /// </summary>
        private int RenderPageBreakElement(IXLWorksheet worksheet, int row)
        {
            worksheet.PageSetup.AddHorizontalPageBreak(row);
            return row;
        }

        #endregion

        #region 輔助方法

        /// <summary>
        /// 清理工作表名稱（移除不允許的字元）
        /// </summary>
        private static string SanitizeSheetName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "Report";

            // Excel 工作表名稱不允許的字元：: \ / ? * [ ]
            var invalidChars = new[] { ':', '\\', '/', '?', '*', '[', ']' };
            var sanitized = name;
            foreach (var c in invalidChars)
            {
                sanitized = sanitized.Replace(c, '_');
            }

            // 最大長度 31 字元
            if (sanitized.Length > 31)
            {
                sanitized = sanitized[..31];
            }

            return sanitized;
        }

        /// <summary>
        /// 轉換對齊方式
        /// </summary>
        private static XLAlignmentHorizontalValues GetXLAlignment(TextAlignment alignment)
        {
            return alignment switch
            {
                TextAlignment.Left => XLAlignmentHorizontalValues.Left,
                TextAlignment.Center => XLAlignmentHorizontalValues.Center,
                TextAlignment.Right => XLAlignmentHorizontalValues.Right,
                _ => XLAlignmentHorizontalValues.Left
            };
        }

        #endregion
    }
}
