namespace RifaDeliverySystem.Web.Models;
public class Annulment { public int Id{get;set;} 
    public int RenditionId{get;set;} 
    public Rendition Rendition{get;set;}=null!; 
    public string Reason{get;set;}=null!; public int Count{get;set;} }