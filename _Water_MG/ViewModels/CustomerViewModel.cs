using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Security;
using System.Windows.Input;
using _Water_MG.Models;

namespace _Water_MG.ViewModels
{
    public class CustomerViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string _username;
        private SecureString _password;
        private string _fullName;
        private string _address;
        private string _phonenumber;
        private string _email;
        private int _idkh;
        private string _searchKeyword;
        private string _errorMessage;
        private bool _isViewVisible = true;
        private Customer _selectedCustomer;
        private readonly WaterContext _dbContext;

        // Properties
        public string Username
        {
            get { return _username; }
            set
            {
                _username = value;
                OnPropertyChanged(nameof(Username));
            }
        }

        public SecureString Password
        {
            get { return _password; }
            set
            {
                _password = value;
                OnPropertyChanged(nameof(Password));
            }
        }

        public string FullName
        {
            get { return _fullName; }
            set
            {
                _fullName = value;
                OnPropertyChanged(nameof(FullName));
            }
        }

        public string Address
        {
            get { return _address; }
            set
            {
                _address = value;
                OnPropertyChanged(nameof(Address));
            }
        }
        public string Email
        {
            get => _email;
            set 
            { 
              _email = value;
               OnPropertyChanged(nameof(Email));
            }
        }

        public string PhoneNumber
        {
            get { return _phonenumber; }
            set {
                _phonenumber = value; 
                OnPropertyChanged(nameof(PhoneNumber));
            }
        }

        public int idKH
        {
            get { return _idkh; }
            set
            {
                _idkh = value;
                OnPropertyChanged($"IdKH");
            }
            
        }
        public string SearchKeyword
        {
            get { return _searchKeyword; }
            set
            {
                _searchKeyword = value;
                OnPropertyChanged(nameof(SearchKeyword));
                FilterCustomers();
            }
        }

        public string ErrorMessage
        {
            get { return _errorMessage; }
            set
            {
                _errorMessage = value;
                OnPropertyChanged(nameof(ErrorMessage));
            }
        }

        public bool IsViewVisible
        {
            get { return _isViewVisible; }
            set
            {
                _isViewVisible = value;
                OnPropertyChanged(nameof(IsViewVisible));
            }
        }

        public ObservableCollection<string> AccountTypeItemsSource { get; } = new ObservableCollection<string>
        {
            "Doanh nghiệp",
            "Hộ gia đình"
        };

        private string _selectedAccountType;
        public string SelectedAccountType
        {
            get { return _selectedAccountType; }
            set
            {
                _selectedAccountType = value;
                OnPropertyChanged(nameof(SelectedAccountType));
            }
        }

        public ObservableCollection<Customer> Customers { get; set; }

        public Customer SelectedCustomer
        {
            get { return _selectedCustomer; }
            set
            {
                _selectedCustomer = value;
                OnPropertyChanged(nameof(SelectedCustomer));
                if (_selectedCustomer != null)
                {
                    FullName = _selectedCustomer.Name;
                    PhoneNumber = _selectedCustomer.PhoneNumber;
                    Email = _selectedCustomer.Email;
                    Address = _selectedCustomer.Address;
                    idKH = _selectedCustomer.CustomerId;
                    // Populate other fields as needed
                }
            }
        }

        public ICommand RegisterCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand UpdateCommand { get; }
        public ICommand SearchCommand { get; }
        // Constructor
        public CustomerViewModel()
        {
            _dbContext = new WaterContext();
            RegisterCommand = new ViewModelCommand(ExecuteRegisterCommand, CanExecuteRegisterCommand);
            DeleteCommand = new ViewModelCommand(ExecuteDeleteCommand, CanExecuteDeleteCommand);
            UpdateCommand = new ViewModelCommand(ExecuteUpdateCommand, CanExecuteUpdateCommand);
            SearchCommand = new ViewModelCommand(ExecuteSearchCommand, CanExecuteSearchCommand);

            Customers = new ObservableCollection<Customer>();
            LoadData();
        }

/*XỬ LÍ ĐĂNG KÍ TÀI KHOẢN*/
        private bool CanExecuteRegisterCommand(object obj)
        {
            return !string.IsNullOrWhiteSpace(Username) && Username.Length >= 3 &&
                   Password != null && Password.Length >= 3 &&
                   !string.IsNullOrWhiteSpace(FullName) &&
                   !string.IsNullOrWhiteSpace(SelectedAccountType);
        }

