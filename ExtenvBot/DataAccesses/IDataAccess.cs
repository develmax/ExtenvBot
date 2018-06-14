namespace ExtenvBot.DataAccesses
{
    public interface IDataAccess
    {
        ISettingsDataAccess SettingsDataAccess { get; }
        IExternalCommandDataAccess ExternalCommandDataAccess { get; }
        ISubscribesDataAccess SubscribesDataAccess { get; }
    }
}