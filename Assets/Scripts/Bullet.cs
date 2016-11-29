using UnityEngine;
using System.Collections;

public class Bullet : MonoBehaviour {
    void Start() {
        Destroy(gameObject, 5.0f);
    }

    void OnCollisionEnter(Collision collision) {
        GameObject hitObject = collision.gameObject;
        UnitControl unit = hitObject.GetComponent<UnitControl>();

        if (unit != null) {
            unit.takeDamage(10);
        }

        Destroy(gameObject);
    }
}
