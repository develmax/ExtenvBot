using ExtenvBot.DataAccesses;

namespace ExtenvBot.Storages
{
    public class Settings : ISettings
    {
        public Settings(ISettingsDataAccess storage)
        {
            _storage = storage;
        }

        private ISettingsDataAccess _storage;

        public long? AdminId
        {
            get
            {
                var adminId = _storage.ReadSetting(nameof(AdminId));
                if (adminId != null && long.TryParse(adminId, out var value))
                    return value;

                return null;
            }
            set => _storage.WriteSetting(nameof(AdminId), value.HasValue ? value.ToString() : null);
        }

        public string AdminName
        {
            get => _storage.ReadSetting(nameof(AdminName));
            set => _storage.WriteSetting(nameof(AdminName), value);
        }
    }
}