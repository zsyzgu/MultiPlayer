using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class TankControl : UnitControl {
    private float view;
    private float range;
    private float speed;
    private float angularSpeed;
    private int attack;

    private Vector2 mapPos;
    private Vector2 mapSize;

    private Vector3 targetPos;
    private GameObject targetObj;

	void Start () {
        TerrainInfo info = GameObject.Find("Terrain").GetComponent<TerrainInfo>();
        mapPos = info.getPos();
        mapSize = info.getSize();
        float mapScale = mapSize.magnitude;
        view = mapScale * 0.5f;
        range = mapScale * 0.3f;
        speed = mapScale * 0.01f;
        angularSpeed = 45f;
        attack = 80;
	}
	
	void Update () {
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
            if (units[i].GetComponent<TankControl>() != null && units[i] != gameObject) {
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
            Vector3 pos = new Vector3(Random.Range(mapPos.x + mapSize.x * 0.1f, mapPos.x + mapSize.x * 0.9f), 0.0f, Random.Range(mapPos.y + mapSize.y * 0.1f, mapPos.y + mapSize.y * 0.9f));
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
                fire(targetObj.transform.position);
            } else {
                moveTo(targetObj.transform.position);
            }
        } else {
            if (moveTo(targetPos)) {
                targetPos = Vector3.zero;
            }
        }
    }

    void fire(Vector3 pos) {
        moveTo(pos);
    }

    bool moveTo(Vector3 pos) {
        float dist = Vector3.Distance(transform.position, pos);
        if (dist < 1f) {
            return true;
        }
        Vector3 direction = (pos - transform.position).normalized;
        Vector2 direction2D = new Vector2(direction.x, direction.z);
        Vector2 forward2D = new Vector2(transform.forward.x, transform.forward.z);
        float angle = Vector2.Angle(forward2D, direction) * (Vector3.Dot(Vector3.up, Vector3.Cross(transform.forward, direction)) < 0 ? -1 : 1);
        if (angle < 0) {
            CmdRoate(-Mathf.Min(-angle, angularSpeed * Time.deltaTime));
        } else if (angle > 0) {
            CmdRoate(Mathf.Min(angle, angularSpeed * Time.deltaTime));
        }
        CmdMove(Mathf.Min(dist, speed * Time.deltaTime));
        return false;
    }

    [Command]
    void CmdRoate(float angle) {
        transform.Rotate(0, angle, 0);
    }

    [Command]
    void CmdMove(float dist) {
        transform.Translate(0, 0, dist);
    }
}
