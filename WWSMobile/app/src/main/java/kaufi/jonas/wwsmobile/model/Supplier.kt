package kaufi.jonas.wwsmobile.model

import java.util.*

class Supplier
{
    var id: Int = 0
    var name: String? = null
    var string Place { get; set; }
    var string EMail { get; set; }
    var string Phone { get; set; }
    var string Street { get; set; }
    var string Postcode { get; set; }
    var decimal? SupplierProportion { get; set; }
    var int PickUp { get; set; }
    var DateTime CreationDate { get; set; }
    var string Notes { get; set; }
    var Title Title { get; set; }
    var string Company { get; set; }

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