using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;

using Nulah.VSIX.TaskTool.StandardLib.Models;

namespace Nulah.VSIX.TaskTool.ToolWindows.TaskManager
{
    /// <summary>
    /// Interaction logic for TaskManagerWindowControl.
    /// </summary>
    public partial class TaskManagerWindowControl : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TaskManagerWindowControl"/> class.
        /// </summary>
        public TaskManagerWindowControl()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Handles click on the button by displaying a message box.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event args.</param>
        [SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions", Justification = "Sample code")]
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Default event handler naming pattern")]
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                string.Format(System.Globalization.CultureInfo.CurrentUICulture, "Invoked '{0}'", this.ToString()),
                "TaskManagerWindow");
        }
    }
}