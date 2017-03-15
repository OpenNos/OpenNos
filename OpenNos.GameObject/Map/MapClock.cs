using System.Timers;

namespace OpenNos.GameObject
{
    public class MapClock
    {
        Timer timer; 
        public bool Enabled { get; set; }
        public int DeciSecondRemaining { get; set; }

        public string GetClock()
        {
            return $"evnt {(Enabled?1:-1)} 0 {(int)(DeciSecondRemaining)} 1";
        }
        public MapClock()
        {
            timer = new Timer(100);
            timer.Elapsed += this.OnTimerElapsed;
            timer.Start();
        }

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            if(Enabled && DeciSecondRemaining > 0)
            {
                DeciSecondRemaining--;
            }
        }
    }
}
