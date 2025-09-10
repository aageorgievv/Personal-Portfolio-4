using UnityEngine;

public class Ship : MonoBehaviour
{
    public int Size => size;
    public bool IsHorizontal => isHorizontal;

    [Header("References")]

    [Header("Settings")]
    [SerializeField] private int size;

    private bool isHorizontal = true;
    private GridManager gridManager;
    private Vector3 spawnPosition;

    private void Awake()
    {
        spawnPosition = transform.position;
        GameManager.ExecuteWhenInitialized(HandleWhenInitialized);
    }

    private void HandleWhenInitialized()
    {
        gridManager = GameManager.GetManager<GridManager>();
        ValidationUtility.ValidateReference(gridManager, nameof(gridManager));
    }

    public void Rotate()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            isHorizontal = !isHorizontal;
            transform.Rotate(0, isHorizontal ? 90 : -90, 0);
        }
    }

    public Cell GetNearestCell()
    {

        float searchRadius = 1.25f;

        Collider[] hits = Physics.OverlapSphere(transform.position, searchRadius);

        Cell nearestCell = null;
        float minDistance = float.MaxValue;

        foreach (Collider hit in hits)
        {
            Cell cell = hit.GetComponent<Cell>();

            if (cell != null)
            {
                float distance = Vector3.Distance(transform.position, cell.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestCell = cell;
                }
            }
        }

        return nearestCell;
    }

/*    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        float radius = gridManager.CellSize * 0.3f;
        Gizmos.DrawWireSphere(transform.position, radius);
    }*/

    public void ReturnToSpawnPosition()
    {
        transform.position = spawnPosition;
    }

    private void Sink()
    {
        Destroy(gameObject);
    }
}
