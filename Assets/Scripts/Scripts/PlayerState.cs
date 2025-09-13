using System;
using Unity.Netcode;
using UnityEngine;

public class PlayerState : NetworkBehaviour
{
    public NetworkVariable<int> PlacedShips => placedShips;
    public NetworkVariable<bool> IsReady => isReady;

    public static PlayerState localPlayer;

    private NetworkVariable<int> placedShips = new NetworkVariable<int>(0);
    private NetworkVariable<bool> isReady = new NetworkVariable<bool>(false);

    private GameManager gameManager;
    private ShipSelection shipSelection;

    private int shipsRequired = 5;

    private void Awake()
    {
        GameManager.ExecuteWhenInitialized(HandleWhenInitialized);
    }

    private void HandleWhenInitialized()
    {
        gameManager = GameManager.GetManager<GameManager>();
        shipSelection = GameManager.GetManager<ShipSelection>();
        ValidationUtility.ValidateReference(gameManager, nameof(gameManager));
        ValidationUtility.ValidateReference(shipSelection, nameof(shipSelection));
        shipSelection.OnShipPlacedEvent += HandleShipPlacedEvent;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        shipSelection.OnShipPlacedEvent += HandleShipPlacedEvent;
    }

    private void Update()
    {
        if (IsOwner && Input.GetKeyDown(KeyCode.K))
        {
            SubmitReadyToServerRpc();
        }
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            localPlayer = this;
        }
    }

    private void HandleShipPlacedEvent(int count)
    {
        if (IsOwner)
        {
            SubmitPlacedShipsToServerRpc(count);
        }
    }

    public void SetReady()
    {
        if (IsOwner)
        {
            SubmitReadyToServerRpc();
        }
    }
    
    [ServerRpc]
    private void SubmitPlacedShipsToServerRpc(int count)
    {
        placedShips.Value = count;
    }

    [ServerRpc]
    private void SubmitReadyToServerRpc()
    {
        bool allShipsPlaced = placedShips.Value >= shipsRequired;
        isReady.Value = allShipsPlaced;
    }
}
