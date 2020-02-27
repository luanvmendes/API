using System;
using System.Collections.Generic;
using System.Linq;
using API.Data;
using API.HATEOAS;
using API.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("v1/[controller]")]
    [ApiController]
    [Authorize]
    public class ProdutosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private HATEOAS.HATEOAS HATEOAS;

        public ProdutosController(ApplicationDbContext context)
        {
            _context = context;
            HATEOAS = new HATEOAS.HATEOAS("localhost:5001/v1/produtos");
            HATEOAS.AddAction("GET_INFO", "GET");
            HATEOAS.AddAction("DELETE_PRODUCT", "DELETE");
            HATEOAS.AddAction("EDIT_PRODUCT", "PATCH");
        }

        [HttpGet]
        public IActionResult PegarProduto() {
            var produtos = _context.Produtos.ToList();
            List<ProdutoContainer> produtosHATEOAS = new List<ProdutoContainer>();
            foreach (var prod in produtos) {
                ProdutoContainer produtoHATEOAS = new ProdutoContainer();
                produtoHATEOAS.produto = prod;
                produtoHATEOAS.links = HATEOAS.GetActions(prod.Id.ToString());
                produtosHATEOAS.Add(produtoHATEOAS);
            }
            return Ok(produtosHATEOAS);
        }

        [HttpGet("{id}")]
        public IActionResult PegarProduto(int id) {
            try {
                var produto = _context.Produtos.First(p => p.Id == id);
                ProdutoContainer produtoHATEOAS = new ProdutoContainer();
                produtoHATEOAS.produto = produto;
                produtoHATEOAS.links = HATEOAS.GetActions(produto.Id.ToString());
                return Ok(new {produtoHATEOAS.produto, produtoHATEOAS.links});               
            } catch (Exception e) {
                Response.StatusCode = 404;

                return new ObjectResult ("");
            }
        }

        [HttpPost]
        public IActionResult Post([FromBody] ProdutoTemp p) {

            //Validação
            if (p.Preco <= 0) {
                Response.StatusCode = 400;
                return new ObjectResult (new {msg = "O preço do produto não pode ser menor ou igual a zero"});
            }

            if (p.Nome.Length <= 1) {
                Response.StatusCode = 400;
                return new ObjectResult (new {msg = "O nome do produto precisa ter mais de um caracter"});                
            }

            Produto produto = new Produto();
            produto.Nome = p.Nome;
            produto.Preco = p.Preco;
            _context.Add(produto);
            _context.SaveChanges();

            Response.StatusCode = 201;

            return new ObjectResult ("");
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id) {
            try {
                var produto = _context.Produtos.First(p => p.Id == id);
                _context.Remove(produto);
                _context.SaveChanges();
                return Ok();               
            } catch (Exception e) {

                Response.StatusCode = 404;

                return new ObjectResult ("");
            }
        }

        
        [HttpPatch]
        public IActionResult Patch([FromBody] Produto p) {
            if (p.Id > 0) {
                try {
                    var produto = _context.Produtos.First(pTemp => pTemp.Id == p.Id); 

                    if (produto != null ) {
                        // condição ? faz algo : senão 
                        produto.Nome = p.Nome != null ? p.Nome : produto.Nome;
                        produto.Preco = p.Preco != 0 ? p.Preco : produto.Preco;

                        _context.SaveChanges();

                        return Ok();

                    } else {
                        Response.StatusCode = 404;

                        return new ObjectResult ("Produto não encontrado");
                    }
                    
                } catch {
                    Response.StatusCode = 404;

                    return new ObjectResult ("Produto não encontrado");
                }
            } else {                  
                Response.StatusCode = 404;

                return new ObjectResult (new {msg = "Id inválido"});              
            }
        }
        public class ProdutoTemp
    {
        public string Nome { get; set; }
        public float Preco { get; set; }
    }
public class ProdutoContainer
    {
        public Produto produto;
        public Link[] links;
    }
    }
}

