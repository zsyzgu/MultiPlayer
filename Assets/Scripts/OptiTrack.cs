using UnityEngine;
using System;
using System.Collections;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Collections.Generic;

public class OptiTrack : MonoBehaviour {
    const string SERVER_IP = "192.168.1.124";
    const int SERVER_PORT = 7643;

    private TcpClient socket;
    private OptiFrame currFrame = null;

    private void connect() {
        Thread thread = new Thread(receiveThread);
        thread.Start();
    }

    private void receiveThread() {
        socket = new TcpClient();
        socket.Connect(SERVER_IP, SERVER_PORT);

        if (socket.Connected == false) {
            return;
        }

        StreamReader streamReader = new StreamReader(socket.GetStream());
        OptiFrame frame = null;
        
        while (true) {
            string line = streamReader.ReadLine();
            if (line == null) {
                break;
            }
            string[] args = line.Split(' ');
            switch (args[0]) {
                case "framestart":
                    frame = new OptiFrame();
                    break;
                case "frameend":
                    currFrame = frame;
                    break;
                case "rbposition":
                    frame.addRigidBody(0.5f * new Vector3(float.Parse(args[1]), -float.Parse(args[2]), float.Parse(args[3])));
                    break;
                case "rbrotation":
                    frame.setRigidBodyRotation(new Vector4(float.Parse(args[1]), float.Parse(args[2]), float.Parse(args[3]), float.Parse(args[4])));
                    break;
                case "othermarker":
                    frame.addMarker(0.5f * new Vector3(float.Parse(args[1]), -float.Parse(args[2]), float.Parse(args[3])));
                    break;
            }
        }
    }

	void Start () {
        connect();
	}
	
    public OptiFrame getFrame() {
        return currFrame;
    }

    public Vector3 getPlayerPos(int player) {
        Vector3 pos = Vector3.zero;
        if (currFrame == null || currFrame.countMarker() == 0) {
            return pos;
        }
        if (player == 0) {
            pos = new Vector3(0f, 0f, 1e9f);
            for (int i = 0; i < currFrame.countMarker(); i++) {
                if (currFrame.getMarker(i).z < pos.z) {
                    pos = currFrame.getMarker(i);
                }
            }
        } else if (player == 1) {
            pos = new Vector3(0f, 0f, -1e9f);
            for (int i = 0; i < currFrame.countMarker(); i++) {
                if (currFrame.getMarker(i).z > pos.z) {
                    pos = currFrame.getMarker(i);
                }
            }
        }
        return pos;
    }

    void OnDestory() {
        socket.Close();
    }
}
