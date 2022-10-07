using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleUIController : MonoBehaviour
{
    public GameEventSO OnGemsSwappedEvent;

    public void OnGemsSwapped()
    {
        Debug.Log("Swapping");
    }
}
