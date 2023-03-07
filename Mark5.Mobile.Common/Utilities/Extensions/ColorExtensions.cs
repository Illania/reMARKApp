using System.Drawing;

namespace Mark5.Mobile.Common.Utilities.Extensions
{
    public static class ColorExtensions
    {
        public static string ToHtml(this Color color)
        {
            return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
        }
    }
}
