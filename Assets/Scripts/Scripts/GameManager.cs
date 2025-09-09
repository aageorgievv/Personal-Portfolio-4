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

    private static Dictionary<Type, IManager> managers = new Dictionary<Type, IManager>();

    private static bool isInitialized;

    private bool triedAsClient = false;

    private float startHostDelay = 1;

    private void Awake()
    {
        //Validate
        ValidationUtility.ValidateReference(gridManager, nameof(gridManager));
        ValidationUtility.ValidateReference(shipSelection, nameof(shipSelection));
        managers.Clear();
        //Add references
        managers.Add(typeof(GameManager), this);
        managers.Add(typeof(GridManager), gridManager);
        managers.Add(typeof(ShipSelection), shipSelection);


        isInitialized = true;
        onInitializedCallback?.Invoke();
        onInitializedCallback = null;
    }

    private void Start()
    {
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        TryJoinAsClient();
    }

    private void Update()
    {
        CheckIfAllPlayersAreReady();
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
        }
    }

    private void CheckIfAllPlayersAreReady()
    {
        if(!IsServer)
        {
            return;
        }

        var allReady = NetworkManager.Singleton.ConnectedClientsList.All(c => c.PlayerObject.GetComponent<PlayerState>().IsReady.Value);

        if(allReady)
        {
            Debug.Log("All players ready → Start Game!");
        }
    }
}
