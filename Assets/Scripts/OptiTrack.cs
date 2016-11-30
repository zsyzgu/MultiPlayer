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
    private int frameId = 0;
    private OptiFrame currFrame = null;

    private void connect() {
        Thread thread = new Thread(receiveThread);
        thread.Start();
    }

    private void receiveThread() {
        try {
            socket = new TcpClient();
            socket.Connect(SERVER_IP, SERVER_PORT);
        }
        catch (Exception e) {
            Debug.Log(e);
        }

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
                    frameId++;
                    frame = new OptiFrame();
                    break;
                case "frameend":
                    currFrame = frame;
                    break;
                case "rbposition":
                    frame.addRigidBody(new Vector3(float.Parse(args[1]), float.Parse(args[2]), -float.Parse(args[3])));
                    break;
                case "rbrotation":
                    frame.setRigidBodyRotation(new Vector4(float.Parse(args[1]), float.Parse(args[2]), float.Parse(args[3]), float.Parse(args[4])));
                    break;
                case "othermarker":
                    frame.addMarker(new Vector3(float.Parse(args[1]), float.Parse(args[2]), -float.Parse(args[3])));
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

    void OnDestory() {
        socket.Close();
    }
}
