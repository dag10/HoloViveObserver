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

    [SyncVar]
    public GameObject alignmentManagerObject; // SyncVars can be GameObjects but not Behaviors.
    public AlignmentManager alignmentManager { get { return alignmentManagerObject.GetComponent<AlignmentManager>(); } }

    [SyncVar]
    private bool requestingAlignment = false;

    private bool attemptedAlignment = false;
    private bool aligned = false;

    void Start()
    {
        if (isServer)
        {
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
        }
    }

    [Server]
    private void AlignmentManager_EventAlignmentFinished(bool success)
    {
        RpcManagerFinished(success);
    }

    [ClientRpc]
    private void RpcManagerFinished(bool success)
    {
        if (requestingAlignment)
        {
            if (success)
            {
                requestingAlignment = false;
                aligned = true;
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
}
