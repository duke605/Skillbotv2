﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class Database : DbContext
    {
        public Database()
            : base("name=Database")
        {
            Database.Connection.ConnectionString = Database.Connection.ConnectionString.Replace("{password}", Secret.DbPassword);
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<item> items { get; set; }
        public virtual DbSet<recipe> recipes { get; set; }
        public virtual DbSet<input> inputs { get; set; }
        public virtual DbSet<output> outputs { get; set; }
        public virtual DbSet<user> users { get; set; }
        public virtual DbSet<channel> channels { get; set; }
        public virtual DbSet<server> servers { get; set; }
    }
}
