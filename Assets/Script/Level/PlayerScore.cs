using Enderlook.Unity.Utils;

using Game.Menu;

using Photon.Pun;

using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;

namespace Game.Level
{
    [DisallowMultipleComponent, RequireComponent(typeof(PhotonView)), DefaultExecutionOrder(1)]
    public sealed class PlayerScore : MonoBehaviourPunCallbacks
    {
#pragma warning disable CS0649
        [SerializeField, Tooltip("Holder of all players.")]
        private RectTransform playersHolder;

        [SerializeField, Tooltip("Prefab of player name in UI.")]
        private Text playerPrefab;

        [SerializeField, Tooltip("Screen shown on finalized game.")]
        private GameObject finalizePanel;

        [SerializeField, Tooltip("Name of the winner.")]
        private Text winner;

        [SerializeField, Tooltip("Scoreboard of players.")]
        private Text scoreboard;

        [SerializeField, Tooltip("Required score to win.")]
        private int winScore;
#pragma warning restore CS0649

        private Dictionary<Photon.Realtime.Player, (Color color, int kills)> players = new Dictionary<Photon.Realtime.Player, (Color color, int kills)>();

        private static PlayerScore instance;
        private static PlayerScore Instance {
            get {
                if (instance == null)
                {
                    instance = FindObjectOfType<PlayerScore>();
                    int i = 0;
                    foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerList)
                        instance.players.Add(player, (GetPlayerColor(i++), 0));
                    instance.UpdateValues();
                }
                return instance;
            }
        }

        public static bool HasFinalized { get; internal set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void Awake()
        {
            PlayerScore _ = Instance;
        }

        public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
        {
            Photon.Realtime.Player[] remainingPlayers = PhotonNetwork.PlayerList;
            if (PhotonNetwork.CurrentRoom.PlayerCount < ConnectMenu.minPlayers)
                Finalize(players
                    .Where(e => remainingPlayers.Contains(e.Key))
                    .OrderByDescending(e => e.Value.kills)
                    .ThenByDescending(e => e.Key.NickName)
                    .First().Key);
        }

        public static Color GetShipColor(Photon.Realtime.Player owner)
        {
            Color color = Instance.players[owner].color;
            color.SetSaturation(.3f);
            return color;
        }

        public static void IncreaseCounter(Photon.Realtime.Player player)
            => Instance.photonView.RPC(nameof(RPC_IncreaseCounter), RpcTarget.All, player);

        [PunRPC]
        private void RPC_IncreaseCounter(Photon.Realtime.Player player)
        {
            (Color color, int kills) info = players[player];
            int kills = info.kills + 1;
            players[player] = (info.color, kills);
            UpdateValues();

            if (kills == winScore)
                Finalize(player);
        }

        private void Finalize(Photon.Realtime.Player player)
        {
            HasFinalized = true;
            finalizePanel.SetActive(true);
            winner.text = player.NickName;
            scoreboard.text = string.Join("\n", players
                .OrderByDescending(e => e.Value.kills)
                .ThenByDescending(e => e.Key.NickName)
                .Select(e => $"{e.Key.NickName} ({e.Value.kills})"));
        }

        private void UpdateValues()
        {
            for (int i = playersHolder.childCount - 1; i >= 0; i--)
                Destroy(playersHolder.GetChild(i).gameObject);

            foreach (KeyValuePair<Photon.Realtime.Player, (Color color, int kills)> kvp in players
                .OrderByDescending(e => e.Value.kills)
                .ThenByDescending(e => e.Key.NickName))
            {
                Text text = Instantiate(playerPrefab, playersHolder);
                text.text = $"{kvp.Key.NickName} ({kvp.Value.kills})";
                text.color = kvp.Value.color;
            }
        }

        private static Color GetPlayerColor(int index)
        {
            // https://gamedev.stackexchange.com/a/46469 from https://gamedev.stackexchange.com/questions/46463/how-can-i-find-an-optimum-set-of-colors-for-10-players
            return Color.HSVToRGB(index * 0.618033988749895f % 1.0f, 5f, 1f);
        }
    }
}
