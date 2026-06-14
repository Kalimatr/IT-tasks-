using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace AircraftWpfApp
{
    public class MethodViewModel : INotifyPropertyChanged
    {
        public MethodInfo MethodInfo { get; set; }
        public string Name => MethodInfo.Name;
        public ObservableCollection<ParameterViewModel> Parameters { get; } = new();

        public MethodViewModel(MethodInfo methodInfo)
        {
            MethodInfo = methodInfo;
            foreach (var param in methodInfo.GetParameters())
            {
                Parameters.Add(new ParameterViewModel { Name = param.Name, TypeName = param.ParameterType.Name, Value = GetDefault(param.ParameterType) });
            }
        }

        private string GetDefault(System.Type type) => type == typeof(string) ? "Default" : type == typeof(int) ? "1" : type == typeof(double) ? "1.0" : "";

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}