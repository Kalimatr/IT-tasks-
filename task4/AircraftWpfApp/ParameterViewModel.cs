using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AircraftWpfApp
{
    public class ParameterViewModel : INotifyPropertyChanged
    {
        public string Name { get; set; }
        public string TypeName { get; set; }
        private string _value;
        public string Value { get => _value; set { _value = value; OnPropertyChanged(); } }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}