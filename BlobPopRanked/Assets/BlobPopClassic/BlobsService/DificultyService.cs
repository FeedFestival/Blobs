using System.Linq;
using UnityEngine;
using Assets.Scripts.utils;
using System.Collections.Generic;
using System;
using Assets.BlobPopClassic.BlobPopColor;

namespace Assets.BlobPopClassic.BlobsService
{
    public class DificultyService : MonoBehaviour
    {
        [Range(0.0f, 100.0f)]
        public float PercentIncrease = 0f;
        private readonly int MAX_TIME_SECONDS = 600;
        private float _dificultyPercentIncrease
        {
            get
            {
                CalculatePlayTime();

                PercentIncrease = __percent.What(_is: SecondsPlayed, _of: MAX_TIME_SECONDS);
                PercentIncrease = PercentIncrease < 100 ? PercentIncrease : 100;
                return 100 - PercentIncrease;
            }
        }
        [Range(0.0f, 100.0f)]
        [SerializeField]
        private float _healthPercent;
        public int AdditionalRows = 0;
        public int Dificulty = 2;
        public int MinHits;
        public int MaxHits;
        public int HitsToReset;
        public int Hits;
        public List<int> Colors;
        public List<int> LevelIncreseThreshhold;    // 8 to have: Pink, White, Black // 5 to have snooker
        private DateTime PlayStartedTime;
        [SerializeField]
        private int SecondsPlayed;
        public bool BrownIsWall;

        public void Init()
        {

            Colors = new List<int>();
            Colors.Add(0);
            Colors.Add(0);

            SecondsPlayed = 0;
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

        public BlobColor GetColorByDificulty(bool newBlob = false)
        {
            if (Colors.Count < Dificulty && LevelIncreseThreshhold.Count > Colors.Count)
            {
                Colors.Add(0);
            }

            int colorInt;
            int splitPercentage;
            List<int> percentages;
            List<int> percentageDistribution;

            if (BrownIsWall && Colors.Count > (int)BlobColor.BROWN)
            {
                int allOtherCombined = 0;
                for (var i = 0; i < Colors.Count; i++)
                {
                    if (i != (int)BlobColor.BROWN)
                    {
                        allOtherCombined += Colors[i];
                    }
                }
                Colors[(int)BlobColor.BROWN] = (int)__percent.Find(_healthPercent, allOtherCombined);
            }

            if (Colors.Any(c => c == 0))
            {
                int countOfZeros = Colors.Count(c => c == 0);
                splitPercentage = 100 / Colors.Count;

                if (countOfZeros == Colors.Count)
                {
                    percentages = GetWithZeroPercentages(splitPercentage, splitPercentage);
                    percentageDistribution = SetupPercentages(percentages);
                    colorInt = ExtractRandomColor(percentageDistribution, newBlob);
                    return BlobColorService.ReturnBlobColor(colorInt);
                }

                int countOfNumbers = Colors.Count - countOfZeros;
                int numbersPercentage = splitPercentage / Colors.Count;
                int remainingPercentage = 100 - (numbersPercentage * countOfNumbers);
                int zerosPercentage = remainingPercentage / countOfZeros;

                percentages = GetWithZeroPercentages(zerosPercentage, numbersPercentage);
                percentageDistribution = SetupPercentages(percentages);

                colorInt = ExtractRandomColor(percentageDistribution, newBlob);
                return BlobColorService.ReturnBlobColor(colorInt);
            }
            int maxValue = Colors.Max();

            List<float> coeficientColors = new List<float>();
            for (var i = 0; i < Colors.Count; i++)
            {
                coeficientColors.Add((float)maxValue / (float)Colors[i]);
            }
            float coeficientSum = coeficientColors.Sum();
            float commonD = 100 / coeficientSum;

            percentages = new List<int>();
            for (var i = 0; i < Colors.Count; i++)
            {
                percentages.Add((int)Mathf.Floor(coeficientColors[i] * commonD));
            }

            int remaineder = 100 - percentages.Sum();
            percentages[percentages.Count - 1] += remaineder;
            percentageDistribution = SetupPercentages(percentages);

            colorInt = ExtractRandomColor(percentageDistribution, newBlob);
            return BlobColorService.ReturnBlobColor(colorInt);
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

        private int ExtractRandomColor(List<int> percentages, bool newBlob)
        {
            int randomNumber = UnityEngine.Random.Range(0, 100);
            int index = percentages.FindIndex(p => randomNumber < p);

            // When making a new Blob and BROWN is Wall
            //  - we dont want to shoot brown so we pick the lowest color
            int brownIndex = (int)BlobColor.BROWN;
            if (BrownIsWall && newBlob && index == brownIndex)
            {
                int minValue = Colors.Max();
                for (var i = 0; i < Colors.Count; i++)
                {
                    if (i == brownIndex)
                    {
                        continue;
                    }
                    if (Colors[i] < minValue)
                    {
                        minValue = Colors[i];
                    }
                }
                int minIndex = Colors.FindIndex(c => c == minValue);
                index = minIndex;
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
        }



        public void CheckIfAddingNewRow()
        {
            bool isAtLeastOnBlobConnectedToCeil = (BlobPopClassic._.Blobs == null || BlobPopClassic._.Blobs.Count == 0) == false;

            if (isAtLeastOnBlobConnectedToCeil == false)
            {
                isAtLeastOnBlobConnectedToCeil = BlobPopClassic._.Blobs.Exists(b =>
                {
                    bool isConnectedToCeil = b.StickedTo.Exists(s => s == BlobPopClassic._.CEILD_ID);
                    return isConnectedToCeil;
                });
            }

            if (isAtLeastOnBlobConnectedToCeil)
            {
                float blobY = BlobPopClassic._.Blobs.Min(b => b.transform.position.y);

                float dashedLine = -4.23f;
                float newDashedLine = dashedLine - BlobPopClassic._.WALL_STICK_LIMIT;
                float newBlobY = blobY - BlobPopClassic._.WALL_STICK_LIMIT;

                int minHealthPercent = 25;
                _healthPercent = Mathf.CeilToInt(__percent.What(_is: newBlobY, _of: newDashedLine));
                _healthPercent = _healthPercent > minHealthPercent ? _healthPercent : minHealthPercent;


                int minHits = Mathf.CeilToInt(__percent.Find(_dificultyPercentIncrease, MinHits));
                if (minHits <= 0)
                {
                    minHits = 1;
                }
                int maxHits = Mathf.CeilToInt(__percent.Find(_dificultyPercentIncrease, MaxHits));
                maxHits = maxHits > MinHits ? maxHits : MinHits;

                HitsToReset = Mathf.CeilToInt(__percent.Find(_healthPercent, maxHits));

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
                    BlobPopClassic._.AddAnotherBlobLevel();
                }
                else if (_healthPercent > 80)
                {
                    BlobPopClassic._.ActivateEndGame();
                }
            }
            else
            {
                BlobPopClassic._.AddAnotherBlobLevel();
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
}
