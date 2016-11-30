using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Networking;

public class UnitControl : NetworkBehaviour {
    const int MAX_HEALTH = 100;
    [SyncVar (hook = "onChangeHealth")] public int currentHealth = MAX_HEALTH;
    public bool destroyOnDeath;
    public int player;
    public GameObject bulletPrefab;
    protected Transform bulletSpawn;
    protected float timeInterval = 0f;
    private float cd = 0f;

    protected void Update() {
        cd = Mathf.Max(0f, cd - Time.deltaTime);
    }

    protected bool canFire() {
        return cd <= 0;
    }

    protected void resetCd() {
        cd = timeInterval;
    }

    public void takeDamage(int amount) {
        if (isServer == false) {
            return;
        }

        currentHealth -= amount;
        if (currentHealth <= 0) {
            if (destroyOnDeath) {
                destroy();
                Destroy(gameObject);
            } else {
                currentHealth = MAX_HEALTH;
                RpcRespawn();
            }
        }
    }

    public virtual void destroy() {

    }

    void onChangeHealth(int health) {

    }

    [ClientRpc]
    void RpcRespawn() {
        if (isLocalPlayer) {

        }
    }
}
