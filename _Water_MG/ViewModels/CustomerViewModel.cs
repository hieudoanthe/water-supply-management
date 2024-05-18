using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Security;
using System.Windows.Input;
using _Water_MG.Models;
using Microsoft.EntityFrameworkCore;

namespace _Water_MG.ViewModels
{
    public class CustomerViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        // Fields
        private string _username;
        private SecureString _password;
        private string _fullName;
        private string _errorMessage;
        private bool _isViewVisible = true;
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

        // Commands
        public ICommand RegisterCommand { get; }

        // Constructor
        public CustomerViewModel()
        {
            _dbContext = new WaterContext();
            RegisterCommand = new ViewModelCommand(ExecuteRegisterCommand, CanExecuteRegisterCommand);
            Customers = new ObservableCollection<Customer>();
            LoadData();
        }

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
                    ErrorMessage = "Username already exists.";
                    return;
                }

                var account = new Account
                {
                    Username = this.Username,
                    Password = ConvertToUnsecureString(this.Password),
                    TypeAccount = SelectedAccountType // Lưu loại tài khoản vào cơ sở dữ liệu
                };

                _dbContext.Accounts.Add(account);
                _dbContext.SaveChanges();

                if (account.AccountId > 0)
                {
                    var customer = new Customer
                    {
                        Name = this.FullName,
                        AccountId = account.AccountId
                    };

                    _dbContext.Customers.Add(customer);
                    _dbContext.SaveChanges();

                    ErrorMessage = "Account added successfully!";
                    LoadData(); // Reload data after successful addition

                    // Reset the fields after successful addition
                    Username = "";
                    Password = new SecureString();
                    FullName = "";
                    SelectedAccountType = "";
                }
                else
                {
                    // Xử lý trường hợp lỗi khi không thể lưu Account
                    ErrorMessage = "Failed to save Account.";
                }
            }
            catch (Exception ex)
            {
                // In ra thông tin lỗi của ngoại lệ chính
                Console.WriteLine($"An error occurred: {ex.Message}");

                // Kiểm tra xem có ngoại lệ bên trong không và in ra thông tin lỗi của ngoại lệ bên trong
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }

                // Xử lý lỗi hoặc thông báo lỗi cho người dùng
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

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Method to load data
        private void LoadData()
        {
            try
            {
                Customers.Clear();
                var customersFromDb = _dbContext.Customers
                                          .Include(c => c.Account)  // Include Account table
                                          .ToList();

                foreach (var customer in customersFromDb)
                {
                    Customers.Add(customer);
                }

                // Thông báo rằng dữ liệu đã thay đổi
                OnPropertyChanged(nameof(Customers));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while loading data: {ex.Message}");
            }
        }
    }
}
