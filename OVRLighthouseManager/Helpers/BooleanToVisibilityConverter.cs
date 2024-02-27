using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace OVRLighthouseManager.Helpers;

internal class BooleanToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool b)
        {
            if (parameter is string p && bool.Parse(p))
            {
                b = !b;
            }
            return b ? Visibility.Visible : Visibility.Collapsed;
        }
        throw new ArgumentException("ExceptionBooleanToVisibilityConverterParameterMustBeABoolean");
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value is Visibility visibility)
        {
            if (parameter is string p && bool.Parse(p))
            {
                return visibility != Visibility.Visible;
            }
            return visibility == Visibility.Visible;
        }
        throw new ArgumentException("ExceptionBooleanToVisibilityConverterParameterMustBeAVisibility");
    }
}
