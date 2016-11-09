using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Networking;

public class Health : NetworkBehaviour {
    const int MAX_HEALTH = 100;
    [SyncVar (hook = "onChangeHealth")] public int currentHealth = MAX_HEALTH;
    public RectTransform healthBar;
    public bool destroyOnDeath;

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
        healthBar.sizeDelta = new Vector2(health * 2.0f, healthBar.sizeDelta.y);
    }

    [ClientRpc]
    void RpcRespawn() {
        if (isLocalPlayer) {
            transform.position = new Vector3(Random.Range(-8.0f, 8.0f), 0.0f, Random.Range(-8.0f, 8.0f));
            transform.rotation = Quaternion.Euler(0.0f, Random.Range(0.0f, 180.0f), 0.0f);
        }
    }
}
