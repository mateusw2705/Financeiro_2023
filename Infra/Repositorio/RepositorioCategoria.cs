using Domain.Interfaces.ICategoria;
using Entities.Entidades;
using Infra.Configuracao;
using Infra.Repositorio.Generics;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositorio;

public class RepositorioCategoria(
    ContextBase context
) : RepositoryGenerics<Categoria>, InterfaceCategoria
{
    private readonly ContextBase _context = context;

    public async Task<IList<Categoria>> ListarCategoriasUsuario(string emailUsuario)
    {
        var record = from sistemaFinaceiro in _context.SistemaFinanceiro.AsNoTracking()
                     join categoria in _context.Categoria.AsNoTracking() on sistemaFinaceiro.Id equals categoria.IdSistema
                     join usuarioSistema in _context.UsuarioSistemaFinanceiro.AsNoTracking() on sistemaFinaceiro.Id equals usuarioSistema.IdSistema
                     where usuarioSistema.EmailUsuario.Equals(emailUsuario) && usuarioSistema.SistemaAtual
                     select categoria;

        return await  record.ToListAsync();
    }
}
