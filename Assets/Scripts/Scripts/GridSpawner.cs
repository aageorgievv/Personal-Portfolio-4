using Unity.VisualScripting;
using UnityEngine;

public class GridSpawner : MonoBehaviour
{
    public int GridSize => gridSize;
    public Cell[,] Cells => grid;
    //Later make a Cell abstract class and Water, Land child classes
    [Header("References")]
    [SerializeField] private Cell waterCellPrefab;
    [SerializeField] private Cell landCellPrefab;

    [Header("Grid Settings")]
    [SerializeField] private int gridSize;
    [SerializeField] private float gridSpacingOffset = 1f;
    [SerializeField] Vector3 gridOrigin = Vector3.zero;

    [Header("Island Settings")]
    [SerializeField, Range(1, 5)] private int islandSize = 2;
    [SerializeField, Range(0.1f, 0.5f)] private float islandChance = 0.1f;

    private bool[,] occupied;
    private Cell[,] grid;
    private GridManager gridManager;

    void Awake()
    {
        occupied = new bool[gridSize, gridSize];
        grid = new Cell[gridSize, gridSize];
        SpawnGrid();
        GameManager.ExecuteWhenInitialized(HandleAfterInitialized);
    }

    private void HandleAfterInitialized()
    {
        gridManager = GameManager.GetManager<GridManager>();
        ValidationUtility.ValidateReference(gridManager, nameof(gridManager));
        gridManager.Initialize(this);
    }

    private void SpawnGrid()
    {

        for (int x = 0; x < gridSize; x++)
        {
            for (int z = 0; z < gridSize; z++)
            {
                if (occupied[x, z])
                {
                    continue;
                }

                if (Random.value < islandChance && CanPlaceIsland(x, z))
                {
                    PlaceIsland(x, z);
                }
                else
                {
                    grid[x, z] = SpawnCell(waterCellPrefab, x, z);
                }
            }
        }
    }

    private Cell SpawnCell(Cell prefab, int x, int z)
    {
        Vector3 spawnPosition = new Vector3(x * gridSpacingOffset, 0, z * gridSpacingOffset) + gridOrigin;
        Cell cell = Instantiate(prefab, spawnPosition, Quaternion.identity, transform);
        occupied[x, z] = true;
        cell.SetCell(x, z);

        return cell;
    }

    private void PlaceIsland(int startX, int startZ)
    {
        for (int x = 0; x < islandSize; x++)
        {
            for (int z = 0; z < islandSize; z++)
            {
                grid[startX + x, startZ + z] = SpawnCell(landCellPrefab, startX + x, startZ + z);
            }
        }
    }

    private bool CanPlaceIsland(int startX, int startZ)
    {
        if (startX + islandSize > gridSize || startZ + islandSize > gridSize)
        {
            return false;
        }

        for (int x = 0; x < islandSize; x++)
        {
            for (int z = 0; z < islandSize; z++)
            {
                if (occupied[startX + x, startZ + z])
                {
                    return false;
                }
            }
        }

        return true;
    }
}
