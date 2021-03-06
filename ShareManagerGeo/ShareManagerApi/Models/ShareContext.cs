﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Data.Entity.Validation;
using System.Data.Linq.Mapping;
using System.Threading;
using System.Threading.Tasks;


namespace ShareManagerApi.Models
{
    public class ShareContext : DbContext
    {
        // You can add custom code to this file. Changes will not be overwritten.
        // 
        // If you want Entity Framework to drop and regenerate your database
        // automatically whenever you change your model schema, please use data migrations.
        // For more information refer to the documentation:
        // http://msdn.microsoft.com/en-us/data/jj591621.aspx

        public ShareContext() : base("ShareContextConnection") { }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
        }

        public System.Data.Entity.DbSet<ShareManagerApi.Models.CifsShare> CifsShares { get; set; }

        public System.Data.Entity.DbSet<ShareManagerApi.Models.Ou> Ous { get; set; }
         
        public System.Data.Entity.DbSet<ShareManagerApi.Models.CifsPermissionRequest> CifsPermissionRequests { get; set; }

        public System.Data.Entity.DbSet<ShareManagerApi.Models.AuditLog> AuditLogs { get; set; }

        public override int SaveChanges()
        {
            throw new InvalidOperationException("User ID must be provided");
        }

        public override Task<int> SaveChangesAsync()
        {
            throw new InvalidOperationException("User ID must be provided");
        }

        public int SaveChanges(string userId)
        {
            // Get all Added/Deleted/Modified entities (not Unmodified or Detached)
            //userId = "test";
            foreach (var ent in this.ChangeTracker.Entries().Where(p => p.State == System.Data.Entity.EntityState.Added || p.State == System.Data.Entity.EntityState.Deleted || p.State == System.Data.Entity.EntityState.Modified))
            {
                // For each changed record, get the audit record entries and add them
                foreach (AuditLog x in GetAuditRecordsForChange(ent, userId))
                {
                    this.AuditLogs.Add(x);
                }
            }

            // Call the original SaveChanges(), which will save both the changes made and the audit records
            try
            {
                return base.SaveChanges();
            }
            catch (DbEntityValidationException dbEx)
            {
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        Trace.TraceInformation("Property: {0} Error: {1}",
                                                validationError.PropertyName,
                                                validationError.ErrorMessage);
                    }
                }
                throw;
            }
        }

        public Task<int> SaveChangesAsync(string userId)
        {
            // Get all Added/Deleted/Modified entities (not Unmodified or Detached)
            //userId = "test";
            foreach (var ent in this.ChangeTracker.Entries().Where(p => p.State == System.Data.Entity.EntityState.Added || p.State == System.Data.Entity.EntityState.Deleted || p.State == System.Data.Entity.EntityState.Modified))
            {
                // For each changed record, get the audit record entries and add them
                foreach (AuditLog x in GetAuditRecordsForChange(ent, userId))
                {
                    this.AuditLogs.Add(x);
                }
            }

            // Call the original SaveChanges(), which will save both the changes made and the audit records
            try
            {
                return base.SaveChangesAsync();
            }
            catch (DbEntityValidationException dbEx)
            {
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        Trace.TraceInformation("Property: {0} Error: {1}",
                                                validationError.PropertyName,
                                                validationError.ErrorMessage);
                    }
                }
                throw;
            }
        }

        private List<AuditLog> GetAuditRecordsForChange(DbEntityEntry dbEntry, string userId)
        {
            List<AuditLog> result = new List<AuditLog>();

            DateTime changeTime = DateTime.UtcNow;

            // Get the Table() attribute, if one exists
            TableAttribute tableAttr = dbEntry.Entity.GetType().GetCustomAttributes(typeof(TableAttribute), false).SingleOrDefault() as TableAttribute;

            // Get table name (if it has a Table attribute, use that, otherwise get the pluralized name)
            string tableName = tableAttr != null ? tableAttr.Name : dbEntry.Entity.GetType().Name;

            // Get primary key value (If you have more than one key column, this will need to be adjusted)
            string keyName = dbEntry.Entity.GetType().GetProperties().Single(p => p.GetCustomAttributes(typeof(KeyAttribute), false).Count() > 0).Name;

            if (dbEntry.State == System.Data.Entity.EntityState.Added)
            {
                // For Inserts, just add the whole record
                // If the entity implements IDescribableEntity, use the description from Describe(), otherwise use ToString()
                result.Add(new AuditLog()
                {
                    AuditLogID = Guid.NewGuid(),
                    UserID = userId,
                    EventDateUTC = changeTime,
                    EventType = "A", // Added
                    TableName = tableName,
                    RecordID = dbEntry.CurrentValues.GetValue<object>(keyName).ToString(),  // Again, adjust this if you have a multi-column key
                    ColumnName = "*ALL",    // Or make it nullable, whatever you want
                    NewValue = (dbEntry.CurrentValues.ToObject() is IDescribableEntity) ? (dbEntry.CurrentValues.ToObject() as IDescribableEntity).Describe() : dbEntry.CurrentValues.ToObject().ToString()
                }
                    );
            }
            else if (dbEntry.State == System.Data.Entity.EntityState.Deleted)
            {
                // Same with deletes, do the whole record, and use either the description from Describe() or ToString()
                result.Add(new AuditLog()
                {
                    AuditLogID = Guid.NewGuid(),
                    UserID = userId,
                    EventDateUTC = changeTime,
                    EventType = "D", // Deleted
                    TableName = tableName,
                    RecordID = dbEntry.OriginalValues.GetValue<object>(keyName).ToString(),
                    ColumnName = "*ALL",
                    OriginalValue = (dbEntry.OriginalValues.ToObject() is IDescribableEntity) ? (dbEntry.OriginalValues.ToObject() as IDescribableEntity).Describe() : dbEntry.OriginalValues.ToObject().ToString()
                }
                    );
            }
            else if (dbEntry.State == System.Data.Entity.EntityState.Modified)
            {
                foreach (string propertyName in dbEntry.OriginalValues.PropertyNames)
                {
                    // For updates, we only want to capture the columns that actually changed
                    //if (!object.Equals(dbEntry.OriginalValues.GetValue<object>(propertyName), dbEntry.CurrentValues.GetValue<object>(propertyName)))
                    if (!object.Equals(dbEntry.GetDatabaseValues().GetValue<object>(propertyName), dbEntry.CurrentValues.GetValue<object>(propertyName)))
                    {
                        result.Add(new AuditLog()
                        {
                            AuditLogID = Guid.NewGuid(),
                            UserID = userId,
                            EventDateUTC = changeTime,
                            EventType = "M",    // Modified
                            TableName = tableName,
                            RecordID = dbEntry.OriginalValues.GetValue<object>(keyName).ToString(),
                            ColumnName = propertyName,
                            OriginalValue = dbEntry.GetDatabaseValues().GetValue<object>(propertyName) == null ? null : dbEntry.GetDatabaseValues().GetValue<object>(propertyName).ToString(),
                            NewValue = dbEntry.CurrentValues.GetValue<object>(propertyName) == null ? null : dbEntry.CurrentValues.GetValue<object>(propertyName).ToString()
                        }
                            );
                    }
                }
            }
            // Otherwise, don't do anything, we don't care about Unchanged or Detached entities

            return result;
        }
    
    
    
    
    }
}
