using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class OptiFrame {
    private List<int> idList;
    private List<Vector3> posList;
    private List<Vector3> dirList;
    private List<Vector3> markers;

    public OptiFrame() {
        idList = new List<int>();
        posList = new List<Vector3>();
        dirList = new List<Vector3>();
        markers = new List<Vector3>();
    }

    public int count() {
        return idList.Count;
    }

    public void addMarker(Vector3 marker) {
        markers.Add(marker);
    }

    public void addRb(int rbID, Vector3 pos, Vector3 dir) {
        idList.Add(rbID);
        posList.Add(pos);
        dirList.Add(dir);
    }

    public void addRb(int rbID, List<Vector3> posList) {
        if (rbID == 1 || rbID == 2 && posList.Count == 3) {
            Vector3 dir = Vector3.Cross(posList[1] - posList[0], posList[2] - posList[0]).normalized;
            Vector3 pos = posList[0] - dir * 0.15f;
            addRb(rbID, pos, dir);
        } else {

        }
    }

    public Vector3 getPos(int id) {
        for (int i = 0; i < idList.Count; i++) {
            if (idList[i] == id) {
                return posList[i];
            }
        }
        return Vector3.zero;
    }

    public Vector3 getDir(int id) {
        for (int i = 0; i < idList.Count; i++) {
            if (idList[i] == id) {
                return dirList[i];
            }
        }
        return Vector3.zero;
    }

    public List<Vector3> getMarkers() {
        return markers;
    }
}
