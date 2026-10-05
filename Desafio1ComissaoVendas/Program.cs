using System.Globalization;
using System.Text.Json;

namespace ComissaoVendas;

// Modelos: representam o formato do JSON (em PHP seria um array associativo).
public record Venda(string Vendedor, decimal Valor);
public record ArquivoVendas(List<Venda> Vendas);

// Regra de negócio isolada, fácil de testar.
public static class CalculadoraComissao
{
    private const decimal LimiteSemComissao = 100m;
    private const decimal LimiteComissaoBaixa = 500m;
    private const decimal PercentualBaixo = 0.01m; // 1%
    private const decimal PercentualAlto = 0.05m;  // 5%

    public static decimal Calcular(decimal valorVenda)
    {
        if (valorVenda < LimiteSemComissao)
            return 0m;

        if (valorVenda < LimiteComissaoBaixa)
            return valorVenda * PercentualBaixo;

        // A partir de R$ 500,00 (inclusive)
        return valorVenda * PercentualAlto;
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        string caminho = args.Length > 0
            ? args[0]
            : Path.Combine(AppContext.BaseDirectory, "vendas.json");

        if (!File.Exists(caminho))
        {
            Console.WriteLine($"Arquivo não encontrado: {caminho}");
            return;
        }

        var opcoes = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var arquivo = JsonSerializer.Deserialize<ArquivoVendas>(File.ReadAllText(caminho), opcoes);

        if (arquivo?.Vendas is null || arquivo.Vendas.Count == 0)
        {
            Console.WriteLine("Nenhuma venda encontrada no arquivo.");
            return;
        }

        // Agrupa as vendas por vendedor e soma a comissão de cada venda.
        var comissoes = arquivo.Vendas
            .GroupBy(v => v.Vendedor)
            .Select(g => new
            {
                Vendedor = g.Key,
                Comissao = Math.Round(
                    g.Sum(v => CalculadoraComissao.Calcular(v.Valor)),
                    2,
                    MidpointRounding.AwayFromZero)
            });

        var ptBr = new CultureInfo("pt-BR");

        Console.WriteLine("Comissão por vendedor");
        Console.WriteLine(new string('-', 35));
        foreach (var item in comissoes)
        {
            Console.WriteLine($"{item.Vendedor,-18} {item.Comissao.ToString("C2", ptBr),15}");
        }
    }
}
