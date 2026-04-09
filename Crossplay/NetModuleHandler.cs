using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Net;
using TerrariaApi.Server;

namespace Crossplay
{
    internal class NetModuleHandler
    {
        private static readonly Dictionary<ushort, (int MinLength, int NetIdOffset)> ItemNetIdPackets = new()
        {
            { 5, (11, 9) },   // PlayerSlot
            { 21, (27, 25) }, // UpdateItemDrop
        };

        internal static void OnSendNetData(SendNetDataEventArgs args)
        {
            if (args.Handled)
            {
                return;
            }

            int playerId = -1;
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                if (Netplay.Clients[i].Socket == args.socket)
                {
                    playerId = i;
                    break;
                }
            }

            if (playerId < 0)
            {
                return;
            }

            if (InvalidNetPacket(args.packet, playerId))
            {
                args.Handled = true;
            }
        }

        private static bool InvalidNetPacket(NetPacket packet, int playerId)
        {
            CrossplayPlugin plugin = CrossplayPlugin.Instance;
            if (plugin is null || playerId < 0 || playerId >= Main.maxPlayers)
            {
                return false;
            }

            int clientVersion = plugin.ClientVersions[playerId];
            if (clientVersion <= 0)
            {
                return false;
            }

            if (ItemNetIdPackets.TryGetValue(packet.Id, out var itemNetIdPacket))
            {
                if (packet.Buffer?.Data is null)
                {
                    return false;
                }

                byte[] data = packet.Buffer.Data;
                if (packet.Length < itemNetIdPacket.MinLength || data.Length < itemNetIdPacket.NetIdOffset + 2)
                {
                    return false;
                }

                short itemNetID = BitConverter.ToInt16(data, itemNetIdPacket.NetIdOffset);
                if (plugin.MaxItems.TryGetValue(clientVersion, out int maxItemId) && itemNetID > maxItemId)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
