using Client.Model;
using Grpc.Net.Client;
using Server;
using System.Threading.Tasks;

namespace Client
{
    class Program
    {
        static async Task Main(string[] args)
        {
            using var channel = GrpcChannel.ForAddress("http://localhost:5083");
            var client = new ServiMoto.ServiMotoClient(channel);

            await HandleClient(client);
        }

        static async Task HandleClient(ServiMoto.ServiMotoClient client)
        {
            ClientModel currentUser = new ClientModel();
            TarefaModel currentTask = new TarefaModel();
            string? input = string.Empty;
            string? command = string.Empty;

            // Ciclo para login
            while (true)
            {
                currentUser = await LoginAsync(client);

                if (currentUser.Id != string.Empty)
                {
                    // Depois de se autenticar procura a tarefa atual
                    Console.Clear();
                    currentTask = await GetCurrentTaskAsync(client, currentUser);
                    break;
                }

                Console.Clear();
            }

            while (true)
            {
                HelpCommand();
                input = Console.ReadLine();
                command = input.ToUpper();

                switch (command)
                {
                    // Pedir tarefa atual
                    case "TASK INFO":
                        // Para pedir informação sobre tarefa atual tem que estar em um serviço
                        if(currentUser.Service != string.Empty)
                        {
                            await GetCurrentTaskAsync(client, currentUser);
                        }
                        else
                        {
                            Console.WriteLine("You are not allocated to a service.");
                        }

                        break;
                    case "TASK NEW":
                        // Para pedir uma nova tarefa não pode ter nenhuma em curso
                        if (currentTask.Id == string.Empty && currentUser.Service != string.Empty)
                        {
                            currentTask = await GetNewTaskAsync(client, currentUser);
                        }
                        else
                        {
                            Console.WriteLine("You can't use this command.");
                        }

                        break;
                    case "TASK COMPLETE":
                        // Para concluir uma tarefa tem que ter uma tarefa em curso
                        if (currentTask.State == "Em curso" && currentUser.Service != string.Empty)
                        {
                            currentTask = await FinishTaskAsync(client, currentTask, currentUser.Service);
                        }
                        else
                        {
                            Console.WriteLine("You can't use this command.");
                        }

                        break;
                    case "TASK ADD":
                        if (currentUser.Service != string.Empty)
                        {

                        }
                        else
                        {
                            Console.WriteLine("You can't use this command.");
                        }
                        break;
                    case "SERVICE LEAVE":
                        // Para sair de um serviço tem que estar alocado em um serviço
                        if (currentUser.Service != string.Empty)
                        {
                            await LeaveService(client, currentTask, currentUser, currentUser.Service);
                        }
                        else
                        {
                            Console.WriteLine("You can't use this command.");
                        }
                        break;
                    case "SERVICE NEW":
                        // Para pedir um novo serviço não pode estar alocado em nenhum
                        if (currentUser.Service == string.Empty)
                        {
                            await NewService(client, currentUser, currentUser.Service);
                        }
                        else
                        {
                            Console.WriteLine("You can't use this command.");
                        }
                        break;
                    case "QUIT":
                        Console.WriteLine("Press any key to continue...");
                        Console.ReadKey();
                        return;
                    default:
                        Console.WriteLine("Invalid command.");
                        break;
                }  
            }
        }

        // Metodo para Login
        static async Task<ClientModel> LoginAsync(ServiMoto.ServiMotoClient client)
        {
            Console.WriteLine("Please log in.");
            Console.WriteLine(">[ID] [Password]");
            string? dadosInseridos = Console.ReadLine();
            
            ClientModel currentClient = new ClientModel();
            string[] dados = dadosInseridos.Split(' ');

            // Envia ID e Password e recebe o serviço
            var request = new ClientLookup { Id = dados[0], Password = dados[1] };
            var response = await client.LogInClientAsync(request);

            if (response.Servico != string.Empty)
            {
                currentClient.Update(dados[0], response.Servico);
                Console.WriteLine("\nLogin successful. Welcome {0} from {1}.", currentClient.Id, currentClient.Service);
            }
            else
            {
                Console.WriteLine("\nIncorrect username or password.\n");
            }

            return currentClient;
        }

