using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// Refactor recursion (in compliance with DRY)
// Add drop anim

public class BoardController : MonoBehaviour
{
    [Header("Events")]
    public GameEventSO OnTrySelectGemEvent;
    public GameEventSO OnSelectGemEvent;
    public GameEventSO OnDeselectGemEvent;
    public GameEventSO OnGemSwapEvent;
    public GameEventSO OnEndSwapEvent;
    public GameEventSO OnFailedSwapEvent;
    public GameEventSO OnMatchGemsEvent;
    public GameEventSO OnAddPoints;
    public GameEventSO OnDetonateBombEvent;
    public GameEventSO OnColorMatchEvent;
    [Header("References")]
    public IntVariable AllowedMoveDistance;
    public IntVariable ChanceToSpawnSpecialObject;
    public IntVariable WrongMovePoints;
    [SerializeField] private IntVariable _numberOfGemsToMatch;
    [SerializeField] private FloatVariable _clickCooldown;
    [SerializeField] private IntVariable _multiplierThreshold; // How many more gems have to be matched in order to get bonus points

    [Space(15)]
    [Header("Preview")]
    public TileController SpecialGemMatchTile;
    public List<TileController> ComboMatches = new List<TileController>(); // All matched tiles in the last move.
    public List<TileController> CurrentMatch = new List<TileController>();
    public TileController[,] Tiles;
    public List<TileController> SelectedTiles = new List<TileController>();
    [HideInInspector] public int Score { get; private set; }
    [HideInInspector] public int Rows { get; private set; }
    [HideInInspector] public int Columns { get; private set; }
    private bool _canSelectGem = true;
    private float _clickCooldownTimer = 0;


    private void Awake()
    {
        Score = 0;
    }

    /// <summary>
    ///  Checks last drawn's tile neighbours. Preventing from a match.
    /// </summary>
    /// <param name="tile"> Last spawned tile to be checked.</param>
    /// <returns> Returns FALSE in case of a potencial match.
    ///           Return TRUE when there's no match. </returns>
    public bool IsDrawnGemValid(TileController tile)
    {
        int count = 1;
        return CheckNeighbours(tile, ref count);
    }

    public void SetBoardDimensions(int rows, int columns)
    {
        Rows = rows;
        Columns = columns;
    }

    private bool IsInDistance(Vector2Int origin, Vector2Int target, int distance)
    {
        float dist = Vector2Int.Distance(origin, target);
        return (dist <= distance && dist % 1 == 0);
    }

    private void SpawnNewGemsSequence()
    {
        int totalBlanks = 0;

        DropTiles(ref totalBlanks);

        Vector2Int[] newTileCords = new Vector2Int[ComboMatches.Count];
        for (int i = 0; i < ComboMatches.Count; i++)
            newTileCords[i] = ComboMatches[i].GetBoardPosition();
        ComboMatches.Clear();

        SpawnNewGems(totalBlanks, newTileCords);

        UpdateNeighboursForEveryTile();

        StartCoroutine(TryMatchNewTiles(newTileCords));
    }

    private bool flag = false;
    private bool flag2 = false;

    #region DROP
    private void SpawnNewGems(int amount, Vector2Int[] newTileCords)
    {
        if (amount == 0)
            return;

        List<TileController> blankTiles = GetBlankTiles();
        TileController tile = blankTiles[0];

        for (int i = 0; i < blankTiles.Count; i++)
        {
            Gem drawnGem;
            do
            {
                drawnGem = blankTiles[i].GetGemOfRandomType(ChanceToSpawnSpecialObject.Value);
                blankTiles[i].AssignGem(drawnGem);
            } while (!IsDrawnGemValid(blankTiles[i]));
        }
    }

    private void DropTiles(ref int totalBlanks)
    {
        List<int> columnsToDrop = new List<int>();

        for (int i = 0; i < ComboMatches.Count; i++)
        {
            Vector2Int pos = ComboMatches[i].GetBoardPosition();
            if (!columnsToDrop.Contains(pos.x))
                columnsToDrop.Add(pos.x);
        }

        DropColumn(columnsToDrop, ref totalBlanks);
    }

