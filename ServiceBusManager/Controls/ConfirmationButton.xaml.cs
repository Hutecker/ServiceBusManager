using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace ServiceBusManager.Controls;

public partial class ConfirmationButton : Button
{
    private readonly System.Timers.Timer _resetTimer;
    private bool _isConfirming;
    private Color _lightBackgroundColor;
    private Color _darkBackgroundColor;
    private Color _lightTextColor;
    private Color _darkTextColor;
    private Color _warningColor;
    private Color _blackColor;

    public static readonly BindableProperty ConfirmationCommandProperty =
        BindableProperty.Create(nameof(ConfirmationCommand), typeof(ICommand), typeof(ConfirmationButton));

    public static readonly BindableProperty OriginalTextProperty =
        BindableProperty.Create(nameof(OriginalText), typeof(string), typeof(ConfirmationButton));

    public static readonly BindableProperty ConfirmationTextProperty =
        BindableProperty.Create(nameof(ConfirmationText), typeof(string), typeof(ConfirmationButton), "Confirm?");

    public ICommand ConfirmationCommand
    {
        get => (ICommand)GetValue(ConfirmationCommandProperty);
        set => SetValue(ConfirmationCommandProperty, value);
    }

    public string OriginalText
    {
        get => (string)GetValue(OriginalTextProperty);
        set => SetValue(OriginalTextProperty, value);
    }

    public string ConfirmationText
    {
        get => (string)GetValue(ConfirmationTextProperty);
        set => SetValue(ConfirmationTextProperty, value);
    }

    public ConfirmationButton()
    {
        InitializeComponent();
        
        // Set theme colors from application resources
        _lightBackgroundColor = (Color)Application.Current.Resources["Primary"];
        _darkBackgroundColor = (Color)Application.Current.Resources["PrimaryDark"];
        _lightTextColor = (Color)Application.Current.Resources["White"];
        _darkTextColor = (Color)Application.Current.Resources["PrimaryDarkText"];
        _warningColor = (Color)Application.Current.Resources["Warning"];
        _blackColor = (Color)Application.Current.Resources["Black"];

        // Set initial colors based on current theme
        UpdateThemeColors();
        
        _resetTimer = new System.Timers.Timer(3000); // 3 seconds
        _resetTimer.Elapsed += ResetTimer_Elapsed;
        this.Clicked += ConfirmationButton_Clicked;
        
        // Subscribe to theme changes
        Application.Current.RequestedThemeChanged += (s, e) => UpdateThemeColors();
    }

    private void UpdateThemeColors()
    {
        if (!_isConfirming)
        {
            var isDark = Application.Current.RequestedTheme == AppTheme.Dark;
            this.BackgroundColor = isDark ? _darkBackgroundColor : _lightBackgroundColor;
            this.TextColor = isDark ? _darkTextColor : _lightTextColor;
        }
    }

    private void ConfirmationButton_Clicked(object sender, EventArgs e)
    {
        if (!_isConfirming)
        {
            // First click - enter confirmation mode
            _isConfirming = true;
            this.Text = ConfirmationText;
            this.BackgroundColor = _warningColor;
            this.TextColor = _blackColor;
            _resetTimer.Start();
        }
        else
        {
            // Second click - execute command and reset
            _resetTimer.Stop();
            ConfirmationCommand?.Execute(null);
            ResetButton();
        }
    }

    private void ResetTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(ResetButton);
    }

    private void ResetButton()
    {
        _isConfirming = false;
        this.Text = OriginalText;
        UpdateThemeColors();
        _resetTimer.Stop();
    }
}