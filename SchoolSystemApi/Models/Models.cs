using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SchoolSystemApi.Models
{

    #region Accounts
    public class UserLogin
    {
        public string Username { get; set; }
        public string Password { get; set; }

    }

    public class UserLoginResponse
    {
        public string Status { get; set; }

        public string Username { get; set; }
        public string UserID { get; set; }

    }

    public class GenericModel
    {
        public string Status { get; set; }
        public string Data { get; set; }

    }

    public class ForgotPassword
    {
        public string Email { get; set; }
    }

    public class UpdatePassword
    {
        public string UserID { get; set; }
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }


    }

    #endregion

    #region Portal

    public class GetUser
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Status { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Data { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string UserID { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string School_ID { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string SchoolName { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Surname { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Username { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Email { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string UserType { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int Department_ID { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string DepartmentName { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool HOD { get; set; }
    }

    #endregion
}