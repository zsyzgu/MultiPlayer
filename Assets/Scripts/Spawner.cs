using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class Spawner : NetworkBehaviour {
    public GameObject tankPrefab;

    public override void OnStartServer() {
        spawn();
    }

    void Update() {
        int tankCnt = 0;
        GameObject[] units = GameObject.FindGameObjectsWithTag("Unit");
        foreach (GameObject unit in units) {
            if (unit.GetComponent<TankControl>() != null) {
                tankCnt++;
            }
        }
        if (tankCnt == 0) {
            spawn();
        }
    }

    void spawn() {
        TerrainInfo info = GameObject.Find("Terrain").GetComponent<TerrainInfo>();
        Vector2 mapPos = info.getPos();
        Vector2 mapSize = info.getSize();

        for (int i = 0; i < 8; i++) {
            float z = 0f, rot = 0f;
            int player = i % 2;
            if (player == 0) {
                z = Random.Range(mapPos.y + mapSize.y * 0.1f, mapPos.y + mapSize.y * 0.2f);
                rot = 0f;
            } else {
                z = Random.Range(mapPos.y + mapSize.y * 0.8f, mapPos.y + mapSize.y * 0.9f);
                rot = 180f;
            }
            Vector3 spawnPosition = new Vector3(Random.Range(mapPos.x + mapSize.x * 0.3f, mapPos.x + mapSize.x * 0.7f), 1.0f, z);
            Quaternion spawnRotation = Quaternion.Euler(0.0f, rot, 0.0f);

            GameObject tank = (GameObject)Instantiate(tankPrefab, spawnPosition, spawnRotation);
            tank.GetComponent<TankControl>().player = player;
            NetworkServer.Spawn(tank);
        }
    }
}
