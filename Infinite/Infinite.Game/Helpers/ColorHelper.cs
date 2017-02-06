using SiliconStudio.Core.Mathematics;

namespace Infinite.Helpers
{
    public static class ColorHelper
    {
        public static Color Blend(this Color color, Color backColor)
        {
            byte r = (byte)((color.R + backColor.R) / 2.0);
            byte g = (byte)((color.G + backColor.G) / 2.0);
            byte b = (byte)((color.B + backColor.B) / 2.0);

            return new Color(r, g, b);
        }
    }
}
