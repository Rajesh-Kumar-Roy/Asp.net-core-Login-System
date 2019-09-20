using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthorizationTestProject.Models;
using AuthorizationTestProject.ViewModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AuthorizationTestProject.Controllers
{
    public class AdministrationController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        public AdministrationController(RoleManager<IdentityRole> roleManager,UserManager<ApplicationUser> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult CreateRole()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> CreateRole(CreateRoleViewModel model)
        {
            if (ModelState.IsValid)
            {
                IdentityRole identity = new IdentityRole
                {
                    Name = model.RoleName
                };
                IdentityResult result = await _roleManager.CreateAsync(identity);
                if (result.Succeeded)
                {
                    return RedirectToAction("ListRole", "Administration");
                }

                foreach (IdentityError error in result.Errors)
                {
                    ModelState.AddModelError("",error.Description);
                }
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult ListRole()
        {
            var roles = _roleManager.Roles;
            return View(roles);
        }

        [HttpGet]
        public async Task< IActionResult> EditRole(string id)
        {
           var role= await _roleManager.FindByIdAsync(id);
           if (role == null)
           {
               ViewBag.ErrorMessage = $"Role with id {id} can not found!";
               return View("NotFound");
           }

           var model = new EditRoleViewModel
           {
               Id = role.Id,
               RoleName = role.Name
           };
           foreach (var user in _userManager.Users)
           {
              if(await _userManager.IsInRoleAsync(user, role.Name))
              {
                  model.User.Add(user.UserName);
              }
           }

           return View(model);

        }
        [HttpPost]
        public async Task<IActionResult> EditRole(EditRoleViewModel model)
        {
            var role = await _roleManager.FindByIdAsync(model.Id);
            if (role == null)
            {
                ViewBag.ErrorMessage = $"Role with id {model.Id} can not found!";
                return View("NotFound");
            }
            else
            {
                role.Name = model.RoleName;
              var result=  await _roleManager.UpdateAsync(role);
              if (result.Succeeded)
              {
                  return RedirectToAction("ListRole");
              }

              foreach (var error in result.Errors)
              {
                  ModelState.AddModelError("",error.Description);
              }
              return View(model);
            }
           

        }
    }
}