using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class TankControl : MonoBehaviour {
	void Start () {
	
	}
	
	void Update () {
        transform.Translate(0, 0, Time.deltaTime);
	}
}
