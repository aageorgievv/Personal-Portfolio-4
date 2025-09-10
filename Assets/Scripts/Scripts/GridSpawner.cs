using System;
using Unity.Netcode;
using UnityEngine;

//public class GridState : NetworkBehaviour
//{
//    public bool isSpawned;
//    public ECellType[,] cells;
//}

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

    //private NetworkVariable<GridState> gridState = new NetworkVariable<GridState>();

    void Awake()
    {
        occupied = new bool[gridSize, gridSize];
        grid = new Cell[gridSize, gridSize];
        //gridState.Value.cells = new ECellType[gridSize, gridSize];
        SpawnGrid();
        GameManager.ExecuteWhenInitialized(HandleAfterInitialized);

        //gridState.OnValueChanged += OnGridValueChanged;
    }
    
    private void OnDestroy()
    {
        //gridState.OnValueChanged -= OnGridValueChanged;
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

                if (UnityEngine.Random.value < islandChance && CanPlaceIsland(x, z))
                {
                    PlaceIsland(x, z);

                    //for (int x2 = 0; x2 < islandSize; x2++)
                    //{
                    //    for(int z2 = 0; z2 < islandSize; z2++)
                    //    {
                    //        gridState.Value.cells[x2, z2] = ECellType.Water;
                    //    }
                    //}
                }
                else
                {
                    grid[x, z] = SpawnCell(waterCellPrefab, x, z);

                    //gridState.Value.cells[x, z] = ECellType.Land;
                }
            }
        }

        //gridState.Value.isSpawned = true;
    }

    //private void OnGridValueChanged(GridState prevValue, GridState newValue)
    //{
    //    if (!newValue.isSpawned)
    //    {
    //        return;
    //    }

    //    for (int x = 0; x < gridSize; x++)
    //    {
    //        for (int z = 0; z < gridSize; z++)
    //        {
    //            ECellType cellType = gridState.Value.cells[x, z];
    //            switch (cellType)
    //            {
    //                case ECellType.Water:
    //                    grid[x, z] = SpawnCell(waterCellPrefab, x, z);
    //                    break;
    //                case ECellType.Land:
    //                    grid[x, z] = SpawnCell(landCellPrefab, x, z);
    //                    break;
    //                default: throw new NotImplementedException(nameof(cellType));
    //            }
    //        }
    //    }
    //}

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
