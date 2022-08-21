using UnityEngine.Networking;

namespace Main
{
    public class SolarSystemNetworkManager : NetworkManager
    {
        #region Base methods

        public override void OnServerAddPlayer(
            NetworkConnection conn,
            short playerControllerId)
        {
            var spawnTransform = GetStartPosition();

            var player =
                Instantiate(
                    playerPrefab,
                    spawnTransform.position,
                    spawnTransform.rotation);

            NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);
        }

        #endregion
    }
}
