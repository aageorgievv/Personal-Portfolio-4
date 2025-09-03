using UnityEngine;

public class Ship : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int size;
    [SerializeField] private int hitpoints;

    private bool isPlaced;
    private bool isVertical;

    public void Rotate()
    {
        isVertical = !isVertical;
        transform.Rotate(0, 0, isVertical ? 90 : -90);
    }

    private void TakeHit()
    {
        hitpoints--;

        if(hitpoints <= 0)
        {
            Sink();
        }
    }

    private void Sink()
    {
        Destroy(gameObject);
    }
}
