using System.Globalization;

namespace CalculoJuros;

// Regra de negócio isolada. Recebe "hoje" como parâmetro para ser fácil de testar.
public static class CalculadoraJuros
{
    // 2,5% ao dia, juros simples (incide sempre sobre o valor original).
    public const decimal TaxaDiaria = 0.025m;

    public static int DiasEmAtraso(DateOnly vencimento, DateOnly hoje)
        => Math.Max(0, hoje.DayNumber - vencimento.DayNumber);

    public static decimal Calcular(decimal valor, DateOnly vencimento, DateOnly hoje)
    {
        if (valor <= 0)
            throw new ArgumentException("O valor deve ser maior que zero.");

        int dias = DiasEmAtraso(vencimento, hoje);

        return Math.Round(valor * TaxaDiaria * dias, 2, MidpointRounding.AwayFromZero);
    }
}

public class Program
{
    private static readonly CultureInfo PtBr = new("pt-BR");

    public static void Main(string[] args)
    {
        Console.WriteLine("===== CÁLCULO DE JUROS POR ATRASO (2,5% ao dia) =====");

        do
        {
            decimal valor = LerValor("Valor (ex.: 1000,50): ");
            DateOnly vencimento = LerData("Data de vencimento (dd/MM/aaaa): ");
            DateOnly hoje = DateOnly.FromDateTime(DateTime.Today);

            int dias = CalculadoraJuros.DiasEmAtraso(vencimento, hoje);
            decimal juros = CalculadoraJuros.Calcular(valor, vencimento, hoje);

            Console.WriteLine();
            Console.WriteLine($"Data de hoje:      {hoje:dd/MM/yyyy}");
            Console.WriteLine($"Dias em atraso:    {dias}");
            Console.WriteLine($"Valor original:    {valor.ToString("C2", PtBr)}");
            Console.WriteLine($"Juros:             {juros.ToString("C2", PtBr)}");
            Console.WriteLine($"Total a pagar:     {(valor + juros).ToString("C2", PtBr)}");

            if (dias == 0)
                Console.WriteLine("(Sem atraso: não há juros a cobrar.)");

            Console.WriteLine();
            Console.Write("Calcular outro? (s/n): ");
        }
        while (string.Equals(Console.ReadLine()?.Trim(), "s", StringComparison.OrdinalIgnoreCase));
    }

    private static decimal LerValor(string mensagem)
    {
        while (true)
        {
            Console.Write(mensagem);

            if (decimal.TryParse(Console.ReadLine(), NumberStyles.Number, PtBr, out decimal valor) && valor > 0)
                return valor;

            Console.WriteLine("Digite um valor numérico maior que zero. Use vírgula para os centavos.");
        }
    }

    private static DateOnly LerData(string mensagem)
    {
        while (true)
        {
            Console.Write(mensagem);

            if (DateOnly.TryParseExact(Console.ReadLine()?.Trim(), "dd/MM/yyyy", PtBr, DateTimeStyles.None, out DateOnly data))
                return data;

            Console.WriteLine("Data inválida. Use o formato dd/MM/aaaa.");
        }
    }
}
