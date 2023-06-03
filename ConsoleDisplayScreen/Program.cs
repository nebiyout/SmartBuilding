// See https://aka.ms/new-console-template for more information
using System.IO.Pipes;

SetUpConnection();

void SetUpConnection()
{
    var server = new NamedPipeServerStream("PipeName");
    Console.WriteLine("Waiting for connection...");
    server.WaitForConnection();

    StreamReader reader = new StreamReader(server);
    
    while (true)
    {
        var data = reader.ReadLine();
        
        if (string.IsNullOrEmpty(data))
            break;
        
        Console.WriteLine(data);
    }
}