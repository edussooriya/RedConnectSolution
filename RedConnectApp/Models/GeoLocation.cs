using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using RedConnectApp.Enums;
using RedConnectApp.Models;

namespace RedConnect.Models
{
    public class GeoLocation
    {
        public string Type { get; set; } = "Point";
        public double[] Coordinates { get; set; }
    }

}
