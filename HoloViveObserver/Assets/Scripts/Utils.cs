using UnityEngine.VR;

public class Utils
{
    public static bool IsHoloLens
    {
        get
        {
            return VRSettings.loadedDeviceName.Equals("HoloLens");
        }
    }

    public static bool IsVR
    {
        get
        {
            return VRSettings.loadedDeviceName.Equals("OpenVR");
        }
    }
}
