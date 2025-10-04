using Estoque.API.Data;
using Estoque.API.Models;
using Estoque.API.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization; // <<== ADICIONADO PARA O ATRIBUTO [Authorize]


// Define a rota base da API: /api/Produtos
[Authorize] // <<== PROTEÇÃO: EXIGE UM TOKEN JWT VÁLIDO
[Route("api/[controller]")]
[ApiController]
public class ProdutosController : ControllerBase
{
    private readonly EstoqueContext _context;

    // INJEÇÃO DE DEPENDÊNCIA
    public ProdutosController(EstoqueContext context)
    {
        _context = context;
    }

    // --------------------------------------------------------------------------
    // 1. Cadastro de Produtos (POST /api/produtos)
    // --------------------------------------------------------------------------
    /// <summary> Cadastra um novo produto no estoque. </summary>
    [HttpPost]
    public async Task<ActionResult<Produto>> CadastrarProduto(Produto produto)
    {
        if (produto.QuantidadeEmEstoque < 0)
        {
             return BadRequest("A quantidade em estoque inicial não pode ser negativa.");
        }
        
        _context.Produtos.Add(produto);
        await _context.SaveChangesAsync();

        // Retorna 201 Created com a URI para o novo recurso
        return CreatedAtAction(nameof(ConsultarProdutoPorId), new { id = produto.Id }, produto);
    }

    // --------------------------------------------------------------------------
    // 2. Consulta de Produtos (GET /api/produtos e /api/produtos/{id})
    // --------------------------------------------------------------------------
    /// <summary> Retorna a lista completa de produtos. </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Produto>>> ConsultarProdutos()
    {
        return await _context.Produtos.ToListAsync();
    }

    /// <summary> Retorna um produto específico pelo ID. </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<Produto>> ConsultarProdutoPorId(int id)
    {
        var produto = await _context.Produtos.FindAsync(id);

        if (produto == null)
        {
            return NotFound($"Produto com ID {id} não encontrado.");
        }

        return produto;
    }
    
    // --------------------------------------------------------------------------
    // 3. Validação e Reserva de Estoque (POST /api/produtos/validar-reserva)
    // --------------------------------------------------------------------------
    /// <summary> Endpoint interno para o Microserviço de Vendas validar e reservar o estoque. </summary>
    [HttpPost("validar-reserva")] 
    public async Task<IActionResult> ValidarEReservarEstoque([FromBody] ReservaEstoqueDTO reservaDto)
    {
        if (reservaDto == null || reservaDto.QuantidadeSolicitada <= 0)
        {
            return BadRequest("Dados de reserva inválidos.");
        }

        var produto = await _context.Produtos.FirstOrDefaultAsync(p => p.Id == reservaDto.ProdutoId);

        if (produto == null)
        {
            return NotFound($"Produto com ID {reservaDto.ProdutoId} não encontrado.");
        }

        if (produto.QuantidadeEmEstoque < reservaDto.QuantidadeSolicitada)
        {
            // Retorna 400 Bad Request: estoque insuficiente
            return BadRequest($"Estoque insuficiente. Disponível: {produto.QuantidadeEmEstoque}.");
        }

        // Reserva: Reduz o estoque e salva no banco de dados.
        produto.QuantidadeEmEstoque -= reservaDto.QuantidadeSolicitada;
        await _context.SaveChangesAsync();

        return Ok(new { mensagem = "Estoque reservado com sucesso.", novoEstoque = produto.QuantidadeEmEstoque });
    }
}