using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class Spawner : NetworkBehaviour {
    public GameObject tankPrefab;
    public int numberOfTanks;

    public override void OnStartServer() {
        for (int i = 0; i < numberOfTanks; i++) {
            Vector3 spawnPosition = new Vector3(Random.Range(-10.0f, 10.0f), 0.0f, Random.Range(-10.0f, 10.0f));
            Quaternion spawnRotation = Quaternion.Euler(0.0f, Random.Range(0.0f, 180.0f), 0.0f);

            GameObject tank = (GameObject)Instantiate(tankPrefab, spawnPosition, spawnRotation);
            NetworkServer.Spawn(tank);
        }
    }
}
