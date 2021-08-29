using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClasicColorManager : MonoBehaviour
{
    public Color GetColorByBlobColor(BlobColor blobColor)
    {
        return ColorBank._.GetColorByName(ColorName(blobColor));
    }

    internal Color GetLinkColor(BlobColor fromColor, BlobColor toColor)
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
        //
        else
        {
            color = HiddenSettings._.TransparentColor;
        }
        color.a = 0.75f;
        return color;
    }

    public string ColorName(BlobColor clobColor)
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
                return "Brown_Ferra";
                // return "White_Black_Haze";
            case BlobColor.RED:
            default:
                return "Red_Torch";
        }
    }
}

public enum BlobColor
{
    RED, BLUE, YELLOW, GREEN, BROWN
}