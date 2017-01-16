using UnityEngine;

public class DeviceDependent : MonoBehaviour {
    public Utils.PlayerType requiredPlayerType;

	void Awake() {
        gameObject.SetActive(Utils.CurrentPlayerType == requiredPlayerType);
	}
}
