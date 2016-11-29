using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class PlayerControl : UnitControl {
	void Start () {
        foreach (Transform child in transform) {
            if (child.gameObject.name == "BulletSpawner") {
                bulletSpawn = child;
                break;
            }
        }
        if (NetManager.isPlayer0() ^ isLocalPlayer) {
            player = 1;
        } else {
            player = 0;
        }
	}
	
	new void Update() {
        base.Update();

        if (isLocalPlayer == false) {
            return;
        }

        if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer) {
            controlWithDesktop();
        } else if (Application.platform == RuntimePlatform.Android) {
            controlWithHead();
        }

        calibrateTransform();
    }

    void calibrateTransform() {
        OptiFrame frame = GetComponent<OptiTrack>().getFrame();
        if (frame != null) {
            if (frame.countMarker() > 0) {
                float smoothRate = 0.5f;
                transform.position = transform.position * smoothRate + frame.getMarker(0) * (1 - smoothRate);
            }
        }
    }

    void controlWithDesktop() {
        float x = Input.GetAxis("Horizontal") * Time.deltaTime * 3.0f;
        float z = Input.GetAxis("Vertical") * Time.deltaTime * 3.0f;
        transform.Translate(x, 0, z);

        float rotationX = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * 5.0f;
        float rotationY = transform.localEulerAngles.x - Input.GetAxis("Mouse Y") * 5.0f;
        if (rotationY > 180.0f) {
            rotationY -= 360.0f;
        }
        rotationY = Mathf.Clamp(rotationY, -80.0f, 80.0f);
        transform.localEulerAngles = new Vector3(rotationY, rotationX, 0);
        float roatationZ = Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime * 200f;
        transform.Translate(0, 0, roatationZ);

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetButtonDown("Fire1")) {
            CmdFire();
        }
    }

    void controlWithHead() {
        if (Input.GetButtonDown("Fire1")) {
            CmdFire();
        }
    }

    [Command]
    void CmdFire() {
        GameObject bullet = (GameObject)Instantiate(bulletPrefab, bulletSpawn.position, bulletSpawn.rotation);
        bullet.GetComponent<Rigidbody>().velocity = bullet.transform.forward * 6.0f;
        NetworkServer.Spawn(bullet);
    }

    public override void OnStartLocalPlayer() {
        GetComponent<Renderer>().material.color = new Color(0.0f, 0.0f, 0.0f, 0.1f);
        GetComponent<Camera>().enabled = true;
    }
}
