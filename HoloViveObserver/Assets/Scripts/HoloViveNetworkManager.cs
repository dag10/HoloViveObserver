using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using UnityEngine.Networking.Match;

public class HoloViveNetworkManager : NetworkManager
{
    public class CustomMsgType
    {
        public static short AddHybridPlayer = MsgType.Highest + 1;
    };

    public class AddHybridPlayerMessage : AddPlayerMessage
    {
        public Utils.PlayerType playerType;
    }

    public GameObject vrPlayerPrefab;
    public GameObject holoLensPlayerPrefab;
    
    public delegate void AttemptingConnectionHandler();
    public delegate void ConnectionEstablishedHandler();
    public delegate void ConnectionLostHandler(bool willRetry);

    public event AttemptingConnectionHandler AttemptingConnection;
    public event ConnectionEstablishedHandler ConnectionEstablished;
    public event ConnectionLostHandler ConnectionLost;

    private bool triggeredAttemptingConnection = false;
    private bool triggeredConnectionLost = false;

    private static string DEFAULT_MATCH_NAME = "default";
    private static string DEFAULT_MATCH_PASSWORD = "";

    public void Start()
    {
        StartMatchMaker();

        if (Utils.IsVR)
        {
            CreateDefaultMatch();
            RegisterServerMessageHandlers();
        }
        else if (Utils.IsHoloLens)
        {
            JoinDefaultMatch();
        }
    }

    #region VR Server

    private void RegisterServerMessageHandlers()
    {
        NetworkServer.RegisterHandler(CustomMsgType.AddHybridPlayer, OnAddHybridPlayerMessage);
    }

    private void OnAddHybridPlayerMessage(NetworkMessage netMsg)
    {
        var msg = netMsg.ReadMessage<AddHybridPlayerMessage>();
        SpawnPlayer(netMsg.conn, msg.playerControllerId, msg.playerType);
    }

    public void SpawnPlayer(NetworkConnection conn, short playerControllerId, Utils.PlayerType type)
    {
        GameObject prefab = null;
        switch (type)
        {
            case Utils.PlayerType.HoloLens:
                prefab = holoLensPlayerPrefab;
                break;
            case Utils.PlayerType.VR:
                prefab = vrPlayerPrefab;
                break;
        }

        if (!prefab)
        {
            Debug.LogError("Unspawnable player type for client: " + type.ToString());
            return;
        }

        Debug.Log("Spawning a " + type.ToString() + " player.");
        GameObject player = Instantiate(prefab, Vector3.zero, Quaternion.identity);
        NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);
    }

    private void CreateDefaultMatch()
    {
        matchMaker.CreateMatch(DEFAULT_MATCH_NAME, 3, true, "", "", "", 0, 0, this.OnMatchCreate);
    }
    
    public override void OnMatchCreate(bool success, string extendedInfo, MatchInfo matchInfo)
    {
        base.OnMatchCreate(success, extendedInfo, matchInfo);
        if (success)
        {
            Debug.Log("Match created: " + extendedInfo);
        }
        else
        {
            Debug.LogError("Failed to create match.");
        }
    }

    public override void OnServerError(NetworkConnection conn, int errorCode)
    {
        base.OnServerError(conn, errorCode);
        Debug.LogError("A server error occurred: " + errorCode);
    }

    #endregion

    #region HoloLens Client

    private void JoinDefaultMatch()
    {
        if (!matchMaker)
        {
            StartMatchMaker();
        }

        matchMaker.ListMatches(0, 5, DEFAULT_MATCH_NAME, false, 0, 0, this.OnMatchList);

        if (!triggeredAttemptingConnection)
        {
            triggeredAttemptingConnection = true;
            if (AttemptingConnection != null) AttemptingConnection();
        }
    }
    public override void OnMatchJoined(bool success, string extendedInfo, MatchInfo matchInfo)
    {
        base.OnMatchJoined(success, extendedInfo, matchInfo);
        if (success)
        {
            Debug.Log("Joined match: " + extendedInfo);
        } else
        {
            Debug.LogError("Failed to join match.");
        }
    }

    public override void OnMatchList(bool success, string extendedInfo, List<MatchInfoSnapshot> matchList)
    {
        base.OnMatchList(success, extendedInfo, matchList);

        if (success)
        {
            if (matchList.Count > 0)
            {
                Debug.Log("Found " + matchList.Count + " matches:");
                foreach (MatchInfoSnapshot match in matchList)
                {
                    Debug.Log("Match: " + match.name + " (" + match.currentSize + ")");
                }

                // Join the first match found
                matchMaker.JoinMatch(matchList[0].networkId, DEFAULT_MATCH_PASSWORD, "", "", 0, 0, OnMatchJoined);
            } else
            {
                Debug.LogWarning("Found no matches. Refreshing match list...");
                JoinDefaultMatch();
            }
        } else
        {
            Debug.LogError("Failed to list matches: " + extendedInfo);
        }
    }

    public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect(conn);
        
        Debug.Log("Connected to server!");
        if (Utils.IsHoloLens && ConnectionEstablished != null) ConnectionEstablished();
        triggeredConnectionLost = false;

        AddHybridPlayerMessage msg = new AddHybridPlayerMessage();
        msg.playerControllerId = 0;
        msg.playerType = Utils.CurrentPlayerType;
        conn.Send(CustomMsgType.AddHybridPlayer, msg);

    }

    public override void OnClientDisconnect(NetworkConnection conn)
    {
        base.OnClientDisconnect(conn);

        Debug.LogWarning("Disconnected from server! Rejoining default match...");
        if (!triggeredConnectionLost)
        {
            triggeredConnectionLost = true;
            if (ConnectionLost != null) ConnectionLost(true);
        }

        JoinDefaultMatch();
    }

    public override void OnClientError(NetworkConnection conn, int errorCode)
    {
        base.OnClientError(conn, errorCode);
        Debug.LogError("Client error: " + errorCode);
    }

    #endregion
}
