using UnityEngine;
using UnityEngine.Networking;

public class BlockController : NetworkBehaviour
{
    private enum State
    {
        Static,
        Placing,
    }

    private Material staticMaterial;
    public Material placingMaterial;

    [SyncVar]
    private State state = State.Static;

    private MeshRenderer meshRenderer;

    private Vector3 oldPosition;
    private Vector3 oldScale;

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (isClient)
        {
            meshRenderer = GetComponent<MeshRenderer>();
            staticMaterial = meshRenderer.material;
            UpdateMaterial();
        }

        if (isServer)
        {
            SendCurrentPosition();
        }
    }

    private void Update()
    {
        if (isServer)
        {
            SendCurrentPosition();
        }

        if (isClient)
        {
            UpdateMaterial();
        }
    }

    [Server]
    void SendCurrentPosition()
    {
        var position = transform.localPosition;
        var scale = transform.localScale;

        if (position == oldPosition && scale == oldScale) return;

        oldPosition = position;
        oldScale = scale;
        
        RpcUpdatePosition(position, scale);
    }

    [ClientRpc]
    void RpcUpdatePosition(Vector3 position, Vector3 scale)
    {
        this.transform.localPosition = position;
        this.transform.localScale = scale;
    }

    [ServerCallback]
    public void StartPlacing()
    {
        if (state != State.Static)
        {
            return;
        }

        state = State.Placing;
        RpcUpdateMaterial();
    }

    [ServerCallback]
    public void FinishPlacing()
    {
        if (state != State.Placing)
        {
            return;
        }

        state = State.Static;
        RpcUpdateMaterial();
    }

    [ClientRpc]
    private void RpcUpdateMaterial()
    {
        UpdateMaterial();
    }

    [Client]
    private void UpdateMaterial() {
        if (!meshRenderer)
        {
            return;
        }

        switch (state)
        {
            case State.Static:
                meshRenderer.material = staticMaterial;
                break;
            case State.Placing:
                meshRenderer.material = placingMaterial;
                break;
        }
    }
}