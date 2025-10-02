using System.ComponentModel.DataAnnotations;

namespace Estoque.API.Models
{
    // Documentação: Representa o item de estoque e suas características básicas.
    public class Produto
    {
        // Chave primária do Entity Framework.
        [Key]
        public int Id { get; set; }

        // Nome do produto (obrigatório).
        [Required]
        [MaxLength(100)]
        public string Nome { get; set; }

        // Descrição do produto.
        public string Descricao { get; set; }

        // Preço unitário do produto.
        public decimal Preco { get; set; }

        // Quantidade disponível em estoque. CRUCIAL para o controle de estoque.
        public int QuantidadeEmEstoque { get; set; }
    }
}