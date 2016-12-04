using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class OtherMarker : NetworkBehaviour {
    public GameObject markerPrefab;
    private List<GameObject> objs;

	void Start () {
        objs = new List<GameObject>();
	}
	
	void Update () {
        if (isServer == false) {
            return;
        }

        CmdTrans();
	}

    [Command]
    void CmdTrans() {
        OptiTrack optiTrack = GetComponent<OptiTrack>();
        if (optiTrack != null) {
            List<Vector3> markers = optiTrack.getMarkers();
            if (markers == null) {
                markers = new List<Vector3>();
            }
            for (int i = 0; i < markers.Count; i++) {
                if (i >= objs.Count) {
                    objs.Add((GameObject)Instantiate(markerPrefab));
                    NetworkServer.Spawn(objs[i]);
                }
                Vector3 targetPos = markers[i] * 100f;
                if (Vector3.Distance(objs[i].transform.position, targetPos) <= 1f) {
                    objs[i].transform.position = (objs[i].transform.position + targetPos) / 2f;
                } else {
                    objs[i].transform.position = targetPos;
                }
            }
            while (objs.Count > markers.Count) {
                Destroy(objs[objs.Count - 1]);
                objs.Remove(objs[objs.Count - 1]);
            }
        }
    }
}
