//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SchoolSystemApi.Data
{
    using System;
    using System.Collections.Generic;
    
    public partial class Log
    {
        public int ID { get; set; }
        public string ApiKey { get; set; }
        public string Method { get; set; }
        public string RequestData { get; set; }
        public string ErrorData { get; set; }
        public string Activity { get; set; }
        public System.DateTime DateCreated { get; set; }
        public Nullable<System.DateTime> DateModified { get; set; }
    }
}