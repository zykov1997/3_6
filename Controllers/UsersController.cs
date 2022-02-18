using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Task3_5.Models;
using Task3_5.ViewModels;
using Microsoft.AspNetCore.Authorization;
using System;

namespace Task3_5.Controllers
{
    [Authorize]
    public class UsersController : Controller
    {
        private readonly SignInManager<User> _signInManager;
        UserManager<User> _userManager;

        public UsersController(UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public IActionResult Index()
        {
            return View(_userManager.Users.ToList());
        }

        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                User user = new User { Email = model.Email, UserName = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                User user = await _userManager.FindByIdAsync(model.Id);
                if (user != null)
                {
                    user.Email = model.Email;
                    user.UserName = model.Email;

                    var result = await _userManager.UpdateAsync(user);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                    }
                }
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string[] usersId)
        {
            if (usersId != null)
            {
                foreach (var u in usersId)
                {
                    var user = await _userManager.FindByIdAsync(u);
                    if (user != null)
                    {
                        await _userManager.DeleteAsync(user);
                    }
                    else
                    {
                        ModelState.AddModelError("", "User Not Found");
                    }
                }
                foreach (var u in usersId)
                {
                    await _signInManager.SignOutAsync();
                    return RedirectToAction("Index", "Home");
                }
            }
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> Block(string[] usersId)
        {
            if (usersId != null)
            {
                foreach (var u in usersId)
                {
                    var user = await _userManager.FindByIdAsync(u);
                    if (user != null)
                    {
                        user.Status = false;
                        user.LockoutEnabled = true;
                        user.LockoutEnd = DateTime.Now.AddYears(300);
                        await _userManager.UpdateAsync(user);
                    }
                    else
                    {
                        ModelState.AddModelError("", "User Not Found");
                    }

                }
                foreach (var u in usersId)
                {
                    var user = await _userManager.FindByIdAsync(u);
                    if (User.Identity.Name == user.UserName)
                    {
                        await _signInManager.SignOutAsync();
                        return RedirectToAction("Index", "Home");
                    }
                }
            }
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> UnBlock(string[] usersId)
        {
            if (usersId != null)
            {
                foreach (var u in usersId)
                {
                    var user = await _userManager.FindByIdAsync(u);
                    if (user != null)
                    {
                        user.Status = true;
                        user.LockoutEnabled = false;
                        user.LockoutEnd = DateTime.Now;
                        await _userManager.UpdateAsync(user);
                    }
                    else
                    {
                        ModelState.AddModelError("", "User Not Found");
                    }
                }
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                User user = await _userManager.FindByIdAsync(model.Id);
                if (user != null)
                {
                    var _passwordValidator =
                        HttpContext.RequestServices.GetService(typeof(IPasswordValidator<User>)) as IPasswordValidator<User>;
                    var _passwordHasher =
                        HttpContext.RequestServices.GetService(typeof(IPasswordHasher<User>)) as IPasswordHasher<User>;

                    IdentityResult result =
                        await _passwordValidator.ValidateAsync(_userManager, user, model.NewPassword);
                    if (result.Succeeded)
                    {
                        user.PasswordHash = _passwordHasher.HashPassword(user, model.NewPassword);
                        await _userManager.UpdateAsync(user);
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Пользователь не найден");
                }
            }
            return View(model);
        }
    }
}