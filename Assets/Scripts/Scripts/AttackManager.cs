using UnityEngine;

public class AttackManager : MonoBehaviour, IManager
{
    private float raycastDistance = 100f;
    private Camera mainCam;

    private ulong activePlayerId;
    private void Awake()
    {
        mainCam = Camera.main;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && PlayerState.localPlayer != null)
        {
            if(PlayerState.localPlayer.OwnerClientId != activePlayerId)
            {
                return;
            }

            Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, raycastDistance))
            {
                Cell cell = hit.collider.GetComponent<Cell>();

                if (cell != null && cell.IsAttackable)
                {
                    PlayerState.localPlayer.AttackCell(cell.Row, cell.Col);
                }
            }
        }
    }

    public void SetActivePlayerTurn(ulong playerId)
    {
        activePlayerId = playerId;
    }
}
