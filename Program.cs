using TrabalhoP2P;

using System;
using System.IO;
using System.Threading.Tasks;

class Program
{
    // Método de entrada assíncrono e ponto de inicialização de todos os componentes.
    static async Task Main(string[] args)
    {
        // Verifica a entrada obrigatória: porta, arquivo de peers e diretório.
        if (args.Length < 3)
        {
            Console.WriteLine("Uso: dotnet run <porta> <peers.txt> <diretorio>");
            return;
        }

        int port = int.Parse(args[0]);
        string peersFile = args[1];
        string dir = args[2];

        // Cria o diretório de sincronização caso ele não exista.
        Directory.CreateDirectory(dir);

        // Cria a instância principal do Peer.
        Peer peer = new Peer($"127.0.0.1:{port}", dir);

        // Carrega a lista de peers conhecidos a partir do arquivo de configuração.
        string[] lines = await File.ReadAllLinesAsync(peersFile);
        foreach (var line in lines)
        {
            if (!string.IsNullOrWhiteSpace(line))
            {
                peer.KnownPeers.Add(line.Trim());
            }
        }

        // Inicializa as classes de gerenciamento e monitoramento.
        FileManager fileManager = new FileManager(dir, peer);
        PeerServer server = new PeerServer(peer, port, fileManager);
        DirectoryWatcher watcher = new DirectoryWatcher(dir, fileManager);

        // Inicia o servidor (tarefa de escuta UDP) e o monitoramento do diretório (watcher).
        Task serverTask = server.Start();
        watcher.Start();

        Console.WriteLine($"Peer rodando na porta {port} e diretório {dir}");
        Console.WriteLine($"Peers conhecidos: {string.Join(", ", peer.KnownPeers)}");

        // Mantém o programa em execução enquanto a tarefa principal do servidor estiver ativa.
        await serverTask;
    }
}