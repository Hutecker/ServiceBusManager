#if WINDOWS
using Microsoft.Maui.Handlers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

public class CustomDatePickerHandler : DatePickerHandler
{
    protected override void ConnectHandler(CalendarDatePicker platformView)
    {
        base.ConnectHandler(platformView);

        // Traverse the visual tree to find the button inside the CalendarDatePicker
        var flyoutButton = FindChild<Microsoft.UI.Xaml.Controls.Button>(platformView);
        if (flyoutButton != null)
        {
            // Replace the default icon with a custom SymbolIcon
            var calendarIcon = new SymbolIcon(Symbol.Calendar)
            {
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Red) // Set the icon color here
            };
            flyoutButton.Content = calendarIcon;
        }
    }

    // Helper method to find a child of a specific type in the visual tree
    private T FindChild<T>(DependencyObject parent) where T : DependencyObject
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is T typedChild)
            {
                return typedChild;
            }

            var result = FindChild<T>(child);
            if (result != null)
            {
                return result;
            }
        }
        return null;
    }
}
#endif