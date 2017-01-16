using UnityEngine;
using UnityEngine.VR;

public class MainScene : MonoBehaviour
{
    public GameObject holoLensCamera;
    public GameObject vrCameraRig;
    public HoloViveNetworkManager networkManager;

    public string serverAddress;
    public int serverPort;

    void Start()
    {
        if (Utils.IsHoloLens)
        {
            vrCameraRig.SetActive(false);
            holoLensCamera.SetActive(true);
            networkManager.StartHoloLensClient(serverAddress, serverPort);
        }
        else if (Utils.IsVR)
        {
            holoLensCamera.SetActive(false);
            vrCameraRig.SetActive(true);
            networkManager.StartVRServer();
        }
        else
        {
            Debug.LogError("Can't detect current device formfactor: " + VRSettings.loadedDeviceName);
        }
    }

    void Update()
    {
    }
}
