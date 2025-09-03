using Unity.VisualScripting;
using UnityEngine;

public class ShipSelection : MonoBehaviour
{
    [SerializeField] private Ship currentlyHeldShip;

    private Plane boardPlane;
    private float yOffset = 0.6f;
    private int raycastDistance = 30;

    private void Start()
    {
        boardPlane = new Plane(Vector3.up, Vector3.zero);
    }

    private void Update()
    {
        UpdateMouseLeft();
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
        if(ship == null)
        {
            return;
        }

        Cell cell = ship.GetNearestCell();

        if (cell != null)
        {
            Vector3 snapPosition = new Vector3(cell.transform.position.x, yOffset, cell.transform.position.z);
            ship.transform.position = snapPosition;
            Debug.Log($"Snapped to {cell.GetCellType()} at {snapPosition}");
        }
    }
}
