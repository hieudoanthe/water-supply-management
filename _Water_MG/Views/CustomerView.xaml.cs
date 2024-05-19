using _Water_MG.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
namespace _Water_MG.Views
{
    /// <summary>
    /// Interaction logic for CustomerView.xaml
    /// </summary>
    public partial class CustomerView : Page
    {
        public CustomerView()
        {
            InitializeComponent();
        }
        private void BindablePasswordBox_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void dgView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void dgView_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {

        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            // Tạo dialog để chọn nơi lưu trữ và tên file
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
            if (saveFileDialog.ShowDialog() == true)
            {
                // Mở file để ghi dữ liệu
                using (StreamWriter writer = new StreamWriter(saveFileDialog.FileName))
                {
                    // Viết tiêu đề cột
                    foreach (var column in dgView.Columns)
                    {
                        writer.Write(column.Header);
                        writer.Write(",");
                    }
                    writer.WriteLine();

                    // Viết dữ liệu từ các hàng
                    foreach (var item in dgView.Items)
                    {
                        foreach (var property in item.GetType().GetProperties())
                        {
                            writer.Write(property.GetValue(item, null));
                            writer.Write(",");
                        }
                        writer.WriteLine();
                    }
                }
            }
        }
    }
}
