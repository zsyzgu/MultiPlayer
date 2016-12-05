using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class JeepControl : UnitControl {
    private Spawner spawner;

	void Start () {
        if (isServer == false) {
            return;
        }

        spawner = GameObject.Find("Spawner").GetComponent<Spawner>();
        currentHealth = 500;
        timeInterval = Random.Range(10f, 15f);
        resetCd();
    }
	
	new void Update () {
        base.Update();

        if (isServer == false) {
            return;
        }

        spawnUnit();
        calibrateTransform();
        selfDestruction();
	}

    void spawnUnit() {
        if (canFire()) {
            timeInterval = Random.Range(10f, 15f);
            resetCd();
            float ran = Random.Range(0f, 1f);
            if (ran < 0.5f) {
                spawner.spawnTank(transform.position + transform.forward * 5f, transform.rotation, player);
            } else if (ran < 0.8f) {
                spawner.spawnCopter(transform.position + transform.forward * 5f, transform.rotation, player);
            } else {
                spawner.spawnAntiair(transform.position + transform.forward * 5f, transform.rotation, player);
            }
        }
    }

    void calibrateTransform() {
        OptiTrack track = GetComponent<OptiTrack>();
        if (track != null) {
            Vector3 targetPos = track.getRbPos(player + 3);
            if (targetPos != Vector3.zero) {
                CmdMoveTo(targetPos * 100f);
                Vector3 dir = track.getRbDir(player + 3);
                float angle = calnAngle(transform.forward, dir);
                CmdRotateTo(transform.eulerAngles.y + angle);
            }
        }
    }

    [Command]
    void CmdMoveTo(Vector3 targetPos) {
        targetPos = new Vector3(targetPos.x, transform.position.y, targetPos.y);
        float smoothRate = 0.5f;
        transform.position = transform.position * smoothRate + targetPos * (1 - smoothRate);
    }

    [Command]
    void CmdRotateTo(float y) {
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, y, transform.eulerAngles.z);
    }
}
