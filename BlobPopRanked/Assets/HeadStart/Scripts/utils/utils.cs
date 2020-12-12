using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Assets.Scripts.utils
{
    public static class utils
    {
        public static readonly string _version = "1.0.3";
        public static string ConvertNumberToK(int num)
        {
            if (num >= 1000)
                return string.Concat(num / 1000, "k");
            else
                return num.ToString();
        }

        public static void VarDump<T>(T obj)
        {
            foreach (var propertyInfo in obj.GetType()
                                .GetProperties(
                                        BindingFlags.Public
                                        | BindingFlags.Instance))
            {
                var x = propertyInfo;
                Debug.Log(propertyInfo);
            }
        }

        public static void DumpToConsole(object obj, bool isArray = false)
        {
            string output = string.Empty;
            if (isArray)
            {
                output = JsonHelper.ToJson<object>(obj);
            }
            else
            {
                output = JsonUtility.ToJson(obj, true);
            }
            Debug.Log(output);
        }

        public static void DumpToJsonConsole(IJsonConsole[] jsonConsoles)
        {
            foreach (var json in jsonConsoles)
            {
                Debug.Log(json.ToJsonString());
            }
        }

        public static string RemoveWhitespace(this string input)
        {
            return new string(input.ToCharArray()
                .Where(c => !Char.IsWhiteSpace(c))
                .ToArray());
        }

        public static string DebugList(List<int> array, string name)
        {
            string debug = string.Empty;
            foreach (int bId in array)
            {
                debug += bId + ",";
            }
            return name + " [" + debug + "](" + array.Count + ")";
        }

        public delegate string DebugFunc<T>(T obj);

        public static string DebugList<T>(List<T> array, string name = null, DebugFunc<T> debugFunc = null)
        {
            string debug = string.Empty;
            foreach (T bId in array)
            {
                if (debugFunc == null)
                {
                    debug += bId.ToString() + " ; ";
                }
                else
                {
                    debug += debugFunc(bId);
                }
            }
            return string.IsNullOrEmpty(name) ? debug
                : name + " [" + debug + "](" + array.Count + ")";
        }

        public static string DebugQueue<T>(Queue<T> array, string name)
        {
            string debug = string.Empty;
            foreach (T t in array)
            {
                debug += JsonHelper.ToJson<T>(t);
            }
            return name + " (" + debug + ")[" + array.Count + "]";
        }

        public static string DebugDict<T>(Dictionary<T, T> array, string name = null)
        {
            string debug = string.Empty;
            foreach (var bId in array)
            {
                debug += "[" + bId.Key.ToString() + "] " + bId.Value.ToString();
            }
            return string.IsNullOrEmpty(name) ? debug
                : name + " [" + debug + "](" + array.Count + ")";
        }

        public static void AddIfNone(int value, ref List<int> array, string debugAdd = null)
        {
            if (array.Contains(value))
            {
                return;
            }
            array.Add(value);
            if (string.IsNullOrEmpty(debugAdd) == false)
            {
                Debug.Log(debugAdd);
            }
        }

        public static int CreateLayerMask(bool aExclude, params int[] aLayers)
        {
            int v = 0;
            foreach (var L in aLayers)
                v |= 1 << L;
            if (aExclude)
                v = ~v;
            return v;
        }
    }

    public static class percent
    {
        public static float Find(float _percent, float _of)
        {
            return (_of / 100f) * _percent;
        }
        public static float What(float _is, float _of)
        {
            return (_is * 100f) / _of;
        }

        public static int PennyToss(int _from = 0, int _to = 100)
        {
            var randomNumber = Random.Range(_from, _to);
            return (randomNumber > 50) ? 1 : 0;
        }

        public static T GetRandomFromArray<T>(T[] list)
        {
            List<int> percentages = new List<int>();
            int splitPercentages = Mathf.FloorToInt(100 / list.Length);
            int remainder = 100 - (splitPercentages * list.Length);
            for (int i = 0; i < list.Length; i++)
            {
                int percent = i == (list.Length - 1) ? splitPercentages + remainder : splitPercentages;
                percentages.Add(percent);
            }
            for (int i = 1; i < percentages.Count; i++)
            {
                percentages[i] = percentages[i - 1] + percentages[i];
            }
            int randomNumber = UnityEngine.Random.Range(0, 100);
            int index = percentages.FindIndex(p => randomNumber < p);
            percentages = null;
            return list[index];
        }

        public static T GetRandomFromList<T>(List<T> list)
        {
            List<int> percentages = new List<int>();
            int splitPercentages = Mathf.FloorToInt(100 / list.Count);
            int remainder = 100 - (splitPercentages * list.Count);
            for (int i = 0; i < list.Count; i++)
            {
                int percent = i == (list.Count - 1) ? splitPercentages + remainder : splitPercentages;
                percentages.Add(percent);
            }
            for (int i = 1; i < percentages.Count; i++)
            {
                percentages[i] = percentages[i - 1] + percentages[i];
            }
            int randomNumber = UnityEngine.Random.Range(0, 100);
            int index = percentages.FindIndex(p => randomNumber < p);
            percentages = null;
            return list[index];
        }
    }

    public static class world2d
    {
        public static Vector2 GetNormalizedDirection(Vector2 lastVelocity, Vector2 collisionNormal)
        {
            return Vector2.Reflect(lastVelocity.normalized, collisionNormal).normalized;
        }

        public static Vector3 LookRotation2D(Vector2 from, Vector2 to, bool fromFront = false)
        {
            Vector2 vectorToTarget = to - from;
            float angle = Mathf.Atan2(vectorToTarget.y, vectorToTarget.x) * Mathf.Rad2Deg;
            Quaternion q = Quaternion.AngleAxis(angle, Vector3.forward);
            if (fromFront)
            {
                return new Vector3(q.eulerAngles.x, q.eulerAngles.y, q.eulerAngles.z - 90);
            }
            return q.eulerAngles;
        }
    }
}
