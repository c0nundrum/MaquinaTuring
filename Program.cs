using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MaquinaTuring
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var lista = new List<string> { "a", "b", "c" };

            Permutacoes.Permutar(lista, 3);

            await MaquinaTuring.Executar();
        }
    }
}
