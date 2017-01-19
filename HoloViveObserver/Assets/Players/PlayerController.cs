using UnityEngine;
using UnityEngine.Networking;

public class PlayerController : NetworkBehaviour
{
    public Utils.PlayerType playerType;

    public GameObject visuals;

    [SyncVar, HideInInspector]
    public GameObject alignmentManagerObject; // SyncVars can be GameObjects but not Behaviors.
    public AlignmentManager alignmentManager { get { return alignmentManagerObject.GetComponent<AlignmentManager>(); } }

    [SyncVar]
    public Vector3 alignmentTranslation;

    [SyncVar]
    public float alignmentRotation;

    private Vector3 privateAlignmentTranslation = Vector3.zero;
    private float privateAlignmentRotation = 0;
    private float lastNetworkSync = 0;

    private GameObject hmd;
    private AlignmentClient alignmentClient;

    [ClientCallback]
    void Start()
    {
        // The models displayed for a VR player in a HoloLens observer must be manually offset
        // since the positions of the Player object itself are still going to be absolute relative
        // to the HoloLens's true origin.
        if (IsVR)
        {
            visuals.SetActive(false); // Hide VR player avatar
            alignmentManager.EventAlignmentFinished += AlignmentManager_EventAlignmentFinished;
        }

        if (isLocalPlayer && IsHoloLens)
        {
            // If HoloLens, hide its own player visuals, since it won't be seen nor aligned anyway.
            visuals.SetActive(false);
        }
    }

    private void AlignmentManager_EventAlignmentFinished(bool success, Vector3 position, float rotation)
    {
        privateAlignmentTranslation = position;
        privateAlignmentRotation = rotation;
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
    
    void Update()
    {
        if (isLocalPlayer && hmd)
        {
            this.transform.localPosition = (Quaternion.Euler(0, -alignmentRotation, 0) * hmd.transform.position) - alignmentTranslation;
            this.transform.localRotation = Quaternion.Euler(0, -alignmentRotation, 0) * hmd.transform.rotation;
        }

        if (isServer && IsVR)
        {
            // Disabled because this requires so much bandwidth the matchmaker kicks us off
            // after a couple minutes.
            //RpcManualSync(transform.position, transform.rotation);
        }
    }

    [ClientRpc]
    private void RpcManualSync(Vector3 position, Quaternion rotation)
    {
        if (isLocalPlayer) return;

        this.transform.position = (Quaternion.Euler(0, privateAlignmentRotation, 0) * position) + privateAlignmentTranslation;
        this.transform.rotation = Quaternion.Euler(0, privateAlignmentRotation, 0) * rotation;
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
