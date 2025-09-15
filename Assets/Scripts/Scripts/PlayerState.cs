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

    public NetworkList<ShipPlacementStruct> Ships { get; private set; } = new NetworkList<ShipPlacementStruct>();

    private GameManager gameManager;
    private ShipSelection shipSelection;

    private int shipsRequired = 5;

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
        if (IsServer)
        {
            Ships = new NetworkList<ShipPlacementStruct>();
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
    private void SubmitShipsToServerRpc(ShipPlacementStruct[] ships)
    {
        Ships.Clear();

        foreach (var ship in ships)
        {
            Ships.Add(ship);
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
        ShipPlacementStruct[] placements = new ShipPlacementStruct[playerShips.Length];

        for (int i = 0; i < playerShips.Length; i++)
        {
            Cell anchorCell = playerShips[i].GetNearestCell();
            placements[i] = new ShipPlacementStruct
            {
                x = anchorCell.Row,
                y = anchorCell.Col,
                size = playerShips[i].Size,
                horizontal = playerShips[i].IsHorizontal
            };
        }
        SubmitShipsToServerRpc(placements);
    }

    private void HandleOnShipsListChanged(NetworkListEvent<ShipPlacementStruct> changeEvent)
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
        AttackServerRpc(row, col);
    }

    [ServerRpc]
    private void AttackServerRpc(int row, int col, ServerRpcParams rpcParams = default)
    {
        ulong attackerId = rpcParams.Receive.SenderClientId;
        ulong defenderId = GetOpponentClientId(attackerId);

        PlayerState defender = NetworkManager.Singleton.ConnectedClients[defenderId].PlayerObject.GetComponent<PlayerState>();

        bool isHit = false;
        foreach (var ship in defender.Ships)
        {
            if (ship.horizontal)
            {
                if (col == ship.y && row >= ship.x && row < ship.x + ship.size)
                    isHit = true;
            }
            else
            {
                if (row == ship.x && col >= ship.y && col < ship.y + ship.size)
                    isHit = true;
            }
        }

        AttackResultClientRpc(row, col, isHit, new ClientRpcParams
        {
            Send = new ClientRpcSendParams { TargetClientIds = new[] { attackerId } }
        });

        AttackResultClientRpc(row, col, isHit, new ClientRpcParams
        {
            Send = new ClientRpcSendParams { TargetClientIds = new[] { defenderId } }
        });
    }

    [ClientRpc]
    private void AttackResultClientRpc(int row, int col, bool hit, ClientRpcParams clientRpcParams = default)
    {
        GridManager grid = GameManager.GetManager<GridManager>();
        Cell attackedCell = grid.GetCell(row, col);

        if (attackedCell != null)
        {
            attackedCell.SetAttackResult(hit);
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
}
