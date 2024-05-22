using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using _Water_MG.Models;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using MigraDocCore.DocumentObjectModel;
using MigraDocCore.Rendering;
using MigraDocCore.DocumentObjectModel.Tables;
using System.Net.Mail;
using System.Net;
using System.Diagnostics.Metrics;

namespace _Water_MG.ViewModels
{
    public class BillViewModel : ViewModelBase
    {
        private readonly WaterContext _dbContext;
        private string _searchKeyword;
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
        public ObservableCollection<PdfContent> PdfContents { get; set; }

        public ICommand CreateBillCommand { get; }
        public ICommand GeneratePdfCommand { get; }
        public ICommand RequestPaymentCommand { get; }
        public ICommand DeleteBillCommand { get; }

        public BillViewModel()
        {
            _dbContext = new WaterContext();
            CreateBillCommand = new ViewModelCommand(ExecuteCreateBillCommand, CanExecuteCreateBillCommand);
            GeneratePdfCommand = new ViewModelCommand(ExecuteGeneratePdfCommand, CanExecuteGeneratePdfCommand);
            RequestPaymentCommand = new ViewModelCommand(ExecuteRequestPaymentCommand, CanExecuteRequestPaymentCommand);
            DeleteBillCommand = new ViewModelCommand(ExecuteDeleteBillCommand, CanExecuteDeleteBillCommand);
            Bills = new ObservableCollection<Bill>();
            PdfContents = new ObservableCollection<PdfContent>();
            LoadData();
        }
        /* TẠO HÓA ĐƠN */
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
/* TẢI LẠI DỮ LIỆU*/
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
                    LoadPdfContentForSelectedBill();
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
/* TẠP FILE PDF */
        private bool CanExecuteGeneratePdfCommand(object obj)
        {
            return SelectedBill != null;
        }

        private void ExecuteGeneratePdfCommand(object obj)
        {
            if (SelectedBill != null)
            {
                GeneratePdfForSelectedBill();
                LoadPdfContentForSelectedBill();
            }
        }
        private void GeneratePdfForSelectedBill()
        {
            try
            {
                string fileName = $"Bill_{SelectedBill.BillId}.pdf";
                string filePath = Path.Combine(Environment.CurrentDirectory, fileName);

                // Tạo tài liệu mới
                Document document = new Document();
                Section section = document.AddSection();

                // Thêm tiêu đề
                Paragraph paragraph = section.AddParagraph();
                paragraph.Format.Alignment = ParagraphAlignment.Center;
                paragraph.Format.Font.Size = 16;
                paragraph.Format.Font.Bold = true;
                paragraph.AddFormattedText($"Hóa Đơn: {SelectedBill.BillId}", TextFormat.Bold);

                // Thêm khoảng trắng
                section.AddParagraph().AddLineBreak();

                // Thêm thông tin khách hàng

                paragraph = section.AddParagraph();
                paragraph.Format.Alignment = ParagraphAlignment.Center;
                paragraph.Format.Font.Size = 12;
                paragraph.AddFormattedText("Vui lòng thanh toán trong vòng 3 ngày kể từ lúc nhận được mail này!", TextFormat.Bold);
                section.AddParagraph().AddLineBreak();
                section.AddParagraph().AddLineBreak();
                // Thêm bảng chi tiết
                Table table = section.AddTable();
                table.Borders.Width = 0.75;

                // Định nghĩa các cột
                Column column = table.AddColumn("4cm");
                column.Format.Alignment = ParagraphAlignment.Center;
                column = table.AddColumn("8cm");
                column.Format.Alignment = ParagraphAlignment.Center;

                // Thêm hàng tiêu đề
                Row row = table.AddRow();
                row.Shading.Color = Colors.LightGray;
                row.Cells[0].AddParagraph("Thông Tin");
                row.Cells[0].Format.Font.Bold = true;
                row.Cells[1].AddParagraph("Giá Trị");
                row.Cells[1].Format.Font.Bold = true;

                // Thêm các hàng dữ liệu
                row = table.AddRow();
                row.Cells[0].AddParagraph("Mã Khách Hàng");
                row.Cells[1].AddParagraph(SelectedBill.CustomerId.ToString());

                row = table.AddRow();
                row.Cells[0].AddParagraph("Ngày Lập");
                row.Cells[1].AddParagraph(SelectedBill.BillingDate.ToShortDateString());

                row = table.AddRow();
                row.Cells[0].AddParagraph("Số Tiền");
                row.Cells[1].AddParagraph($"{SelectedBill.AmountDue} VND");

                row = table.AddRow();
                row.Cells[0].AddParagraph("Trạng Thái");
                row.Cells[1].AddParagraph(SelectedBill.IsPaid ? "Đã Thanh Toán" : "Chưa Thanh Toán");

                // Render và lưu file PDF
                PdfDocumentRenderer pdfRenderer = new PdfDocumentRenderer(true);
                pdfRenderer.Document = document;
                pdfRenderer.RenderDocument();
                pdfRenderer.Save(filePath);

                ErrorMessage = $"Đã tạo file PDF: {filePath}";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Đã xảy ra lỗi khi tạo file PDF: {ex.Message}";
            }
        }


