namespace OpenNos.Data
{
    public interface IMapDTO
    {
        byte[] Data { get; set; }
        short MapId { get; set; }
        int Music { get; set; }
        string Name { get; set; }
    }
}