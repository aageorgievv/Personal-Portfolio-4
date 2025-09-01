using UnityEngine;
public static class ValidationUtility
{
    public static bool ValidateReference<T>(T reference, string referenceName) where T : class
    {
        if (reference == null)
        {
            Debug.LogError($"{referenceName} is null");
            return false;
        }

        return true;
    }
}
