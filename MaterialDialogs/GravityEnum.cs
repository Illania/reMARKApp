using System;
using Android.Views;

namespace MaterialDialogs
{
    public enum GravityEnum
    {
        Start,
        Center,
        End
    }

    public static class GravityEnumExtensions
    {
        public static GravityFlags GetGravityInt(this GravityEnum enumValue)
        {
            switch (enumValue)
            {
                case GravityEnum.Start:
                    return GravityFlags.Start;
                case GravityEnum.Center:
                    return GravityFlags.CenterHorizontal;
                case GravityEnum.End:
                    return GravityFlags.End;
                default:
                    throw new ArgumentException("Invalid gravity constant");
            }
        }

        public static TextAlignment GetTextAlignment(this GravityEnum enumValue)
        {
            switch (enumValue)
            {
                case GravityEnum.Center:
                    return TextAlignment.Center;
                case GravityEnum.End:
                    return TextAlignment.ViewEnd;
                default:
                    return TextAlignment.ViewStart;
            }
        }
    }
}