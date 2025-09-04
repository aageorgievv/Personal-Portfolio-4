using UnityEngine;

public class Cell : MonoBehaviour
{
    public int Row {  get; private set; }
    public int Col { get; private set; }

    [SerializeField] private ECellType cellType;

    public ECellType GetCellType()
    {
        return cellType;
    }

    public void SetCell(int rol, int col)
    {
        Row = rol;
        Col = col;
    }
}
