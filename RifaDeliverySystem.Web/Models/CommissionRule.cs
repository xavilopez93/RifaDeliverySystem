using System.ComponentModel.DataAnnotations.Schema;
namespace RifaDeliverySystem.Web.Models;
public class CommissionRule { 
    public int Id{get;set;} 
    public string VendorType{get;set;}=null!; 
    public string VendorClass{get;set;}=null!; 
    public int MinCoupons{get;set;} 
    public int? MaxCoupons{get;set;} 
    public decimal Percentage{get;set;} }