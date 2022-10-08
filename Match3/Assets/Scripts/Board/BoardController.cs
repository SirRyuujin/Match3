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
    public IntVariable AllowedMoveDistance;
    public IntVariable ChanceToSpawnSpecialObject;
    [SerializeField] private IntVariable _numberOfGemsToMatch;
    [SerializeField] private FloatVariable _clickCooldown;
    // how many more gems have to be matched in order to get bonus points
    [SerializeField] private IntVariable _multiplierThreshold;

    [Space(10)]
    public List<TileController> Matches = new List<TileController>();
    public TileController[,] Tiles;
    public List<TileController> SelectedTiles = new List<TileController>();
    [HideInInspector] public int Rows { get; private set; }
    [HideInInspector] public int Columns { get; private set; }
    private bool _canSelectGem = true;
    private float _clickCooldownTimer = 0;
    public int Score { get; private set; }

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

    private bool CheckNeighbours(TileController tile, ref int count, bool addNodes = false)
    {
        if (!CheckSingleNeighbour(tile.Gem.ID, tile.Neighbours[3], ref count, addNodes)) // left 
            return false;
        if (!CheckSingleNeighbour(tile.Gem.ID, tile.Neighbours[0], ref count,addNodes)) // top
            return false;
        if (!CheckSingleNeighbour(tile.Gem.ID, tile.Neighbours[2], ref count,addNodes)) // down
            return false;
        return CheckSingleNeighbour(tile.Gem.ID, tile.Neighbours[1], ref count,addNodes); // right
    }

    private bool CheckSingleNeighbour(int ID, TileController tile, ref int count, bool addNodes = false)
    {
        if (tile == null)
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

    public void SetBoardDimensions(int rows, int columns)
    {
        Rows = rows;
        Columns = columns;
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
        if (SelectedTiles.Count == 2)
            TryDeselectBothSelectedTiles();
    }

    private void TryDeselectBothSelectedTiles()
    {
        DeselectTile(SelectedTiles[0]);
        if (SelectedTiles.Count > 0)
            DeselectTile(SelectedTiles[0]);
    }

    private void DeselectTile(TileController tile)
    {
        SelectedTiles.Remove(tile);
        OnDeselectGemEvent.Raise(tile.gameObject);
    }

    private void TrySwapGemsOnSelectedTiles()
    {
        bool flag = false;

        Gem temp = SelectedTiles[0].Gem;
        SelectedTiles[0].AssignGem(SelectedTiles[1].Gem);
        SelectedTiles[1].AssignGem(temp);

 //       AssignNeighbours(SelectedTiles[0], true);
  //      AssignNeighbours(SelectedTiles[1], true);

        flag = TryMatch();
        if (!flag)
            ReverseSwap();

        // drop tiles
        // create new tiles

        UpdateNeighboursForEveryTile();
    }

    private bool TryMatch()
    {
        bool flag = IsMatch(SelectedTiles[0]);
        if (flag)
        {
            StartCoroutine(MatchCoroutine());

            bool tmp = IsMatch(SelectedTiles[1]);
            if (tmp)
                StartCoroutine(MatchCoroutine());
        }
        else
        {
            flag = IsMatch(SelectedTiles[1]);
            if (flag)
                StartCoroutine(MatchCoroutine());
        }
        
        return flag;
    }

    private void ReverseSwap()
    {

    }

    private void DropTiles()
    {

    }

    private IEnumerator MatchCoroutine()
    {
        OnGemSwapEvent.Raise(gameObject);
        StartCoroutine(AnimationSwapSyncCoroutine());
        yield return null;
    }

    private IEnumerator AnimationSwapSyncCoroutine()
    {
        yield return new WaitForSeconds(_clickCooldownTimer / 2);
        Score += CalculatePoints();
        DeleteGems();
        UpdateNeighboursForEveryTile();
    }

    private void DeleteGems()
    {
        for (int i = 0; i < Matches.Count; i++)
            Matches[i].AssignGem(null);
    }

    private int CalculatePoints()
    {
        return (Matches.Count >= _multiplierThreshold.Value) ?
            (int)(Matches[0].Gem.BaseValue * Matches[0].Gem.Multiplier) :
            Matches[0].Gem.BaseValue;
    }

    private bool IsMatch(TileController tile)
    {
        Matches.Clear();
        Matches.Add(tile);
        LookForConnectedNodes(tile);

        return Matches.Count >= _numberOfGemsToMatch.Value;
    }

    private void LookForConnectedNodes(TileController tile)
    {
        CheckEveryNeighbour(tile);
    }

    private void CheckEveryNeighbour(TileController tile)
    {
        CheckOneNeighbour(tile.Gem.ID, tile.Neighbours[3]); // left 
        CheckOneNeighbour(tile.Gem.ID, tile.Neighbours[0]); // top
        CheckOneNeighbour(tile.Gem.ID, tile.Neighbours[2]); // down
        CheckOneNeighbour(tile.Gem.ID, tile.Neighbours[1]); // right
    }

    private bool CheckOneNeighbour(int ID, TileController tile)
    {
        if (tile == null)
            return false;

        if (tile.Gem.ID != ID)
            return false;

        if (Matches.Contains(tile))
            return false;

        Matches.Add(tile);
        return true;
    }

    public void UpdateNeighboursForEveryTile()
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

    private bool IsInDistance(Vector2Int origin,Vector2Int target, int distance)
    {
        float dist = Vector2Int.Distance(origin, target);
        return (dist <= distance && dist % 1 == 0);
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
}