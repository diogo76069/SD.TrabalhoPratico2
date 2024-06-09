using Grpc.Core;

namespace Server.Services
{
    public class ServiMotoService : ServiMoto.ServiMotoBase
    {
        public ILogger<ServiMotoService> _logger { get; }

        public ServiMotoService(ILogger<ServiMotoService> logger)
        {
            _logger = logger;
        }

        // Verifica os dados de login e retorna o servico do cliente
        public override Task<ClientService> LogInClient(ClientLookup request, ServerCallContext context)
        {
            ClientService output = new ClientService();

            string workingDirectory = Environment.CurrentDirectory;
            string filePath = @$"{workingDirectory}\Data\Clientes.csv";

            var reader = new StreamReader(filePath);

            string? line;

            while ((line = reader.ReadLine()) != null)
            {
                // ClientID,Password,Servico
                string[] columns = line.Split(',');

                if (columns[0] == request.Id && columns[1] == request.Password)
                {
                    output.Servico = columns[2];
                }
            }

            // Retorna o servico encontrado
            return Task.FromResult(output);
        }

        public override Task<CurrentTask> FindCurrentTask(TaskLookup request, ServerCallContext context)
        {
            CurrentTask output = new CurrentTask();

            string servico = request.Servico;
            string workingDirectory = Environment.CurrentDirectory;
            string filePath = @$"{workingDirectory}\Data\{servico}.csv";

            var reader = new StreamReader(filePath);

            string? line;

            while ((line = reader.ReadLine()) != null)
            {
                // TarefaId,Descricao,Estado,ClienteId
                string[] columns = line.Split(',');

                if (columns[3] == request.Id) // Verificar todas as linhas com o id do cliente
                {
                    if (columns[2] != "Concluido") // Ignorar as tarefas concluidas
                    {
                        output.Id = columns[0];
                        output.Descricao = columns[1];
                        output.Estado = columns[2];
                        output.ClientId = columns[3];
                    }
                }
            }
            return Task.FromResult(output);
        }
    }
}
