using System;
using System.Collections.Generic;
using System.Globalization;
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

namespace Nulah.VSIX.TaskTool.ToolWindows.TaskManager.Controls.Pages
{
    /// <summary>
    /// Interaction logic for NewTaskPage.xaml
    /// </summary>
    public partial class NewTaskPage : Page
    {
        public NewTaskPage()
        {
            InitializeComponent();
        }
    }

    public class IsStringNullOrEmptyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.IsNullOrWhiteSpace(value as string);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException("IsStringNullOrEmptyConverter can only be used OneWay.");
        }
    }
}
