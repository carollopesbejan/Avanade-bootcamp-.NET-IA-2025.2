namespace Estoque.API.DTOs
{
    // DTO para a requisição de reserva de estoque pelo serviço de Vendas.
    public class ReservaEstoqueDTO
    {
        public int ProdutoId { get; set; }
        public int QuantidadeSolicitada { get; set; }
    }
}