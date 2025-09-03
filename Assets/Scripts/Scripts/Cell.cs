using UnityEngine;

public class Cell : MonoBehaviour
{
    [SerializeField] private ECellType cellType;

    public ECellType GetCellType()
    {
        return cellType;
    }
}
