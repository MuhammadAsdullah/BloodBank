//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace BloodBan.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class donation
    {
        public int Id { get; set; }
        public Nullable<int> bloodrequestid { get; set; }
        public int userid { get; set; }
        public System.DateTime datetime { get; set; }
    
        public virtual bloodrequest bloodrequest { get; set; }
        public virtual user user { get; set; }
    }
}