//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SkillBotv2
{
    using System;
    using System.Collections.Generic;
    
    public partial class input
    {
        public decimal ItemId { get; set; }
        public decimal RecipeId { get; set; }
        public int Quantity { get; set; }
    
        public virtual item item { get; set; }
        public virtual recipe recipe { get; set; }
    }
}
