using System.Globalization;
using Microsoft.Maui.Controls;

namespace XExplorer.Core.Converts;

/// <summary>
/// 减法转换器。
/// 用于根据参数从输入值中减去指定的偏移量。
/// 实现了 <see cref="IValueConverter"/> 接口。
/// </summary>
public class MinusConverter : IValueConverter
{
    /// <summary>
    /// 将输入值减去指定的偏移量。
    /// </summary>
    /// <param name="value">输入值，可为 <see cref="double"/> 类型。</param>
    /// <param name="targetType">绑定目标属性的类型，暂未使用。</param>
    /// <param name="parameter">偏移量参数，为字符串表示的数值。</param>
    /// <param name="culture">绑定时使用的区域信息，暂未使用。</param>
    /// <returns>计算后的减法结果，如果输入值或参数无效，则返回原值。</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double width && parameter is string offsetString &&
            double.TryParse(offsetString, out double offset))
        {
            return width - offset;
        }
        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}