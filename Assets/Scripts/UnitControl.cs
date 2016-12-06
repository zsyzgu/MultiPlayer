using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Networking;

public class UnitControl : NetworkBehaviour {
    protected int maxHealth = 100;
    [SyncVar] public int currentHealth = 100;
    [SyncVar] public int player;
    public bool destroyOnDeath;
    public GameObject bulletPrefab;
    public GameObject colorCube;
    protected Transform bulletSpawn;
    protected float timeInterval = 0f;
    private float cd = 0f;
    protected List<string> attackTypes;
    protected float minHeight;
    protected float maxHeight;
    protected Transform bulletSpawner;
    protected float bulletSpeed = 100f;
    protected float accuracy = 0.5f;

    protected void Update() {
        cd = Mathf.Max(0f, cd - Time.deltaTime);
        setColor();
    }

    private void setColor() {
        float v = (float)currentHealth / maxHealth;
        if (player == 0) {
            colorCube.GetComponent<MeshRenderer>().material.color = new Color(v, 0f, 0f, 0.5f);
        } else if (player == 1) {
            colorCube.GetComponent<MeshRenderer>().material.color = new Color(0f, 0f, v, 0.5f);
        } else {
            colorCube.GetComponent<MeshRenderer>().material.color = new Color(0f, v, 0f, 0.5f);
        }
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
                currentHealth = maxHealth;
            }
        }
    }

    public virtual void destroy() {

    }

    void onChangeHealth(int health) {

    }

    protected GameObject searchChild(GameObject father, string son) {
        foreach (Transform child in father.transform) {
            if (child.gameObject.name == son) {
                return child.gameObject;
            }
        }
        return null;
    }

    protected GameObject getNearbyUnit(float view, List<string> types) {
        GameObject[] units = GameObject.FindGameObjectsWithTag("Unit");
        float minDist = 1e9f;
        GameObject targetObj = null;
        for (int i = 0; i < units.Length; i++) {
            if (types.Contains(units[i].name) && units[i].GetComponent<UnitControl>().player != player) {
                float dist = Vector3.Distance(transform.position, units[i].transform.position);
                if (dist < view && dist < minDist) {
                    minDist = dist;
                    targetObj = units[i];
                }
            }
        }
        return targetObj;
    }

    protected Vector3 getRandomTargetPos(float minHeight = 0f, float maxHeight = 0f) {
        float maxDot = -1f;
        Vector3 targetPos = new Vector3();
        for (int i = 0; i < 5; i++) {
            TerrainInfo info = GameObject.Find("Terrain").GetComponent<TerrainInfo>();
            Vector3 mapPos = info.getPos();
            Vector3 mapSize = info.getSize();
            Vector3 pos = new Vector3(Random.Range(mapPos.x + mapSize.x * 0.2f, mapPos.x + mapSize.x * 0.8f), Random.Range(minHeight, maxHeight), Random.Range(mapPos.y + mapSize.y * 0.2f, mapPos.y + mapSize.y * 0.8f));
            Vector3 direction = (pos - transform.position).normalized;
            float dot = Vector3.Dot(transform.forward, direction);
            if (dot > maxDot) {
                maxDot = dot;
                targetPos = pos;
            }
        }
        return targetPos;
    }

    protected float calnAngle(Vector3 from, Vector3 to) {
        from.Normalize();
        to.Normalize();
        Vector2 to2D = new Vector2(to.x, to.z);
        Vector2 from2D = new Vector2(from.x, from.z);
        return Vector2.Angle(from2D, to2D) * (Vector3.Dot(Vector3.up, Vector3.Cross(from, to)) < 0 ? -1 : 1);
    }

    protected Vector3 randomVector(float radius) {
        return new Vector3(Random.Range(-radius, radius), Random.Range(-radius, radius), Random.Range(-radius, radius));
    }

    [Command]
    protected void CmdFire(Vector3 targetPos) {
        Vector3 v = (targetPos + randomVector(accuracy) - bulletSpawner.position).normalized * bulletSpeed;
        GameObject bullet = (GameObject)Instantiate(bulletPrefab, bulletSpawner.position, new Quaternion());
        bullet.transform.forward = v.normalized;
        bullet.GetComponent<Rigidbody>().velocity = v;
        NetworkServer.Spawn(bullet);
        bulletSpawner.GetComponent<AudioSource>().Play();
    }

    protected void selfDestruction() {
        if (Vector3.Dot(transform.up, Vector3.up) <= 0f || transform.position.y < -100f) {
            takeDamage(1000);
        }
    }
}
