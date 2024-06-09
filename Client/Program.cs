using Client.Model;
using Grpc.Net.Client;
using Server;

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

                switch(input)
                {
                    // Pedir tarefa atual
                    case "TASK INFO":
                        await GetCurrentTaskAsync(client, currentUser);
                        break;
                    case "TASK NEW":
                        // Para pedir uma nova tarefa não pode ter nenhuma em curso
                        if (currentTask.Id == string.Empty)
                        {

                        }
                        else
                        {

                        }

                        break;
                    case "TASK COMPLETE":
                        // Apenas pode declarar uma tarefa como concluida se tiver uma tarefa em curso
                        if (currentTask.Id != string.Empty)
                        {

                            currentTask = new TarefaModel();
                        }
                        else
                        {

                        }

                        break;
                    case "SERVICE LEAVE":

                        break;
                    case "SERVICE NEW":

                        break;
                    case "QUIT":
                        Console.WriteLine("Press any key to continue...");
                        Console.ReadKey();
                        return;
                    default:

                        break;
                }
            }
        }

        static async Task<ClientModel> LoginAsync(ServiMoto.ServiMotoClient client)
        {
            Console.WriteLine("Please log in.");
            Console.WriteLine(">[ID] [Password]");
            string? dadosInseridos = Console.ReadLine();
            
            ClientModel currentClient = new ClientModel();
            string[] dados = dadosInseridos.Split(' ');

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

        static void HelpCommand()
        {
            Console.Write("You can use the following commands: \n" +
                ">TASK NEW \n>TASK COMPLETED \n" +
                ">SERVICE LEAVE \n>SERVICE NEW \n" +
                ">QUIT\n\n");
        }
    }
}