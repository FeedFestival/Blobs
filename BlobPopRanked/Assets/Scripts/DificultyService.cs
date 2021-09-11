using System.Linq;
using UnityEngine;
using Assets.Scripts.utils;
using System.Collections.Generic;
using System;

public class DificultyService : MonoBehaviour
{
    [Range(0.0f, 100.0f)]
    public float PercentIncrease = 0f;
    private float _dificultyPercentIncrease
    {
        get
        {
            CalculatePlayTime();

            PercentIncrease = percent.What(_is: SecondsPlayed, _of: HiddenSettings._.MaxTimeSeconds);
            PercentIncrease = PercentIncrease < 100 ? PercentIncrease : 100;
            return 100 - PercentIncrease;
        }
    }
    [Range(0.0f, 100.0f)]
    [SerializeField]
    private float _healthPercent;
    public int AdditionalRows = 0;
    public int Dificulty = 2;
    public int DificultySeed;
    public int MinHits;
    public int MaxHits;
    public int HitsToReset;
    public int Hits;
    private ClasicLv _clasicLv;
    public List<int> Colors;
    public List<int> LevelIncreseThreshhold;
    private DateTime PlayStartedTime;
    [SerializeField]
    private int SecondsPlayed;

    public void Init(ClasicLv levelRandomRanked)
    {
        _clasicLv = levelRandomRanked;

        Colors = new List<int>();
        Colors.Add(0);
        Colors.Add(0);

        SecondsPlayed = 0;
    }

    public void CalculateDificultySeed()
    {
        string zeros = "0";
        for (var i = 0; i < Dificulty - 1; i++)
        {
            zeros += "0";
        }
        DificultySeed = System.Convert.ToInt32("1" + zeros);
        if (_clasicLv.__debug__._dificulty)
        {
            Debug.Log("DificultySeed: " + DificultySeed);
        }
    }

    public void CalculateDificulty()
    {
        AdditionalRows++;
        foreach (int key in LevelIncreseThreshhold)
        {
            if (AdditionalRows == key)
            {
                Dificulty++;
            }
        }
    }

    public BlobColor GetColorByDificulty()
    {
        if (Colors.Count < Dificulty)
        {
            Colors.Add(0);
        }

        if (_clasicLv.__debug__._colorDistribution)
        {
            Debug.Log("GetColorByDificulty() ------------------- " + __debug.DebugList(Colors, "Colors"));
        }

        int colorInt;
        int splitPercentage;
        List<int> percentages;
        List<int> percentageDistribution;

        if (Colors.Any(c => c == 0))
        {
            int countOfZeros = Colors.Count(c => c == 0);
            splitPercentage = 100 / Colors.Count;

            if (_clasicLv.__debug__._colorDistribution)
            {
                Debug.Log("countOfZeros: " + countOfZeros);
                Debug.Log("splitPercentage: " + splitPercentage);
            }

            if (countOfZeros == Colors.Count)
            {
                percentages = GetWithZeroPercentages(splitPercentage, splitPercentage);
                percentageDistribution = SetupPercentages(percentages);

                if (_clasicLv.__debug__._colorDistribution)
                {
                    Debug.Log(__debug.DebugList<int>(percentages, "percentages"));
                    Debug.Log(__debug.DebugList<int>(percentageDistribution, "percentageDistribution"));
                }

                colorInt = ExtractRandomColor(percentageDistribution);
                return ReturnBlobColor(colorInt);
            }

            int countOfNumbers = Colors.Count - countOfZeros;
            int numbersPercentage = splitPercentage / Colors.Count;
            int remainingPercentage = 100 - (numbersPercentage * countOfNumbers);
            int zerosPercentage = remainingPercentage / countOfZeros;

            if (_clasicLv.__debug__._colorDistribution)
            {
                Debug.Log("countOfNumbers: " + countOfNumbers);
                Debug.Log("numbersPercentage: " + numbersPercentage);
                Debug.Log("remainingPercentage: " + remainingPercentage);
                Debug.Log("zerosPercentage: " + zerosPercentage);
            }

            percentages = GetWithZeroPercentages(zerosPercentage, numbersPercentage);
            percentageDistribution = SetupPercentages(percentages);

            colorInt = ExtractRandomColor(percentageDistribution);
            return ReturnBlobColor(colorInt);
        }

        int maxValue = Colors.Max();

        List<float> coeficientColors = new List<float>();
        for (var i = 0; i < Colors.Count; i++)
        {
            coeficientColors.Add((float)maxValue / (float)Colors[i]);
        }
        float coeficientSum = coeficientColors.Sum();
        float commonD = 100 / coeficientSum;

        if (_clasicLv.__debug__._colorDistribution)
        {
            Debug.Log("maxValue: " + maxValue);
            Debug.Log(__debug.DebugList<float>(coeficientColors, "coeficientColors"));
            Debug.Log("coeficientSum: " + coeficientSum);
            Debug.Log("commonD: " + commonD);
        }

        percentages = new List<int>();
        for (var i = 0; i < Colors.Count; i++)
        {
            percentages.Add((int)Mathf.Floor(coeficientColors[i] * commonD));
        }

        int remaineder = 100 - percentages.Sum();
        percentages[percentages.Count - 1] += remaineder;
        percentageDistribution = SetupPercentages(percentages);

        if (_clasicLv.__debug__._colorDistribution)
        {
            Debug.Log("remaineder: " + remaineder);
            Debug.Log(__debug.DebugList<int>(percentages, "percentages"));
            Debug.Log(__debug.DebugList<int>(percentageDistribution, "percentageDistribution"));
        }

        colorInt = ExtractRandomColor(percentageDistribution);
        return ReturnBlobColor(colorInt);
    }

