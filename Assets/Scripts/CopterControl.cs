using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class CopterControl : UnitControl {
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
    private Transform bulletSpawner;

    private AudioSource tankMoveSound;
    private AudioSource batteryRotateSound;

    private bool rotated = false;
    private bool moved = false;

    public GameObject destroyEffect;

    void Start() {
        TerrainInfo info = GameObject.Find("Terrain").GetComponent<TerrainInfo>();
        mapPos = info.getPos();
        mapSize = info.getSize();
        view = 50f;
        range = 40f;
        speed = 2f;
        angularSpeed = 15f;
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
                bulletSpawner = child;
                break;
            }
        }
        tankMoveSound = GetComponent<AudioSource>();
        batteryRotateSound = battery.GetComponent<AudioSource>();
    }

    new void Update() {
        base.Update();

        if (isServer == false) {
            return;
        }

        lookForTarget();
        act();
        playSound();
        selfDestruction();
    }

    void selfDestruction() {
        if (Vector3.Dot(transform.up, Vector3.up) <= 0f || transform.position.y < -100f) {
            takeDamage(100);
        }
    }

    void playSound() {
        if (moved) {
            if (!tankMoveSound.isPlaying) {
                tankMoveSound.Play();
            }
        }
        else {
            if (tankMoveSound.isPlaying) {
                tankMoveSound.Stop();
            }
        }
        if (rotated) {
            if (!batteryRotateSound.isPlaying) {
                batteryRotateSound.Play();
            }
        }
        else {
            if (batteryRotateSound.isPlaying) {
                batteryRotateSound.Stop();
            }
        }
        moved = false;
        rotated = false;
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
            float dist = Vector3.Distance(bulletSpawner.position, targetObj.transform.position);
            if (dist < range) {
                Vector3 targetPos = targetObj.transform.position;
                BoxCollider collider = targetObj.GetComponent<BoxCollider>();
                if (collider != null) {
                    targetPos += new Vector3(0f, collider.center.y, 0f);
                }
                fireAt(targetPos);
            }
            else {
                moveTo(targetObj.transform.position);
            }
        }
        else {
            if (moveTo(targetPos)) {
                targetPos = Vector3.zero;
            }
        }
    }

    void fireAt(Vector3 pos) {
        float angle = calnAngle(battery.transform.forward, pos - transform.position);
        if (Mathf.Abs(angle) <= 1f) {
            if (canFire()) {
                resetCd();
                CmdFire(pos);
            }
        }
        else {
            if (angle < -1f) {
                CmdBatteryRotate(-Mathf.Min(-angle, batteryAngularSpeed * Time.deltaTime));
            }
            else if (angle > 1f) {
                CmdBatteryRotate(Mathf.Min(angle, batteryAngularSpeed * Time.deltaTime));
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
        if (angle < -1f) {
            CmdRoate(-Mathf.Min(-angle, angularSpeed * Time.deltaTime));
        }
        else if (angle > 1f) {
            CmdRoate(Mathf.Min(angle, angularSpeed * Time.deltaTime));
        }
        if (Mathf.Abs(angle) <= 90) {
            CmdMove(Mathf.Min(dist, speed * Time.deltaTime));
            angle = calnAngle(battery.transform.forward, transform.forward);
            if (angle < -1f) {
                CmdBatteryRotate(-Mathf.Min(-angle, batteryAngularSpeed * Time.deltaTime));
            }
            else if (angle > 1f) {
                CmdBatteryRotate(Mathf.Min(angle, batteryAngularSpeed * Time.deltaTime));
            }
        }
        return false;
    }

    public override void destroy() {
        CmdDestroy();
    }

    [Command]
    void CmdDestroy() {
        GameObject effect = (GameObject)Instantiate(destroyEffect, transform.position, new Quaternion());
        NetworkServer.Spawn(effect);
    }

    [Command]
    void CmdFire(Vector3 targetPos) {
        GameObject bullet = (GameObject)Instantiate(bulletPrefab, bulletSpawner.position, bulletSpawner.rotation);
        bullet.GetComponent<Rigidbody>().velocity = (targetPos - bulletSpawner.position).normalized * bulletSpeed;
        NetworkServer.Spawn(bullet);
        bulletSpawner.GetComponent<AudioSource>().Play();
    }

    [Command]
    void CmdRoate(float angle) {
        moved = true;
        transform.Rotate(0, angle, 0);
    }

    [Command]
    void CmdBatteryRotate(float angle) {
        rotated = true;
        battery.transform.Rotate(0, angle, 0);
    }

    [Command]
    void CmdMove(float dist) {
        moved = true;
        transform.Translate(0, 0, dist);
    }
}
