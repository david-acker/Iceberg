﻿namespace Iceberg.Export;

public class DGMLColorProvider
{
    public static IList<string> GetColors()
    {
        // DGML apparently only uses a subset of the WPF color definitions included below, although it's unclear
        // whether this applies to only the named colors or the ARGB codes themselves.
        // Copied from: https://docs.microsoft.com/en-us/dotnet/api/system.windows.media.colors?view=netframework-4.8&preserve-view=true

        return new List<string>
        {
            "#FFF0F8FF", // AliceBlue

            "#FFFAEBD7", // AntiqueWhite

            "#FF00FFFF", // Aqua

            "#FF7FFFD4", // Aquamarine

            "#FFF0FFFF", // Azure

            "#FFF5F5DC", // Beige

            "#FFFFE4C4", // Bisque

            "#FF000000", // Black

            "#FFFFEBCD", // BlanchedAlmond

            "#FF0000FF", // Blue

            "#FF8A2BE2", // BlueViolet

            "#FFA52A2A", // Brown

            "#FFDEB887", // BurlyWood

            "#FF5F9EA0", // CadetBlue

            "#FF7FFF00", // Chartreuse

            "#FFD2691E", // Chocolate

            "#FFFF7F50", // Coral

            "#FF6495ED", // CornflowerBlue

            "#FFFFF8DC", // Cornsilk

            "#FFDC143C", // Crimson

            "#FF00FFFF", // Cyan

            "#FF00008B", // DarkBlue

            "#FF008B8B", // DarkCyan

            "#FFB8860B", // DarkGoldenrod

            "#FFA9A9A9", // DarkGray

            "#FF006400", // DarkGreen

            "#FFBDB76B", // DarkKhaki

            "#FF8B008B", // DarkMagenta

            "#FF556B2F", // DarkOliveGreen

            "#FFFF8C00", // DarkOrange

            "#FF9932CC", // DarkOrchid

            "#FF8B0000", // DarkRed

            "#FFE9967A", // DarkSalmon

            "#FF8FBC8F", // DarkSeaGreen

            "#FF483D8B", // DarkSlateBlue

            "#FF2F4F4F", // DarkSlateGray

            "#FF00CED1", // DarkTurquoise

            "#FF9400D3", // DarkViolet

            "#FFFF1493", // DeepPink

            "#FF00BFFF", // DeepSkyBlue

            "#FF696969", // DimGray

            "#FF1E90FF", // DodgerBlue

            "#FFB22222", // Firebrick

            "#FFFFFAF0", // FloralWhite

            "#FF228B22", // ForestGreen

            "#FFFF00FF", // Fuchsia

            "#FFDCDCDC", // Gainsboro

            "#FFF8F8FF", // GhostWhite

            "#FFFFD700", // Gold

            "#FFDAA520", // Goldenrod

            "#FF808080", // Gray

            "#FF008000", // Green

            "#FFADFF2F", // GreenYellow

            "#FFF0FFF0", // Honeydew

            "#FFFF69B4", // HotPink

            "#FFCD5C5C", // IndianRed

            "#FF4B0082", // Indigo

            "#FFFFFFF0", // Ivory

            "#FFF0E68C", // Khaki

            "#FFE6E6FA", // Lavender

            "#FFFFF0F5", // LavenderBlush

            "#FF7CFC00", // LawnGreen

            "#FFFFFACD", // LemonChiffon

            "#FFADD8E6", // LightBlue

            "#FFF08080", // LightCoral

            "#FFE0FFFF", // LightCyan

            "#FFFAFAD2", // LightGoldenrodYellow

            "#FFD3D3D3", // LightGray

            "#FF90EE90", // LightGreen

            "#FFFFB6C1", // LightPink

            "#FFFFA07A", // LightSalmon

            "#FF20B2AA", // LightSeaGreen

            "#FF87CEFA", // LightSkyBlue

            "#FF778899", // LightSlateGray

            "#FFB0C4DE", // LightSteelBlue

            "#FFFFFFE0", // LightYellow

            "#FF00FF00", // Lime

            "#FF32CD32", // LimeGreen

            "#FFFAF0E6", // Linen

            "#FFFF00FF", // Magenta

            "#FF800000", // Maroon

            "#FF66CDAA", // MediumAquamarine

            "#FF0000CD", // MediumBlue

            "#FFBA55D3", // MediumOrchid

            "#FF9370DB", // MediumPurple

            "#FF3CB371", // MediumSeaGreen

            "#FF7B68EE", // MediumSlateBlue

            "#FF00FA9A", // MediumSpringGreen

            "#FF48D1CC", // MediumTurquoise

            "#FFC71585", // MediumVioletRed

            "#FF191970", // MidnightBlue

            "#FFF5FFFA", // MintCream

            "#FFFFE4E1", // MistyRose

            "#FFFFE4B5", // Moccasin

            "#FFFFDEAD", // NavajoWhite

            "#FF000080", // Navy

            "#FFFDF5E6", // OldLace

            "#FF808000", // Olive

            "#FF6B8E23", // OliveDrab

            "#FFFFA500", // Orange

            "#FFFF4500", // OrangeRed

            "#FFDA70D6", // Orchid

            "#FFEEE8AA", // PaleGoldenrod

            "#FF98FB98", // PaleGreen

            "#FFAFEEEE", // PaleTurquoise

            "#FFDB7093", // PaleVioletRed

            "#FFFFEFD5", // PapayaWhip

            "#FFFFDAB9", // PeachPuff

            "#FFCD853F", // Peru

            "#FFFFC0CB", // Pink

            "#FFDDA0DD", // Plum

            "#FFB0E0E6", // PowderBlue

            "#FF800080", // Purple

            "#FFFF0000", // Red

            "#FFBC8F8F", // RosyBrown

            "#FF4169E1", // RoyalBlue

            "#FF8B4513", // SaddleBrown

            "#FFFA8072", // Salmon

            "#FFF4A460", // SandyBrown

            "#FF2E8B57", // SeaGreen

            "#FFFFF5EE", // SeaShell

            "#FFA0522D", // Sienna

            "#FFC0C0C0", // Silver

            "#FF87CEEB", // SkyBlue

            "#FF6A5ACD", // SlateBlue

            "#FF708090", // SlateGray

            "#FFFFFAFA", // Snow

            "#FF00FF7F", // SpringGreen

            "#FF4682B4", // SteelBlue

            "#FFD2B48C", // Tan

            "#FF008080", // Teal

            "#FFD8BFD8", // Thistle

            "#FFFF6347", // Tomato

            "#00FFFFFF", // Transparent

            "#FF40E0D0", // Turquoise

            "#FFEE82EE", // Violet

            "#FFF5DEB3", // Wheat

            "#FFFFFFFF", // White

            "#FFF5F5F5", // WhiteSmoke

            "#FFFFFF00", // Yellow

            "#FF9ACD32", // YellowGreen
        };
    }
}
