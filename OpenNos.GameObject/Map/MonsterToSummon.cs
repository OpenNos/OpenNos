namespace OpenNos.GameObject
{
    public class MonsterToSummon
    {

        public MonsterToSummon(short vnum, MapCell spawnCell, long target, bool move)
        {
            VNum = vnum;
            SpawnCell = spawnCell;
            Target = target;
            isMoving = move;
        }

        public short VNum { get; set; }
        public MapCell SpawnCell { get; set; }
        public long Target { get; set; }
        public bool isMoving { get; set; }
    }
}