﻿namespace dsf_service_template_net6.Data.Models
{

    public class CitizenDataResponse
    {
        public int errorCode { get; set; }
        public object errorMessage { get; set; }
        public CitizenDataDetails data { get; set; }
        public bool succeeded { get; set; }
    }

    public class CitizenDataDetails
    {
        public string pin { get; set; }
        public string email { get; set; }
        public string mobile { get; set; }
        public Addressinfo[] addressInfo { get; set; }
    }

    public class Addressinfo
    {
        public string type { get; set; }
        public int postalCode { get; set; }
        public string language { get; set; }
        public Item item { get; set; }
        public Town town { get; set; }
        public Parish parish { get; set; }
        public District district { get; set; }
        public Country country { get; set; }
        public string addressText { get; set; }
    }

    public class Item
    {
        public Street street { get; set; }
        public int code { get; set; }
        public string name { get; set; }
    }

    public class Street
    {
        public string apartmentNumber { get; set; }
        public string streetNumber { get; set; }
    }

    public class Town
    {
        public int code { get; set; }
        public string name { get; set; }
    }

    public class Parish
    {
        public int code { get; set; }
        public string name { get; set; }
    }

    public class District
    {
        public int code { get; set; }
        public string name { get; set; }
    }

    public class Country
    {
        public int code { get; set; }
        public string name { get; set; }
    }
  
}