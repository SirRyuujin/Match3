using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MovesController : MonoBehaviour
{
    [Header("References")]
    public GameEventSO OnGemsSwappedEvent;
    [SerializeField] private TextMeshProUGUI _movesCounterText;

    [Header("Preview")]
    [SerializeField] private int _moves = 0;

    public void IncrementMoves()
    {
        _moves += 1;
    }

    public void UpdateMovesCounterText()
    {
        _movesCounterText.text = _moves.ToString();
    }
}