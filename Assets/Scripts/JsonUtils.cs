using System;
using System.Collections.Generic;
using System.IO;
using Common.Json;
using Newtonsoft.Json;
using UnityEngine;
using Formatting = Newtonsoft.Json.Formatting;

namespace Common.Json
{
    public static class JsonUtils
    {
        private static JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.Auto,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        public static void WriteJson<T>(T obj, string relativeStreamingAssetsPath)
        {
            if (string.IsNullOrEmpty(relativeStreamingAssetsPath))
            {
                return;
            }

            string fullPath = Path.Combine(Application.streamingAssetsPath, $"{relativeStreamingAssetsPath}.json");

            if (File.Exists(fullPath) == false)
            {
                string directoryName = Path.GetDirectoryName(fullPath);
                if (string.IsNullOrEmpty(directoryName) == false)
                {
                    if (Directory.Exists(directoryName) == false)
                    {
                        Directory.CreateDirectory(directoryName);
                    }
                }
            }

            string json = GetJson<T>(obj);
            File.WriteAllText(fullPath, json);
        }

        public static string GetJson<T>(T obj)
        {
            string json = JsonConvert.SerializeObject((T) obj, Formatting.Indented,_jsonSerializerSettings);
            return json;
        }

        public static T ReadJson<T>(string relativeStreamingAssetsPath)
        {
            try
            {
                string fullPath = Path.Combine(Application.streamingAssetsPath, $"{relativeStreamingAssetsPath}.json");
                string json = File.ReadAllText(fullPath);
                T t = ParseJson<T>(json);
                return t;
            }
            catch (Exception e)
            {
                Debug.LogError($"读取文件失败：{e}");
                return default;
            }
        }

        public static T ParseJson<T>(string json)
        {
            T t = JsonConvert.DeserializeObject<T>(json, _jsonSerializerSettings);
            return t;
        }
    }
}