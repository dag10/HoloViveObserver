using UnityEngine;
using HoloToolkit.Unity;

public class EventAnnouncer : MonoBehaviour
{
    public HoloViveNetworkManager networkManager;

    private TextToSpeechManager tts;
    
    void Start()
    {
        tts = GetComponent<TextToSpeechManager>();

        if (networkManager)
        {
            networkManager.AttemptingConnection += NetworkManager_AttemptingConnection;
            networkManager.ConnectionEstablished += NetworkManager_ConnectionEstablished;
            networkManager.ConnectionLost += NetworkManager_ConnectionLost;
        }
    }

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

    private void Say(string message)
    {
        tts.SpeakText(message);
    }
}
