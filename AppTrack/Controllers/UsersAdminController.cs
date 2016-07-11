using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using AppTrack.Models;
using AppTrack.SharedModels;
using AppTrack.Helpers;
using System.Data.Entity.Core.Objects;
namespace AppTrack.Controllers
{
    [AuthorizeAdminRedirect(Roles = Constants.adminRoles)]
    public class UsersAdminController : BaseController
    {

        private DevProvidentIDOCEntities db = new DevProvidentIDOCEntities();

        public UsersAdminController()
        {
        }

        public UsersAdminController(ApplicationUserManager userManager, ApplicationRoleManager roleManager)
        {
            UserManager = userManager;
            RoleManager = roleManager;
        }

        private ApplicationUserManager _userManager;
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        private ApplicationRoleManager _roleManager;
        public ApplicationRoleManager RoleManager
        {
            get
            {
                return _roleManager ?? HttpContext.GetOwinContext().Get<ApplicationRoleManager>();
            }
            private set
            {
                _roleManager = value;
            }
        }

        //
        // GET: /Users/
        [AuthorizeAdminRedirect(Roles = "Admin")]
        public ActionResult Index()
        {
            var thisAdminID = AdminID.ToString();

            string sqlCommand = "exec dbo.[LB_GetAdminUserList] " + thisAdminID; 

            var AdminUserRows = db.Database.SqlQuery<AdminUser>(sqlCommand).ToList();

            return View(AdminUserRows);

        }

        //
        // GET: /Users/Details/5
        // Note this is used by all Admin users to view the details of their account
        public async Task<ActionResult> Details(string id)
        {
            if (id == null)
            {
                var thisAdminID = AdminID.ToString();

                string sqlCommand = "exec dbo.[LB_GetAdminUserByID] " + thisAdminID;

                var thisAdminUser = db.Database.SqlQuery<AdminUser>(sqlCommand).FirstOrDefault();

                if (thisAdminUser == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                else
                {
                    id = thisAdminUser.ID;
                }
            }
            var user = await UserManager.FindByIdAsync(id);

            ViewBag.RoleNames = await UserManager.GetRolesAsync(user.Id);

            string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
            ViewBag.callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);		

            return View(user);
        }

        //
        // GET: /Users/Create
        [AuthorizeAdminRedirect(Roles = "Admin")]
        public async Task<ActionResult> Create()
        {
            //Get the list of Roles
            ViewBag.RoleId = new SelectList(await RoleManager.Roles.ToListAsync(), "Name", "Name");
            return View();
        }

