using UnityEngine;
using System.Collections;

public class TerrainInfo : MonoBehaviour {
    private Terrain terrain;

    void Start() {
        terrain = GetComponent<Terrain>();
    }

    public Vector2 getSize() {
        float sizeX = gameObject.GetComponent<Collider>().bounds.size.x;
        float scalX = transform.localScale.x;
        float sizeZ = gameObject.GetComponent<Collider>().bounds.size.z;
        float scalZ = transform.localScale.z;
        float width = sizeX * scalX;
        float height = sizeZ * scalZ;
        return new Vector2(width, height);
    }

    public Vector2 getPos() {
        Vector3 pos = terrain.GetPosition();
        return new Vector2(pos.x, pos.z);
    }
}
