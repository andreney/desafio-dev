using System.Text.Json;

namespace EstoqueMovimentacao;

// ---------- Modelos ----------

public class Produto
{
    public int CodigoProduto { get; set; }
    public string DescricaoProduto { get; set; } = "";
    public int Estoque { get; set; }
}

public record ArquivoEstoque(List<Produto> Estoque);

public enum TipoMovimentacao
{
    Entrada,
    Saida
}

public record Movimentacao(
    int Id,
    int CodigoProduto,
    TipoMovimentacao Tipo,
    int Quantidade,
    string Descricao,
    DateTime DataHora,
    int SaldoFinal);

// ---------- Regra de negócio ----------

public class ServicoEstoque
{
    private readonly Dictionary<int, Produto> _produtos;
    private readonly List<Movimentacao> _historico = new();
    private int _proximoId = 1; // identificador único e sequencial

    public ServicoEstoque(IEnumerable<Produto> produtos)
    {
        _produtos = produtos.ToDictionary(p => p.CodigoProduto);
    }

    public IReadOnlyCollection<Produto> Produtos => _produtos.Values;
    public IReadOnlyList<Movimentacao> Historico => _historico;

    public Movimentacao Movimentar(int codigoProduto, TipoMovimentacao tipo, int quantidade, string descricao)
    {
        if (!_produtos.TryGetValue(codigoProduto, out var produto))
            throw new InvalidOperationException($"Produto {codigoProduto} não encontrado.");

        if (quantidade <= 0)
            throw new ArgumentException("A quantidade deve ser maior que zero.");

        if (string.IsNullOrWhiteSpace(descricao))
            throw new ArgumentException("A descrição da movimentação é obrigatória.");

        if (tipo == TipoMovimentacao.Saida && quantidade > produto.Estoque)
            throw new InvalidOperationException(
                $"Estoque insuficiente. Disponível: {produto.Estoque}, solicitado: {quantidade}.");

        produto.Estoque += tipo == TipoMovimentacao.Entrada ? quantidade : -quantidade;

        var movimentacao = new Movimentacao(
            _proximoId++,
            codigoProduto,
            tipo,
            quantidade,
            descricao.Trim(),
            DateTime.Now,
            produto.Estoque);

        _historico.Add(movimentacao);
        return movimentacao;
    }
}

// ---------- Interface de console ----------

public class Program
{
    public static void Main(string[] args)
    {
        string caminho = args.Length > 0 ? args[0] : "estoque.json";

        if (!File.Exists(caminho))
        {
            Console.WriteLine($"Arquivo não encontrado: {caminho}");
            return;
        }

        var opcoes = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var arquivo = JsonSerializer.Deserialize<ArquivoEstoque>(File.ReadAllText(caminho), opcoes);

        if (arquivo?.Estoque is null || arquivo.Estoque.Count == 0)
        {
            Console.WriteLine("Nenhum produto encontrado no arquivo.");
            return;
        }

        var servico = new ServicoEstoque(arquivo.Estoque);

        while (true)
        {
            Console.WriteLine();
            Console.WriteLine("===== CONTROLE DE ESTOQUE =====");
            Console.WriteLine("1 - Listar estoque");
            Console.WriteLine("2 - Entrada de mercadoria");
            Console.WriteLine("3 - Saída de mercadoria");
            Console.WriteLine("4 - Histórico de movimentações");
            Console.WriteLine("0 - Sair");
            Console.Write("Opção: ");

            switch (Console.ReadLine()?.Trim())
            {
                case "1":
                    ListarEstoque(servico);
                    break;
                case "2":
                    LancarMovimentacao(servico, TipoMovimentacao.Entrada);
                    break;
                case "3":
                    LancarMovimentacao(servico, TipoMovimentacao.Saida);
                    break;
                case "4":
                    ListarHistorico(servico);
                    break;
                case "0":
                    return;
                default:
                    Console.WriteLine("Opção inválida.");
                    break;
            }
        }
    }

    private static void ListarEstoque(ServicoEstoque servico)
    {
        Console.WriteLine();
        Console.WriteLine($"{"Código",-8}{"Produto",-30}{"Estoque",8}");
        foreach (var p in servico.Produtos)
            Console.WriteLine($"{p.CodigoProduto,-8}{p.DescricaoProduto,-30}{p.Estoque,8}");
    }

    private static void LancarMovimentacao(ServicoEstoque servico, TipoMovimentacao tipo)
    {
        ListarEstoque(servico);
        Console.WriteLine();

        int codigo = LerInteiro("Código do produto: ");
        int quantidade = LerInteiro("Quantidade: ");

        Console.Write("Descrição da movimentação (ex.: Compra de fornecedor, Venda, Ajuste): ");
        string descricao = Console.ReadLine() ?? "";

        try
        {
            var mov = servico.Movimentar(codigo, tipo, quantidade, descricao);

            Console.WriteLine();
            Console.WriteLine($"Movimentação #{mov.Id} registrada ({mov.Tipo}): {mov.Descricao}");
            Console.WriteLine($"Quantidade final em estoque do produto {mov.CodigoProduto}: {mov.SaldoFinal}");
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
        {
            Console.WriteLine($"Não foi possível registrar: {ex.Message}");
        }
    }

    private static void ListarHistorico(ServicoEstoque servico)
    {
        if (servico.Historico.Count == 0)
        {
            Console.WriteLine("Nenhuma movimentação registrada ainda.");
            return;
        }

        Console.WriteLine();
        foreach (var m in servico.Historico)
        {
            Console.WriteLine(
                $"#{m.Id} | {m.DataHora:dd/MM/yyyy HH:mm} | Produto {m.CodigoProduto} | " +
                $"{m.Tipo} {m.Quantidade} | Saldo: {m.SaldoFinal} | {m.Descricao}");
        }
    }

    private static int LerInteiro(string mensagem)
    {
        while (true)
        {
            Console.Write(mensagem);
            if (int.TryParse(Console.ReadLine(), out int valor))
                return valor;

            Console.WriteLine("Digite um número inteiro válido.");
        }
    }
}