        //
        // POST: /Users/Create
        [AuthorizeAdminRedirect(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult> Create(RegisterViewModel userViewModel, params string[] selectedRoles)
        {
            if (ModelState.IsValid)
            {

                ObjectParameter returnAdminID = new ObjectParameter("returnAdminID", typeof(int));
                ObjectParameter returnMessage = new ObjectParameter("returnMessage", typeof(string));

                db.LB_InsertAdmin(userViewModel.DisplayName, userViewModel.Email, returnAdminID, returnMessage);

                var scalarAdminID = (int)returnAdminID.Value;
                var errorMessage = (string)returnMessage.Value ?? "";

                if (scalarAdminID == -1)
                {
                    ModelState.AddModelError("", errorMessage);
                }
                else
                {
                    var user = new ApplicationUser { UserName = userViewModel.Email, Email = userViewModel.Email, DisplayName = userViewModel.DisplayName, AdminID = scalarAdminID };
                    var adminresult = await UserManager.CreateAsync(user, userViewModel.Password);

                    //Add User to the selected Roles 
                    if (adminresult.Succeeded)
                    {
                        if (selectedRoles != null)
                        {
                            var result = await UserManager.AddToRolesAsync(user.Id, selectedRoles);
                            if (!result.Succeeded)
                            {
                                ModelState.AddModelError("", result.Errors.First());
                                ViewBag.RoleId = new SelectList(await RoleManager.Roles.ToListAsync(), "Name", "Name");
                                return View();
                            }
                        }
                        else
                        {
                            ModelState.AddModelError("", adminresult.Errors.First());
                            ViewBag.RoleId = new SelectList(RoleManager.Roles, "Name", "Name");
                            return View();
                        }
                        return RedirectToAction("Index");
                    }
                    else
                    {

                        db.LB_DeleteAdminUser(scalarAdminID);
                        ModelState.AddModelError("", adminresult.Errors.First());
                        ViewBag.RoleId = new SelectList(RoleManager.Roles, "Name", "Name");
                        return View();


                    }
                }
            } 

            ViewBag.RoleId = new SelectList(RoleManager.Roles, "Name", "Name");
            return View();
        }

        //
        // GET: /Users/Edit/1
        [AuthorizeAdminRedirect(Roles = "Admin")]
        public async Task<ActionResult> Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var user = await UserManager.FindByIdAsync(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            var userRoles = await UserManager.GetRolesAsync(user.Id);

            return View(new EditUserViewModel()
            {
                Id = user.Id,
                DisplayName = user.DisplayName,
                Email = user.Email,
                RolesList = RoleManager.Roles.ToList().Select(x => new SelectListItem()
                {
                    Selected = userRoles.Contains(x.Name),
                    Text = x.Name,
                    Value = x.Name
                })
            });
        }

        //
        // POST: /Users/Edit/5
        [AuthorizeAdminRedirect(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "DisplayName,Email,Id")] EditUserViewModel editUser, params string[] selectedRoles)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByIdAsync(editUser.Id);
                if (user == null)
                {
                    return HttpNotFound();
                }

                var userRoles = await UserManager.GetRolesAsync(user.Id);
                bool isUpdateNeeded = false;

                if (user.DisplayName != editUser.DisplayName)
                {
                    isUpdateNeeded = true;
                    user.DisplayName = editUser.DisplayName;
                }
                if (user.Email != editUser.Email)
                {
                    isUpdateNeeded = true;
                    user.Email = editUser.Email;
                    user.UserName = editUser.Email;
                }

                var RolesList = RoleManager.Roles.ToList().Select(x => new SelectListItem()
                {
                    Selected = selectedRoles.Contains(x.Name),
                    Text = x.Name,
                    Value = x.Name
                });
                editUser.RolesList = RolesList;

                var result = await UserManager.AddToRolesAsync(user.Id, selectedRoles.Except(userRoles).ToArray<string>());


                if (result.Succeeded)
                {
                    if (isUpdateNeeded)
                    {
                        // update c_Info with new displayname, email and usersname
                        //Instantiate the UserManager in ASP.Identity system so you can look up the user in the system
                        var manager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));
                        //Get the User object
                        var currentUser = manager.FindById(user.Id);
                        //The currentUser object has properties that match the columns in the AspNetUsers table
                        // Note when assigning nullable column to int variable the ?? say set it to 0 if it is null
                        AdminID = currentUser.AdminID ?? 0;
                        if (AdminID != 0)
                        {
                            try
                            {
                                ObjectParameter returnAdminID = new ObjectParameter("returnAdminID", typeof(int));
                                ObjectParameter returnMessage = new ObjectParameter("returnMessage", typeof(string));

                                db.LB_UpdateAdmin(AdminID, user.DisplayName, user.Email, returnAdminID, returnMessage);

                                var scalarCustID = (int)returnAdminID.Value;
                                var errorMessage = (string)returnMessage.Value ?? "";

                                if (scalarCustID == -1)
                                {
                                    ModelState.AddModelError("", errorMessage);
                                    return View(editUser);
                                }
                            }
                            catch
                            {
                                ModelState.AddModelError("", "Error encountered updating Admin user account");
                                return View(editUser);
                            }
                        }
                    }
                }
                else
                {
                    // Hacky workaround becuase we don't maintain username and if we change the email and thus the username
                    // and the edit check will fail and the first error message will say Name xyz is already taken and we 
                    // want to override to say email is taken

                    string eMsg = result.Errors.First().ToString();

                    if (eMsg.Contains("Name " + user.Email + " is already taken"))
                    {
                        ModelState.AddModelError("", "Email " + user.Email + " is already taken");
                    }
                    else
                    {
                        ModelState.AddModelError("", result.Errors.First());
                    }
                    return View(editUser);
                }
                result = await UserManager.RemoveFromRolesAsync(user.Id, userRoles.Except(selectedRoles).ToArray<string>());

                if (!result.Succeeded)
                {
                    ModelState.AddModelError("", result.Errors.First());
                    return View(editUser);
                }
                return RedirectToAction("Index");
            }
            ModelState.AddModelError("", "Error validating input information");
            return View(editUser);
        }

        //
        // GET: /Users/Delete/5
        [AuthorizeAdminRedirect(Roles = "Admin")]
        public async Task<ActionResult> Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var user = await UserManager.FindByIdAsync(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        //
        // POST: /Users/Delete/5
        [AuthorizeAdminRedirect(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByIdAsync(id);
                if (user == null)
                {
                    ModelState.AddModelError("", "Error encountered - User not found");
                    return View(user);
                }
                try
                {
                    ObjectParameter returnAdminID = new ObjectParameter("returnAdminID", typeof(int));
                    ObjectParameter returnMessage = new ObjectParameter("returnMessage", typeof(string));

                    db.LB_DeleteAdmin(user.Id, returnAdminID, returnMessage);

                    var scalarCustID = (int)returnAdminID.Value;
                    var errorMessage = (string)returnMessage.Value ?? "";

                    if (scalarCustID == -1)
                    {
                        ModelState.AddModelError("", errorMessage);
                        return View(user);
                    }
                    return RedirectToAction("Index");
                }
                catch
                {
                    ModelState.AddModelError("", "Error encountered - User not found");
                    return View(user);
                }
            }
            return RedirectToAction("Delete","Account",id);
        }
    }
}
