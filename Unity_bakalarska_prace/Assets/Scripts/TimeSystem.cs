using UnityEngine;
using System;

public class TimeSystem : MonoBehaviour
{
    public static event Action<int> OnTick;

    public float tickInterval = 1f;

    private float tickTimer;
    private int currentTick;

    private void Update()
    {
        tickTimer += Time.deltaTime;

        if (tickTimer >= tickInterval)
        {
            tickTimer -= tickInterval;
            currentTick++;

            OnTick?.Invoke(currentTick);
        }
    }
}