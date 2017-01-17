using UnityEngine;
using UnityEngine.Networking;

public class ControllerManagerAlignment : NetworkBehaviour
{
    public AlignmentManager alignmentManager;

    private SteamVR_ControllerManager controllerManager;
    private int numControllers = 0;
    
    void Start()
    {
        controllerManager = GetComponent<SteamVR_ControllerManager>();
        if (!controllerManager)
        {
            Debug.LogError("CameraRig must have SteamVR_ControllerManager behavior.");
            return;
        }
    }
    
    [ClientCallback]
    void Update()
    {
        int newNumControllers = 0;
        foreach (var obj in controllerManager.objects)
        {
            var trackedObj = obj.GetComponent<SteamVR_TrackedObject>();
            if (trackedObj == null) continue;
            if (trackedObj.isValid) newNumControllers++;
        }
        
        if (newNumControllers != numControllers)
        {
            numControllers = newNumControllers;
            alignmentManager.CmdSetNumControllers(numControllers);
        }
    }
}
