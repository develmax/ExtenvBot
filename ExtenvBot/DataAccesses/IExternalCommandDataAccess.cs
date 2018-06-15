using ExtenvBot.Storages.Entities;

namespace ExtenvBot.DataAccesses
{
    public interface IExternalCommandDataAccess
    {
        void AddExternalCommand(string id, string chatId, string command, string request);
        ExternalCommandEntity GetNextExternalCommand();
        ExternalCommandEntity GetExternalCommand(string id);
        void SetRequestReceivedExternalCommand(string id);
        void SetResponseExternalCommand(string id, string response);
        void ResponseReceivedExternalCommand(string id);
        void SetProcessedExternalCommand(string id);
        ExternalCommandEntity[] GetExternalCommands();
    }
}