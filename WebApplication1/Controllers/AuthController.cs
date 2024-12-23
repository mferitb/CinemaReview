﻿using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models.ViewModels;
using WebApplication1.Data;
using WebApplication1.Models;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies; 

public class AuthController : Controller
{
    private readonly AppDbContext _context;

    public AuthController(AppDbContext context)
    {
        _context = context;
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
            // Veritabanında kullanıcıyı kontrol et
            var user = _context.Users.FirstOrDefault(u => u.Email == model.Email);

            if (user == null || user.PasswordHash != model.Password)
            {
                // Kullanıcı bulunamadı veya şifre yanlış
                ViewData["ErrorMessage"] = "Geçersiz kullanıcı adı veya şifre.";
                return View(model);
            }

            // Kullanıcı bulunduysa giriş işlemini gerçekleştir
            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email)
        };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

            return RedirectToAction("Index", "Home");
        }

        ViewData["ErrorMessage"] = "Geçersiz giriş denemesi.";
        return View(model);
    }



    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Register(RegisterViewModel model)
    {
        if (ModelState.IsValid)
        {
            if (_context.Users.Any(u => u.Email == model.Email))
            {
                ModelState.AddModelError("Email", "Bu e-posta adresi zaten kayıtlı.");
                return View(model);
            }

            var user = new User
            {
                Username = model.Username,
                Email = model.Email,
                PasswordHash = model.Password 
            };

            _context.Users.Add(user);
            _context.SaveChanges(); 

            return RedirectToAction("Login", "Auth");
        }

        return View(model); 
    }

    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }


}
