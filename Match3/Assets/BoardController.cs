using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BoardController : MonoBehaviour
{
    [Header("References")]
    public GameEventSO OnGemClickedEvent;
    public GameEventSO OnGemsSwappedEvent;
    public IntVariable AllowedMoveDistance;
    public IntVariable _chanceToSpawnSpecialObject;
    [SerializeField] private IntVariable _numberOfGemsToMatch;

    [Header("Preview")]
    public TileController[,] Tiles;
    public List<TileController> SelectedTiles = new List<TileController>();
    [HideInInspector] public int Rows { get; private set; }
    [HideInInspector] public int Columns { get; private set; }


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

    private bool CheckNeighbours(TileController tile, ref int count)
    {
        if (!CheckSingleNeighbour(tile.Gem.ID, tile.Neighbours[3], ref count)) // left 
            return false;
        if (!CheckSingleNeighbour(tile.Gem.ID, tile.Neighbours[0], ref count)) // top
            return false;
        return CheckSingleNeighbour(tile.Gem.ID, tile.Neighbours[1], ref count); // top
    }

    private bool CheckSingleNeighbour(int ID, TileController tile, ref int count)
    {
        if (tile == null)
            return true;

        if (tile.Gem.ID != ID)
            return true;

        if (count + 1 == _numberOfGemsToMatch.Value)
            return false;
        else
        {
            count++;
            return CheckNeighbours(tile, ref count);
        }
    }

    public void SelectTile()
    {
        if (OnGemClickedEvent.RecentCaller == null)
            return;

        TileController tile = OnGemClickedEvent.RecentCaller.GetComponent<TileController>();

        //
        if (SelectedTiles.Count == 0)
        {
            SelectedTiles.Add(tile);
        }
        else if (SelectedTiles.Count == 1)
        {
            if (tile.Gem.Type == SelectedTiles[0].Gem.Type
                && tile.Gem.ID == SelectedTiles[0].Gem.ID)
            {
                SelectedTiles.Clear();
                return;
            }

            if (!SelectedTiles.Contains(tile) &&
                IsInDistance(SelectedTiles[0].GetPosition(), tile.GetPosition(), AllowedMoveDistance.Value))
            {
                SelectedTiles.Add(tile);
                SwapGemsOnSelectedTiles();
            }
            else; // visually notify that you cannot pick that
            SelectedTiles.Clear();
        }
    }

    public void SetBoardDimensions(int rows, int columns)
    {
        Rows = rows;
        Columns = columns;
    }

    private void SwapGemsOnSelectedTiles()
    {
        Gem temp = SelectedTiles[0].Gem;
        SelectedTiles[0].AssignGem(SelectedTiles[1].Gem);
        SelectedTiles[1].AssignGem(temp);

        OnGemsSwappedEvent.Raise(gameObject);
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
        Vector2Int pos = tile.GetPosition();

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
        return (Vector2Int.Distance(origin, target) <= distance);
    }

    private void AssignRightNeighbour(TileController tile)
    {
        Vector2Int pos = tile.GetPosition();
        tile.Neighbours[1] = GetRightNeighbour(pos.x, pos.y);
    }

    private void AssignBottomNeighbour(TileController tile)
    {
        Vector2Int pos = tile.GetPosition();
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