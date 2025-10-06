using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour, IManager
{
    private static event Action onInitializedCallback;

    [Header("References")]
    [SerializeField] private GridSpawner spawner;
    [SerializeField] private GridManager gridManager;
    [SerializeField] private ShipSelection shipSelection;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private AttackManager attacksManager;

    private static Dictionary<Type, IManager> managers = new Dictionary<Type, IManager>();

    public NetworkVariable<ulong> CurrentTurnPlayerId = new NetworkVariable<ulong>();

    private static bool isInitialized;

    private bool triedAsClient = false;

    private float startHostDelay = 1;
    private const int playerCount = 2;

    private Coroutine waitForClientsRoutine;

    private void Awake()
    {
        //Validate
        ValidationUtility.ValidateReference(gridManager, nameof(gridManager));
        ValidationUtility.ValidateReference(shipSelection, nameof(shipSelection));
        ValidationUtility.ValidateReference(uiManager, nameof(uiManager));
        ValidationUtility.ValidateReference(attacksManager, nameof(attacksManager));

        managers.Clear();
        //Add references
        managers.Add(typeof(GameManager), this);
        managers.Add(typeof(GridManager), gridManager);
        managers.Add(typeof(ShipSelection), shipSelection);
        managers.Add(typeof(UIManager), uiManager);
        managers.Add(typeof(AttackManager), attacksManager);

        isInitialized = true;
        onInitializedCallback?.Invoke();
        onInitializedCallback = null;
    }

    private void Start()
    {
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        TryJoinAsClient();
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        if (waitForClientsRoutine != null)
        {
            StopCoroutine(waitForClientsRoutine);
        }

        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
    }

    public static T GetManager<T>() where T : IManager
    {
        return (T)managers[typeof(T)];
    }

    public static void ExecuteWhenInitialized(Action callback)
    {
        if (isInitialized)
        {
            callback?.Invoke();
        }
        else
        {
            onInitializedCallback += callback;
        }
    }

    private void TryJoinAsClient()
    {
        triedAsClient = true;
        NetworkManager.Singleton.StartClient();
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (triedAsClient && clientId == NetworkManager.Singleton.LocalClientId)
        {
            StartCoroutine(StartHostWithDelay(startHostDelay));
        }
    }

    private IEnumerator StartHostWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (!NetworkManager.Singleton.IsListening)
        {
            Debug.Log("Becoming host now...");
            NetworkManager.Singleton.StartHost();
            waitForClientsRoutine = StartCoroutine(CheckIfAllPlayersAreReady());
        }
    }

    private IEnumerator CheckIfAllPlayersAreReady()
    {
        if (!IsServer)
        {
            yield break;
        }

        Debug.Log($"Waiting for {playerCount} clients to join");
        while (NetworkManager.Singleton.ConnectedClients.Count < playerCount)
        {
            yield return null;
        }

        // spawn grid
        GridState gridState = spawner.GenerateGridState();

        spawner.RegenerateGrid(gridState);
        SendGridClientRpc(gridState);

        IEnumerable<PlayerState> playerStates = NetworkManager.ConnectedClients.Select(c => c.Value.PlayerObject.GetComponent<PlayerState>());
        foreach (PlayerState playerState in playerStates)
        {
            playerState.SaveOwnGrid();
        }

        Debug.Log($"Waiting for players to ready up");
        bool allReady = false;
        while (!allReady)
        {
            // waiting until all players ready
            allReady = NetworkManager.Singleton.ConnectedClientsList.All(c => c.PlayerObject.GetComponent<PlayerState>().IsReady.Value);
            yield return null;
        }

        Debug.LogError("All players ready → Start Game!");

        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            EnableOpponentCellsClientRpc(client.ClientId);
        }

        CurrentTurnPlayerId.Value = NetworkManager.Singleton.ConnectedClientsList[0].ClientId;
        UpdateTurnClientRpc(CurrentTurnPlayerId.Value);
    }

    [ClientRpc]
    private void SendGridClientRpc(GridState state)
    {
        if (!IsServer)
        {
            spawner.RegenerateGrid(state);
        }
    }

    [ClientRpc]
    private void EnableOpponentCellsClientRpc(ulong playerId)
    {
        GridManager grid = GetManager<GridManager>();

        foreach (var cell in grid.GetAllCells())
        {
            if (cell.OwnerId != playerId)
            {
                cell.EnableAttackMode();
            }
        }
    }

    [ClientRpc]
    public void UpdateTurnClientRpc(ulong playerId)
    {
        AttackManager attackManager = GameManager.GetManager<AttackManager>();
        attackManager.SetActivePlayerTurn(playerId);

        /*        PlayerState playerState = NetworkManager.ConnectedClients.Select(c => c.Value.PlayerObject.GetComponent<PlayerState>()).FirstOrDefault(state => state.OwnerClientId == playerId);
                playerState.UpdateOwnGrid();*/

        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            var state = client.PlayerObject.GetComponent<PlayerState>();

            if (state.IsOwner)
            {
                if (state.OwnerClientId == playerId)
                {
                    state.SetAttackMode();
                }
                else
                {
                    state.SetDefenseMode();
                }
            }
        }
    }
}
