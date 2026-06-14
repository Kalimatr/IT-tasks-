using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace AircraftWpfApp
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private string _assemblyPath;
        private string _selectedClassName;
        private MethodViewModel _selectedMethod;
        private string _log;
        private CreatedObjectViewModel _selectedObject;
        private Assembly _loadedAssembly;
        private int _counter = 1;

        public string AssemblyPath { get => _assemblyPath; set { _assemblyPath = value; OnPropertyChanged(); } }
        public ObservableCollection<string> ClassNames { get; } = new();
        
        public string SelectedClassName
        {
            get => _selectedClassName;
            set { _selectedClassName = value; OnPropertyChanged(); LoadConstructorParams(); }
        }

        public ObservableCollection<ParameterViewModel> ConstructorParams { get; } = new();
        public ObservableCollection<MethodViewModel> Methods { get; } = new();
        public ObservableCollection<CreatedObjectViewModel> CreatedObjects { get; } = new();

        public CreatedObjectViewModel SelectedObject
        {
            get => _selectedObject;
            set { _selectedObject = value; OnPropertyChanged(); LoadMethods(); }
        }

        public MethodViewModel SelectedMethod { get => _selectedMethod; set { _selectedMethod = value; OnPropertyChanged(); } }
        public string Log { get => _log; set { _log = value; OnPropertyChanged(); } }

        public ICommand LoadAssemblyCommand { get; }
        public ICommand CreateObjectCommand { get; }
        public ICommand ExecuteMethodCommand { get; }

        public MainViewModel()
        {
            LoadAssemblyCommand = new RelayCommand(LoadAssembly);
            CreateObjectCommand = new RelayCommand(CreateObject, () => !string.IsNullOrEmpty(SelectedClassName));
            ExecuteMethodCommand = new RelayCommand(ExecuteMethod, () => SelectedObject != null && SelectedMethod != null);
            AssemblyPath = AppDomain.CurrentDomain.BaseDirectory + "task2.dll";
        }

        private void LoadAssembly()
        {
            try
            {
                ClassNames.Clear(); CreatedObjects.Clear(); SelectedObject = null; Log = "Загрузка...\n";
                _loadedAssembly = Assembly.LoadFrom(AssemblyPath);
                
                Type interfaceType = _loadedAssembly.GetType("task2.IAirCraft");
                var types = interfaceType != null 
                    ? _loadedAssembly.GetTypes().Where(t => interfaceType.IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface)
                    : _loadedAssembly.GetTypes().Where(t => !t.IsAbstract && !t.IsInterface);

                foreach (var type in types) ClassNames.Add(type.Name);
                Log += $"Найдено классов: {ClassNames.Count}\n";
            }
            catch (Exception ex) { Log += $"Ошибка: {ex.Message}\n"; }
        }

        private void LoadConstructorParams()
        {
            ConstructorParams.Clear();
            if (string.IsNullOrEmpty(SelectedClassName)) return;
            Type type = _loadedAssembly.GetType($"task2.{SelectedClassName}") ?? _loadedAssembly.GetType(SelectedClassName);
            var ctor = type?.GetConstructors().FirstOrDefault();
            if (ctor != null)
            {
                foreach (var p in ctor.GetParameters())
                    ConstructorParams.Add(new ParameterViewModel { Name = p.Name, TypeName = p.ParameterType.Name, Value = GetDefault(p.ParameterType) });
            }
        }

        private void CreateObject()
        {
            try
            {
                Type type = _loadedAssembly.GetType($"task2.{SelectedClassName}") ?? _loadedAssembly.GetType(SelectedClassName);
                var ctor = type.GetConstructors().FirstOrDefault();
                object[] args = ConstructorParams.Select(p => Convert.ChangeType(p.Value, type.GetConstructor(Type.EmptyTypes) == null ? ctor.GetParameters()[Array.IndexOf(ctor.GetParameters(), ctor.GetParameters().FirstOrDefault(x => x.Name == p.Name))].ParameterType : typeof(string))).ToArray(); 
                object[] finalArgs = new object[ctor.GetParameters().Length];
                for(int i=0; i<ctor.GetParameters().Length; i++) finalArgs[i] = Convert.ChangeType(ConstructorParams[i].Value, ctor.GetParameters()[i].ParameterType);

                object instance = Activator.CreateInstance(type, finalArgs);
                SubscribeToEvents(instance);

                var wrapper = new CreatedObjectViewModel(instance, $"{SelectedClassName} #{_counter++}");
                CreatedObjects.Add(wrapper);
                SelectedObject = wrapper;
                Log += $"✅ Создан: {wrapper.DisplayName}\n";
            }
            catch (Exception ex) { Log += $"❌ Ошибка создания: {ex.InnerException?.Message ?? ex.Message}\n"; }
        }

        private void LoadMethods()
        {
            Methods.Clear(); SelectedMethod = null;
            if (SelectedObject == null) return;
            var methods = SelectedObject.Instance.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(m => !m.IsSpecialName && m.Name != "ToString" && m.Name != "Equals" && m.Name != "GetHashCode");
            foreach (var m in methods) Methods.Add(new MethodViewModel(m));
        }

        private void ExecuteMethod()
        {
            try
            {
                Log += $"\n--- {SelectedObject.DisplayName} ---\n";
                var method = SelectedMethod.MethodInfo;
                object[] args = new object[method.GetParameters().Length];
                for (int i = 0; i < method.GetParameters().Length; i++)
                    args[i] = Convert.ChangeType(SelectedMethod.Parameters[i].Value, method.GetParameters()[i].ParameterType);

                Log += $"Вызов: {method.Name}\n";
                object result = method.Invoke(SelectedObject.Instance, args);
                if (method.ReturnType == typeof(bool)) Log += $"Результат: {(bool)result}\n";
                
                SelectedObject.NotifyStateChanged(); 
                Log += "--- Успешно ---\n";
            }
            catch (TargetInvocationException tie) { Log += $"❌ Ошибка: {tie.InnerException?.Message ?? tie.Message}\n"; }
            catch (Exception ex) { Log += $"❌ Ошибка: {ex.Message}\n"; }
        }

        private void SubscribeToEvents(object instance)
        {
            Action<string> log = msg => Application.Current.Dispatcher.Invoke(() => { Log += msg + "\n"; SelectedObject?.NotifyStateChanged(); });
            string[] events = { "TakingOff", "Climbing", "Descending", "Landing", "OnGround" };
            string[] prefixes = { "ВЗЛЁТ", "НАБОР", "СНИЖЕНИЕ", "ПОСАДКА", "ЗЕМЛЯ" };

            for (int i = 0; i < events.Length; i++)
            {
                var evt = instance.GetType().GetEvent(events[i]);
                if (evt != null)
                {
                    var handler = new GenericEventHandler(log, prefixes[i]);
                    var method = handler.GetType().GetMethod(nameof(GenericEventHandler.Handle), BindingFlags.Public | BindingFlags.Instance);
                    var genericMethod = method.MakeGenericMethod(evt.EventHandlerType.GetGenericArguments()[0]);
                    evt.AddEventHandler(instance, Delegate.CreateDelegate(evt.EventHandlerType, handler, genericMethod));
                }
            }
        }

        private class GenericEventHandler
        {
            private readonly Action<string> _log; private readonly string _prefix;
            public GenericEventHandler(Action<string> log, string prefix) { _log = log; _prefix = prefix; }
            public void Handle<T>(object sender, T e) where T : EventArgs
            {
                string msg = "Событие";
                var prop = e.GetType().GetProperty("Message");
                if (prop != null) msg = prop.GetValue(e)?.ToString() ?? msg;
                _log($"[{_prefix}] {msg}");
            }
        }

        private string GetDefault(Type type) => type == typeof(string) ? "Model" : type == typeof(int) ? "100" : type == typeof(double) ? "1500" : "";

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}

  