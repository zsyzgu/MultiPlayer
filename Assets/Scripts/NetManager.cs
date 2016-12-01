namespace UnityEngine.Networking {
    [AddComponentMenu("Network/NetManager")]
    [RequireComponent(typeof(NetworkManager))]
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public class NetManager : MonoBehaviour {
        const string SERVER_IP = "";
        const float TIMEOUT = 1.0f;
        private string serverIP;
        private NetworkManager manager;
        private NetworkClient client = null;
        private float waitTime = 0.0f;

        void Awake() {
            manager = GetComponent<NetworkManager>();
            if (SERVER_IP == "") {
                serverIP = Network.player.ipAddress;
            } else {
                serverIP = SERVER_IP;
            }
        }

        void Update() {
            if (waitTime > 0.0f) {
                waitTime = Mathf.Max(waitTime - Time.deltaTime, 0.0f);
            }

            if (!(client != null && client.isConnected) && !NetworkServer.active) {
                manager.networkAddress = serverIP;

                if (client == null) {
                    client = manager.StartClient();
                    waitTime = 1.0f;
                } else {
                    if (waitTime == 0.0f) {
                        manager.StopClient();
                        manager.StartHost();
                        client = null;
                    }
                }
            }
        }
        
        bool isClient() {
            return (client != null);
        }

        static public bool isPlayer0() {
            return !GameObject.Find("Network Manager").GetComponent<NetManager>().isClient();
        }
    }
};