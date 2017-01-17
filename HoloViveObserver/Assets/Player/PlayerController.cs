using UnityEngine;
using UnityEngine.Networking;

public class PlayerController : NetworkBehaviour
{
    public Utils.PlayerType playerType;

    private GameObject hmd;
    private AlignmentClient alignmentClient;

    [ClientCallback]
    void Start()
    {
        if (!isLocalPlayer)
            return;
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        hmd = GameObject.FindGameObjectWithTag("MainCamera");
        alignmentClient = GetComponent<AlignmentClient>();

        EventAnnouncer.GetTaggedInstance().SetCurrentPlayer(this.gameObject);
    }

    [ClientRpc]
    public void RpcPlayerIsInitialized()
    {
        if (alignmentClient != null)
        {
            alignmentClient.RequestAlignment();
        }
    }

    [ClientCallback]
    void Update()
    {
        if (!isLocalPlayer || !hmd)
            return;

        this.transform.position = hmd.transform.position;
        this.transform.rotation = hmd.transform.rotation;
    }

    public bool IsHoloLens
    {
        get
        {
            return playerType == Utils.PlayerType.HoloLens;
        }
    }

    public bool IsVR
    {
        get
        {
            return playerType == Utils.PlayerType.VR;
        }
    }
}
