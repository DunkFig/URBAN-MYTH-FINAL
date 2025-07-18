using UnityEngine;
using System;
using System.Collections.Generic;

public class CollectibleManager : MonoBehaviour
{
    public static CollectibleManager Instance { get; private set; }

    // Fired when any type’s count changes: (type, newCount)
    public static event Action<CollectibleType, int> OnTypeCountChanged;

    // Internal counts
    private Dictionary<CollectibleType, int> counts = new Dictionary<CollectibleType, int>()
    {
        { CollectibleType.Coin, 0 },
        // Add other types if needed
    };

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Prevent duplicates
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Persist across scenes
    }

    public void Add(CollectibleType type, int amount)
    {
        if (!counts.ContainsKey(type))
            counts[type] = 0;

        counts[type] += amount;
        OnTypeCountChanged?.Invoke(type, counts[type]);
    }

    public int GetCount(CollectibleType type)
    {
        return counts.TryGetValue(type, out int count) ? count : 0;
    }
}
