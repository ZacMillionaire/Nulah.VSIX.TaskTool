using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Nulah.VSIX.TaskTool.StandardLib.Models;

namespace Nulah.VSIX.TaskTool.ToolWindows.TaskManager.ViewModels.Pages
{
    public class NewTaskPageViewModel : ViewModelBase
    {
        public ICommand CreateNewTaskCommand { get; private set; }

        private bool _isCreateEnabled;

        public bool IsCreateEnabled
        {
            get { return _isCreateEnabled; }
            set { _isCreateEnabled = value; OnPropertyChanged(nameof(IsCreateEnabled)); }
        }

        private string _taskTitle;

        public string TaskTitle
        {
            get { return _taskTitle; }
            set
            {
                ValidateTaskTitle(value);
            }
        }

        private string _taskContent;

        public string TaskContent
        {
            get { return _taskContent; }
            set
            {
                ValidateTaskContent(value);
            }
        }

        private string _resultMessage = null;

        public string ResultMessage
        {
            get { return _resultMessage; }
            set
            {
                _resultMessage = value;
                // Don't fire property changes if the value is null
                if (string.IsNullOrWhiteSpace(value) == false)
                {
                    OnPropertyChanged(nameof(ResultMessage));
                }
            }
        }

        public NewTaskPageViewModel()
        {
            IsCreateEnabled = false;
            CreateNewTaskCommand = new RelayCommand(_ => CreateNewTask(), x => IsCreateEnabled);
        }

        private void ValidateTaskTitle(string title)
        {
            _taskTitle = title;
            OnPropertyChanged(nameof(TaskTitle));
            ValidateRequiredFields();
        }

        private void ValidateTaskContent(string content)
        {
            _taskContent = content;
            OnPropertyChanged(nameof(TaskContent));
            ValidateRequiredFields();
        }

        private void ValidateRequiredFields()
        {
            IsCreateEnabled = string.IsNullOrWhiteSpace(_taskTitle) == false && string.IsNullOrWhiteSpace(_taskContent) == false;
        }

        private void CreateNewTask()
        {
            var newTaskCreated = GetDependency<TaskListManager>().CreateTask(_taskTitle, _taskContent);
            if (newTaskCreated == true)
            {
                TaskTitle = null;
                TaskContent = null;
                ResultMessage = "Task Created";
            }
        }
    }
}
