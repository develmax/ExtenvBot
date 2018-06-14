using ExtenvBot.Storages.Entities;

namespace ExtenvBot.DataAccesses
{
    public class ExternalCommandDataAccess: IExternalCommandDataAccess
    {
        public void AddExternalCommand(string id, string chatId, string command, string request)
        {
            throw new System.NotImplementedException();
        }

        public ExternalCommandEntity GetNextExternalCommand()
        {
            throw new System.NotImplementedException();
        }

        public void SetRequestReceivedExternalCommand(string id)
        {
            throw new System.NotImplementedException();
        }

        public void SetResponseExternalCommand(string id, string response)
        {
            throw new System.NotImplementedException();
        }

        public void ResponseReceivedExternalCommand(string id)
        {
            throw new System.NotImplementedException();
        }

        public void SetProcessedExternalCommand(string id)
        {
            throw new System.NotImplementedException();
        }
    }
}