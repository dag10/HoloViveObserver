using UnityEngine;
using UnityEngine.Networking;

public class AlignmentManager : NetworkBehaviour
{
    public enum State
    {
        Normal,
        Aligning,
    }

    public delegate void AlignmentStartedHandler();
    public delegate void AlignmentFinishedHandler(bool success);
    public delegate void ControllersAvailableHandler();
    public delegate void ControllersUnavailableHandler();

    [SyncEvent]
    public event AlignmentStartedHandler EventAlignmentStarted;

    [SyncEvent]
    public event AlignmentFinishedHandler EventAlignmentFinished;

    [SyncEvent]
    public event ControllersAvailableHandler EventControllersAvailable;

    [SyncEvent]
    public event ControllersUnavailableHandler EventControllersUnavailable;

    [SyncVar]
    private State state = State.Normal;

    [SyncVar]
    private int numControllersPresent = 0;

    public bool CanAlign
    {
        get
        {
            return HasControllers && !CurrentlyAligning;
        }
    }

    public bool HasControllers
    {
        get
        {
            return numControllersPresent > 0;
        }
    }

    public bool CurrentlyAligning
    {
        get
        {
            return state == State.Aligning;
        }
    }

    [Server]
    public void RequestAlignment()
    {
        if (CanAlign)
        {
            StartAlignment();
        }
    }

    [Server]
    private void StartAlignment()
    {
        state = State.Aligning;

        Debug.Log("Alignment started.");
        if (EventAlignmentStarted != null) EventAlignmentStarted();
    }

    [Server]
    private void CancelAlignment()
    {
        state = State.Normal;

        Debug.Log("Alignment canceled.");
        if (EventAlignmentFinished != null) EventAlignmentFinished(false);
    }

    [Command]
    public void CmdSetNumControllers(int numControllers)
    {
        bool hadControllers = HasControllers;
        numControllersPresent = numControllers;

        if (HasControllers && !hadControllers)
        {
            if (EventControllersAvailable != null) EventControllersAvailable();
        }
        else if (!HasControllers && hadControllers)
        {
            if (EventControllersUnavailable != null) EventControllersUnavailable();
            if (CurrentlyAligning) CancelAlignment();
        }
    }
}
