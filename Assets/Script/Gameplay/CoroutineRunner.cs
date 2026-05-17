using System.Collections;
using UnityEngine;

/// <summary>
/// Singleton persistant utilisé pour exécuter des coroutines indépendamment
/// du cycle de vie des MonoBehaviours appelants.
/// Évite que des coroutines soient annulées quand leur caller est détruit.
/// </summary>
public class CoroutineRunner : MonoBehaviour
{
    private static CoroutineRunner instance;

    public static CoroutineRunner Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("[CoroutineRunner]");
                DontDestroyOnLoad(go);
                instance = go.AddComponent<CoroutineRunner>();
            }

            return instance;
        }
    }

    /// <summary>
    /// Démarre une coroutine sur le runner persistant.
    /// </summary>
    public void Run(IEnumerator routine)
    {
        StartCoroutine(routine);
    }
}
