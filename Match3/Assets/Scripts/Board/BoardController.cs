using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// Refactor recursion
public class BoardController : MonoBehaviour
{
    [Header("References")]
    public GameEventSO OnTrySelectGemEvent;
    public GameEventSO OnSelectGemEvent;
    public GameEventSO OnDeselectGemEvent;
    public GameEventSO OnGemSwapEvent;
    public GameEventSO OnEndSwapEvent;
    public GameEventSO OnFailedSwapEvent;
    public IntVariable AllowedMoveDistance;
    public IntVariable ChanceToSpawnSpecialObject;
    [SerializeField] private IntVariable _numberOfGemsToMatch;
    [SerializeField] private FloatVariable _clickCooldown;
    // how many more gems have to be matched in order to get bonus points
    [SerializeField] private IntVariable _multiplierThreshold;

    [Space(10)]
    public List<TileController> Matches = new List<TileController>();
    public List<TileController> ComboMatches = new List<TileController>(); // Holds info about all matched tiles in last move.
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
    ///     Return TRUE when there's no match. </returns>
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

    private bool flag = false;
    private bool flag2 = false;

    private void SpawnNewGems(int amount)
    {
        List<TileController> blankTiles = GetBlankTiles();
        TileController tile = blankTiles[0];

        for (int i = 0; i < blankTiles.Count; i++)
            blankTiles[i].AssignGem(tile.GetGemOfRandomType(ChanceToSpawnSpecialObject.Value));
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

            // put in method: GetBlankTiles
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

    private void SpawnNewTilesSequence()
    {
        int totalBlanks = 0;

        DropTiles(ref totalBlanks);
        ComboMatches.Clear();
        SpawnNewGems(totalBlanks);
        UpdateNeighboursForEveryTile();
        // check for new matches
    }


    #region CLICKING
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
            else; // visually notify that you cannot pick that

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

    #region SWAPPING/MATCHING
    private void TrySwapGemsOnSelectedTiles()
    {
        SwapGems(SelectedTiles[0], SelectedTiles[1]);
        StartCoroutine(TryMatch());
    }

    /// Refactor
    // Check both selectd games for matches (hold in ComboMatches list)
    // Release'em both at the same time
    // ^gets rid of the delay + fixes scroe
    private IEnumerator TryMatch()
    {
        flag = false;
        flag2 = true;

        flag = IsMatch(SelectedTiles[0]);
        if (flag)
        {
            OnGemSwapEvent.Raise(gameObject);
            StartCoroutine(MatchCoroutine());
            while (!flag2)
                yield return null;

            bool tmp = IsMatch(SelectedTiles[1]);
            if (tmp)
                StartCoroutine(MatchCoroutine());
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

        if (!flag) // Reswap, no matches.
        {
            SwapGems(SelectedTiles[0], SelectedTiles[1]);
            OnFailedSwapEvent.Raise(gameObject);
        }
        else
        {
            UpdateNeighboursForEveryTile();
            OnEndSwapEvent.Raise(gameObject);
            SpawnNewTilesSequence();
        }

        TryDeselectBothSelectedTiles();
    }

    private IEnumerator MatchCoroutine()
    {
        for (int i = 0; i < Matches.Count; i++)
            ComboMatches.Add(Matches[i]);

        flag2 = false;
        StartCoroutine(AnimationSwapSyncCoroutine());
        while (!flag2)
            yield return null;
    }

    private IEnumerator AnimationSwapSyncCoroutine()
    {
        yield return new WaitForSeconds(_clickCooldown.Value);
        Score += CalculatePoints();
        DeleteGems();
        UpdateNeighboursForEveryTile();
        flag2 = true;
    }

    private void SwapGems(TileController tile1, TileController tile2)
    {
        Gem temp = tile1.Gem;
        tile1.AssignGem(tile2.Gem);
        tile2.AssignGem(temp);
    }

    private void DeleteGems()
    {
        for (int i = 0; i < Matches.Count; i++)
        {
            var pos = Matches[i].GetBoardPosition();
            Tiles[pos.y, pos.x].AssignGem(null);
            Matches[i].AssignGem(null);
        }
    }

    private int CalculatePoints()
    {
        int count = Matches.Count;
        return (count >= _multiplierThreshold.Value) ?
            (int)(Matches[0].Gem.BaseValue * Matches[0].Gem.Multiplier * count) :
            Matches[0].Gem.BaseValue * count;
    }

    private bool IsMatch(TileController tile)
    {
        Matches.Clear();
        Matches.Add(tile);
        CheckEveryNeighbourForMatch(tile);

        return Matches.Count >= _numberOfGemsToMatch.Value;
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
            return false;

        if (tile.Gem.ID != ID)
            return false;

        if (Matches.Contains(tile))
            return false;

        Matches.Add(tile);
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
                Matches.Add(tile);
                return CheckNeighbours(tile, ref count, addNodes);
            }
        }
        else
        {
            if (addNodes)
                Matches.Add(tile);
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