    private List<TileController> GetBlankTiles()
    {
        List<TileController> blankTiles = new List<TileController>();

        for (int i = 0; i < Tiles.GetLength(0); i++)
        {
            for (int j = 0; j < Tiles.GetLength(1); j++)
            {
                if (Tiles[i, j].Gem == null)
                    blankTiles.Add(Tiles[i, j]);
            }
        }

        return blankTiles;
    }

    private void DropColumn(List<int> list, ref int totalBlanks)
    {
        int blanks;
        int lastBlankID;

        for (int i = 0; i < list.Count; i++)
        {
            blanks = 0;
            lastBlankID = -1;

            for (int j = 0; j < Tiles.GetLength(0); j++) // rows
            {
                if (Tiles[j, list[i]].Gem == null)
                {
                    blanks++;
                    lastBlankID = j;
                }
            }
                
            if (blanks > 0)
            {
                totalBlanks += blanks;

                for (int k = lastBlankID; k > 0; k--)
                {
                    if (k - blanks >= 0)
                    {
                        Tiles[k, list[i]].AssignGem(Tiles[k - blanks, list[i]].Gem);
                    }
                }
                Tiles[0, list[i]].AssignGem(null);
            }
        }   
    }
    #endregion

    #region CLICK
    public void TrySelectTile()
    {
        if (!_canSelectGem)
            return;
        if (OnTrySelectGemEvent.RecentCaller == null)
            return;

        TileController tile = OnTrySelectGemEvent.RecentCaller.GetComponent<TileController>();

        if (SelectedTiles.Count >= 2)
            TryDeselectBothSelectedTiles();

        if (SelectedTiles.Count == 0)
        {
            SelectTile(tile);
        }
        else if (SelectedTiles.Count == 1)
        {
            if (tile.Gem.Type == SelectedTiles[0].Gem.Type
                && tile.Gem.ID == SelectedTiles[0].Gem.ID)
            {
                DeselectTile(SelectedTiles[0]);
                return;
            }

            if (!SelectedTiles.Contains(tile) &&
                IsInDistance(SelectedTiles[0].GetBoardPosition(), tile.GetBoardPosition(), AllowedMoveDistance.Value))
            {
                SelectTile(tile);
                TrySwapGemsOnSelectedTiles();
                return;
            }

            TryDeselectBothSelectedTiles();
        }
    }

    private void SelectTile(TileController tile)
    {
        SelectedTiles.Add(tile);
        StartCoroutine(ClickCooldownCoroutine());
        OnSelectGemEvent.Raise(tile.gameObject);
    }

    private IEnumerator ClickCooldownCoroutine()
    {
        _canSelectGem = false;
        float cd = _clickCooldown.Value;
        while (_clickCooldownTimer < cd)
        {
            _clickCooldownTimer += Time.deltaTime;
            yield return null;
        }
        
        _clickCooldownTimer = 0;
        _canSelectGem = true;
        //if (SelectedTiles.Count == 2)
        //    TryDeselectBothSelectedTiles();
    }

    private void TryDeselectBothSelectedTiles()
    {
        DeselectTile(SelectedTiles[0]);
        if (SelectedTiles.Count > 0)
            DeselectTile(SelectedTiles[0]);
    }

    private void DeselectTile(TileController tile)
    {
        OnDeselectGemEvent.Raise(tile.gameObject);
        SelectedTiles.Remove(tile);
    }
    #endregion

    #region SWAP/MATCH
    private IEnumerator TryMatchNewTiles(Vector2Int[] cords)
    {
        for (int i = 0; i < Tiles.GetLength(0); i++)
        {
            for (int j = 0; j < Tiles.GetLength(1); j++)
            {
                IsMatch(Tiles[i, j]);
            }
        }

        while (!flag2)
            yield return null;

        if (ComboMatches.Count > 0)
        {
            StartCoroutine(MatchCoroutine());
            OnMatchGemsEvent.Raise(gameObject);
            SpawnNewGemsSequence();
        }
    }

    private void TrySwapGemsOnSelectedTiles()
    {
        SwapGems(SelectedTiles[0], SelectedTiles[1]);
        StartCoroutine(TryMatch());
    }

