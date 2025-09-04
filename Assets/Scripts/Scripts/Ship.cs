using UnityEngine;

public class Ship : MonoBehaviour
{
    public int Size => size;
    public bool IsVertical => IsHorizontal;

    [Header("References")]

    [Header("Settings")]
    [SerializeField] private int size;
    [SerializeField] private bool IsHorizontal = false;

    private Cell currentStandingCell;
    private Vector3 spawnPosition;

    private void Start()
    {
        spawnPosition = transform.position;
    }

    public void Rotate()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            IsHorizontal = !IsHorizontal;
            transform.Rotate(0, IsHorizontal ? 90 : -90, 0);
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
