using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour, IManager
{
    [Header("References")]
    [SerializeField] private Button readyButton;
    [SerializeField] private TMP_Text readyButtonText;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TMP_Text gameOverText;

    private ShipSelection shipSelection;

    private int readyCount = 5;

    private void Awake()
    {
        GameManager.ExecuteWhenInitialized(HandleWhenInitialized);
    }

    private void HandleWhenInitialized()
    {
        shipSelection = GameManager.GetManager<ShipSelection>();
        ValidationUtility.ValidateReference(shipSelection, nameof(shipSelection));
        shipSelection.OnShipPlacedEvent += UpdateReadyButton;
    }

    private void UpdateReadyButton(int placedShips)
    {
        readyButton.interactable = placedShips == readyCount;

        if(placedShips == readyCount)
        {
            readyButtonText.text = $"Ready";
            readyButton.image.color = Color.green;
        } else
        {
            readyButtonText.text = $"{placedShips}/5";
        }
    }

    public void ShowGameOver(bool isWinner)
    {
        gameOverPanel.SetActive(true);
        gameOverText.text = isWinner ? "You Win!" : "You Lose!";
    }
}
