using UnityEngine;
using UnityEngine.Networking;

public class PlayerController : NetworkBehaviour
{
    public Utils.PlayerType playerType;

    [SyncVar]
    public Vector3 alignmentTranslation;

    [SyncVar]
    public float alignmentRotation;

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
        if (alignmentClient != null && alignmentClient.isActiveAndEnabled)
        {
            alignmentClient.RequestAlignment();
        }
    }

    [Command]
    private void CmdApplyRelativeAlignment(Vector3 translation, float rotation)
    {
        alignmentTranslation = translation;
        alignmentRotation = rotation;
    }

    [Client]
    public void ApplyRelativeAlignment(Vector3 translation, float rotation)
    {
        CmdApplyRelativeAlignment(translation, rotation);
    }

    [ClientCallback]
    void Update()
    {
        if (!isLocalPlayer || !hmd)
            return;

        this.transform.position = (Quaternion.Euler(0, -alignmentRotation, 0) * hmd.transform.position) - alignmentTranslation;
        this.transform.rotation = Quaternion.Euler(0, -alignmentRotation, 0) * hmd.transform.rotation;
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
