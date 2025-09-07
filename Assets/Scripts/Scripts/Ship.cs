using UnityEngine;

public class Ship : MonoBehaviour
{
    public int Size => size;
    public bool IsHorizontal => isHorizontal;

    [Header("References")]

    [Header("Settings")]
    [SerializeField] private int size;
    [SerializeField] private bool isHorizontal = false;

    private Cell currentStandingCell;
    private Vector3 spawnPosition;

    private void Awake()
    {
        spawnPosition = transform.position;
    }

    public void Rotate()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            isHorizontal = !isHorizontal;
            transform.Rotate(0, isHorizontal ? 90 : -90, 0);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Cell cell = other.gameObject.GetComponent<Cell>();

        if (cell != null)
        {
            currentStandingCell = cell;
        }
    }

    public Cell GetNearestCell()
    {

        // sphere cast
        // get a list of cells in the sphere
        // get the nearest one
        // return it

        // or the more common way
        // give the ship position to the grid manager
        // and ask the grid manager to tell you what cell that is based on math, usually (position / cellSize)


        return currentStandingCell;
    }

    public void ReturnToSpawnPosition()
    {
        transform.position = spawnPosition;
    }

    private void Sink()
    {
        Destroy(gameObject);
    }
}
