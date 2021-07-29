using Game.Level;

using Photon.Pun;
using Photon.Realtime;

using UnityEngine;
using UnityEngine.UI;

using Random = UnityEngine.Random;

namespace Game.Menu
{
    [DisallowMultipleComponent, RequireComponent(typeof(PhotonView))]
    public sealed class ConnectMenu : MonoBehaviourPunCallbacks
    {
        private const int MaximumClients = 6;
        public const int MinimumClientsToStart = 4;
        public const int MinimumClientsToPlay = 2;
        private const int codeLength = 6;

        private const int MaximumPlayers = Server.IsFullAuth ? MaximumClients + 1 : MaximumClients;

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
            PhotonNetwork.JoinRandomRoom(null, MaximumPlayers);
        }

        public void CreateRoom()
        {
            connectPanel.SetActive(false);
            loadingPanel.SetActive(true);
            PhotonNetwork.CreateRoom(GenerateRoomName(), new RoomOptions() { MaxPlayers = MaximumPlayers });
        }

        private string GenerateRoomName()
        {
            const string text = "abcdefghijklmnopqrstuvwxyz0123456789";
            char[] chars = new char[codeLength];
            for (int i = 0; i < codeLength; i++)
                chars[i] = text[Random.Range(0, text.Length)];
            return new string(chars);
        }

        public override void OnJoinedRoom()
        {
            loadingPanel.SetActive(false);
            connectPanel.SetActive(false);
            waitingPanel.SetActive(true);
            roomCode.text = "Code: " + PhotonNetwork.CurrentRoom.Name;

            byte playerCount = Server.ClientsCount;
            playersCount.text = $"Players: {playerCount}/{MaximumClients}";
            oldPlayerCount = playerCount;

            if (playerCount > 1)
            {
                int copy = 1;
                string originalName = PhotonNetwork.LocalPlayer.NickName;
                foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerListOthers)
                {
                    if (PhotonNetwork.LocalPlayer.NickName == player.NickName)
                        PhotonNetwork.LocalPlayer.NickName = $"{originalName}{copy++}";
                }
            }

            UpdatePlayButton();
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

            byte playerCount = Server.ClientsCount;

            if (oldPlayerCount != playerCount)
            {
                oldPlayerCount = playerCount;
                playersCount.text = $"Players: {playerCount}/{MaximumClients}";

                UpdatePlayButton();
            }
        }

        private void UpdatePlayButton()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                play.gameObject.SetActive(true);
                play.interactable = Server.ClientsCount >= MinimumClientsToStart;
            }
            else
                play.gameObject.SetActive(false);
        }

        public void Play()
        {
            loading = true;
            PhotonNetwork.CurrentRoom.IsOpen = false;
            this.RPC(() => LoadLevel(), RpcTarget.All);
        }

        [PunRPC]
        private void LoadLevel() => PhotonNetwork.LoadLevel(playScene);
    }
}
