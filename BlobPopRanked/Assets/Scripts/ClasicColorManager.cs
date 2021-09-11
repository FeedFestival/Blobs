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
                    || fromColor == BlobColor.RED && toColor == BlobColor.PINK) {
            color = ColorBank._.GetColor("#C3337E".ToLower());
        }
        else if (fromColor == BlobColor.PINK && toColor == BlobColor.GREEN
                    || fromColor == BlobColor.GREEN && toColor == BlobColor.PINK) {
            color = ColorBank._.GetColor("#805B56".ToLower());
        }
        else if (fromColor == BlobColor.PINK && toColor == BlobColor.BROWN
                    || fromColor == BlobColor.BROWN && toColor == BlobColor.PINK) {
            color = ColorBank._.GetColor("#B06D8A".ToLower());
        }
        else if (fromColor == BlobColor.PINK && toColor == BlobColor.YELLOW
                    || fromColor == BlobColor.YELLOW && toColor == BlobColor.PINK) {
            color = ColorBank._.GetColor("#F08588".ToLower());
        }
        else if (fromColor == BlobColor.PINK && toColor == BlobColor.BLUE
                    || fromColor == BlobColor.BLUE && toColor == BlobColor.PINK) {
            color = ColorBank._.GetColor("#9D6AC3".ToLower());
        }
        else if (fromColor == BlobColor.WHITE && toColor == BlobColor.RED
                    || fromColor == BlobColor.RED && toColor == BlobColor.WHITE) {
            color = ColorBank._.GetColor("#F880A8".ToLower());
        }
        else if (fromColor == BlobColor.WHITE && toColor == BlobColor.GREEN
                    || fromColor == BlobColor.GREEN && toColor == BlobColor.WHITE) {
            color = ColorBank._.GetColor("#A2D4BF".ToLower());
        }
        else if (fromColor == BlobColor.WHITE && toColor == BlobColor.BROWN
                    || fromColor == BlobColor.BROWN && toColor == BlobColor.WHITE) {
            color = ColorBank._.GetColor("#C38B86".ToLower());
        }
        else if (fromColor == BlobColor.WHITE && toColor == BlobColor.YELLOW
                    || fromColor == BlobColor.YELLOW && toColor == BlobColor.WHITE) {
            color = ColorBank._.GetColor("#FFF294".ToLower());
        }
        else if (fromColor == BlobColor.WHITE && toColor == BlobColor.BLUE
                    || fromColor == BlobColor.BLUE && toColor == BlobColor.WHITE) {
            color = ColorBank._.GetColor("#99DBF8".ToLower());
        }
        else if (fromColor == BlobColor.WHITE && toColor == BlobColor.PINK
                    || fromColor == BlobColor.PINK && toColor == BlobColor.WHITE) {
            color = ColorBank._.GetColor("#F8A6D4".ToLower());
        }
        //
        else if (fromColor == BlobColor.BLACK && toColor == BlobColor.RED
                    || fromColor == BlobColor.RED && toColor == BlobColor.BLACK) {
            color = ColorBank._.GetColor("#850E35".ToLower());
        }
        else if (fromColor == BlobColor.BLACK && toColor == BlobColor.GREEN
                    || fromColor == BlobColor.GREEN && toColor == BlobColor.BLACK) {
            color = ColorBank._.GetColor("#30624D".ToLower());
        }
        else if (fromColor == BlobColor.BLACK && toColor == BlobColor.BROWN
                    || fromColor == BlobColor.BROWN && toColor == BlobColor.BLACK) {
            color = ColorBank._.GetColor("#4A3735".ToLower());
        }
        else if (fromColor == BlobColor.BLACK && toColor == BlobColor.YELLOW
                    || fromColor == BlobColor.YELLOW && toColor == BlobColor.BLACK) {
            color = ColorBank._.GetColor("#866B41".ToLower());
        }
        else if (fromColor == BlobColor.BLACK && toColor == BlobColor.BLUE
                    || fromColor == BlobColor.BLUE && toColor == BlobColor.BLACK) {
            color = ColorBank._.GetColor("#32517C".ToLower());
        }
        else if (fromColor == BlobColor.BLACK && toColor == BlobColor.PINK
                    || fromColor == BlobColor.PINK && toColor == BlobColor.BLACK) {
            color = ColorBank._.GetColor("#B0407E".ToLower());
        }
        else if (fromColor == BlobColor.BLACK && toColor == BlobColor.WHITE
                    || fromColor == BlobColor.WHITE && toColor == BlobColor.BLACK) {
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
}

public enum BlobColor
{
    RED, BLUE, YELLOW, GREEN, BROWN, PINK, WHITE, BLACK
}