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

            // Ciclo para login
            while (true)
            {
                currentClient = await LoginAsync(client);

                if (currentClient.Id != string.Empty)
                {
                    Console.WriteLine("Login successful. Welcome {0} from {1}.", currentClient.Id, currentClient.Service);
                    break;
                }

                Console.WriteLine("Incorrect username or password.");
            }

            //while (true)
            //{
            //    Console.WriteLine("Enter a name");
            //    var command = Console.ReadLine();

            //    var input = new HelloRequest { Name = command };
            //    var reply = await client.SayHelloAsync(input);

            //    Console.WriteLine(reply.Message);
            //}
            Console.WriteLine("Press a key to continue...");
            Console.ReadKey();
        }

        static async Task<ClientModel> LoginAsync(ServiMoto.ServiMotoClient client)
        {
            Console.WriteLine("Please log in.");
            Console.WriteLine("[ID] [Password]");
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
    }
}