public class ClientData
{
    #region Fields

    private int _connectionID = -1;
    private string _nickname = "";

    #endregion

    #region Properties

    public int ConnectionID
    {
        get => _connectionID;
        set => _connectionID = value;
    }

    public string Nickname
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(_nickname))
            {
                return _nickname;
            }
            else
            {
                return _connectionID.ToString();
            };
        }

        set => _nickname = value;
    }

    #endregion
}