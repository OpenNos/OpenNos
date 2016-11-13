namespace OpenNos.Domain
{
    public enum RequestExchangeType : byte
    {
        Unknown = 0,
        Requested = 1,
        Confirmed = 3,
        Cancelled = 4,
        Declined = 5
    }
}