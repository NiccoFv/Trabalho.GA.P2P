using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TrabalhoP2P
{
    public class PeerServer
    {
        private readonly Peer _peer;
        private readonly FileManager _fileManager;
        private readonly int _port;

        public PeerServer(Peer peer, int port, FileManager fileManager)
        {
            _peer = peer;
            _port = port;
            _fileManager = fileManager;
        }

        // Inicia o servidor UDP para escutar por mensagens de outros peers.
        public async Task Start()
        {
            // Cria um cliente UDP que atua como listener na porta especificada.
            using (UdpClient listener = new UdpClient(_port))
            {
                Console.WriteLine($"Peer escutando na porta {_port}");

                // Loop contínuo para receber datagramas.
                while (true)
                {
                    try
                    {
                        // Método que bloqueia a thread até receber um datagrama UDP.
                        UdpReceiveResult result = await listener.ReceiveAsync();
                        byte[] receivedBytes = result.Buffer;
                        // Processa a mensagem de forma assíncrona.
                        await ProcessMessage(receivedBytes);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Erro ao receber mensagem: {e.Message}");
                    }
                }
            }
        }

        // Classifica a mensagem recebida com base no seu prefixo (comando).
        private async Task ProcessMessage(byte[] data)
        {
            // Decodifica a mensagem para verificar o comando.
            string command = Encoding.UTF8.GetString(data);

            try
            {
                // Verifica o prefixo para determinar o tipo de operação.
                if (command.StartsWith("FILE:"))
                {
                    await ProcessFileMessage(data);
                }
                else if (command.StartsWith("DELETE:"))
                {
                    await ProcessDeleteMessage(command);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Erro ao processar comando: {e.Message}");
            }
        }

        // Processa uma mensagem de arquivo, extraindo nome e conteúdo.
        private async Task ProcessFileMessage(byte[] data)
        {
            // Lógica para encontrar o delimitador (:) do cabeçalho "FILE:<nome_do_arquivo>:".
            int firstColon = Array.IndexOf(data, (byte)':');
            int secondColon = -1;
            for (int i = firstColon + 1; i < data.Length; i++)
            {
                if (data[i] == (byte)':')
                {
                    secondColon = i;
                    break;
                }
            }

            if (secondColon > -1)
            {
                // Extrai o nome do arquivo a partir da string do cabeçalho.
                string header = Encoding.UTF8.GetString(data, 0, secondColon);
                string[] parts = header.Split(':');
                string fileName = parts[1];

                // Separa o conteúdo binário do arquivo (o restante dos bytes).
                int contentStartIndex = secondColon + 1;
                int contentLength = data.Length - contentStartIndex;
                byte[] fileContent = new byte[contentLength];
                Array.Copy(data, contentStartIndex, fileContent, 0, contentLength);

                Console.WriteLine($"Recebendo arquivo: {fileName}");
                // Delega a tarefa de salvar o arquivo ao FileManager.
                await _fileManager.SaveFile(fileName, fileContent);
            }
        }

        // Processa uma mensagem de deleção.
        private async Task ProcessDeleteMessage(string message)
        {
            // Analisa a mensagem "DELETE:<nome_do_arquivo>".
            string[] parts = message.Split(':');
            if (parts.Length >= 2)
            {
                string fileName = parts[1];
                Console.WriteLine($"Recebendo comando DELETE: {fileName}");
                // Delega a tarefa de deletar o arquivo ao FileManager.
                await _fileManager.DeleteReceivedFile(fileName);
            }
        }
    }
}