using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenNos.Core;
using OpenNos.GameObject;
using OpenNos.GameObject.Mock;
using OpenNos.GameObject.Packets.ServerPackets;
using System;
using System.Collections.Generic;

namespace OpenNos.Test
{
    [TestClass]
    public class BasicPacketHandlerTest
    {
        #region Methods

        [TestMethod]
        public void TestWalkMove()
        {
            // login, create character, start game
            FakeNetworkClient client = HandlerTestHelper.InitializeTestEnvironment();

            WalkPacket walkPacket = new WalkPacket() { Speed = 11, XCoordinate = 89, YCoordinate = 126 };

            // send walkpacket to client
            client.ReceivePacket(walkPacket);

            string mvPacket = HandlerTestHelper.WaitForPacket(client, "mv");
            MovePacket movePacket = PacketFactory.Serialize<MovePacket>(mvPacket);

            Assert.AreEqual(walkPacket.XCoordinate, movePacket.MapX);
            Assert.AreEqual(walkPacket.YCoordinate, movePacket.MapY);
            Assert.AreEqual(walkPacket.Speed, movePacket.Speed);

            HandlerTestHelper.ShutdownTestingEnvironment();
        }

        #endregion
    }
}