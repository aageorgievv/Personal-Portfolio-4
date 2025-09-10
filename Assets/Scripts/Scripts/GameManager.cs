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
    [SerializeField] private GridManager gridManager;
    [SerializeField] private ShipSelection shipSelection;
    [SerializeField] private UIManager uiManager;

    private static Dictionary<Type, IManager> managers = new Dictionary<Type, IManager>();

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

        managers.Clear();
        //Add references
        managers.Add(typeof(GameManager), this);
        managers.Add(typeof(GridManager), gridManager);
        managers.Add(typeof(ShipSelection), shipSelection);
        managers.Add(typeof(UIManager), uiManager);

        isInitialized = true;
        onInitializedCallback?.Invoke();
        onInitializedCallback = null;
    }

    private void Start()
    {
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        NetworkManager.Singleton.OnConnectionEvent += HandleConnectionEvent;
        TryJoinAsClient();
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        if (waitForClientsRoutine != null)
        {
            StopCoroutine(waitForClientsRoutine);
        }
    }

    private void HandleConnectionEvent(NetworkManager arg1, ConnectionEventData arg2)
    {
        Debug.LogError($"[val] HandleConnectionEvent {arg2.EventType}");
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
        if(!IsServer)
        {
            yield break;
        }

        Debug.Log($"Waiting for {playerCount} clients to join");
        while(NetworkManager.Singleton.ConnectedClients.Count < playerCount)
        {
            yield return null;
        }

        // spawn map

        Debug.Log($"Waiting for players to ready up");
        bool allReady = false;
        while (!allReady)
        {
            // waiting until all players ready
            allReady = NetworkManager.Singleton.ConnectedClientsList.All(c => c.PlayerObject.GetComponent<PlayerState>().IsReady.Value);
            yield return null;
        }

        Debug.Log("All players ready → Start Game!");
    }
}
