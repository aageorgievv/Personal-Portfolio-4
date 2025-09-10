using UnityEngine;

public class GridManager : MonoBehaviour, IManager
{
    private Cell[,] grid;
    private int gridSize;

    private GridSpawner gridSpawner;

    public void Initialize(GridSpawner spawner)
    {
        gridSpawner = spawner;
        grid = gridSpawner.Cells;
        gridSize = gridSpawner.GridSize;
    }

    public Cell GetCell(int row, int col)
    {
        ValidationUtility.ValidateReference(grid, nameof(grid));

        if (row < 0 || col < 0 || row >= gridSize || col >= gridSize)
        {
            return null;

        }
        return grid[row, col];
    }
}