        private void LoadPdfContentForSelectedBill()
        {
            try
            {
                string fileName = $"Bill_{SelectedBill.BillId}.pdf";
                string filePath = Path.Combine(Environment.CurrentDirectory, fileName);

                if (File.Exists(filePath))
                {
                    using (PdfReader pdfReader = new PdfReader(filePath))
                    {
                        using (PdfDocument pdfDoc = new PdfDocument(pdfReader))
                        {
                            PdfContents.Clear();
                            for (int i = 1; i <= pdfDoc.GetNumberOfPages(); i++)
                            {
                                var page = pdfDoc.GetPage(i);
                                string text = PdfTextExtractor.GetTextFromPage(page);
                                PdfContents.Add(new PdfContent
                                {
                                    PageNumber = i,
                                    Content = text
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Đã xảy ra lỗi khi đọc file PDF: {ex.Message}";
            }
        }
/* YÊU CẦU THANH TOÁN */
        private bool CanExecuteRequestPaymentCommand(object obj)
        {
            return SelectedBill != null;
        }

        private void ExecuteRequestPaymentCommand(object obj)
        {
            if (SelectedBill != null)
            {
                SendPaymentRequestEmail();
            }
        }

        private void SendPaymentRequestEmail()
        {
            try
            {
                string customerEmail = "";
                using (var dbContext = new WaterContext())
                {
                    var customer = dbContext.Customers.FirstOrDefault(c => c.CustomerId == SelectedBill.CustomerId);
                    if (customer != null)
                    {
                        customerEmail = customer.Email;
                    }
                }

                // Nếu không có địa chỉ email, hiển thị thông báo lỗi
                if (string.IsNullOrWhiteSpace(customerEmail))
                {
                    ErrorMessage = "Không thể gửi email vì địa chỉ email của khách hàng không được cung cấp.";
                    return;
                }

                string fileName = $"Bill_{SelectedBill.BillId}.pdf";
                string filePath = Path.Combine(Environment.CurrentDirectory, fileName);

                // Tạo và cấu hình email
                MailMessage mail = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");
                SmtpServer.EnableSsl = true;

                mail.From = new MailAddress("thehieu0814@gmail.com"); // Địa chỉ email của bạn
                mail.To.Add(customerEmail);
                mail.Subject = "Yêu cầu thanh toán hóa đơn";
                mail.Body = "Kính gửi quý khách, đính kèm là hóa đơn cần thanh toán.";

                // Đính kèm file pdf
                Attachment attachment = new Attachment(filePath);
                mail.Attachments.Add(attachment);

                // Đăng nhập vào email của bạn
                SmtpServer.Port = 587;
                SmtpServer.Credentials = new NetworkCredential("thehieu0814@gmail.com", "gsok hvno bndx vviw"); 

                // Gửi email
                SmtpServer.Send(mail);

                ErrorMessage = "Email yêu cầu thanh toán đã được gửi thành công.";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Đã xảy ra lỗi khi gửi email: {ex.Message}";
            }
        }
/* XÓA HÓA ĐƠN */
        private bool CanExecuteDeleteBillCommand(object obj)
        {
            return SelectedBill != null;
        }

        private void ExecuteDeleteBillCommand(object obj)
        {
            try
            {
                _dbContext.Bills.Remove(SelectedBill);
                _dbContext.SaveChanges();

                // Xóa file PDF tương ứng
                string fileName = $"Bill_{SelectedBill.BillId}.pdf";
                string filePath = Path.Combine(Environment.CurrentDirectory, fileName);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                // Xóa hóa đơn khỏi danh sách hiển thị
                Bills.Remove(SelectedBill);
                SelectedBill = null; // Đặt SelectedBill về null để cập nhật giao diện

                ErrorMessage = "Hóa đơn đã được xóa thành công.";
                PdfContents.Clear();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Đã xảy ra lỗi khi xóa hóa đơn: {ex.Message}";
            }
        }
        /* TÌM KIẾM HÓA ĐƠN */
        public string SearchKeyword
        {
            get { return _searchKeyword; }
            set
            {
                _searchKeyword = value;
                OnPropertyChanged(nameof(SearchKeyword)); // Kích hoạt sự kiện PropertyChanged
                FilterCustomers(); // Gọi phương thức lọc dữ liệu khi từ khóa tìm kiếm thay đổi
            }
        }
        private void FilterCustomers()
        {
            string keyword = SearchKeyword?.ToLower() ?? string.Empty;

            var filteredBills = _dbContext.Bills
                .Where(b => b.BillId.ToString().Contains(keyword) || b.CustomerId.ToString().Contains(keyword))
                .ToList();

            Bills.Clear();
            foreach (var bill in filteredBills)
            {
                Bills.Add(bill);
            }
        }

    }



    public class PdfContent
    {
        public int PageNumber { get; set; }
        public string Content { get; set; }
    }
}
