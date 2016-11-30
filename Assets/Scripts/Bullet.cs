using UnityEngine;
using System.Collections;

public class Bullet : MonoBehaviour {
    private Vector3 startPos;
    private float maxDist;
    public GameObject explosionEffect;
    public int damage = 50;
    public float range = 10f;

    void Start() {
        Destroy(gameObject, 10.0f);
        maxDist = -1f;
        startPos = transform.position;
    }

    void Update() {
        if (maxDist > 0f) {
            if (Vector3.Distance(startPos, transform.position) > maxDist) {
                explode();
            }
        }
    }

    public void setExplodeDist(float maxDist) {
        this.maxDist = maxDist;
    }

    void OnCollisionEnter(Collision collision) {
        explode();
    }

    void explode() {
        GameObject[] units = GameObject.FindGameObjectsWithTag("Unit");

        foreach (GameObject unit in units) {
            UnitControl unitControl = unit.GetComponent<UnitControl>();

            if (unitControl != null) {
                float dist = Vector3.Distance(transform.position, unit.transform.position);
                if (dist < range) {
                    unitControl.takeDamage((int)(damage * (1f - dist / range)));
                }
            }
        }

        Instantiate(explosionEffect, transform.position, new Quaternion());
        Destroy(gameObject);
    }
}
