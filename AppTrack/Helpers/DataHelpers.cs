using AppTrack.Models;
using AppTrack.ViewModels;
using AppTrack.SharedModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Data.SqlClient;
using System.Data.Entity.Core.Objects;

using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.ComponentModel.DataAnnotations;
using System.IO;

namespace AppTrack.Helpers
{

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthorizeRedirect : AuthorizeAttribute
    {
        //        public string RedirectUrl = "~/Error/Unauthorized";
        private string RedirectUrl = "~/Home/Error";

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            base.HandleUnauthorizedRequest(filterContext);

            if (filterContext.RequestContext.HttpContext.User.Identity.IsAuthenticated)
            {
                filterContext.RequestContext.HttpContext.Response.Redirect(RedirectUrl);
            }
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthorizeAdminRedirect : AuthorizeAttribute
    {
        //        public string RedirectUrl = "~/Error/Unauthorized";
        private string RedirectUrl = "~/Admin/Error/99";

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            base.HandleUnauthorizedRequest(filterContext);

            if (filterContext.RequestContext.HttpContext.User.Identity.IsAuthenticated)
            {
                filterContext.RequestContext.HttpContext.Response.Redirect(RedirectUrl);
            }
        }
    }


    public class BaseController : Controller
    {
        public string DisplayName = "";
        public int CustID = 0;
        public int AdminID = 0;

        public BaseController()
        {

            if (System.Web.HttpContext.Current.User.Identity.IsAuthenticated)
            {

                // Consider replacing this with storing Temp Data values which are thread safe

                // This code gets the additional column data that we added AspNetUsers.CustID
                var currentUserId = System.Web.HttpContext.Current.User.Identity.GetUserId();
                //Instantiate the UserManager in ASP.Identity system so you can look up the user in the system
                var manager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));
                //Get the User object
                var currentUser = manager.FindById(System.Web.HttpContext.Current.User.Identity.GetUserId());
                //The currentUser object has properties that match the columns in the AspNetUsers table
                // Note when assigning nullable column to int variable the ?? say set it to 0 if it is null
                DisplayName = currentUser.DisplayName ?? "";
                AdminID = currentUser.AdminID ?? 0;
                CustID = currentUser.CustID ?? 0;

                if (AdminID > 0)
                {
                    System.Web.HttpContext context = System.Web.HttpContext.Current;
                    if (context != null)
                    {
                        HttpCookie custIDCookie = context.Request.Cookies["CustID"];

                        if (custIDCookie != null)
                        {
                            string sCustID = custIDCookie.Value;
                            int.TryParse(sCustID, out CustID);
                        } 
                    } 
                }
                // Why not put these in the ViewBag!
                ViewBag.DisplayName = DisplayName;
                ViewBag.AdminID = AdminID;
                ViewBag.CustID = CustID;

                int userStatusID = 9;

                if (TempData["userStatusID"] != null)
                {
                    userStatusID = (int)TempData["userStatusID"];
                }

