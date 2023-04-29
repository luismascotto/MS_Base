using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace MS_Base.Helpers;

public static class Extensoes
{
    /// <summary>
    /// Se passar minimo maior que o máximo, não valida nada e retorna o próprio número
    /// </summary>
    /// <param name="valor"></param>
    /// <param name="minimo"></param>
    /// <param name="maximo"></param>
    /// <returns></returns>
    public static int LimitarNaFaixa(this int valor, int minimo, int maximo)
    {
        if (minimo > maximo)
        {
            return valor;
        }
        if (valor < minimo)
        {
            return minimo;
        }
        if (valor > maximo)
        {
            return maximo;
        }
        return valor;
    }

    public static TaskAwaiter GetAwaiter(this TimeSpan timespan)
    {
        return Task.Delay(timespan).GetAwaiter();
    }
   
    public static TimeSpan Segundos(this int segundos)
    {
        return TimeSpan.FromSeconds(segundos);
    }

    public static TimeSpan Milissegundos(this int milissegundos)
    {
        return TimeSpan.FromMilliseconds(milissegundos);
    }
}
