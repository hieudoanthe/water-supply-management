using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security;
using System.Text;
using System.Windows.Input;
using _Water_MG.Models;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;

namespace _Water_MG.ViewModels
{
    public class MeterViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        private string _newMeterNumber;
        private DateTime? _newLastReadingDate;
        private decimal? _newLastReadingValue;
        private string _typeMeter;
        private int _idKh;
        private string _searchKeyword;
        private string _errorMessage;

        private readonly WaterContext _dbContext;
        public ObservableCollection<string> MeterTypeItemsSource { get; } = new ObservableCollection<string>
        {
            "Cũ",
            "Mới",
            "Tạm khóa",
            "Đang hoạt động"
        };

        private string _selectedMeterType;
        public string SelectedMeterType
        {
            get { return _selectedMeterType; }
            set
            {
                _selectedMeterType = value;
                OnPropertyChanged(nameof(SelectedMeterType));
            }
        }

        public string NewMeterNumber
        {
            get { return _newMeterNumber; }
            set
            {
                _newMeterNumber = value;
                OnPropertyChanged(nameof(NewMeterNumber));
            }
        }

        public DateTime? NewLastReadingDate
        {
            get { return _newLastReadingDate; }
            set
            {
                _newLastReadingDate = value;
                OnPropertyChanged(nameof(NewLastReadingDate));
            }
        }

        public decimal? NewLastReadingValue
        {
            get { return _newLastReadingValue; }
            set
            {
                _newLastReadingValue = value;
                OnPropertyChanged(nameof(NewLastReadingValue));
            }
        } 

