using UnityEngine;

public class Launch : MonoBehaviour
{
    [SerializeField] private TextAsset csv;
    
    private void Awake()
    {
        RemoteSettings.Init(csv);
        Debug.Log("Launched");
    }
}
