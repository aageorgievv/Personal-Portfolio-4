using UnityEngine;

public class Ship : MonoBehaviour
{
    public int Size => size;
    public bool IsVertical => isVertical;

    [Header("Settings")]
    [SerializeField] private int size;
    [SerializeField] private int hitpoints;

    private bool isVertical = false;

    private Cell currentStandingCell;

    public void Rotate()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            isVertical = !isVertical;
            transform.Rotate(0, isVertical ? 90 : -90, 0);
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

    private void Sink()
    {
        Destroy(gameObject);
    }
}
