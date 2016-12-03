using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class CopterControl : UnitControl {
    private float view = 75f;
    private float range = 50f;
    private float speed = 3.5f;
    private float angularSpeed = 15f;
    private float roterAngularSpeed = 720f;

    private Vector3 targetPos;
    private GameObject targetObj;
    private GameObject roter;

    private AudioSource tankMoveSound;

    public GameObject destroyEffect;

    void Start() {
        timeInterval = 0.25f;
        roter = searchChild(gameObject, "Roter");
        bulletSpawner = searchChild(gameObject, "BulletSpawner").transform;
        tankMoveSound = GetComponent<AudioSource>();
        attackTypes = new List<string>();
        attackTypes.Add("Tank");
        attackTypes.Add("Copter");
        attackTypes.Add("Antiair");
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
        if (!tankMoveSound.isPlaying) {
            tankMoveSound.Play();
        }
    }

    void lookForTarget() {
        if (targetObj != null) {
            return;
        }
        
        targetObj = getNearbyUnit(view, attackTypes);

        if (targetObj != null || targetPos != Vector3.zero) {
            return;
        }

        targetPos = getRandomTargetPos(20f, 50f);
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
            } else {
                moveTo(targetObj.transform.position);
            }
        } else {
            if (moveTo(targetPos)) {
                targetPos = Vector3.zero;
            }
        }
        CmdRoterRotate(roterAngularSpeed * Time.deltaTime);
        transform.eulerAngles = new Vector3(0f, transform.eulerAngles.y, 0f);
    }

    void fireAt(Vector3 pos) {
        float angle = calnAngle(transform.forward, pos - transform.position);
        if (Mathf.Abs(angle) <= 1f) {
            if (canFire()) {
                resetCd();
                CmdFire(pos);
            }
        } else {
            if (angle < -1f) {
                CmdRotate(-Mathf.Min(-angle, angularSpeed * Time.deltaTime));
            }
            else if (angle > 1f) {
                CmdRotate(Mathf.Min(angle, angularSpeed * Time.deltaTime));
            }
        }
    }

    bool moveTo(Vector3 pos) {
        if (pos.y < 20f) {
            pos = new Vector3(pos.x, 20f, pos.z);
        }
        float dist = Vector3.Distance(transform.position, pos);
        if (dist < 0.1f) {
            return true;
        }
        float angle = calnAngle(transform.forward, pos - transform.position);
        if (angle < -1f) {
            CmdRotate(-Mathf.Min(-angle, angularSpeed * Time.deltaTime));
        } else if (angle > 1f) {
            CmdRotate(Mathf.Min(angle, angularSpeed * Time.deltaTime));
        }

        float upSpeed = Mathf.Clamp(pos.y - transform.position.y, -speed, speed);
        CmdUp(upSpeed * Time.deltaTime);
        if (Mathf.Abs(angle) <= 90) {
            CmdMove(Mathf.Min(dist, speed * Time.deltaTime));
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
        transform.Rotate(0, angle, 0);
    }

    [Command]
    void CmdRoterRotate(float angle) {
        roter.transform.Rotate(0, angle, 0);
    }

    [Command]
    void CmdMove(float dist) {
        transform.Translate(0, 0, dist);
    }

    [Command]
    void CmdUp(float dist) {
        transform.Translate(0, dist, 0);
    }
}
