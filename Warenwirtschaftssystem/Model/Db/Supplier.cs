using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Warenwirtschaftssystem.Model.Db
{
    public class Supplier
    {
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

        public virtual ICollection<Article> Articles { get; set; }
        public virtual ICollection<ArticleReservation> Reservations { get; set; }

        public override string ToString()
        {
            return Name;
        }
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
                Type type = e.GetType();
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

    public class SupplierProportion
    {
        [Key]
        public decimal FromPrice { get; set; }
        public decimal Proportion { get; set; }
    }
}
