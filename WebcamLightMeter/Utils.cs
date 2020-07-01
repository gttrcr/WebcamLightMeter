using Accord;
using System;
using System.Collections.Generic;
using System.Linq;

namespace WebcamLightMeter
{
    public class Utils
    {
        public static void AddOrUpdateDictionary(ref Dictionary<string, List<Tuple<string, double>>> dictionary, string key, double value)
        {
            string dateTime = DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss.fff");
            if (dictionary.ContainsKey(key))
                dictionary[key].Add(new Tuple<string, double>(dateTime, value));
            else
                dictionary.Add(key, new List<Tuple<string, double>>() { new Tuple<string, double>(dateTime, value) });
        }

        public static string BeaufityStringOutput(Dictionary<string, double> outputDictionary, string title)
        {
            string ret = title + Environment.NewLine + Environment.NewLine;
            for (int i = 0; i < outputDictionary.Keys.Count; i++)
                ret += outputDictionary.Keys.ElementAt(i) + ": " + outputDictionary[outputDictionary.Keys.ElementAt(i)] + Environment.NewLine;
            return ret;
        }
    }
}