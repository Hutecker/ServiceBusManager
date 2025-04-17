using CommunityToolkit.Mvvm.ComponentModel;

namespace ServiceBusManager.Controls;

public partial class RefreshTimerIndicator : ContentView
{
    public static readonly BindableProperty ProgressProperty = 
        BindableProperty.Create(nameof(Progress), typeof(double), typeof(RefreshTimerIndicator), 0.0);

    public static readonly BindableProperty HeightProperty = 
        BindableProperty.Create(nameof(Height), typeof(double), typeof(RefreshTimerIndicator), 35.0);

    public static readonly BindableProperty WidthProperty = 
        BindableProperty.Create(nameof(Width), typeof(double), typeof(RefreshTimerIndicator), 40.0);

    public double Progress
    {
        get => (double)GetValue(ProgressProperty);
        set => SetValue(ProgressProperty, value);
    }

    public double Height
    {
        get => (double)GetValue(HeightProperty);
        set => SetValue(HeightProperty, value);
    }

    public double Width
    {
        get => (double)GetValue(WidthProperty);
        set => SetValue(WidthProperty, value);
    }

    public RefreshTimerIndicator()
    {
        InitializeComponent();
    }
} 