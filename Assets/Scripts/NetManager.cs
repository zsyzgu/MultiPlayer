namespace UnityEngine.Networking {
    [AddComponentMenu("Network/NetManager")]
    [RequireComponent(typeof(NetworkManager))]
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public class NetManager : MonoBehaviour {
        //const string SERVER_IP = "192.168.1.133";
        const string SERVER_IP = "192.168.1.163";
        const float TIMEOUT = 1.0f;
        private NetworkManager manager;
        private NetworkClient client = null;
        private float waitTime = 0.0f;

        void Awake() {
            manager = GetComponent<NetworkManager>();
        }

        void Update() {
            if (waitTime > 0.0f) {
                waitTime = Mathf.Max(waitTime - Time.deltaTime, 0.0f);
            }

            if (!(client != null && client.isConnected) && !NetworkServer.active) {
                manager.networkAddress = SERVER_IP;

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