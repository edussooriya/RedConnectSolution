using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using RedConnectApp.Enums;
using RedConnectApp.Models;

using RedConnectApp.Enums;

namespace RedConnect.Models
{
    public class UserDetails
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public string NIC { get; set; }
        public string Phone { get; set; }
        public GenderEnum Gender { get; set; }
    }

}
