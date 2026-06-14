using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AircraftWpfApp
{
    public class CreatedObjectViewModel : INotifyPropertyChanged
    {
        public object Instance { get; }
        public string DisplayName { get; }

        // Получаем состояние через переопределенный ToString() в библиотеке
        public string StateInfo => Instance?.ToString() ?? "Неизвестно";

        public CreatedObjectViewModel(object instance, string displayName)
        {
            Instance = instance;
            DisplayName = displayName;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyStateChanged() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StateInfo)));
        protected void OnPropertyChanged([CallerMemberName] string name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}