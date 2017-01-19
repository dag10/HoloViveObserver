using UnityEngine;
using UnityEngine.Networking;

public class AlignmentClient : NetworkBehaviour
{
    public enum AlignmentRejectionReason
    {
        OngoingAlignment,
        NoControllers,
    }

    public delegate void PlayerAlignmentRejectionHandler(AlignmentRejectionReason reason);
    public delegate void PlayerAlignmentStartedHandler();
    public delegate void PlayerAlignmentFinishedHandler(bool success);

    // These are only ever invoked on the client. Don't listen to them from the server.
    public event PlayerAlignmentRejectionHandler EventPlayerAlignmentRejection;
    public event PlayerAlignmentStartedHandler EventPlayerAlignmentStarted;
    public event PlayerAlignmentFinishedHandler EventPlayerAlignmentFinished;

    [SyncVar, HideInInspector]
    public GameObject alignmentManagerObject; // SyncVars can be GameObjects but not Behaviors.
    public AlignmentManager alignmentManager { get { return alignmentManagerObject.GetComponent<AlignmentManager>(); } }

    [SyncVar]
    private bool requestingAlignment = false;

    public GameObject controllerTargetPrefab;

    private bool attemptedAlignment = false;
    private bool aligned = false;
    private GameObject controllerTarget = null;
    private PlayerController playerController = null;

    void Start()
    {
        playerController = GetComponent<PlayerController>();

        if (isServer)
        {
            alignmentManagerObject = GetComponent<PlayerController>().alignmentManagerObject;
            alignmentManager.EventAlignmentStarted += AlignmentManager_EventAlignmentStarted;
            alignmentManager.EventAlignmentFinished += AlignmentManager_EventAlignmentFinished;
            alignmentManager.EventControllersAvailable += AlignmentManager_EventControllersAvailable;
        }
    }

    [Server]
    private void AlignmentManager_EventAlignmentStarted()
    {
        RpcManagerStarted();
    }

    [ClientRpc]
    private void RpcManagerStarted()
    {
        if (requestingAlignment)
        {
            if (EventPlayerAlignmentStarted != null) EventPlayerAlignmentStarted();
            CreateAlignmentTarget();
        }
    }

    [Server]
    private void AlignmentManager_EventAlignmentFinished(bool success, Vector3 position, float rotation)
    {
        RpcManagerFinished(success, position, rotation);
    }

    [ClientRpc]
    private void RpcManagerFinished(bool success, Vector3 position, float rotation)
    {
        if (requestingAlignment)
        {
            if (success)
            {
                requestingAlignment = false;
                aligned = true;
                ApplyAlignment(position, rotation);
            }

            if (EventPlayerAlignmentFinished != null) EventPlayerAlignmentFinished(success);
        }
    }

    [Server]
    private void AlignmentManager_EventControllersAvailable()
    {
        if (!aligned && attemptedAlignment)
        {
            CmdRequestAlignment();
        }
    }

    [Command]
    private void CmdTargetInfo(Vector3 position, float rotation)
    {
        alignmentManager.TargetInfo(position, rotation);
    }

    [Client]
    private void CreateAlignmentTarget()
    {
        if (!isLocalPlayer) return;

        Debug.Log("Creating floating controller.");
        controllerTarget = Instantiate(controllerTargetPrefab);
        controllerTarget.transform.position = Vector3.zero;
        controllerTarget.transform.rotation = Quaternion.identity;

        CmdTargetInfo(controllerTarget.transform.position, controllerTarget.transform.rotation.eulerAngles.y);
    }

    [Client]
    private void ApplyAlignment(Vector3 position, float rotation)
    {
        if (isLocalPlayer && isClient)
        {
            alignmentManager.ApplyLocalAlignment(position, rotation);
            playerController.ApplyRelativeAlignment(position, rotation);
            Destroy(controllerTarget);
        }
    }

    [Client]
    public void RequestAlignment()
    {
        requestingAlignment = true;
        CmdRequestAlignment();
    }

    [Command]
    public void CmdRequestAlignment()
    {
        if (!alignmentManager.CanAlign)
        {
            attemptedAlignment = true;
            var reason = alignmentManager.CurrentlyAligning ? AlignmentRejectionReason.OngoingAlignment : AlignmentRejectionReason.NoControllers;
            RpcManagerRejected(reason);
            return;
        }

        requestingAlignment = true;
        alignmentManager.RequestAlignment();
    }

    [ClientRpc]
    private void RpcManagerRejected(AlignmentRejectionReason reason)
    {
        if (EventPlayerAlignmentRejection != null) EventPlayerAlignmentRejection(reason);
    }

    public bool IsAligning
    {
        get
        {
            return requestingAlignment;
        }
    }

    public bool IsAligned
    {
        get
        {
            return IsAligned;
        }
    }
}
