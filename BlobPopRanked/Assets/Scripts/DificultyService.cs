using System.Linq;
using UnityEngine;
using Assets.Scripts.utils;

public class DificultyService : MonoBehaviour
{
    public int AdditionalRows = 0;
    public int Dificulty = 2;
    public int DificultySeed;
    public int MaxHits;
    public int HitsToReset;
    public int Hits;
    private LevelRandomRanked _levelRandomRanked;

    public void Init(LevelRandomRanked levelRandomRanked)
    {
        _levelRandomRanked = levelRandomRanked;
    }

    public void CalculateDificultySeed()
    {
        string zeros = "0";
        for (var i = 0; i < Dificulty - 1; i++)
        {
            zeros += "0";
        }
        DificultySeed = System.Convert.ToInt32("1" + zeros);
        if (_levelRandomRanked.DebugController.DebugDificulty)
        {
            Debug.Log("DificultySeed: " + DificultySeed);
        }
    }

    public void CalculateDificulty()
    {
        AdditionalRows++;

        if (AdditionalRows == 3)
        {
            Dificulty++;
        }
        else if (AdditionalRows == 5)
        {
            Dificulty++;
        }
        else if (AdditionalRows == 8)
        {
            Dificulty++;
        }
        else if (AdditionalRows == 13)
        {
            Dificulty++;
        }
    }

    public BlobColor GetColorByDificulty()
    {
        var randomNumber = UnityEngine.Random.Range(0, DificultySeed);
        int colorInt = 0;
        if (Dificulty == 2)
        {
            colorInt = (randomNumber > (DificultySeed / 2)) ? 2 : 1;
            return ReturnBlobColor(colorInt);
        }
        else
        {
            var div = DificultySeed / Dificulty;
            var parts = new int[Dificulty + 1];
            for (var i = 1; i <= Dificulty; i++)
            {
                parts[i] = i * div;
                if (randomNumber < parts[i])
                {
                    colorInt = i;
                    break;
                }
            }
        }
        return ReturnBlobColor(colorInt);
    }

    public BlobColor ReturnBlobColor(int colorInt)
    {
        switch (colorInt)
        {
            case 2:
                return BlobColor.AtlantisColor;
            case 3:
                return BlobColor.RoyalBlue;
            case 4:
                return BlobColor.Candlelight;
            case 5:
                return BlobColor.MediumPurple;
            case 1:
            default:
                return BlobColor.PomegranateColor;
        }
    }

    public void CheckIfAddingNewRow()
    {
        bool isAtLeastOnBlobConnectedToCeil = _levelRandomRanked.Blobs.Exists(b =>
        {
            bool isConnectedToCeil = b.StickedTo.Exists(s => s == HiddenSettings._.CeilId);
            return isConnectedToCeil;
        });

        if (isAtLeastOnBlobConnectedToCeil)
        {
            float maxYBlob = _levelRandomRanked.Blobs.Min(b => b.transform.position.y);
            float min = -3f;
            maxYBlob = maxYBlob - min;
            float max = 4.44f;
            float total = max - min;
            float colorPerc = percent.What(_is: maxYBlob, _of: total);
            // Debug.Log("colorPerc: " + colorPerc);
            float perc = (100 - colorPerc) + 10;
            // Debug.Log("perc: " + perc);

            HitsToReset = (int)Mathf.Ceil(percent.Find(perc, _of: MaxHits));
            HitsToReset = HitsToReset + (int)Mathf.Ceil(percent.Find(colorPerc, Dificulty));
            if (Hits == HitsToReset)
            {
                Hits = 0;
                _levelRandomRanked.AddAnotherBlobLevel();
            }
            else
            {
                Hits++;
            }
        }
        else
        {
            _levelRandomRanked.AddAnotherBlobLevel();
        }
    }
}