        // Metodo para receber a tarefa atual do cliente
        static async Task<TarefaModel> GetCurrentTaskAsync(ServiMoto.ServiMotoClient client, ClientModel currentClient)
        {
            TarefaModel currentTask = new TarefaModel();

            var request = new TaskLookup { Id = currentClient.Id, Servico = currentClient.Service };
            var response = await client.FindCurrentTaskAsync(request);

            if (response.Id != string.Empty)
            {
                currentTask.UpdateTask(response.Id, response.Descricao, response.Estado, response.ClientId);
                Console.WriteLine($"Current task: {currentTask.Description}\n");
            }
            else
            {
                Console.WriteLine("You don't have an allocated task.\n");
            }

            return currentTask;
        }

        // Metodo para pedir uma nova tarefa
        static async Task<TarefaModel> GetNewTaskAsync(ServiMoto.ServiMotoClient client, ClientModel currentClient)
        {
            TarefaModel currentTask = new TarefaModel();

            var request = new TaskLookup { Id = currentClient.Id, Servico = currentClient.Service };
            var response = await client.GetNewTaskAsync(request);

            if (response.Id != string.Empty)
            {
                currentTask.UpdateTask(response.Id, response.Descricao, response.Estado, response.ClientId);
                Console.WriteLine($"New task: {currentTask.Description}\n");
            }
            else
            {
                // Pode nao ser alocada se não houver tarefas disponíveis, ou erros
                Console.WriteLine("Failed to allocate a new task. Try again later.\n");
            }

            return currentTask;
        }

        static async Task<TarefaModel> FinishTaskAsync(ServiMoto.ServiMotoClient client, TarefaModel currentTask, string servico)
        {
            TarefaModel task = currentTask;

            if (task.Id != string.Empty)
            {
                var request = new TaskLookup { Id = task.Id, Servico = servico };
                var response = await client.CompleteTaskAsync(request);

                if (response.Result)
                {
                    Console.WriteLine("Task completed successfully.");
                    task = new TarefaModel();
                }
                else
                {
                    Console.WriteLine("Failed to complete task.");
                }

                return task;
            }

            Console.WriteLine("You don't have an assigned task.");

            return task;
        }

        static async Task CreateNewTaskAsync(ServiMoto.ServiMotoClient client, ClientModel currentUser)
        {
            Console.WriteLine("\nPlease insert the task description");
            string? descricao = Console.ReadLine();

            if (descricao != null)
            {
                var request = new TaskInfo { Servico = currentUser.Service, Descricao = descricao };
                var response = await client.NewTaskAsync(request);
            }
        }

        static async Task LeaveService(ServiMoto.ServiMotoClient client, TarefaModel currentTask, ClientModel currentClient, string servico)
        {
            TarefaModel task = currentTask;

            if (servico != string.Empty)
            {
                var request = new ServiceLookup { IdTask = task.Id, IdUtilizador = currentClient.Id, Servico = servico};
                var response = await client.LeaveServiceAsync(request);

                if (response.Result)
                {
                    Console.WriteLine("Service leaved successfully.\n");
                }
                else
                {
                    Console.WriteLine("Failed to leave service.\n");
                }
            }
            else
            {
                Console.WriteLine("You don't have an assigned service.\n");
            }

        }
        static async Task NewService(ServiMoto.ServiMotoClient client, ClientModel currentClient, string servico)
        {
            if (servico == string.Empty) {
                var request = new TaskLookup { Id = currentClient.Id, Servico = currentClient.Service };
                var response = await client.NewServiceAsync(request);

                if (response.Result)
                {
                    Console.WriteLine($"New service: {currentClient.Service}");
                }
                else
                {
                    Console.WriteLine("Failed to allocate a new service. Try again later.\n");
                }
            }
        }

        // Metodo para informar utilizador sobre comandos que pode usar.
        static void HelpCommand()
        {
            string message = "You can use the following commands: \n" +
                ">TASK INFO \n>TASK NEW \n" +
                ">TASK COMPLETE \n>SERVICE LEAVE \n" +
                ">SERVICE NEW \n>QUIT\n\n";

            Console.Write(message);
            return;
        }
    }
}