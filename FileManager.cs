namespace TrabalhoP2P;

using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;

public class FileManager
{
    private readonly string _baseDir;
    private readonly Peer _peer;

    // Flag para desativar o DirectoryWatcher localmente durante operações de recebimento.
    public bool IsReceivingOperation { get; private set; } = false;

    public FileManager(string baseDir, Peer peer)
    {
        _baseDir = baseDir;
        _peer = peer;
    }

    // Prepara e envia um arquivo para todos os peers (usado para CREATE e MODIFY).
    public async Task SendFile(string filePath)
    {
        if (IsReceivingOperation) return;

        Console.WriteLine($"Preparando envio do arquivo: {filePath}");
        try
        {
            // Verificação crucial: se o arquivo não existe mais (possível exclusão rápida), propaga a exclusão em vez do arquivo.
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"Arquivo '{Path.GetFileName(filePath)}' não encontrado. Propagando como exclusão.");
                await DeleteFile(Path.GetFileName(filePath));
                return;
            }

            byte[] content = await File.ReadAllBytesAsync(filePath);
            string fileName = Path.GetFileName(filePath);

            // Monta o cabeçalho do protocolo: "FILE:<nome_do_arquivo>:" seguido pelo conteúdo binário.
            string header = $"FILE:{fileName}:";
            byte[] headerBytes = Encoding.UTF8.GetBytes(header);
            
            byte[] message = new byte[headerBytes.Length + content.Length];
            Buffer.BlockCopy(headerBytes, 0, message, 0, headerBytes.Length);
            Buffer.BlockCopy(content, 0, message, headerBytes.Length, content.Length);

            await SendToAllPeers(message);
        }
        catch (IOException e)
        {
            Console.WriteLine($"Erro ao ler o arquivo: {e.Message}");
        }
    }

    // Envia uma mensagem de deleção para todos os peers.
    public async Task DeleteFile(string fileName)
    {
        if (IsReceivingOperation) return;
        
        Console.WriteLine($"Solicitando deleção do arquivo: {fileName}");
        // Comando de deleção simples: "DELETE:<nome_do_arquivo>".
        string message = $"DELETE:{fileName}";
        await SendToAllPeers(Encoding.UTF8.GetBytes(message));
    }

    // Envia a mensagem (comando + dados) para todos os endereços de peers conhecidos via UDP.
    private async Task SendToAllPeers(byte[] message)
    {
        Console.WriteLine($"Tentando enviar para peers: {string.Join(", ", _peer.KnownPeers)}");
        
        using (var client = new UdpClient())
        {
            foreach (var peerAddress in _peer.KnownPeers)
            {
                try
                {
                    string[] parts = peerAddress.Split(':');
                    string host = parts[0];
                    int port = int.Parse(parts[1]);
                    
                    // Envio assíncrono do datagrama UDP.
                    await client.SendAsync(message, message.Length, host, port);
                    
                    Console.WriteLine($"Operação enviada com SUCESSO para: {peerAddress}");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"ERRO ao enviar para {peerAddress}: {e.Message}");
                }
            }
        }
    }

    // Salva um arquivo recebido de outro peer no diretório base.
    public async Task SaveFile(string fileName, byte[] content)
    {
        // Ativa a flag para evitar que o DirectoryWatcher re-envie o arquivo recém-salvo.
        IsReceivingOperation = true;
        try
        {
            string dest = Path.Combine(_baseDir, fileName);
            await File.WriteAllBytesAsync(dest, content);
            Console.WriteLine($"Arquivo salvo em: {dest}");
            await Task.Delay(100); 
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
        finally
        {
            // Desativa a flag para permitir monitoramento normal.
            IsReceivingOperation = false;
        }
    }

    // Deleta um arquivo localmente com base em uma mensagem de deleção recebida.
    public async Task DeleteReceivedFile(string fileName)
    {
        // Ativa a flag para evitar que o DirectoryWatcher detecte a exclusão e a re-propague.
        IsReceivingOperation = true;
        try
        {
            string filePath = Path.Combine(_baseDir, fileName);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                Console.WriteLine($"Arquivo deletado: {fileName}");
            }
            await Task.Delay(100);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
        finally
        {
            // Desativa a flag.
            IsReceivingOperation = false;
        }
    }
}