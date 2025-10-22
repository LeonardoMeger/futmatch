using FutMatchApp.Data;
using FutMatchApp.Models.ViewModels;
using FutMatchApp.Models;
using FutMatchApp.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace FutMatchApp.Controllers
{
    public class AccountController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly JwtService _jwtService;
        private readonly IEmailService _emailService;

        public AccountController(
            ApplicationDbContext context,
            JwtService jwtService,
            IEmailService emailService) : base(context)
        {
            _context = context;
            _jwtService = jwtService;
            _emailService = emailService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _context.Users
                    .Include(u => u.SelectedTeam)
                    .FirstOrDefaultAsync(u => u.Email == model.Email);

                if (user != null && VerifyPassword(model.Senha, user.Senha))
                {
                    // Gerar token JWT
                    var token = _jwtService.GenerateToken(user);

                    // Armazenar token em cookie (opcional, para uso híbrido)
                    Response.Cookies.Append("jwt", token, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict,
                        Expires = DateTimeOffset.UtcNow.AddDays(7)
                    });

                    // Autenticação tradicional com cookies
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                        new Claim(ClaimTypes.Name, user.Nome),
                        new Claim(ClaimTypes.Email, user.Email)
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = model.LembrarMe
                    };

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);

                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError("", "Email ou senha inválidos");
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == model.Email);

                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "Este email já está cadastrado");
                    return View(model);
                }

                var user = new User
                {
                    Nome = model.Nome,
                    Email = model.Email,
                    Senha = HashPassword(model.Senha),
                    Telefone = model.Telefone,
                    Cidade = model.Cidade,
                    Estado = model.Estado,
                    CEP = model.CEP
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Cadastro realizado com sucesso! Faça login para continuar.";
                return RedirectToAction("Login");
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == model.Email);

                if (user != null)
                {
                    // Gerar token de reset
                    var resetToken = _jwtService.GeneratePasswordResetToken();
                    user.PasswordResetToken = resetToken;
                    user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(24);

                    await _context.SaveChangesAsync();

                    // Criar link de reset
                    var resetLink = Url.Action(
                        "ResetPassword",
                        "Account",
                        new { token = resetToken, email = user.Email },
                        Request.Scheme);

                    // Enviar email
                    await _emailService.SendPasswordResetEmailAsync(user.Email, resetLink);
                }

                // Sempre mostrar mensagem de sucesso (segurança)
                TempData["Success"] = "Se o email existir em nossa base, você receberá um link para redefinir sua senha.";
                return RedirectToAction("Login");
            }

            return View(model);
        }

        [HttpGet]
        [Authorize] // Requer autenticação
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [Authorize] // Requer autenticação
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var user = await _context.Users.FindAsync(userId);

                if (user == null)
                {
                    return RedirectToAction("Login");
                }

                // Verificar se a senha atual está correta
                if (!VerifyPassword(model.SenhaAtual, user.Senha))
                {
                    ModelState.AddModelError("SenhaAtual", "Senha atual incorreta");
                    return View(model);
                }

                // Atualizar senha
                user.Senha = HashPassword(model.NovaSenha);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Senha alterada com sucesso!";
                return RedirectToAction("Profile", "Account"); // ou outra página
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> ResetPassword(string token, string email)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Login");
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email && u.PasswordResetToken == token);

            if (user == null || user.PasswordResetTokenExpiry < DateTime.UtcNow)
            {
                TempData["Error"] = "Link de redefinição inválido ou expirado.";
                return RedirectToAction("Login");
            }

            var model = new ResetPasswordViewModel
            {
                Token = token,
                Email = email
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == model.Email &&
                                            u.PasswordResetToken == model.Token);

                if (user == null || user.PasswordResetTokenExpiry < DateTime.UtcNow)
                {
                    ModelState.AddModelError("", "Link de redefinição inválido ou expirado.");
                    return View(model);
                }

                // Atualizar senha
                user.Senha = HashPassword(model.NovaSenha);
                user.PasswordResetToken = null;
                user.PasswordResetTokenExpiry = null;

                await _context.SaveChangesAsync();

                TempData["Success"] = "Senha redefinida com sucesso! Faça login com sua nova senha.";
                return RedirectToAction("Login");
            }

            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            Response.Cookies.Delete("jwt");
            return RedirectToAction("Index", "Home");
        }

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        private bool VerifyPassword(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
    }
}