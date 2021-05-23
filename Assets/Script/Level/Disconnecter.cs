using Photon.Pun;

using UnityEngine;

namespace Game.Level
{
    [DisallowMultipleComponent, RequireComponent(typeof(PhotonView))]
    public sealed class Disconnecter : MonoBehaviourPunCallbacks
    {
#pragma warning disable CS0649
        [SerializeField, Tooltip("Name of the main menu scene.")]
        private string mainMenuScene;
#pragma warning restore CS0649

        private bool leaving;

        public void Exit()
        {
            if (leaving)
                return;
            leaving = true;
            PhotonNetwork.LeaveRoom();
        }

        public override void OnLeftRoom()
        {
            base.OnLeftRoom();
            PhotonNetwork.LoadLevel(mainMenuScene);
        }
    }
}
