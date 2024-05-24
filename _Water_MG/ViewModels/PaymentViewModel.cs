using _Water_MG.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace _Water_MG.ViewModels
{
    public class PaymentViewModel : ViewModelBase
    {
        private readonly WaterContext _dbContext;

        private int _idBill;
        public int BillId
        {
            get { return _idBill; }
            set { _idBill = value; OnPropertyChanged(nameof(BillId)); }
        }
        private decimal _amount;
        public decimal Amount
        {
            get { return _amount; }
            set { _amount = value; OnPropertyChanged(nameof(Amount)); }
        }
        private DateTime _date;
        public DateTime Date
        {
            get { return _date; }
            set { _date = value; OnPropertyChanged(nameof(Date)); }
        }
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

        private string _searchKeyword;
        public string SearchKeyword
        {
            get { return _searchKeyword; }
            set
            {
                _searchKeyword = value;
                OnPropertyChanged(nameof(SearchKeyword));
                FilterPayments();
            }
        }
        private void FilterPayments()
        {
            string keyword = SearchKeyword?.ToLower() ?? string.Empty;

            var filteredPayments = _dbContext.Payments
                .Where(p => p.BillId.ToString().Contains(keyword) || p.AmountPaid.ToString().Contains(keyword))
                .ToList();

            Payments.Clear();
            foreach (var pay in filteredPayments)
            {
                Payments.Add(pay);
            }
        }

        /*        LỌC - LỰA CHỌN TÌM KIẾM*/
        private bool CanExecuteSearchCommand(object obj)
        {
            return !string.IsNullOrWhiteSpace(SelectedPaymentType);
        }

        private void ExecuteSearchCommand(object obj)
        {
            FilterCustomersByAccountType();
        }

        private void FilterCustomersByAccountType()
        {
            // Lọc dữ liệu theo loại tài khoản được chọn từ ComboBox
            var filteredPays = _dbContext.Payments
                .Where(c => c.TypePay == SelectedPaymentType)
                .ToList();

            Payments.Clear();
            foreach (var pay in filteredPays)
            {
                Payments.Add(pay);
            }
        }
/**/

        private string _customerName;
        public string CustomerName
        {
            get { return _customerName; }
            set { _customerName = value; OnPropertyChanged(nameof(CustomerName)); }
        }
        public ObservableCollection<string> PaymentTypeItemsSource { get; } = new ObservableCollection<string>
        {
            "Null",
            "Chưa thanh toán",
            "Đã thanh toán"
        };

        private string _selectedPaymentType;
        public string SelectedPaymentType
        {
            get { return _selectedPaymentType; }
            set
            {
                _selectedPaymentType = value;
                OnPropertyChanged(nameof(SelectedPaymentType));
            }
        }

        public ICommand SearchCommand { get; }
        public ICommand DeleteCommand { get; }
        public PaymentViewModel()
        {
            _dbContext = new WaterContext();
            SearchCommand = new ViewModelCommand(ExecuteSearchCommand, CanExecuteSearchCommand);
            DeleteCommand = new ViewModelCommand(ExecuteDeleteCommand, CanExecuteDeleteCommand);
            Payments = new ObservableCollection<Payment>();
            LoadData();
        }

        public ObservableCollection<Payment> Payments { get; set; }
        private Payment _selectedPayment;
        public Payment SelectedPayment
        {
            get { return _selectedPayment; }
            set
            {
                _selectedPayment = value;
                OnPropertyChanged(nameof(SelectedPayment));
                if (SelectedPayment != null)
                {
                    BillId = _selectedPayment.BillId;
                    Date = _selectedPayment.PaymentDate;
                    Amount = _selectedPayment.AmountPaid;
                    LoadCustomerName(_selectedPayment.BillId);
                }
            }
        }

        private void LoadCustomerName(int billId)
        {
            var customerName = (from bill in _dbContext.Bills
                                join customer in _dbContext.Customers on bill.CustomerId equals customer.CustomerId
                                where bill.BillId == billId
                                select customer.Name).FirstOrDefault();

            CustomerName = customerName;
        }


        /* XÓA DỮ LIỆU */
        private bool CanExecuteDeleteCommand(object obj)
        {
            return SelectedPayment != null;
        }

        private void ExecuteDeleteCommand(object obj)
        {
            try
            {
                _dbContext.Payments.Remove(SelectedPayment);
                _dbContext.SaveChanges();

                Payments.Remove(SelectedPayment);
                SelectedPayment = null; 

                ErrorMessage = "Xóa thành công!";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Đã xảy ra lỗi khi xóa hóa đơn: {ex.Message}";
            }
        }


        /* TẢI LẠI DỮ LIỆU */
        private void LoadData()
        {
            Payments.Clear();
            foreach (var payment in _dbContext.Payments)
            {
                Payments.Add(payment);
            }
        }
    }
}
