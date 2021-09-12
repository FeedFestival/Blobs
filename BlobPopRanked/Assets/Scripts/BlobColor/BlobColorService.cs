using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    public static class BlobColorService
    {
        public static Color GetColorByBlobColor(BlobColor blobColor)
        {
            return ColorBank._.GetColorByName(ColorName(blobColor));
        }

        public static string ColorName(BlobColor clobColor)
        {
            switch (clobColor)
            {
                case BlobColor.BLUE:
                    return "Blue_Cornflower";
                case BlobColor.YELLOW:
                    return "Orange_Koromiko";
                case BlobColor.GREEN:
                    return "Green_Ocean";
                case BlobColor.BROWN:
                    return "Brown_Roman_Coffee";
                case BlobColor.PINK:
                    return "Pink_Hot";
                case BlobColor.WHITE:
                    return "White_Black_Haze";
                case BlobColor.BLACK:
                    return "Black_Cod_Gray";
                case BlobColor.RED:
                default:
                    return "Red_Torch";
            }
        }

        public static BlobColor ReturnBlobColor(int colorInt)
        {
            switch (colorInt)
            {
                case 1:
                    return BlobColor.BLUE;
                case 2:
                    return BlobColor.YELLOW;
                case 3:
                    return BlobColor.GREEN;
                case 4:
                    return BlobColor.BROWN;
                case 5:
                    return BlobColor.PINK;
                case 6:
                    return BlobColor.WHITE;
                case 7:
                    return BlobColor.BLACK;
                case 0:
                default:
                    return BlobColor.RED;
            }
        }

        public static int GetBlobColorPoints(BlobColor blobColor)
        {
            int multiplier = 1;
            switch (blobColor)
            {
                case BlobColor.BLUE:
                    multiplier = 2;
                    break;
                case BlobColor.YELLOW:
                    multiplier = 3;
                    break;
                case BlobColor.GREEN:
                    multiplier = 4;
                    break;
                case BlobColor.BROWN:
                    multiplier = 0;
                    break;
                case BlobColor.PINK:
                    multiplier = 5;
                    break;
                case BlobColor.WHITE:
                    multiplier = 6;
                    break;
                case BlobColor.BLACK:
                    multiplier = 7;
                    break;
                case BlobColor.RED:
                default:
                    multiplier = 1;
                    break;
            }
            return multiplier;
        }

        public static Color GetBlobAuraColor(BlobColor blobColor)
        {
            switch (blobColor)
            {
                case BlobColor.BLUE:
                    return ColorBank._.GetColor("#1E64D9".ToLower());
                case BlobColor.YELLOW:
                    return ColorBank._.GetColor("#53522D".ToLower());
                case BlobColor.GREEN:
                    return ColorBank._.GetColor("#3E6550".ToLower());
                case BlobColor.BROWN:
                    return ColorBank._.GetColor("#383E6F".ToLower());
                case BlobColor.RED:
                default:
                    return ColorBank._.GetColor("#BF3373".ToLower());
            }
        }

        public static Color GetLinkColor(BlobColor fromColor, BlobColor toColor)
        {
            Color color;
            // Debug.Log("fromColor: " + fromColor + " toColor: " + toColor);
            if (fromColor == BlobColor.RED && toColor == BlobColor.RED)
            {
                color = ColorBank._.Pink_Dark_Night_Shadz;
            }
            else if (fromColor == BlobColor.BLUE && toColor == BlobColor.BLUE)
            {
                color = ColorBank._.Blue_San_Marino;
            }
            else if (fromColor == BlobColor.YELLOW && toColor == BlobColor.YELLOW)
            {
                color = ColorBank._.Yellow_Gold_Sand;
            }
            else if (fromColor == BlobColor.GREEN && toColor == BlobColor.GREEN)
            {
                color = ColorBank._.Green_Aqua_Forest;
            }
            else if (fromColor == BlobColor.BROWN && toColor == BlobColor.BROWN)
            {
                color = ColorBank._.Brown_Ferra;
            }
            else if (fromColor == BlobColor.PINK && toColor == BlobColor.PINK)
            {
                color = ColorBank._.GetColor("#C158A6".ToLower());
            }
            else if (fromColor == BlobColor.WHITE && toColor == BlobColor.WHITE)
            {
                color = ColorBank._.GetColor("#D5D5D5".ToLower());
            }
            else if (fromColor == BlobColor.BLACK && toColor == BlobColor.BLACK)
            {
                color = ColorBank._.GetColor("#464646".ToLower());
            }

            //
            else if (fromColor == BlobColor.BLUE && toColor == BlobColor.RED
                || fromColor == BlobColor.RED && toColor == BlobColor.BLUE)
            {
                color = ColorBank._.Purple_Studio;
            }
            else if (fromColor == BlobColor.BLUE && toColor == BlobColor.YELLOW ||
                fromColor == BlobColor.YELLOW && toColor == BlobColor.BLUE)
            {
                color = ColorBank._.Green_Yellow;
            }
            else if (fromColor == BlobColor.BLUE && toColor == BlobColor.GREEN ||
                fromColor == BlobColor.GREEN && toColor == BlobColor.BLUE)
            {
                color = ColorBank._.Green_Blue_Wedgewood;
            }
            else if (fromColor == BlobColor.BLUE && toColor == BlobColor.BROWN ||
                fromColor == BlobColor.BROWN && toColor == BlobColor.BLUE)
            {
                color = ColorBank._.Purple_Salt_Box;
            }
            else if (fromColor == BlobColor.RED && toColor == BlobColor.YELLOW
                || fromColor == BlobColor.YELLOW && toColor == BlobColor.RED)
            {
                color = ColorBank._.Orange_Burnt_Sienna;
            }
            else if (fromColor == BlobColor.RED && toColor == BlobColor.GREEN
                || fromColor == BlobColor.GREEN && toColor == BlobColor.RED)
            {
                color = ColorBank._.Green_Red_Xanadu;
            }
            else if (fromColor == BlobColor.RED && toColor == BlobColor.BROWN
                || fromColor == BlobColor.BROWN && toColor == BlobColor.RED)
            {
                color = ColorBank._.Brown_Buccaneer;
            }
            else if (fromColor == BlobColor.YELLOW && toColor == BlobColor.GREEN
                        || fromColor == BlobColor.GREEN && toColor == BlobColor.YELLOW)
            {
                color = ColorBank._.Green_Wild_Willow;
            }
            else if (fromColor == BlobColor.YELLOW && toColor == BlobColor.BROWN
                        || fromColor == BlobColor.BROWN && toColor == BlobColor.YELLOW)
            {
                color = ColorBank._.Brown_Muddy_Waters;
            }
            else if (fromColor == BlobColor.GREEN && toColor == BlobColor.BROWN
                        || fromColor == BlobColor.BROWN && toColor == BlobColor.GREEN)
            {
                color = ColorBank._.Brown_Flint;
            }
            else if (fromColor == BlobColor.PINK && toColor == BlobColor.RED
                        || fromColor == BlobColor.RED && toColor == BlobColor.PINK)
            {
                color = ColorBank._.GetColor("#C3337E".ToLower());
            }
            else if (fromColor == BlobColor.PINK && toColor == BlobColor.GREEN
                        || fromColor == BlobColor.GREEN && toColor == BlobColor.PINK)
            {
                color = ColorBank._.GetColor("#805B56".ToLower());
            }
            else if (fromColor == BlobColor.PINK && toColor == BlobColor.BROWN
                        || fromColor == BlobColor.BROWN && toColor == BlobColor.PINK)
            {
                color = ColorBank._.GetColor("#B06D8A".ToLower());
            }
            else if (fromColor == BlobColor.PINK && toColor == BlobColor.YELLOW
                        || fromColor == BlobColor.YELLOW && toColor == BlobColor.PINK)
            {
                color = ColorBank._.GetColor("#F08588".ToLower());
            }
            else if (fromColor == BlobColor.PINK && toColor == BlobColor.BLUE
                        || fromColor == BlobColor.BLUE && toColor == BlobColor.PINK)
            {
                color = ColorBank._.GetColor("#9D6AC3".ToLower());
            }
            else if (fromColor == BlobColor.WHITE && toColor == BlobColor.RED
                        || fromColor == BlobColor.RED && toColor == BlobColor.WHITE)
            {
                color = ColorBank._.GetColor("#F880A8".ToLower());
            }
            else if (fromColor == BlobColor.WHITE && toColor == BlobColor.GREEN
                        || fromColor == BlobColor.GREEN && toColor == BlobColor.WHITE)
            {
                color = ColorBank._.GetColor("#A2D4BF".ToLower());
            }
            else if (fromColor == BlobColor.WHITE && toColor == BlobColor.BROWN
                        || fromColor == BlobColor.BROWN && toColor == BlobColor.WHITE)
            {
                color = ColorBank._.GetColor("#C38B86".ToLower());
            }
            else if (fromColor == BlobColor.WHITE && toColor == BlobColor.YELLOW
                        || fromColor == BlobColor.YELLOW && toColor == BlobColor.WHITE)
            {
                color = ColorBank._.GetColor("#FFF294".ToLower());
            }
            else if (fromColor == BlobColor.WHITE && toColor == BlobColor.BLUE
                        || fromColor == BlobColor.BLUE && toColor == BlobColor.WHITE)
            {
                color = ColorBank._.GetColor("#99DBF8".ToLower());
            }
            else if (fromColor == BlobColor.WHITE && toColor == BlobColor.PINK
                        || fromColor == BlobColor.PINK && toColor == BlobColor.WHITE)
            {
                color = ColorBank._.GetColor("#F8A6D4".ToLower());
            }
            //
            else if (fromColor == BlobColor.BLACK && toColor == BlobColor.RED
                        || fromColor == BlobColor.RED && toColor == BlobColor.BLACK)
            {
                color = ColorBank._.GetColor("#850E35".ToLower());
            }
            else if (fromColor == BlobColor.BLACK && toColor == BlobColor.GREEN
                        || fromColor == BlobColor.GREEN && toColor == BlobColor.BLACK)
            {
                color = ColorBank._.GetColor("#30624D".ToLower());
            }
            else if (fromColor == BlobColor.BLACK && toColor == BlobColor.BROWN
                        || fromColor == BlobColor.BROWN && toColor == BlobColor.BLACK)
            {
                color = ColorBank._.GetColor("#4A3735".ToLower());
            }
            else if (fromColor == BlobColor.BLACK && toColor == BlobColor.YELLOW
                        || fromColor == BlobColor.YELLOW && toColor == BlobColor.BLACK)
            {
                color = ColorBank._.GetColor("#866B41".ToLower());
            }
            else if (fromColor == BlobColor.BLACK && toColor == BlobColor.BLUE
                        || fromColor == BlobColor.BLUE && toColor == BlobColor.BLACK)
            {
                color = ColorBank._.GetColor("#32517C".ToLower());
            }
            else if (fromColor == BlobColor.BLACK && toColor == BlobColor.PINK
                        || fromColor == BlobColor.PINK && toColor == BlobColor.BLACK)
            {
                color = ColorBank._.GetColor("#B0407E".ToLower());
            }
            else if (fromColor == BlobColor.BLACK && toColor == BlobColor.WHITE
                        || fromColor == BlobColor.WHITE && toColor == BlobColor.BLACK)
            {
                color = ColorBank._.GetColor("#919191".ToLower());
            }
            //
            else
            {
                color = HiddenSettings._.TransparentColor;
            }
            color.a = 0.75f;
            return color;
        }
    }
}
public enum BlobColor
{
    RED, BLUE, YELLOW, GREEN, BROWN, PINK, WHITE, BLACK
}
