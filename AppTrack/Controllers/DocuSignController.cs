using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Data.SqlClient;
using System.Data.Entity.Core.Objects;

using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;

using System.Web;

using AppTrack.Models;
using AppTrack.SharedModels;
using AppTrack.ViewModels;
using AppTrack.Helpers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

// install DocuSign NuGet package or download source from GitHub:  
// https://www.nuget.org/packages/DocuSign.Integration.Client.dll/
using DocuSign.Integrations.Client;
using Newtonsoft.Json.Linq;

namespace AppTrack.Controllers
{
        
    [AuthorizeAdminRedirect(Roles = "MemberServices")]
    public class DocuSignController : BaseController
    {
        private DevProvidentIDOCEntities db = new DevProvidentIDOCEntities();
        DataHelpers DataHelper = new DataHelpers();

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        // SendDocument : GET
        [HttpGet]
        public ActionResult SendDocument(int CustID, int ID)
        {
            // Use the MemberVendorRequirementID then get the Requirement ID from that.  Use the MemberVendorRequirement ID to update status
            ViewBag.DocuSignError = "";
            int VendorID = 0;

            //=======================================================================================================================
            // STEP 1: Login API 
            //=======================================================================================================================

            if (Constants.isLive)
            {
                RestSettings.Instance.DocuSignAddress = "https://www.docusign.net";
                RestSettings.Instance.WebServiceUrl = RestSettings.Instance.DocuSignAddress + "/restapi/v2";
                RestSettings.Instance.IntegratorKey = "AppTrack-8724bcb9-3339-47e4-ac15-b116ae4cc365";
            }
            else
            {
                RestSettings.Instance.DocuSignAddress = "https://demo.docusign.net";
                RestSettings.Instance.WebServiceUrl = RestSettings.Instance.DocuSignAddress + "/restapi/v2";
                RestSettings.Instance.IntegratorKey = "AppTrack-8724bcb9-3339-47e4-ac15-b116ae4cc365";
            }
            Account account = new Account();
            // Might be user id and password or email and password
            //            account.UserId = "14f01e9d-6c28-49f4-8d95-2bee74d60173";
            //            account.ApiPassword = "d1In3auePoJfwelZOdUi9iAP0Vw=";
            account.Email = Constants.docuSignAccountEmail;
            account.Password = Constants.docuSignAccountPassword;

            // Login API call (retrieves your baseUrl and accountId)
            bool result = account.Login();

            if (!result)
            {
                ViewBag.DocuSignError = "Error: Unable to log into DocuSign";
            }
            else
            {
                Envelope envelope = new Envelope();

                // Get Template ID from Document table

                //envelope.TemplateId = "365A1343-FD47-4FF2-ACFE-CB2F53A4F883";

                MemberVendorRequirement thisRequirement = db.Database.SqlQuery<MemberVendorRequirement>("exec dbo.[LB_GetMemberVendorRequirementByID] @ID",
                    new SqlParameter("@ID", ID)
                    ).First();

                if (thisRequirement == null)
                {
                    TempData["ErrorCode"] = Constants.fatalErrorCode;
                    TempData["ErrorMessage"] = "Invalid Requirement ID supplied to Send Document action";
                    return RedirectToAction("Error", "Admin");
                }
                else
                {
                    
                    // create envelope object and assign login info
                    envelope.Login = account;
                    envelope.Status = "sent";

                    envelope.TemplateId = thisRequirement.TemplateID;

                    Int32.TryParse(thisRequirement.CustID.ToString(), out VendorID);

                    // Validate the Member ID passed in
                    CheckCustomerResult checkCustomerResult = DataHelper.CheckCustomer(CustID, Constants.memberCustomerType);

                    if (checkCustomerResult.ErrorCode != 0)
                    {
                        TempData["ErrorCode"] = Constants.fatalErrorCode;
                        TempData["ErrorMessage"] = "Invalid Member ID supplied to Send Document page";
                        return RedirectToAction("Error", "Admin");
                    }
                    else
                    {
                        envelope.EmailSubject = checkCustomerResult.C_Info.NameTitle + " " +checkCustomerResult.C_Info.FirstName + " " + checkCustomerResult.C_Info.LastName + " (" + checkCustomerResult.C_Info.CustID + ") this " + thisRequirement.DocumentName + " requires your signature";
                        envelope.EmailBlurb = "Please click on the link in this email to sign this document electronically using DocuSign.";
                        
                        string emailTo = "";

                        if (Constants.isLive)
                        {
                            emailTo = checkCustomerResult.C_Info.Email;
                        }
                        else 
                        {
                            emailTo = "vere@motiongrid.com";
                        }

                        //checkCustomerResult.C_Info.Email
                        envelope.TemplateRoles = new TemplateRole[]
                        {
                            new TemplateRole()
                            {
                            email = emailTo, 
                            name =  checkCustomerResult.C_Info.FirstName + " " + checkCustomerResult.C_Info.LastName,
                            roleName = "RoleOne"
                            }
                        };

                        envelope.TemplateRoles[0].tabs = new RoleTabs();

                        envelope.TemplateRoles[0].tabs.textTabs = new RoleTextTab[14] 
                        {
                            new RoleTextTab()
                            {
                                tabLabel = "PracticeName",
                                value = checkCustomerResult.C_Info.DisplayName
                            },
                            new RoleTextTab()
                            {
                                tabLabel = "Company",
                                value = checkCustomerResult.C_Info.Company
                            },
                            new RoleTextTab()
                            {
                                tabLabel = "Address1",
                                value = checkCustomerResult.C_Info.Address1
                            },
                            new RoleTextTab()
                            {
                                tabLabel = "City",
                                value = checkCustomerResult.C_Info.City
                            },
                            new RoleTextTab()
                            {
                                tabLabel = "State",
                                value = checkCustomerResult.C_Info.State
                            },
                            new RoleTextTab()
                            {
                                tabLabel = "Zip",
                                value = checkCustomerResult.C_Info.PostalCode
                            },
                            new RoleTextTab()
                            {
                                tabLabel = "DayPhone",
                                value = DataHelper.FormatPhone(checkCustomerResult.C_Info.DayPhone)
                            },
                            new RoleTextTab()
                            {
                                tabLabel = "Fax",
                                value = checkCustomerResult.C_Info.Fax
                            },
                            new RoleTextTab()
                            {
                                tabLabel = "Email",
                                value = checkCustomerResult.C_Info.Email
                            },
                            new RoleTextTab()
                            {
                                tabLabel = "Mobile",
                                value = checkCustomerResult.C_Info.Mobile
                            },
                            new RoleTextTab()
                            {
                                tabLabel = "NameTitle",
                                value = checkCustomerResult.C_Info.NameTitle
                            },
                            new RoleTextTab()
                            {
                                tabLabel = "FirstName",
                                value = checkCustomerResult.C_Info.FirstName
                            },
                            new RoleTextTab()
                            {
                                tabLabel = "LastName",
                                value = checkCustomerResult.C_Info.LastName
                            },
                            new RoleTextTab()
                            {
                                tabLabel = "CustID",
                                value = checkCustomerResult.C_Info.CustID.ToString()
                            }
                        };

                        //*** Specify document and send the signature request
                        //            result = envelope.Create("C:\\AppTrackBetaSite\\TestDoc.pdf");
                        result = envelope.Create();

                        if (!result)
                        {
                            ViewBag.DocuSignError = "Error: Unable to send Document";
                        }
                        else
                        {
                            db.LB_UpdateMemberVendorRequirementByID(
                                ID,
                                "Sent",
                                "",""
                                );
                        }
                    }
                }
            }
            var requirementRows = db.Database.SqlQuery<MemberVendorRequirement>("exec dbo.[LB_GetMemberVendorRequirementByMemberVendor] @CustID, @VendorID",
                new SqlParameter("@CustID", CustID),
                new SqlParameter("@VendorID", VendorID)
                ).ToList();

            var model = new MemberVendorRequirementViewModel();

            model.MemberVendorRequirementList = requirementRows;
            model.CustID = CustID;
            model.VendorID = VendorID;

            IEnumerable<System.Web.Mvc.SelectListItem> MemberVendorRequirementStatusList = DataHelper.GetMemberVendorRequirementStatusList();

            return PartialView("~/Views/Member/_MemberVendorRequirements.cshtml", model);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

    }
}