        public int idKH
        {
            get { return _idKh; }
            set
            {
                _idKh = value;
                OnPropertyChanged(nameof(idKH));
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
        public ICommand AddMeter { get; }
        public ICommand DeleteMeter { get; }
        public ICommand UpdateMeter { get; }
        public ICommand LockMeter { get; }  
        public ICommand SearchCommand { get; }
        public MeterViewModel()
        {
            _dbContext = new WaterContext();
            AddMeter = new ViewModelCommand(ExecuteAddMeterCommand, CanExecuteAddMeterCommand);
            DeleteMeter = new ViewModelCommand(ExecuteDeleteMeterCommand, CanExecuteDeleteMeterCommand);
            UpdateMeter = new ViewModelCommand(ExecuteUpdateMeterCommand, CanExecuteUpdateMeterCommand);
            LockMeter = new ViewModelCommand(ExecuteLockMeterCommand, CanExecuteLockMeterCommand);
            SearchCommand = new ViewModelCommand(ExecuteSearchCommand, CanExecuteSearchCommand);
            Meters = new ObservableCollection<Meter>();
            LoadData();
        }

/*        THÊM ĐÒNG HỒ MỚI*/
        private bool CanExecuteAddMeterCommand(object obj)
        {
            return !string.IsNullOrWhiteSpace(NewMeterNumber) && idKH > 0;
        }
        private void ExecuteAddMeterCommand(object obj)
        {
            try
            {
                var customer = _dbContext.Customers.FirstOrDefault(c => c.CustomerId == idKH);
                if (customer == null)
                {
                    ErrorMessage = "Mã khách hàng không tồn tại";
                    return;
                }

                var meter = new Meter
                {
                    CustomerId = this.idKH,
                    MeterNumber = this.NewMeterNumber,
                    LastReadingDate = this.NewLastReadingDate,
                    LastReadingValue = this.NewLastReadingValue,
                    TypeMeter = SelectedMeterType,
                };

                _dbContext.Meters.Add(meter);
                _dbContext.SaveChanges();
                ErrorMessage = "Thêm đồng hồ thành công";
                LoadData();
                idKH = 0;
                NewMeterNumber = "";
                NewLastReadingDate = DateTime.Now;
                NewLastReadingValue = 0;
                NewMeterNumber = "";
                SelectedMeterType = "";
            }
            catch (Exception ex)
            {
                var sb = new StringBuilder();
                sb.AppendLine($"Lỗi: {ex.Message}");
                if (ex.InnerException != null)
                {
                    sb.AppendLine($"Lỗi: {ex.InnerException.Message}");
                    if (ex.InnerException.InnerException != null)
                    {
                        sb.AppendLine($"Lỗi: {ex.InnerException.InnerException.Message}");
                    }
                }
                ErrorMessage = sb.ToString();
            }
        }

        /*XÓA DỮ LIỆU*/
        private bool CanExecuteDeleteMeterCommand(object obj)
        {
            return SelectedMeter != null;
        }
        private void ExecuteDeleteMeterCommand( object obj)
        {
            try 
            {
               if (SelectedMeter != null)
                {
                    _dbContext.Meters.Remove(SelectedMeter);
                    _dbContext.SaveChanges();
                    Meters.Remove(SelectedMeter);
                    ErrorMessage = "Đã xóa đồng hồ";
                    SelectedMeter = null;
                }
            }
            catch(Exception ex)
            {
                ErrorMessage = "Lỗi xảy ra trong quá trình xử lí!";
            }
        }

        /*CẬP NHẬT DỮ LIỆU ĐỒNG HỒ*/
        public bool CanExecuteUpdateMeterCommand(object obj)
        {
            return SelectedMeter != null;
        }
        public void ExecuteUpdateMeterCommand( object obj)
        {
            try
            {
                if(SelectedMeter != null)
                {
                    SelectedMeter.LastReadingDate = NewLastReadingDate;
                    SelectedMeter.LastReadingValue = NewLastReadingValue;
                    SelectedMeter.MeterNumber = NewMeterNumber;
                    SelectedMeter.TypeMeter = SelectedMeterType;
                    _dbContext.SaveChanges();
                    ErrorMessage = "Cập nhật dữ liệu thành công !";
                    LoadData();
                    idKH = 0;
                    idKH = 0;
                    NewMeterNumber = "";
                    NewLastReadingDate = DateTime.Now;
                    NewLastReadingValue = 0;
                    NewMeterNumber = "";
                    SelectedMeterType = "";
                }
            } catch(Exception ex)
            {
                ErrorMessage = "Lỗi cập nhật. Vui lòng thử lại sau !";
            }
        }
        /*KHÓA ĐỒNG HỒ */
        private bool CanExecuteLockMeterCommand(object obj)
        {
            return SelectedMeter != null;
        }
        private void ExecuteLockMeterCommand( object obj)
        {
            try 
            {
                if(SelectedMeter != null)
                {
                    SelectedMeter.TypeMeter = "Tạm khóa";
                    _dbContext.SaveChanges();
                    ErrorMessage = "Đã tạm khóa đồng hồ. Chờ thanh toán...";
                    LoadData();
                    SelectedMeterType = "";
                }            
            }
            catch(Exception ex)
            {
                ErrorMessage = "Xảy ra lỗi trong quá trình xử lí !";
            }
        }
        /*TÌM KIẾM BẰNG KEYWORD*/
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

            var filteredMeters = _dbContext.Meters
                .Where(m =>
                    (m.LastReadingValue != null && m.LastReadingValue.ToString().Contains(keyword)) ||
                    (m.LastReadingDate != null && m.LastReadingDate.ToString().Contains(keyword)) ||
                    (!string.IsNullOrEmpty(m.TypeMeter) && m.TypeMeter.ToLower().Contains(keyword)) ||
                    (!string.IsNullOrEmpty(m.MeterNumber) && m.MeterNumber.ToLower().Contains(keyword)))
                .ToList();

            Meters.Clear();
            foreach (var meter in filteredMeters)
            {
                Meters.Add(meter);
            }
        }
        /*TÌM KIẾM THEO TYPE BOX*/
        private bool CanExecuteSearchCommand(object obj)
        {
            return !string.IsNullOrWhiteSpace(SelectedMeterType);
        }

        private void ExecuteSearchCommand(object obj)
        {
            FilterMeterByMeterType();
        }

        private void FilterMeterByMeterType()
        {
            // Lọc dữ liệu theo loại tài khoản được chọn từ ComboBox
            var filteredMeters= _dbContext.Meters
                .Where(m => m.TypeMeter == SelectedMeterType)
                .ToList();

            Meters.Clear();
            foreach (var meter in filteredMeters)
            {
                Meters.Add(meter);
            }
        }
      
        /* TẢI LẠI DỮ LIỆU*/
        public ObservableCollection<Meter> Meters { get; set; }
        private Meter _selectedMeter;
        public Meter SelectedMeter
        {
            get { return _selectedMeter; }
            set
            {
                _selectedMeter = value;
                OnPropertyChanged(nameof(SelectedMeter));
                if (_selectedMeter != null)
                {
                    idKH = _selectedMeter.CustomerId;
                    NewLastReadingDate = _selectedMeter.LastReadingDate;
                    NewLastReadingValue = _selectedMeter.LastReadingValue;
                    NewMeterNumber = _selectedMeter.MeterNumber;
                }
            }
        }
        private void LoadData()
        {
            Meters.Clear();
            foreach (var meter in _dbContext.Meters)
            {
                Meters.Add(meter);
            }
        }

    }
}
