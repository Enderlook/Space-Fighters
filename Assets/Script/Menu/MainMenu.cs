using Enderlook.Unity.AudioManager;

using Photon.Pun;
using Photon.Realtime;

using System;

using UnityEngine;
using UnityEngine.UI;

using Random = UnityEngine.Random;

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
        private InputField playerName;

        [SerializeField]
        private Error error;
#pragma warning restore CS0649

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void Awake()
        {
            playerName.text = PhotonNetwork.LocalPlayer.NickName = GeneratePlayerName();
            AudioController.PlayLoop(backgroundMusic, Vector3.zero);
        }

        public void SetPlayerName()
        {
            string playerName = this.playerName.text;
            PhotonNetwork.LocalPlayer.NickName = string.IsNullOrEmpty(playerName) ? GeneratePlayerName() : playerName;
        }

        private static string GeneratePlayerName() => $"Player{Random.Range(0, 9999)}";

        public void Connect()
        {
            menuPanel.SetActive(false);
            if (!PhotonNetwork.IsConnected)
            {
                loadingPanel.SetActive(true);
                PhotonNetwork.ConnectUsingSettings();
            }
            else if (!PhotonNetwork.InLobby)
            {
                loadingPanel.SetActive(true);
                PhotonNetwork.JoinLobby();
            }
            else
                connectPanel.SetActive(true);
        }

        public void Open() => gameObject.SetActive(true);

        public override void OnConnectedToMaster() => PhotonNetwork.JoinLobby();

        public override void OnDisconnected(DisconnectCause cause) => error.Show("Error: " + cause.ToString(), () => menuPanel.SetActive(true));

        public override void OnJoinedLobby()
        {
            if (menuPanel.activeInHierarchy) // Prevent opening panel when returning from a match.
                return;

            loadingPanel.SetActive(false);
            connectPanel.SetActive(true);
        }

        public void Exit() => Application.Quit();
    }
}