                ViewBag.UserStatusID = userStatusID;
            }
        }
    }

    public class DataHelpers
    {
        private DevProvidentIDOCEntities db = new DevProvidentIDOCEntities();

        public IEnumerable<SelectListItem> GetNameTitleList()
        {
            List<SelectListItem> titles = new List<SelectListItem>()
            {
                new SelectListItem() {Value = " ", Text = " "},
                new SelectListItem() {Value = "Dr.", Text = "Dr."},
                new SelectListItem() {Value = "Mr.", Text = "Mr."},
                new SelectListItem() {Value = "Mrs.", Text = "Mrs."},
                new SelectListItem() {Value = "Ms.", Text = "Ms."}
            };

            return titles;
        }

        public IEnumerable<SelectListItem> GetVendorContactTypeList()
        {
            List<SelectListItem> contactTypes = new List<SelectListItem>()
            {
                new SelectListItem() {Value = "Program", Text = "Program"},
                new SelectListItem() {Value = "Rebate", Text = "Rebate"},
                new SelectListItem() {Value = "Website", Text = "Website"},
                new SelectListItem() {Value = "Other", Text = "Other"}
            };

            return contactTypes;
        }

        public IEnumerable<SelectListItem> GetVendorRequirementTypeList()
        {
            List<SelectListItem> requirementTypes = new List<SelectListItem>()
            {
                new SelectListItem() {Value = "Contact", Text = "Contact"},
                new SelectListItem() {Value = "Document", Text = "Document"},
                new SelectListItem() {Value = "Other", Text = "Other"}
            };

            return requirementTypes;
        }

        public IEnumerable<SelectListItem> GetStoreList(bool isBoth = true)
        {
            List<SelectListItem> stores = new List<SelectListItem>()
            {
                new SelectListItem() {Value = "0", Text = "ANY"},
                new SelectListItem() {Value = Constants.AppTrackStoreID.ToString(), Text = "AppTrack"},
                new SelectListItem() {Value = Constants.PRIMAStoreID.ToString(), Text = "PRIMA"}
            };

            if (isBoth)
            {
                stores.Add(new SelectListItem() { Text = "Both", Value = "1015" });
            }
            return stores;
        }

        public IEnumerable<SelectListItem> GetStateSelectList(bool isRequired = false, bool isAll = false)
        {
            List<Region> regionData = new List<Region>();
            
            regionData = db.Database.SqlQuery<Region>("exec dbo.[LB_GetRegionsByType] @RegionTypeID",
              new SqlParameter("@RegionTypeID", Constants.stateRegionTypeID)
            ).ToList();

            if (isAll)
            {
                var blankRegion = new Region
                {
                    RegionID = 0,
                    RegionName = " ",
                    RegionDescription = "All",
                    Seqno = 1
                };
                regionData.Insert(0, blankRegion);
            }
            else
            {
                if (!isRequired)
                {
                    var blankRegion = new Region
                    {
                        RegionID = 0,
                        RegionName = " ",
                        RegionDescription = " ",
                        Seqno = 1
                    };
                    regionData.Insert(0, blankRegion);
                }
            }

            var stateList = regionData
            .Select(region => new SelectListItem
            {
                Value = region.RegionName,
                Text = region.RegionDescription
            });

            return new SelectList(stateList, "Value", "Text");
        }

        public IEnumerable<SelectListItem> GetItemSelectList(bool isRequired = false, bool isAll = false)
        {
            List<Item> itemData = new List<Item>();

            itemData = db.Database.SqlQuery<Item>("exec dbo.[LB_GetItemsInStore] @StoreID",
              new SqlParameter("@StoreID", Constants.memberStoreID)
            ).ToList();

            if (isAll)
            {
                var allItem = new Item
                {
                    ItemID = 0,
                    ItemName = "All",
                    ItemDescription = "Show All"
                };
                itemData.Insert(0, allItem);
            }
            else
            {
                if (!isRequired)
                {

                    var allItem = new Item
                    {
                        ItemID = 0,
                        ItemName = "(optional)",
                        ItemDescription = "(optional)"
                    };
                    itemData.Insert(0, allItem);
                }
            }
            var itemList = itemData
            .Select(item => new SelectListItem
            {
                Value = item.ItemID.ToString(),
                Text = item.ItemName
            });

            return new SelectList(itemList, "Value", "Text");
        }

        public IEnumerable<SelectListItem> GetAllMembershipItemsSelectList(bool isRequired = false, bool isAll = false)
        {
            List<Item> itemData = new List<Item>();

            int two = 2;

            itemData = db.Database.SqlQuery<Item>("exec dbo.[LB_GetItemsInStore] @StoreID, @StatusID",
              new SqlParameter("@StoreID", Constants.memberStoreID),
              new SqlParameter("@StatusID", two)

            ).ToList();

            if (isAll)
            {
                var allItem = new Item
                {
                    ItemID = 0,
                    ItemName = "All",
                    ItemDescription = "Show All"
                };
                itemData.Insert(0, allItem);
            }
            else
            {
                if (!isRequired)
                {

                    var allItem = new Item
                    {
                        ItemID = 0,
                        ItemName = "(optional)",
                        ItemDescription = "(optional)"
                    };
                    itemData.Insert(0, allItem);
                }
            }
            var itemList = itemData
            .Select(item => new SelectListItem
            {
                Value = item.ItemID.ToString(),
                Text = item.ItemName
            });

            return new SelectList(itemList, "Value", "Text");
        }
        public IEnumerable<SelectListItem> GetVendorDocumentSelectList(int CustID = 0, bool isRequired = false, bool isAll = false)
        {
            List<Document> documentData = new List<Document>();
            if (CustID != 0)
            {
                documentData = db.Database.SqlQuery<Document>("exec dbo.[LB_GetVendorDocuments] @CustID",
                new SqlParameter("@CustID", CustID)
                ).ToList();

                if (isAll)
                {
                    var allDocument = new Document
                    {
                        DocumentID = 0,
                        DocumentName = "All",
                        DocumentDescription = "Show All"
                    };
                    documentData.Insert(0, allDocument);
                }
                else
                {
                    if (!isRequired)
                    {
                        var allDocument = new Document
                        {
                            DocumentID = 0,
                            DocumentName = "Select Document",
                            DocumentDescription = "(optional)"
                        };
                        documentData.Insert(0, allDocument);
                    }
                }
            }
            var documentList = documentData
            .Select(document => new SelectListItem
            {
                Value = document.DocumentID.ToString(),
                Text = document.DocumentName
            });

            return new SelectList(documentList, "Value", "Text");
        }

        public IEnumerable<SelectListItem> GetVendorProgramSelectList(int CustID = 0, bool isRequired = false, bool isAll = false)
        {
            List<VendorProgram> programData = new List<VendorProgram>();
            if (CustID != 0)
            {
                programData = db.Database.SqlQuery<VendorProgram>("exec dbo.[LB_GetVendorPrograms] @CustID",
                new SqlParameter("@CustID", CustID)
                ).ToList();

                if (isAll)
                {
                    var allProgram = new VendorProgram
                    {
                        ProgramID = 0,
                        ProgramName = "All",
                        ProgramDescription = "Show All"
                    };
                    programData.Insert(0, allProgram);
                }
                else
                {
                    if (!isRequired)
                    {
                        var allProgram = new VendorProgram
                        {
                            ProgramID = 0,
                            ProgramName = "Select Program",
                            ProgramDescription = "(optional)"
                        };
                        programData.Insert(0, allProgram);
                    }
                }
            }
            var programList = programData
            .Select(program => new SelectListItem
            {
                Value = program.ProgramID.ToString(),
                Text = program.ProgramName
            });

            return new SelectList(programList, "Value", "Text");
        }

        public IEnumerable<SelectListItem> GetCompanyProgramSelectList(bool isRequired = false, bool isAll = false)
        {
            List<C_Program> programData = new List<C_Program>();
                programData = db.Database.SqlQuery<C_Program>("exec dbo.[LB_GetCompanyPrograms]").ToList();

                if (isAll)
                {
                    var allProgram = new C_Program
                    {
                        ID = 0,
                        ProgramName = "All",
                        ProgramDescription = "Show All"
                    };
                    programData.Insert(0, allProgram);
                }
                else
                {
                    if (!isRequired)
                    {
                        var allProgram = new C_Program
                        {
                            ID = 0,
                            ProgramName = "Select Program",
                            ProgramDescription = "(optional)"
                        };
                        programData.Insert(0, allProgram);
                    }
                }

            var programList = programData
            .Select(program => new SelectListItem
            {
                Value = program.ID.ToString(),
                Text = program.ProgramName
            });

            return new SelectList(programList, "Value", "Text");
        }

        public IEnumerable<SelectListItem> GetAdminSelectList(bool isAll = false)
        {
            List<AdminUser> adminData = new List<AdminUser>();

            adminData = db.Database.SqlQuery<AdminUser>("exec dbo.[LB_GetAdminUsers]").ToList();

            var adminList = adminData
            .Select(admin => new SelectListItem
            {
                Value = admin.AdminID.ToString(),
                Text = admin.DisplayName
            });

            if (isAll)
            {
                var allAdmin = new AdminUser
                {
                    AdminID = 0,
                    DisplayName = "All"
                };
                adminData.Insert(0, allAdmin);
            }

            return new SelectList(adminList, "Value", "Text");
        }

        public IEnumerable<SelectListItem> GetAccountManagerSelectList(bool isRequired = false, bool isAll = false)
        {
            List<AdminUser> adminData = new List<AdminUser>();

            int adminID = 0;

            adminData = db.Database.SqlQuery<AdminUser>("exec dbo.[LB_GetAdminUserList] @AdminID, @RoleName", 
            new SqlParameter("@AdminID", adminID),
            new SqlParameter("@RoleName", "AccountManager")
                ).ToList();

            if (isAll)
            {
                var allAdmin = new AdminUser
                {
                    AdminID = 0,
                    DisplayName = "All"
                };
                adminData.Insert(0, allAdmin);
            }
            else
            {
                if (!isRequired)
                {
                    var anyAdmin = new AdminUser
                    {
                        AdminID = 0,
                        DisplayName = "(optional)"
                    };
                    adminData.Insert(0, anyAdmin);
                }
            }

            var adminList = adminData
            .Select(admin => new SelectListItem
            {
                Value = admin.AdminID.ToString(),
                Text = admin.DisplayName
            });

            return new SelectList(adminList, "Value", "Text");
        }

        public IEnumerable<SelectListItem> GetMemberDirectorSelectList(bool isRequired = false, bool isAll = false)
        {
            List<MemberDirector> memberDirectorData = new List<MemberDirector>();
            
            memberDirectorData = db.Database.SqlQuery<MemberDirector>("exec dbo.[LB_GetCustByLastNameStatusType]  @LastName, @Status, @CustomerType",
            new SqlParameter("@LastName", " "),
            new SqlParameter("@Status", "Active"),
            new SqlParameter("@CustomerType", Constants.memberDirectorCustomerType)
            ).ToList();

            if (isAll)
            {
                var allMemberDirector = new MemberDirector
                {
                    CustID = 0,
                    DisplayName = "Show All"
                };
                memberDirectorData.Insert(0, allMemberDirector);
            }
            else
            {
                if (!isRequired)
                {
                    var anyMemberDirector = new MemberDirector
                    {
                        CustID = 0,
                        DisplayName = "(optional)"
                    };
                    memberDirectorData.Insert(0, anyMemberDirector);
                }
            }
            var memberDirectorList = memberDirectorData
            .Select(item => new SelectListItem
            {
                Value = item.CustID.ToString(),
                Text = item.DisplayName
            });

            return new SelectList(memberDirectorList, "Value", "Text");
        }


        public IEnumerable<SelectListItem> GetStatusSelectList(int? lookupGroupID, bool isRequired = false, bool isAll = false)
        {
            List<LookUpItem> lookupData = new List<LookUpItem>();

            if (lookupGroupID != null)
            {
                lookupData = GetLookup(lookupGroupID);
            }
            if (isAll)
            {
                lookupData.Insert(0, new LookUpItem()
                {
                    DataLabel = " ",
                    IDValue = 1,
                    Description = "All"
                });
            }
            else
            {
                if (!isRequired)
                {
                    lookupData.Insert(0, new LookUpItem()
                    {
                        DataLabel = " ",
                        IDValue = 1,
                        Description = "(optional)"
                    });
                }
            }

            var statusList = lookupData
                .Select(x =>
                        new SelectListItem
                        {
                            Value = x.DataLabel,
                            Text = x.Description
                        });
            return new SelectList(statusList, "Value", "Text");
        }

        public List<LookUpItem> GetLookup(int? dataGroupID)
        {

            List<LookUpItem> lookupData = new List<LookUpItem>();

            lookupData = db.Database.SqlQuery<LookUpItem>("exec dbo.[LB_GetLookUpListByGroupID] @DataGroupID",
             new SqlParameter("@DataGroupID", dataGroupID)
            ).ToList();

            return lookupData;
        }

        public IEnumerable<SelectListItem> GetCategorySelectList(int CategoryGroupID = 1, bool isRequired = false, bool isAll = false)
        {
            List<C_Categories> categoryData = new List<C_Categories>();

            categoryData = db.Database.SqlQuery<C_Categories>("exec dbo.[LB_GetCategoryByGroupID] @CategoryGroupID",
            new SqlParameter("@CategoryGroupID", CategoryGroupID)
            ).ToList();

            if (isAll)
            {
                var allCategory = new C_Categories
                {
                    CategoryID = 0,
                    CategoryName = "All"
                };
                categoryData.Insert(0, allCategory);
            }
            else
            {
                if (!isRequired)
                {
                    var anyCategory = new C_Categories
                    {
                        CategoryID = 0,
                        CategoryName = "(optional)"
                    };
                    categoryData.Insert(0, anyCategory);
                }
            }

            var categoryList = categoryData
            .Select(category => new SelectListItem
            {
                Value = category.CategoryID.ToString(),
                Text = category.CategoryName
            });

            return new SelectList(categoryList, "Value", "Text");
        }

        public CheckCustomerResult CheckCustomer(int? custID)
        {
            CheckCustomerResult checkCustomerResult = new CheckCustomerResult();

            checkCustomerResult.ErrorCode = 0;
            checkCustomerResult.ErrorMessage = "";

            if (custID == null)
            {
                checkCustomerResult.ErrorMessage = "The Customer ID supplied is not valid";
                checkCustomerResult.ErrorCode = 1;
            }
            else
            {
                C_Info c_Info = db.Database.SqlQuery<C_Info>("exec dbo.[LB_GetCustByCustID] @CustID",
                new SqlParameter("@CustID", custID)
                ).FirstOrDefault();

                if (c_Info == null)
                {
                    checkCustomerResult.ErrorMessage = "The Customer ID supplied is not valid";
                    checkCustomerResult.ErrorCode = 1;
                }
                else
                {
                    checkCustomerResult.DisplayName = c_Info.DisplayName;
                    checkCustomerResult.C_Info = c_Info;
                }
            }
            return checkCustomerResult;
        }

        public CheckCustomerResult CheckCustomer(int? custID, int? customerType)
        {
            CheckCustomerResult checkCustomerResult = new CheckCustomerResult();

            checkCustomerResult.ErrorCode = 0;
            checkCustomerResult.ErrorMessage = "";

            if (custID == null)
            {
                checkCustomerResult.ErrorMessage = "The Customer ID supplied is not valid";
                checkCustomerResult.ErrorCode = 1;
            }
            else
            {
                C_Info c_Info = db.Database.SqlQuery<C_Info>("exec dbo.[LB_GetCustByCustID] @CustID",
                new SqlParameter("@CustID", custID)
                ).FirstOrDefault();

                if (c_Info == null)
                {
                    checkCustomerResult.ErrorMessage = "The Customer ID supplied is not valid";
                    checkCustomerResult.ErrorCode = 1;
                }
                else
                {
                    checkCustomerResult.DisplayName = c_Info.DisplayName;
                    checkCustomerResult.C_Info = c_Info;
                    if (c_Info.CustomerType != customerType)
                    {
                        checkCustomerResult.ErrorMessage = "The Customer ID supplied is not valid";
                        checkCustomerResult.ErrorCode = 1;
                    }
                }
            }
            return checkCustomerResult;
        }

        public CheckCustomerResult CheckCustomer(int? custID, int? customerType, int? statusID)
        {
            CheckCustomerResult checkCustomerResult = new CheckCustomerResult();

            checkCustomerResult.ErrorCode = 0;
            checkCustomerResult.ErrorMessage = "";
            checkCustomerResult.DisplayName = "";

            if (custID == null)
            {
                checkCustomerResult.ErrorMessage = "The Customer ID supplied is not valid";
                checkCustomerResult.ErrorCode = 1;
            }
            else
            {
                C_Info c_Info = db.Database.SqlQuery<C_Info>("exec dbo.[LB_GetCustByCustID] @CustID",
                new SqlParameter("@CustID", custID)
                ).FirstOrDefault();
                if (c_Info == null)
                {
                    checkCustomerResult.ErrorMessage = "The Customer ID supplied is not valid";
                    checkCustomerResult.ErrorCode = 1;
                }
                else
                {
                    checkCustomerResult.DisplayName = c_Info.DisplayName;
                    if (c_Info.CustomerType != customerType)
                    {
                        checkCustomerResult.ErrorMessage = "The Customer Type does not match";
                        checkCustomerResult.ErrorCode = 2;
                    }
                    else
                    {
                        if (c_Info.StatusID != statusID)
                        {
                            checkCustomerResult.ErrorMessage = "The Customer Status ID does not match";
                            checkCustomerResult.ErrorCode = 3;
                        }
                    }
                }
            }
            return checkCustomerResult;
        }

/*        // Sample code to generate a list of years from this year to this year + 10

        public IEnumerable<SelectListItem> GetYearListForward()
        {
            var yr = Enumerable.Range(DateTime.Now.Year, 10).Select(x => new SelectListItem { Value = x.ToString(), Text = x.ToString() });
            return new SelectList(yr.ToList(), "Value", "Text");
        }
*/

        public IEnumerable<SelectListItem> GetTimeHourList(bool isRequired = false, bool isAll = false)
        {
            List<SelectListItem> timeHours = new List<SelectListItem>()
            {
                new SelectListItem() {Value = "1", Text = "1"},
                new SelectListItem() {Value = "2", Text = "2"},
                new SelectListItem() {Value = "3", Text = "3"},
                new SelectListItem() {Value = "4", Text = "4"},
                new SelectListItem() {Value = "5", Text = "5"},
                new SelectListItem() {Value = "6", Text = "6"},
                new SelectListItem() {Value = "7", Text = "7"},
                new SelectListItem() {Value = "8", Text = "8"},
                new SelectListItem() {Value = "9", Text = "9"},
                new SelectListItem() {Value = "10", Text = "10"},
                new SelectListItem() {Value = "11", Text = "11"},
                new SelectListItem() {Value = "12", Text = "12"}
            };

            if (isAll)
            {
                timeHours.Insert(0, (new SelectListItem { Text = "All", Value = "0" }));
            }
            else
            {
                if (!isRequired)
                {
                    timeHours.Insert(0, (new SelectListItem { Text = "Hour", Value = "" }));
                }
            }
            return timeHours;
        }

        //var selectList = new SelectList(Enumerable.Range(2008, DateTime.Now.Year - 2007).Reverse());

        public IEnumerable<SelectListItem> GetTimeMinuteList(bool isRequired = false, bool isAll = false)
        {
            List<SelectListItem> timeMins = new List<SelectListItem>()
            {
                new SelectListItem() {Value = "0", Text = "00"},
                new SelectListItem() {Value = "15", Text = "15"},
                new SelectListItem() {Value = "30", Text = "30"},
                new SelectListItem() {Value = "45", Text = "45"}
            };

            if (isAll)
            {
                timeMins.Insert(0, (new SelectListItem { Text = "All", Value = "0" }));
            }
            else
            {
                if (!isRequired)
                {
                    timeMins.Insert(0, (new SelectListItem { Text = "Min", Value = "1" }));
                }
            }
            return timeMins;
        }

        public IEnumerable<SelectListItem> GetTimeAMPMList()
        {
            List<SelectListItem> timeAMPM = new List<SelectListItem>()
            {
                new SelectListItem() {Value = "AM", Text = "AM"},
                new SelectListItem() {Value = "PM", Text = "PM"}
            };

            return timeAMPM;
        }
        public IEnumerable<SelectListItem> GetCustIDList(int CustomerType = 0, bool isRequired = false, bool isAll = false)
        {
            List<CustomerBasic> customerData = new List<CustomerBasic>();
            if (CustomerType != 0)
            {
                customerData = db.Database.SqlQuery<CustomerBasic>("exec dbo.[LB_GetC_InfoByCustomerType] @CustomerType",
                new SqlParameter("@CustomerType", CustomerType)
                ).ToList().OrderBy(m => m.DisplayName).ToList();

                if (isAll)
                {
                    var allCustomer = new CustomerBasic
                    {
                        CustID = 0,
                        DisplayName = "All"
                    };
                    customerData.Insert(0, allCustomer);
                }
                else
                {
                    if (!isRequired)
                    {
                        var allCustomer = new CustomerBasic
                        {
                            CustID = 0,
                            DisplayName = "(optional)"
                        };
                        customerData.Insert(0, allCustomer);
                    }
                }
            }
            var custIDList = customerData
            .Select(customer => new SelectListItem
            {
                Value = customer.CustID.ToString(),
                Text = customer.DisplayName
            });

            return new SelectList(custIDList, "Value", "Text");
        }

        public IEnumerable<SelectListItem> GetIMDByContactNameList(int CustomerType = 0, bool isRequired = false, bool isAll = false)
        {
            List<CustomerBasic> customerData = new List<CustomerBasic>();
            if (CustomerType != 0)
            {
                customerData = db.Database.SqlQuery<CustomerBasic>("exec dbo.[LB_GetC_InfoByCustomerType] @CustomerType",
                new SqlParameter("@CustomerType", CustomerType)
                ).ToList().OrderBy(m => m.LastName).ToList();

                if (isAll)
                {
                    var allCustomer = new CustomerBasic
                    {
                        CustID = 0,
                        FirstName = "All",
                        LastName = ""
                    };
                    customerData.Insert(0, allCustomer);
                }
                else
                {
                    if (!isRequired)
                    {
                        var allCustomer = new CustomerBasic
                        {
                            CustID = 0,
                            FirstName = "(optional)",
                            LastName = ""
                        };
                        customerData.Insert(0, allCustomer);
                    }
                }
            }
            var custIDList = customerData
            .Select(customer => new SelectListItem
            {
                Value = customer.CustID.ToString(),
                Text = customer.LastName + ' ' + customer.FirstName
            });

            return new SelectList(custIDList, "Value", "Text");
        }

        public IEnumerable<SelectListItem> GetCommissionIDList(int CommissionID = 0, bool isRequired = false, bool isAll = false)
        {
            List<C_Commissions> commissionData = new List<C_Commissions>();
            commissionData = db.Database.SqlQuery<C_Commissions>("exec dbo.[LB_GetCommissionByID] @CommissionID",
            new SqlParameter("@CommissionID", CommissionID)
            ).ToList();

            if (isAll)
            {
                var allValue = new C_Commissions
                {
                    CommissionID = 0,
                    CommissionName = "All"
                };
                commissionData.Insert(0, allValue);
            }
            else
            {
                if (!isRequired)
                {
                    var allValue = new C_Commissions
                    {
                        CommissionID = 0,
                        CommissionName = "(optional)"
                    };
                    commissionData.Insert(0, allValue);
                }
            }
            var commissionIDList = commissionData
            .Select(commission => new SelectListItem
            {
                Value = commission.CommissionID.ToString(),
                Text = commission.CommissionName
            });

            return new SelectList(commissionIDList, "Value", "Text");
        }

        public IEnumerable<SelectListItem> GetSalesCommissionIDList(bool isRequired = false, bool isAll = false)
        {
            List<C_Commissions> commissionData = new List<C_Commissions>();
            commissionData = db.Database.SqlQuery<C_Commissions>("exec dbo.[LB_GetCommissionByDataGroupID] @DataGroupID",
            new SqlParameter("@DataGroupID", Constants.SalesRepCommissionLookupGroupID)
            ).ToList();

            if (isAll)
            {
                var allValue = new C_Commissions
                {
                    CommissionID = 0,
                    CommissionName = "All"
                };
                commissionData.Insert(0, allValue);
            }
            else
            {
                if (!isRequired)
                {
                    var allValue = new C_Commissions
                    {
                        CommissionID = 0,
                        CommissionName = "(optional)"
                    };
                    commissionData.Insert(0, allValue);
                }
            }
            var commissionIDList = commissionData
            .Select(commission => new SelectListItem
            {
                Value = commission.CommissionID.ToString(),
                Text = commission.CommissionName
            });

            return new SelectList(commissionIDList, "Value", "Text");
        }
        public IEnumerable<SelectListItem> GetIMDCommissionIDList(bool isRequired = false, bool isAll = false)
        {
            List<C_Commissions> commissionData = new List<C_Commissions>();
            commissionData = db.Database.SqlQuery<C_Commissions>("exec dbo.[LB_GetCommissionByDataGroupID] @DataGroupID",
            new SqlParameter("@DataGroupID", Constants.IMDCommissionLookupGroupID)
            ).ToList();

            if (isAll)
            {
                var allValue = new C_Commissions
                {
                    CommissionID = 0,
                    CommissionName = "All"
                };
                commissionData.Insert(0, allValue);
            }
            else
            {
                if (!isRequired)
                {
                    var allValue = new C_Commissions
                    {
                        CommissionID = 0,
                        CommissionName = "(optional)"
                    };
                    commissionData.Insert(0, allValue);
                }
            }
            var commissionIDList = commissionData
            .Select(commission => new SelectListItem
            {
                Value = commission.CommissionID.ToString(),
                Text = commission.CommissionName
            });

            return new SelectList(commissionIDList, "Value", "Text");
        }

        public IEnumerable<SelectListItem> GetManualCommissionIDSelectList(bool isRequired = false, bool isAll = false)
        {
            string thisCommissionName = "Select A Commission";
            if (isAll)
            {
                thisCommissionName = "All";
            }
            
            List<SelectListItem> commissionList = new List<SelectListItem>()
            {
                new SelectListItem() {Value = "0", Text = thisCommissionName},
                new SelectListItem() {Value = "30", Text = "Sales Member Enrollment"},
                new SelectListItem() {Value = "40", Text = "IMD Member Enrollment Commissions"},
                new SelectListItem() {Value = "41", Text = "IMD Six Month Member Bonus"},
                new SelectListItem() {Value = "45", Text = "IMD Meeting Commission"},
                new SelectListItem() {Value = "16", Text = "General Override Commissions"}
            };

            return commissionList;
        }

        public IEnumerable<SelectListItem> GetRebateVendorList(bool isRequired = false, bool isAll = false)
        {
            List<Vendor> vendorData = new List<Vendor>();
            vendorData = db.Database.SqlQuery<Vendor>("exec dbo.[LB_GetVendorList] "
            ).ToList();

            if (isAll)
            {
                var allValue = new Vendor
                {
                    CustID = 0,
                    DisplayName = "All"
                };
                vendorData.Insert(0, allValue);
            }
            else
            {
                if (!isRequired)
                {
                    var allValue = new Vendor
                    {
                        CustID = 0,
                        DisplayName = "(optional)"
                    };
                    vendorData.Insert(0, allValue);
                }
            }
            var vendorList = vendorData
            .Select(vendor => new SelectListItem
            {
                Value = vendor.CustID.ToString(),
                Text = vendor.DisplayName
            });

            return new SelectList(vendorList, "Value", "Text");
        }

        public IEnumerable<SelectListItem> GetRebateVolumeList(int VolumeType = 0, bool isRequired = false, bool isAll = false)
        {
            List<C_Volumes> volumeListData = new List<C_Volumes>();
            volumeListData = db.Database.SqlQuery<C_Volumes>("exec dbo.[LB_GetVolumesBYType] @VolumeType",
            new SqlParameter("@VolumeType", VolumeType)
            ).ToList();

            if (isAll)
            {
                var allValue = new C_Volumes
                {
                    VolumeID = 0,
                    VolumeName = "All"
                };
                volumeListData.Insert(0, allValue);
            }
            else
            {
                if (!isRequired)
                {
                    var allValue = new C_Volumes
                    {
                        VolumeID = 0,
                        VolumeName = "(optional)"
                    };
                    volumeListData.Insert(0, allValue);
                }
            }
            var volumeList = volumeListData
            .Select(volume => new SelectListItem
            {
                Value = volume.VolumeID.ToString(),
                Text = volume.VolumeName
            });

            return new SelectList(volumeList, "Value", "Text");
        }

        public IEnumerable<SelectListItem> GetPeriodIDList(int PeriodTypeID = 0, int PeriodCount = 12, bool isRequired = false, bool isAll = false)
        {
            List<C_Periods> listData = new List<C_Periods>();
            if (PeriodTypeID != 0)
            {
                listData = db.Database.SqlQuery<C_Periods>("exec dbo.[LB_GetPeriodList] @PeriodTypeID, @PeriodCount",
                new SqlParameter("@PeriodTypeID", PeriodTypeID),
                new SqlParameter("@PeriodCount", PeriodCount)
                ).ToList();

                if (isAll)
                {
                    var allRecord = new C_Periods
                    {
                        PeriodID = 0,
                        PeriodName = "All"
                    };
                    listData.Insert(0, allRecord);
                }
                else
                {
                    if (!isRequired)
                    {
                        var allRecord = new C_Periods
                        {
                            PeriodID = 0,
                            PeriodName = "(optional)"
                        };
                        listData.Insert(0, allRecord);
                    }
                }
            }
            var thisList = listData
            .Select(listItem => new SelectListItem
            {
                Value = listItem.PeriodID.ToString(),
                Text = listItem.PeriodName
            });

            return new SelectList(thisList, "Value", "Text");
        }



        public IEnumerable<SelectListItem> GetCardTypeSelectList()
        {
            List<SelectListItem> cardTypes = new List<SelectListItem>()
            {
                new SelectListItem() {Value = "", Text = ""},
                new SelectListItem() {Value = "Visa", Text = "Visa"},
                new SelectListItem() {Value = "MasterCard", Text = "Master Card"},
                new SelectListItem() {Value = "AMEX", Text = "American Express"},
                new SelectListItem() {Value = "Discover", Text = "Discover"}
            };

            return cardTypes;
        }

        public IEnumerable<SelectListItem> GetCardExpMonthsSelectList()
        {
            List<SelectListItem> cardExpMonths = new List<SelectListItem>()
            {
                new SelectListItem() {Value = "", Text = ""},
                new SelectListItem() {Value = "01", Text = "January"},
                new SelectListItem() {Value = "02", Text = "February"},
                new SelectListItem() {Value = "03", Text = "March"},
                new SelectListItem() {Value = "04", Text = "April"},
                new SelectListItem() {Value = "05", Text = "May"},
                new SelectListItem() {Value = "06", Text = "June"},
                new SelectListItem() {Value = "07", Text = "July"},
                new SelectListItem() {Value = "08", Text = "August"},
                new SelectListItem() {Value = "09", Text = "September"},
                new SelectListItem() {Value = "10", Text = "October"},
                new SelectListItem() {Value = "11", Text = "November"},
                new SelectListItem() {Value = "12", Text = "December"}
            };

            return cardExpMonths;
        }

        public IEnumerable<SelectListItem> GetCardExpYearsSelectList()
        {
            List<SelectListItem> cardExpYears = new List<SelectListItem>()
            {
                new SelectListItem() {Value = "", Text = ""},
                new SelectListItem() {Value = "2015", Text = "2015"},
                new SelectListItem() {Value = "2016", Text = "2016"},
                new SelectListItem() {Value = "2017", Text = "2017"},
                new SelectListItem() {Value = "2018", Text = "2018"},
                new SelectListItem() {Value = "2019", Text = "2019"},
                new SelectListItem() {Value = "2020", Text = "2020"},
                new SelectListItem() {Value = "2021", Text = "2021"},
                new SelectListItem() {Value = "2022", Text = "2022"},
                new SelectListItem() {Value = "2023", Text = "2023"},
                new SelectListItem() {Value = "2024", Text = "2024"},
                new SelectListItem() {Value = "2025", Text = "2025"},
                new SelectListItem() {Value = "2026", Text = "2026"},
                new SelectListItem() {Value = "2027", Text = "2027"},
                new SelectListItem() {Value = "2028", Text = "2028"},
                new SelectListItem() {Value = "2029", Text = "2029"},
                new SelectListItem() {Value = "2030", Text = "2030"},
                new SelectListItem() {Value = "2031", Text = "2032"}
            };

            return cardExpYears;
        }


        public IEnumerable<SelectListItem> GetItemPromotionSelectList(int ItemID = 0, bool isRequired = false, bool isAll = false)
        {
            List<ItemPromotion> itemPromotionData = new List<ItemPromotion>();
            itemPromotionData = db.Database.SqlQuery<ItemPromotion>("exec dbo.[LB_GetItemPromotion] @ItemID",
              new SqlParameter("@ItemID", ItemID)
            ).ToList();

            var optionalItemPromotion = new ItemPromotion
            {
                ID = 0,
                ItemID = 0,
                PromotionID = -1,
                PromotionTitle = "None"
            };
            itemPromotionData.Insert(0, optionalItemPromotion);

            var itemPromotionList = itemPromotionData
            .Select(item => new SelectListItem
            {
                Value = item.PromotionID.ToString(),
                Text = item.PromotionTitle
            });

            return new SelectList(itemPromotionList, "Value", "Text");
        }

        public string FixPhone(string phone)
        {
            string plainPhone = "";

            if (!String.IsNullOrWhiteSpace(phone))
            {
                plainPhone = phone.Replace("(", "").Replace(")", "").Replace("x", "");
                plainPhone = plainPhone.Replace("-", "");
                plainPhone = plainPhone.Replace(" ", "");
            }
            return plainPhone;
        }

        public string FormatPhone(string phone)
        {
            string formatPhone = "";

            if (!String.IsNullOrWhiteSpace(phone))
            {
                if (phone.Length < 10)
                {
                    formatPhone = phone;
                }
                if (phone.Length == 10)
                {
                    string area = phone.Substring(0, 3);
                    string major = phone.Substring(3, 3);
                    string minor = phone.Substring(6);
                    formatPhone = string.Format("{0}-{1}-{2}", area, major, minor);
                }
                if (phone.Length > 10)
                {
                    int extLength = phone.Length - 10;

                    string area = phone.Substring(0, 3);
                    string major = phone.Substring(3, 3);
                    string minor = phone.Substring(6, 4);
                    string ext = phone.Substring(10);
                    formatPhone = string.Format("{0}-{1}-{2} {3}", area, major, minor, ext);
                }
            }
            return formatPhone;
        }

        public IEnumerable<SelectListItem> GetRebateCommissionList()
        {
            List<SelectListItem> commissionIDList = new List<SelectListItem>()
            {
                new SelectListItem() {Value = "60", Text = "Corporate"},
                new SelectListItem() {Value = "70", Text = "Member"},
            };

            return commissionIDList;
        }

        public IEnumerable<SelectListItem> GetPeriodListVS(int periodTypeID, int periodCount, bool isRequired = false, bool isAll = false)
        {
            List<C_Periods> periodData = new List<C_Periods>();
            periodData = db.Database.SqlQuery<C_Periods>("exec dbo.[LB_GetPeriodList] @PeriodTypeID, @PeriodCount",
              new SqlParameter("@PeriodTypeID", periodTypeID),
              new SqlParameter("@PeriodCount", periodCount)
            ).ToList();

            if (isAll)
            {
                var allValue = new C_Periods
                {
                    PeriodID = 0,
                    PeriodName = "All"
                };
                periodData.Insert(0, allValue);
            }
            else
            {
                if (!isRequired)
                {
                    var allValue = new C_Periods
                    {
                        PeriodID = 0,
                        PeriodName = "(optional)"
                    };
                    periodData.Insert(0, allValue);
                }
            }
            var optionalPeriod = new C_Periods
            {
                PeriodID = 0,
                PeriodName = "(optional)"
            };

            periodData.Insert(0, optionalPeriod);
            var periodList = periodData
            .Select(item => new SelectListItem
            {
                Value = item.PeriodID.ToString(),
                Text = item.PeriodName
            });

            return new SelectList(periodList, "Value", "Text");
        }

        public IEnumerable<SelectListItem> GetCancelReasonCodesSelectList()
        {
            List<SelectListItem> cancelReasonCodes = new List<SelectListItem>()
            {
                new SelectListItem() {Value = "", Text = ""},
                new SelectListItem() {Value = "Sold Practice", Text = "Sold Practice"},
                new SelectListItem() {Value = "Dissatisfied with rebates", Text = "Dissatisfied with rebates"},
                new SelectListItem() {Value = "Dissatisfied with programs", Text = "Dissatisfied with programs"},
                new SelectListItem() {Value = "Joined another alliance", Text = "Joined another alliance"},
                new SelectListItem() {Value = "Low rebates", Text = "Low rebates"}
            };

            return cancelReasonCodes;
        }

        public IEnumerable<SelectListItem> GetAccountReviewReasonCodesSelectList()
        {
            List<SelectListItem> accountReviewReasonCodes = new List<SelectListItem>()
            {
                new SelectListItem() {Value = "", Text = ""},
                new SelectListItem() {Value = "Dissatisfied", Text = "Dissatisfied"},
                new SelectListItem() {Value = "Low Rebates", Text = "Low Rebates"},
                new SelectListItem() {Value = "No study groups/IMD", Text = "No study groups/IMD"},
                new SelectListItem() {Value = "Payment declined", Text = "Payment declined"},
                new SelectListItem() {Value = "Joined another alliance", Text = "Joined another alliance"},
                new SelectListItem() {Value = "Vistakon", Text = "Vistakon"}
            };

            return accountReviewReasonCodes;
        }

        public IEnumerable<SelectListItem> GetReviewStatusList()
        {
            List<SelectListItem> reviewStatusList = new List<SelectListItem>()
            {
                new SelectListItem() {Value = "New", Text = "New"},
                new SelectListItem() {Value = "InProgress", Text = "InProgress"},
                new SelectListItem() {Value = "OnHold", Text = "OnHold"},
                new SelectListItem() {Value = "Completed", Text = "Completed"},
                new SelectListItem() {Value = "Cancelled", Text = "Cancelled"},
            };
            return reviewStatusList;
        }

        public IEnumerable<SelectListItem> GetReview2StatusList()
        {
            List<SelectListItem> reviewStatusList = new List<SelectListItem>()
            {
                new SelectListItem() {Value = "InProgress", Text = "InProgress"},
                new SelectListItem() {Value = "OnHold", Text = "OnHold"},
                new SelectListItem() {Value = "Completed", Text = "Completed"},
                new SelectListItem() {Value = "Cancelled", Text = "Cancelled"},
            };
            return reviewStatusList;
        }
        
        public IEnumerable<SelectListItem> GetReviewOutcomeList()
        {
            List<SelectListItem> reviewOutcomeList = new List<SelectListItem>()
            {
                new SelectListItem() {Value = "", Text = ""},
                new SelectListItem() {Value = "Save", Text = "Save"},
                new SelectListItem() {Value = "Lost", Text = "Lost"},
            };
            return reviewOutcomeList;
        }


        public IEnumerable<SelectListItem> GetOtherAlliancesSelectList()
        {
            List<SelectListItem> otherAlliances = new List<SelectListItem>()
            {
        
                new SelectListItem() {Value = "", Text = ""},
                new SelectListItem() {Value = "Vision Source", Text = "Vision Source"},
                new SelectListItem() {Value = "Perc", Text = "Perc"},
                new SelectListItem() {Value = "PECCA", Text = "PECCA"},
                new SelectListItem() {Value = "Prima", Text = "Prima"},
                new SelectListItem() {Value = "Vision West", Text = "Vision West"},
                new SelectListItem() {Value = "Vision Trends", Text = "Vision Trends"},
                new SelectListItem() {Value = "HMI", Text = "HMI"},
                new SelectListItem() {Value = "Red Tray", Text = "Red Tray"}, 
                new SelectListItem() {Value = "Block", Text = "Block"}, 
                new SelectListItem() {Value = "The Alliance", Text = "The Alliance"}, 
                new SelectListItem() {Value = "Villa-Vecchia", Text = "Villa-Vecchia"}, 
                new SelectListItem() {Value = "PEN", Text = "PEN"}, 
                new SelectListItem() {Value = "Opti-port", Text = "Opti-port"}, 
                new SelectListItem() {Value = "ADO", Text = "ADO"}, 
                new SelectListItem() {Value = "C&E", Text = "C&E"}, 
                new SelectListItem() {Value = "Energeyes", Text = "Energeyes"}, 
                new SelectListItem() {Value = "IECP", Text = "IECP"}, 
                new SelectListItem() {Value = "IVA", Text = "IVA"}, 
                new SelectListItem() {Value = "Newton", Text = "Newton"}, 
                new SelectListItem() {Value = "ODX", Text = "ODX"} 
            };
            return otherAlliances;
        }

        public IEnumerable<SelectListItem> GetPracticeManagementSoftwareSelectList()
        {
            List<SelectListItem> practiceManagementSoftware = new List<SelectListItem>()
            {
        
                new SelectListItem() {Value = "", Text = ""},
                new SelectListItem() {Value = "RevolutionEHR", Text = "RevolutionEHR"},  
                new SelectListItem() {Value = "Officemate", Text = "Officemate"},  
                new SelectListItem() {Value = "Compulink", Text = "Compulink"}, 
                new SelectListItem() {Value = "Practice Director", Text = "Practice Director"}, 
                new SelectListItem() {Value = "MaximEyes", Text = "MaximEyes"}, 
                new SelectListItem() {Value = "OD Link", Text = "OD Link"}, 
                new SelectListItem() {Value = "Crystal PM", Text = "Crystal PM"}, 
                new SelectListItem() {Value = "Uprise", Text = "Uprise"}, 
                new SelectListItem() {Value = "My Vision  Express", Text = "My Vision  Express"}, 
                new SelectListItem() {Value = "Liquid EHR", Text = "Liquid EHR"} 
            };
            return practiceManagementSoftware;
        }


        public IEnumerable<SelectListItem> GetContactRolesSelectList()
        {
            List<SelectListItem> contactRoles = new List<SelectListItem>()
            {
                new SelectListItem() {Value = "Dr.", Text = "Doctor"},  
                new SelectListItem() {Value = "Office Mgr", Text = "Office Mgr"},  
                new SelectListItem() {Value = "Practice Mgr", Text = "Practice Mgr"}, 
                new SelectListItem() {Value = "Optical Mgr", Text = "Optical Mgr"}, 
                new SelectListItem() {Value = "CPA", Text = "CPA"}, 
                new SelectListItem() {Value = "Bookkeeper", Text = "Bookkeeper"}, 
                new SelectListItem() {Value = "Comptroller", Text = "Comptroller"}, 
                new SelectListItem() {Value = "Staff", Text = "Staff"}, 
            };
            return contactRoles;
        }


        public IEnumerable<SelectListItem> GetMemberStatusList()
        {
            List<SelectListItem> statusList = new List<SelectListItem>()
            {
                new SelectListItem() {Value = "", Text = ""},
                new SelectListItem() {Value = "New", Text = "New"},
                new SelectListItem() {Value = "Active", Text = "Active"},
                new SelectListItem() {Value = "Suspended", Text = "Suspended"},
                new SelectListItem() {Value = "Cancelled", Text = "Cancelled"},
            };

            return statusList;
        }


        public IEnumerable<SelectListItem> GetMemberBoardingStatusList()
        {
            List<SelectListItem> memberBoardingStatusList = new List<SelectListItem>()
            {
                new SelectListItem() {Value = "New", Text = "New"},
                new SelectListItem() {Value = "Inprogress", Text = "Inprogress"},
                new SelectListItem() {Value = "Onhold", Text = "Onhold"},
                new SelectListItem() {Value = "Scheduled", Text = "Scheduled"},
                new SelectListItem() {Value = "Completed", Text = "Completed"},
            };

            return memberBoardingStatusList;
        }

        public IEnumerable<SelectListItem> GetPracticeSizeList()
        {
            List<SelectListItem> practiceSizeListList = new List<SelectListItem>()
            {
                new SelectListItem() {Value = "", Text = ""},
                new SelectListItem() {Value = "$0-$500K", Text = "$0-$500K"},
                new SelectListItem() {Value = "$500K-$750K", Text = "$500K-$750K"},
                new SelectListItem() {Value = "$750K-$1000K", Text = "$750K-$1000K"},
                new SelectListItem() {Value = "$1000K +", Text = "$1000K +"},
            };

            return practiceSizeListList;
        }

        public IEnumerable<SelectListItem> GetRebateProcessingPeriodSelectList()
        {
            int PeriodTypeID = Constants.quarterlyPeriodTypeID;

            var listData = db.Database.SqlQuery<C_Periods>("exec dbo.[LB_GetRebatePeriodsForProcessing] @PeriodTypeID",
             new SqlParameter("@PeriodTypeID", PeriodTypeID)
             ).ToList();

            var thisList = listData
            .Select(listItem => new SelectListItem
            {
                Value = listItem.PeriodID.ToString(),
                Text = listItem.PeriodName
            });

            return new SelectList(thisList, "Value", "Text");
        }

        public IEnumerable<SelectListItem> GetRebatePostingPeriodSelectList()
        {
            int PeriodTypeID = Constants.quarterlyPeriodTypeID;

            var listData = db.Database.SqlQuery<C_Periods>("exec dbo.[LB_GetRebatePeriodsForPosting] @PeriodTypeID",
             new SqlParameter("@PeriodTypeID", PeriodTypeID)
             ).ToList();

            var thisList = listData
            .Select(listItem => new SelectListItem
            {
                Value = listItem.PeriodID.ToString(),
                Text = listItem.PeriodName
            });

            return new SelectList(thisList, "Value", "Text");
        }

        public IEnumerable<SelectListItem> GetCommissionProcessingPeriodSelectList()
        {
            int PeriodTypeID = Constants.monthlyPeriodTypeID;

            var listData = db.Database.SqlQuery<C_Periods>("exec dbo.[LB_GetCommissionPeriodsForProcessing] @PeriodTypeID",
             new SqlParameter("@PeriodTypeID", PeriodTypeID)
             ).ToList();

            var thisList = listData
            .Select(listItem => new SelectListItem
            {
                Value = listItem.PeriodID.ToString(),
                Text = listItem.PeriodName
            });

            return new SelectList(thisList, "Value", "Text");
        }

        public IEnumerable<SelectListItem> GetCommissionPostingPeriodSelectList()
        {
            int PeriodTypeID = Constants.monthlyPeriodTypeID;

            var listData = db.Database.SqlQuery<C_Periods>("exec dbo.[LB_GetCommissionPeriodsForPosting] @PeriodTypeID",
             new SqlParameter("@PeriodTypeID", PeriodTypeID)
             ).ToList();

            var thisList = listData
            .Select(listItem => new SelectListItem
            {
                Value = listItem.PeriodID.ToString(),
                Text = listItem.PeriodName
            });

            return new SelectList(thisList, "Value", "Text");
        }

        public IEnumerable<SelectListItem> GetEnrollmentMeetingList()
        {
            List<EnrollmentMeetingBasic> meetingData = new List<EnrollmentMeetingBasic>();
            meetingData = db.Database.SqlQuery<EnrollmentMeetingBasic>("exec dbo.LB_GetEventHeaderStudyGroups"
            ).ToList();

            var allMeeting = new EnrollmentMeetingBasic
            {
                ID = Constants.orphanMeetingID,
                EventTitle = "(optional)"
            };
            meetingData.Insert(0, allMeeting);
            var thisList = meetingData
            .Select(listItem => new SelectListItem
            {
                Value = listItem.ID.ToString(),
                Text = listItem.City + ' ' + listItem.State + ' ' + listItem.EventStartDate + ' ' + listItem.EventTitle
            });

            return new SelectList(thisList, "Value", "Text");
        }

        public IEnumerable<SelectListItem> GetMeetingFormCommissionsList()
        {
            List<EnrollmentMeetingBasic> meetingData = new List<EnrollmentMeetingBasic>();
            meetingData = db.Database.SqlQuery<EnrollmentMeetingBasic>("exec dbo.LB_GetEventHeaderMeetingsForCommissions"
            ).ToList();

            var allMeeting = new EnrollmentMeetingBasic
            {
                EventID = 0,
                EventTitle = "(optional)"
            };
            meetingData.Insert(0, allMeeting);
            var thisList = meetingData
            .Select(listItem => new SelectListItem
            {
                Value = listItem.ID.ToString(),
                Text = listItem.City + ' ' + listItem.State + ' ' + listItem.EventStartDate + ' ' + listItem.HostName + ' ' + listItem.EventTitle
            });

            return new SelectList(thisList, "Value", "Text");
        }

        public IEnumerable<SelectListItem> GetRebateVendorsList()
        {
            List<Vendor> vendorData = new List<Vendor>();
            vendorData = db.Database.SqlQuery<Vendor>("exec dbo.LB_GetVendorList '', 'Active', 7, 1"
            ).OrderBy(m => m.Company).ToList();

            var allVendor = new Vendor
            {
                CustID = 0,
                Company = "(optional)"
            };
            vendorData.Insert(0, allVendor);
            var thisList = vendorData
            .Select(listItem => new SelectListItem
            {
                Value = listItem.CustID.ToString(),
                Text = listItem.Company
            });

            return new SelectList(thisList, "Value", "Text");
        }

        public IEnumerable<SelectListItem> GetProgramList(int CustID = 0, bool isRequired = false, bool isAll = false)
        {
            List<MemberProgramBasic> programData = new List<MemberProgramBasic>();
            programData = db.Database.SqlQuery<MemberProgramBasic>("exec dbo.[LB_GetProgramItemsForMember] @CustID",
              new SqlParameter("@CustID", CustID)
              ).ToList().OrderBy(m => m.ItemName).ToList();

            if (isAll)
            {
                var allProgram = new MemberProgramBasic
                {
                    ItemID = 0,
                    ItemName = "All"
                };
                programData.Insert(0, allProgram);
            }
            else
            {
                if (!isRequired)
                {
                    var allProgram = new MemberProgramBasic
                    {
                        ItemID = 0,
                        ItemName = "(optional)"
                    };
                    programData.Insert(0, allProgram);
                }
            }

            var thisList = programData
            .Select(listItem => new SelectListItem
            {
                Value = listItem.ItemID.ToString(),
                Text = listItem.ItemName
            });

            return new SelectList(thisList, "Value", "Text");
        }

        public IEnumerable<SelectListItem> GetCounterList(int? maxCounter)
        {
            if (maxCounter == null)
            {
                maxCounter = 3;
            }
            List<SelectListItem> counterList = new List<SelectListItem>();
            for (int i = 3; i <= maxCounter; i++)
            {
                counterList.Add(new SelectListItem { Value = i.ToString(), Text = i.ToString() });
            }


            return counterList;
        }

        public IEnumerable<SelectListItem> GetMemberVendorRequirementStatusList()
        {
            List<SelectListItem> requirementStatusTypes = new List<SelectListItem>()
            {
                new SelectListItem() {Value = "Pending", Text = "Pending"},
                new SelectListItem() {Value = "Waved", Text = "Waved"},
                new SelectListItem() {Value = "Completed", Text = "Completed"}
            };

            return requirementStatusTypes;
        }

        public IEnumerable<SelectListItem> GetChangeConfirmedList()
        {
            List<SelectListItem> changeConfirmedList = new List<SelectListItem>()
            {
                new SelectListItem() {Value = " ", Text = "All"},
                new SelectListItem() {Value = "Y", Text = "Yes"},
                new SelectListItem() {Value = "N", Text = "No"},
            };

            return changeConfirmedList;
        }

        public IEnumerable<SelectListItem> GetContractProviderList(bool isRequired = false, bool isAll = false)
        {
            List<Contact> vendorData = new List<Contact>();
            vendorData = db.Database.SqlQuery<Contact>("exec dbo.[LB_GetContractProviderList] "
            ).ToList();

            if (isAll)
            {
                var allValue = new Contact
                {
                    CustID = 0,
                    Company = "All"
                };
                vendorData.Insert(0, allValue);
            }
            else
            {
                if (!isRequired)
                {
                    var allValue = new Contact
                    {
                        CustID = 0,
                        Company = "(optional)"
                    };
                    vendorData.Insert(0, allValue);
                }
            }
            var vendorList = vendorData
            .Select(vendor => new SelectListItem
            {
                Value = vendor.CustID.ToString(),
                Text = vendor.Company
            });

            return new SelectList(vendorList, "Value", "Text");
        }

        public IEnumerable<SelectListItem> GetVendorsWithRebatesList(bool isRequired = false, bool isAll = false)
        {
            List<Vendor> vendorData = new List<Vendor>();
            vendorData = db.Database.SqlQuery<Vendor>("exec dbo.[LB_GetVendorsWithRebatedList] "
            ).ToList();

            if (isAll)
            {
                var allValue = new Vendor
                {
                    CustID = 0,
                    DisplayName = "All"
                };
                vendorData.Insert(0, allValue);
            }
            else
            {
                if (!isRequired)
                {
                    var allValue = new Vendor
                    {
                        CustID = 0,
                        DisplayName = "(optional)"
                    };
                    vendorData.Insert(0, allValue);
                }
            }
            var vendorList = vendorData
            .Select(vendor => new SelectListItem
            {
                Value = vendor.CustID.ToString(),
                Text = vendor.DisplayName
            });

            return new SelectList(vendorList, "Value", "Text");
        }

        
        public IEnumerable<SelectListItem> GetCustomerTypeList(bool isRequired = false, bool isAll = false)
        {
            List<SelectListItem> customerType = new List<SelectListItem>()
            {
                new SelectListItem() {Value = "6", Text = "Members"},
                new SelectListItem() {Value = "66", Text = "Locations"},
                new SelectListItem() {Value = "61", Text = "Doctors"},
            };

            if (isAll)
            {
                customerType.Insert(0, (new SelectListItem { Text = "All", Value = "0" }));
            }
            else
            {
                if (!isRequired)
                {
                    customerType.Insert(0, (new SelectListItem { Text = "Select", Value = "1" }));
                }
            }
            return customerType;
        }


    }
}
