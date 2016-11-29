using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class TankControl : UnitControl {
    private float view;
    private float range;
    private float speed;
    private float angularSpeed;
    private float batteryAngularSpeed;
    private float bulletSpeed;

    private Vector2 mapPos;
    private Vector2 mapSize;

    private Vector3 targetPos;
    private GameObject targetObj;
    private GameObject battery;

	void Start () {
        TerrainInfo info = GameObject.Find("Terrain").GetComponent<TerrainInfo>();
        mapPos = info.getPos();
        mapSize = info.getSize();
        float mapScale = mapSize.magnitude;
        view = mapScale * 0.3f;
        range = mapScale * 0.2f;
        speed = mapScale * 0.01f;
        angularSpeed = 20f;
        batteryAngularSpeed = 30f;
        timeInterval = 4f;
        bulletSpeed = 100f;
        foreach (Transform child in transform) {
            if (child.gameObject.name == "Battery") {
                battery = child.gameObject;
                break;
            }
        }
        foreach (Transform child in battery.transform) {
            if (child.gameObject.name == "BulletSpawner") {
                bulletSpawn = child;
                break;
            }
        }
    }
	
	new void Update () {
        base.Update();

        if (isServer == false) {
            return;
        }

        lookForTarget();
        act();
	}

    void lookForTarget() {
        if (targetObj != null) {
            return;
        }

        GameObject[] units = GameObject.FindGameObjectsWithTag("Unit");
        float minDist = 1e9f;
        for (int i = 0; i < units.Length; i++) {
            if (units[i].GetComponent<TankControl>() != null && units[i].GetComponent<TankControl>().player != player) {
                float dist = Vector3.Distance(transform.position, units[i].transform.position);
                if (dist < view && dist < minDist) {
                    minDist = dist;
                    targetObj = units[i];
                }
            }
        }

        if (targetObj != null || targetPos != Vector3.zero) {
            return;
        }

        float maxDot = -1f;
        for (int i = 0; i < 10; i++) {
            Vector3 pos = new Vector3(Random.Range(mapPos.x + mapSize.x * 0.2f, mapPos.x + mapSize.x * 0.8f), 0.0f, Random.Range(mapPos.y + mapSize.y * 0.2f, mapPos.y + mapSize.y * 0.8f));
            Vector3 direction = (pos - transform.position).normalized;
            float dot = Vector3.Dot(transform.forward, direction);
            if (dot > maxDot) {
                maxDot = dot;
                targetPos = pos;
            }
        }
    }

    void act() {
        if (targetObj != null) {
            float dist = Vector3.Distance(transform.position, targetObj.transform.position);
            if (dist < range) {
                fireAt(targetObj.transform.position);
            } else {
                moveTo(targetObj.transform.position);
            }
        } else {
            if (moveTo(targetPos)) {
                targetPos = Vector3.zero;
            }
        }
    }

    void fireAt(Vector3 pos) {
        float angle = calnAngle(battery.transform.forward, pos - transform.position);
        if (Mathf.Abs(angle) <= 0.1f) {
            if (canFire()) {
                resetCd();
                CmdFire();
            }
        } else {
            if (angle < 0) {
                CmdBatteryRotate(-Mathf.Min(-angle, angularSpeed * Time.deltaTime));
            } else if (angle > 0) {
                CmdBatteryRotate(Mathf.Min(angle, angularSpeed * Time.deltaTime));
            }
        }
    }

    float calnAngle(Vector3 from, Vector3 to) {
        from.Normalize();
        to.Normalize();
        Vector2 to2D = new Vector2(to.x, to.z);
        Vector2 from2D = new Vector2(from.x, from.z);
        return Vector2.Angle(from2D, to2D) * (Vector3.Dot(Vector3.up, Vector3.Cross(from, to)) < 0 ? -1 : 1);
    }

    bool moveTo(Vector3 pos) {
        float dist = Vector3.Distance(transform.position, pos);
        if (dist < 0.1f) {
            return true;
        }
        float angle = calnAngle(transform.forward, pos - transform.position);
        if (angle < 0) {
            CmdRoate(-Mathf.Min(-angle, angularSpeed * Time.deltaTime));
        } else if (angle > 0) {
            CmdRoate(Mathf.Min(angle, angularSpeed * Time.deltaTime));
        }
        if (Mathf.Abs(angle) <= 90) {
            CmdMove(Mathf.Min(dist, speed * Time.deltaTime));
            angle = calnAngle(battery.transform.forward, transform.forward);
            if (angle < 0) {
                CmdBatteryRotate(-Mathf.Min(-angle, angularSpeed * Time.deltaTime));
            } else if (angle > 0) {
                CmdBatteryRotate(Mathf.Min(angle, angularSpeed * Time.deltaTime));
            }
        }
        return false;
    }

    [Command]
    void CmdFire() {
        GameObject bullet = (GameObject)Instantiate(bulletPrefab, bulletSpawn.position, bulletSpawn.rotation);
        bullet.GetComponent<Rigidbody>().velocity = bullet.transform.forward * bulletSpeed;
        NetworkServer.Spawn(bullet);
    }

    [Command]
    void CmdRoate(float angle) {
        transform.Rotate(0, angle, 0);
    }

    [Command]
    void CmdBatteryRotate(float angle) {
        battery.transform.Rotate(0, angle, 0);
    }

    [Command]
    void CmdMove(float dist) {
        transform.Translate(0, 0, dist);
    }
}
