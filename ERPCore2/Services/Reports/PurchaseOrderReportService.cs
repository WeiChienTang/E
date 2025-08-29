using ERPCore2.Data.Entities;
using ERPCore2.Models;
using ERPCore2.Services;

namespace ERPCore2.Services.Reports
{
    /// <summary>
    /// 採購單報表服務實作
    /// </summary>
    public class PurchaseOrderReportService : IPurchaseOrderReportService
    {
        private readonly IReportService _reportService;
        private readonly IPurchaseOrderService _purchaseOrderService;
        private readonly ISupplierService _supplierService;
        private readonly IProductService _productService;
        private readonly ICompanyService _companyService;

        public PurchaseOrderReportService(
            IReportService reportService,
            IPurchaseOrderService purchaseOrderService,
            ISupplierService supplierService,
            IProductService productService,
            ICompanyService companyService)
        {
            _reportService = reportService;
            _purchaseOrderService = purchaseOrderService;
            _supplierService = supplierService;
            _productService = productService;
            _companyService = companyService;
        }

        public async Task<string> GeneratePurchaseOrderReportAsync(int purchaseOrderId, ReportFormat format = ReportFormat.Html)
        {
            try
            {
                // 載入採購單資料
                var purchaseOrder = await _purchaseOrderService.GetByIdAsync(purchaseOrderId);
                if (purchaseOrder == null)
                {
                    throw new ArgumentException($"找不到採購單 ID: {purchaseOrderId}");
                }

                // 載入採購單明細
                var orderDetails = await _purchaseOrderService.GetOrderDetailsAsync(purchaseOrderId);

                // 載入相關資料
                Supplier? supplier = null;
                if (purchaseOrder.SupplierId > 0)
                {
                    supplier = await _supplierService.GetByIdAsync(purchaseOrder.SupplierId);
                }

                // 載入公司資料
                Company? company = null;
                if (purchaseOrder.CompanyId > 0)
                {
                    company = await _companyService.GetByIdAsync(purchaseOrder.CompanyId);
                }

                // 載入商品資料並建立字典以便快速查詢
                var allProducts = await _productService.GetAllAsync();
                var productDict = allProducts.ToDictionary(p => p.Id, p => p);

                // 建立報表資料
                var reportData = new ReportData<PurchaseOrder, PurchaseOrderDetail>
                {
                    MainEntity = purchaseOrder,
                    DetailEntities = orderDetails,
                    AdditionalData = new Dictionary<string, object>
                    {
                        { "SupplierName", supplier?.CompanyName ?? "未指定" },
                        { "SupplierContactPerson", supplier?.ContactPerson ?? "" },
                        { "CompanyName", company?.CompanyName ?? "未指定" },
                        { "CompanyAddress", company?.Address ?? "" },
                        { "CompanyPhone", company?.Phone ?? "" },
                        { "ProductDict", productDict },
                        { "DetailCount", orderDetails?.Count ?? 0 }
                    }
                };

                // 取得報表配置
                var configuration = GetPurchaseOrderReportConfiguration(company);
                
                // 動態設置明細筆數
                var detailCountField = configuration.FooterSections
                    .FirstOrDefault(s => s.Title == "合計資訊")?.Fields
                    .FirstOrDefault(f => f.Label == "明細筆數");
                if (detailCountField != null)
                {
                    detailCountField.Value = (orderDetails?.Count ?? 0).ToString();
                }

                // 根據格式生成報表
                return format switch
                {
                    ReportFormat.Html => await _reportService.GenerateHtmlReportAsync(configuration, reportData),
                    ReportFormat.Excel => throw new NotImplementedException("Excel 格式尚未實作"),
                    _ => throw new ArgumentException($"不支援的報表格式: {format}")
                };
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"生成採購單報表時發生錯誤: {ex.Message}", ex);
            }
        }

