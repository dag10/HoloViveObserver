using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class TestPlayerController : NetworkBehaviour
{
    public GameObject droppingPrefab;
    private GameObject droppingsContainer;
    private Quaternion originalRotation;

    public override void OnStartLocalPlayer()
    {
        GetComponentInChildren<MeshRenderer>().material.color = Color.red;
    }

    void Start() {
        droppingsContainer = GameObject.FindGameObjectWithTag("DroppingsContainer");
        originalRotation = transform.rotation;
	}

    [Command]
    void CmdSpawnDropping()
    {
        var dropping = Instantiate(droppingPrefab, droppingsContainer.transform);
        dropping.transform.position = this.transform.position - new Vector3(1.5f, 0, 0);
        NetworkServer.Spawn(dropping);
    }
	
	void Update()
    {
        if (!isLocalPlayer)
            return;

        var x = Input.GetAxis("Horizontal") * 0.1f;
        var z = Input.GetAxis("Vertical") * 0.1f;

        transform.Translate(x, 0, z);
        transform.rotation = originalRotation;

        if (Input.GetKeyDown(KeyCode.Space))
        {
           CmdSpawnDropping();
        }
    }
}
