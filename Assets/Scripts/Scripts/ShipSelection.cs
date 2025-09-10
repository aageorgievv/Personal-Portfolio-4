using System;
using UnityEngine;

public class ShipSelection : MonoBehaviour, IManager
{
    public Action<int> OnShipPlacedEvent;

    private Plane boardPlane;
    private float yOffset = 0.2f;
    private int raycastDistance = 30;
    private int currentlyPlacedShips = 0;

    private Ship currentlyHeldShip;
    private GridManager gridManager;

    private void Awake()
    {
        boardPlane = new Plane(Vector3.up, Vector3.zero);
        GameManager.ExecuteWhenInitialized(HandleWhenInitialized);
    }

    private void OnDestroy()
    {

    }

    private void Update()
    {
        UpdateMouseLeft();
    }

    private void HandleWhenInitialized()
    {
        gridManager = GameManager.GetManager<GridManager>();
        ValidationUtility.ValidateReference(gridManager, nameof(gridManager));
    }

    private void UpdateMouseLeft()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(ray, out RaycastHit hit, raycastDistance))
            {
                Ship ship = hit.collider.GetComponent<Ship>();

                if (ship != null)
                {
                    currentlyHeldShip = ship;
                }
            }
        }

        if (Input.GetMouseButton(0) && currentlyHeldShip != null)
        {
            if (boardPlane.Raycast(ray, out float enter))
            {
                Vector3 hitPoint = ray.GetPoint(enter);
                currentlyHeldShip.transform.position = new Vector3(hitPoint.x, yOffset, hitPoint.z);
                currentlyHeldShip.Rotate();
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            SnapShipToNearestCell(currentlyHeldShip);
            currentlyHeldShip = null;
        }
    }

    private void SnapShipToNearestCell(Ship ship)
    {
        if (ship == null)
        {
            return;
        }

        Cell anchorCell = ship.GetNearestCell();

        if (anchorCell == null)
        {
            return;
        }

        bool isValid = true;
        Cell[] occupiedCells = new Cell[ship.Size];

        for (int i = 0; i < ship.Size; i++)
        {
            int row = anchorCell.Row + (ship.IsHorizontal ? i : 0);
            int col = anchorCell.Col + (ship.IsHorizontal ? 0 : i);

            Cell cell = gridManager.GetCell(row, col);

            if (cell == null || cell.GetCellType() == ECellType.Land)
            {
                isValid = false;
                break;
            }

            occupiedCells[i] = cell;
        }

        if (isValid)
        {
            Vector3 snapPosition = new Vector3(anchorCell.transform.position.x, yOffset, anchorCell.transform.position.z);
            ship.transform.position = snapPosition;
            currentlyPlacedShips++;
            OnShipPlacedEvent?.Invoke(currentlyPlacedShips);
            Debug.Log($"Snapped to {anchorCell.GetCellType()} at {snapPosition}");
        }
        else
        {
            if(currentlyPlacedShips > 0)
            {
                currentlyPlacedShips--;
            }

            ship.ReturnToSpawnPosition();
            OnShipPlacedEvent?.Invoke(currentlyPlacedShips);
            Debug.LogError("Invalid placement! Outside of grid or Land");
        }
    }
}
