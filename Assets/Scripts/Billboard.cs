using UnityEngine;
using System.Collections;

public class Billboard : MonoBehaviour {
	void Start () {
	
	}
	
	void Update () {
        if (Camera.main != null) {
            transform.LookAt(Camera.main.transform);
        }
	}
}
