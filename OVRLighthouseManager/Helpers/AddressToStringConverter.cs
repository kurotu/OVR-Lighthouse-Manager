using System;
using System.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace OVRLighthouseManager.Helpers;

public class AddressToStringConverter : IValueConverter
{
    public AddressToStringConverter()
    {
    }

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is ulong address)
        {
            return AddressToString(address);
        }
        throw new ArgumentException("ExceptionAddressToStringConverterParameterMustBeAnUlong");
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (parameter is string addressString)
        {
            return StringToAddress(addressString);
        }

        throw new ArgumentException("ExceptionAddressToStringConverterParameterMustBeAString");
    }

    public static string AddressToString(ulong bluetoothAddress)
    {
        var hex = bluetoothAddress.ToString("X012");
        var sb = new StringBuilder();
        for (var i = 0; i < hex.Length; i += 2)
        {

            sb.Append(hex[i..(i + 2)]);
            if (i < hex.Length - 2)
            {

                sb.Append(':');
            }
        }
        return sb.ToString();
    }

    public static ulong StringToAddress(string bluetoothAddressString)
    {
        var hex = bluetoothAddressString.Replace(":", "");
        return System.Convert.ToUInt64(hex, 16);
    }
}
