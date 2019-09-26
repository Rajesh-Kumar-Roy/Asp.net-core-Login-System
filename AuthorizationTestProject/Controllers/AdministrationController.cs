using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Claims;
using System.Threading.Tasks;
using AuthorizationTestProject.Models;
using AuthorizationTestProject.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Web.CodeGeneration;


namespace AuthorizationTestProject.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdministrationController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AdministrationController> _logger;
        public AdministrationController(RoleManager<IdentityRole> roleManager,UserManager<ApplicationUser> userManager,
                                             ILogger<AdministrationController> logger)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _logger = logger;
        }



        [HttpGet]
        public async Task<IActionResult> ManageUserClaim(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                ViewBag.ErrorMessage = $"User With id {userId} Can not found!";
                return View("NotFound");
            }

            var existingclaims = await _userManager.GetClaimsAsync(user);
            var model =new UserClaimViewModel
            {
                UserId = userId
            };
            foreach (var claim in ClaimStore.AllClaims)
            {
                UserCliam userCliam =new UserCliam
                {
                    ClaimType = claim.Type
                };
                //if the user has the claim, set isSelect Property is true,so the checkbox
                //next to the claim is checked on the other UI
                if (existingclaims.Any(c => c.Type == claim.Type))
                {
                    userCliam.IsSelected = true;
                }
                model.Cliams.Add(userCliam);
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ManageUserClaim(UserClaimViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with Id: {model.UserId} can not found!";
                return View("NotFound");
            }

            var claims = await _userManager.GetClaimsAsync(user);
            var result = await _userManager.RemoveClaimsAsync(user, claims);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Can not remove Existing Claims!");
                return View(model);
            }

            result = await _userManager.AddClaimsAsync(user,
            model.Cliams.Where(c=>c.IsSelected).Select(d=>new Claim(d.ClaimType,d.ClaimType)));
            if (!result.Succeeded)
            {
                ModelState.AddModelError("","Can not add to selected claim to user");
                return View(model);
            }

            return RedirectToAction("EditUser", new {Id = model.UserId});
        }



        [HttpGet]
        public async Task<IActionResult> ManageUserRole(string userId)
        {
            ViewBag.userId = userId;
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with id:{userId} Can not found!";
                return View("NotFound");
            }
            var model = new List<ManageUserRoleViewModel>();
            foreach (var role in _roleManager.Roles)
            {
                var viewModel = new ManageUserRoleViewModel
                {
                    RoleId = role.Id,
                    RoleName = role.Name

                };
                if (await _userManager.IsInRoleAsync(user, role.Name))
                {
                    viewModel.IsSelected = true;
                }
                else
                {
                    viewModel.IsSelected = false;
                }
                model.Add(viewModel);
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ManageUserRole(List<ManageUserRoleViewModel> model,string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with Id {userId} can not found";
                return View("NotFound");
            }

            var roles = await _userManager.GetRolesAsync(user);
            var result = await _userManager.RemoveFromRolesAsync(user, roles);

            if (!result.Succeeded)
            {
                ModelState.AddModelError("","can not remove user existing Roles!");
                return View(model);
            }

            result = await _userManager.AddToRolesAsync(user,
                model.Where(c=>c.IsSelected).Select(y=>y.RoleName));
            if (!result.Succeeded)
            {
                ModelState.AddModelError("","Can not select roles to user!");
                return View(model);
            }

            return RedirectToAction("EditUser", new {Id = userId});
        }

        [HttpGet]
        public async  Task<IActionResult> EditUser(string id)
        {
          var user= await _userManager.FindByIdAsync(id);
          if (user == null)
          {
              ViewBag.ErrorMessage($"User With Id: {id} can not found!");
              return View("NotFound");
          }

          var userCliams = await _userManager.GetClaimsAsync(user);
          var userRoles =await _userManager.GetRolesAsync(user);
          var model = new EditUserViewModel();
          model.Id = user.Id;
          model.Email = user.Email;
          model.UserName = user.UserName;
          model.City = user.City;
          model.Claims = userCliams.Select(c => c.Value).ToList();
          model.Roles = userRoles;


          return View(model);

        }

        [HttpPost]
        public async Task<IActionResult> EditUser(EditUserViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null)
            {
                ViewBag.ErrorMessage($"user with Id{model.Id} can not found!");
                return View("NotFound");
            }
            else
            {
              
                user.UserName = model.UserName;
                user.Email = model.Email;
                user.City = model.City;
                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    return RedirectToAction("ListUser");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("",error.Description);
                }

                return View(model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteRole(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                ViewBag.ErrorMessage($"Role with Id:{id} Can not found!");
                return View("NotFound");
            }
            else
            {
                try
                {
                    var result = await _roleManager.DeleteAsync(role);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("ListRole");
                    }

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }

                    return View("ListRole");
                }
                catch(DbUpdateException ex)
                {
                    _logger.LogError($"error Deleting Role {ex}");
                    ViewBag.ErrorTitle = $"{role.Name} is in use";
                    ViewBag.ErrorMessage =
                        $"{role.Name} you can not deleted as there user in this role. " +
                        $"If you want to delete this role, " +
                        $"please remove the user from this role,then try to Delete.";
                    return View("Error");
                }
            }
        }


        [HttpPost]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                ViewBag.ErrorMessage($"User with Id:{id} Can not found!");
                return View("NotFound");
            }
            else
            {
                var result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    return RedirectToAction("ListUser");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("",error.Description);
                }

                return View("ListUser");
            }
        }

        [HttpGet]
        public IActionResult ListUser()
        {
            var user = _userManager.Users;
            return View(user);
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

        [HttpGet]
        public async Task<IActionResult> EditUsersInRole(string roleId)
        {
            ViewBag.roleId = roleId;
            var role =await _roleManager.FindByIdAsync(roleId);
          
            if (role == null)
            {
                ViewBag.ErrorMessage($"Role with Id= {roleId} is Can not found!");
                return View("NotFound");
            }

            var model = new List<UserRoleViewModel>();
            var uanager = _userManager.Users;
            foreach (var user in uanager)
            {
                var userRoleviewModel = new UserRoleViewModel
                {
                    UserId = user.Id,
                    UserName = user.UserName
                };
                var checkrole= await _userManager.IsInRoleAsync(user, role.Name);
                if (checkrole)
                {
                    userRoleviewModel.IsSelected = true;
                }
                else
                {
                    userRoleviewModel.IsSelected = false;
                }
                model.Add(userRoleviewModel);
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditUsersInRole(List<UserRoleViewModel> model,string roleId)
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
            {
                ViewBag.ErrorMessage = $"Role with id:{roleId} can not found!";
                return View("NotFound");
            }

            for (int i = 0; i < model.Count; i++)
            {
             var user=  await _userManager.FindByIdAsync(model[i].UserId);
             IdentityResult result = null;
             if (model[i].IsSelected && !(await _userManager.IsInRoleAsync(user,role.Name)))
             {
                result= await _userManager.AddToRoleAsync(user, role.Name);
             }
             else if (!model[i].IsSelected && await _userManager.IsInRoleAsync(user, role.Name))
             {
               result= await _userManager.RemoveFromRoleAsync(user, role.Name);
             }
             else
             {
                 continue;
                 
             }

             if (result.Succeeded)
             {
                 if(i<(model.Count - 1)) 
                     continue;
                 else
                 {
                     return RedirectToAction("EditRole", new {Id = roleId});
                 }
             }
            }

            return RedirectToAction("EditRole", new {Id = roleId});
        }
    }
}