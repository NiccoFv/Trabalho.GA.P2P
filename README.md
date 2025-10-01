# TrabalhoRedesP2P

Este projeto implementa um sistema de **compartilhamento e replicação de arquivos Peer-to-Peer (P2P)**.

## Abordagem do Problema Principal

O objetivo central deste trabalho é simular uma **rede P2P descentralizada** onde os nós (Peers) podem compartilhar, adicionar e remover arquivos de forma autônoma e resiliente.

A solução aborda a **replicação de arquivos** para garantir que o conteúdo não se perca se um Peer sair da rede. Cada Peer é capaz de:

1.  **Conexão e Descoberta:** Conectar-se a Peers conhecidos para iniciar a comunicação.
2.  **Sincronização:** Manter o seu *filesystem* sincronizado com o de outros Peers.
3.  **Resiliência:** Replicar arquivos automaticamente, garantindo que a remoção de um arquivo só ocorra quando todos os Peers concordarem, ou que um arquivo se mantenha disponível mesmo que seu Peer original saia.

## 💻 Tecnologia Utilizada

O projeto foi desenvolvido em **C\#** utilizando o framework **.NET Core**.

O uso do .NET Core garante compatibilidade *cross-platform* (Windows, macOS, Linux) e oferece ferramentas robustas para desenvolvimento de aplicações de rede, como a manipulação de sockets e comunicação TCP/IP.

## Pré-requisitos para Execução

Para rodar o projeto localmente, você precisa ter instalado:

1.  **SDK do .NET Core (versão 6.0 ou superior):** Necessário para compilar e executar o código C\#.
2.  **Docker e Docker Compose:** Necessário apenas para a execução e teste da versão conteinerizada do projeto.

-----

## Como Executar e Testar

Existem duas formas de rodar o ambiente de testes: **Local** (utilizando o .NET) ou **Conteinerizada** (utilizando Docker).

### 1\. Execução e Teste Local

Execute os comandos a seguir no terminal, a partir da raiz do projeto (`TrabalhoP2P/`).

#### 1.1. Inicializar os Peers

Abra **quatro janelas de terminal** separadas. Em cada uma, execute um comando diferente para inicializar os Peers (os arquivos `.txt` contêm os endereços dos Peers "conhecidos" para iniciar a rede):

| Peer | Comando | Função |
| :--- | :--- | :--- |
| **Peer 1** | `dotnet run 5000 knownPeers/knownPeers1_local.txt tmp/peer1` | Escuta na porta **5000** |
| **Peer 2** | `dotnet run 5001 knownPeers/knownPeers2_local.txt tmp/peer2` | Escuta na porta **5001** |
| **Peer 3** | `dotnet run 5002 knownPeers/knownPeers3_local.txt tmp/peer3` | Escuta na porta **5002** |
| **Peer 4** | `dotnet run 5003 knownPeers/knownPeers4_local.txt tmp/peer4` | Escuta na porta **5003** |

#### 1.2. Comandos de Teste (Adicionar e Remover Arquivos)

Os Peers devem replicar automaticamente os arquivos entre si. Os diretórios `tmp/peerX` representam os *filesystems* locais de cada Peer.

**A. Adicionar Arquivos:**
Crie arquivos nos diretórios locais dos Peers. Eles serão propagados na rede.

```bash
# Cria "teste1.txt" no Peer 1
echo “Criando teste 1, pelo Peer1” > tmp/peer1/teste1.txt

# Cria "teste2.txt" no Peer 3
echo “Criando teste 2, pelo Peer3“ > tmp/peer3/teste2.txt
```

**B. Remover Arquivos:**
Exclua arquivos de um diretório local. A remoção será propagada na rede para sincronização.

```bash
# Remove "teste1.txt" do Peer 2 (a remoção será sincronizada)
rm tmp/peer2/teste1.txt

# Remove "teste2.txt" do Peer 1 (a remoção será sincronizada)
rm tmp/peer1/teste2.txt
```

-----

### 2\. Execução Conteinerizada (Docker)

Esta opção utiliza o `docker-compose.yaml` para subir a rede de Peers em containers isolados, simplificando a inicialização do ambiente.

#### 2.1. Iniciar a Rede de Peers (Containers)

O comando a seguir irá construir a imagem e iniciar quatro containers em *background*.

```bash
docker-compose up --build -d
```

#### 2.2. Mostrar Logs

O comando a seguir libera a visualização de logs no terminal, em tempo real.

```bash
docker-compose logs -f
```

#### 2.3. Parar a Rede de Peers

Para derrubar todos os containers e limpar a rede:

```bash
docker-compose down
```
