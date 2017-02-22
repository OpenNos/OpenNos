using NUnit.Framework;
using OpenNos.Core;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Mock;
using OpenNos.GameObject.Packets.ServerPackets;
using System.Threading;

namespace OpenNos.Test
{
    [TestFixture]
    public class BasicPacketHandlerTest
    {
        #region Methods

        [Test, MaxTime(10000)]
        public void GroupTest()
        {
            // login, create character, start game
            FakeNetworkClient clientA = HandlerTestHelper.InitializeTestEnvironment();
            FakeNetworkClient clientB = HandlerTestHelper.CreateFakeNetworkClient();

            Thread.Sleep(1000);

            // client A asks client B for group
            PJoinPacket pjoinPacketRequest = new PJoinPacket
            {
                CharacterId = clientB.Session.Character.CharacterId,
                RequestType = GroupRequestType.Invited
            };

            clientA.ReceivePacket(pjoinPacketRequest);
            HandlerTestHelper.WaitForPackets(clientA, 1);

            // client B accepts group request
            PJoinPacket pjoinPacketAccept = new PJoinPacket
            {
                CharacterId = clientA.Session.Character.CharacterId,
                RequestType = GroupRequestType.Accepted
            };

            clientB.ReceivePacket(pjoinPacketAccept);
            HandlerTestHelper.WaitForPackets(clientA, 1);

            // check if group has been created successfully
            Assert.IsNotNull(clientA.Session.Character.Group);
            Assert.IsNotNull(clientB.Session.Character.Group);
            Assert.AreEqual(2, clientA.Session.Character.Group.CharacterCount);
        }

        // [Test]
        public void InitializeTestEnvironmentTest()
        {
            // login, create character, start game
            FakeNetworkClient client = HandlerTestHelper.InitializeTestEnvironment();
            HandlerTestHelper.ShutdownTestingEnvironment();
            Assert.Pass();
        }

        [Test, MaxTime(10000), RequiresThread(ApartmentState.STA)]
        public void TestCharacterOption()
        {
            // login, create character, start game
            FakeNetworkClient client = HandlerTestHelper.InitializeTestEnvironment();

            CharacterOptionPacket optionPacket = new CharacterOptionPacket { IsActive = false, Option = CharacterOption.FamilyRequestBlocked };

            // check family request
            client.ReceivePacket(optionPacket);
            string msgPacket = HandlerTestHelper.WaitForPacket(client, "msg");
            Assert.IsTrue(client.Session.Character.FamilyRequestBlocked);

            HandlerTestHelper.ShutdownTestingEnvironment();
            Assert.Pass();
        }

        [Test, MaxTime(10000), RequiresThread(ApartmentState.STA)]
        public void TestWalkMove()
        {
            // login, create character, start game
            FakeNetworkClient client = HandlerTestHelper.InitializeTestEnvironment();

            WalkPacket walkPacket = new WalkPacket { Speed = 11, XCoordinate = 89, YCoordinate = 126 };

            // send walkpacket to client
            client.ReceivePacket(walkPacket);

            string mvPacket = HandlerTestHelper.WaitForPacket(client, "mv");
            MovePacket movePacket = PacketFactory.Deserialize<MovePacket>(mvPacket);

            Assert.AreEqual(walkPacket.XCoordinate, movePacket.MapX);
            Assert.AreEqual(walkPacket.YCoordinate, movePacket.MapY);
            Assert.AreEqual(walkPacket.Speed, movePacket.Speed);

            HandlerTestHelper.ShutdownTestingEnvironment();
            Assert.Pass();
        }

        #endregion
    }
}