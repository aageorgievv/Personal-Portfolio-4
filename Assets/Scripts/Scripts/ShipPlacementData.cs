using Unity.Netcode;
using UnityEngine.UIElements;

[System.Serializable]
public struct ShipPlacementData : INetworkSerializable, System.IEquatable<ShipPlacementData>
{
    public int x;
    public int y;
    public int size;
    public bool horizontal;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref x);
        serializer.SerializeValue(ref y);
        serializer.SerializeValue(ref size);
        serializer.SerializeValue(ref horizontal);
    }

    public bool Equals(ShipPlacementData other)
    {
        return x == other.x &&
        y == other.y &&
               size == other.size &&
               horizontal == other.horizontal;
    }
}
