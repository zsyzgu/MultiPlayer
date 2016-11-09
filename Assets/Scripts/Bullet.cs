using UnityEngine;
using System.Collections;

public class Bullet : MonoBehaviour {
    void OnCollisionEnter(Collision collision) {
        GameObject hitObject = collision.gameObject;
        Health health = hitObject.GetComponent<Health>();

        if (health != null) {
            health.takeDamage(10);
        }

        Destroy(gameObject);
    }
}
