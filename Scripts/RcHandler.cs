using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Unity.RemoteConfig;
using UnityEngine;
using UnityEngine.Networking;

public static class RcHandler
{
    private static bool _initialized;

    private struct UserAttributes { }
    private struct AppAttributes { }

    public static void Init()
    {
        if(!_initialized)
        {
            ConfigManager.FetchCompleted += UpdateSettings;
            _initialized                 =  true;
        }

        ConfigManager.FetchConfigs(new UserAttributes(), new AppAttributes());
    }

    private static void UpdateSettings(ConfigResponse response)
    {
        Debug.Log($"Config Manager returned {response.requestOrigin}.");

        var propertyInfos = typeof(Settings).GetProperties();

        foreach(var prop in propertyInfos)
        {
            if(!ConfigManager.appConfig.HasKey(prop.Name))
                continue;

            var newVal = ConfigMethods[prop.PropertyType](prop);
            prop.SetValue(prop, newVal);
        }

#if UNITY_EDITOR
        UpdateRemote(propertyInfos);
#endif
    }

#if UNITY_EDITOR

    private static string PROJECT_ID = MyInfo.PROJECT_ID;

    private static void UpdateRemote(PropertyInfo[] properties)
    {
        var data    = new List<UpdateConfigsPayload<object>.ConfigData>();
        foreach(var p in properties)
        {
            var t  = ConfigTypes[p.PropertyType];
            var datum = new UpdateConfigsPayload<object>.ConfigData(p.Name, t, Convert.ChangeType(p.GetValue(p), p.PropertyType));
            data.Add(datum);
        }

        UpdateRemoteConfig(data.ToArray());
    }

#region Methods

    private static async void UpdateRemoteConfig<T>(UpdateConfigsPayload<T>.ConfigData[] data)
    {
        var authResponse = await Authenticate();
        var configId     = await GetConfigs(authResponse.access_token);
        
        UpdateConfigs(authResponse.access_token, configId, data);
    }
    
    private static async Task<AuthResponse> Authenticate()
    {
        var request = UnityWebRequest.Put(
                                          "https://api.unity.com/v1/core/api/login",
                                          JsonUtility.ToJson(new AuthDataInput())
                                         );
        request.SetRequestHeader("Content-Type", "application/json");
        request.method = "POST";
        request.SendWebRequest();

        while(!request.isDone)
            await Task.Delay(100);

        if(request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(request.error);
            return null;
        }

        while(!request.downloadHandler.isDone)
            await Task.Delay(100);

        var response = request.downloadHandler.text;
        return JsonUtility.FromJson<AuthResponse>(response);
    }

    private static async Task<string> GetConfigs(string token, string environmentId = "")
    {
        // Soon to be deprecated, gets configs by specific environment ID.
        // var request = UnityWebRequest.Get($"https://remote-config-api.uca.cloud.unity3d.com/environments/" +
        //                                   $"{environmentId}/configs?projectId={PROJECT_ID}");
        var request = UnityWebRequest.Get($"https://remote-config-api.uca.cloud.unity3d.com/configs?" +
                                          $"projectId={PROJECT_ID}");
        request.SetRequestHeader("Authorization", $"Bearer {token}");
        request.SendWebRequest();

        while(!request.isDone)
            await Task.Delay(100);

        if(request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(request.error);
            return null;
        }

        while(!request.downloadHandler.isDone)
            await Task.Delay(100);

        var responseJson = request.downloadHandler.text;
        var response     = JsonConvert.DeserializeObject<ReceiveConfigResponse>(responseJson);
        return response.configs[0].id;
    }

    private static async Task UpdateConfigs<T>(string token, string configId, UpdateConfigsPayload<T>.ConfigData[] data)
    {
        var ccd     = new UpdateConfigsPayload<T>(data);
        var ccdJson = JsonConvert.SerializeObject(ccd);

        var request = UnityWebRequest.Put($"https://remote-config-api.uca.cloud.unity3d.com/configs/" +
                                          $"{configId}?projectId={PROJECT_ID}",
                                          ccdJson);

        request.SetRequestHeader("Content-Type",  "application/json");
        request.SetRequestHeader("Authorization", $"Bearer {token}");
        request.SendWebRequest();

        while(!request.isDone)
            await Task.Delay(100);

        if(request.result != UnityWebRequest.Result.Success)
            Debug.Log(request.error);
    }

#endregion

#region Data Structures

    [Serializable]
    private class AuthDataInput
    {
        public string username   = MyInfo.USERNAME;
        public string password   = MyInfo.PASSWORD;
        public string grant_type = "PASSWORD";
    }

    [Serializable]
    private class AuthResponse
    {
        public string access_token;
        public string refresh_token;
        public string expires_in;
    }

    [Serializable]
    private class UpdateConfigsPayload<T>
    {
        public string       type;
        public ConfigData[] value;

        public UpdateConfigsPayload(ConfigData[] values, string type = "settings")
        {
            this.type = type;
            value     = values;
        }

        [Serializable]
        internal class ConfigData
        {
            public string key;
            public string type;
            public T value;

            public ConfigData(string key, string type, T value)
            {
                this.key   = key;
                this.type  = type;
                this.value = value;
            }
        }
    }

    [Serializable]
    private class ReceiveConfigResponse
    {
        public Configs[] configs;

        [Serializable]
        internal class Configs
        {
            public string id;
        }
    }

#endregion

#endif
    
#region ConfigReferences

    private static readonly Dictionary<Type, Func<PropertyInfo, object>> ConfigMethods = new()
    {
        { typeof(string), (prop) => ConfigManager.appConfig.GetString(prop.Name, (string)prop.GetValue(prop)) },
        { typeof(float), (prop)  => ConfigManager.appConfig.GetFloat(prop.Name, (float)prop.GetValue(prop)) },
        { typeof(int), (prop)    => ConfigManager.appConfig.GetInt(prop.Name, (int)prop.GetValue(prop)) },
        { typeof(bool), (prop)   => ConfigManager.appConfig.GetBool(prop.Name, (bool)prop.GetValue(prop)) },
        { typeof(long), (prop)   => ConfigManager.appConfig.GetLong(prop.Name, (long)prop.GetValue(prop)) }
    };

    private static readonly Dictionary<Type, string> ConfigTypes = new()
    {
        { typeof(string), "string" }, // or json
        { typeof(float), "float" },
        { typeof(int), "int" },
        { typeof(bool), "bool" },
        { typeof(long), "long" }
    };

#endregion
}