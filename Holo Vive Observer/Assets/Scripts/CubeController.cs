using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeController : MonoBehaviour {
    private enum State
    {
        Static,
        Placing,
    }

    private Material staticMaterial;
    public Material placingMaterial;

    private State state = State.Static;
    private MeshRenderer meshRenderer;
    
	void Start() {
        meshRenderer = GetComponent<MeshRenderer>();
        staticMaterial = meshRenderer.material;
        UpdateMaterial();
	}
	
	void Update() {
	}

    public void StartPlacing()
    {
        if (state != State.Static)
        {
            return;
        }

        state = State.Placing;
        UpdateMaterial();
    }

    public void FinishPlacing()
    {
        if (state != State.Placing)
        {
            return;
        }
        
        state = State.Static;
        UpdateMaterial();
    }

    private void UpdateMaterial()
    {
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
