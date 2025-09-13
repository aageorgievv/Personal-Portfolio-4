using UnityEngine;
using UnityEngine.UI;

public class ReadyButtonHandler : MonoBehaviour
{
    private ShipSelection shipSelection;
    private Button button;

    private int maxPlacedShips = 5;
    private void Awake()
    {
        button = GetComponent<Button>();
        button.interactable = false;
        button.onClick.AddListener(OnButtonClicked);
        GameManager.ExecuteWhenInitialized(HandleWhenInitialized);
    }

    private void HandleWhenInitialized()
    {
        shipSelection = GameManager.GetManager<ShipSelection>();
        ValidationUtility.ValidateReference(shipSelection, nameof(shipSelection));
        shipSelection.OnShipPlacedEvent += HandleShipPlacedEvent;
    }

    private void OnDestroy()
    {
        shipSelection.OnShipPlacedEvent -= HandleShipPlacedEvent;
        button.onClick.RemoveListener(OnButtonClicked);
    }

    private void HandleShipPlacedEvent(int currentlyPlacedShips)
    {
        button.interactable = currentlyPlacedShips >= maxPlacedShips;
    }

    private void OnButtonClicked()
    {
        PlayerState.localPlayer.SetReady();
    }
}
