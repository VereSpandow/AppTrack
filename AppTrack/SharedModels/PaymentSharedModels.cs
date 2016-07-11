using AppTrack.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AppTrack.SharedModels
{

    public class PaymentAttempt
    {
        [Display(Name = "OrderID")]
        public Nullable<int> OrderID { get; set; }
        [Display(Name = "CustID")]
        public Nullable<int> CustID { get; set; }
        [Display(Name = "Payment Method")]
        public int PMID { get; set; }

        public string PaymentType { get; set; }

        [Display(Name = "Auth net Customer Profile")]
        [MaxLength(100)]
        public string CustomerProfileID { get; set; }
        [Display(Name = "Auth net Payment Profile")]
        [MaxLength(100)]
        public string PaymentProfileID { get; set; }
        [Display(Name = "Shipping Address Profile ID")]
        [MaxLength(100)]
        public string ShippingProfileID { get; set; }


        [Display(Name = "Name on Payment Method")]
        [MaxLength(100)]
        public string PName { get; set; }
        [Display(Name = "Email Payment Method")]
        [MaxLength(100)]
        public string Email { get; set; }
        [Display(Name = "Phone")]
        [MaxLength(20)]
        public string PPhone { get; set; }
        [Display(Name = "Address Line 1")]
        [MaxLength(40)]
        public string PAddress1 { get; set; }
        [Display(Name = "Address Line 2")]
        [MaxLength(40)]
        public string PAddress2 { get; set; }
        [Display(Name = "City")]
        [MaxLength(40)]
        public string PCity{ get; set; }
        [Display(Name = "State")]
        [MaxLength(40)]
        public string PState{ get; set; }
        [Display(Name = "PostalCode")]
        [MaxLength(10)]
        public string PPostalCode{ get; set; }
        [Display(Name = "Balance Due")]
        [DataType(DataType.Currency)]
        [Range(0.0, 2500.00,
        ErrorMessage = "Balance Due must be between 0 and 2500")]
        public decimal BalanceDue{ get; set; }
    
        [Display(Name = "Response String")]
        [MaxLength(4500)]
        public string ResponseString { get; set; }
        [Display(Name = "Response Code")]
        public Nullable<int> ResponseCode { get; set; }
        [Display(Name = "TransactionID")]
        [MaxLength(200)]
        public string TransactionID { get; set; }

        
        [Display(Name = "Transaction Amount")]
        [DataType(DataType.Currency)]
        [Range(0.0, 2500.00,
        ErrorMessage = "Transaction must be between 0 and 2500")]
        public decimal Amount{ get; set; }

        [Display(Name = "Order Date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public Nullable<System.DateTime> OrderDate { get; set; }
        [Display(Name = "Paid Date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public Nullable<System.DateTime> PaidDate { get; set; }

    }


    public class FailedPaymentAttempt
    {
        [Display(Name = "OrderID")]
        public Nullable<int> OrderID { get; set; }
        
        [Display(Name = "CustID")]
        public Nullable<int> CustID { get; set; }

        [Display(Name = "Payment Method")]
        public int PMID { get; set; }

        [Display(Name = "Payment ID")]
        public int PaymentID { get; set; }

        [Display(Name = "Status")]
        public string Status { get; set; }

        [Display(Name = "Name on Payment")]
        [MaxLength(100)]
        public string Name { get; set; }

        [Display(Name = "Name on Payment")]
        [MaxLength(100)]
        public string PaymentType { get; set; }

        [Display(Name = "Email on Payment Method")]
        [MaxLength(100)]
        public string Email { get; set; }
    
        [Display(Name = "Response String")]
        [MaxLength(4500)]
        public string ResponseString { get; set; }

        [Display(Name = "TransactionID")]
        [MaxLength(200)]
        public string ExtTransactionID { get; set; }

        [Display(Name = "Card Number")]
        [MaxLength(20)]
        public string CardNumber { get; set; }

        [Display(Name = "Amount")]
        [DataType(DataType.Currency)]
        [Range(0.0, 2500.00,
        ErrorMessage = "Transaction must be between 0 and 2500")]
        public decimal Amount{ get; set; }

        [Display(Name = "Payment Date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public Nullable<System.DateTime> PaymentDate { get; set; }

    }

}