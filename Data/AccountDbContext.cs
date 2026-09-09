using NDLP_Project.Models.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace NDLP_Project.Data
{
    public class AccountDbContext:DbContext
    {
        public AccountDbContext() : base("name=NDLPConnection")
        {
            // Optional: prevents EF from automatically trying to drop/create tables if the DB already exists
            Database.SetInitializer<AccountDbContext>(null);
        }
        // The 3 Database Tables
        public DbSet<CustomerAccount> CustomerAccounts { get; set; }
        public DbSet<AccountTransaction> AccountTransactions { get; set; }
        public DbSet<AccountQueryRequest> AccountQueryRequests { get; set; }
        public DbSet<AccountAuditLog> AccountAuditLogs { get; set; }
    }
}