    private IEnumerator TryMatch()
    {
        flag = false;
        flag2 = true;

        flag = IsMatch(SelectedTiles[0]);
        if (flag)
        {
            IsMatch(SelectedTiles[1]);
            StartCoroutine(MatchCoroutine());
            OnGemSwapEvent.Raise(gameObject);
        }
        else
        {
            flag = IsMatch(SelectedTiles[1]);
            if (flag)
            {
                StartCoroutine(MatchCoroutine());
                OnGemSwapEvent.Raise(gameObject);
            }
        }

        while (!flag2)
            yield return null;

        if (!flag)
        {
            Reswap();
        }
        else
        {
            UpdateNeighboursForEveryTile();
            OnEndSwapEvent.Raise(gameObject);
            OnMatchGemsEvent.Raise(gameObject);
            SpawnNewGemsSequence();
        }

        TryDeselectBothSelectedTiles();
    }

    private void Reswap()
    {
        SwapGems(SelectedTiles[0], SelectedTiles[1]);
        AddToScore(WrongMovePoints.Value);
        OnFailedSwapEvent.Raise(gameObject);
        SpecialGemMatchTile = null;
    }

    private IEnumerator MatchCoroutine()
    {
        flag2 = false;
        StartCoroutine(AnimationSwapSyncCoroutine());
        while (!flag2)
            yield return null;
    }

    private IEnumerator AnimationSwapSyncCoroutine()
    {
        AccountForSpecialGems();
        yield return new WaitForSeconds(_clickCooldown.Value);
        AddToScore(CalculatePoints());
        DeleteGems();
        UpdateNeighboursForEveryTile();
        flag2 = true;
    }

    #region PowerUp
    private void AccountForSpecialGems()
    {
        if (SpecialGemMatchTile != null)
        {
            if (SpecialGemMatchTile.Gem != null)
            {
                switch (SpecialGemMatchTile.Gem.ID)
                {
                    case 0: // bomb 3x3
                        Vector2Int pos = SpecialGemMatchTile.GetBoardPosition();
                        AddAllAdjacentTillesToComboMatch(pos);
                        OnDetonateBombEvent.Raise(gameObject);
                        AddToScore(SpecialGemMatchTile.Gem.BaseValue);
                        break;
                    case 1: // same colour
                        AddGemsOfTheSameIDToComboMatch();
                        OnColorMatchEvent.Raise(gameObject);
                        AddToScore(SpecialGemMatchTile.Gem.BaseValue);
                        break;
                }

                SpecialGemMatchTile.AssignGem(null);
                SpecialGemMatchTile = null;
            }
        }
    }
    #endregion

    private void AddGemsOfTheSameIDToComboMatch()
    {
        TileController tile = ComboMatches[0];
        for (int i = 0; i < Tiles.GetLength(0); i++)
        {
            for (int j = 0; j < Tiles.GetLength(1); j++)
            {
                TileController current = Tiles[i, j];
                if (tile.Gem == null)
                    continue;

                if (current.Gem.Type == tile.Gem.Type)
                    if (current.Gem.ID == tile.Gem.ID)
                        if (!ComboMatches.Contains(current))
                            ComboMatches.Add(current);
            }
        }
    }

    private void AddToScore(int amount)
    {
        Score += amount;
        OnAddPoints.Raise(gameObject);
    }

