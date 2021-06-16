using System.Collections.Generic;

namespace Warenwirtschaftssystem.Model.Db
{
    public abstract class ArticleAttributeBase
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<Article> Articles { get; set; }

        public override string ToString()
        {
            return Name;
        }

        public string ToRepresentation()
        {
            return Name;
        }
    }

    public class Color : ArticleAttributeBase
    {
        public string ColorCode { get; set; }
    }

    public class Gender : ArticleAttributeBase
    {
        public string Short { get; set; }

        public new string ToRepresentation()
        {
            return Short;
        }
    }

    public class Category : ArticleAttributeBase
    {
        public virtual ICollection<SubCategory> SubCategories { get; set; }
    }

    public class SubCategory : ArticleAttributeBase
    {
        public virtual Category Category { get; set; }
    }

    public class Size : ArticleAttributeBase
    {

    }

    public class Material : ArticleAttributeBase
    {

    }

    public class Parts : ArticleAttributeBase
    {

    }

    public class Brand : ArticleAttributeBase
    {

    }

    public class Defect : ArticleAttributeBase
    {

    }

    public static class ArticleAttributes
    {
        public static string ToRepresentation<T>(ICollection<T> articleAttributes) where T : ArticleAttributeBase
        {
            var s = string.Empty;

            bool firstIteration = true;
            foreach (var attribute in articleAttributes)
            {
                if (firstIteration)
                {
                    s = attribute.ToRepresentation();
                    firstIteration = false;
                } else
                {
                    s += ", " + attribute.ToRepresentation();
                }
            }

            return s;
        }
    }
}
