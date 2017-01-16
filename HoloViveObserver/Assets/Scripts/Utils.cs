using UnityEngine.VR;

public class Utils
{
    private static PlayerType playerType = PlayerType.Undetermined;

    public enum PlayerType
    {
        Undetermined,
        Unknown,
        HoloLens,
        VR,
    }

    public static PlayerType CurrentPlayerType
    {
        get
        {
            if (playerType == PlayerType.Undetermined)
            {
                switch (VRSettings.loadedDeviceName)
                {
                    case "HoloLens":
                        playerType = PlayerType.HoloLens;
                        break;
                    case "OpenVR":
                        playerType = PlayerType.VR;
                        break;
                    default:
                        playerType = PlayerType.Unknown;
                        break;
                }
            }

            return playerType;
        }
    }

    public static bool IsHoloLens
    {
        get
        {
            return CurrentPlayerType == PlayerType.HoloLens;
        }
    }

    public static bool IsVR
    {
        get
        {
            return CurrentPlayerType == PlayerType.VR;
        }
    }
}
