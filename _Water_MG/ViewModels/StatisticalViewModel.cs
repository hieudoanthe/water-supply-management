using System;
using System.Collections.ObjectModel;
using System.Linq;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using _Water_MG.Models;
using OxyPlot.Axes;
using System.Windows.Input;

namespace _Water_MG.ViewModels
{
    public class StatisticalViewModel : ViewModelBase
    {
        public ObservableCollection<Bill> Bills { get; set; }
        public SeriesCollection SeriesCollection { get; set; } 
        public SeriesCollection PieSeriesCollection { get; set; }
        public SeriesCollection BarSeriesCollection { get; set; }
        public ObservableCollection<string> Labels { get; set; } 
        public Func<double, string> YAxisLabelFormatter { get; set; }
        public Func<ChartPoint, string> PiePointLabel { get; set; }

        private string _selectedMonth;
        public string SelectedMonth
        {
            get { return _selectedMonth; }
            set
            {
                _selectedMonth = value;
                OnPropertyChanged(nameof(SelectedMonth));
            }
        }

        private string _selectedYear;
        public string SelectedYear
        {
            get { return _selectedYear; }
            set
            {
                _selectedYear = value;
                OnPropertyChanged(nameof(SelectedYear));
            }
        }
        private double _totalAmount;
        public double TotalAmount
        {
            get { return _totalAmount; }
            set
            {
                _totalAmount = value;
                OnPropertyChanged(nameof(TotalAmount));
            }
        }

        public ObservableCollection<string> MonthItemsSource { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> YearItemsSource { get; } = new ObservableCollection<string>();

        public ICommand TKCommand { get;}
        public StatisticalViewModel()
        {
            Bills = [];
            SeriesCollection = [];
            PieSeriesCollection = [];
            BarSeriesCollection = [];
            Labels = [];
            YAxisLabelFormatter = value => $"{value:N0} Đồng";
            PiePointLabel = chartPoint => string.Format("{0} ({1:P})", chartPoint.Y, chartPoint.Participation);
            for (int i = 1; i <= 12; i++)
            {
                MonthItemsSource.Add(i.ToString());
            }

            // Lấy 4 năm gần nhất từ năm hiện tại (tính đến năm 2023)
            int currentYear = DateTime.Now.Year;
            for (int i = currentYear; i >= currentYear - 3; i--)
            {
                YearItemsSource.Add(i.ToString());
            }

            // Khởi tạo các giá trị mặc định cho SelectedMonth và SelectedYear
            SelectedMonth = MonthItemsSource.FirstOrDefault();
            SelectedYear = YearItemsSource.FirstOrDefault();
            TKCommand = new ViewModelCommand(ExecuteTKCommand, CanExecuteTKCommand);
            InitializeData();
            InitializeChart();
            InitializePieChart();
        }
        private bool CanExecuteTKCommand(object obj)
        {
            return true;
        }
        private void ExecuteTKCommand(object parameter)
        {
            int selectedMonth = int.Parse(SelectedMonth);
            int selectedYear = int.Parse(SelectedYear);

            DateTime selectedDate = new DateTime(selectedYear, selectedMonth, 1);

            using (var context = new WaterContext())
            {
                var billsInSelectedMonth = context.Bills
                    .Where(b => b.BillingDate.Year == selectedDate.Year && b.BillingDate.Month == selectedDate.Month)
                    .ToList();

                // Tạo và cập nhật biểu đồ
                UpdateChart(billsInSelectedMonth);
            }
        }
        private void UpdateChart(List<Bill> bills)
        {
            BarSeriesCollection.Clear();
            Labels.Clear();

            if (bills != null && bills.Any())
            {
                ChartValues<double> chartValues = new ChartValues<double>();
                foreach (var bill in bills)
                {
                    chartValues.Add(Convert.ToDouble(bill.AmountDue));
                    Labels.Add(bill.BillingDate.ToShortDateString());
                }

                BarSeriesCollection.Add(new ColumnSeries
                {
                    Title = "Hóa đơn:",
                    Values = chartValues,
                    DataLabels = true
                });
            }
        }

        private void InitializeData()
        {
            // Khởi tạo dữ liệu mẫu cho Bills nếu cần
            using var context = new WaterContext();
            var billsFromDb = context.Bills.ToList();
            double total = 0;
            foreach (var bill in billsFromDb)
            {
                Bills.Add(bill);
                total += (double)bill.AmountDue;
            }
            TotalAmount = total;
        }

        private void InitializeChart()
        {
            if (Bills != null && Bills.Any())
            {
                ChartValues<ObservablePoint> chartValues = [];
                foreach (var bill in Bills)
                {
                    chartValues.Add(new ObservablePoint(DateTimeAxis.ToDouble(bill.BillingDate), Convert.ToDouble(bill.AmountDue)));
                    Labels.Add(bill.BillingDate.ToShortDateString());
                }

               
                SeriesCollection.Add(new LineSeries
                {
                    Title = "Hóa đơn:",
                    Values = chartValues,
                    PointGeometrySize = 10
                });
            }
        }

        private void InitializePieChart()
        {
            if (Bills != null && Bills.Any())
            {
                int range1 = 0, range2 = 0, range3 = 0, range4 = 0;

                foreach (var bill in Bills)
                {
                    double amount = Convert.ToDouble(bill.AmountDue);
                    if (amount <= 500000)
                        range1++;
                    else if (amount <= 1000000)
                        range2++;
                    else if (amount <= 2000000)
                        range3++;
                    else
                        range4++;
                }

                PieSeriesCollection.Add(new PieSeries
                {
                    Title = "0 - 500.000",
                    Values = new ChartValues<double> { range1 },
                    DataLabels = true,
                    LabelPoint = PiePointLabel
                });

                PieSeriesCollection.Add(new PieSeries
                {
                    Title = "500.000 - 1M",
                    Values = new ChartValues<double> { range2 },
                    DataLabels = true,
                    LabelPoint = PiePointLabel
                });

                PieSeriesCollection.Add(new PieSeries
                {
                    Title = "1M - 2M",
                    Values = new ChartValues<double> { range3 },
                    DataLabels = true,
                    LabelPoint = PiePointLabel
                });

                PieSeriesCollection.Add(new PieSeries
                {
                    Title = "Over 2M",
                    Values = new ChartValues<double> { range4 },
                    DataLabels = true,
                    LabelPoint = PiePointLabel
                });
            }
        }
        private void InitializeBarChart()
        {
            BarSeriesCollection.Clear();
            var barSeries = new ColumnSeries
            {
                Title = "Hóa đơn",
                Values = new ChartValues<double>(),
                DataLabels = true
            };
            BarSeriesCollection.Add(barSeries);
        }
    }
}
