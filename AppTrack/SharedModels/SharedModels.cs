using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Security;
using AppTrack.Models;
using System.Web.Mvc;

namespace AppTrack.SharedModels
{
    public class ClientsList
    {

        public ClientsList(string firstname, string lastname, DateTime dob, string email)
        {
            this.FirstName = firstname;
            this.LastName = lastname;
            this.Dob = dob;
            this.Email = email;
        }

        public string FirstName { set; get; }
        public string LastName { set; get; }
        [DataType(DataType.EmailAddress)]
        public string Email { set; get; }
        [DataType(DataType.DateTime)]
        public DateTime Dob { set; get; }
    }


    public class AdminUser
    {
        public string ID { get; set; }
        public int AdminID { get; set; }
        public string DisplayName { get; set; }
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        public int StatusID { get; set; }
    }

    public class MemberInfoDetail
    {
        [Display(Name = "Member ID")]
        public int? CustID { get; set; }

        [Display(Name = "Reason for Joining")]
        public string EnrollmentReason { get; set; }

        [Display(Name = "Size of Practice")]
        public string PracticeSize { get; set; }

        [Display(Name = "Practice Management Software")]
        public string PracticeSoftware { get; set; }

        [Display(Name = "License Number for Dr.")]
        public string LicenseNumber { get; set; }

        [Display(Name = "Continuing Education ID for Dr.")]
        public string OETracker { get; set; }
    }

    public class Member
    {
        [Display(Name = "Member ID")]
        public int? CustID { get; set; }

        [Display(Name = "Customer Type")]
        public int CustomerType { get; set; }

        [Display(Name = "Event ID")]
        public int? EventID { get; set; }

        [Required]
        [Display(Name = "Sales Rep ID")]
        public int? SponsorID { get; set; }

        [Display(Name = "IMD ID")]
        public int? SecSponsorID { get; set; }

        [Display(Name = "Primary Practice ID")]
        public int? ParentID { get; set; }

        [Display(Name = "AppTrack ID")]
        public string SecID { get; set; }

        [Display(Name = "Sales Force ID")]
        [MinLength(5)]
        [MaxLength(20)]
        public string SalesForceID { get; set; }

        [Display(Name = "Rebate Payee ID")]
        [RegularExpression("([1-9][0-9]*)", ErrorMessage = "Please enter a valid Member ID value")]
        public int? PayoutCustID { get; set; }

        [Display(Name = "Sage ID")]
        public string AccountingID { get; set; }

        [Display(Name = "Member ID")]
        [RegularExpression("([1-9][0-9]*)", ErrorMessage = "Please enter a valid Member ID value")]
        public int? MemberID { get; set; }

        [Display(Name = "Source Meeting ID")]
        public int? SourceID { get; set; }

        [Display(Name = "Meeting Code")]
        public string SourceCode { get; set; }

        [Display(Name = "Tax ID")]
        [MinLength(9)]
        [MaxLength(11)]
        [RegularExpression(@"^\d{9}|\d{3}-\d{2}-\d{4}|\d{2}-\d{7}$", ErrorMessage = "Invalid Tax ID Number")]
        public string TaxID { get; set; }

        [Display(Name = "TIN Name")]
        [MaxLength(50)]
        public string Company { get; set; }

        [Display(Name = "Title")]
        [MaxLength(10)]
        public string NameTitle { get; set; }

        [Required, Display(Name = "First Name")]
        [MaxLength(50)]
        public string FirstName { get; set; }

        [Required, Display(Name = "Last Name")]
        [MaxLength(50)]
        public string LastName { get; set; }

        [Required, Display(Name = "Practice Name")]
        [MaxLength(50)]
        public string DisplayName { get; set; }

        [Required, Display(Name = "Address Line 1")]
        [MaxLength(50)]
        public string Address1 { get; set; }

        [Display(Name = "Address Line 2")]
        [MaxLength(50)]
        public string Address2 { get; set; }

        [Required]
        [MaxLength(50)]
        public string City { get; set; }

        [Required]
        [MaxLength(50)]
        public string State { get; set; }

        [Required, Display(Name = "Zip Code")]
        [DataType(DataType.PostalCode)]
        public string PostalCode { get; set; }

        [Required, Display(Name = "Email")]
        [MaxLength(100)]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Display(Name = "Practice Office Phone")]
        [DataType(DataType.PhoneNumber)]
        public string CompanyPhone { get; set; }

        [Required, Display(Name = "Contact Phone")]
        [DataType(DataType.PhoneNumber)]
        public string DayPhone { get; set; }

        [Display(Name = "Mobile")]
        [DataType(DataType.PhoneNumber)]
        public string Mobile { get; set; }

        [DataType(DataType.PhoneNumber)]
        public string Fax { get; set; }

        [Display(Name = "SiteName")]
        [DataType(DataType.Url)]
        [MaxLength(500)]
        public string SiteName { get; set; }

        [Display(Name = "Email Flag")]
        public byte? EmailFlag { get; set; }

        [Display(Name = "Text Flag")]
        public byte? TextFlag { get; set; }

