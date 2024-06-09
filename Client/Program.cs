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

            ClientModel currentClient = new ClientModel();
            TarefaModel currentTask = new TarefaModel();

            // Ciclo para login
            while (true)
            {
                currentClient = await LoginAsync(client);

                if (currentClient.Id != string.Empty)
                {
                    Console.WriteLine("Login successful. Welcome {0} from {1}.\n", currentClient.Id, currentClient.Service);
                    break;
                }

                Console.WriteLine("Incorrect username or password.\n");
            }

            currentTask = await GetCurrentTaskAsync(client, currentClient);

            Console.WriteLine($"{currentTask.Id},{currentTask.Description},{currentTask.State},{currentTask.ClienteId}");
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
            //HandleCommand(client, currentClient);
        }

        static async void HandleCommand(ServiMoto.ServiMotoClient client, ClientModel currentClient)
        {
            //string? command = Console.ReadLine();
            //ClientModel currentClient = new ClientModel();
            TarefaModel currentTask = await GetCurrentTaskAsync(client, currentClient);
            
            if (currentClient.Id != string.Empty)
            {
                HelpCommand();

                Console.WriteLine($"{currentTask.Id},{currentTask.Description},{currentTask.State},{currentTask.ClienteId}");
                /*switch (command)
                {
                    // Pedir tarefa atual
                    case "TASK":

                        break;
                    case "TASK NEW":
                        // Para pedir uma nova tarefa não pode ter nenhuma em curso
                        if (currentTask.Id == null)
                        {

                        }
                        else
                        {

                        }

                        break;
                    case "TASK COMPLETE":
                        // Apenas pode declarar uma tarefa como concluida se tiver uma tarefa em curso
                        if (currentTask.Id != null)
                        {

                            currentTask = new TarefaModel();
                        }

                        break;
                    case "SERVICE LEAVE":

                        break;
                    case "SERVICE NEW":

                        break;
                    case "QUIT":

                        return;
                    default:

                        break;
                }*/
            }
            else
            {
                Console.WriteLine("No ingles");
            }
            Console.ReadKey();
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
            }

            return currentTask;
        }

        static void HelpCommand()
        {
            Console.Write("You can use the following commands: \n" +
                ">TASK NEW \n>TASK COMPLETED \n" +
                ">SERVICE LEAVE \n>SERVICE NEW \n" +
                ">QUIT\n");
        }
    }
}