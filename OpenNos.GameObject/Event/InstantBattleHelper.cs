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
            for (int i = 0; i < amount; i++)
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

                case 1:
                    switch (wave)
                    {
                        case 0:
                            SummonParameters.AddRange(GenerateMonsters(map, 1, 16, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 58, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 105, 16, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 107, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 108, 8, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 111, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 136, 15, true));
                            break;
                        case 1:
                            SummonParameters.AddRange(GenerateMonsters(map, 194, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 114, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 99, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 39, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 2, 16, true));
                            break;
                        case 2:
                            SummonParameters.AddRange(GenerateMonsters(map, 140, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 100, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 81, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 12, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 4, 16, true));
                            break;
                        case 3:
                            SummonParameters.AddRange(GenerateMonsters(map, 115, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 112, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 110, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 14, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 5, 16, true));
                            break;
                        case 4:
                            SummonParameters.AddRange(GenerateMonsters(map, 979, 1, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 167, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 137, 10, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 22, 15, false));
                            SummonParameters.AddRange(GenerateMonsters(map, 17, 8, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 16, 16, true));
                            break;
                    }
                    break;
                case 40:
                    switch (wave)
                    {
                        case 0:
                            SummonParameters.AddRange(GenerateMonsters(map, 120, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 151, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 149, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 139, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 73, 16, true));
                            break;
                        case 1:
                            SummonParameters.AddRange(GenerateMonsters(map, 152, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 147, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 104, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 62, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 8, 16, true));
                            break;
                        case 2:
                            SummonParameters.AddRange(GenerateMonsters(map, 153, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 132, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 86, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 76, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 68, 16, true));
                            break;
                        case 3:
                            SummonParameters.AddRange(GenerateMonsters(map, 134, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 91, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 133, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 70, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 89, 16, true));
                            break;
                        case 4:
                            SummonParameters.AddRange(GenerateMonsters(map, 154, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 200, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 77, 8, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 217, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 724, 1, true));
                            break;
                    }
                    break;
                case 50:
                    switch (wave)
                    {
                        case 0:
                            SummonParameters.AddRange(GenerateMonsters(map, 134, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 91, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 89, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 77, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 71, 16, true));
                            break;
                        case 1:
                            SummonParameters.AddRange(GenerateMonsters(map, 217, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 200, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 154, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 92, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 79, 16, true));
                            break;
                        case 2:
                            SummonParameters.AddRange(GenerateMonsters(map, 235, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 226, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 214, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 204, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 201, 15, true));
                            break;
                        case 3:
                            SummonParameters.AddRange(GenerateMonsters(map, 249, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 236, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 227, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 218, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 202, 15, true));
                            break;
                        case 4:
                            SummonParameters.AddRange(GenerateMonsters(map, 583, 1, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 400, 13, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 255, 8, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 253, 13, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 251, 10, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 205, 14, true));
                            break;
                    }
                    break;
                case 60:
                    switch (wave)
                    {
                        case 0:
                            SummonParameters.AddRange(GenerateMonsters(map, 242, 12, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 234, 12, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 215, 12, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 207, 12, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 202, 13, true));
                            break;
                        case 1:
                            SummonParameters.AddRange(GenerateMonsters(map, 402, 12, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 253, 12, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 237, 12, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 216, 12, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 205, 13, true));
                            break;
                        case 2:
                            SummonParameters.AddRange(GenerateMonsters(map, 402, 12, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 243, 12, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 228, 12, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 255, 12, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 205, 13, true));
                            break;
                        case 3:
                            SummonParameters.AddRange(GenerateMonsters(map, 268, 12, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 255, 12, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 254, 12, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 174, 12, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 172, 13, true));
                            break;
                        case 4:
                            SummonParameters.AddRange(GenerateMonsters(map, 725, 1, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 407, 12, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 272, 12, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 261, 12, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 256, 12, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 275, 13, true));
                            break;
                    }
                    break;
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
                case 80:
                    switch (wave)
                    {
                        case 0:
                            SummonParameters.AddRange(GenerateMonsters(map, 1007, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 1003, 15, false));
                            SummonParameters.AddRange(GenerateMonsters(map, 1002, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 1001, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 1000, 16, true));
                            break;
                        case 1:
                            SummonParameters.AddRange(GenerateMonsters(map, 1199, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 1198, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 1197, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 1196, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 1123, 16, true));
                            break;
                        case 2:
                            SummonParameters.AddRange(GenerateMonsters(map, 1305, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 1304, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 1303, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 1302, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 1194, 16, true));
                            break;
                        case 3:
                            SummonParameters.AddRange(GenerateMonsters(map, 1902, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 1901, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 1900, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 1045, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 1043, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 1042, 16, true));
                            break;
                        case 4:
                            SummonParameters.AddRange(GenerateMonsters(map, 637, 1, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 1903, 13, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 1053, 13, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 1051, 13, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 1049, 13, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 1048, 13, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 1047, 13, true));
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
                case 1:
                    switch (wave)
                    {
                        case 0:
                            dropParameters.AddRange(GenerateDrop(map, 1046, 15, 500));
                            dropParameters.AddRange(GenerateDrop(map, 2027, 8, 5));
                            dropParameters.AddRange(GenerateDrop(map, 2018, 5, 5));
                            dropParameters.AddRange(GenerateDrop(map, 180, 5, 1));
                            break;
                        case 1:
                            dropParameters.AddRange(GenerateDrop(map, 1046, 15, 1000));
                            dropParameters.AddRange(GenerateDrop(map, 1002, 8, 3));
                            dropParameters.AddRange(GenerateDrop(map, 1005, 16, 3));
                            dropParameters.AddRange(GenerateDrop(map, 181, 5, 1));
                            break;
                        case 2:
                            dropParameters.AddRange(GenerateDrop(map, 1046, 15, 1500));
                            dropParameters.AddRange(GenerateDrop(map, 1002, 10, 5));
                            dropParameters.AddRange(GenerateDrop(map, 1005, 10, 5));
                            break;
                        case 3:
                            dropParameters.AddRange(GenerateDrop(map, 1046, 15, 2000));
                            dropParameters.AddRange(GenerateDrop(map, 1003, 10, 5));
                            dropParameters.AddRange(GenerateDrop(map, 1006, 10, 5));
                            break;
                    }
                    break;
                case 40:
                    switch (wave)
                    {
                        case 0:
                            dropParameters.AddRange(GenerateDrop(map, 1046, 15, 1500));
                            dropParameters.AddRange(GenerateDrop(map, 1008, 5, 3));
                            dropParameters.AddRange(GenerateDrop(map, 180, 5, 1));
                            break;
                        case 1:
                            dropParameters.AddRange(GenerateDrop(map, 1046, 15, 2000));
                            dropParameters.AddRange(GenerateDrop(map, 1008, 8, 3));
                            dropParameters.AddRange(GenerateDrop(map, 181, 5, 1));
                            break;
                        case 2:
                            dropParameters.AddRange(GenerateDrop(map, 1046, 15, 2500));
                            dropParameters.AddRange(GenerateDrop(map, 1009, 10, 3));
                            dropParameters.AddRange(GenerateDrop(map, 1246, 5, 1));
                            dropParameters.AddRange(GenerateDrop(map, 1247, 5, 1));
                            break;
                        case 3:
                            dropParameters.AddRange(GenerateDrop(map, 1046, 15, 3000));
                            dropParameters.AddRange(GenerateDrop(map, 1009, 10, 3));
                            dropParameters.AddRange(GenerateDrop(map, 1248, 5, 1));
                            break;
                    }
                    break;
                case 50:
                    switch (wave)
                    {
                        case 0:
                            dropParameters.AddRange(GenerateDrop(map, 1046, 15, 1500));
                            dropParameters.AddRange(GenerateDrop(map, 1008, 5, 3));
                            dropParameters.AddRange(GenerateDrop(map, 180, 5, 1));
                            break;
                        case 1:
                            dropParameters.AddRange(GenerateDrop(map, 1046, 15, 2000));
                            dropParameters.AddRange(GenerateDrop(map, 1008, 8, 3));
                            dropParameters.AddRange(GenerateDrop(map, 181, 5, 1));
                            break;
                        case 2:
                            dropParameters.AddRange(GenerateDrop(map, 1046, 15, 2500));
                            dropParameters.AddRange(GenerateDrop(map, 1009, 10, 3));
                            dropParameters.AddRange(GenerateDrop(map, 1246, 5, 1));
                            dropParameters.AddRange(GenerateDrop(map, 1247, 5, 1));
                            break;
                        case 3:
                            dropParameters.AddRange(GenerateDrop(map, 1046, 15, 3000));
                            dropParameters.AddRange(GenerateDrop(map, 1009, 10, 3));
                            dropParameters.AddRange(GenerateDrop(map, 1248, 5, 1));
                            break;
                    }
                    break;
                case 60:
                    switch (wave)
                    {
                        case 0:
                            dropParameters.AddRange(GenerateDrop(map, 1046, 15, 3000));
                            dropParameters.AddRange(GenerateDrop(map, 1010, 8, 4));
                            dropParameters.AddRange(GenerateDrop(map, 1246, 5, 1));
                            break;
                        case 1:
                            dropParameters.AddRange(GenerateDrop(map, 1046, 15, 4000));
                            dropParameters.AddRange(GenerateDrop(map, 1010, 10, 3));
                            dropParameters.AddRange(GenerateDrop(map, 1247, 5, 1));
                            break;
                        case 2:
                            dropParameters.AddRange(GenerateDrop(map, 1046, 15, 5000));
                            dropParameters.AddRange(GenerateDrop(map, 1010, 10, 13));
                            dropParameters.AddRange(GenerateDrop(map, 1246, 8, 1));
                            dropParameters.AddRange(GenerateDrop(map, 1247, 8, 1));
                            break;
                        case 3:
                            dropParameters.AddRange(GenerateDrop(map, 1046, 15, 7000));
                            dropParameters.AddRange(GenerateDrop(map, 1011, 13, 5));
                            dropParameters.AddRange(GenerateDrop(map, 1029, 5, 1));
                            dropParameters.AddRange(GenerateDrop(map, 1248, 13, 1));
                            break;
                    }
                    break;
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
                case 80:
                    switch (wave)
                    {
                        case 0:
                            dropParameters.AddRange(GenerateDrop(map, 1046, 15, 10000));
                            dropParameters.AddRange(GenerateDrop(map, 1011, 15, 5));
                            dropParameters.AddRange(GenerateDrop(map, 1246, 15, 1));
                            break;
                        case 1:
                            dropParameters.AddRange(GenerateDrop(map, 1046, 15, 12000));
                            dropParameters.AddRange(GenerateDrop(map, 1011, 15, 5));
                            dropParameters.AddRange(GenerateDrop(map, 1247, 15, 1));
                            break;
                        case 2:
                            dropParameters.AddRange(GenerateDrop(map, 1046, 15, 15000));
                            dropParameters.AddRange(GenerateDrop(map, 1011, 20, 5));
                            dropParameters.AddRange(GenerateDrop(map, 1246, 15, 1));
                            dropParameters.AddRange(GenerateDrop(map, 1247, 15, 1));
                            break;
                        case 3:
                            dropParameters.AddRange(GenerateDrop(map, 1046, 30, 20000));
                            dropParameters.AddRange(GenerateDrop(map, 1011, 30, 5));
                            dropParameters.AddRange(GenerateDrop(map, 1030, 30, 1));
                            dropParameters.AddRange(GenerateDrop(map, 2282, 12, 3));
                            break;
                    }
                    break;
            }
            return dropParameters;
        }
    }
}
