using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class PlayerController : NetworkBehaviour {
    public GameObject bulletPrefab;
    public Transform bulletSpawn;

	void Start () {
	
	}
	
	void Update () {
        if (isLocalPlayer == false) {
            return;
        }

        if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer) {
            controlWithKeyboard();
        } else if (Application.platform == RuntimePlatform.Android) {
            controlWithHead();
        }
    }

    void controlWithKeyboard() {
        float x = Input.GetAxis("Horizontal") * Time.deltaTime * 150.0f;
        float z = Input.GetAxis("Vertical") * Time.deltaTime * 3.0f;

        transform.Rotate(0, x, 0);
        transform.Translate(0, 0, z);

        if (Input.GetKeyDown(KeyCode.Space)) {
            CmdFire();
        }
    }

    void controlWithHead() {
        if (Input.GetButtonDown("Fire1")) {
            CmdFire();
        }

        /*float rotationX = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * 5.0f;
        float rotationY = transform.localEulerAngles.x - Input.GetAxis("Mouse Y") * 5.0f;
        if (rotationY > 180.0f) {
            rotationY -= 360.0f;
        }
        rotationY = Mathf.Clamp(rotationY, -60.0f, 60.0f);
        transform.localEulerAngles = new Vector3(rotationY, rotationX, 0);

        float z = Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime * 200f;
        transform.Translate(0, 0, z);*/
    }

    [Command]
    void CmdFire() {
        GameObject bullet = (GameObject)Instantiate(bulletPrefab, bulletSpawn.position, bulletSpawn.rotation);
        bullet.GetComponent<Rigidbody>().velocity = bullet.transform.forward * 6.0f;
        NetworkServer.Spawn(bullet);
        Destroy(bullet, 2.0f);
    }

    public override void OnStartLocalPlayer() {
        GetComponent<Renderer>().material.color = new Color(0.0f, 0.0f, 0.0f, 0.1f);
        GetComponent<Camera>().enabled = true;
    }
}
