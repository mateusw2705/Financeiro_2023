using Domain.Interfaces.ISistemaFinanceiro;
using Entities.Entidades;
using Infra.Configuracao;
using Infra.Repositorio.Generics;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositorio;

public class RepositorioSistemaFinanceiro(
    ContextBase context
) : RepositoryGenerics<SistemaFinanceiro>, InterfaceSistemaFinanceiro
{
    private readonly ContextBase _context = context;

    public async Task<bool> ExecuteCopiaDespesasSistemafinanceiro()
    {
        var listaSistemasFinanceiros = new List<SistemaFinanceiro>();

        try
        {
            listaSistemasFinanceiros = await _context.SistemaFinanceiro
                .Where(sistema => sistema.GerarCopiaDespesa)
                .AsNoTracking()
                .ToListAsync();

            foreach (var sistema in listaSistemasFinanceiros)
            {
                var dataAtual = DateTime.Now;
                var mesAtual = dataAtual.Month;
                var anoAtual = dataAtual.Year;

                var despesaJaExiste = await (
                    from despesa in _context.Despesa
                    join categoria in _context.Categoria on despesa.IdCategoria equals categoria.Id
                    where categoria.IdSistema == sistema.Id
                        && despesa.Mes == mesAtual
                        && despesa.Ano == anoAtual
                    select despesa.Id
                ).AnyAsync();

                if (!despesaJaExiste)
                {
                    var despesasParaCopiar = await (
                        from despesa in _context.Despesa
                        join categoria in _context.Categoria on despesa.IdCategoria equals categoria.Id
                        where categoria.IdSistema == sistema.Id
                            && despesa.Mes == sistema.MesCopia
                            && despesa.Ano == sistema.AnoCopia
                        select despesa
                    ).AsNoTracking().ToListAsync();

                    despesasParaCopiar.ForEach(despesa =>
                    {
                        despesa.DataVencimento = new DateTime(anoAtual, mesAtual, despesa.DataVencimento.Day);
                        despesa.Mes = mesAtual;
                        despesa.Ano = anoAtual;
                        despesa.DataAlteracao = DateTime.MinValue;
                        despesa.DataCadastro = dataAtual;
                        despesa.DataPagamento = DateTime.MinValue;
                        despesa.Pago = false;
                        despesa.Id = 0;
                    });

                    if (despesasParaCopiar.Any())
                    {
                        await _context.Despesa.AddRangeAsync(despesasParaCopiar);
                        await _context.SaveChangesAsync();
                    }
                }
            }
        }
        catch (Exception)
        {
            return false;
        }

        return true;
    }

    public async Task<IList<SistemaFinanceiro>> ListaSistemasUsuario(string emailUsuario)
    {
        var records =
            from sistemaFinanceiro in _context.SistemaFinanceiro.AsNoTracking()
            join usuarioSistema in _context.UsuarioSistemaFinanceiro.AsNoTracking() on sistemaFinanceiro.Id equals usuarioSistema.IdSistema
            where usuarioSistema.EmailUsuario.Equals(emailUsuario)
            select sistemaFinanceiro;

        return await records.ToListAsync();
    }
}
