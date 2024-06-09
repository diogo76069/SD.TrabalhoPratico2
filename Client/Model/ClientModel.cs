using System.Data;

namespace Client.Model
{
    public class ClientModel
    {
        public string Id { get; set; }
        public string Service { get; set; }

        public ClientModel()
        {
            Id = string.Empty;
            Service = string.Empty;
        }

        public ClientModel(string id, string service)
        {
            Id = id;
            Service = service;
        }

        public void Update(string id, string service)
        {
            Id = id;
            Service = service;
        }
    }
}
