using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using API.Data;
using API.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace API.Controllers
{
    [Route("v1/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UsuariosController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("registro")]
        public IActionResult Registro ([FromBody] Usuario usuario) {
            _context.Add(usuario);
            _context.SaveChanges();
            return Ok(new {msg = "Usuário cadastrado com sucesso"});
        }

        [HttpPost("Login")]
        public IActionResult Login ([FromBody] Usuario credenciais) {

            try {
                Usuario usuario = _context.Usuarios.First(user => user.Email.Equals(credenciais.Email));

                if (usuario != null) {
                    if (usuario.Senha.Equals(credenciais.Senha)){
                        string chaveDeSeguranca = "teste_chave_de_seguranca_api";
                        var chaveSimetrica = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(chaveDeSeguranca));
                        var credenciaisDeAcesso = new SigningCredentials(chaveSimetrica, SecurityAlgorithms.HmacSha256Signature);

                        var claims = new List<Claim>();
                        claims.Add(new Claim("id", usuario.Id.ToString()));
                        claims.Add(new Claim("email", usuario.Email));

                        var JWT = new JwtSecurityToken(
                            issuer: "testeapirest",
                            expires: DateTime.Now.AddHours(1),
                            audience: "usuario_comum",
                            signingCredentials: credenciaisDeAcesso
                        );

                        return Ok(new JwtSecurityTokenHandler().WriteToken(JWT));
                    } else {
                        Response.StatusCode = 401;
                        return new ObjectResult("");
                    }          
                } else {
                    Response.StatusCode = 401;
                    return new ObjectResult("");
                }

            } catch (Exception e){
                Response.StatusCode = 401;
                return new ObjectResult("");
            }
        }
    }
}