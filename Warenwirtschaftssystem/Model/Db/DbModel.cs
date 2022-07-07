namespace Warenwirtschaftssystem.Model.Db
{
    using Microsoft.Data.SqlClient;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Windows;

    public class DbModel : DbContext
    {
        public DbSet<Setting> Settings { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Article> Articles { get; set; }
        public DbSet<Color> Colors { get; set; }
        public DbSet<Gender> Genders { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<SubCategory> SubCategories { get; set; }
        public DbSet<Size> Sizes { get; set; }
        public DbSet<Material> Materials { get; set; }
        public DbSet<Parts> Parts { get; set; }
        public DbSet<Brand> Brands { get; set; }
        public DbSet<Defect> Defects { get; set; }
        public DbSet<SupplierProportion> SupplierProportions { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<SavedArticleAttributes> SavedArticleAttributes { get; set; }

        private readonly string ConnectionString;


        public DbModel()
        {
            ConnectionString = string.Empty;
        }

        public DbModel(string connectionString)
        {
            ConnectionString = connectionString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (string.IsNullOrEmpty(ConnectionString))
            {
                var builder = new SqlConnectionStringBuilder();
                builder.DataSource = ".\\SQLEXPRESS";
                builder.IntegratedSecurity = true;
                builder.InitialCatalog = "WWS-Dev";

                optionsBuilder.UseSqlServer(builder.ConnectionString);
            }
            else
            {
                optionsBuilder
                    .UseLazyLoadingProxies()
                    .UseSqlServer(ConnectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //Präzision von Supplier.SupplierPoportion festlegen
            modelBuilder.Entity<Supplier>()
                        .Property(p => p.SupplierProportion)
                        .HasPrecision(5, 2);
            //Präzision von GraduationSupplierProportion.SupplierPoportion festlegen
            modelBuilder.Entity<SupplierProportion>()
                .Property(p => p.Proportion)
                .HasPrecision(5, 2);

            // Relationships
            modelBuilder.Entity<Article>().HasOne<Supplier>(a => a.Supplier).WithMany(s => s.Articles);
            modelBuilder.Entity<Article>().HasOne<Supplier>(a => a.ReservingSupplier).WithMany(s => s.Reservations);
        }

        /// <summary>
        /// Loop: Presents the user a MessageBox if an exception is encountered and retry saving after its closing
        /// </summary>
        public void SaveChangesRetryOnUserInput()
        {
            bool operationSuccessful = false;
            do
            {
                try
                {
                    SaveChanges();
                    operationSuccessful = true;
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString(), "Fehler beim Speichern in Datenbank - Neuversuch nach Schließung dieses Fensters", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            } while (!operationSuccessful);
        }

    }

    public class Setting
    {
        [Key]
        public string Key { get; set; }
        public string Value { get; set; }
    }

    //Dient dazu UI bei Veränderung eines Attributes zu benachrichtigen, sodass sie updated 
    public class ObservableObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}