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
    
    public partial class experience
    {
        public experience()
        {
            this.likes = new HashSet<like>();
        }
    
        public int expid { get; set; }
        public int userid { get; set; }
        public string experimenttext { get; set; }
        public System.DateTime DateTime { get; set; }
    
        public virtual user user { get; set; }
        public virtual ICollection<like> likes { get; set; }
    }
}
