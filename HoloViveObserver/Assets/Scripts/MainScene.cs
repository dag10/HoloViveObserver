using UnityEngine;
using UnityEngine.VR;

public class MainScene : MonoBehaviour
{
    void Start()
    {
        if (Utils.CurrentPlayerType == Utils.PlayerType.Unknown)
        {
            Debug.LogError("Unknown headset type: " + VRSettings.loadedDeviceName);
        }
    }
}
