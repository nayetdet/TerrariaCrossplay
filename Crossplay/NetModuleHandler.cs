using System.Runtime.CompilerServices;
using Terraria;
using Terraria.Net;

namespace Crossplay
{
    internal class NetModuleHandler
    {
        internal static void OnBroadcast(On.Terraria.Net.NetManager.orig_Broadcast_NetPacket_int orig, NetManager self, NetPacket packet, int ignoreClient)
        {
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                if (i != ignoreClient && Netplay.Clients[i].IsConnected())
                {
                    self.SendToClient(packet, i);
                }
            }
        }

        internal static void OnSendToClient(On.Terraria.Net.NetManager.orig_SendToClient orig, NetManager self, NetPacket packet, int playerId)
        {
            if (!InvalidNetPacket(packet, playerId))
            {
                orig(self, packet, playerId);
            }
        }

        private static bool InvalidNetPacket(NetPacket packet, int playerId)
        {
            int clientVersion = CrossplayPlugin.Instance.ClientVersions[playerId];
            if (clientVersion <= 0)
            {
                return false;
            }

            switch (packet.Id)
            {
                case 5:
                    {
                        var itemNetID = Unsafe.As<byte, short>(ref packet.Buffer.Data[3]); // https://unsafe.as/
                        if (CrossplayPlugin.Instance.MaxItems.TryGetValue(clientVersion, out int maxItemId) && itemNetID > maxItemId)
                        {
                            return true;
                        }
                    }
                    break;
            }
            return false;
        }
    }
}
