using UnityEngine;
using System.Collections;

public class Shell : MonoBehaviour {
    public GameObject explosionEffect;
    public int damage = 50;
    public float range = 10f;

    void Start() {
        Destroy(gameObject, 10.0f);
    }

    void OnCollisionEnter(Collision collision) {
        explode(collision.gameObject);
    }

    void explode(GameObject target) {
        GameObject[] units = GameObject.FindGameObjectsWithTag("Unit");

        foreach (GameObject unit in units) {
            UnitControl unitControl = unit.GetComponent<UnitControl>();
            
            if (unitControl != null) {
                if (unit == target) {
                    unitControl.takeDamage(damage);
                } else {
                    float dist = Vector3.Distance(transform.position, unit.transform.position);
                    if (dist < range) {
                        unitControl.takeDamage((int)(damage * (1f - dist / range)));
                    }
                }
            }
        }

        Instantiate(explosionEffect, transform.position, new Quaternion());
        Destroy(gameObject);
    }
}
