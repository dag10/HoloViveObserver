using UnityEngine;
using UnityEngine.VR;
using UnityEngine.SceneManagement;

public class MainScene : MonoBehaviour
{
    void Start()
    {
        if (IsHoloLens)
        {
            SceneManager.LoadScene("HoloLens");
        }
        else if (IsVR)
        {
            SceneManager.LoadScene("Vive");
        }
    }

    public bool IsHoloLens
    {
        get
        {
            return VRSettings.loadedDeviceName.Equals("HoloLens");
        }
    }

    public bool IsVR
    {
        get
        {
            return VRSettings.loadedDeviceName.Equals("OpenVR");
        }
    }

    void Update()
    {
    }
}