        public ReportConfiguration GetPurchaseOrderReportConfiguration(Company? company = null)
        {
            return new ReportConfiguration
            {
                Title = "採購單",
                CompanyName = company?.CompanyName ?? "貴公司名稱", // 使用實際公司名稱
                CompanyAddress = company?.Address ?? "公司地址", // 使用實際公司地址
                CompanyPhone = company?.Phone ?? "公司電話", // 使用實際公司電話
                Orientation = PageOrientation.Portrait,
                PageSize = PageSize.ContinuousForm,
                
                // 頁首區段
                HeaderSections = new List<ReportHeaderSection>
                {
                    new ReportHeaderSection
                    {
                        Title = "",
                        Order = 1,
                        FieldsPerRow = 3,
                        Fields = new List<ReportField>
                        {
                            new ReportField
                            {
                                Label = "採購單號",
                                PropertyName = nameof(PurchaseOrder.PurchaseOrderNumber),
                                IsBold = true
                            },
                            new ReportField
                            {
                                Label = "採購日期",
                                PropertyName = nameof(PurchaseOrder.OrderDate),
                                Format = "yyyy/MM/dd"
                            },
                            new ReportField
                            {
                                Label = "預定交貨日期",
                                PropertyName = nameof(PurchaseOrder.ExpectedDeliveryDate),
                                Format = "yyyy/MM/dd"
                            }
                        }
                    },
                    new ReportHeaderSection
                    {
                        Title = "",
                        Order = 2,
                        FieldsPerRow = 2,
                        Fields = new List<ReportField>
                        {
                            new ReportField
                            {
                                Label = "供應商名稱",
                                PropertyName = "SupplierName",
                                IsBold = true
                            },
                            new ReportField
                            {
                                Label = "聯絡人",
                                PropertyName = "SupplierContactPerson"
                            }
                        }
                    }
                },
                
                // 明細表格欄位
                Columns = new List<ReportColumnDefinition>
                {
                    new ReportColumnDefinition
                    {
                        Header = "序號",
                        PropertyName = "",
                        Width = "4%",
                        Alignment = TextAlignment.Center,
                        Order = 1
                        // 序號會在渲染時動態生成
                    },
                    new ReportColumnDefinition
                    {
                        Header = "商品名稱",
                        PropertyName = nameof(PurchaseOrderDetail.ProductId),
                        Width = "20%",
                        Alignment = TextAlignment.Left,
                        Order = 2
                        // 商品名稱會透過 ProductDict 在渲染時解析
                    },
                    new ReportColumnDefinition
                    {
                        Header = "採購數量",
                        PropertyName = nameof(PurchaseOrderDetail.OrderQuantity),
                        Width = "5%",
                        Alignment = TextAlignment.Right,
                        Format = "N0",
                        Order = 3
                    },
                    new ReportColumnDefinition
                    {
                        Header = "單位",
                        PropertyName = "Unit",
                        Width = "4%",
                        Alignment = TextAlignment.Center,
                        Order = 4,
                        ValueGenerator = (detail) => "個" // 暫時固定值，未來可擴展
                    },
                    new ReportColumnDefinition
                    {
                        Header = "單價",
                        PropertyName = nameof(PurchaseOrderDetail.UnitPrice),
                        Width = "10%",
                        Alignment = TextAlignment.Right,
                        Format = "N2",
                        Order = 5
                    },
                    new ReportColumnDefinition
                    {
                        Header = "小計",
                        PropertyName = nameof(PurchaseOrderDetail.SubtotalAmount),
                        Width = "10%",
                        Alignment = TextAlignment.Right,
                        Format = "N2",
                        Order = 6
                    },
                    new ReportColumnDefinition
                    {
                        Header = "備註",
                        PropertyName = nameof(PurchaseOrderDetail.Remarks),
                        Width = "20%",
                        Alignment = TextAlignment.Left,
                        Order = 7
                    }
                },
                
                // 頁尾區段
                FooterSections = new List<ReportFooterSection>
                {
                    new ReportFooterSection
                    {
                        Title = "合計資訊",
                        Order = 1,
                        FieldsPerRow = 2,
                        IsStatisticsSection = true,
                        Fields = new List<ReportField>
                        {
                            new ReportField
                            {
                                Label = "總金額",
                                PropertyName = nameof(PurchaseOrder.TotalAmount),
                                Format = "N2",
                                IsBold = true
                            },
                            new ReportField
                            {
                                Label = "明細筆數",
                                Value = "", // 需要動態計算
                                IsBold = true
                            }
                        }
                    }
                }
            };
        }
    }
}