        [Display(Name = "Status")]
        [MaxLength(30)]
        public string Status { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public Nullable<System.DateTime> StatusDate { get; set; }

        [Display(Name = "Boarding Status")]
        [MaxLength(50)]
        public string ActivationStatus { get; set; }

        [Display(Name = "Boarding Status Date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public Nullable<System.DateTime> ActivationStatusDate { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public Nullable<System.DateTime> StartDate { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public Nullable<System.DateTime> PostDate { get; set; }

        [Display(Name = "StatusID")]
        public int StatusID { get; set; }

        [Display(Name = "AdminID")]
        public int AdminID { get; set; }

        [Display(Name = "ContactType")]
        public string VariantData1 { get; set; }

        [Display(Name = "License Number for Dr.")]
        public string VariantData2 { get; set; }

        [Display(Name = "Continuing Education ID for Dr.")]
        public string VariantData3 { get; set; }

        public string VariantData4 { get; set; }

        [Display(Name = "Reason for Cancel/Inactive")]
        public string CancellationReason { get; set; }

        [Display(Name = "ID of Previous Practice if Sale Or Transfer has occured")]
        public string SecCode { get; set; }

        [Display(Name = "Number of Staff")]
        public int? Flag1 { get; set; }

        [Display(Name = "TBD")]
        public int? Flag2 { get; set; }

        [Display(Name = "TBD")]
        public int? Flag3 { get; set; }

        [Display(Name = "TBD")]
        public int? Flag4 { get; set; }

        [Display(Name = "Reason for Joining")]
        public string EnrollmentReason { get; set; }

        [Display(Name = "Size of Practice")]
        public string PracticeSize { get; set; }

        [Display(Name = "Practice Management Software")]
        public string PracticeSoftware { get; set; }

    }

    public class Location
    {
        [Display(Name = "Location ID")]
        [RegularExpression("([1-9][0-9]*)", ErrorMessage = "Please enter a valid Location ID value")]
        public int? CustID { get; set; }

        [Display(Name = "Sales Rep")]
        public int? SponsorID { get; set; }

        [Display(Name = "IMD ID")]
        public int? SecSponsorID { get; set; }

        [Display(Name = "Primary Practice ID")]
        [RegularExpression("([1-9][0-9]*)", ErrorMessage = "Please enter a valid Member ID value")]
        public int? ParentID { get; set; }

        [Display(Name = "AppTrack ID")]
        public string SecID { get; set; }

        [Display(Name = "Sales Force ID")]
        [MinLength(5)]
        [MaxLength(20)]
        public string SalesForceID { get; set; }

        [Display(Name = "Rebate Payee ID")]
        [RegularExpression("([1-9][0-9]*)", ErrorMessage = "Please enter a valid Member ID value")]
        public int? PayoutCustID { get; set; }

        [Display(Name = "Sage ID")]
        public string AccountingID { get; set; }

        [Display(Name = "Tax ID")]
        [MinLength(9)]
        [MaxLength(11)]
        [RegularExpression(@"^\d{9}|\d{3}-\d{2}-\d{4}|\d{2}-\d{7}$", ErrorMessage = "Invalid Tax ID Number")]
        public string TaxID { get; set; }

        [Required]
        [Display(Name = "TIN Name")]
        [MaxLength(50)]
        public string Company { get; set; }

        [Display(Name = "Title")]
        [MaxLength(10)]
        public string NameTitle { get; set; }

        [Required, Display(Name = "First Name")]
        [MaxLength(50)]
        public string FirstName { get; set; }

        [Required, Display(Name = "Last Name")]
        [MaxLength(50)]
        public string LastName { get; set; }

        [Required]
        [Display(Name = "Practice Name")]
        [MaxLength(50)]
        public string DisplayName { get; set; }

        [Required, Display(Name = "Address Line 1")]
        [MaxLength(50)]
        public string Address1 { get; set; }

        [Display(Name = "Address Line 2")]
        [MaxLength(50)]
        public string Address2 { get; set; }

        [Required]
        [MaxLength(50)]
        public string City { get; set; }

        [Required]
        [MaxLength(50)]
        public string State { get; set; }

        [Required, Display(Name = "Zip Code")]
        [DataType(DataType.PostalCode)]
        public string PostalCode { get; set; }

        [Required, Display(Name = "Email")]
        [MaxLength(100)]
        public string Email { get; set; }

        [Display(Name = "Email Flag")]
        public byte? EmailFlag { get; set; }

        [Display(Name = "Text Flag")]
        public byte? TextFlag { get; set; }

        [Display(Name = "Location Phone")]
        [DataType(DataType.PhoneNumber)]
        public string CompanyPhone { get; set; }

        [Required, Display(Name = "Contact Phone")]
        [DataType(DataType.PhoneNumber)]
        public string DayPhone { get; set; }

        [Display(Name = "Mobile")]
        [DataType(DataType.PhoneNumber)]
        public string Mobile { get; set; }

        [Display(Name = "Location Fax")]
        [DataType(DataType.PhoneNumber)]
        public string Fax { get; set; }

        [Display(Name = "Status")]
        [MaxLength(30)]
        public string Status { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public Nullable<System.DateTime> StatusDate { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public Nullable<System.DateTime> StartDate { get; set; }

        [Display(Name = "StatusID")]
        public int StatusID { get; set; }

        [Display(Name = "AdminID")]
        public int AdminID { get; set; }
    }

    public class Contact
    {
        [Display(Name = "Contact ID")]
        public Nullable<int> CustID { get; set; }

        [Display(Name = "Sales Force ID")]
        [MinLength(5)]
        [MaxLength(20)]
        public string SalesForceID { get; set; }

        [Display(Name = "Member ID")]
        public Nullable<int> SponsorID { get; set; }

        public string Company { get; set; }
        [Display(Name = "Title")]
        [MaxLength(10)]
        public string NameTitle { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        public string Email { get; set; }
        [Display(Name = "Contact Phone")]
        [DataType(DataType.PhoneNumber)]
        public string DayPhone { get; set; }
        [Display(Name = "Mobile")]
        [DataType(DataType.PhoneNumber)]
        public string Mobile { get; set; }
        public string Fax { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }

        [Display(Name = "Contact Type")]
        public string ContactType { get; set; }
        [Display(Name = "Variant Data")]
        public string VariantData1 { get; set; }
        [Display(Name = "License Number")]
        public string VariantData2 { get; set; }
        [Display(Name = "O/E Tracker")]
        public string VariantData3 { get; set; }

        [Display(Name = "Status")]
        [MaxLength(30)]
        public string Status { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public Nullable<System.DateTime> StatusDate { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public Nullable<System.DateTime> StartDate { get; set; }

        public int StatusID { get; set; }
        public Nullable<int> AdminID { get; set; }

    }

    public class MemberVendor
    {
        [Display(Name = "Category Name")]
        [MaxLength(50)]
        public string CategoryName { get; set; }
        [Display(Name = "Vendor ID")]
        public int CustID { get; set; }
        [Display(Name = "Company Name")]
        [MaxLength(50)]
        public string Company { get; set; }
        [Display(Name = "Display Name")]
        [MaxLength(50)]
        public string DisplayName { get; set; }
        [Display(Name = "Website URL")]
        [MaxLength(500)]
        [DataType(DataType.Url, ErrorMessage = "Please enter a valid URL")]
        public string SiteName { get; set; }
        [Display(Name = "Member Participation Y/N")]
        public int Flag1 { get; set; }
        [Display(Name = "Member Notifications Y/N")]
        public int Flag2 { get; set; }
        [Display(Name = "Has Requirements Y/N")]
        public int Flag3 { get; set; }

        [Display(Name = "Member ID")]
        public int Member { get; set; }
        [Display(Name = "Has Requirement")]
        public int HasRequirement { get; set; }
        [Display(Name = "Met Requirements")]
        public int MetRequirement { get; set; }
        [Display(Name = "Vendor/Member Payee ID")]
        public string VendorMemberPayeeID { get; set; }

        [Display(Name = "ProductID")]
        public int ProductID { get; set; }
        [Display(Name = "ProgramID")]
        public int ProgramID { get; set; }
        [Display(Name = "AppTrack Program")]
        public int C_ProgramID { get; set; }
        [Display(Name = "Participation")]
        public string Participation { get; set; }
        [Display(Name = "Program Name")]
        public string ProgramName { get; set; }
        [Display(Name = "Program Requirements")]
        public string ProgramRequirements { get; set; }
        [Display(Name = "Requirement Status")]
        public string RequirementStatus { get; set; }
        [Display(Name = "RequirementStatusDate")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public Nullable<System.DateTime> RequirementStatusDate { get; set; }

        public string Status { get; set; }
        public int StatusID { get; set; }
        public Nullable<int> AdminID { get; set; }
    }

    public class RebatePayeeOld
    {
        [Display(Name = "Repate Payee ID")]
        [RegularExpression("([1-9][0-9]*)", ErrorMessage = "Please enter a valid Member ID value")]
        public int? CustID { get; set; }

        [Display(Name = "Member/Location ID")]
        [RegularExpression("([1-9][0-9]*)", ErrorMessage = "Please enter a valid Member ID value")]
        public int? ParentID { get; set; }

        [Display(Name = "Member/Location ID")]
        [RegularExpression("([1-9][0-9]*)", ErrorMessage = "Please enter a valid Member ID value")]
        public int? SponsorID { get; set; }

        [Display(Name = "Sage ID")]
        [RegularExpression("([1-9][0-9]*)", ErrorMessage = "Please enter a valid Member ID value")]
        public string AccountingID { get; set; }

        [MinLength(9)]
        [MaxLength(11)]
        [RegularExpression(@"^\d{9}|\d{3}-\d{2}-\d{4}|\d{2}-\d{7}$", ErrorMessage = "Invalid Tax ID Number")]
        public string TaxID { get; set; }

        [Display(Name = "Payee Name")]
        [MaxLength(50)]
        public string Company { get; set; }

        [Required, Display(Name = "Name Title")]
        [MaxLength(10)]
        public string NameTitle { get; set; }

        [Required, Display(Name = "First Name")]
        [MaxLength(50)]
        public string FirstName { get; set; }

        [Required, Display(Name = "Last Name")]
        [MaxLength(50)]
        public string LastName { get; set; }

        [Display(Name = "Payee Name")]
        [MaxLength(50)]
        public string DisplayName { get; set; }

        [Required, Display(Name = "Address Line 1")]
        [MaxLength(50)]
        public string Address1 { get; set; }

        [Display(Name = "Address Line 2")]
        [MaxLength(50)]
        public string Address2 { get; set; }

        [Required]
        [MaxLength(50)]
        public string City { get; set; }

        [Required]
        [MaxLength(50)]
        public string State { get; set; }

        [Required, Display(Name = "Zip Code")]
        [DataType(DataType.PostalCode)]
        public string PostalCode { get; set; }

        [Required, Display(Name = "Email")]
        [MaxLength(100)]
        public string Email { get; set; }

        [Display(Name = "Contact Phone (Day)")]
        [DataType(DataType.PhoneNumber)]
        public string DayPhone { get; set; }

        [Display(Name = "Contact Phone(Evenings)")]
        [DataType(DataType.PhoneNumber)]
        public string Mobile { get; set; }

        [Display(Name = "Status")]
        [MaxLength(30)]
        public string Status { get; set; }

        [Display(Name = "Status Date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public Nullable<System.DateTime> StatusDate { get; set; }

        [Display(Name = "Start Date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public Nullable<System.DateTime> StartDate { get; set; }

        [Display(Name = "StatusID")]
        public int StatusID { get; set; }

        [Display(Name = "AdminID")]
        public int AdminID { get; set; }
    }

    public class RebatePayee
    {
        public int? CustomerType { get; set; }
        public int? ParentID { get; set; }
        public int? PayoutSeparateFromParent { get; set; }

        [Display(Name = "Member/Location ID")]
        [RegularExpression("([1-9][0-9]*)", ErrorMessage = "Please enter a valid Member/Location ID value")]
        public int? CustID { get; set; }

        [Display(Name = "Payee ID")]
        [RegularExpression("([1-9][0-9]*)", ErrorMessage = "Please enter a valid Payee ID value")]
        public int? PayoutCustID { get; set; }

        [Display(Name = "First Name")]
        [MaxLength(50)]
        public string ShipFirstName { get; set; }

        [Display(Name = "Last Name")]
        [MaxLength(50)]
        public string ShipLastName { get; set; }

        [Display(Name = "Payee Name")]
        [MaxLength(50)]
        public string ShipName { get; set; }

        [Required, Display(Name = "Address Line 1")]
        [MaxLength(50)]
        public string ShipAddress1 { get; set; }

        [Display(Name = "Address Line 2")]
        [MaxLength(50)]
        public string ShipAddress2 { get; set; }

        [Required, Display(Name = "City")]
        [MaxLength(50)]
        public string ShipCity { get; set; }

        [Required, Display(Name = "State")]
        [MaxLength(50)]
        public string ShipState { get; set; }

        [Required, Display(Name = "Postal Code")]
        [DataType(DataType.PostalCode)]
        public string ShipPostalCode { get; set; }

        [Display(Name = "Payee Email")]
        [MaxLength(100)]
        public string ShipEmail { get; set; }

        [Display(Name = "Payee Phone")]
        [DataType(DataType.PhoneNumber)]
        public string ShipPhone { get; set; }

        [Display(Name = "AdminID")]
        public int AdminID { get; set; }
    }

    public class MemberDirector
    {
        [Display(Name = "Member Director ID")]
        public int CustID { get; set; }

        [Display(Name = "Member ID")]
        [RegularExpression("([1-9][0-9]*)", ErrorMessage = "Please enter a valid Member ID value")]
        public int? SponsorID { get; set; }

        [Required(ErrorMessage = "Sage ID is required"), Display(Name = "Sage ID")]
        public string AccountingID { get; set; }

        [Required(ErrorMessage = "Tax Id value is required"), Display(Name = "Tax ID")]
        [MinLength(9)]
        [MaxLength(11)]
        [RegularExpression(@"^\d{9}|\d{3}-\d{2}-\d{4}|\d{2}-\d{7}$", ErrorMessage = "Invalid Tax ID Number")]
        public string TaxID { get; set; }

        [Required, Display(Name = "TIN Name")]
        [MaxLength(50)]
        public string Company { get; set; }

        [Display(Name = "Title")]
        [MaxLength(10)]
        public string NameTitle { get; set; }

        [Required, Display(Name = "First Name")]
        [MaxLength(50)]
        public string FirstName { get; set; }

        [Required, Display(Name = "Last Name")]
        [MaxLength(50)]
        public string LastName { get; set; }

        [Display(Name = "Display Name")]
        [MaxLength(50)]
        public string DisplayName { get; set; }

        [Required, Display(Name = "Address Line 1")]
        [MaxLength(50)]
        public string Address1 { get; set; }

        [Display(Name = "Address Line 2")]
        [MaxLength(50)]
        public string Address2 { get; set; }

        [Required]
        [MaxLength(50)]
        public string City { get; set; }

        [Required]
        [MaxLength(50)]
        public string State { get; set; }

        [Required, Display(Name = "Zip Code")]
        [DataType(DataType.PostalCode)]
        public string PostalCode { get; set; }

        [Required, Display(Name = "Email")]
        [MaxLength(100)]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Display(Name = "Contact Phone")]
        [MinLength(10)]
        [MaxLength(21)]
        [DataType(DataType.PhoneNumber)]
        public string DayPhone { get; set; }

        [Display(Name = "Mobile")]
        [MinLength(10)]
        [MaxLength(21)]
        [DataType(DataType.PhoneNumber)]
        public string Mobile { get; set; }

        [MinLength(10)]
        [MaxLength(21)]
        [DataType(DataType.PhoneNumber)]
        public string Fax { get; set; }

        [Display(Name = "Status")]
        [MaxLength(30)]
        public string Status { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public Nullable<System.DateTime> StatusDate { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public Nullable<System.DateTime> StartDate { get; set; }

        [Display(Name = "StatusID")]
        public int StatusID { get; set; }

        [Display(Name = "AdminID")]
        public int AdminID { get; set; }
    }

    public class SalesRep
    {
        [Display(Name = "CustID")]
        public Nullable<int> CustID { get; set; }
        [Display(Name = "Sales Force ID")]
        [MinLength(5)]
        [MaxLength(20)]
        public string SalesForceID { get; set; }
        public string DisplayName { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        [Display(Name = "Tax ID")]
        [MinLength(9)]
        [MaxLength(11)]
        [RegularExpression(@"^\d{9}|\d{3}-\d{2}-\d{4}|\d{2}-\d{7}$", ErrorMessage = "Invalid Tax ID Number")]
        public string TaxID { get; set; }
        public string Status { get; set; }
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public Nullable<System.DateTime> StatusDate { get; set; }
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public Nullable<System.DateTime> StartDate { get; set; }
        public int StatusID { get; set; }
        public Nullable<int> AdminID { get; set; }

    }

    public class Vendor
    {
        [Display(Name = "Vendor ID")]
        public int CustID { get; set; }
        public int SponsorID { get; set; }
        [Display(Name = "Company Name")]
        [MaxLength(50)]
        public string Company { get; set; }
        [Display(Name = "Display Name")]
        [MaxLength(100)]
        public string DisplayName { get; set; }
        [Display(Name = "Title")]
        [MaxLength(10)]
        public string NameTitle { get; set; }
        [Required, Display(Name = "First Name")]
        [MaxLength(50)]
        public string FirstName { get; set; }
        [Required, Display(Name = "Last Name")]
        [MaxLength(50)]
        public string LastName { get; set; }
        [MaxLength(100)]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        [Display(Name = "Address Line 1")]
        [MaxLength(50)]
        public string Address1 { get; set; }
        [Display(Name = "Address Line 2")]
        [MaxLength(50)]
        public string Address2 { get; set; }
        [MaxLength(50)]
        public string City { get; set; }
        [MaxLength(50)]
        public string State { get; set; }
        [Display(Name = "Zip Code")]
        [DataType(DataType.PostalCode)]
        public string PostalCode { get; set; }
        [Display(Name = "Company Phone")]
        [DataType(DataType.PhoneNumber)]
        public string CompanyPhone { get; set; }
        [Display(Name = "Contact Phone")]
        [DataType(DataType.PhoneNumber)]
        public string DayPhone { get; set; }
        [Display(Name = "Mobile")]
        [DataType(DataType.PhoneNumber)]
        public string Mobile { get; set; }
        [DataType(DataType.PhoneNumber)]
        public string Fax { get; set; }
        [MaxLength(500)]
        [DataType(DataType.ImageUrl)]
        public string Logo { get; set; }
        [Display(Name = "Website URL")]
        [MaxLength(500)]
        [DataType(DataType.Url, ErrorMessage = "Please enter a valid URL")]
        public string SiteName { get; set; }
        [Display(Name = "Start Date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public Nullable<System.DateTime> StartDate { get; set; }
        [Display(Name = "End Date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public Nullable<System.DateTime> EndDate { get; set; }
        [Display(Name = "Display Y/N")]
        public int HideFlag { get; set; }
        [MaxLength(30)]
        public string Status { get; set; }
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public Nullable<System.DateTime> StatusDate { get; set; }

        [Display(Name = "Account Number")]
        public string SecID { get; set; }

        [Display(Name = "Account Details")]
        public string VariantData1 { get; set; }

        public string VariantData2 { get; set; }

        public int Flag1 { get; set; }
        public int Flag2 { get; set; }
        public int StatusID { get; set; }
        public Nullable<int> AdminID { get; set; }
    }

    public class VendorContact
    {
        [Display(Name = "Contact ID")]
        public int CustID { get; set; }
        [Display(Name = "Vendor ID")]
        public int SponsorID { get; set; }
        [Display(Name = "Contact Type")]
        public string ContactType { get; set; }
        [Display(Name = "Display Name")]
        [MaxLength(100)]
        public string DisplayName { get; set; }
        [Display(Name = "Title")]
        [MaxLength(10)]
        public string NameTitle { get; set; }
        [Required, Display(Name = "First Name")]
        [MaxLength(50)]
        public string FirstName { get; set; }
        [Required, Display(Name = "Last Name")]
        [MaxLength(50)]
        public string LastName { get; set; }
        [MaxLength(100)]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        [Display(Name = "Address Line 1")]
        [MaxLength(50)]
        public string Address1 { get; set; }
        [Display(Name = "Address Line 2")]
        [MaxLength(50)]
        public string Address2 { get; set; }
        [MaxLength(50)]
        public string City { get; set; }
        [MaxLength(50)]
        public string State { get; set; }
        [Display(Name = "Zip Code")]
        [DataType(DataType.PostalCode)]
        public string PostalCode { get; set; }

        [Display(Name = "Contact Phone")]
        [DataType(DataType.PhoneNumber)]
        public string DayPhone { get; set; }
        [Display(Name = "Mobile")]
        [DataType(DataType.PhoneNumber)]
        public string Mobile { get; set; }
        [DataType(DataType.PhoneNumber)]
        public string Fax { get; set; }

        [Display(Name = "Start Date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public Nullable<System.DateTime> StartDate { get; set; }
        [Display(Name = "End Date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public Nullable<System.DateTime> EndDate { get; set; }
        [MaxLength(30)]
        public string Status { get; set; }
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public Nullable<System.DateTime> StatusDate { get; set; }
        [Display(Name = "Member Notifications Y/N")]
        public int Flag2 { get; set; }
        public int StatusID { get; set; }
        public Nullable<int> AdminID { get; set; }
    }

    public class VendorRebate
    {
        [Required, Display(Name = "Rebate ID")]
        public int VolumeID { get; set; }
        [Required, Display(Name = "Rebate Type")]
        public int VolumeType { get; set; }
        [Required, Display(Name = "Rebate Name")]
        public string VolumeName { get; set; }
        [Required, Display(Name = "Rebate Description")]
        public string VolumeDesc { get; set; }
        public Nullable<int> CustID { get; set; }
        public Nullable<int> StatusID { get; set; }
    }

    public class VendorProgram
    {
        [Display(Name = "Vendor Program ID")]
        public int ProgramID { get; set; }

        [Display(Name = "Vendor ID")]
        public int CustID { get; set; }

        [Display(Name = Constants.companyName + " Product")]
        public int C_ProgramID { get; set; }

        [Display(Name = Constants.companyName + " Product")]
        [MaxLength(100)]
        public string C_ProgramName { get; set; }

        [Required, Display(Name = "Program Name")]
        [MaxLength(100)]
        public string ProgramName { get; set; }

        [Required, Display(Name = "Program Summary")]
        [MaxLength(2500)]
        [AllowHtml]
        public string ProgramSummary { get; set; }

        [Required, Display(Name = "Description")]
        [MaxLength(8000)]
        [AllowHtml]
        public string ProgramDescription { get; set; }

        [Display(Name = "Requirements")]
        [MaxLength(2500)]
        [AllowHtml]
        public string ProgramRequirements { get; set; }

        [Display(Name = "Directions")]
        [MaxLength(2500)]
        [AllowHtml]
        public string ProgramDirections { get; set; }

        [Required, Display(Name = "Member Participation Required")]
        public int MemberParticipationRequired { get; set; }

        [Required, Display(Name = "Member Rebate")]
        [DataType(DataType.Currency)]
        [Range(0.0, 50.00,
            ErrorMessage = "Member Rebate must be a % between 0 and 50")]
        public decimal MemberRebate { get; set; }

        [Required, Display(Name = "Corporate Rebate")]
        [DataType(DataType.Currency)]
        [Range(0.0, 50.00,
            ErrorMessage = "Corporate Rebate must be a % between 0 and 50")]
        public decimal CorporateRebate { get; set; }

        [Display(Name = "Start Date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public Nullable<System.DateTime> StartDate { get; set; }

        [Display(Name = "End Date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public Nullable<System.DateTime> EndDate { get; set; }

        [MaxLength(30)]
        public string Status { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public Nullable<System.DateTime> StatusDate { get; set; }

        public int StatusID { get; set; }

        public Nullable<int> AdminID { get; set; }
    }

    public class VendorRequirement
    {
        [Display(Name = "Requirement ID")]
        public int RequirementID { get; set; }

        [Display(Name = "Vendor ID")]
        public int CustID { get; set; }

        [Required, Display(Name = "Program")]
        public int ProgramID { get; set; }

        [Display(Name = "Program Name")]
        public string ProgramName { get; set; }

        [Required, Display(Name = "Requirement Type")]
        [MaxLength(50)]
        public string RequirementType { get; set; }

        [Required, Display(Name = "Requirement Name")]
        [MaxLength(150)]
        public string RequirementName { get; set; }

        [Required, Display(Name = "Description")]
        [MaxLength(500)]
        public string RequirementDescription { get; set; }

        public int DocumentID { get; set; }

        [Display(Name = "Document Name")]
        public string DocumentName { get; set; }

        [Display(Name = "File Name")]
        public string FileName { get; set; }

        [Display(Name = "Template ID")]
        [MaxLength(50)]
        public string TemplateID { get; set; }

        [Display(Name = "Start Date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public Nullable<System.DateTime> StartDate { get; set; }

        [Display(Name = "End Date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public Nullable<System.DateTime> EndDate { get; set; }

        [MaxLength(30)]
        public string Status { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public Nullable<System.DateTime> StatusDate { get; set; }

        public int StatusID { get; set; }

        public Nullable<int> AdminID { get; set; }
    }

    public class Document
    {
        public int DocumentID { get; set; }

        public int CustID { get; set; }

        [Display(Name = "AppTrack ID")]
        public int SecID { get; set; }

        [Display(Name = "File Name")]
        [MaxLength(500)]
        public string FileName { get; set; }

        [Display(Name = "Path")]
        [MaxLength(500)]
        public string Path { get; set; }

        [Required, Display(Name = "DocuSign Template ID")]
        [StringLength(36, MinimumLength = 36, ErrorMessage = "Template ID must be 36 characters")]
        public string TemplateID { get; set; }

        [Display(Name = "Document Title")]
        [MaxLength(100)]
        public string DocumentTitle { get; set; }

        [Required, Display(Name = "Document Name")]
        [MaxLength(100)]
        public string DocumentName { get; set; }

        [Required, Display(Name = "Description")]
        [MaxLength(1500)]
        public string DocumentDescription { get; set; }

        [Display(Name = "Start Date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public Nullable<System.DateTime> StartDate { get; set; }

        [Display(Name = "End Date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public Nullable<System.DateTime> EndDate { get; set; }

        [Required, Display(Name = "Display on website?")]
        public int HideFlag { get; set; }

        [MaxLength(30)]
        public string Status { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public Nullable<System.DateTime> StatusDate { get; set; }

        public int StatusID { get; set; }

        public Nullable<int> AdminID { get; set; }
    }


    public class CustomerNote
    {
        public int NoteID { get; set; }
        public int CustID { get; set; }
        public int VendorID { get; set; }
        public int OwnerID { get; set; }
        public int ActivityID { get; set; }
        public string NoteType { get; set; }
        public string CommType { get; set; }
        public string CommDirection { get; set; }
        public string Title { get; set; }
        public string NoteReason { get; set; }
        [Required]
        public string NoteText { get; set; }
        [Display(Name = "Start Date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public Nullable<System.DateTime> StartDate { get; set; }
        [Display(Name = "Due Date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public Nullable<System.DateTime> ScheduledDate { get; set; }
        [Display(Name = "Completed On")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public Nullable<System.DateTime> EndDate { get; set; }
        public Nullable<int> AssignedTo { get; set; }
        [Display(Name = "Assigned On")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public Nullable<System.DateTime> AssignedDate { get; set; }
        public string Status { get; set; }
        public Nullable<int> AccessLevel { get; set; }
        public int AdminID { get; set; }
        [Display(Name = "Entered By")]
        public string AdminName { get; set; }
        [Display(Name = "Entered On")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public System.DateTime PostDate { get; set; }
        public string OwnerName { get; set; }
        public string AssignedToName { get; set; }
        public string MemberName { get; set; }
        public string VendorName { get; set; }
        public string ContactName { get; set; }
    }

    public class Region
    {
        public int RegionID { get; set; }
        public string RegionName { get; set; }
        public string RegionDescription { get; set; }
        public Nullable<int> Seqno { get; set; }
    }

    public class Item
    {
        public int ItemID { get; set; }
        [Display(Name = "Product Name")]
        public string ItemName { get; set; }
        [Display(Name = "Product Description")]
        public string ItemDescription { get; set; }
        public Nullable<int> StatusID { get; set; }
    }

    public class LookUpItem
    {
        public string LookUpID { get; set; }
        public int IDValue { get; set; }
        public int DataGroupID { get; set; }
        public string DataLabel { get; set; }
        public string Description { get; set; }
        public int StatusID { get; set; }
    }
    public class CheckCustomerResult
    {
        public int ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        public C_Info C_Info { get; set; }
        public string DisplayName { get; set; }
    }

    public class MeetingEvent
    {
        public int ID { get; set; }
        public int CategoryID { get; set; }
        public int CustID { get; set; }

        [Display(Name = "Sponsored by")]
        [MaxLength(250)]
        public string SponsorName { get; set; }

        [Display(Name = "Hosted by")]
        [MaxLength(100)]
        public string HostName { get; set; }

        [Required(ErrorMessage = "Topic is required")]
        [Display(Name = "Topic")]
        [MaxLength(250)]
        public string EventTitle { get; set; }

        [Display(Name = "Description")]
        [MaxLength(2500)]
        public string EventDescription { get; set; }

        [Required, Display(Name = "Date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public System.DateTime EventStartDate { get; set; }

        [Display(Name = "Date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public System.DateTime EventEndDate { get; set; }

        [MaxLength(50)]
        public string EventDateTimeString { get; set; }

        public Nullable<int> Capacity { get; set; }
        public Nullable<int> Reserved { get; set; }
        public Nullable<int> Available { get; set; }

        [Display(Name = "Location Name")]
        [MaxLength(250)]
        public string LocationTitle { get; set; }

        [Required, Display(Name = "Address Line 1")]
        [MaxLength(50)]
        public string Address1 { get; set; }

        [Display(Name = "Address Line 2")]
        [MaxLength(50)]
        public string Address2 { get; set; }

        [Required(ErrorMessage = "City is required"), MaxLength(50)]
        public string City { get; set; }

        [Required(ErrorMessage = "State is required"), MaxLength(50)]
        public string State { get; set; }

        [Required(ErrorMessage = "Zip is required"), Display(Name = "Zip Code")]
        [DataType(DataType.PostalCode)]
        public string PostalCode { get; set; }

        public string Status { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public Nullable<System.DateTime> StatusDate { get; set; }

        public int StatusID { get; set; }

        public int AdminID { get; set; }

        [Display(Name = "Entered On")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public System.DateTime PostDate { get; set; }
    }

    public class CommissionHeader
    {
        public int CHID { get; set; }
        public int CustID { get; set; }
        public string DisplayName { get; set; }
        public int CommissionID { get; set; }
        public int? ParentID { get; set; }
        public int? CustomerType { get; set; }
        public int? PayoutID { get; set; }
        public int? CheckNumber { get; set; }
        public string CommissionName { get; set; }
        public int PeriodID { get; set; }
        public string PeriodName { get; set; }
        [DisplayFormat(DataFormatString = "{0:c}")]
        public Nullable<decimal> Commission { get; set; }
        public Nullable<int> StatusID { get; set; }
        public string Status { get; set; }
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public System.DateTime StatusDate { get; set; }
        public System.DateTime PostDate { get; set; }
    }
    public class CommissionDetail
    {
        public int CDID { get; set; }
        public int CustID { get; set; }
        [Display(Name = "Sage ID")]
        public string AccountingID { get; set; }
        public string DisplayName { get; set; }
        public int CommissionID { get; set; }
        public string CommissionName { get; set; }
        public int PeriodID { get; set; }
        public string PeriodName { get; set; }
        [DisplayFormat(DataFormatString = "{0:c}")]
        public Nullable<decimal> VolumeTotal { get; set; }
        [DisplayFormat(DataFormatString = "{0:c}")]
        public Nullable<decimal> Commission { get; set; }
        public int StatusID { get; set; }
        public string Status { get; set; }
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public System.DateTime StatusDate { get; set; }
        public System.DateTime PostDate { get; set; }
        public Nullable<int> CheckNumber { get; set; }
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public Nullable<System.DateTime> PayoutDate { get; set; }
        [DisplayFormat(DataFormatString = "{0:c}")]
        public Nullable<decimal> PayoutAmount { get; set; }

    }
    public class VolumeDetail
    {
        public int CustID { get; set; }
        public int VolumeID { get; set; }
        public string VolumeName { get; set; }
        public int OrderID { get; set; }
        public int ItemID { get; set; }
        public int SourceID { get; set; }
        public string SourceName { get; set; }
        public string SourceDescription { get; set; }
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy hh:mm tt}", ApplyFormatInEditMode = true)]
        public System.DateTime SourceDate { get; set; }
        [DisplayFormat(DataFormatString = "{0:c}")]
        public decimal TotalSalesAmount { get; set; }
        [DisplayFormat(DataFormatString = "{0:c}")]
        public decimal VolumeTotal { get; set; }
        public int CommissionDetailID { get; set; }
        public int PeriodID { get; set; }
        public string PeriodName { get; set; }
        public string Status { get; set; }
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public System.DateTime StatusDate { get; set; }
        public int StatusID { get; set; }
        public System.DateTime PostDate { get; set; }
    }
    public class CustomerBasic
    {
        [Display(Name = "Customer ID")]
        public int CustID { get; set; }
        [Display(Name = "AppTrack ID")]
        public string SecID { get; set; }
        [Required, Display(Name = "Company Name")]
        [MaxLength(50)]
        public string Company { get; set; }
        [Required, Display(Name = "Display Name")]
        [MaxLength(100)]
        public string DisplayName { get; set; }
        [Required, Display(Name = "Type")]
        [MaxLength(50)]
        public int CustomerType { get; set; }
        [Display(Name = "Title")]
        [MaxLength(10)]
        public string NameTitle { get; set; }
        [Required, Display(Name = "First Name")]
        [MaxLength(50)]
        public string FirstName { get; set; }
        [Required, Display(Name = "Last Name")]
        [MaxLength(50)]
        public string LastName { get; set; }
        [MaxLength(100)]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        [Display(Name = "Address Line 1")]
        [MaxLength(50)]
        public string Address1 { get; set; }
        [Display(Name = "Address Line 2")]
        [MaxLength(50)]
        public string Address2 { get; set; }
        [MaxLength(50)]
        public string City { get; set; }
        [MaxLength(50)]
        public string State { get; set; }
        [Display(Name = "Zip Code")]
        [DataType(DataType.PostalCode)]
        public string PostalCode { get; set; }
        [Display(Name = "Company Phone")]
        [DataType(DataType.PhoneNumber)]
        public string CompanyPhone { get; set; }
        [Display(Name = "Contact Phone")]
        [DataType(DataType.PhoneNumber)]
        public string DayPhone { get; set; }
        [Display(Name = "Mobile")]
        [DataType(DataType.PhoneNumber)]
        public string Mobile { get; set; }
        [DataType(DataType.PhoneNumber)]
        public string Fax { get; set; }
        [Display(Name = "Start Date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public Nullable<System.DateTime> StartDate { get; set; }
        [Display(Name = "End Date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public Nullable<System.DateTime> EndDate { get; set; }
        [Display(Name = "Display Y/N")]
        public int HideFlag { get; set; }
        [MaxLength(30)]
        public string Status { get; set; }
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public Nullable<System.DateTime> StatusDate { get; set; }
        public int StatusID { get; set; }
        public Nullable<int> AdminID { get; set; }
    }

    public class ItemPromotion
    {
        [Display(Name = "ID")]
        public int ID { get; set; }
        [Display(Name = "Item ID")]
        public int ItemID { get; set; }
        [Display(Name = "Promotion ID")]
        public int PromotionID { get; set; }
        [Display(Name = "Promotion Title")]
        public string PromotionTitle { get; set; }
        public Nullable<int> StatusID { get; set; }
    }
    public class AutoshipBasic
    {
        [Display(Name = "ID")]
        public int AutoshipID { get; set; }
        [Display(Name = "Member ID")]
        public int CustID { get; set; }
        [Display(Name = "Start Date")]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime StartDate { get; set; }
        [Display(Name = "End Date")]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime EndDate { get; set; }
        [Display(Name = "Next Date")]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime NextDate { get; set; }
        [Display(Name = "Amount")]
        [DisplayFormat(DataFormatString = "{0:c}")]
        public decimal OrderTotal { get; set; }
        [Display(Name = "Status")]
        public string Status { get; set; }
        [Display(Name = "ProgramID")]
        public Nullable<int> TemplateID { get; set; }
        [Display(Name = "ProgramName")]
        public String VariantData1 { get; set; }
        public Nullable<int> StatusID { get; set; }
    }

    public class OrderBasic
    {
        [Display(Name = "Order ID")]
        public int? OrderID { get; set; }

        [Display(Name = "Member ID")]
        public int? MemberID { get; set; }

        [Display(Name = "Member Name")]
        public string Name { get; set; }

        [Display(Name = "Order Date")]
        public DateTime OrderDate { get; set; }

        [Display(Name = "Order Total")]
        public decimal OrderTotal { get; set; }

        [Display(Name = "Paid")]
        public decimal Paid { get; set; }

        [Display(Name = "Balance")]
        public decimal BalanceDue { get; set; }

        [Display(Name = "Status")]
        public string Status { get; set; }

    }

    public class PaymentMethod
    {
        public int? CustID { get; set; }
        [Required]
        [Display(Name = "Card Holder Name")]
        [MaxLength(100)]
        public string PName { get; set; }

        [Required]
        [Display(Name = "Card Type")]
        [MaxLength(10)]
        public string PCardType { get; set; }

        [Required]
        [Display(Name = "Card Number")]
        [MinLength(15)]
        [MaxLength(16)]
        [DataType(DataType.CreditCard)]
        public string PCardNumber { get; set; }

        [Display(Name = "Expiration Date")]
        [MaxLength(7)]
        public string PExpirationDate { get; set; }

        [Required]
        [RegularExpression(@"^(1[0-2]|0[1-9])$", ErrorMessage = "Please Enter a 2 digit month")]
        [Display(Name = "Expiration Month (MM)")]
        [MaxLength(2)]
        public string PExpirationMonth { get; set; }

        [Required]
        [RegularExpression(@"^\d{4}", ErrorMessage = "Please Enter a 4 digit year")]
        [Range(2015, 2050)]
        [Display(Name = "Expiration Year (YYYY)")]
        [MaxLength(4)]
        public string PExpirationYear { get; set; }

        public string CustomerProfile { get; set; }
        public string PaymentProfile { get; set; }
    }

    public partial class MemberReview
    {
        [Display(Name = "Review ID")]
        public int ID { get; set; }

        [Required]
        [Display(Name = "Member ID")]
        public int CustID { get; set; }

        [Required]
        [Display(Name = "Reason For Review")]
        public string ReviewReason { get; set; }

        [Display(Name = "Description Of Review")]
        public string Description { get; set; }

        [Display(Name = "Status of Review")]
        public string Status { get; set; }

        [Display(Name = "Outcome of Review")]
        public string Outcome { get; set; }

        [Display(Name = "Reason for Outcome")]
        public string OutcomeReasonCode { get; set; }

        [Display(Name = "Name of New Alliance ")]
        public string ChangeAlliance { get; set; }

        [Display(Name = "Description of Outcome")]
        public string OutcomeDescription { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public Nullable<System.DateTime> StartDate { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public Nullable<System.DateTime> EndDate { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public Nullable<System.DateTime> StatusDate { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public Nullable<System.DateTime> OutcomeDate { get; set; }

    }

    public class EnrollmentMeetingBasic
    {
        [Display(Name = "Meeting ID")]
        public int ID { get; set; }
        [Display(Name = "Meeting ID")]
        public int EventID { get; set; }
        [Required, Display(Name = "Event Title")]
        [MaxLength(50)]
        public string EventTitle { get; set; }
        [Required, Display(Name = "Event Description")]
        [MaxLength(100)]
        public string EventDescription { get; set; }
        [Required, Display(Name = "IMD-Host")]
        [MaxLength(100)]
        public string HostName { get; set; }
        [Display(Name = "Address Line 1")]
        [MaxLength(50)]
        public string Address1 { get; set; }
        [Display(Name = "Address Line 2")]
        [MaxLength(50)]
        public string Address2 { get; set; }
        [MaxLength(50)]
        public string City { get; set; }
        [MaxLength(50)]
        public string State { get; set; }
        [Display(Name = "Zip Code")]
        [DataType(DataType.PostalCode)]
        public string PostalCode { get; set; }
        [Display(Name = "Meeting Phone")]
        [DataType(DataType.PhoneNumber)]
        public string Phone { get; set; }
        [Display(Name = "Start Date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public Nullable<System.DateTime> EventStartDate { get; set; }
        [Display(Name = "End Date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public Nullable<System.DateTime> EventEndDate { get; set; }
        public string Status { get; set; }
        public int StatusID { get; set; }
        public Nullable<int> AdminID { get; set; }
    }

    public class MemberProgramBasic
    {
        [Display(Name = "Member ID")]
        public int CustID { get; set; }
        [Display(Name = "Program")]
        public int ID { get; set; }
        [Display(Name = "ProgramName")]
        public string ProgramName { get; set; }
        [Display(Name = "Item")]
        public int StoreID { get; set; }
        public int ItemID { get; set; }
        [Display(Name = "Item Name")]
        public string ItemName { get; set; }
        [Display(Name = "Autoship ID")]
        public int AutoshipID { get; set; }
        [Display(Name = "Next Date")]
        public DateTime NextDate { get; set; }
        [Display(Name = "Amount")]
        public decimal OrderTotal { get; set; }
        [Display(Name = "Status")]
        public string Status { get; set; }
    }

    public partial class ContactMin
    {
        [Required, Display(Name = "Name")]
        [MaxLength(100)]
        public string DisplayName { get; set; }

        [Required, Display(Name = "Email")]
        [MaxLength(100)]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Display(Name = "Subject")]
        [MaxLength(150)]
        public string Subject { get; set; }

        [Display(Name = "Message")]
        [DataType(DataType.MultilineText)]
        [StringLength(2500, ErrorMessage = "Message cannot exceed 2500 characters")]
        public string EditNoteText { get; set; }

    }
    public class ContactMe
    {
        [Required]
        [Display(Name = "Name")]
        [MaxLength(100)]
        public string contactDisplayName { get; set; }
        [Display(Name = "First Name")]
        [MaxLength(40)]
        public string contactFirstName { get; set; }
        [Display(Name = "Last Name")]
        [MaxLength(40)]
        public string contactLastName { get; set; }
        [Display(Name = "Email")]
        [MaxLength(100)]
        [DataType(DataType.EmailAddress)]
        public string contactEmail { get; set; }
        [Display(Name = "Street Address")]
        [MaxLength(40)]
        public string contactAddress1 { get; set; }
        [Display(Name = "Address Line 2")]
        [MaxLength(40)]
        public string contactAddress2 { get; set; }
        [Display(Name = "City")]
        [MaxLength(40)]
        public string contactCity { get; set; }
        [Display(Name = "State")]
        [MaxLength(2)]
        public string contactState { get; set; }
        [Display(Name = "Postal Code")]
        [MaxLength(10)]
        [DataType(DataType.PostalCode)]
        public string contactPostalCode { get; set; }
        [Display(Name = "Phone")]
        [DataType(DataType.PhoneNumber)]
        public string contactPhone { get; set; }
        [Display(Name = "Subject")]
        [MaxLength(200)]
        public string contactSubject { get; set; }
        [Display(Name = "Source")]
        [MaxLength(100)]
        public string contactSource { get; set; }
        [Display(Name = "Message")]
        [MaxLength(2000)]
        public string contactMessage { get; set; }
    }
    public class PromoContact
    {
        [Required]
        [Display(Name = "Name")]
        [MaxLength(100)]
        public string contactName { get; set; }
        [Display(Name = "Practice Name")]
        [MaxLength(100)]
        public string contactDisplayName { get; set; }
        [Display(Name = "First Name")]
        [MaxLength(40)]
        public string contactFirstName { get; set; }
        [Display(Name = "Last Name")]
        [MaxLength(40)]
        public string contactLastName { get; set; }
        [Display(Name = "Email")]
        [MaxLength(100)]
        [DataType(DataType.EmailAddress)]
        public string contactEmail { get; set; }
        [Display(Name = "Street Address")]
        [MaxLength(40)]
        public string contactAddress1 { get; set; }
        [Display(Name = "Address Line 2")]
        [MaxLength(40)]
        public string contactAddress2 { get; set; }
        [Display(Name = "City")]
        [MaxLength(40)]
        public string contactCity { get; set; }
        [Display(Name = "State")]
        [MaxLength(2)]
        public string contactState { get; set; }
        [Display(Name = "Postal Code")]
        [MaxLength(10)]
        [DataType(DataType.PostalCode)]
        public string contactPostalCode { get; set; }
        [Required]
        [Display(Name = "Phone")]
        [DataType(DataType.PhoneNumber)]
        public string contactPhone { get; set; }
        [Display(Name = "Subject")]
        [MaxLength(200)]
        public string contactSubject { get; set; }
        [Display(Name = "Source")]
        [MaxLength(100)]
        public string contactSource { get; set; }
        [Display(Name = "Message")]
        [MaxLength(2000)]
        public string contactMessage { get; set; }
    }
    public class PromoContactEmail
    {
        [Display(Name = "First Name")]
        [MaxLength(40)]
        public string contactFirstName { get; set; }
        [Display(Name = "Last Name")]
        [MaxLength(40)]
        public string contactLastName { get; set; }
        [Required]
        [Display(Name = "Name")]
        [MaxLength(100)]
        public string contactName { get; set; }
        [Required]
        [Display(Name = "Practice Name")]
        [MaxLength(100)]
        public string contactDisplayName { get; set; }
        [Required]
        [Display(Name = "Email")]
        [MaxLength(100)]
        [DataType(DataType.EmailAddress)]
        public string contactEmail { get; set; }
        [Display(Name = "Phone")]
        [DataType(DataType.PhoneNumber)]
        public string contactPhone { get; set; }

        public bool Option1 { get; set; }

        public bool Agreement { get; set; }
    }

    public class Contract
    {
        [Display(Name = "Contract ID")]
        public int ID { get; set; }
        [Required]
        [Display(Name = "Provider ID")]
        public int CustID { get; set; }
        [Display(Name = "Provider Name")]
        [MaxLength(50)]
        public string Company { get; set; }
        [Required]
        [Display(Name = "Contract Type")]
        [MaxLength(100)]
        public string ContractType { get; set; }
        [Required]
        [Display(Name = "Contract Title")]
        [MaxLength(200)]
        public string ContractTitle { get; set; }
        [Display(Name = "Contract Description")]
        [MaxLength(2500)]
        public string ContractDescription { get; set; }
        [Required]
        [Display(Name = "Effective Date")]
        [DataType(DataType.DateTime), DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public Nullable<System.DateTime> EffectiveDate { get; set; }
        [Required]
        [Display(Name = "Expiration Date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public Nullable<System.DateTime> ExpirationDate { get; set; }
        [Display(Name = "Signature Date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public Nullable<System.DateTime> SignatureDate { get; set; }
        [Required]
        [Display(Name = "Exclusivity Y/N")]
        public int ExclusivityFlag { get; set; }
        [Display(Name = "Exclusivity Description")]
        [MaxLength(2500)]
        public string ExclusivityDescription { get; set; }
        [Display(Name = "Special Terms")]
        [MaxLength(2500)]
        public string SpecialTerms { get; set; }
        [Required]
        [Display(Name = "Admin View Only Y/N")]
        public int AdminOnly { get; set; }
        public string Status { get; set; }
        public int StatusID { get; set; }
        public Nullable<int> AdminID { get; set; }
    }

    public class ContractDetail
    {
        [Display(Name = "ID")]
        public int ID { get; set; }

        [Display(Name = "Contract ID")]
        public int ContractID { get; set; }
        
        [Display(Name = "Detail Type")]
        [MaxLength(100)]
        public string DetailType { get; set; }
        
        [Display(Name = "Detail Description")]
        [MaxLength(5000)]
        public string DetailDescription { get; set; }

        [Display(Name = "Contract Amount")]
        [DisplayFormat(DataFormatString = "{0:c}")]
        public decimal ContractAmount { get; set; }

        [Display(Name = "Contract Percent")]
        [DataType(DataType.Currency)]
        [Range(0.0, 50.00,
            ErrorMessage = "Contract Percent must be a % between 0 and 50")]
        [DisplayFormat(DataFormatString = "{0:P2}")]
        public decimal ContractPercent { get; set; }

        [Display(Name = "Projected Amount")]
        [DisplayFormat(DataFormatString = "{0:c}")]
        public decimal ProjectedAmount { get; set; }

        public string Status { get; set; }

        public int StatusID { get; set; }
        
        public Nullable<int> AdminID { get; set; }
    }

    public class ContractDocument
    {
        public int DocumentID { get; set; }

        public int ContractID { get; set; }

        [Display(Name = "File Name")]
        [MaxLength(500)]
        public string FileName { get; set; }

        [Display(Name = "Path")]
        [MaxLength(500)]
        public string Path { get; set; }

        [Display(Name = "Document Type")]
        [MaxLength(100)]
        public string DocumentType { get; set; }

        [Display(Name = "Document Title")]
        [MaxLength(100)]
        public string DocumentTitle { get; set; }

        [Required, Display(Name = "Document Name")]
        [MaxLength(100)]
        public string DocumentName { get; set; }

        [Required, Display(Name = "Description")]
        [MaxLength(1500)]
        public string DocumentDescription { get; set; }

        [MaxLength(30)]
        public string Status { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public Nullable<System.DateTime> StatusDate { get; set; }

        public int StatusID { get; set; }

        public Nullable<int> AdminID { get; set; }
    }

    public class MembershipActivityForChart
    {
        public string SeriesName { get; set; }

        public string PeriodName { get; set; }

        public DateTime StartDate { get; set; }

        public int Count { get; set; }
    }

    public class MembershipActivityData
    {
        public string SeriesName { get; set; }
        public DateTime StartDate { get; set; }
        public string Period { get; set; }
        public int Membership { get; set; }

        public MembershipActivityData(string seriesname, string period, DateTime startdate, int membership)
        {
            SeriesName = seriesname;
            StartDate = startdate;
            Period = period;
            Membership = membership;
        }
    }






}
