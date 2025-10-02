using Estoque.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Estoque.API.Data
{
    // Documentação: Gerencia a conexão com o DB e o mapeamento da entidade Produto.
    public class EstoqueContext : DbContext
    {
        public EstoqueContext(DbContextOptions<EstoqueContext> options) : base(options)
        {
        }

        // Representa a tabela 'Produtos' no banco de dados.
        public DbSet<Produto> Produtos { get; set; }
    }
}