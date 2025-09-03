using Unity.VisualScripting;
using UnityEngine;

public class GridSpawner : MonoBehaviour
{
    //Later make a Cell abstract class and Water, Land child classes
    [Header("References")]
    [SerializeField] private Cell waterCellPrefab;
    [SerializeField] private Cell landCellPrefab;

    [Header("Grid Settings")]
    [SerializeField] private int sizeX;
    [SerializeField] private int sizeZ;
    [SerializeField] private float gridSpacingOffset = 1f;
    [SerializeField] Vector3 gridOrigin = Vector3.zero;

    [Header("Island Settings")]
    [SerializeField] private int islandSize = 2;
    [SerializeField] private float islandChance = 0.1f;

    private bool[,] occupied;

    void Start()
    {
        occupied = new bool[sizeX, sizeZ];
        SpawnGrid();
    }

    private void SpawnGrid()
    {
        for (int x = 0; x < sizeX; x++)
        {
            for (int z = 0; z < sizeZ; z++)
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
                    SpawnCell(waterCellPrefab, x, z);
                }
            }
        }
    }

    private void SpawnCell(Cell prefab, int x, int z)
    {
        Vector3 spawnPosition = new Vector3(x * gridSpacingOffset, 0, z * gridSpacingOffset) + gridOrigin;
        Instantiate(prefab, spawnPosition, Quaternion.identity, transform);
        occupied[x, z] = true;
    }

    private void PlaceIsland(int startX, int startZ)
    {
        for (int x = 0; x < islandSize; x++)
        {
            for (int z = 0; z < islandSize; z++)
            {
                SpawnCell(landCellPrefab, startX + x, startZ + z);
            }
        }
    }

    private bool CanPlaceIsland(int startX, int startZ)
    {
        if (startX + islandSize > sizeX || startZ + islandSize > sizeZ)
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
