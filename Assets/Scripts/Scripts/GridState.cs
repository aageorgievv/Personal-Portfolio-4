using UnityEngine;
using Unity.Netcode;

public struct GridState : INetworkSerializable
{
    public int size;
    public byte[] cells;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref size);

        if (serializer.IsReader)
        {
            int lenght = size * size;
            cells = new byte[lenght];

            for (int i = 0; i < lenght; i++)
            {
                serializer.SerializeValue(ref cells[i]);
            }
        }
        else
        {
            for (int i = 0; i < cells.Length; i++)
            {
                serializer.SerializeValue(ref cells[i]);
            }
        }
    }
}
