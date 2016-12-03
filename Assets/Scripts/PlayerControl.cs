using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class PlayerControl : UnitControl {
    private GameObject eye;
	
    void Start() {
        if (NetManager.isPlayer0() ^ isLocalPlayer) {
            player = 1;
        } else {
            player = 0;
        }
        foreach (Transform child in transform) {
            if (child.gameObject.name == "Camera") {
                eye = child.gameObject;
                break;
            }
        }
        foreach (Transform child in eye.transform) {
            if (child.gameObject.name == "BulletSpawner") {
                bulletSpawn = child;
                break;
            }
        }

        if (isLocalPlayer == false) {
            return;
        }
        
        eye.GetComponent<Camera>().enabled = true;
        GetComponent<AudioListener>().enabled = true;
        if (player == 1) {
            eye.transform.eulerAngles = new Vector3(0f, 180f, 0f);
        }
    }

	new void Update() {
        if (isLocalPlayer == false) {
            return;
        }

        base.Update();

        calibrateTransform();

        if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer) {
            controlWithDesktop();
        } else if (Application.platform == RuntimePlatform.Android) {
            controlWithHead();
        }
    }

    void calibrateTransform() {
        OptiTrack track = GetComponent<OptiTrack>();
        if (track != null) {
            Vector3 targetPos = track.getPlayerPos(player);
            if (targetPos != Vector3.zero) {
                moveTo(targetPos * 100f);
                //TODO: Direction calibration.
                //transform.forward = track.getPlayerDir(player);
            }
        }
    }

    void controlWithDesktop() {
        float x = Input.GetAxis("Horizontal") * Time.deltaTime * 3.0f;
        float z = Input.GetAxis("Vertical") * Time.deltaTime * 3.0f;
        transform.Translate(x, 0, z);

        float rotationX = eye.transform.localEulerAngles.y + Input.GetAxis("Mouse X") * 5.0f;
        float rotationY = eye.transform.localEulerAngles.x - Input.GetAxis("Mouse Y") * 5.0f;
        if (rotationY > 180.0f) {
            rotationY -= 360.0f;
        }
        rotationY = Mathf.Clamp(rotationY, -80.0f, 80.0f);
        eye.transform.localEulerAngles = new Vector3(rotationY, rotationX, 0);
        
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
        bullet.GetComponent<Rigidbody>().velocity = bullet.transform.forward * 100.0f;
        NetworkServer.Spawn(bullet);
    }
    
    void moveTo(Vector3 targetPos) {
        float smoothRate = 0.5f;
        transform.position = transform.position * smoothRate + targetPos * (1 - smoothRate);
    }

    public override void OnStartLocalPlayer() {

    }
}
