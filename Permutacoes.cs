using System;
using System.Collections.Generic;
using System.Linq;

namespace MaquinaTuring
{
    class Permutacoes
    {
        private static IEnumerable<IEnumerable<T>> GetPermutacoes<T>(IEnumerable<T> lista, int tamanho)
        {
            if (tamanho == 1)
                return lista.Select(_palavra => new T[] { _palavra });

            return GetPermutacoes(lista, tamanho - 1).SelectMany(palavra => lista, (p1, p2) => p1.Concat(new T[] { p2 }));
        }

        public static void Permutar(List<string> lista, int tamanhoMaximoPalavras)
        {            
            GetListaPalavras(lista, tamanhoMaximoPalavras);
        }

        private static void GetListaPalavras(List<string> lista, int tamanhoMaximoPalavras)
        {
            for (int i = 1; i <= tamanhoMaximoPalavras; i++)
            {
                var permutacoes = GetPermutacoes(lista, i);

                PrintLinha(permutacoes);
            }
        }

        private static void PrintLinha(IEnumerable<IEnumerable<string>> permutacoes)
        {
            foreach (var palavra in permutacoes)
            {
                string _palavra = "";
                foreach (var item in palavra)
                {
                    _palavra += item.ToString();
                }

                Console.WriteLine(_palavra);
            }
        }
    }
}
