namespace Common.Contract
{
    public interface IRemoteRecord
    {
        bool IsRemoteRecord { get; set; }
        bool AddRecord();
        bool StopRecord();
    }
}
