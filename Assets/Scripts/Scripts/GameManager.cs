using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour, IManager
{
    [Header("References")]

    private static Dictionary<Type, IManager> managers = new Dictionary<Type, IManager>();

    private static event Action onInitializedCallback;
    private static bool isInitialized;

    private void Awake()
    {
        //Validate

        managers.Clear();
        //Add references
        managers.Add(typeof(GameManager), this);

        isInitialized = true;
        onInitializedCallback?.Invoke();
        onInitializedCallback = null;
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
}
