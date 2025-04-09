using System.Globalization;

namespace ServiceBusManager.Converters;

/// <summary>
/// Converts a null value to true and a non-null value to false
/// </summary>
public class IsNullConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value == null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts a null value to false and a non-null value to true
/// </summary>
public class IsNotNullConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value != null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Checks if a collection count is greater than zero
/// </summary>
public class CountGreaterThanZeroConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int count)
        {
            return count > 0;
        }

        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Compares a value to a parameter and returns true if equal
/// </summary>
public class EqualityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (parameter == null || value == null)
            return false;

        // Try to convert both to integers for numeric comparison
        if (int.TryParse(value.ToString(), out int valueInt) && 
            int.TryParse(parameter.ToString(), out int paramInt))
        {
            return valueInt == paramInt;
        }

        // String comparison
        return value.ToString() == parameter.ToString();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Compares a value to a parameter and returns a brush based on equality
/// </summary>
public class EqualityToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (parameter == null || value == null)
            return Colors.Transparent;

        bool isEqual = false;

        // Try to convert both to integers for numeric comparison
        if (int.TryParse(value.ToString(), out int valueInt) && 
            int.TryParse(parameter.ToString(), out int paramInt))
        {
            isEqual = valueInt == paramInt;
        }
        else
        {
            // String comparison
            isEqual = value.ToString() == parameter.ToString();
        }

        // Use Primary color for selected state, Gray for unselected
        return isEqual ? Color.FromArgb("#3284bb") : Color.FromArgb("#C8C8C8");
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
} 