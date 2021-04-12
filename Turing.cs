using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MaquinaTuring
{
    public class MaquinaTuring
    {
        public const int Esq = -1,
                         Dir = 1,
                         Pula = 0;
            

        private readonly Fita fita;
        private readonly HashSet<string> estadosFinais;
        private Dictionary<(string estado, char le), (char escreve, int move, string paraEstado)> transicoes;

        public int Passos { get; private set; }
        public string Estado { get; private set; }
        public bool Sucesso => estadosFinais.Contains(Estado);
        public int TamanhoFita => fita.TamanhoTotal;
        public string FitaToString => fita.ToString();

        public static async Task Executar()
        {
            void PrintEstadoAtual(MaquinaTuring tm) => Console.WriteLine(tm.FitaToString + "\tEstado " + tm.Estado);

            void PrintResultado(MaquinaTuring tm)
            {
                Console.WriteLine($"Estado final: {tm.Estado} = {(tm.Sucesso ? "Sucesso" : "Falha")}");
                Console.WriteLine(tm.Passos + " loops");
                Console.WriteLine("Tamanho da fita: " + tm.TamanhoFita);
                Console.WriteLine();
            }

            var cincoEstados = new MaquinaTuring("A", '0', "H")
                .Transicoes(
                    ("A", '0', '1', Dir, "B"),
                    ("A", '1', '1', Esq, "C"),
                    ("B", '0', '1', Dir, "C"),
                    ("B", '1', '1', Dir, "B"),
                    ("C", '0', '1', Dir, "D"),
                    ("C", '1', '0', Esq, "E"),
                    ("D", '0', '1', Esq, "A"),
                    ("D", '1', '1', Esq, "D"),
                    ("E", '0', '1', Pula, "H"),
                    ("E", '1', '0', Esq, "A")
            );
            var tarefa = cincoEstados.TimeAsync();

            var incrementer = new MaquinaTuring("q0", 'B', "qf").Transicoes(("q0", '1', '1', Dir, "q0"),("q0", 'B', '1', Pula, "qf")).Input("111");
            foreach (var _ in incrementer.Exec())
            {
                PrintEstadoAtual(incrementer);
            }

            PrintResultado(incrementer);

            var tresEstados = new MaquinaTuring("a", '0', "parar").Transicoes(
                ("a", '0', '1', Dir, "b"),
                ("a", '1', '1', Esq, "c"),
                ("b", '0', '1', Esq, "a"),
                ("b", '1', '1', Dir, "b"),
                ("c", '0', '1', Esq, "b"),
                ("c", '1', '1', Pula, "parar")
            );
            foreach (var _ in tresEstados.Exec())
            {
                PrintEstadoAtual(tresEstados);
            }

            PrintResultado(tresEstados);
           
            Console.WriteLine(await tarefa);
            PrintResultado(cincoEstados);
        }

        public MaquinaTuring(string estadoInicial, char simboloVazio, params string[] estadoFinal)
        {
            Estado = estadoInicial;
            fita = new Fita(simboloVazio); //TODO - adicionar opção para automato de pilha aqui;
            this.estadosFinais = estadoFinal.ToHashSet();
        }

        public MaquinaTuring Transicoes(
            params (string estado, char lido, char escrever, int move, string paraEstado)[] transitions)
        {
            this.transicoes = transitions.ToDictionary(k => (k.estado, k.lido), k => (k.escrever, k.move, k.paraEstado));
            return this;
        }

        public MaquinaTuring Input(string input)
        {
            fita.Input(input);
            return this;
        }

        public IEnumerable<string> Exec()
        {
            yield return Estado;

            while (Passo())
            {
                yield return Estado;
            }
        }

        public async Task<TimeSpan> TimeAsync(CancellationToken cancel = default)
        {
            var chrono = Stopwatch.StartNew();
            await RunAsync(cancel);
            chrono.Stop();
            return chrono.Elapsed;
        }

        public Task RunAsync(CancellationToken cancel = default)
            => Task.Run(() => {
                while (Passo()) cancel.ThrowIfCancellationRequested();
            }, cancel);

        private bool Passo()
        {
            if (!transicoes.TryGetValue((Estado, fita.Atual), out var acao)) 
                return false;
            fita.Atual = acao.escreve;
            fita.Move(acao.move);
            Estado = acao.paraEstado;
            Passos++;
            return true;
        }


        private class Fita
        {
            private readonly List<char> fitaDireita = new();
            private readonly List<char> fitaEsquerda = new();
            private int cabeca = 0;
            private readonly char vazio;

            public Fita(char simboloVazio) => fitaDireita.Add(vazio = simboloVazio);

            public void Reset()
            {
                fitaEsquerda.Clear();
                fitaDireita.Clear();
                cabeca = 0;
                fitaDireita.Add(vazio);
            }

            public void Input(string input)
            {
                Reset();
                fitaDireita.Clear();
                fitaDireita.AddRange(input);
            }

            public void Move(int direction)
            {
                cabeca += direction;

                if (cabeca >= 0 && fitaDireita.Count <= cabeca) 
                    fitaDireita.Add(vazio);

                if (cabeca < 0 && fitaEsquerda.Count <= ~cabeca) 
                    fitaEsquerda.Add(vazio);
            }

            public char Atual
            {
                get => cabeca < 0 ? fitaEsquerda[~cabeca] : fitaDireita[cabeca];
                set
                {
                    if (cabeca < 0) 
                        fitaEsquerda[~cabeca] = value;
                    else 
                        fitaDireita[cabeca] = value;
                }
            }

            public int TamanhoTotal => fitaEsquerda.Count + fitaDireita.Count;

            public override string ToString()
            {
                int h = (cabeca < 0 ? ~cabeca : fitaEsquerda.Count + cabeca) * 2 + 1;

                var builder = new StringBuilder(" ", TamanhoTotal * 2 + 1);

                if (fitaEsquerda.Count > 0)
                {
                    builder.Append(string.Join(" ", fitaEsquerda)).Append(' ');

                    if (cabeca < 0) 
                        (builder[h + 1], builder[h - 1]) = ('(', ')');

                    for (int l = 0, r = builder.Length - 1; l < r; l++, r--) 
                        (builder[l], builder[r]) = (builder[r], builder[l]);
                }

                _ = builder.Append(string.Join(" ", fitaDireita)).Append(' ');

                if (cabeca >= 0) 
                    (builder[h - 1], builder[h + 1]) = ('(', ')');

                return builder.ToString();
            }

        }

    }
}
