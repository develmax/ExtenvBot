using ExtenvBot.Storages.Entities;

namespace ExtenvBot.DataAccesses
{
    public interface ISettingsDataAccess
    {
        string ReadSetting(string name);
        void WriteSetting(string name, string value);
        SettingEntity[] GetSettings();
    }
}