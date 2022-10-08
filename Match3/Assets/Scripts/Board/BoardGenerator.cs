using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class BoardGenerator : MonoBehaviour
{
    private int _rows;
    private int _columns;
    private const int WALL_SIZE = 5;
    private int _tileSize;

    [Header("Customization")]
    public bool AddWalls = true;

    [Header("References")]
    public IntVariable DesiredNrOfRows;
    public IntVariable DesiredNrOfColumns;
    [SerializeField] private BoardController _boardController;
    [SerializeField] private RectTransform _boardTransform;
    [SerializeField] private GameObject _board;
    [SerializeField] private GameObject _wallPrefab;
    [SerializeField] private GameObject _tilePrefab;
    [SerializeField] private RectTransform _tilesParent;
    [SerializeField] private RectTransform _wallsParent;
    [SerializeField] private IntVariable _minNrOfRowsAndColumns;
    [SerializeField] private IntVariable _maxNrOfRows;
    [SerializeField] private IntVariable _maxNrOfColumns;
    [SerializeField] private IntVariable _minTileSize;
    [SerializeField] private IntVariable _maxTileSize;
    
    
    private void Awake()
    {
        ValidateAndSetTileSize();
        GenerateBoard();
    }

    public void GenerateBoard()
    {
        CleanUpChildObjects();
        ValidateAndSetRowsAndColumns(DesiredNrOfRows.Value, DesiredNrOfColumns.Value);
        SetBoardSize();
        if (AddWalls)
            SplitBoardWithWalls((int)_boardTransform.sizeDelta.x, (int)_boardTransform.sizeDelta.y);

        _boardController.SetBoardDimensions(_rows, _columns);
        AddTiles();
    }

    private void CleanUpChildObjects()
    {
        foreach (Transform child in _tilesParent)
            Destroy(child.gameObject);

        foreach (Transform child in _wallsParent)
            Destroy(child.gameObject);
    }

    private void AddTiles()
    {
        _boardController.Tiles = new TileController[_rows, _columns];
        int startingPosX = -(int)_boardTransform.sizeDelta.x / 2 + _tileSize / 2;
        int startingPosY = (int)_boardTransform.sizeDelta.y / 2 - _tileSize / 2;

        for (int y = 0; y < _rows; y++)
        {
            for (int x = 0; x < _columns; x++)
            {
                int offsetX = x * _tileSize;
                int offsetY = -y * _tileSize;
                GameObject tile = Instantiate(_tilePrefab, _tilesParent);
                tile.transform.localPosition = new Vector3(startingPosX + offsetX, startingPosY + offsetY, 0);

                TileController tileController = tile.GetComponent<TileController>();
                _boardController.Tiles[y, x] = tileController;
                SetSpawnedTileProperties(tileController, new Vector2(x, y));
            }
        }
    }

    private void SetSpawnedTileProperties(TileController tileController, Vector2 pos)
    {
        tileController.SetBoardControllerReference(_boardController);
        int specialSpawnChance = GetValidValueInGivenRange(_boardController.ChanceToSpawnSpecialObject.Value, 1, 100);
        tileController.SetPosition(pos);
        _boardController.AssignNeighbours(tileController);

        Gem drawnGem; 
        do
        {
            drawnGem = tileController.GetGemOfRandomType(specialSpawnChance);
            tileController.AssignGem(drawnGem);
        } while (!_boardController.IsDrawnGemValid(tileController));   
    }

    private void AnchorBoardToTheRightSide()
    {
        _boardTransform.anchorMin = new Vector2(1, 0.5f);
        _boardTransform.anchorMax = new Vector2(1, 0.5f);
        _boardTransform.pivot = new Vector2(1, 0.5f);
    }

    private void SetBoardSize()
    {
        int height = _rows * _tileSize;
        int width = _columns * _tileSize;
        _boardTransform.sizeDelta = new Vector2(width, height);
    }

    private void SplitBoardWithWalls(int width, int height)
    {
        int nrOfHorizontalWalls = _rows- 1;
        int nrOfVerticalWalls = _columns - 1;
        int tileHeight = height / _rows;
        int tileWidth = width  / _columns;

        SpawnWalls(nrOfVerticalWalls, nrOfHorizontalWalls, tileWidth, tileHeight);
    }

    private void SpawnWalls(int nrOfVerticalWalls, int nrOfHorizontalWalls, int tileWidth, int tileHeight)
    {
        for (int i = 0; i < nrOfVerticalWalls; i++)
        {
            GameObject go = Instantiate(_wallPrefab, _wallsParent);
            go.transform.localPosition = new Vector3((-_boardTransform.sizeDelta.x / 2) + tileWidth * (i + 1), 0);
            go.GetComponent<RectTransform>().sizeDelta = new Vector2(WALL_SIZE, _boardTransform.sizeDelta.y);
        }

        for (int i = 0; i < nrOfHorizontalWalls; i++)
        {
            GameObject go = Instantiate(_wallPrefab, _wallsParent);
            go.transform.eulerAngles = new Vector3(0, 0, 90);
            go.transform.localPosition = new Vector3(0, (-_boardTransform.sizeDelta.y / 2) + tileHeight * (i + 1));
            go.GetComponent<RectTransform>().sizeDelta = new Vector2(WALL_SIZE, _boardTransform.sizeDelta.x);
        }
    }

    private void ValidateAndSetTileSize()
    {
        _tileSize = GetValidValueInGivenRange((int)_tilePrefab.GetComponent<RectTransform>().sizeDelta.x, _minTileSize.Value, _maxTileSize.Value);
    }

    private void ValidateAndSetRowsAndColumns(int nrOfRows, int nrOfColumns)
    {
        _rows = GetValidValueInGivenRange(nrOfRows, _minNrOfRowsAndColumns.Value, _maxNrOfRows.Value);
        _columns = GetValidValueInGivenRange(nrOfColumns, _minNrOfRowsAndColumns.Value, _maxNrOfColumns.Value);
    }

    private int GetValidValueInGivenRange(int value, int minRange, int maxRange)
    {
        if (value < minRange)
            return minRange;
        else if (value > maxRange)
            return maxRange;

        return value;
    }
}