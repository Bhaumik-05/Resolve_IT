using Microsoft.EntityFrameworkCore;
using rsit.Models;
using System.Net.Mail;
using System.Net.Sockets;

namespace rsit.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Department> Departments { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Ticket> Tickets { get; set; }
    public DbSet<System.Net.Mail.Attachment> Attachments { get; set; }
    public DbSet<Assignment> Assignments { get; set; }
    public DbSet<TicketHistory> TicketHistories { get; set; }
    public DbSet<Feedback> Feedbacks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // USER
        modelBuilder.Entity<User>()
            .HasIndex(u => u.EmployeeId)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        // DEPARTMENT -> USER
        modelBuilder.Entity<User>()
            .HasOne(u => u.Department)
            .WithMany(d => d.Users)
            .HasForeignKey(u => u.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        // DEPARTMENT -> TICKET
        modelBuilder.Entity<Ticket>()
            .HasOne(t => t.Department)
            .WithMany(d => d.Tickets)
            .HasForeignKey(t => t.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        // CATEGORY -> TICKET
        modelBuilder.Entity<Ticket>()
            .HasOne(t => t.Category)
            .WithMany(c => c.Tickets)
            .HasForeignKey(t => t.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // USER -> TICKET
        modelBuilder.Entity<Ticket>()
            .HasOne(t => t.Employee)
            .WithMany(u => u.Tickets)
            .HasForeignKey(t => t.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        // TICKET -> ATTACHMENT
        modelBuilder.Entity<System.Net.Mail.Attachment>()
            .HasOne(a => a.Ticket)
            .WithMany((object t) => t.Attachments)
            .HasForeignKey((object a) => a.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

        // USER -> ATTACHMENT
        modelBuilder.Entity<System.Net.Mail.Attachment>()
            .HasOne(a => a.Uploader)
            .WithMany((object u) => u.Attachments)
            .HasForeignKey((object a) => a.UploadedBy)
            .OnDelete(DeleteBehavior.Restrict);

        // TICKET -> ASSIGNMENT
        modelBuilder.Entity<Assignment>()
            .HasOne(a => a.Ticket)
            .WithMany(t => t.Assignments)
            .HasForeignKey(a => a.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

        // USER -> ASSIGNMENT (staff)
        modelBuilder.Entity<Assignment>()
            .HasOne(a => a.Staff)
            .WithMany(u => u.Assignments)
            .HasForeignKey(a => a.StaffId)
            .OnDelete(DeleteBehavior.Restrict);

        // USER -> ASSIGNMENT (assigned by)
        modelBuilder.Entity<Assignment>()
            .HasOne(a => a.AssignedByUser)
            .WithMany(u => u.AssignmentsCreated)
            .HasForeignKey(a => a.AssignedBy)
            .OnDelete(DeleteBehavior.Restrict);

        // TICKET -> HISTORY
        modelBuilder.Entity<TicketHistory>()
            .HasOne(h => h.Ticket)
            .WithMany(t => t.History)
            .HasForeignKey(h => h.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

        // USER -> HISTORY
        modelBuilder.Entity<TicketHistory>()
            .HasOne(h => h.ChangedByUser)
            .WithMany(u => u.TicketHistories)
            .HasForeignKey(h => h.ChangedBy)
            .OnDelete(DeleteBehavior.Restrict);

        // TICKET -> FEEDBACK (one-to-zero/one)
        modelBuilder.Entity<Feedback>()
            .HasOne(f => f.Ticket)
            .WithOne(t => t.Feedback)
            .HasForeignKey<Feedback>(f => f.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

        // USER -> FEEDBACK
        modelBuilder.Entity<Feedback>()
            .HasOne(f => f.Employee)
            .WithMany(u => u.Feedbacks)
            .HasForeignKey(f => f.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}