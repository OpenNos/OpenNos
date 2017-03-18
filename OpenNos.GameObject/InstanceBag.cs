using System.Timers;

namespace OpenNos.GameObject
{
    public class InstanceBag
    {
        public Clock Clock { get; set; }
        public InstanceBag()
        {
            Clock = new Clock(3);
        }
    }
}
