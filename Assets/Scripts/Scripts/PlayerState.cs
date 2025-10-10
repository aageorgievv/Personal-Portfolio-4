using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayerState : NetworkBehaviour
{
    public NetworkVariable<int> PlacedShips => placedShips;
    public NetworkVariable<bool> IsReady => isReady;

    public static PlayerState localPlayer;

    private NetworkVariable<int> placedShips = new NetworkVariable<int>(0);
    private NetworkVariable<bool> isReady = new NetworkVariable<bool>(false);

    public NetworkList<ShipPlacementData> Ships { get; private set; } = new NetworkList<ShipPlacementData>();

    private GameManager gameManager;
    private ShipSelection shipSelection;

    private int shipsRequired = 5;

    private readonly List<(int row, int col, Color color)> attackHistory = new();
    private readonly List<(int row, int col, Color color)> defenseHistory = new();

    private Dictionary<ShipPlacementData, int> shipHealth = new();

    private bool hasAttacked = false;

    private void Awake()
    {
        GameManager.ExecuteWhenInitialized(HandleWhenInitialized);
    }

    private void HandleWhenInitialized()
    {
        gameManager = GameManager.GetManager<GameManager>();
        shipSelection = GameManager.GetManager<ShipSelection>();
        ValidationUtility.ValidateReference(gameManager, nameof(gameManager));
        ValidationUtility.ValidateReference(shipSelection, nameof(shipSelection));
        shipSelection.OnShipPlacedEvent += HandleShipPlacedEvent;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        shipSelection.OnShipPlacedEvent -= HandleShipPlacedEvent;
        Ships.OnListChanged -= HandleOnShipsListChanged;
    }

    private void Update()
    {
        if (IsOwner && Input.GetKeyDown(KeyCode.K))
        {
            SubmitReadyToServerRpc();
        }
    }

    public override void OnNetworkSpawn()
    {
        if (Ships == null)
        {
            Ships = new NetworkList<ShipPlacementData>();
        }

        if (IsOwner)
        {
            localPlayer = this;
        }

        Ships.OnListChanged += HandleOnShipsListChanged;
    }

    private void HandleShipPlacedEvent(int count)
    {
        if (IsOwner)
        {
            SubmitPlacedShipsToServerRpc(count);
        }
    }

    public void SetReady()
    {
        if (IsOwner)
        {
            SubmitReadyToServerRpc();
            SendShipsToServer();
        }
    }

    [ServerRpc]
    private void SubmitPlacedShipsToServerRpc(int count)
    {
        placedShips.Value = count;
    }

    [ServerRpc]
    private void SubmitReadyToServerRpc()
    {
        bool allShipsPlaced = placedShips.Value >= shipsRequired;
        isReady.Value = allShipsPlaced;
    }

    [ServerRpc]
    private void SubmitShipsToServerRpc(ShipPlacementData[] ships)
    {
        Ships.Clear();
        shipHealth.Clear();

        foreach (var ship in ships)
        {
            Ships.Add(ship);
            shipHealth[ship] = ship.size;
        }

        placedShips.Value = Ships.Count;
    }

    private void SendShipsToServer()
    {
        if (!IsOwner)
        {
            return;
        }

        Ship[] playerShips = shipSelection.GetAllShips();
        ShipPlacementData[] placements = new ShipPlacementData[playerShips.Length];

        for (int i = 0; i < playerShips.Length; i++)
        {
            Cell anchorCell = playerShips[i].GetNearestCell();
            placements[i] = new ShipPlacementData
            {
                x = anchorCell.Row,
                y = anchorCell.Col,
                size = playerShips[i].Size,
                horizontal = playerShips[i].IsHorizontal
            };
        }
        SubmitShipsToServerRpc(placements);
    }

    private void HandleOnShipsListChanged(NetworkListEvent<ShipPlacementData> changeEvent)
    {
        if (!IsOwner)
        {
            foreach (var ship in Ships)
            {
                Debug.LogError($"Received Ship at ({ship.x},{ship.y}) Size:{ship.size} Horizontal:{ship.horizontal}");
            }
        }
    }

    public void AttackCell(int row, int col)
    {
        if (hasAttacked)
        {
            return;
        }

        hasAttacked = true;

        AttackServerRpc(row, col);
    }

    public void UpdateOwnGrid(bool isAttackMode)
    {
        GridManager grid = GameManager.GetManager<GridManager>();
        Cell[] cells = grid.GetAllCells();

        foreach (Cell cell in cells)
        {
            if (cell.GetCellType() != ECellType.Land)
            {
                cell.SetColor(cell.OriginalColor);
            }
        }

        if (isAttackMode)
        {
            Debug.LogError($"Client {OwnerClientId} UpdateOwnGrid attack");

            foreach (var entry in attackHistory)
            {
                Cell cell = grid.GetCell(entry.row, entry.col);
                cell?.SetColor(entry.color);
            }
        }
        else
        {
            Debug.LogError($"Client {OwnerClientId} UpdateOwnGrid defense");

            foreach (var entry in defenseHistory)
            {
                Cell cell = grid.GetCell(entry.row, entry.col);
                cell?.SetColor(entry.color);
            }
        }
    }

    public void SaveAttackGrid()
    {
        Debug.LogError($"Client {OwnerClientId} SaveAttackGrid");
        GridManager grid = GameManager.GetManager<GridManager>();
        Cell[] cells = grid.GetAllCells();

        attackHistory.Clear();
        foreach (Cell cell in cells)
        {
            if (cell.HitColor == Color.red || cell.HitColor == Color.white && cell.GetCellType() != ECellType.Land)
            {
                attackHistory.Add((cell.Row, cell.Col, cell.HitColor));
            }
        }
    }

    public void SaveDefenseGrid()
    {
        Debug.LogError($"Client {OwnerClientId} SaveDefenseGrid");

        GridManager grid = GameManager.GetManager<GridManager>();
        Cell[] cells = grid.GetAllCells();

        defenseHistory.Clear();
        foreach (Cell cell in cells)
        {
            if (cell.HitColor == Color.red || cell.HitColor == Color.white && cell.GetCellType() != ECellType.Land)
            {
                defenseHistory.Add((cell.Row, cell.Col, cell.HitColor));
            }
        }
    }


    [ServerRpc(RequireOwnership = false)]
    private void AttackServerRpc(int row, int col, ServerRpcParams rpcParams = default)
    {
        ulong attackerId = rpcParams.Receive.SenderClientId;
        ulong defenderId = GetOpponentClientId(attackerId);

        PlayerState attacker = NetworkManager.Singleton.ConnectedClients[attackerId].PlayerObject.GetComponent<PlayerState>();
        PlayerState defender = NetworkManager.Singleton.ConnectedClients[defenderId].PlayerObject.GetComponent<PlayerState>();

        bool isHit = false;
        ShipPlacementData? hitShip = null;
        foreach (var ship in defender.Ships)
        {
            if (ship.horizontal)
            {
                if (col == ship.y && row >= ship.x && row < ship.x + ship.size)
                {
                    isHit = true;
                    hitShip = ship;
                    break;
                }
            }
            else
            {
                if (row == ship.x && col >= ship.y && col < ship.y + ship.size)
                {
                    isHit = true;
                    hitShip = ship;
                    break;
                }
            }
        }

        defender.AttackResultClientRpc(row, col, isHit, attackerId);
        attacker.AttackResultClientRpc(row, col, isHit, attackerId);

        if (isHit && hitShip.HasValue)
        {
            defender.shipHealth[hitShip.Value]--;

            bool allDestroyed = true;
            foreach (var kvp in defender.shipHealth)
            {
                if (kvp.Value > 0)
                {
                    allDestroyed = false;
                    break;
                }
            }

            if (allDestroyed)
            {
                GameOverClientRpc(attackerId, defenderId);
            }
        }
        ulong nextPlayerId = GetOpponentClientId(attackerId);
        StartCoroutine(DelayTurnSwitch(1.5f, nextPlayerId));
    }

    private IEnumerator DelayTurnSwitch(float delay, ulong nextPlayerId)
    {
        yield return new WaitForSeconds(delay);
        gameManager.CurrentTurnPlayerId.Value = nextPlayerId;
        gameManager.UpdateTurnClientRpc(nextPlayerId);
    }

    [ClientRpc]
    private void AttackResultClientRpc(int row, int col, bool hit, ulong attackerId)
    {
        Debug.LogError($"[val] Client {attackerId} attacks");

        GridManager grid = GameManager.GetManager<GridManager>();
        Cell attackedCell = grid.GetCell(row, col);

        if (attackedCell != null)
        {
            attackedCell.SetAttackResult(hit);

            if (attackerId == OwnerClientId)
            {
                SaveAttackGrid();
            }
            else
            {
                SaveDefenseGrid();
            }
        }
    }

    private ulong GetOpponentClientId(ulong attackerId)
    {
        foreach (var kvp in NetworkManager.Singleton.ConnectedClients)
        {
            if (kvp.Key != attackerId)
            {
                return kvp.Key;
            }
        }
        Debug.LogError("No opponent found!");
        return attackerId;
    }

    public void SetAttackMode()
    {
        hasAttacked = false;
        Ship[] ships = shipSelection.GetAllShips();
        foreach (var ship in ships)
        {
            ship.HideVisual();
        }

        GridManager grid = GameManager.GetManager<GridManager>();
        foreach (var cell in grid.GetAllCells())
        {
            cell.EnableAttackMode();
        }

        Debug.Log("Switched to Attack mode");
    }

    public void SetDefenseMode()
    {
        Ship[] ships = shipSelection.GetAllShips();
        foreach (var ship in ships)
        {
            ship.ShowVisual();
        }

        Debug.Log("Switched to Defense mode");
    }

    [ClientRpc]
    private void GameOverClientRpc(ulong winnerId, ulong loserId)
    {
        UIManager ui = GameManager.GetManager<UIManager>();

        if (NetworkManager.Singleton.LocalClientId == winnerId)
        {
            ui.ShowGameOver(true);
        }
        else if (NetworkManager.Singleton.LocalClientId == loserId)
        {
            ui.ShowGameOver(false);
        }
    }
}
