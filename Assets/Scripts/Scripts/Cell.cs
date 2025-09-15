using UnityEngine;

public class Cell : MonoBehaviour
{
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

    public void SetAttackResult(bool hit)
    {
        GetComponent<Renderer>().material.color = hit ? Color.red : Color.white;
    }
}
