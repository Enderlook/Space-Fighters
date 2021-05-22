using Photon.Pun;
using Photon.Realtime;

using System;

using UnityEngine;
using UnityEngine.UI;

namespace Game.Menu
{
    [DisallowMultipleComponent, RequireComponent(typeof(PhotonView))]
    public sealed class ConnectMenu : MonoBehaviourPunCallbacks
    {
        private const int maxPlayers = 4;
        private const int minPlayers = 2;
        private const int codeLength = 6;

#pragma warning disable CS0649
        [SerializeField]
        private GameObject connectPanel;

        [SerializeField]
        private GameObject waitingPanel;

        [SerializeField]
        private GameObject loadingPanel;

        [SerializeField]
        private InputField inputCode;

        [SerializeField]
        private Button joinRoom;

        [SerializeField]
        private Button play;

        [SerializeField]
        private Text roomCode;

        [SerializeField]
        private Text playersCount;

        [SerializeField]
        private Error error;

        [SerializeField]
        private string playScene;
#pragma warning restore CS0649

        private bool loading;
        private byte oldPlayerCount;

        public void JoinRoomValidate() => joinRoom.interactable = inputCode.text.Length == codeLength;

        public void JoinRoom()
        {
            connectPanel.SetActive(false);
            loadingPanel.SetActive(true);
            PhotonNetwork.JoinRoom(inputCode.text.ToLower());
        }

        public void RandomRoom()
        {
            connectPanel.SetActive(false);
            loadingPanel.SetActive(true);
            PhotonNetwork.JoinRandomRoom(null, maxPlayers);
        }

        public void CreateRoom()
        {
            connectPanel.SetActive(false);
            loadingPanel.SetActive(true);
            string room = Guid.NewGuid().ToString().Substring(0, codeLength).ToLower(); // Don't pass null beacuse random rooms has very large names
            PhotonNetwork.CreateRoom(room, new RoomOptions() { MaxPlayers = maxPlayers });
        }

        public override void OnJoinedRoom()
        {
            loadingPanel.SetActive(false);
            connectPanel.SetActive(false);
            waitingPanel.SetActive(true);
            roomCode.text = "Code: " + PhotonNetwork.CurrentRoom.Name;

            byte playerCount = PhotonNetwork.CurrentRoom.PlayerCount;
            playersCount.text = $"Players: {playerCount}/{maxPlayers}";
            oldPlayerCount = playerCount;

            if (photonView.IsMine)
            {
                play.gameObject.SetActive(true);
                play.interactable = playerCount >= minPlayers;
            }
            else
                play.gameObject.SetActive(false);
        }

        public override void OnJoinRandomFailed(short returnCode, string message) => CreateRoom();

        public override void OnCreateRoomFailed(short returnCode, string message)
            => error.Show($"Failed create room {returnCode} Message: {message}", () => connectPanel.SetActive(true));

        public override void OnJoinRoomFailed(short returnCode, string message)
            => error.Show($"Failed join room {returnCode} Message: {message}", () => connectPanel.SetActive(true));

        public void Back()
        {
            PhotonNetwork.LeaveRoom();
            waitingPanel.SetActive(false);
            connectPanel.SetActive(true);
        }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void Update()
        {
            if (!PhotonNetwork.InRoom || loading)
                return;

            byte playerCount = PhotonNetwork.CurrentRoom.PlayerCount;

            if (oldPlayerCount != playerCount)
            {
                oldPlayerCount = playerCount;
                playersCount.text = $"Players: {playerCount}/{maxPlayers}";

                if (photonView.IsMine)
                    play.interactable = playerCount >= minPlayers;
            }
        }

        public void Play()
        {
            loading = true;
            PhotonNetwork.CurrentRoom.IsOpen = false;
            photonView.RPC(nameof(LoadLevel), RpcTarget.All);
        }

        [PunRPC]
        private void LoadLevel() => PhotonNetwork.LoadLevel(playScene);
    }
}
