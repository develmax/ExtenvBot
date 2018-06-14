using ExtenvBot.DataAccesses;

namespace ExtenvBot.Storages
{
    public class DataAccess: IDataAccess
    {
        public DataAccess(
            IExternalCommandDataAccess externalCommandDataAccess,
            ISettingsDataAccess settingsDataAccess,
            ISubscribesDataAccess subscribesDataAccess)
        {
            ExternalCommandDataAccess = externalCommandDataAccess;
            SettingsDataAccess = settingsDataAccess;
            SubscribesDataAccess = subscribesDataAccess;
        }


        public ISettingsDataAccess SettingsDataAccess { get; }
        public IExternalCommandDataAccess ExternalCommandDataAccess { get; }
        public ISubscribesDataAccess SubscribesDataAccess { get; }
    }
}