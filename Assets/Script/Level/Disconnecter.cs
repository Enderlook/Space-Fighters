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

        [SerializeField, Tooltip("Key used to surrender.")]
        private KeyCode leaveKey;
#pragma warning restore CS0649

        private bool leaving;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void Update()
        {
            if (Input.GetKeyDown(leaveKey))
                Exit();
        }

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
