using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class Spawner : NetworkBehaviour {
    public GameObject tankPrefab;
    public GameObject copterPrefab;
    public GameObject antiairPrefab;
    public GameObject jeepPrefab;

    public GameObject bornEffect;

    public override void OnStartServer() {
        spawn();
    }

    void Update() {
        GameObject[] units = GameObject.FindGameObjectsWithTag("Unit");
        bool player0Jeep = false;
        bool player1Jeep = false;
        for (int i = 0; i < units.Length; i++) {
            if (units[i].name == "Jeep") {
                if (units[i].GetComponent<UnitControl>().player == 0) {
                    player0Jeep = true;
                }
                if (units[i].GetComponent<UnitControl>().player == 1) {
                    player1Jeep = true;
                }
            }
        }
        if (units.Length == 0) {
            StartCoroutine(waitAndSpawn(3.0f));
        } else if (player0Jeep == false || player1Jeep == false) {
            StartCoroutine(waitAndDestroy(3.0f));
        }
    }

    IEnumerator waitAndSpawn(float waitTime) {
        yield return new WaitForSeconds(waitTime);
        GameObject[] units = GameObject.FindGameObjectsWithTag("Unit");
        if (units.Length == 0) {
            spawn();
        }
    }

    IEnumerator waitAndDestroy(float waitTime) {
        yield return new WaitForSeconds(waitTime);
        GameObject[] units = GameObject.FindGameObjectsWithTag("Unit");
        for (int i = 0; i < units.Length; i++) {
            units[i].GetComponent<UnitControl>().takeDamage(1000);
        }
    }

    void spawn() {
        TerrainInfo info = GameObject.Find("Terrain").GetComponent<TerrainInfo>();
        Vector2 mapPos = info.getPos();
        Vector2 mapSize = info.getSize();

        int n = 10;
        for (int i = 0; i < n; i++) {
            float z = 0f, rot = 0f;
            int player = i % 2;
            if (player == 0) {
                z = mapPos.y + mapSize.y * 0.1f;
                rot = 0f;
            } else {
                z = mapPos.y + mapSize.y * 0.9f;
                rot = 180f;
            }
            Vector3 spawnPosition = new Vector3(mapPos.x + mapSize.x * (0.25f + 0.5f * i / n), 0f, z);
            Quaternion spawnRotation = Quaternion.Euler(0.0f, rot, 0.0f);
            
            if (i < 2) {
                spawnJeep(spawnPosition, spawnRotation, player);
            } else if (i < 6) {
                spawnTank(spawnPosition, spawnRotation, player);
            } else if (i < 8) {
                spawnCopter(spawnPosition, spawnRotation, player);
            } else {
                spawnAntiair(spawnPosition, spawnRotation, player);
            }
        }
    }

    public void spawnTank(Vector3 pos, Quaternion rot, int player) {
        GameObject tank = (GameObject)Instantiate(tankPrefab, pos, rot);
        tank.name = "Tank";
        tank.GetComponent<TankControl>().player = player;
        NetworkServer.Spawn(tank);
        NetworkServer.Spawn((GameObject)Instantiate(bornEffect, pos, rot));
    }

    public void spawnCopter(Vector3 pos, Quaternion rot, int player) {
        GameObject copter = (GameObject)Instantiate(copterPrefab, pos, rot);
        copter.name = "Copter";
        copter.GetComponent<CopterControl>().player = player;
        NetworkServer.Spawn(copter);
        NetworkServer.Spawn((GameObject)Instantiate(bornEffect, pos, rot));
    }

    public void spawnAntiair(Vector3 pos, Quaternion rot, int player) {
        GameObject antiair = (GameObject)Instantiate(antiairPrefab, pos, rot);
        antiair.name = "Antiair";
        antiair.GetComponent<AntiairControl>().player = player;
        NetworkServer.Spawn(antiair);
        NetworkServer.Spawn((GameObject)Instantiate(bornEffect, pos, rot));
    }

    public void spawnJeep(Vector3 pos, Quaternion rot, int player) {
        GameObject jeep = (GameObject)Instantiate(jeepPrefab, pos, rot);
        jeep.name = "Jeep";
        jeep.GetComponent<JeepControl>().player = player;
        NetworkServer.Spawn(jeep);
        NetworkServer.Spawn((GameObject)Instantiate(bornEffect, pos, rot));
    }
}
