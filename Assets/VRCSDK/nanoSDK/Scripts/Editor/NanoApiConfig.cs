using System;
using System.IO;
using JetBrains.Annotations;
using Unity.Plastic.Newtonsoft.Json;
using UnityEngine;

namespace nanoSDK
{
    public static class NanoApiConfig
    {
        private static NanoConf _internalConfig;
        private static readonly string Path = Application.persistentDataPath + "/apiConfig.json";

        public static NanoConf Config
        {
            get
            {
                TryLoad();
                return _internalConfig;
            }
        }

        static NanoApiConfig() => TryLoad();

        private static void TryLoad()
        {
            if (File.Exists(Path))
            {
                var json = File.ReadAllText(Path);
                if (!string.IsNullOrEmpty(json))
                {
                    try
                    {
                        _internalConfig = JsonConvert.DeserializeObject<NanoConf>(json);
                    }
                    catch (Exception ex)
                    {
                        Debug.Log(ex);
                    }
                }
            }

            if (_internalConfig != null) return;
            _internalConfig = new NanoConf();
            Save();
        }


        public static void Save()
        {
            try
            {
                File.WriteAllText(Path, JsonConvert.SerializeObject(_internalConfig));
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }

        public class NanoConf
        {
            [CanBeNull] public string AuthKey { get; set; }
        }
    }
}