using UnityEngine;

public class ControllerAlignment : MonoBehaviour
{
    public AlignmentManager alignmentManager;

    private SteamVR_TrackedController controller;

    void Start()
    {
        controller = GetComponent<SteamVR_TrackedController>();
        if (!controller)
        {
            Debug.LogError("Controller must have SteamVR_TrackedController behavior.");
            return;
        }

        controller.TriggerClicked += Controller_TriggerClicked;
    }

    private void Controller_TriggerClicked(object sender, ClickedEventArgs e)
    {
        if (alignmentManager.CurrentlyAligning)
        {
            alignmentManager.ControllerClicked(this.transform);
        }
    }
}
