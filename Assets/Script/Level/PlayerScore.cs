using Enderlook.Enumerables;
using Enderlook.Unity.Utils;

using Game.Menu;

using Photon.Pun;

using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;

namespace Game.Level
{
    [DisallowMultipleComponent, RequireComponent(typeof(PhotonView))]
    public sealed class PlayerScore : MonoBehaviourPunCallbacks
    {
#pragma warning disable CS0649
        [SerializeField, Tooltip("Holder of all players.")]
        private RectTransform playersHolder;

        [SerializeField, Tooltip("Prefab of player name in UI.")]
        private Text playerPrefab;

        [SerializeField, Tooltip("Screen shown on finalized game.")]
        private GameObject finalizePanel;

        [SerializeField, Tooltip("Title of the winner.")]
        private Text winnerTitle;

        [SerializeField, Tooltip("Name of the winner.")]
        private Text winner;

        [SerializeField, Tooltip("Scoreboard of players.")]
        private Text scoreboard;

        [SerializeField, Tooltip("Required score to win.")]
        private int winScore;
#pragma warning restore CS0649

        private int order;
        private Dictionary<Photon.Realtime.Player, (Color color, int kills, int order)> players = new Dictionary<Photon.Realtime.Player, (Color color, int kills, int order)>();

        private static PlayerScore instance;
        private static PlayerScore Instance {
            get {
                if (instance == null)
                {
                    instance = FindObjectOfType<PlayerScore>();
                    int i = 0;
                    foreach (Photon.Realtime.Player player in Server.Clients)
                        instance.players.Add(player, (GetPlayerColor(i++), 0, 0));
                    instance.UpdateValues();
                }
                return instance;
            }
        }

        public static bool HasFinalized { get; internal set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void Awake() => Server.AfterServerIsConnected(() => { PlayerScore _ = Instance; });

        public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
        {
            Photon.Realtime.Player[] remainingPlayers = Server.Clients;
            players = players.Where(e => remainingPlayers.Contains(e.Key)).ToDictionary();
            UpdateValues();

            if (Server.ClientsCount < ConnectMenu.MinimumClients)
                Finalize(Sort()
                    .GroupBy(e => e.kills)
                    .OrderByDescending(e => e.Key)
                    .First()
                    .Select(e => e.player));
        }

        public static Color GetShipColor(Photon.Realtime.Player owner)
        {
            Color color = Instance.players[owner].color;
            color.SetSaturation(.3f);
            return color;
        }

        public static void ChangeCounter(Photon.Realtime.Player player, bool decrease)
            => Instance.RPC_FromServer(() => Instance.RPC_IncreaseCounter(player, decrease));

        [PunRPC]
        private void RPC_IncreaseCounter(Photon.Realtime.Player player, bool decrease)
        {
            (Color color, int kills, int) info = players[player];
            int kills = info.kills + (decrease ? -1 : 1);
            players[player] = (info.color, kills, ++order);
            UpdateValues();

            if (kills == winScore)
                Finalize(new Photon.Realtime.Player[] { player });
        }

        private void Finalize(IEnumerable<Photon.Realtime.Player> player)
        {
            HasFinalized = true;
            finalizePanel.SetActive(true);
            string[] winners = player.Select(e => e.NickName).ToArray();
            Debug.Assert(winners.Length != 0);
            winner.text = string.Join(", ", winners);
            winnerTitle.text = winners.Length == 1 ? "The winner is" : "The winners are:";
            scoreboard.text = string.Join("\n", Sort().Select(e => $"{e.player.NickName} ({e.kills})"));
        }

        private void UpdateValues()
        {
            for (int i = playersHolder.childCount - 1; i >= 0; i--)
                Destroy(playersHolder.GetChild(i).gameObject);

            foreach ((Photon.Realtime.Player player, Color color, int kills) in Sort())
            {
                Text text = Instantiate(playerPrefab, playersHolder);
                text.text = $"{player.NickName} ({kills})";
                text.color = color;
            }
        }

        private IEnumerable<(Photon.Realtime.Player player, Color color, int kills)> Sort()
        {
            return players
                .OrderByDescending(e => e.Value.kills)
                .ThenByDescending(e => e.Value.order)
                .ThenByDescending(e => e.Key.NickName)
                .Select(e => (e.Key, e.Value.color, e.Value.kills));
        }

        private static Color GetPlayerColor(int index)
        {
            // https://gamedev.stackexchange.com/a/46469 from https://gamedev.stackexchange.com/questions/46463/how-can-i-find-an-optimum-set-of-colors-for-10-players
            return Color.HSVToRGB(index * 0.618033988749895f % 1.0f, 5f, 1f);
        }
    }
}
