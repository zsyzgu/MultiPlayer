using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class OptiFrame {
    private List<Vector3> rbList;
    private Vector4 rbRotation;
    private List<Vector3> mkList;

    public OptiFrame() {
        rbList = new List<Vector3>();
        rbRotation = new Vector4();
        mkList = new List<Vector3>();
    }

    public int countMarker() {
        return mkList.Count;
    }

    public int countRigidBody() {
        return rbList.Count;
    }

    public void setRigidBodyRotation(Vector4 rotation) {
        rbRotation = rotation;
    }

    public void addMarker(Vector3 marker) {
        mkList.Add(marker);
    }

    public void addRigidBody(Vector3 rb) {
        rbList.Add(rb);
    }

    public Vector4 getRigidRodyRotation() {
        return rbRotation;
    }

    public Vector3 getMarker(int id) {
        return mkList[id];
    }

    public Vector3 getRigidBody(int id) {
        return rbList[id];
    }
}
