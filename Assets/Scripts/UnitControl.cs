﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
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
    protected List<string> attackTypes;
    protected float minHeight;
    protected float maxHeight;

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
}
