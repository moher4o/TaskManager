using Microsoft.EntityFrameworkCore;
using TaskManager.Data.Models;

namespace TaskManager.Data
{
    public class TasksDbContext : DbContext
    {
        public DbSet<Directorate> Directorates { get; set; }

        public DbSet<Department> Departments { get; set; }

        public DbSet<Sector> Sectors { get; set; }

        public DbSet<TasksType> TasksTypes { get; set; }

        public DbSet<TasksStatus> TasksStatuses { get; set; }

        public DbSet<Priority> Priorities { get; set; }

        public DbSet<JobTitle> JobTitles { get; set; }

        public DbSet<Employee> Employees { get; set; }

        public DbSet<Task> Tasks { get; set; }

        public DbSet<EmployeesTasks> EmployeesTasks { get; set; }

        public DbSet<WorkedHours> WorkedHours { get; set; }

        public DbSet<TaskNote> Notes { get; set; }

        public DbSet<Role> Roles { get; set; }

        public TasksDbContext(DbContextOptions<TasksDbContext> options)
             : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Directorate>()
                .HasMany(tt => tt.Departments)
                .WithOne(t => t.Directorate)
                .HasForeignKey(t => t.DirectorateId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Directorate>()
                .HasMany(tt => tt.Sectors)
                .WithOne(t => t.Directorate)
                .HasForeignKey(t => t.DirectorateId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Directorate>()
                .HasMany(tt => tt.Employees)
                .WithOne(t => t.Directorate)
                .HasForeignKey(t => t.DirectorateId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Directorate>()
                .HasMany(tt => tt.Tasks)
                .WithOne(t => t.Directorate)
                .HasForeignKey(t => t.DirectorateId)
                .OnDelete(DeleteBehavior.Restrict);


            builder.Entity<Department>()
                .HasMany(tt => tt.Sectors)
                .WithOne(t => t.Department)
                .HasForeignKey(t => t.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Department>()
                .HasMany(tt => tt.Employees)
                .WithOne(t => t.Department)
                .HasForeignKey(t => t.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Department>()
                .HasMany(tt => tt.Tasks)
                .WithOne(t => t.Department)
                .HasForeignKey(t => t.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Sector>()
                .HasMany(tt => tt.Employees)
                .WithOne(t => t.Sector)
                .HasForeignKey(t => t.SectorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Sector>()
                .HasMany(tt => tt.Tasks)
                .WithOne(t => t.Sector)
                .HasForeignKey(t => t.SectorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<TasksType>()
                .HasMany(tt => tt.Tasks)
                .WithOne(t => t.TaskType)
                .HasForeignKey(t => t.TypeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<TasksStatus>()
                .HasMany(tt => tt.Tasks)
                .WithOne(t => t.TaskStatus)
                .HasForeignKey(t => t.StatusId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Priority>()
                .HasMany(tt => tt.Tasks)
                .WithOne(t => t.TaskPriority)
                .HasForeignKey(t => t.PriorityId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<JobTitle>()
                .HasMany(tt => tt.Employees)
                .WithOne(t => t.JobTitle)
                .HasForeignKey(t => t.JobTitleId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<EmployeesTasks>()
                .HasKey(pc => new { pc.EmployeeId, pc.TaskId });

            builder.Entity<Task>()
                .HasMany(tt => tt.AssignedExperts)
                .WithOne(t => t.Task)
                .HasForeignKey(t => t.TaskId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Employee>()
                .HasMany(tt => tt.Tasks)
                .WithOne(t => t.Employee)
                .HasForeignKey(t => t.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Employee>()
                .HasMany(tt => tt.TasksCreator)
                .WithOne(t => t.Owner)
                .HasForeignKey(t => t.OwnerId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Employee>()
                .HasMany(tt => tt.TasksCloser)
                .WithOne(t => t.CloseUser)
                .HasForeignKey(t => t.CloseUserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Employee>()
                .HasMany(tt => tt.TasksDeleted)
                .WithOne(t => t.DeletedByUser)
                .HasForeignKey(t => t.DeletedByUserId)
                .OnDelete(DeleteBehavior.NoAction);


            builder.Entity<Employee>()
                .HasMany(tt => tt.TasksAssigner)
                .WithOne(t => t.Assigner)
                .HasForeignKey(t => t.AssignerId)
                .OnDelete(DeleteBehavior.NoAction);


            builder.Entity<WorkedHours>()
                .HasKey(pc => new { pc.TaskId, pc.EmployeeId, pc.WorkDate });

            builder.Entity<Employee>()
                .HasMany(tt => tt.WorkedHoursByTask)
                .WithOne(t => t.Employee)
                .HasForeignKey(t => t.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Task>()
                .HasMany(tt => tt.WorkedHours)
                .WithOne(t => t.Task)
                .HasForeignKey(t => t.TaskId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<TaskNote>()
                .HasKey(pc => new { pc.Id});

            builder.Entity<Employee>()
                .HasMany(tt => tt.Notes)
                .WithOne(t => t.Employee)
                .HasForeignKey(t => t.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Task>()
                .HasMany(tt => tt.Notes)
                .WithOne(t => t.Task)
                .HasForeignKey(t => t.TaskId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Role>()
                .HasMany(tt => tt.Employees)
                .WithOne(t => t.Role)
                .HasForeignKey(t => t.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Employee>()
               .HasIndex(p => p.DaeuAccaunt)
               .IsUnique();

            builder.Entity<Directorate>()
               .HasIndex(p => p.DirectorateName)
               .IsUnique();


            base.OnModelCreating(builder);
        }
    }
}
