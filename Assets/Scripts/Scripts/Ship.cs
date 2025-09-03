using UnityEngine;

public class Ship : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int size;
    [SerializeField] private int hitpoints;

    private bool isPlaced;
    private bool isVertical;

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

    private void TakeHit()
    {
        hitpoints--;

        if(hitpoints <= 0)
        {
            Sink();
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
