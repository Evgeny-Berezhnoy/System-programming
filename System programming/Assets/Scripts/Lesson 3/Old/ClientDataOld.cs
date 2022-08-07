public class ClientDataOld
{
    #region Fields

    private int _port = -1;
    private int _hostID = -1;
    private int _connectionID = -1;

    #endregion

    #region Properties

    public int Port
    {
        get => _port;
        set => _port = value;
    }

    public int HostID
    {
        get => _hostID;
        set => _hostID = value;
    }

    public int ConnectionID
    {
        get => _connectionID;
        set => _connectionID = value;
    }

    #endregion
}