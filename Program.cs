using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("P2PChatアプリです。");
        Console.WriteLine("1. サーバーモード");
        Console.WriteLine("2. クライアントモード");
        Console.Write("モードを入力してください: ");
        
        var input = Console.ReadLine();
        
        if (input == "1")
        {
            await StartServer();
        }
        else if (input == "2")
        {
            await StartClient();
        }
    }
    
    static async Task StartServer()
    {
        const int port = 8080;
        
        // TCPリスナーを作成
        TcpListener listener = new TcpListener(IPAddress.Any, port);
        
        try
        {
            // リスナーを開始
            listener.Start();
            Console.WriteLine($"Server started on port {port}");
            Console.WriteLine("Waiting for client connection...");
            
            // クライアントからの接続を待機
            TcpClient client = await listener.AcceptTcpClientAsync();
            Console.WriteLine("Client connected!");
            
            // ネットワークストリームを取得
            NetworkStream stream = client.GetStream();
            
            // メッセージ送受信処理
            await HandleClient(stream);
            
            // 接続を閉じる
            client.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Server error: {ex.Message}");
        }
        finally
        {
            listener.Stop();
            Console.WriteLine("Server stopped.");
        }
    }
    
    static async Task HandleClient(NetworkStream stream)
    {
        try
        {
            // 受信用のバッファ
            byte[] buffer = new byte[1024];
            
            // 受信処理を別タスクで実行
            Task receiveTask = Task.Run(async () =>
            {
                while (true)
                {
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead == 0)
                    {
                        Console.WriteLine("Client disconnected.");
                        break;
                    }
                    
                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Console.WriteLine($"Client: {message}");
                }
            });
            
            // 送信処理
            while (true)
            {
                // Console.Write("Server: ");
                string input = Console.ReadLine();

                if (string.IsNullOrEmpty(input))
                    break;

                byte[] data = Encoding.UTF8.GetBytes(input);
                await stream.WriteAsync(data, 0, data.Length);
            }
            
            await receiveTask;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Communication error: {ex.Message}");
        }
    }
    
    static async Task StartClient()
    {
        Console.Write("Enter server IP address (or press Enter for localhost): ");
        string serverIP = Console.ReadLine();
        
        // 空文字の場合はlocalhostを使用
        if (string.IsNullOrEmpty(serverIP))
        {
            serverIP = "127.0.0.1";
        }
        
        const int port = 8080;
        
        try
        {
            // TCPクライアントを作成
            TcpClient client = new TcpClient();
            
            Console.WriteLine($"Connecting to {serverIP}:{port}...");
            
            // サーバーに接続
            await client.ConnectAsync(serverIP, port);
            Console.WriteLine("Connected to server!");
            
            // ネットワークストリームを取得
            NetworkStream stream = client.GetStream();
            
            // メッセージ送受信処理
            await HandleServer(stream);
            
            // 接続を閉じる
            client.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Client error: {ex.Message}");
        }
    }
    
    static async Task HandleServer(NetworkStream stream)
    {
        try
        {
            // 受信用のバッファ
            byte[] buffer = new byte[1024];
            
            // 受信処理を別タスクで実行
            Task receiveTask = Task.Run(async () =>
            {
                while (true)
                {
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead == 0)
                    {
                        Console.WriteLine("Server disconnected.");
                        break;
                    }
                    
                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Console.WriteLine($"Server: {message}");
                }
            });
            
            // 送信処理
            while (true)
            {
                // Console.Write("Client: ");

                string input = Console.ReadLine();
                
                if (string.IsNullOrEmpty(input))
                    break;
                               
                byte[] data = Encoding.UTF8.GetBytes(input);
                await stream.WriteAsync(data, 0, data.Length);
            }
            
            await receiveTask;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Communication error: {ex.Message}");
        }
    }
}