    private void AddAllAdjacentTillesToComboMatch(Vector2Int pos)
    {
        TileController neighbour;

        neighbour = GetTopNeighbour(pos.x, pos.y);
        if (neighbour != null && !ComboMatches.Contains(neighbour))
            ComboMatches.Add(neighbour);

        neighbour = GetBottomNeighbour(pos.x, pos.y);
        if (neighbour != null && !ComboMatches.Contains(neighbour))
            ComboMatches.Add(neighbour);

        neighbour = GetRightNeighbour(pos.x, pos.y);
        if (neighbour != null && !ComboMatches.Contains(neighbour))
            ComboMatches.Add(neighbour);

        neighbour = GetLeftNeighbour(pos.x, pos.y);
        if (neighbour != null && !ComboMatches.Contains(neighbour))
            ComboMatches.Add(neighbour);

        neighbour = GetTopLeftNeighbour(pos.x, pos.y);
        if (neighbour != null && !ComboMatches.Contains(neighbour))
            ComboMatches.Add(neighbour);

        neighbour = GetTopRightNeighbour(pos.x, pos.y);
        if (neighbour != null && !ComboMatches.Contains(neighbour))
            ComboMatches.Add(neighbour);

        neighbour = GetBottomLeftNeighbour(pos.x, pos.y);
        if (neighbour != null && !ComboMatches.Contains(neighbour))
            ComboMatches.Add(neighbour);

        neighbour = GetBottomRightNeighbour(pos.x, pos.y);
        if (neighbour != null && !ComboMatches.Contains(neighbour))
            ComboMatches.Add(neighbour);
    }

    private void SwapGems(TileController tile1, TileController tile2)
    {
        Gem temp = tile1.Gem;
        tile1.AssignGem(tile2.Gem);
        tile2.AssignGem(temp);
    }

    private void DeleteGems()
    {
        for (int i = 0; i < ComboMatches.Count; i++)
        {
            var pos = ComboMatches[i].GetBoardPosition();
            Tiles[pos.y, pos.x].AssignGem(null);
        }
    }

    private int CalculatePoints()
    {
        List<Gem> gems = new List<Gem>();
        List<int> count = new List<int>();

        int counter = 0;
        for (int i = 0; i < ComboMatches.Count; i++)
        {
            if (!gems.Contains(ComboMatches[i].Gem))
            {
                if (i != 0)
                    count[count.Count - 1] = counter;

                gems.Add(ComboMatches[i].Gem);

                if (count.Count < gems.Count)
                    count.Add(counter);
                else
                    count[count.Count - 1] = counter;

                counter = 0;
                counter++;
            }
            else
                counter++;

            if (i == ComboMatches.Count - 1)
                count[count.Count - 1] = counter;
        }

        int[] pointsPerGemType = new int[gems.Count];
        int points = 0;
        for (int i = 0; i < gems.Count; i++)
        {
            pointsPerGemType[i] += gems[i].BaseValue * count[i];

            if (count[i] >= _numberOfGemsToMatch.Value + _multiplierThreshold.Value)
                pointsPerGemType[i] = (int)(pointsPerGemType[i] * ComboMatches[i].Gem.Multiplier);

            points += pointsPerGemType[i];
        }     
        return points;
    }

    private bool IsMatch(TileController tile)
    {
        CurrentMatch.Clear();
        CurrentMatch.Add(tile);
        CheckEveryNeighbourForMatch(tile);

        if (CurrentMatch.Count >= _numberOfGemsToMatch.Value)
        {
            for (int i = 0; i < CurrentMatch.Count; i++)
                ComboMatches.Add(CurrentMatch[i]);

            return true;
        }
        else
            return false;
    }

    private void CheckEveryNeighbourForMatch(TileController tile)
    {
        CheckOneNeighbourForMatch(tile.Gem.ID, tile.Neighbours[3]); // left 
        CheckOneNeighbourForMatch(tile.Gem.ID, tile.Neighbours[0]); // top
        CheckOneNeighbourForMatch(tile.Gem.ID, tile.Neighbours[2]); // down
        CheckOneNeighbourForMatch(tile.Gem.ID, tile.Neighbours[1]); // right
    }

    private bool CheckOneNeighbourForMatch(int ID, TileController tile)
    {
        if (tile == null)
            return false;

        if (tile.Gem == null)
            return false;

        if (tile.Gem.Type != tile.NormalGemType)
        {
            SpecialGemMatchTile = tile;
            return false;
        }

        if (tile.Gem.ID != ID)
            return false;

        if (CurrentMatch.Contains(tile))
            return false;

        CurrentMatch.Add(tile);
        CheckEveryNeighbourForMatch(tile);
        return true;
    }

