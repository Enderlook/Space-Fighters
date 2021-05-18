using Enderlook.Unity.AudioManager;

using Photon.Pun;
using Photon.Realtime;

using UnityEngine;

namespace Game.Menu
{
    public sealed class MainMenu : MonoBehaviourPunCallbacks
    {
#pragma warning disable CS0649
        [SerializeField]
        private GameObject menuPanel;

        [SerializeField]
        private AudioFile backgroundMusic;

        [SerializeField]
        private GameObject connectPanel;

        [SerializeField]
        private GameObject loadingPanel;

        [SerializeField]
        private Error error;
#pragma warning restore CS0649

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void Awake() => AudioController.PlayLoop(backgroundMusic, Vector3.zero);

        public void Connect()
        {
            menuPanel.SetActive(false);
            loadingPanel.SetActive(true);
            PhotonNetwork.ConnectUsingSettings();
        }

        public void Open() => gameObject.SetActive(true);

        public override void OnConnectedToMaster() => PhotonNetwork.JoinLobby();

        public override void OnDisconnected(DisconnectCause cause) => error.Show("Error: " + cause.ToString(), () => menuPanel.SetActive(true));

        public override void OnJoinedLobby()
        {
            loadingPanel.SetActive(false);
            connectPanel.SetActive(true);
        }

        public void Exit() => Application.Quit();
    }
}