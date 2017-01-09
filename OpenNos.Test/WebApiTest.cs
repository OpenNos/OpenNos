using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using System.Linq;
using OpenNos.WebApi.Reference;

namespace OpenNos.Test
{
    [TestClass]
    public class WebApiTest
    {
        [TestMethod]
        public async Task TestParelellConnections()
        {
            await ServerCommunicationClient.Instance.HubProxy.Invoke("Cleanup");

            foreach (int x in Enumerable.Range(1, 50000))
            {
                await Task.Factory.StartNew(async () =>
                {
                    await ServerCommunicationClient.Instance.HubProxy.Invoke("RegisterAccountLogin", x, x);
                    bool hasRegisteredAccountLogin = await ServerCommunicationClient.Instance.HubProxy.Invoke<bool>("HasRegisteredAccountLogin", x, x);
                    Assert.IsTrue(hasRegisteredAccountLogin);
                });
            }

        }
    }
}
