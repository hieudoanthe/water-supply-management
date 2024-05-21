using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using _Water_MG.Models;
using System.IO;
using MigraDocCore.DocumentObjectModel;
using MigraDocCore.Rendering;
using System.Reflection;
using PdfSharp.Fonts;

namespace _Water_MG.ViewModels
{
    public class BillViewModel : ViewModelBase
    {
        private readonly WaterContext _dbContext;

        private string _errorMessage;
        public string ErrorMessage
        {
            get { return _errorMessage; }
            set
            {
                _errorMessage = value;
                OnPropertyChanged(nameof(ErrorMessage));
            }
        }

        private int _billId;
        public int BillId
        {
            get { return _billId; }
            set
            {
                _billId = value;
                OnPropertyChanged(nameof(BillId));
            }
        }
        private int _idKH;
        public int IdKH
        {
            get { return _idKH; }
            set
            {
                _idKH = value;
                OnPropertyChanged(nameof(IdKH));
            }
        }
        private DateTime _billingDate;
        public DateTime BillingDate
        {
            get { return _billingDate; }
            set
            {
                _billingDate = value;
                OnPropertyChanged(nameof(BillingDate));
            }
        }
        private decimal _billingAmount;
        public decimal BillingAmount
        {
            get { return _billingAmount; }
            set
            {
                _billingAmount = value;
                OnPropertyChanged(nameof(BillingAmount));
            }
        }
        public ObservableCollection<Bill> Bills { get; set; }
        public ObservableCollection<string> PdfFiles { get; set; }

        public ICommand CreateBillCommand { get; }
        public ICommand GeneratePdfCommand { get; }

        public BillViewModel()
        {
            _dbContext = new WaterContext();
            CreateBillCommand = new ViewModelCommand(ExecuteCreateBillCommand, CanExecuteCreateBillCommand);
            GeneratePdfCommand = new ViewModelCommand(ExecuteGeneratePdfCommand, CanExecuteGeneratePdfCommand);
            Bills = new ObservableCollection<Bill>();
            PdfFiles = new ObservableCollection<string>();
            LoadData();
        }

        private bool CanExecuteCreateBillCommand(object obj)
        {
            return true;
        }

        private void ExecuteCreateBillCommand(object obj)
        {
            try
            {
                var customers = _dbContext.Customers.ToList();

                foreach (var customer in customers)
                {
                    var existingBill = _dbContext.Bills.FirstOrDefault(b => b.CustomerId == customer.CustomerId);

                    if (existingBill != null)
                    {
                        var meters = _dbContext.Meters
                            .Where(m => m.CustomerId == customer.CustomerId && m.TypeMeter != "Tạm khóa")
                            .ToList();

                        decimal totalWaterUsage = meters.Sum(m => m.LastReadingValue ?? 0);
                        decimal amountDue = CalculateAmountDue(totalWaterUsage);

                        existingBill.BillingDate = DateTime.Now;
                        existingBill.AmountDue = amountDue;
                        existingBill.IsPaid = false;
                    }
                    else
                    {
                        var meters = _dbContext.Meters
                            .Where(m => m.CustomerId == customer.CustomerId && m.TypeMeter != "Tạm khóa")
                            .ToList();

                        decimal totalWaterUsage = meters.Sum(m => m.LastReadingValue ?? 0);
                        decimal amountDue = CalculateAmountDue(totalWaterUsage);

                        if (totalWaterUsage > 0)
                        {
                            var bill = new Bill
                            {
                                CustomerId = customer.CustomerId,
                                BillingDate = DateTime.Now,
                                AmountDue = amountDue,
                                IsPaid = false
                            };

                            _dbContext.Bills.Add(bill);
                            Bills.Add(bill);
                        }
                    }
                }
                _dbContext.SaveChanges();
                ErrorMessage = "Đã tạo hoặc cập nhật hóa đơn cho tất cả khách hàng thành công.";
                LoadData();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Đã xảy ra lỗi: {ex.Message}";
            }
        }

