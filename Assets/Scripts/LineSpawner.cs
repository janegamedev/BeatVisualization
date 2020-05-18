using UnityEngine;

public abstract class LineSpawner : MonoBehaviour
{
    public static LineRenderer Initialize(GameObject prefab, Material material)
    {
        GameObject go = Instantiate(prefab);
        go.transform.position = Vector3.zero;

        LineRenderer line = go.GetComponent<LineRenderer>();
        line.sharedMaterial = material;

        line.positionCount = 2;
        line.useWorldSpace = true;
        return line;
    } 
}