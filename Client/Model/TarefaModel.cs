namespace Client.Model
{
    public class TarefaModel
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public string State { get; set; }
        public string ClienteId { get; set; }

        public TarefaModel()
        {
            Id = string.Empty;
            Description = string.Empty;
            State = string.Empty;
            ClienteId = string.Empty;
        }

        public TarefaModel(string id, string desc, string state, string clientId)
        {
            Id = id;
            Description = desc;
            State = state;
            ClienteId = clientId;
        }

        public void UpdateTask(string newId, string newDesc, string newState, string newClient)
        {
            Id = newId;
            Description = newDesc;
            State = newState;
            ClienteId = newClient;
        }
    }
}
