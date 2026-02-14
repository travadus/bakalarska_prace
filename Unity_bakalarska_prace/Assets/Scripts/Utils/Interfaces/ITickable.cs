using System;
using UnityEngine;

public interface ITickable
{
    void OnTick(DateTime currentDateTime);
}
