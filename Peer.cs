namespace TrabalhoP2P;
using System.Collections.Generic;

public class Peer
{
    // O endereço (IP:porta) deste peer, usado como identificador na rede.
    public string Address { get; set; }
    // Conjunto de endereços (IP:porta) dos peers para os quais as mensagens devem ser enviadas (multicast manual).
    public HashSet<string> KnownPeers { get; set; }
    // O caminho do diretório local que está sendo sincronizado.
    public string Directory { get; set; }

    public Peer(string address, string directory)
    {
        Address = address;
        Directory = directory;
        KnownPeers = new HashSet<string>();
    }
}