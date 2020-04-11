namespace GameServer.Network
{
    public class DataReceivedEventArgs<T>
    {
        public DataReceivedEventArgs(T data)
        {
            Data = data;
        }

        public T Data { get; }
    }
}
