using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.GameObject.Event
{
    public class InstantBattleHelper
    {
        public static List<Tuple<short, short, short, long, bool>> GetLodMonster(Map map, short instantbattletype, int wave)
        {
            List<Tuple<short, short, short, long, bool>> SummonParameters = new List<Tuple<short, short, short, long, bool>>();
            MapCell cell;
            switch (instantbattletype)
            {
                case 70:
                    switch (wave)
                    {
                        case 0:
                            for (int i = 0; i < 15; i++)
                            {
                                cell = map.GetRandomPosition();
                                SummonParameters.Add(new Tuple<short, short, short, long, bool>(402, cell.X, cell.Y, -1, true));
                                cell = map.GetRandomPosition();
                                SummonParameters.Add(new Tuple<short, short, short, long, bool>(253, cell.X, cell.Y, -1, true));
                                cell = map.GetRandomPosition();
                                SummonParameters.Add(new Tuple<short, short, short, long, bool>(237, cell.X, cell.Y, -1, true));
                                cell = map.GetRandomPosition();
                                SummonParameters.Add(new Tuple<short, short, short, long, bool>(216, cell.X, cell.Y, -1, true));
                                cell = map.GetRandomPosition();
                                SummonParameters.Add(new Tuple<short, short, short, long, bool>(205, cell.X, cell.Y, -1, true));
                            }

                            break;
                        case 1:
                            for (int i = 0; i < 15; i++)
                            {
                                cell = map.GetRandomPosition();
                                SummonParameters.Add(new Tuple<short, short, short, long, bool>(402, cell.X, cell.Y, -1, true));
                                cell = map.GetRandomPosition();
                                SummonParameters.Add(new Tuple<short, short, short, long, bool>(243, cell.X, cell.Y, -1, true));
                                cell = map.GetRandomPosition();
                                SummonParameters.Add(new Tuple<short, short, short, long, bool>(228, cell.X, cell.Y, -1, true));
                                cell = map.GetRandomPosition();
                                SummonParameters.Add(new Tuple<short, short, short, long, bool>(225, cell.X, cell.Y, -1, true));
                                cell = map.GetRandomPosition();
                                SummonParameters.Add(new Tuple<short, short, short, long, bool>(205, cell.X, cell.Y, -1, true));
                            }
                            break;
                        case 2:
                            for (int i = 0; i < 15; i++)
                            {
                                cell = map.GetRandomPosition();
                                SummonParameters.Add(new Tuple<short, short, short, long, bool>(255, cell.X, cell.Y, -1, true));
                                cell = map.GetRandomPosition();
                                SummonParameters.Add(new Tuple<short, short, short, long, bool>(254, cell.X, cell.Y, -1, true));
                                cell = map.GetRandomPosition();
                                SummonParameters.Add(new Tuple<short, short, short, long, bool>(251, cell.X, cell.Y, -1, true));
                                cell = map.GetRandomPosition();
                                SummonParameters.Add(new Tuple<short, short, short, long, bool>(174, cell.X, cell.Y, -1, true));
                                cell = map.GetRandomPosition();
                                SummonParameters.Add(new Tuple<short, short, short, long, bool>(172, cell.X, cell.Y, -1, true));
                            }
                            break;
                        case 3:
                            for (int i = 0; i < 15; i++)
                            {
                                cell = map.GetRandomPosition();
                                SummonParameters.Add(new Tuple<short, short, short, long, bool>(407, cell.X, cell.Y, -1, true));
                                cell = map.GetRandomPosition();
                                SummonParameters.Add(new Tuple<short, short, short, long, bool>(272, cell.X, cell.Y, -1, true));
                                cell = map.GetRandomPosition();
                                SummonParameters.Add(new Tuple<short, short, short, long, bool>(261, cell.X, cell.Y, -1, true));
                                cell = map.GetRandomPosition();
                                SummonParameters.Add(new Tuple<short, short, short, long, bool>(257, cell.X, cell.Y, -1, true));
                                cell = map.GetRandomPosition();
                                SummonParameters.Add(new Tuple<short, short, short, long, bool>(256, cell.X, cell.Y, -1, true));
                                cell = map.GetRandomPosition();
                                SummonParameters.Add(new Tuple<short, short, short, long, bool>(175, cell.X, cell.Y, -1, true));
                            }
                            break;
                        case 4:
                            cell = map.GetRandomPosition();
                            SummonParameters.Add(new Tuple<short, short, short, long, bool>(748, cell.X, cell.Y, -1, true));
                            for (int i = 0; i < 13; i++)
                            {
                                cell = map.GetRandomPosition();
                                SummonParameters.Add(new Tuple<short, short, short, long, bool>(444, cell.X, cell.Y, -1, true));
                                cell = map.GetRandomPosition();
                                SummonParameters.Add(new Tuple<short, short, short, long, bool>(439, cell.X, cell.Y, -1, true));
                                cell = map.GetRandomPosition();
                                SummonParameters.Add(new Tuple<short, short, short, long, bool>(275, cell.X, cell.Y, -1, true));
                                cell = map.GetRandomPosition();
                                SummonParameters.Add(new Tuple<short, short, short, long, bool>(274, cell.X, cell.Y, -1, true));
                                cell = map.GetRandomPosition();
                                SummonParameters.Add(new Tuple<short, short, short, long, bool>(273, cell.X, cell.Y, -1, true));
                                cell = map.GetRandomPosition();
                                SummonParameters.Add(new Tuple<short, short, short, long, bool>(163, cell.X, cell.Y, -1, true));
                            }
                            break;
                    }


                    break;
            }
            return SummonParameters;
        }

        public static List<Tuple<short, int, short, short>> GetLodDrop(Map map, short instantbattletype, int wave)
        {
            List<Tuple<short, int, short, short>> dropParameters = new List<Tuple<short, int, short, short>>();
            MapCell cell = null;
            switch (instantbattletype)
            {
                case 70:
                    switch (wave)
                    {
                        case 0:
                            for (int i = 0; i < 15; i++)
                            {
                                cell = map.GetRandomPosition();
                                dropParameters.Add(new Tuple<short, int, short, short>(1046,3000, cell.X, cell.Y));
                            }
                            for (int i = 0; i < 8; i++)
                            {
                                cell = map.GetRandomPosition();
                                dropParameters.Add(new Tuple<short, int, short, short>(1010, 3, cell.X, cell.Y));
                            }
                            for (int i = 0; i < 5; i++)
                            {
                                cell = map.GetRandomPosition();
                                dropParameters.Add(new Tuple<short, int, short, short>(1246, 1, cell.X, cell.Y));
                            }
                            break;
                        case 1:
                            for (int i = 0; i < 15; i++)
                            {
                                cell = map.GetRandomPosition();
                                dropParameters.Add(new Tuple<short, int, short, short>(1046, 4000, cell.X, cell.Y));
                            }
                            for (int i = 0; i < 10; i++)
                            {
                                cell = map.GetRandomPosition();
                                dropParameters.Add(new Tuple<short, int, short, short>(1010, 4, cell.X, cell.Y));
                            }
                            for (int i = 0; i < 10; i++)
                            {
                                cell = map.GetRandomPosition();
                                dropParameters.Add(new Tuple<short, int, short, short>(1247, 1, cell.X, cell.Y));
                            }
                            break;
                        case 2:
                            for (int i = 0; i < 15; i++)
                            {
                                cell = map.GetRandomPosition();
                                dropParameters.Add(new Tuple<short, int, short, short>(1046, 5000, cell.X, cell.Y));
                            }
                            for (int i = 0; i < 13; i++)
                            {
                                cell = map.GetRandomPosition();
                                dropParameters.Add(new Tuple<short, int, short, short>(1010, 5, cell.X, cell.Y));
                            }
                            for (int i = 0; i < 13; i++)
                            {
                                cell = map.GetRandomPosition();
                                dropParameters.Add(new Tuple<short, int, short, short>(1246, 1, cell.X, cell.Y));
                            }
                            for (int i = 0; i < 13; i++)
                            {
                                cell = map.GetRandomPosition();
                                dropParameters.Add(new Tuple<short, int, short, short>(1247, 1, cell.X, cell.Y));
                            }
                            break;
                        case 3:
                            for (int i = 0; i < 15; i++)
                            {
                                cell = map.GetRandomPosition();
                                dropParameters.Add(new Tuple<short, int, short, short>(1046, 7000, cell.X, cell.Y));
                            }
                            for (int i = 0; i < 13; i++)
                            {
                                cell = map.GetRandomPosition();
                                dropParameters.Add(new Tuple<short, int, short, short>(1011, 5, cell.X, cell.Y));
                                cell = map.GetRandomPosition();
                                dropParameters.Add(new Tuple<short, int, short, short>(1248, 1, cell.X, cell.Y)); 
                            }
                            for (int i = 0; i < 5; i++)
                            {
                                cell = map.GetRandomPosition();
                                dropParameters.Add(new Tuple<short, int, short, short>(1029, 1, cell.X, cell.Y));
                            }
                            break;
                    }


                    break;
            }
            return dropParameters;
        }
    }
}
