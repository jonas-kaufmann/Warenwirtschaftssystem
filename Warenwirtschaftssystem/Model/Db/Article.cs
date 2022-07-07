using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Warenwirtschaftssystem.Model.Db
{
    public class Article : ObservableObject
    {
        private int _id;
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
                if (_id != 0)
                    return _id + 100000;
                else
                    return 0;
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
            //private set
            //{
            //    _description = value;
            //    OnPropertyChanged(nameof(Description));
            //}
        }

        public void RegenerateDescription()
        {
            bool wasNull = _description == null;

            _description = "";
            bool firstElement = true;

            //Gender
            if (Gender != null && !string.IsNullOrEmpty(Gender.Short))
            {
                _description += Gender.Short;
                firstElement = false;
            }

            //Category
            if (Category != null && !string.IsNullOrEmpty(Category.Name))
            {
                if (!firstElement)
                    _description += "-";
                _description += Category.Name;
                firstElement = false;
            }

            //Type
            if (SubCategory != null && !string.IsNullOrEmpty(SubCategory.Name))
            {
                if (!firstElement)
                    _description += "-";
                _description += SubCategory.Name;
                firstElement = false;
            }

            //Brand
            if (Brand != null && !string.IsNullOrEmpty(Brand.Name))
            {
                if (!firstElement)
                    _description += "-";
                _description += Brand.Name;
                firstElement = false;
            }

            //Size
            if (Size != null && !string.IsNullOrEmpty(Size.Name))
            {
                if (!firstElement)
                    _description += "-";
                _description += Size.Name;
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
                    _description += color.Name;
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
                foreach (var material in Materials)
                {
                    if (!firstElementInList)
                        _description += ",";
                    _description += material.Name;
                    firstElementInList = false;
                }
            }

            //Parts
            if (Parts != null && !string.IsNullOrEmpty(Parts.Name))
            {
                if (!firstElement)
                    _description += "-";
                _description += Parts.Name;
            }

            if (!wasNull)
            {
                OnPropertyChanged(nameof(Description));
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
            //set
            //{
            //    _price = value;
            //    OnPropertyChanged("Price");
            //    OnPropertyChanged("Percentage");
            //}
        }

        private decimal _supplierProportion;
        public decimal SupplierProportion
        {
            get => _supplierProportion;
            //set
            //{
            //    _supplierProportion = Math.Round(value, 2);
            //    OnPropertyChanged("SupplierProportion");
            //    OnPropertyChanged("Percentage");
            //}
        }


        [NotMapped]
        public decimal? Percentage
        {
            get
            {
                if (_price == 0)
                    return null;
                else
                    return Math.Round(_supplierProportion / _price, 4);
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

        public virtual ICollection<Color> Colors { get; set; }
        public virtual Gender Gender { get; set; }
        public virtual Category Category { get; set; }
        public virtual SubCategory SubCategory { get; set; }
        public virtual Size Size { get; set; }
        public virtual ICollection<Material> Materials { get; set; }
        public virtual Parts Parts { get; set; }
        public virtual Brand Brand { get; set; }
        public virtual ICollection<Defect> Defects { get; set; }
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


        #region Reservation

        public virtual Supplier ReservingSupplier { get; set; }
        public DateTime? ReservedFrom { get; set; }
        public DateTime? ReservedUntil { get; set; }

        #endregion

        public virtual ICollection<Document> Documents { get; set; }

        public void TakePropertiesFrom(DbModel dbContext, Article article)
        {
            AddedToSortiment = article.AddedToSortiment;
            _description = article.Description;
            AsNew = article.AsNew;
            Brand = article.Brand;
            Category = article.Category;
            Colors = article.Colors;
            Defects = article.Defects;
            Documents = article.Documents;
            EnteredFinalState = article.EnteredFinalState;
            Gender = article.Gender;
            Id = article.Id;
            Materials = article.Materials;
            Notes = article.Notes;
            Parts = article.Parts;
            PickUp = article.PickUp;
            ReservedFrom = article.ReservedFrom;
            ReservedUntil = article.ReservedUntil;
            ReservingSupplier = article.ReservingSupplier;
            Size = article.Size;
            Sold = article.Sold;
            Status = article.Status;
            Supplier = article.Supplier;
            SubCategory = article.SubCategory;

            ChangePriceAndPayout(dbContext, article.Price, article.SupplierProportion);
        }

        public Article Clone()
        {
            var cloned = new Article();

            foreach (var field in typeof(Article).GetFields())
            {
                field.SetValue(cloned, field.GetValue(this));
            }

            //cloned.AddedToSortiment = article.AddedToSortiment;
            //Description = article.Description;
            //AsNew = article.AsNew;
            //Brand = article.Brand;
            //Category = article.Category;
            //Colors = article.Colors;
            //Defects = article.Defects;
            //Documents = article.Documents;
            //EnteredFinalState = article.EnteredFinalState;
            //Gender = article.Gender;
            //Id = article.Id;
            //Materials = article.Materials;
            //Notes = article.Notes;
            //Parts = article.Parts;
            //PickUp = article.PickUp;
            //Price = article.Price;
            //ReservingSupplier = article.ReservingSupplier;
            //ReservedFrom = article.ReservedFrom;
            //ReservedUntil = article.ReservedUntil;
            //Size = article.Size;
            //Sold = article.Sold;
            //Status = article.Status;
            //Supplier = article.Supplier;
            //SupplierProportion = article.SupplierProportion;
            //SubCategory = article.SubCategory;

            return cloned;
        }

        public override string ToString()
        {
            return Description;
        }

        public void ChangePriceAndPayout(DbModel dbContext, decimal newPrice, decimal newPayout)
        {
            // no changes
            if (newPrice == _price && newPayout == _supplierProportion)
            {
                return;
            }

            var savedArticleAttributes = dbContext.SavedArticleAttributes.Where(c => c.Article == this
                && c.Price == _price
                && c.Payout == _supplierProportion).FirstOrDefault();

            if (savedArticleAttributes == null)
                savedArticleAttributes = new SavedArticleAttributes
                {
                    Article = this,
                    Payout = _supplierProportion,
                    Price = _price
                };

            // affected documents that contain this article && don't already contain saved values
            var affectedDocuments = dbContext.Documents
                .Include(d => d.SavedArticleAttributes)
                .Where(d => d.Articles.Contains(this) && d.SavedArticleAttributes.FirstOrDefault(s => s.Article == this) == null);

            foreach (var document in affectedDocuments)
            {
                document.SavedArticleAttributes.Add(savedArticleAttributes);
            }

            _price = newPrice;
            _supplierProportion = newPayout;

            OnPropertyChanged(nameof(Price));
            OnPropertyChanged(nameof(SupplierProportion));
        }

        public void SetPriceAndPayoutSkipChecks(decimal newPrice, decimal newPayout)
        {
            _price = newPrice;
            _supplierProportion = newPayout;
        }

        public decimal SuggestedPayoutFromPrice(DbModel dbContext)
        {
            decimal newPayout;
            if (Supplier.SupplierProportion.HasValue)
            {
                newPayout = Price * Supplier.SupplierProportion.Value / 100;
            }
            else
            {
                SupplierProportion supplierGraduationProportion = dbContext.SupplierProportions.Where(sGP => sGP.FromPrice <= Price).OrderByDescending(sGP => sGP.FromPrice).First();
                newPayout = Price * supplierGraduationProportion.Proportion / 100;
            }

            return newPayout;
        }
    }

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
}
