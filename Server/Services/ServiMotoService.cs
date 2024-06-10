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

            reader.Close();
            // Retorna o servico encontrado
            return Task.FromResult(output);
        }

        // Servico para procurar Tarefa atual do cliente
        public override Task<CurrentTask> FindCurrentTask(ClientInfo request, ServerCallContext context)
        {
            CurrentTask output = new CurrentTask();

            string servico = request.Servico;

            if (servico == string.Empty)
            {
                return Task.FromResult(output);
            }

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
            reader.Close();
            return Task.FromResult(output);
        }

        // Servico para alocar um cliente a uma nova tarefa e devolver a tarefa
        public override Task<CurrentTask> GetNewTask(ClientInfo request, ServerCallContext context)
        {
            CurrentTask output = new CurrentTask();

            string servico = request.Servico;

            if (servico == string.Empty)
            {
                return Task.FromResult(output);
            }

            string workingDirectory = Environment.CurrentDirectory;
            string filePath = @$"{workingDirectory}\Data\{servico}.csv";

            string[] lines = File.ReadAllLines(filePath);

            int lineIndex = Array.FindIndex(lines, line => line.Contains($",Nao alocado,"));

            if (lineIndex >= 0)
            {
                string line = lines[lineIndex];
                // TarefaId,Descricao,Estado,ClienteId
                string[] colunas = line.Split(',');

                // Modificar linha
                colunas[2] = "Em curso";
                colunas[3] = request.Id;

                line = string.Join(',', colunas);
                lines[lineIndex] = line; // Substituir linha

                File.WriteAllLines(filePath, lines);

                // Mensagem com a nova tarefa
                output.Id = colunas[0];
                output.Descricao = colunas[1];
                output.Estado = colunas[2];
                output.ClientId = colunas[3];
            }

            return Task.FromResult(output);
        }

        // Completar tarefa, recebe IdTarefa e Servico da tarefa
        public override Task<Validation> CompleteTask(TaskInfo request, ServerCallContext context)
        {
            Validation output = new Validation();
            output.Result = false;

            string servico = request.Servico;

            if (servico == string.Empty)
            {
                return Task.FromResult(output);
            }

            string workingDirectory = Environment.CurrentDirectory;
            string filePath = @$"{workingDirectory}\Data\{servico}.csv";

            string[] lines = File.ReadAllLines(filePath);

            // Procurar linha com o id da tarefa
            int lineIndex = Array.FindIndex(lines, line => line.StartsWith($"{request.Id},"));

            if (lineIndex >= 0)
            {
                string line = lines[lineIndex];
                // TarefaId,Descricao,Estado,ClienteId
                string[] colunas = line.Split(',');

                // Modificar estado
                colunas[2] = "Concluido";

                line = string.Join(',', colunas);
                lines[lineIndex] = line;

                File.WriteAllLines(filePath, lines);

            }

            return Task.FromResult(output);
        }

        public override Task<Validation> NewTask(ClientService request, ServerCallContext context)
        {
            Validation output = new Validation();

            // Pub Sub

            return Task.FromResult(output);
        }
    }
}
