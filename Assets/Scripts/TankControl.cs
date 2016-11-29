using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class TankControl : UnitControl {
    private float view;
    private float range;
    private float speed;
    private int attack;

    private Vector2 mapPos;
    private Vector2 mapSize;

    private 

	void Start () {
        TerrainInfo info = GameObject.Find("Terrain").GetComponent<TerrainInfo>();
        mapPos = info.getPos();
        mapSize = info.getSize();
        float mapScale = mapSize.magnitude;
        view = mapScale * 0.5f;
        range = mapScale * 0.3f;
        speed = mapScale * 0.01f;
	}
	
	void Update () {
        if (isServer == false) {
            return;
        }


        CmdMove();
	}

    [Command]
    void CmdMove() {
        transform.Translate(0, 0, speed * Time.deltaTime);
    }
}
