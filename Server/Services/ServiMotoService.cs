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
            string[] lines = File.ReadAllLines(filePath);

            string clientLine = lines.FirstOrDefault(line => line.StartsWith($"{request.Id},"));

            if (clientLine != null)
            {
                // Columns [0] = id; [1] = password; [2] = servico
                string[] columns = clientLine.Split(',');

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
            return base.FindCurrentTask(request, context);
        }
    }
}
