using System.Runtime.CompilerServices;
using UnityEngine;

public class GridManager : MonoBehaviour, IManager
{
    [SerializeField] private GridSpawner gridSpawner;

    private Cell[,] grid;
    private int gridSize;

    private void Awake()
    {
        ValidationUtility.ValidateReference(gridSpawner, nameof(gridSpawner));
        gridSize = gridSpawner.GridSize;
        grid = gridSpawner.Cells;
    }

    public Cell GetCell(int row, int col)
    {
        if (row < 0 || col < 0 || row >= gridSize || col >= gridSize)
        {
            return null;

        }
        return grid[row, col];
    }
}
