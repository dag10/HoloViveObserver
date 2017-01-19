using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlignmentBehavior : MonoBehaviour
{
    public AlignmentManager alignmentManager;
    public bool hideDuringAlignment = false;

    private bool wasActive = false;

    void Start()
    {
        alignmentManager.EventAlignmentStarted += AlignmentManager_EventAlignmentStarted;
        alignmentManager.EventAlignmentFinished += AlignmentManager_EventAlignmentFinished;
    }

    private void AlignmentManager_EventAlignmentStarted()
    {
        if (hideDuringAlignment)
        {
            wasActive = this.gameObject.activeSelf;
            gameObject.SetActive(false);
        }
    }

    private void AlignmentManager_EventAlignmentFinished(bool success, Vector3 position, float rotation)
    {
        if (hideDuringAlignment && wasActive)
        {
            gameObject.SetActive(true);
        }
    }
}
