using UnityEngine;
using UnityEngine.Networking;

public class PlayerController : NetworkBehaviour
{
    private GameObject hmd;

    void Start()
    {
        hmd = GameObject.FindGameObjectWithTag("MainCamera");
    }
    
    void Update()
    {
        if (!isLocalPlayer || !hmd)
            return;

        this.transform.position = hmd.transform.position + (hmd.transform.forward * 1.2f);
        this.transform.rotation = hmd.transform.rotation * Quaternion.Euler(45, 45, 45);
    }
}
