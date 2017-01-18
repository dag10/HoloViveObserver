using UnityEngine;
using HoloToolkit.Unity;

public class EventAnnouncer : MonoBehaviour
{
    public static EventAnnouncer GetTaggedInstance()
    {
        return GameObject.FindGameObjectWithTag("EventAnnouncer").GetComponent<EventAnnouncer>();
    }

    public HoloViveNetworkManager networkManager;
    public AlignmentManager alignmentManager;

    private GameObject player = null;
    private TextToSpeechManager tts;
    private AlignmentClient alignmentClient = null;
    
    public void SetCurrentPlayer(GameObject player)
    {
        this.player = player;

        alignmentClient = player.GetComponent<AlignmentClient>();
        if (alignmentClient != null)
        {
            alignmentClient.EventPlayerAlignmentStarted += AlignmentClient_EventPlayerAlignmentStarted;
            alignmentClient.EventPlayerAlignmentFinished += AlignmentClient_EventPlayerAlignmentFinished;
            alignmentClient.EventPlayerAlignmentRejection += AlignmentClient_EventPlayerAlignmentRejection;
        }
    }

    void Start()
    {
        tts = GetComponent<TextToSpeechManager>();

        if (alignmentManager)
        {
            alignmentManager.EventAlignmentStarted += AlignmentManager_EventAlignmentStarted;
            alignmentManager.EventAlignmentFinished += AlignmentManager_EventAlignmentFinished;
            alignmentManager.EventControllersAvailable += AlignmentManager_EventControllersAvailable;
            alignmentManager.EventControllersUnavailable += AlignmentManager_EventControllersUnavailable;
        }

        if (networkManager)
        {
            networkManager.AttemptingConnection += NetworkManager_AttemptingConnection;
            networkManager.ConnectionEstablished += NetworkManager_ConnectionEstablished;
            networkManager.ConnectionLost += NetworkManager_ConnectionLost;
        }
    }

    #region Alignment

    private bool LocalPlayerIsAligning
    {
        get
        {
            return alignmentClient != null && alignmentClient.IsAligning;
        }
    }

    private void AlignmentManager_EventControllersAvailable()
    {
        Say("A controller is now active.");
    }

    private void AlignmentManager_EventControllersUnavailable()
    {
        Say("All controllers lost.");
    }

    private void AlignmentManager_EventAlignmentStarted()
    {
        if (LocalPlayerIsAligning) return;
        Say("A player has started alignment.");
    }

    private void AlignmentManager_EventAlignmentFinished(bool success, Vector3 position, float rotation)
    {
        if (LocalPlayerIsAligning) return;

        if (success)
        {
            Say("Alignment complete.");
        }
        else
        {
            Say("Alignment aborted.");
        }
    }

    private void AlignmentClient_EventPlayerAlignmentStarted()
    {
        Say("Move a controller into the controller outline and pull the trigger.");
    }

    private void AlignmentClient_EventPlayerAlignmentFinished(bool success)
    {
        if (success)
        {
            Say("You are now aligned.");
        }
        else
        {
            Say("Alignment aborted.");
        }
    }

    private void AlignmentClient_EventPlayerAlignmentRejection(AlignmentClient.AlignmentRejectionReason reason)
    {
        switch (reason)
        {
            case AlignmentClient.AlignmentRejectionReason.NoControllers:
                Say("Can't align, no controllers are present.");
                break;
            case AlignmentClient.AlignmentRejectionReason.OngoingAlignment:
                Say("Someone else is currently aligning.");
                break;
        }
    }

    #endregion

    #region Networking

    private void NetworkManager_AttemptingConnection()
    {
        Say("Searching for virtual environment.");
    }

    private void NetworkManager_ConnectionEstablished()
    {
        Say("Connected.");
    }

    private void NetworkManager_ConnectionLost(bool willRetry)
    {
        if (willRetry)
        {
            Say("Connection lost. Searching for virtual environment.");
        }
        else
        {
            Say("Connection lost.");
        }
    }

    #endregion

    private void Say(string message)
    {
        Debug.Log("Announcement: \"" + message + "\"");

        if (Utils.IsHoloLens)
        {
            tts.SpeakText(message);
        }
    }
}
