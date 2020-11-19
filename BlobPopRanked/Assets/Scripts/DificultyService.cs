using System.Linq;
using UnityEngine;
using Assets.Scripts.utils;
using System.Collections.Generic;

public class DificultyService : MonoBehaviour
{
    public int AdditionalRows = 0;
    public int Dificulty = 2;
    public int DificultySeed;
    public int MaxHits;
    public int HitsToReset;
    public int Hits;
    private LevelRandomRanked _levelRandomRanked;
    public List<int> Colors;

    public void Init(LevelRandomRanked levelRandomRanked)
    {
        _levelRandomRanked = levelRandomRanked;

        Colors = new List<int>();
        Colors.Add(0);
        Colors.Add(0);
    }

    public void CalculateDificultySeed()
    {
        string zeros = "0";
        for (var i = 0; i < Dificulty - 1; i++)
        {
            zeros += "0";
        }
        DificultySeed = System.Convert.ToInt32("1" + zeros);
        if (_levelRandomRanked.debugLvl._dificulty)
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
        if (Colors.Count < Dificulty)
        {
            Colors.Add(0);
        }

        if (_levelRandomRanked.debugLvl._colorDistribution)
        {
            Debug.Log("GetColorByDificulty() ------------------- " + utils.DebugList(Colors, "Colors"));
        }

        int colorInt;
        int splitPercentage;
        List<int> percentages;
        List<int> percentageDistribution;

        if (Colors.Any(c => c == 0))
        {
            int countOfZeros = Colors.Count(c => c == 0);
            splitPercentage = 100 / Colors.Count;

            if (_levelRandomRanked.debugLvl._colorDistribution)
            {
                Debug.Log("countOfZeros: " + countOfZeros);
                Debug.Log("splitPercentage: " + splitPercentage);
            }

            if (countOfZeros == Colors.Count)
            {
                percentages = GetWithZeroPercentages(splitPercentage, splitPercentage);
                percentageDistribution = SetupPercentages(percentages);

                if (_levelRandomRanked.debugLvl._colorDistribution)
                {
                    Debug.Log(utils.DebugList<int>(percentages, "percentages"));
                    Debug.Log(utils.DebugList<int>(percentageDistribution, "percentageDistribution"));
                }

                colorInt = ExtractRandomColor(percentageDistribution);
                return ReturnBlobColor(colorInt);
            }

            int countOfNumbers = Colors.Count - countOfZeros;
            int numbersPercentage = splitPercentage / Colors.Count;
            int remainingPercentage = 100 - (numbersPercentage * countOfNumbers);
            int zerosPercentage = remainingPercentage / countOfZeros;

            if (_levelRandomRanked.debugLvl._colorDistribution)
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

        if (_levelRandomRanked.debugLvl._colorDistribution)
        {
            Debug.Log("maxValue: " + maxValue);
            Debug.Log(utils.DebugList<float>(coeficientColors, "coeficientColors"));
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

        if (_levelRandomRanked.debugLvl._colorDistribution)
        {
            Debug.Log("remaineder: " + remaineder);
            Debug.Log(utils.DebugList<int>(percentages, "percentages"));
            Debug.Log(utils.DebugList<int>(percentageDistribution, "percentageDistribution"));
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

        if (_levelRandomRanked.debugLvl._colorDistribution)
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
        if (_levelRandomRanked.debugLvl._colorDistribution)
        {
            Debug.Log(utils.DebugList<int>(Colors, "Colors"));
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
            case 0:
            default:
                return BlobColor.RED;
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
            float blobY = _levelRandomRanked.Blobs.Min(b => b.transform.position.y);

            Debug.Log("blobY: " + blobY);
            float overZero = 4.44f;
            float dashedLine = -4.23f;
            float newDashedLine = dashedLine - overZero;
            float newBlobY = blobY - overZero;

            Debug.Log("newDashedLine: " + newDashedLine);
            Debug.Log("newBlobY: " + newBlobY);

            float healthPercent = percent.What(_is: newBlobY, _of: newDashedLine);
            Debug.Log("healthPercent: " + healthPercent);

            HitsToReset = (int)Mathf.Ceil(percent.Find(healthPercent, _of: MaxHits));

            // HitsToReset = HitsToReset + (int)Mathf.Floor(percent.Find(healthPercent, Dificulty));
            // Debug.Log("HitsToReset: " + HitsToReset);

            if (HitsToReset < 1)
            {
                HitsToReset = 1;
            }
            else if (HitsToReset > 7)
            {
                HitsToReset = 7;
            }
            Debug.Log("HitsToReset: " + HitsToReset);

            Hits++;
            if (Hits >= HitsToReset)
            {
                Hits = 0;
                _levelRandomRanked.AddAnotherBlobLevel();
            }
        }
        else
        {
            _levelRandomRanked.AddAnotherBlobLevel();
        }
    }
}