        private decimal CalculateAmountDue(decimal totalWaterUsage)
        {
            decimal ratePerUnit = 5000;
            return totalWaterUsage * ratePerUnit;
        }

        private Bill _selectedBill;
        public Bill SelectedBill
        {
            get { return _selectedBill; }
            set
            {
                _selectedBill = value;
                OnPropertyChanged(nameof(SelectedBill));
                if (_selectedBill != null)
                {
                    BillId = _selectedBill.BillId;
                    IdKH = _selectedBill.CustomerId;
                    BillingAmount = _selectedBill.AmountDue;
                    BillingDate = _selectedBill.BillingDate;
                    GeneratePdfForSelectedBill();
                }
            }
        }

        private void LoadData()
        {
            Bills.Clear();
            foreach (var bill in _dbContext.Bills)
            {
                Bills.Add(bill);
            }
        }

        private bool CanExecuteGeneratePdfCommand(object obj)
        {
            return SelectedBill != null;
        }

        private void ExecuteGeneratePdfCommand(object obj)
        {
            if (SelectedBill != null)
            {
                GeneratePdfForSelectedBill();
            }
        }

        private void GeneratePdfForSelectedBill()
        {
            try
            {
                string fileName = $"Bill_{SelectedBill.BillId}.pdf";
                string filePath = Path.Combine(Environment.CurrentDirectory, fileName);

                Document document = new Document();
                Section section = document.AddSection();

                // Add document content
                var paragraph = section.AddParagraph($"Hóa Đơn: {SelectedBill.BillId}");
                paragraph.Format.Font.Name = "Times New Roman";
                paragraph.Format.Font.Size = 12;

                paragraph = section.AddParagraph($"Mã Khách Hàng: {SelectedBill.CustomerId}");
                paragraph.Format.Font.Name = "Times New Roman";
                paragraph.Format.Font.Size = 12;

                paragraph = section.AddParagraph($"Ngày Lập: {SelectedBill.BillingDate.ToShortDateString()}");
                paragraph.Format.Font.Name = "Times New Roman";
                paragraph.Format.Font.Size = 12;

                paragraph = section.AddParagraph($"Số Tiền: {SelectedBill.AmountDue} VND");
                paragraph.Format.Font.Name = "Times New Roman";
                paragraph.Format.Font.Size = 12;

                paragraph = section.AddParagraph($"Trạng Thái: {(SelectedBill.IsPaid ? "Đã Thanh Toán" : "Chưa Thanh Toán")}");
                paragraph.Format.Font.Name = "Times New Roman";
                paragraph.Format.Font.Size = 12;

                // Embed font
                GlobalFontSettings.FontResolver = new FontResolver();

                PdfDocumentRenderer pdfRenderer = new PdfDocumentRenderer(true);
                pdfRenderer.Document = document;
                pdfRenderer.RenderDocument();
                pdfRenderer.Save(filePath);

                PdfFiles.Add(filePath);
                ErrorMessage = $"Đã tạo file PDF: {filePath}";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Đã xảy ra lỗi khi tạo file PDF: {ex.Message}";
            }
        }

        public class FontResolver : IFontResolver
        {
            public FontResolver()
            {
                // Load the font from resources
                var assembly = Assembly.GetExecutingAssembly();
                using (var stream = assembly.GetManifestResourceStream("_Water_MG.Styles.Times_New_Roman.ttf"))
                {
                    if (stream != null)
                    {
                        using (var ms = new MemoryStream())
                        {
                            stream.CopyTo(ms);
                            _font = ms.ToArray();
                        }
                    }
                }
            }

            private readonly byte[] _font;

            public byte[] GetFont(string faceName)
            {
                if (faceName == "Times New Roman")
                {
                    return _font;
                }

                return null;
            }

            public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
            {
                if (familyName == "Times New Roman")
                {
                    return new FontResolverInfo("Times New Roman");
                }

                return null;
            }
        }
    }
}
