using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace TrabalhoP2P
{
    public class DirectoryWatcher
    {
        private readonly string _path;
        private readonly FileManager _fileManager;
        // Conjunto e objeto de bloqueio para gerenciar eventos duplicados e race conditions do FileSystemWatcher.
        private readonly HashSet<string> _recentlyProcessed = new HashSet<string>();
        private readonly object _lock = new object();

        public DirectoryWatcher(string path, FileManager fileManager)
        {
            _path = path;
            _fileManager = fileManager;
        }

        // Inicia o monitoramento do diretório.
        public void Start()
        {
            FileSystemWatcher watcher = new FileSystemWatcher(_path);

            // Filtra para monitorar alterações no nome do arquivo, na última escrita e no nome do diretório.
            watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.DirectoryName;

            // Assina os eventos de criação, alteração e exclusão de arquivos.
            watcher.Created += OnFileChanged;
            watcher.Changed += OnFileChanged;
            watcher.Deleted += OnFileDeleted;

            // Ativa o monitoramento.
            watcher.EnableRaisingEvents = true;
            Console.WriteLine($"Monitorando o diretório: {_path}");
        }

        // Manipulador para eventos de criação ou alteração de arquivo.
        private async void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            // Ignora o evento se o nome for nulo ou se o arquivo foi processado recentemente/está sendo recebido.
            if (e.Name == null || ShouldIgnoreEvent(e.Name)) return;

            Console.WriteLine($"Alteração detectada: {e.ChangeType} -> {e.FullPath}");

            // Pequeno atraso para garantir que a escrita do arquivo foi concluída antes de tentar ler.
            await Task.Delay(100);
            // Envia o arquivo alterado através do FileManager.
            await _fileManager.SendFile(e.FullPath);
        }

        // Manipulador para eventos de exclusão de arquivo.
        private async void OnFileDeleted(object sender, FileSystemEventArgs e)
        {
            if (e.Name == null || ShouldIgnoreEvent(e.Name)) return;

            Console.WriteLine($"Alteração detectada: {e.ChangeType} -> {e.FullPath}");
            // Solicita a deleção do arquivo aos outros peers.
            await _fileManager.DeleteFile(e.Name);
        }

        // Lógica para filtrar eventos indesejados (ex: temporários, duplicados, ou operações de recebimento).
        private bool ShouldIgnoreEvent(string fileName)
        {
            lock (_lock)
            {
                // Ignora arquivos temporários (começando com '.') ou eventos gerados por uma operação de recebimento.
                if (fileName.StartsWith(".") || _fileManager.IsReceivingOperation)
                {
                    return true;
                }

                // Evita processar o mesmo arquivo se ele já foi processado há pouco tempo.
                if (_recentlyProcessed.Contains(fileName))
                {
                    return true;
                }

                // Adiciona o arquivo ao conjunto e agenda sua remoção após 1 segundo.
                _recentlyProcessed.Add(fileName);
                Task.Delay(1000).ContinueWith(t =>
                {
                    lock (_lock)
                    {
                        _recentlyProcessed.Remove(fileName);
                    }
                });

                return false;
            }
        }
    }
}