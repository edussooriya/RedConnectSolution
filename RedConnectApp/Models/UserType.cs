using System;
using System.ComponentModel.DataAnnotations;

namespace RedConnect.Models;
public class UserType
{
    [Key]
    public int UserTypeId { get; set; }
    public string UserTypeName { get; set; }
}
 