    private List<int> GetWithZeroPercentages(int zerosPercentage, int numbersPercentage)
    {
        List<int> percentages = new List<int>();
        for (int i = 0; i < Colors.Count; i++)
        {
            if (Colors[i] == 0)
            {
                percentages.Add(zerosPercentage);
            }
            else
            {
                percentages.Add(numbersPercentage);
            }
        }
        return percentages;
    }

    private List<int> SetupPercentages(List<int> percentages)
    {
        for (int i = 1; i < percentages.Count; i++)
        {
            percentages[i] = percentages[i - 1] + percentages[i];
        }
        return percentages;
    }

    private int ExtractRandomColor(List<int> percentages)
    {
        int randomNumber = UnityEngine.Random.Range(0, 100);
        int index = percentages.FindIndex(p => randomNumber < p);

        if (_clasicLv.__debug__._colorDistribution)
        {
            Debug.Log("randomNumber: " + randomNumber + ", indexIn_colors: " + index);
        }
        ChangeColorNumbers(index);
        return index;
    }

    public void ChangeColorNumbers(int index, bool add = true)
    {
        if (add)
        {
            Colors[index]++;
        }
        else
        {
            Colors[index]--;
        }
        if (_clasicLv.__debug__._colorDistribution)
        {
            Debug.Log(__debug.DebugList<int>(Colors, "Colors"));
        }
    }

    public BlobColor ReturnBlobColor(int colorInt)
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

    public int GetBlobColorPoints(BlobColor blobColor)
    {
        int multiplier = 1;
        switch (blobColor)
        {
            case BlobColor.BLUE:
                multiplier = 2;
                break;
            case BlobColor.YELLOW:
                multiplier = 4;
                break;
            case BlobColor.GREEN:
                multiplier = 3;
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

    public void CheckIfAddingNewRow()
    {
        bool isAtLeastOnBlobConnectedToCeil = (_clasicLv.Blobs == null || _clasicLv.Blobs.Count == 0) == false;
        // Debug.Log("_clasicLv.Blobs.Count: " + _clasicLv.Blobs.Count);
        if (isAtLeastOnBlobConnectedToCeil == false)
        {
            isAtLeastOnBlobConnectedToCeil = _clasicLv.Blobs.Exists(b =>
            {
                bool isConnectedToCeil = b.StickedTo.Exists(s => s == HiddenSettings._.CeilId);
                return isConnectedToCeil;
            });
        }

        if (isAtLeastOnBlobConnectedToCeil)
        {
            float blobY = _clasicLv.Blobs.Min(b => b.transform.position.y);

            float dashedLine = -4.23f;
            float newDashedLine = dashedLine - HiddenSettings._.WallStickLimit;
            float newBlobY = blobY - HiddenSettings._.WallStickLimit;

            int minHealthPercent = 25;
            _healthPercent = Mathf.CeilToInt(percent.What(_is: newBlobY, _of: newDashedLine));
            _healthPercent = _healthPercent > minHealthPercent ? _healthPercent : minHealthPercent;
            // Debug.Log("healthPercent: " + _healthPercent);


            int minHits = Mathf.CeilToInt(percent.Find(_dificultyPercentIncrease, MinHits));
            if (minHits <= 0)
            {
                minHits = 1;
            }
            // Debug.Log("minHits: " + minHits);
            int maxHits = Mathf.CeilToInt(percent.Find(_dificultyPercentIncrease, MaxHits));
            maxHits = maxHits > MinHits ? maxHits : MinHits;
            // Debug.Log("maxHits: " + maxHits);

            HitsToReset = Mathf.CeilToInt(percent.Find(_healthPercent, maxHits));

            if (HitsToReset < minHits)
            {
                HitsToReset = (int)Mathf.Ceil(minHits);
            }
            else if (HitsToReset > maxHits)
            {
                HitsToReset = (int)Mathf.Ceil(maxHits);
            }

            Hits++;
            if (Hits >= HitsToReset)
            {
                Hits = 0;
                _clasicLv.AddAnotherBlobLevel();
            }
            else if (_healthPercent > 80)
            {
                _clasicLv.ActivateEndGame();
            }
        }
        else
        {
            _clasicLv.AddAnotherBlobLevel();
        }
    }

    public void CalculatePlayTime(bool start = false)
    {
        if (start)
        {
            PlayStartedTime = DateTime.Now;
            return;
        }

        var timeNow = DateTime.Now;
        TimeSpan ts = PlayStartedTime - timeNow;
        SecondsPlayed += Mathf.Abs(Mathf.CeilToInt(Convert.ToInt32(ts.TotalSeconds)));
    }
}
