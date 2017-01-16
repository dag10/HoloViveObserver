using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;

public class HoloViveNetworkManager : NetworkManager
{
    public GameObject vrPlayerPrefab;
    public GameObject holoLensPlayerPrefab;

    [HideInInspector]
    private new GameObject playerPrefab
    {
        set
        {
            base.playerPrefab = value;
        }
        get
        {
            return base.playerPrefab;
        }
    }

    public void StartVRServer()
    {
        playerPrefab = vrPlayerPrefab;

        Debug.Log("Vive detected. Starting MatchMaker and creating match.");
        StartMatchMaker();
        matchMaker.CreateMatch("default", 3, true, "", "", "", 0, 0, this.OnMatchCreate);

        //GetComponent<NetworkManagerHUD>().enabled = true;
        //StartHost();
    }

    public void StartHoloLensClient(string address, int port)
    {
        playerPrefab = holoLensPlayerPrefab;
        networkAddress = address;
        networkPort = port;

        Debug.Log("HoloLens detected. Starting MatchMaker.");
        StartMatchMaker();
        matchMaker.ListMatches(0, 5, "default", false, 0, 0, this.MyOnMatchList);
        //matchMaker.JoinMatch(UnityEngine.Networking.Types.NetworkID.Invalid, "", )
        //StartClient();
    }

    #region VR Server

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

    public override void OnStartServer()
    {
        base.OnStartServer();
        Debug.Log("Server started.");
    }

    public override void OnServerConnect(NetworkConnection conn)
    {
        base.OnServerConnect(conn);
        Debug.Log("A client connected!");
    }

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        base.OnServerDisconnect(conn);
        Debug.Log("A client disconnected.");
    }

    public override void OnServerError(NetworkConnection conn, int errorCode)
    {
        base.OnServerError(conn, errorCode);
        Debug.LogError("A server error occurred: " + errorCode);
    }

    #endregion

    #region HoloLens Client

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
            Debug.Log("OnMatchList successful");
        } else
        {
            Debug.Log("OnMatchList unsuccessful");
        }
    }

    public virtual void MyOnMatchList(bool success, string extendedInfo, List<MatchInfoSnapshot> matchList)
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
                matchMaker.JoinMatch(matchList[0].networkId, "", "", "", 0, 0, OnMatchJoined);
            } else
            {
                Debug.Log("Found no matches. Refreshing match list...");
                matchMaker.ListMatches(0, 5, "default", false, 0, 1, this.MyOnMatchList);
            }
        } else
        {
            Debug.LogError("Failed to list matches: " + extendedInfo);
        }
    }

    public override void OnStartClient(NetworkClient client)
    {
        base.OnStartClient(client);
        Debug.Log("Started client to connect to " + networkAddress + ":" + networkPort);
    }

    public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect(conn);
        Debug.Log("Connected to server!");
    }

    public override void OnClientDisconnect(NetworkConnection conn)
    {
        base.OnClientDisconnect(conn);
        Debug.Log("Disconnected from server! Retrying...");
        StartClient();
    }

    public override void OnClientError(NetworkConnection conn, int errorCode)
    {
        base.OnClientError(conn, errorCode);
        Debug.LogError("Client error: " + errorCode);
    }

    #endregion

    void Start()
    {
    }

    void Update()
    {
    }
}
