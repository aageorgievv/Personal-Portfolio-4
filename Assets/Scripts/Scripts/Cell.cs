using UnityEngine;

public class Cell : MonoBehaviour
{
    public Color HitColor { get; private set; } = Color.blue;
    public ulong OwnerId { get; set; }
    public int Row {  get; private set; }
    public int Col { get; private set; }

    public bool IsAttackable => isAttackable;

    [SerializeField] private ECellType cellType;

    private bool isAttackable = false;

    public ECellType GetCellType()
    {
        return cellType;
    }

    public void SetCell(int rol, int col)
    {
        Row = rol;
        Col = col;
    }

    public void EnableAttackMode()
    {
        isAttackable = true;
    }

    public void SetColor(Color color)
    {
        GetComponent<Renderer>().material.color = color;
        HitColor = color;
    }

    public void SetAttackResult(bool hit)
    {
        Color color = hit ? Color.red : Color.white;
        SetColor(color);
    }
}

// State 1 - Attack
// - Hide my ships
// - Cells show where I have attacked
// - Never show oponent ships (until they are sunk)

// State 2 - Defense
// - Show My Own ships
// - Cells show where opponent has attacked