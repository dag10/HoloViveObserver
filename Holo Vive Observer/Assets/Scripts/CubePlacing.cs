using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubePlacing : MonoBehaviour {
    public SteamVR_TrackedController leftController;
    public SteamVR_TrackedController rightController;
    public GameObject cubeContainer;
    public GameObject cubeAsset;

    private bool placingCube = false;
    private GameObject currentCube = null;

	void Start()
    {
        leftController.TriggerClicked += TriggerClicked;
        rightController.TriggerClicked += TriggerClicked;
        leftController.TriggerUnclicked += TriggerUnclicked;
        rightController.TriggerUnclicked += TriggerUnclicked;
    }

    void Update()
    {
        if (placingCube)
        {
            UpdateCubePosition();
        }
    }

    private void TriggerUnclicked(object sender, ClickedEventArgs e)
    {
        if (placingCube)
        {
            FinishPlacingCube();
        }
    }

    private void TriggerClicked(object sender, ClickedEventArgs e)
    {
        if (BothTriggersArePressed && !placingCube)
        {
            StartPlacingCube();
        }
    }

    private bool BothTriggersArePressed
    {
        get
        {
            return leftController.triggerPressed && rightController.triggerPressed;
        }
    }

    private void StartPlacingCube()
    {
        currentCube = Instantiate(cubeAsset);
        UpdateCubePosition();

        placingCube = true;
    }

    private void FinishPlacingCube()
    {
        currentCube = null;

        placingCube = false;
    }

    private void UpdateCubePosition()
    {
        Vector3 left = leftController.transform.position;
        Vector3 right = rightController.transform.position;
        currentCube.transform.position = (left + right) / 2;
        currentCube.transform.localScale = right - left;
    }
}
