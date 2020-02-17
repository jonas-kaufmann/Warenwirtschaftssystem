namespace Warenwirtschaftssystem.Model.Db
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity;
    using System.Globalization;

    public class DbModel : DbContext
    {
        public virtual DbSet<Setting> Settings { get; set; }
        public virtual DbSet<Supplier> Suppliers { get; set; }
        public virtual DbSet<Article> Articles { get; set; }
        public virtual DbSet<Color> ArticleColors { get; set; }
        public virtual DbSet<Gender> ArticleGender { get; set; }
        public virtual DbSet<Category> ArticleCategories { get; set; }
        public virtual DbSet<Type> ArticleTypes { get; set; }
        public virtual DbSet<Size> ArticleSizes { get; set; }
        public virtual DbSet<Material> ArticleMaterials { get; set; }
        public virtual DbSet<Part> ArticleParts { get; set; }
        public virtual DbSet<Brand> ArticleBrands { get; set; }
        public virtual DbSet<Defect> ArticleDefects { get; set; }
        public virtual DbSet<GraduationSupplierProportion> GraduationSupplierProportion { get; set; }
        public virtual DbSet<Document> Documents { get; set; }
        public virtual DbSet<SavedArticleAttributes> SalesSavedArticleAttributes { get; set; }
        public virtual DbSet<ArticleReservation> ArticleReservations { get; set; }

        public DbModel() { }

        public DbModel(string connectionString)
            : base(connectionString) { }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //Präzision von Supplier.SupplierPoportion festlegen
            modelBuilder.Entity<Supplier>()
                        .Property(p => p.SupplierProportion)
                        .HasPrecision(5, 2);
            //Präzision von GraduationSupplierProportion.SupplierPoportion festlegen
            modelBuilder.Entity<GraduationSupplierProportion>()
                .Property(p => p.SupplierProportion)
                .HasPrecision(5, 2);
        }
    }

    [Table("Settings")]
    public class Setting
    {
        [Key]
        public string Key { get; set; }
        public string Value { get; set; }
    }

    [Table("Suppliers")]
    public class Supplier
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Place { get; set; }
        public string EMail { get; set; }
        public string Phone { get; set; }
        public string Street { get; set; }
        public string Postcode { get; set; }
        public decimal? SupplierProportion { get; set; }
        public int PickUp { get; set; }
        public DateTime CreationDate { get; set; }
        public string Notes { get; set; }
        public Title Title { get; set; }
        public string Company { get; set; }

        public virtual ObservableCollection<Article> Articles { get; set; }
        public virtual ICollection<ArticleReservation> Reservations { get; set; }
    }

    [Table("Reservations")]
    public class ArticleReservation
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ArticleId { get; set; }

        public virtual Supplier Supplier { get; set; }
        public DateTime? From { get; set; }
        public DateTime? Until { get; set; }
    }

    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum Title
    {
        [Description("Herr")]
        Mr,
        [Description("Frau")]
        Mrs
    }

    static class TitleExtensions
    {
        public static string GetDescription<T>(this T e) where T : IConvertible
        {
            string description = null;

            if (e is Enum)
            {
                System.Type type = e.GetType();
                Array values = Enum.GetValues(type);

                foreach (int val in values)
                {
                    if (val == e.ToInt32(CultureInfo.InvariantCulture))
                    {
                        var memInfo = type.GetMember(type.GetEnumName(val));
                        var descriptionAttributes = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
                        if (descriptionAttributes.Length > 0)
                        {
                            description = ((DescriptionAttribute)descriptionAttributes[0]).Description;
                        }

                        break;
                    }
                }
            }

            return description;
        }
    }

    [Table("Articles")]
    public class Article : ObservableObject
    {
        private int _id;
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id
        {
            get => _id;
            set
            {
                _id = value;
                OnPropertyChanged("Id");
            }
        }

        [NotMapped]
        public int ConvertedId
        {
            get
            {
                if (_id != 0) return _id + 100000;
                else return 0;
            }
        }


        private DateTime? _pickUp;
        public DateTime? PickUp
        {
            get
            {
                return _pickUp;
            }
            set
            {
                _pickUp = value;
                OnPropertyChanged("PickUp");
            }
        }

        private string _description;
        [NotMapped]
        public string Description
        {
            get
            {
                if (string.IsNullOrEmpty(_description))
                {
                    RegenerateDescription();
                }

                return _description;
            }
        }

        public void RegenerateDescription()
        {
            _description = "";
            bool firstElement = true;

            //Gender
            if (Gender != null && !string.IsNullOrEmpty(Gender.Short))
            {
                _description += Gender.Short;
                firstElement = false;
            }

            //Category
            if (Category != null && !string.IsNullOrEmpty(Category.Title))
            {
                if (!firstElement)
                    _description += "-";
                _description += Category.Title;
                firstElement = false;
            }

            //Type
            if (Type != null && !string.IsNullOrEmpty(Type.Title))
            {
                if (!firstElement)
                    _description += "-";
                _description += Type.Title;
                firstElement = false;
            }

            //Brand
            if (Brand != null && !string.IsNullOrEmpty(Brand.Title))
            {
                if (!firstElement)
                    _description += "-";
                _description += Brand.Title;
                firstElement = false;
            }

            //Size
            if (Size != null && !string.IsNullOrEmpty(Size.Value))
            {
                if (!firstElement)
                    _description += "-";
                _description += Size.Value;
                firstElement = false;
            }

            //Colors
            if (Colors != null && Colors.Count != 0)
            {
                if (!firstElement)
                    _description += "-";
                bool firstElementInList = true;
                foreach (Color color in Colors)
                {
                    if (!firstElementInList)
                        _description += ",";
                    _description += color.Description;
                    firstElementInList = false;
                }
                firstElement = false;
            }

            //Materials
            if (Materials != null && Materials.Count != 0)
            {
                if (!firstElement)
                    _description += "-";
                bool firstElementInList = true;
                foreach (Material material in Materials)
                {
                    if (!firstElementInList)
                        _description += ",";
                    _description += material.Title;
                    firstElementInList = false;
                }
            }

            //Parts
            if (Parts != null && !string.IsNullOrEmpty(Parts.Title))
            {
                if (!firstElement)
                    _description += "-";
                _description += Parts.Title;
            }
        }

        [NotMapped] public DateTime? Sold { get; set; }

        private decimal _price;
        public decimal Price
        {
            get
            {
                return _price;
            }
            set
            {
                _price = value;
                OnPropertyChanged("Price");
                OnPropertyChanged("Percentage");
            }
        }

        private decimal _supplierProportion;
        public decimal SupplierProportion
        {
            get => _supplierProportion;
            set
            {
                _supplierProportion = Math.Round(value, 2);
                OnPropertyChanged("SupplierProportion");
                OnPropertyChanged("Percentage");
            }
        }

        [NotMapped]
        public decimal? Percentage
        {
            get
            {
                if (_price == 0)
                    return null;
                else return Math.Round(_supplierProportion / _price, 1);
            }
        }

        private Status _status;
        public Status Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged("Status");
            }
        }

        public DateTime AddedToSortiment { get; set; }
        public DateTime? EnteredFinalState { get; set; }

        public virtual ObservableCollection<Color> Colors { get; set; }
        public virtual Gender Gender { get; set; }
        public virtual Category Category { get; set; }
        public virtual Type Type { get; set; }
        public virtual Size Size { get; set; }
        public virtual ObservableCollection<Material> Materials { get; set; }
        public virtual Part Parts { get; set; }
        public virtual Brand Brand { get; set; }
        public virtual ObservableCollection<Defect> Defects { get; set; }
        public bool AsNew { get; set; }

        private string _notes;
        public string Notes
        {
            get => _notes;
            set
            {
                _notes = value;
                OnPropertyChanged("Notes");
            }
        }

        public virtual Supplier Supplier { get; set; }
        public virtual ArticleReservation Reservation { get; set; }

        public virtual ObservableCollection<Document> Documents { get; set; }

        public void SetDescriptionExplicitly(string description) => _description = description;

        public void notifyAllPropertiesChanged()
        {
            OnPropertyChanged("PickUp");
            OnPropertyChanged("Description");
            OnPropertyChanged("Price");
            OnPropertyChanged("SupplierProportion");
            OnPropertyChanged("Percentage");
            OnPropertyChanged("Colors");
            OnPropertyChanged("Gender");
            OnPropertyChanged("Category");
            OnPropertyChanged("Type");
            OnPropertyChanged("Size");
            OnPropertyChanged("Materials");
            OnPropertyChanged("Parts");
            OnPropertyChanged("Brand");
            OnPropertyChanged("Defects");
            OnPropertyChanged("AsNew");
            OnPropertyChanged("Notes");
        }
    }

    [Table("Colors")]
    public class Color
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Description { get; set; }
        public string ColorCode { get; set; }

        public virtual ObservableCollection<Article> Articles { get; set; }
    }

    [Table("Gender")]
    public class Gender
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Description { get; set; }
        public string Short { get; set; }
    }

    [Table("Categories")]
    public class Category
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Title { get; set; }

        public virtual ObservableCollection<Type> Types { get; set; }
    }

    [Table("Types")]
    public class Type
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Title { get; set; }
    }

    [Table("Sizes")]
    public class Size
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Value { get; set; }
    }

    [Table("Materials")]
    public class Material
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Title { get; set; }

        public virtual ObservableCollection<Article> Articles { get; set; }
    }

    [Table("Parts")]
    public class Part
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Title { get; set; }
    }

    [Table("Brands")]
    public class Brand
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Title { get; set; }

        public virtual ObservableCollection<Article> Articles { get; set; }
    }

    [Table("Defects")]
    public class Defect
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Title { get; set; }

        public virtual ObservableCollection<Article> Articles { get; set; }
    }

    [Table("GraduationSupplierProportion")]
    public class GraduationSupplierProportion
    {
        [Key]
        public decimal FromPrice { get; set; }
        public decimal SupplierProportion { get; set; }
    }

    [Table("SavedDocuments")]
    public class Document
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public DateTime DateTime { get; set; }
        public DocumentType DocumentType { get; set; }

        [NotMapped]
        public decimal? Sum { get; set; }
        [NotMapped]
        public decimal? SupplierSum { get; set; }

        public virtual ObservableCollection<Article> Articles { get; set; }
        public virtual ObservableCollection<SavedArticleAttributes> SavedArticleAttributes { get; set; }

        public virtual Supplier Supplier { get; set; }
    }



    [Table("SavedArticleAttributes")]
    public class SavedArticleAttributes
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int ArticleId { get; set; }

        public decimal Price { get; set; }
        public decimal Payout { get; set; }

        public virtual ObservableCollection<Document> Documents { get; set; }
    }

    #region Enums

    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum Status
    {
        [Description("Sortiment")]
        Sortiment,
        [Description("Lager")]
        InStock,
        [Description("Reserviert")]
        Reserved,
        [Description("Verkauft")]
        Sold,
        [Description("Ausgezahlt")]
        PayedOut,
        [Description("Zurückgegeben")]
        Returned,
        [Description("Ausgebucht")]
        ClosedOut
    }

    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum DocumentType
    {
        [Description("Annahme")]
        Submission,
        [Description("Reservierung")]
        Reservation,
        [Description("Rechnung")]
        Bill,
        [Description("Auszahlung")]
        Payout,
        [Description("Rückgabe")]
        Return
    }

    #endregion

    //Dient dazu UI bei Veränderung eines Attributes zu benachrichtigen, sodass sie updated 
    public class ObservableObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}