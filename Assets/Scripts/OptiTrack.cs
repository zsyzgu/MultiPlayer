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
    private Thread thread;

    private void connect() {
        thread = new Thread(receiveThread);
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
        int rbID = 0;
        List<Vector3> posList = null;
        
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
                case "rbstart":
                    posList = new List<Vector3>();
                    rbID = int.Parse(args[1]);
                    break;
                case "rbend":
                    frame.addRb(rbID, posList);
                    posList.Clear();
                    break;
                case "rbposition":
                    posList.Add(0.5f * new Vector3(-float.Parse(args[1]), float.Parse(args[2]), float.Parse(args[3])));
                    break;
                case "othermarker":
                    frame.addMarker(0.5f * new Vector3(-float.Parse(args[1]), float.Parse(args[2]), float.Parse(args[3])));
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

    public Vector3 getRbPos(int id) {
        Vector3 pos = Vector3.zero;
        if (currFrame != null) {
            pos = currFrame.getPos(id);
        }
        return pos;
    }

    public Vector3 getRbDir(int id) {
        Vector3 pos = Vector3.zero;
        if (currFrame != null) {
            pos = currFrame.getDir(id);
        }
        return pos;
    }

    public List<Vector3> getMarkers() {
        if (currFrame != null) {
            return currFrame.getMarkers();
        }
        return null;
    }

    void OnApplicationQuit() {
        socket.Close();
        thread.Abort();
    }
}