    private bool CheckNeighbours(TileController tile, ref int count, bool addNodes = false)
    {
        if (!CheckSingleNeighbour(tile.Gem.ID, tile.Neighbours[3], ref count, addNodes)) // left 
            return false;
        if (!CheckSingleNeighbour(tile.Gem.ID, tile.Neighbours[0], ref count, addNodes)) // top
            return false;
        if (!CheckSingleNeighbour(tile.Gem.ID, tile.Neighbours[2], ref count, addNodes)) // down
            return false;
        return CheckSingleNeighbour(tile.Gem.ID, tile.Neighbours[1], ref count, addNodes); // right
    }

    private bool CheckSingleNeighbour(int ID, TileController tile, ref int count, bool addNodes = false)
    {
        if (tile == null)
            return true;

        if (tile.Gem == null)
            return true;

        if (tile.Gem.ID != ID)
            return true;

        if (count + 1 == _numberOfGemsToMatch.Value)
        {
            if (!addNodes)
                return false;
            else
            {
                count++;
                ComboMatches.Add(tile);
                return CheckNeighbours(tile, ref count, addNodes);
            }
        }
        else
        {
            if (addNodes)
                ComboMatches.Add(tile);
            count++;
            return CheckNeighbours(tile, ref count, addNodes);
        }
    }
    #endregion

    #region NEIGHBOURS
    private void UpdateNeighboursForEveryTile()
    {
        for (int y = 0; y < Rows; y++)
            for (int x = 0; x < Columns; x++)
                AssignNeighbours(Tiles[y, x], true);
    }

    // Two way assignment (like in the two way list). Kinda.
    // Assuming board creation started in the top left corner.
    public void AssignNeighbours(TileController tile, bool isBoardCreated = false)
    {
        Vector2Int pos = tile.GetBoardPosition();

        tile.Neighbours[0] = GetTopNeighbour(pos.x, pos.y);      
        tile.Neighbours[1] = GetRightNeighbour(pos.x, pos.y);
        tile.Neighbours[2] = GetBottomNeighbour(pos.x, pos.y);
        tile.Neighbours[3] = GetLeftNeighbour(pos.x, pos.y);
        if (!isBoardCreated)
        {
            if (tile.Neighbours[0] != null)
                AssignBottomNeighbour(tile.Neighbours[0]);

            if (tile.Neighbours[3] != null)
                AssignRightNeighbour(tile.Neighbours[3]);
        }   
    }

    private void AssignRightNeighbour(TileController tile)
    {
        Vector2Int pos = tile.GetBoardPosition();
        tile.Neighbours[1] = GetRightNeighbour(pos.x, pos.y);
    }

    private void AssignBottomNeighbour(TileController tile)
    {
        Vector2Int pos = tile.GetBoardPosition();
        tile.Neighbours[2] = GetBottomNeighbour(pos.x, pos.y);
    }

    private TileController GetTopLeftNeighbour(int x, int y)
    {
        return ((y - 1 >= 0) && (x - 1 >= 0)) ? (Tiles[y - 1, x - 1]) : null;
    }

    private TileController GetTopRightNeighbour(int x, int y)
    {
        return ((y - 1 >= 0) && (x + 1 < Columns)) ? (Tiles[y - 1, x + 1]) : null;
    }

    private TileController GetBottomLeftNeighbour(int x, int y)
    {
        return ((y + 1 < Rows) && (x - 1 >= 0)) ? (Tiles[y + 1, x - 1]) : null;
    }

    private TileController GetBottomRightNeighbour(int x, int y)
    {
        return ((y + 1 < Rows) && (x + 1 < Columns)) ? (Tiles[y + 1, x + 1]) : null;
    }

    private TileController GetTopNeighbour(int x, int y)
    {
        return (y - 1 >= 0) ? (Tiles[y - 1, x]) : null;
    }

    private TileController GetRightNeighbour(int x, int y)
    {
        return (x + 1 < Columns) ? (Tiles[y, x + 1]) : null;
    }

    private TileController GetBottomNeighbour(int x, int y)
    {
        return (y + 1 < Rows) ? (Tiles[y + 1, x]) : null;
    }

    private TileController GetLeftNeighbour(int x, int y)
    {
        return (x - 1 >= 0) ? (Tiles[y, x - 1]) : null;
    }
    #endregion
}