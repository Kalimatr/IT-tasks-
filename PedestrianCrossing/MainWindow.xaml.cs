using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using PedestrianCrossing.Events;
using PedestrianCrossing.Interfaces;
using PedestrianCrossing.Models;
using PedestrianCrossing.ViewModel;

namespace PedestrianCrossing;

public partial class MainWindow : Window
{
    private readonly SimulationViewModel _viewModel;
    private readonly Dictionary<Guid, TextBlock> _uiElements = new();

    public MainWindow()
    {
        InitializeComponent();
        _viewModel = new SimulationViewModel();
        
        _viewModel.LightChanged += OnLightChanged;
        _viewModel.AccidentOccurred += OnAccident;
        _viewModel.StateUpdated += OnStateUpdated;
    }

    private void AddModel_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string typeName)
        {
            double startX = typeName.Contains("Car") ? -50 : 370;
            double startY = typeName.Contains("Car") ? 285 : 550;
            
            double speed = _viewModel.Entities.Count % 3 == 0 ? 5.0 : 3.5;

            _viewModel.AddModel(typeName, startX, startY, speed);
        }
    }

    private void Start_Click(object sender, RoutedEventArgs e) => _viewModel.StartSimulation();
    private void Stop_Click(object sender, RoutedEventArgs e) => _viewModel.StopSimulation();

    private void OnLightChanged(object sender, LightChangedEventArgs e)
    {
        Dispatcher.Invoke(() =>
        {
            if (e.CurrentState == TrafficLightState.GreenForCars)
            {
                LightTextBlock.Text = "Светофор: 🟢 Машинам";
                LightTextBlock.Foreground = System.Windows.Media.Brushes.Green;
            }
            else
            {
                LightTextBlock.Text = "Светофор: 🟢 Пешеходам";
                LightTextBlock.Foreground = System.Windows.Media.Brushes.Red;
            }
        });
    }

    private void OnAccident(object sender, AccidentEventArgs e)
    {

        Dispatcher.InvokeAsync(() =>
        {
            MessageBox.Show($"💥 Авария в точке ({e.X:F0}, {e.Y:F0})! Вызвана аварийная служба.", 
                "Событие", MessageBoxButton.OK, MessageBoxImage.Warning);
            
            _viewModel.AddModel("PedestrianCrossing.Models.EmergencyCar", -50, 285, 6.0);
            
            var lastEntity = _viewModel.Entities.Last();
            if (lastEntity is IEmergencyService emergency)
            {
                emergency.Dispatch(e.X, e.Y);
            }
        });
    }

    private void OnStateUpdated()
    {
        Dispatcher.InvokeAsync(() =>
        {
            foreach (var entity in _viewModel.Entities)
            {
                if (!_uiElements.ContainsKey(entity.Id))
                {
                    var textBlock = new TextBlock
                    {
                        Text = entity.Label,
                        FontSize = 28,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Width = 40, 
                        Height = 40,
                        Background = System.Windows.Media.Brushes.Transparent
                    };
                    _uiElements[entity.Id] = textBlock;
                    SimulationCanvas.Children.Add(textBlock);
                }

                var uiElement = _uiElements[entity.Id];
                uiElement.Visibility = entity.IsActive ? Visibility.Visible : Visibility.Collapsed;
                
                Canvas.SetLeft(uiElement, entity.X - 20);
                Canvas.SetTop(uiElement, entity.Y - 20);
            }
        });
    }
}