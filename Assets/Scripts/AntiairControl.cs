using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class AntiairControl : UnitControl {
    private float view = 75f;
    private float range = 45f;
    private float speed = 2.5f;
    private float angularSpeed = 20f;
    private float batteryAngularSpeed = 50f;

    private Vector3 targetPos;
    private GameObject targetObj;
    private GameObject battery;

    private AudioSource tankMoveSound;
    private AudioSource batteryRotateSound;

    private bool rotated = false;
    private bool moved = false;

    public GameObject destroyEffect;

    void Start() {
        timeInterval = 1.5f;
        battery = searchChild(gameObject, "Battery");
        bulletSpawner = searchChild(battery, "BulletSpawner").transform;
        tankMoveSound = GetComponent<AudioSource>();
        batteryRotateSound = battery.GetComponent<AudioSource>();
        attackTypes = new List<string>();
        attackTypes.Add("Copter");
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

        targetObj = getNearbyUnit(view, attackTypes);

        if (targetObj != null || targetPos != Vector3.zero) {
            return;
        }

        targetPos = getRandomTargetPos();
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
        } else {
            if (angle < -1f) {
                CmdBatteryRotate(-Mathf.Min(-angle, batteryAngularSpeed * Time.deltaTime));
            }
            else if (angle > 1f) {
                CmdBatteryRotate(Mathf.Min(angle, batteryAngularSpeed * Time.deltaTime));
            }
        }
    }

    bool moveTo(Vector3 pos) {
        float dist = Vector3.Distance(transform.position, pos);
        if (dist < 0.1f) {
            return true;
        }
        float angle = calnAngle(transform.forward, pos - transform.position);
        if (angle < -1f) {
            CmdRotate(-Mathf.Min(-angle, angularSpeed * Time.deltaTime));
        }
        else if (angle > 1f) {
            CmdRotate(Mathf.Min(angle, angularSpeed * Time.deltaTime));
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
    void CmdRotate(float angle) {
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
