using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Unity.RemoteConfig;
using UnityEditor;
using UnityEngine;

public static class Settings
{
    public static float PlayerMoveSpeed { get; private set; } = 5f;
    public static float PlayerJumpForce { get; private set; } = 250f;
    public static float SpawnBuffer     { get; private set; } = 1f;
    public static float BulletSpeed     { get; private set; } = 10f;
}

public static class RemoteSettings
{
    private static bool      _initialized;
    private static TextAsset _csv;

    private struct UserAttributes { }
    private struct AppAttributes { }
    
    public static void Init(TextAsset newCsv)
    {
        if(!_initialized)
            ConfigManager.FetchCompleted += UpdateSettings;

        _csv = newCsv;
        
        ConfigManager.FetchConfigs(new UserAttributes(), new AppAttributes());
        _initialized = true;
    }

    private static void UpdateSettings(ConfigResponse response)
    {
        Debug.Log($"Config Manager returned {response.requestOrigin}.");

        var floats = typeof(Settings)
            .GetProperties()
            .Where(p => p.PropertyType == typeof(float))
            .ToList();

        foreach(var f in floats)
        {
            if(!ConfigManager.appConfig.HasKey(f.Name))
                continue;
            
            var oldVal = (float)f.GetValue(f);
            var newVal = ConfigManager.appConfig.GetFloat(f.Name, oldVal);
            f.SetValue(f, newVal);
            
            Debug.Log($"{f.Name}: {oldVal}, {newVal}.");
        }

#if UNITY_EDITOR
    UpdateCsv(floats);
#endif
    }

#if UNITY_EDITOR
    private static void UpdateCsv(List<PropertyInfo> properties)
    {
        var buffer = new StringBuilder("key,type,value\n");

        foreach(var p in properties)
            buffer.Append($"{p.Name},float,{p.GetValue(p)}\n");
        
        File.WriteAllText(AssetDatabase.GetAssetPath(_csv), buffer.ToString());
        EditorUtility.SetDirty(_csv);
    }
#endif
}