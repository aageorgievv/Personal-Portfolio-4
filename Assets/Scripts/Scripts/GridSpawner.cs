using UnityEngine;

public class GridSpawner : MonoBehaviour
{
    //Later make a Cell abstract class and Water, Land child classes
    [Header("References")]
    [SerializeField] private GameObject[] cellObjects;

    [Header("Settings")]
    [SerializeField] private int sizeX;
    [SerializeField] private int sizeZ;

    [SerializeField] private float gridSpacingOffset = 1f;

    [SerializeField] Vector3 gridOrigin = Vector3.zero;
    
    void Start()
    {
        SpawnGrid();
    }

    private void SpawnGrid()
    {
        for (int x = 0; x < sizeX; x++)
        {
            for(int z = 0; z < sizeZ; z++)
            {
                Vector3 spawnPosition = new Vector3(x * gridSpacingOffset, 0, z * gridSpacingOffset) + gridOrigin;
                SpawnCell(spawnPosition);
            }
        }
    }

    private void SpawnCell(Vector3 position)
    {
        int randomIndex = Random.Range(0, cellObjects.Length);
        GameObject cell = Instantiate(cellObjects[randomIndex], position, Quaternion.identity);
    }
}
