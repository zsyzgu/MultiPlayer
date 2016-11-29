using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Networking;

public class UnitControl : NetworkBehaviour {
    const int MAX_HEALTH = 100;
    [SyncVar (hook = "onChangeHealth")] public int currentHealth = MAX_HEALTH;
    public bool destroyOnDeath;
    public int player;

    public void takeDamage(int amount) {
        if (isServer == false) {
            return;
        }

        currentHealth -= amount;
        if (currentHealth <= 0) {
            if (destroyOnDeath) {
                Destroy(gameObject);
            } else {
                currentHealth = MAX_HEALTH;
                RpcRespawn();
            }
        }
    }

    void onChangeHealth(int health) {

    }

    [ClientRpc]
    void RpcRespawn() {
        if (isLocalPlayer) {

        }
    }
}
