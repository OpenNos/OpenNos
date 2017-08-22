using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.GameObject.Event
{
    public class IceBreaker
    {
        private List<Tuple<int, int>> _brackets = new List<Tuple<int, int>>()
        {
            new Tuple<int, int>(1, 25),
            new Tuple<int, int>(20, 40),
            new Tuple<int, int>(35, 55),
            new Tuple<int, int>(50, 70),
            new Tuple<int, int>(65, 85),
            new Tuple<int, int>(80, 99)
        };

        private int _currentBracket { get; set; }

        public void Run()
        {

        }

        class IceBreakerTask
        {

        }
    }
}
