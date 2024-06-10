using System.Data;

namespace Client.Model
{
    public class ClientModel
    {
        public string Id { get; set; }
        public string Service { get; set; }
        public string Role { get; set; }

        public ClientModel()
        {
            Id = string.Empty;
            Service = string.Empty;
            Role = string.Empty;
        }

        public ClientModel(string id, string service, string role)
        {
            Id = id;
            Service = service;
            Role = role;
        }

        public void Update(string id, string service, string role)
        {
            Id = id;
            Service = service;
            Role = role;
        }
    }
}
