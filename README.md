# Desafio de Desenvolvimento — C#

![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=csharp&logoColor=white)
![Platform](https://img.shields.io/badge/Console_App-Cross_Platform-informational?style=for-the-badge)
![License](https://img.shields.io/badge/License-MIT-blue.svg?style=for-the-badge)

Solução dos três exercícios do desafio técnico, desenvolvida em **C# (.NET 10)** como aplicações de console.

## Sobre a escolha da tecnologia

O enunciado não definia a linguagem. Como a vaga pede conhecimentos em C#, escolhi essa stack para o desafio.

Minha experiência principal é com **PHP**. Aprendi o necessário de C# para resolver este desafio e fiz questão de aplicar boas práticas como tipagem forte, separação entre regra de negócio e interface, e uso do tipo `decimal` para valores monetários. Estou aberto(a) a feedbacks sobre o código.

## Estrutura do repositório

```
/Desafio1ComissaoVendas          -> Exercício 1: comissão por vendedor
/Desafio2EstoqueMovimentacao     -> Exercício 2: movimentação de estoque
/Desafio3CalculoJuros            -> Exercício 3: juros por atraso
```

Cada pasta é um projeto independente, com seu próprio `.csproj`.

## Requisitos

- [.NET SDK 10](https://dotnet.microsoft.com/download) (os projetos usam `net10.0`)

Para conferir a versão instalada: `dotnet --version`

## Como executar

Em cada pasta de exercício, rode:

```
dotnet run
```

---

## Exercício 1 — Comissão de vendas

Lê o arquivo `vendas.json` e calcula a comissão total de cada vendedor.

**Regras de comissão (aplicadas a cada venda):**

| Valor da venda | Comissão |
|---|---|
| Abaixo de R$ 100,00 | 0% |
| De R$ 100,00 até R$ 499,99 | 1% |
| A partir de R$ 500,00 | 5% |

**Decisões:**

- A regra é aplicada **venda a venda**, e não sobre o total do vendedor.
- Os limites são tratados assim: R$ 100,00 gera 1% e R$ 500,00 gera 5%.
- Uso de `decimal` para evitar erros de arredondamento do ponto flutuante.
- O arredondamento (2 casas) é feito uma única vez, sobre o total de cada vendedor.
- É possível informar outro arquivo: `dotnet run -- caminho/outro.json`

**Resultado esperado com o arquivo fornecido:**

| Vendedor | Comissão |
|---|---|
| João Silva | R$ 495,68 |
| Maria Souza | R$ 465,95 |
| Carlos Oliveira | R$ 379,37 |
| Ana Lima | R$ 404,98 |

## Exercício 2 — Movimentação de estoque

Programa com menu que lê o estoque inicial de `estoque.json` e permite:

1. Listar o estoque
2. Registrar entrada de mercadoria
3. Registrar saída de mercadoria
4. Consultar o histórico de movimentações

Cada movimentação recebe um **identificador único**, o tipo (Entrada ou Saída), a quantidade, uma **descrição** informada pelo usuário e a data/hora. Ao final de cada lançamento, o programa mostra a **quantidade final em estoque** do produto.

**Validações:**

- Produto inexistente
- Quantidade igual ou menor que zero
- Descrição da movimentação vazia
- Saída maior que o saldo disponível (o estoque não fica negativo)
- Entradas inválidas no menu e nos campos numéricos não derrubam o programa

**Decisões e limitações:**

- O identificador da movimentação é um número sequencial gerado em memória. Em um sistema real, seria a chave primária da tabela ou um `Guid`.
- Os dados ficam **em memória**: ao fechar o programa, o estoque volta ao valor do JSON e o histórico é perdido.
- A regra de negócio está na classe `ServicoEstoque`, separada do menu.

## Exercício 3 — Juros por atraso

Recebe um valor e uma data de vencimento e calcula os juros considerando a data de hoje, com taxa de **2,5% ao dia**.

```
juros = valor × 2,5% × dias em atraso
```

**Decisões:**

- O enunciado menciona "multa" e não define se os juros são simples ou compostos. Adotei **juros simples**, sempre sobre o valor original.
- Se o vencimento é hoje ou uma data futura, não há atraso e os juros são zero.
- A data de hoje é passada como parâmetro para o método de cálculo, o que facilita testes automatizados com datas fixas.
- Valores monetários em `decimal`; datas com `DateOnly`.
- Informe o valor com vírgula para os centavos (ex.: `1000,50`) e a data no formato `dd/MM/aaaa`.

**Exemplos (considerando hoje = 05/10/2026):**

| Valor | Vencimento | Dias em atraso | Juros | Total |
|---|---|---|---|---|
| R$ 1.000,00 | 01/10/2026 | 4 | R$ 100,00 | R$ 1.100,00 |
| R$ 200,00 | 25/09/2026 | 10 | R$ 50,00 | R$ 250,00 |
| R$ 500,00 | 05/10/2026 | 0 | R$ 0,00 | R$ 500,00 |

---

## Possíveis melhorias

- Testes unitários (xUnit) para as regras de comissão, estoque e juros
- Persistência do estoque e do histórico em **SQL Server**
- Tratamento de concorrência nas movimentações de estoque (transações)
- Separar as classes em arquivos próprios e reunir os projetos em uma única solution (`.sln`)
- Expor as funcionalidades como API REST

## Autor

**Andreney Santos** — [LinkedIn](https://www.linkedin.com/in/andreney-laranjeira-dos-santos) — andreney@gmail.com