        private void ExecuteRegisterCommand(object obj)
        {
            try
            {
                var existingAccount = _dbContext.Accounts.Any(u => u.Username == Username);
                if (existingAccount)
                {
                    ErrorMessage = "Trùng tên đăng nhập!";
                    return;
                }

                var account = new Account
                {
                    Username = this.Username,
                    Password = ConvertToUnsecureString(this.Password),
                    TypeAccount = SelectedAccountType
                };

                _dbContext.Accounts.Add(account);
                _dbContext.SaveChanges();

                if (account.AccountId > 0)
                {
                    var customer = new Customer
                    {
                        Name = this.FullName,
                        PhoneNumber = this.PhoneNumber,
                        Address = this.Address,
                        Email = this.Email,
                        AccountId = account.AccountId
                    };

                    _dbContext.Customers.Add(customer);
                    _dbContext.SaveChanges();

                    ErrorMessage = "Tài khoản đã được tạo!";
                    LoadData();

                    Username = "";
                    Password = new SecureString();
                    FullName = "";
                    PhoneNumber = "";
                    Email = "";
                    Address = "";
                    idKH = 0;
                    SelectedAccountType = "";
                }
                else
                {
                    ErrorMessage = "Failed to save Account.";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                ErrorMessage = $"An error occurred: {ex.Message}";
            }
        }

        private string ConvertToUnsecureString(SecureString securePassword)
        {
            if (securePassword == null)
                throw new ArgumentNullException(nameof(securePassword));

            IntPtr unmanagedString = IntPtr.Zero;
            try
            {
                unmanagedString = System.Runtime.InteropServices.Marshal.SecureStringToGlobalAllocUnicode(securePassword);
                return System.Runtime.InteropServices.Marshal.PtrToStringUni(unmanagedString);
            }
            finally
            {
                System.Runtime.InteropServices.Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
            }
        }
        /*XỬ LÍ XÓA TÀI KHOẢN*/

        private bool CanExecuteDeleteCommand(object obj)
        {
            // Kiểm tra xem có dòng nào được chọn trong DataGrid hay không
            return SelectedCustomer != null;
        }

        private void ExecuteDeleteCommand(object obj)
        {
            try
            {
                if (SelectedCustomer != null)
                {
                    // Xóa tài khoản tương ứng từ bảng Account
                    DeleteAccount(SelectedCustomer.AccountId);

                    // Xóa dữ liệu từ cơ sở dữ liệu (khách hàng)
                    _dbContext.Customers.Remove(SelectedCustomer);
                    _dbContext.SaveChanges();

                    // Xóa dữ liệu từ ObservableCollection (khách hàng)
                    Customers.Remove(SelectedCustomer);
                    ErrorMessage = "Đã xóa tài khoản!";
                    LoadData();
                    // Đặt SelectedCustomer về null sau khi xóa
                    SelectedCustomer = null;
                    
                }
            }
            catch (Exception ex)
            {
                // Xử lí khi có lỗi xảy ra
                Console.WriteLine($"An error occurred while deleting data: {ex.Message}");
            }
        }
        private void DeleteAccount(int accountId)
        {
            var accountToDelete = _dbContext.Accounts.FirstOrDefault(a => a.AccountId == accountId);
            if (accountToDelete != null)
            {
                _dbContext.Accounts.Remove(accountToDelete);
                _dbContext.SaveChanges();
                ErrorMessage = "Đã xóa tài khoản!";
                LoadData();
                Username = "";
                Password = new SecureString();
                FullName = "";
                PhoneNumber = "";
                Email = "";
                Address = "";
                idKH = 0;
                SelectedAccountType = "";
            }
        }

        /*CẬP NHẬT DỮ LIỆU*/
        private bool CanExecuteUpdateCommand(object obj)
        {
            // Kiểm tra xem có dòng nào được chọn trong DataGrid hay không
            return SelectedCustomer != null;
        }
        private void ExecuteUpdateCommand(object obj)
        {
            try
            {
                if (SelectedCustomer != null)
                {
                    // Lấy thông tin mới từ giao diện và cập nhật vào đối tượng SelectedCustomer
                    SelectedCustomer.Name = FullName; 
                    SelectedCustomer.Email = Email;
                    SelectedCustomer.Address = Address;
                    SelectedCustomer.PhoneNumber = PhoneNumber;
                    // Cập nhật dữ liệu vào cơ sở dữ liệu
                    _dbContext.SaveChanges();

                    ErrorMessage = "Cập nhật dữ liệu thành công!";
                    LoadData();
                    Username = "";
                    Password = new SecureString();
                    FullName = "";
                    PhoneNumber = "";
                    Email = "";
                    Address = "";
                    idKH = 0;
                    SelectedAccountType = "";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while updating data: {ex.Message}");
                ErrorMessage = $"An error occurred: {ex.Message}";
            }
        }

        /* TÌM KIẾM Bằng Box*/
        private void FilterCustomers()
        {
            string keyword = SearchKeyword.ToLower();

            var filteredCustomers = _dbContext.Customers.Where(c => c.Name.ToLower().Contains(keyword) ||
                                                                     c.Address.ToLower().Contains(keyword) ||
                                                                     c.PhoneNumber.ToLower().Contains(keyword) ||
                                                                     c.Email.ToLower().Contains(keyword))
                                                         .ToList();

            Customers.Clear(); 
            foreach (var customer in filteredCustomers)
            {
                Customers.Add(customer); 
            }
        }



        /*        LỌC - LỰA CHỌN TÌM KIẾM*/
        private bool CanExecuteSearchCommand(object obj)
        {
            return !string.IsNullOrWhiteSpace(SelectedAccountType);
        }

        private void ExecuteSearchCommand(object obj)
        {
            FilterCustomersByAccountType();
        }

        private void FilterCustomersByAccountType()
        {
            // Lọc dữ liệu theo loại tài khoản được chọn từ ComboBox
            var filteredCustomers = _dbContext.Customers
                .Where(c => c.Account.TypeAccount == SelectedAccountType)
                .ToList();

            Customers.Clear();
            foreach (var customer in filteredCustomers)
            {
                Customers.Add(customer);
            }
        }

        /* TẢI LẠI DỮ LIỆU*/
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void LoadData()
        {
            Customers.Clear();
            foreach (var customer in _dbContext.Customers)
            {
                Customers.Add(customer);
            }
        }
    }
}
