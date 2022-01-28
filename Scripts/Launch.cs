using UnityEngine;

public class Launch : MonoBehaviour
{
    private void Awake()
    {
        RcHandler.Init();
        
        Debug.Log("Launched");
    }
}
