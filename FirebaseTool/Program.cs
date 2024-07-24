using System.Runtime.CompilerServices;
using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;

namespace FirebaseTool
{
    internal static class Program
    {
        private const string PRIVATE_KEY_PATH = "PrivateKey.json";

        private enum Operations
        {
            Quit = 0,
            CreateUser = 1,
        }
        
        private static async Task Main(string[] args)
        {
            var projectID = PromptAndReadInput("Enter project ID");
            
            var config = new AppOptions()
            {
                Credential = GoogleCredential.FromFile(PRIVATE_KEY_PATH),
                ProjectId = projectID,
            };
            
            var app = FirebaseApp.Create(config);

            var auth = FirebaseAuth.GetAuth(app);

            while (true)
            {
                
                Console.WriteLine(
                $"""
                Select an option:
                - {(int) Operations.CreateUser} | {nameof(Operations.CreateUser)}
                - {(int) Operations.Quit} | {nameof(Operations.Quit)}
                """);

                switch (TryGetEnumInput<Operations>())
                {
                    case Operations.CreateUser:
                        await HandleCreateUser(auth);
                        break;
                    case Operations.Quit:
                        Console.WriteLine("Exiting...");
                        return;
                    default:
                        Console.WriteLine("Invalid input! Try again...");
                        continue;
                }
            }
        }

        private static async Task HandleCreateUser(FirebaseAuth auth)
        {
            var email = PromptAndReadInput("Enter email of user:");
            
            var password = PromptAndReadInput("Enter password of user:");

            ulong userID;
            
            while (!ulong.TryParse(PromptAndReadInput("Enter SnowflakeID of user:"), out userID))
            {
                Console.WriteLine("Please enter a valid 64-bit SnowflakeID");
            }

            try
            {
                var newUser = await auth.CreateUserAsync(new()
                {
                    Email = email,
                    Password = password,
                    Uid = userID.ToString(),
                });

                Console.WriteLine("Success!");
            }
            
            catch (FirebaseAuthException exception)
            {
                Console.WriteLine($"Error creating user: {exception.Message}");
            }
        }
        
        private static string PromptAndReadInput(string prompt)
        {
            Console.WriteLine(prompt);
            return Console.ReadLine()!;
        }
        
        private static unsafe T TryGetEnumInput<T>() where T : unmanaged, Enum
        {
            if (sizeof(T) != sizeof(int))
            {
                throw new InvalidCastException();
            }

            int num;
            
            while (!int.TryParse(Console.ReadLine(), out num))
            {
                Console.WriteLine("Please enter a numeric input");
            }

            return Unsafe.BitCast<int, T>(num);
        }
    }
}