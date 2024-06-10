using Google.Protobuf;
using Grpc.Core;
using System.Text;
using System.Text.RegularExpressions;

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
        public override Task<ClientService> LogInClient(ClientLogin request, ServerCallContext context)
        {
            ClientService output = new ClientService();

            string workingDirectory = Environment.CurrentDirectory;
            string filePath = @$"{workingDirectory}\Data\Clientes.csv";

            var reader = new StreamReader(filePath);
            string? line;

            while ((line = reader.ReadLine()) != null)
            {
                // ClientID,Password,Role,Servico
                string[] columns = line.Split(',');

                if (columns[0] == request.Id && columns[1] == request.Password)
                {
                    output.Role = columns[2];
                    output.Servico = columns[3];
                }
            }

            reader.Close();
            // Retorna o servico encontrado
            return Task.FromResult(output);
        }

        // Servico para procurar Tarefa atual do cliente
        public override Task<CurrentTask> FindCurrentTask(TaskLookup request, ServerCallContext context)
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
        public override Task<CurrentTask> GetNewTask(TaskLookup request, ServerCallContext context)
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
        public override Task<Validation> CompleteTask(TaskLookup request, ServerCallContext context)
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

                output.Result = true;
            }

            // Retorna true ou false dependendo se for bem sucedido.
            return Task.FromResult(output);
        }

        public override Task<Validation> NewTask(TaskInfo request, ServerCallContext context)
        {
            Validation output = new Validation();
            output.Result = false;

            string servico = request.Servico;

            try
            {
                if (servico == string.Empty)
                {
                    return Task.FromResult(output);
                }

                string workingDirectory = Environment.CurrentDirectory;
                string filePath = @$"{workingDirectory}\Data\{servico}.csv";

                List<string> lines = new List<string>(File.ReadAllLines(filePath));

                //Padrao dos IDs
                string pattern = $@"S{servico.Last()}_T(\d+)";

                // Encontrar o ultimo ID
                int maxId = lines
                    .Select(line => Regex.Match(line, pattern))
                    .Where(match => match.Success)
                    .Select(match => int.Parse(match.Groups[1].Value))
                    .DefaultIfEmpty(0)
                    .Max();

                string newId = $"S{servico.Last()}_T{maxId + 1}";
                string newDesc = $"{request.Descricao}";

                // TarefaId,Descricao,Estado,ClienteId
                string newLine = $"{newId},{newDesc},Nao alocado,";

                lines.Add(newLine);

                File.WriteAllLines(filePath, lines);
                output.Result = true;

                return Task.FromResult(output);
            }
            catch
            {
                return Task.FromResult(output);
            }
        }

        public override Task<FileContent> GetTasksInfo(ServiceLookup request, ServerCallContext context)
        {
            FileContent output = new FileContent();
            string servico = request.Service;

            if (servico == string.Empty )
            {
                return Task.FromResult(output);
            }

            string workingDirectory = Environment.CurrentDirectory;
            string filePath = @$"{workingDirectory}\Data\{servico}.csv";

            string content = File.ReadAllText(filePath);
            byte[] byteArray = Encoding.UTF8.GetBytes(content);

            ByteString contentBytes = ByteString.CopyFrom(byteArray);

            output.Content = contentBytes;

            return Task.FromResult(output);
        }
        public override Task<Validation> LeaveService(ServiceLookup request, ServerCallContext context)
        {
            Validation output = new Validation();
            output.Result = false;

            string user = request.IdUtilizador;
            string task = request.IdTask;
            string service = request.Servico;

            if (service == string.Empty || task != string.Empty)
            {
                return Task.FromResult(output);
            }

            string workingDirectory = Environment.CurrentDirectory;
            string filePath = @$"{workingDirectory}\Data\Clientes.csv";

            string[] lines = File.ReadAllLines(filePath);

            // Procurar linha com o cliente atual
            int lineIndex = Array.FindIndex(lines, line => line.StartsWith($"{user},"));

            if (lineIndex >= 0)
            {
                string line = lines[lineIndex];
                // ClienteId, Password, Serviço
                string[] colunas = line.Split(',');

                // Sair do Serviço
                colunas[2] = string.Empty;

                line = string.Join(',', colunas);
                lines[lineIndex] = line;

                File.WriteAllLines(filePath, lines);

                output.Result = true;
            }

            // Retorna true ou false dependendo se for bem sucedido.
            return Task.FromResult(output);
        }

        public override Task<Validation> NewService(TaskLookup request, ServerCallContext context)
        {
            Validation output = new Validation();
            output.Result = false;

            string servico = request.Servico;
            string user = request.Id;

            try
            {
                Console.WriteLine("Escolha o serviço (Servico_A, Servico_B, Servico_C ou Servico_D):\n");
                string novoServico = Console.ReadLine();

                if (servico != string.Empty || novoServico != "Servico_A" || novoServico != "Servico_B" || novoServico != "Servico_C" || novoServico != "Servico_D")
                {
                    return Task.FromResult(output);
                }
                string workingDirectory = Environment.CurrentDirectory;
                string filePath = @$"{workingDirectory}\Data\Clientes.csv";

                string[] lines = File.ReadAllLines(filePath);

                // Procurar linha com o cliente atual
                int lineIndex = Array.FindIndex(lines, line => line.StartsWith($"{user},"));

                if (lineIndex >= 0)
                {
                    string line = lines[lineIndex];
                    // ClienteId, Password, Serviço
                    string[] colunas = line.Split(',');

                    // Novo Serviço
                    colunas[2] = novoServico;

                    line = string.Join(',', colunas);
                    lines[lineIndex] = line;

                    File.WriteAllLines(filePath, lines);

                    output.Result = true;
                }

                return Task.FromResult(output);
            }
            catch
            {
                return Task.FromResult(output);
            }

        }
    }
}
