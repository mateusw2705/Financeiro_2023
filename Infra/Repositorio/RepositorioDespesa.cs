using Domain.Interfaces.IDespesa;
using Entities.Entidades;
using Infra.Configuracao;
using Infra.Repositorio.Generics;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositorio
{
    public class RepositorioDespesa(
        ContextBase context
    ) : RepositoryGenerics<Despesa>, InterfaceDespesa
    {
        private readonly ContextBase _context = context;

        public async Task<IList<Despesa>> ListarDespesasUsuario(string emailUsuario)
        {
            var records = from sistemaFinanceiro in _context.SistemaFinanceiro.AsNoTracking()
                        join categoria in _context.Categoria.AsNoTracking() on sistemaFinanceiro.Id equals categoria.IdSistema
                        join usuarioSistema in _context.UsuarioSistemaFinanceiro.AsNoTracking() on sistemaFinanceiro.Id equals usuarioSistema.IdSistema
                        join despesa in _context.Despesa.AsNoTracking() on categoria.Id equals despesa.IdCategoria
                        where usuarioSistema.EmailUsuario.Equals(emailUsuario)
                              && sistemaFinanceiro.Mes == despesa.Mes
                              && sistemaFinanceiro.Ano == despesa.Ano
                        select despesa;

            return await records.ToListAsync();
        }

        public async Task<IList<Despesa>> ListarDespesasUsuarioNaoPagasMesesAnterior(string emailUsuario)
        {
            var records = from sistemaFinanceiro in _context.SistemaFinanceiro.AsNoTracking()
                        join categoria in _context.Categoria.AsNoTracking() on sistemaFinanceiro.Id equals categoria.IdSistema
                        join usuarioSistema in _context.UsuarioSistemaFinanceiro.AsNoTracking() on sistemaFinanceiro.Id equals usuarioSistema.IdSistema
                        join despesa in _context.Despesa.AsNoTracking() on categoria.Id equals despesa.IdCategoria
                        where usuarioSistema.EmailUsuario.Equals(emailUsuario)
                              && despesa.Mes < DateTime.Now.Month
                              && !despesa.Pago
                        select despesa;

            return await records.ToListAsync();
        }
    }
}
