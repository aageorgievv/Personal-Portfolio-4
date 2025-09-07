using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour, IManager
{
    [Header("References")]
    [SerializeField] private GridManager gridManager;

    private static Dictionary<Type, IManager> managers = new Dictionary<Type, IManager>();

    private static event Action onInitializedCallback;
    private static bool isInitialized;
    private bool triedAsClient = false;

    private void Awake()
    {
        //Validate
        ValidationUtility.ValidateReference(gridManager, nameof(gridManager));

        managers.Clear();
        //Add references
        managers.Add(typeof(GameManager), this);
        managers.Add(typeof(GridManager), gridManager);

        isInitialized = true;
        onInitializedCallback?.Invoke();
        onInitializedCallback = null;
    }

    private void Start()
    {
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;

        // Try to join as client first
        TryJoinAsClient();
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
            StartCoroutine(StartHostWithDelay(3f));
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
}
