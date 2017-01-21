using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.GameObject.Event
{
    public class InstantBattleHelper
    {
        public static List<Tuple<short, short, short, long, bool>> GenerateMonsters(Map map, short vnum, short amount, bool move)
        {
            List<Tuple<short, short, short, long, bool>> SummonParameters = new List<Tuple<short, short, short, long, bool>>();
            MapCell cell;
            for (int i = 0; i < 15; i++)
            {
                cell = map.GetRandomPosition();
                SummonParameters.Add(new Tuple<short, short, short, long, bool>(vnum, cell.X, cell.Y, -1, true));
            }
            return SummonParameters;
        }


        public static List<Tuple<short, int, short, short>> GenerateDrop(Map map, short vnum, int amountofdrop, int amount)
        {
            List<Tuple<short, int, short, short>> dropParameters = new List<Tuple<short, int, short, short>>();
            MapCell cell = null;
            for (int i = 0; i < amountofdrop; i++)
            {
                cell = map.GetRandomPosition();
                dropParameters.Add(new Tuple<short, int, short, short>(vnum, amount, cell.X, cell.Y));
            }
            return dropParameters;
        }
        public static List<Tuple<short, short, short, long, bool>> GetInstantBattleMonster(Map map, short instantbattletype, int wave)
        {
            List<Tuple<short, short, short, long, bool>> SummonParameters = new List<Tuple<short, short, short, long, bool>>();

            switch (instantbattletype)
            {
                case 70:
                    switch (wave)
                    {
                        case 0:
                            SummonParameters.AddRange(GenerateMonsters(map, 402, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 253, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 237, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 216, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 205, 15, true));
                            break;
                        case 1:
                            SummonParameters.AddRange(GenerateMonsters(map, 402, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 243, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 228, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 225, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 205, 15, true));
                            break;
                        case 2:
                            SummonParameters.AddRange(GenerateMonsters(map, 255, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 254, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 251, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 174, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 172, 15, true));
                            break;
                        case 3:
                            SummonParameters.AddRange(GenerateMonsters(map, 407, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 272, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 261, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 257, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 256, 15, true));
                            break;
                        case 4:
                            SummonParameters.AddRange(GenerateMonsters(map, 748, 1, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 444, 13, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 439, 13, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 275, 13, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 274, 13, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 273, 13, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 163, 13, true));
                            break;
                    }


                    break;
            }
            return SummonParameters;
        }

        public static List<Tuple<short, int, short, short>> GetInstantBattleDrop(Map map, short instantbattletype, int wave)
        {
            List<Tuple<short, int, short, short>> dropParameters = new List<Tuple<short, int, short, short>>();
            switch (instantbattletype)
            {
                case 70:
                    switch (wave)
                    {
                        case 0:
                            dropParameters.AddRange(GenerateDrop(map, 1046, 15, 3000));
                            dropParameters.AddRange(GenerateDrop(map, 1010, 8, 3));
                            dropParameters.AddRange(GenerateDrop(map, 1246, 5, 1));
                            break;
                        case 1:
                            dropParameters.AddRange(GenerateDrop(map, 1046, 15, 4000));
                            dropParameters.AddRange(GenerateDrop(map, 1010, 15, 4));
                            dropParameters.AddRange(GenerateDrop(map, 1247, 10, 1));
                            break;
                        case 2:
                            dropParameters.AddRange(GenerateDrop(map, 1046, 15, 5000));
                            dropParameters.AddRange(GenerateDrop(map, 1010, 13, 5));
                            dropParameters.AddRange(GenerateDrop(map, 1246, 13, 1));
                            dropParameters.AddRange(GenerateDrop(map, 1247, 13, 1));
                            break;
                        case 3:
                            dropParameters.AddRange(GenerateDrop(map, 1046, 15, 7000));
                            dropParameters.AddRange(GenerateDrop(map, 1011, 13, 5));
                            dropParameters.AddRange(GenerateDrop(map, 1248, 13, 1));
                            dropParameters.AddRange(GenerateDrop(map, 1029, 5, 1));
                            break;
                    }


                    break;
            }
            return dropParameters;
        }
    }
}
