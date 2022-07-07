using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using CsvHelper;
using System.Text;
using CsvHelper.Configuration;
using System.Globalization;
using Microsoft.Data.SqlClient;

namespace Warenwirtschaftssystem.Model.Db
{
    public class EasyToRunDb : IDisposable
    {
        private DataModel Data;
        private readonly string ConnectionString;
        private DbModel MainDb;

        private List<Supplier> Suppliers;
        private List<Article> Articles;
        private List<Gender> Genders;
        private List<Category> Categories;
        private List<Brand> Brands;
        private List<Size> Sizes;
        private List<Color> Colors;
        private List<Material> Materials;
        private List<Parts> Parts;
        private List<Defect> Defects;
        private List<SupplierProportion> GraduationSupplierProportions;

        private List<int> ParentGroupsGender;
        private List<int> ParentGroupsBrand;
        private List<int> ParentGroupsSize;
        private List<int> ParentGroupsColor;
        private List<int> ParentGroupsMaterial;
        private List<int> ParentGroupsParts;

        private Dictionary<string, string> GenderConversion;
        private Dictionary<(string, string), (string, string)> CategoryTypeConversion;
        private Dictionary<string, string> BrandConversion;
        private Dictionary<string, string> SizeConversion;
        private Dictionary<string, string> ColorConversion;
        private Dictionary<string, string> MaterialConversion;
        private Dictionary<string, string> PartsConversion;
        private bool UseConversionTables = false;

        private List<Gender> genderFromSonstige;

        private int CategoryIdSonstige;
        private int CategoryIdAccessoires;

        private readonly string ConversionFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Warenwirtschaftssystem\\Überarbeitung Artikelattribute";

        private readonly string[] DefectsArray;

        public EasyToRunDb(DataModel dataModel)
        {
            Data = dataModel;

            SqlConnectionStringBuilder conStrBuilder = new SqlConnectionStringBuilder
            {
                IntegratedSecurity = true,
                DataSource = ".\\SQLEXPRESS",
                InitialCatalog = "EasyToRun"
            };

            ConnectionString = conStrBuilder.ConnectionString;

            DefectsArray = new string[47];

            DefectsArray[0] = "Teile fehlen";
            DefectsArray[1] = "Schäden";
            DefectsArray[2] = "Verschmutzungen";
            DefectsArray[3] = "Allgemeinzustand";
            DefectsArray[10] = "Gürtel fehlt";
            DefectsArray[11] = "Kapuze fehlt";
            DefectsArray[12] = "Pelzbesatz fehlt";
            DefectsArray[13] = "Innenweste fehlt";
            DefectsArray[20] = "Nähte offen";
            DefectsArray[21] = "Reißverschl. defekt";
            DefectsArray[22] = "Druckknopf defekt";
            DefectsArray[23] = "Knopf fehlt";
            DefectsArray[24] = "Fäden gezogen";
            DefectsArray[25] = "Loch";
            DefectsArray[26] = "Mottenfraß";
            DefectsArray[27] = "Stellen ausgerissen";
            DefectsArray[30] = "Fusseln/Haare";
            DefectsArray[31] = "Flecken";
            DefectsArray[32] = "Kragenschmutz";
            DefectsArray[33] = "Schmutz an Bündchen";
            DefectsArray[34] = "Kettverschl. verschmutzt";
            DefectsArray[35] = "Taschen verschmutzt";
            DefectsArray[40] = "Knitterfalten";
            DefectsArray[41] = "Geruch";
            DefectsArray[42] = "Wollknötchen";
            DefectsArray[43] = "Tragespuren";
            DefectsArray[44] = "Gummi verdreht";
            DefectsArray[45] = "Verfärbungen";
            DefectsArray[46] = "verzogen";
        }

        private void ImportSuppliers()
        {
            Suppliers = new List<Supplier>();

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = "SELECT * " +
                                      "FROM tblBasPartner " +
                                      "ORDER BY parID";

                var reader = command.ExecuteReader();

                int supplierId = 1;

                Console.WriteLine("Missing Suppliers:");

                while (reader.Read())
                {
                    Supplier supplier = new Supplier
                    {
                        Id = (int)reader["parID"]
                    };

                    if (supplier.Id > supplierId)
                    {
                        for (int i = 0; i < supplier.Id - supplierId; i++)
                        {
                            Suppliers.Add(new Supplier
                            {
                                CreationDate = DateTime.Now,
                                Name = "Platzhalter",
                                Id = supplierId + i,
                                PickUp = 0,
                                Title = Title.Mr

                            });

                            Console.WriteLine(supplierId + i);
                        }

                        supplierId = supplier.Id;
                    }

                    supplierId++;

                    supplier.Company = reader["parFirma"] as string;
                    supplier.Name = ((reader["parVorname"] as string) + " " + reader["parNachname"] as string).Trim();
                    supplier.Street = reader["parStrasse"] as string;
                    supplier.Postcode = reader["parPLZ"] as string;
                    supplier.Place = reader["parOrt"] as string;
                    supplier.Phone = "";
                    supplier.EMail = reader["parEMail"] as string;
                    supplier.PickUp = (short)reader["parAbholungWo"];
                    supplier.Notes = (reader["parBesonderes"] as string) + "\n\n" + reader["parAufgabe"] as string;
                    supplier.CreationDate = (DateTime)reader["parDatumNeuanlage"];

                    if (reader["parAnrede"] as string == "Frau")
                        supplier.Title = Title.Mrs;
                    else supplier.Title = Title.Mr;

                    string phone = reader["parTelefon"] as string;
                    string eveningPhone = reader["parTelefonAbends"] as string;
                    string mobilePhone = reader["parTelefonMobil"] as string;
                    string fax = reader["parTelefax"] as string;

                    bool phoneNumberInserted = false;

                    if (!string.IsNullOrWhiteSpace(phone))
                    {
                        supplier.Phone += phone;
                        phoneNumberInserted = true;
                    }

                    if (!string.IsNullOrWhiteSpace(eveningPhone))
                    {
                        if (phoneNumberInserted)
                        {
                            supplier.Phone += ", " + eveningPhone;
                        }
                        else
                        {
                            supplier.Phone += eveningPhone;
                            phoneNumberInserted = true;
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(mobilePhone))
                    {
                        if (phoneNumberInserted)
                        {
                            supplier.Phone += ", " + mobilePhone;
                        }
                        else
                        {
                            supplier.Phone += mobilePhone;
                            phoneNumberInserted = true;
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(fax))
                    {
                        if (phoneNumberInserted)
                        {
                            supplier.Phone += ", Fax: " + fax;
                        }
                        else
                        {
                            supplier.Phone += fax;
                            phoneNumberInserted = true;
                        }
                    }

                    Suppliers.Add(supplier);
                }
            }
        }

        private void ImportAttributes()
        {
            if (UseConversionTables)
            {
                GenderConversion = new Dictionary<string, string>();
                CategoryTypeConversion = new Dictionary<(string, string), (string, string)>();
                BrandConversion = new Dictionary<string, string>();
                SizeConversion = new Dictionary<string, string>();
                ColorConversion = new Dictionary<string, string>();
                MaterialConversion = new Dictionary<string, string>();
                PartsConversion = new Dictionary<string, string>();
                Categories.Clear();
                Brands.Clear();
                Sizes.Clear();
                Colors.Clear();
                Materials.Clear();
                Parts.Clear();
                Genders.Clear();

                #region Gender
                using (var reader = File.OpenText(ConversionFolder + "\\Geschlecht.csv"))
                using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.CurrentCulture) { Encoding = Encoding.UTF8, HasHeaderRecord = false }))
                {
                    var records = csv.GetRecords<Attribute>().ToArray();

                    for (int i = 2; i < records.Length; i++)
                    {
                        string newAttribute = records[i].NeuesAttribut;

                        if (!string.IsNullOrWhiteSpace(newAttribute))
                        {
                            GenderConversion.Add(records[i].AltesAttribut, newAttribute);

                            if (Genders.Where(g => g.Name == newAttribute).FirstOrDefault() == null)
                            {
                                Genders.Add(new Gender
                                {
                                    Name = newAttribute,
                                    Short = "" + newAttribute[0]
                                });
                            }
                        }
                    }
                }

                #endregion

                #region Category and Type

                using (var reader = File.OpenText(ConversionFolder + "\\KategorienUndArten.csv"))
                using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.CurrentCulture) { Encoding = Encoding.UTF8, HasHeaderRecord = false }))
                {
                    var records = csv.GetRecords<CategoryType>().ToArray();

                    for (int i = 2; i < records.Length; i++)
                    {
                        CategoryType record = records[i];

                        if (!CategoryTypeConversion.TryGetValue((record.AlteKategorie, record.AlteArt), out (string, string) value))
                            CategoryTypeConversion.Add((record.AlteKategorie, record.AlteArt), (record.NeueKategorie, record.NeueArt));

                        if (!string.IsNullOrWhiteSpace(record.AlteKategorie) && !string.IsNullOrWhiteSpace(record.NeueKategorie))
                        {
                            Category category = Categories.Where(c => c.Name == record.NeueKategorie).FirstOrDefault();
                            if (category == null)
                            {
                                category = new Category
                                {
                                    Name = record.NeueKategorie
                                };

                                Categories.Add(category);
                            }

                            if (category.SubCategories == null)
                                category.SubCategories = new ObservableCollection<SubCategory>();

                            if (record.NeueArt != "" && !category.SubCategories.Any(t => t.Name == record.NeueArt))
                                category.SubCategories.Add(new SubCategory
                                {
                                    Name = record.NeueArt
                                });
                        }
                    }
                }

                #endregion

                #region Brand

                using (var reader = File.OpenText(ConversionFolder + "\\Marken.csv"))
                using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.CurrentCulture) { Encoding = Encoding.UTF8, HasHeaderRecord = false }))
                {
                    var records = csv.GetRecords<Attribute>().ToArray();

                    for (int i = 2; i < records.Length; i++)
                    {
                        string newAttribute = records[i].NeuesAttribut;

                        if (!string.IsNullOrWhiteSpace(newAttribute))
                        {
                            BrandConversion.Add(records[i].AltesAttribut, newAttribute);

                            if (Brands.Where(b => b.Name == newAttribute).FirstOrDefault() == null)
                            {
                                Brands.Add(new Brand
                                {
                                    Name = newAttribute
                                });
                            }
                        }
                    }
                }

                #endregion

                #region Size

                using (var reader = new StreamReader(ConversionFolder + "\\Größen.csv"))
                using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.CurrentCulture) { Encoding = Encoding.UTF8, HasHeaderRecord = false }))
                {
                    var records = csv.GetRecords<Attribute>().ToArray();

                    for (int i = 2; i < records.Length; i++)
                    {
                        string newAttribute = records[i].NeuesAttribut;

                        if (!string.IsNullOrWhiteSpace(newAttribute))
                        {
                            SizeConversion.Add(records[i].AltesAttribut, newAttribute);

                            if (Sizes.Where(s => s.Name == newAttribute).FirstOrDefault() == null)
                            {
                                Sizes.Add(new Size
                                {
                                    Name = newAttribute
                                });
                            }
                        }
                    }
                }

                #endregion

                #region Color

                using (var reader = new StreamReader(ConversionFolder + "\\Farben.csv"))
                using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.CurrentCulture) { Encoding = Encoding.UTF8, HasHeaderRecord = false }))
                {
                    var records = csv.GetRecords<Attribute>().ToArray();

                    for (int i = 2; i < records.Length; i++)
                    {
                        string newAttribute = records[i].NeuesAttribut;

                        if (!string.IsNullOrWhiteSpace(newAttribute))
                        {
                            ColorConversion.Add(records[i].AltesAttribut, newAttribute);

                            if (Colors.Where(c => c.Name == newAttribute).FirstOrDefault() == null)
                            {
                                Colors.Add(new Color
                                {
                                    Name = newAttribute
                                });
                            }
                        }
                    }
                }

                #endregion

                #region Material

                using (var reader = new StreamReader(ConversionFolder + "\\Materialien.csv"))
                using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.CurrentCulture) { Encoding = Encoding.UTF8, HasHeaderRecord = false }))
                {
                    var records = csv.GetRecords<Attribute>().ToArray();

                    for (int i = 2; i < records.Length; i++)
                    {
                        string newAttribute = records[i].NeuesAttribut;

                        if (!string.IsNullOrWhiteSpace(newAttribute))
                        {
                            MaterialConversion.Add(records[i].AltesAttribut, newAttribute);

                            if (Materials.Where(m => m.Name == newAttribute).FirstOrDefault() == null)
                            {
                                Materials.Add(new Material
                                {
                                    Name = newAttribute
                                });
                            }
                        }
                    }
                }

                #endregion

                #region Part

                using (var reader = new StreamReader(ConversionFolder + "\\Teile.csv"))
                using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.CurrentCulture) { Encoding = Encoding.UTF8, HasHeaderRecord = false }))
                {
                    var records = csv.GetRecords<Attribute>().ToArray();

                    for (int i = 2; i < records.Length; i++)
                    {
                        string newAttribute = records[i].NeuesAttribut;

                        if (!string.IsNullOrWhiteSpace(newAttribute))
                        {
                            PartsConversion.Add(records[i].AltesAttribut, newAttribute);

                            if (Parts.Where(p => p.Name == newAttribute).FirstOrDefault() == null)
                            {
                                Parts.Add(new Parts
                                {
                                    Name = newAttribute
                                });
                            }
                        }
                    }
                }

                #endregion
            }
            else
            {
                Task[] tasks = new Task[]
{
                Task.Run(() => ImportGenderAttributes()),
                Task.Run(() => ImportCategoryAndTypeAttributes()),
                Task.Run(() => ImportBrandAttributes()),
                Task.Run(() => ImportSizeAttributes()),
                Task.Run(() => ImportColorAttributes()),
                Task.Run(() => ImportMaterialAttributes()),
                Task.Run(() => ImportPartsAttributes()),
                Task.Run(() => ImportDefectsAttributes())
};

                Task.WaitAll(tasks);

                if (genderFromSonstige != null)
                {
                    foreach (Gender gender in genderFromSonstige)
                    {
                        if (Genders.Where(g => g.Name == gender.Name).FirstOrDefault() == null)
                            Genders.Add(gender);
                    }

                    genderFromSonstige = null;
                }
            }
        }

        private void ImportGenderAttributes()
        {
            Genders = new List<Gender>();
            ParentGroupsGender = new List<int>();

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "SELECT parentGroup.grpID " +
                      "FROM tblBasGroups parentGroup " +
                      "WHERE parentGroup.grpLevel = 3 AND parentGroup.grpDesc = 'für'";

                var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    ParentGroupsGender.Add((int)reader[0]);
                }

                reader.Close();

                command = connection.CreateCommand();
                command.CommandText = "SELECT DISTINCT TRIM(genders.grpDesc) AS gender " +
                                      "FROM tblBasGroups genders, tblBasGroups parentGroup " +
                                      "WHERE parentGroup.grpLevel = 3 AND parentGroup.grpDesc = 'für' " +
                                      "      AND genders.grpLevel = 4 AND genders.grpParent = parentGroup.grpID " +
                                      "ORDER BY gender";

                reader = command.ExecuteReader();

                while (reader.Read())
                {
                    string gender = (string)reader[0];

                    Genders.Add(new Gender
                    {
                        Name = gender,
                        Short = "" + gender[0]
                    });
                }
            }
        }

        private void ImportCategoryAndTypeAttributes()
        {
            Categories = new List<Category>();

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "SELECT DISTINCT TRIM(categories.grpDesc) AS Category, TRIM(types.grpDesc) AS Type, categories.grpID AS CategoryId, types.grpID " +
                                      "FROM tblBasGroups categories, tblBasGroups types " +
                                      "WHERE categories.grpLevel = 1 AND types.grpLevel = 2 " +
                                      "      AND types.grpParent = categories.grpID " +
                                      "ORDER BY Category, Type";

                var reader = command.ExecuteReader();

                string categoryName = "";
                List<Category> categoriesBelongingToAccessoires = new List<Category>();

                while (reader.Read())
                {
                    string readCategory = (string)reader["Category"];

                    var subCategory = new SubCategory
                    {
                        Name = (string)reader["Type"]
                    };

                    if (readCategory == "Sonstige")
                    {
                        if (readCategory != categoryName)
                        {
                            CategoryIdSonstige = (int)reader["CategoryId"];

                            Categories.Add(new Category
                            {
                                Name = readCategory
                            });

                            categoryName = readCategory;
                        }
                    }
                    else if (readCategory == "Accessoires")
                    {
                        CategoryIdAccessoires = (int)reader["CategoryId"];

                        string typeTitle = (string)reader["Type"];

                        if (Categories.Where(c => c.Name == typeTitle).FirstOrDefault() == null)
                        {
                            Category category = new Category
                            {
                                Id = (int)reader[3],
                                Name = typeTitle
                            };

                            Categories.Add(category);
                            categoriesBelongingToAccessoires.Add(category);
                        }
                    }
                    else
                    {
                        if (readCategory != categoryName)
                        {
                            Categories.Add(new Category
                            {
                                Name = readCategory,
                                SubCategories = new ObservableCollection<SubCategory> { subCategory }
                            });

                            categoryName = readCategory;
                        }
                        else
                        {
                            Categories[Categories.Count - 1].SubCategories.Add(subCategory);
                        }
                    }
                }

                reader.Close();

                #region Kategorie Accessoires

                if (CategoryIdAccessoires != 0)
                {
                    foreach (Category category in Categories)
                    {
                        command = connection.CreateCommand();
                        command.CommandText = "SELECT DISTINCT TRIM(types.grpDesc) AS Type, types.grpID " +
                                              "FROM tblBasGroups types " +
                                              "WHERE types.grpLevel = 3 AND types.grpParent = " + category.Id + " " +
                                              "      AND NOT types.grpDesc = 'Marke' AND NOT types.grpDesc = 'Zustand' " +
                                              "      AND NOT types.grpDesc = 'Zubehör' AND NOT types.grpDesc = 'Discounter' " +
                                              "      AND NOT types.grpDesc = 'Farbe' AND NOT types.grpDesc = 'für' " +
                                              "      AND NOT types.grpDesc = 'Material' AND NOT types.grpDesc = 'Größe' " +
                                              "      AND NOT types.grpDesc = 'zzForm' AND NOT types.grpDesc = 'zzSaison' " +
                                              "      AND NOT types.grpDesc = 'Länge' AND NOT types.grpDesc = 'Anzahl' " +
                                              "ORDER BY Type";

                        reader = command.ExecuteReader();

                        int artID = 0;

                        while (reader.Read())
                        {
                            if (category.SubCategories == null)
                                category.SubCategories = new ObservableCollection<SubCategory>();

                            string typeName = (string)reader[0];
                            if (typeName == "Art")
                            {
                                artID = (int)reader[1];
                            }
                            else
                            {
                                category.SubCategories.Add(new SubCategory
                                {
                                    Name = typeName
                                });
                            }
                        }

                        reader.Close();

                        if (artID != 0)
                        {
                            command = connection.CreateCommand();
                            command.CommandText = "SELECT DISTINCT TRIM(types.grpDesc) AS Type " +
                                                  "FROM tblBasGroups types " +
                                                  "WHERE types.grpLevel = 4 AND types.grpParent = " + artID + " " +
                                                  "ORDER BY Type";

                            reader = command.ExecuteReader();

                            while (reader.Read())
                            {
                                if (category.SubCategories == null)
                                    category.SubCategories = new ObservableCollection<SubCategory>();

                                string typeName = (string)reader[0];
                                category.SubCategories.Add(new SubCategory
                                {
                                    Name = typeName
                                });
                            }

                            reader.Close();
                        }

                        reader.Close();
                    }
                }

                #endregion

                #region Kategorie 'Sonstige'

                if (CategoryIdSonstige != 0)
                {
                    genderFromSonstige = new List<Gender>();

                    command = connection.CreateCommand();
                    command.CommandText = "SELECT DISTINCT TRIM(genders.grpDesc) AS grpDesc " +
                                          "FROM tblBasGroups genders " +
                                          "WHERE genders.grpLevel = 2 AND genders.grpParent = " + CategoryIdSonstige;

                    reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        string grpDesc = (string)reader["grpDesc"];

                        if (Genders.Where(g => g.Name == grpDesc).FirstOrDefault() == null)
                            genderFromSonstige.Add(new Gender
                            {
                                Name = grpDesc,
                                Short = "" + grpDesc[0]
                            });
                    }

                    reader.Close();

                    command = connection.CreateCommand();
                    command.CommandText = "SELECT DISTINCT TRIM(types.grpDesc) AS grpDesc " +
                                          "FROM tblBasGroups types, tblBasGroups sonstige, tblBasGroups genders " +
                                          "WHERE genders.grpLevel = 2 AND genders.grpParent = " + CategoryIdSonstige + " " +
                                          " AND sonstige.grpLevel = 3 AND sonstige.grpDesc = 'Sonstige' AND sonstige.grpParent = genders.grpID " +
                                          " AND types.grpLevel = 4 AND types.grpParent = sonstige.grpID";

                    reader = command.ExecuteReader();

                    Category categorySonstige = Categories.Where(c => c.Name == "Sonstige").First();

                    while (reader.Read())
                    {
                        if (categorySonstige.SubCategories == null)
                            categorySonstige.SubCategories = new ObservableCollection<SubCategory>();

                        string type = (string)reader["grpDesc"];

                        categorySonstige.SubCategories.Add(new SubCategory
                        {
                            Name = type
                        });
                    }
                }

                #endregion
            }
        }

        private void ImportBrandAttributes()
        {
            Brands = new List<Brand>();
            ParentGroupsBrand = new List<int>();

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "SELECT parentGroups.grpID " +
                                      "FROM tblBasGroups parentGroups " +
                                      "WHERE parentGroups.grpLevel = 3 AND parentGroups.grpDesc = 'Marke'";

                var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    ParentGroupsBrand.Add((int)reader[0]);
                }

                reader.Close();

                command = connection.CreateCommand();
                command.CommandText = "SELECT DISTINCT TRIM(brands.grpDesc) AS brand " +
                                      "FROM tblBasGroups brands, tblBasGroups parentGroups " +
                                      "WHERE parentGroups.grpLevel = 3 AND parentGroups.grpDesc = 'Marke' " +
                                      "      AND brands.grpLevel = 4 AND brands.grpParent = parentGroups.grpID " +
                                      "ORDER BY brand";

                reader = command.ExecuteReader();

                while (reader.Read())
                {

                    Brands.Add(new Brand
                    {
                        Name = (string)reader[0]

                    });
                }
            }
        }

        private void ImportSizeAttributes()
        {
            Sizes = new List<Size>();
            ParentGroupsSize = new List<int>();

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "SELECT parentGroups.grpID " +
                                      "FROM tblBasGroups parentGroups " +
                                      "WHERE parentGroups.grpLevel = 3 AND parentGroups.grpDesc = 'Größe'";

                var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    ParentGroupsSize.Add((int)reader[0]);
                }

                reader.Close();

                command = connection.CreateCommand();
                command.CommandText = "SELECT DISTINCT TRIM(sizes.grpDesc) AS size " +
                                      "FROM tblBasGroups sizes, tblBasGroups parentGroups " +
                                      "WHERE parentGroups.grpLevel = 3 AND parentGroups.grpDesc = 'Größe' " +
                                      "      AND sizes.grpLevel = 4 AND sizes.grpParent = parentGroups.grpID " +
                                      "ORDER BY size";

                reader = command.ExecuteReader();

                while (reader.Read())
                {
                    Sizes.Add(new Size
                    {
                        Name = (string)reader[0]
                    });
                }
            }
        }

        private void ImportColorAttributes()
        {
            Colors = new List<Color>();
            ParentGroupsColor = new List<int>();

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "SELECT parentGroups.grpID " +
                                      "FROM tblBasGroups parentGroups " +
                                      "WHERE parentGroups.grpLevel = 3 AND parentGroups.grpDesc = 'Farbe'";

                var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    ParentGroupsColor.Add((int)reader[0]);
                }

                reader.Close();

                command = connection.CreateCommand();
                command.CommandText = "SELECT DISTINCT TRIM(colors.grpDesc) AS color " +
                                      "FROM tblBasGroups colors, tblBasGroups parentGroups " +
                                      "WHERE parentGroups.grpLevel = 3 AND parentGroups.grpDesc = 'Farbe' " +
                                      "      AND colors.grpLevel = 4 AND colors.grpParent = parentGroups.grpID " +
                                      "ORDER BY color";

                reader = command.ExecuteReader();

                while (reader.Read())
                {
                    Colors.Add(new Color
                    {
                        Name = (string)reader[0]
                    });
                }
            }
        }

        private void ImportMaterialAttributes()
        {
            Materials = new List<Material>();
            ParentGroupsMaterial = new List<int>();

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "SELECT parentGroups.grpID " +
                                      "FROM tblBasGroups parentGroups " +
                                      "WHERE parentGroups.grpLevel = 3 AND parentGroups.grpDesc = 'Material'";

                var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    ParentGroupsMaterial.Add((int)reader[0]);
                }

                reader.Close();

                command = connection.CreateCommand();
                command.CommandText = "SELECT DISTINCT TRIM(materials.grpDesc) AS material " +
                                      "FROM tblBasGroups materials, tblBasGroups parentGroups " +
                                      "WHERE parentGroups.grpLevel = 3 AND parentGroups.grpDesc = 'Material' " +
                                      "      AND materials.grpLevel = 4 AND materials.grpParent = parentGroups.grpID " +
                                      "ORDER BY material";

                reader = command.ExecuteReader();

                while (reader.Read())
                {
                    Materials.Add(new Material
                    {
                        Name = (string)reader[0]
                    });
                }
            }
        }

        private void ImportPartsAttributes()
        {
            Parts = new List<Parts>();
            ParentGroupsParts = new List<int>();

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "SELECT parentGroups.grpID " +
                      "FROM tblBasGroups parentGroups " +
                      "WHERE parentGroups.grpLevel = 3 AND parentGroups.grpDesc = 'Teile'";

                var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    ParentGroupsParts.Add((int)reader[0]);
                }

                reader.Close();

                command = connection.CreateCommand();
                command.CommandText = "SELECT DISTINCT TRIM(parts.grpDesc) AS part " +
                      "FROM tblBasGroups parts, tblBasGroups parentGroups " +
                      "WHERE parentGroups.grpLevel = 3 AND parentGroups.grpDesc = 'Teile' " +
                      "      AND parts.grpLevel = 4 AND parts.grpParent = parentGroups.grpID " +
                      "ORDER BY part";

                reader = command.ExecuteReader();

                while (reader.Read())
                {
                    Parts.Add(new Parts
                    {
                        Name = (string)reader[0]
                    });
                }
            }
        }

        private void ImportDefectsAttributes()
        {
            Defects = new List<Defect>();

            foreach (var item in DefectsArray)
            {
                if (item != null)
                {
                    Defects.Add(new Defect
                    {
                        Name = item
                    });
                }
            }
        }

        private void ImportArticleAttributes(Article article)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "SELECT DISTINCT tblBasGroups.grpParent AS grpParent, TRIM(tblBasGroups.grpDesc) AS grpDesc " +
                                  "FROM tblBasGroups, tblCurArtikelBeschreibung " +
                                  "WHERE tblCurArtikelBeschreibung.arbArtID = " + article.Id + " AND tblBasGroups.grpID = tblCurArtikelBeschreibung.arbGrpID " +
                                  "      AND tblBasGroups.grpLevel = 4 " +
                                  "ORDER BY grpParent";

                var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    int parentGroupId = (int)reader["grpParent"];

                    if (ParentGroupsGender.Contains(parentGroupId))
                    {
                        string grpDesc = (string)reader["grpDesc"];
                        if (!string.IsNullOrWhiteSpace(grpDesc))
                        {
                            var gender = Genders.Where(g => g.Name == grpDesc).Single();
                            article.Gender = gender;
                        }
                    }
                    else if (ParentGroupsBrand.Contains(parentGroupId))
                    {
                        string grpDesc = (string)reader["grpDesc"];
                        if (!string.IsNullOrWhiteSpace(grpDesc))
                        {
                            var brand = Brands.Where(b => b.Name == grpDesc).Single();
                            article.Brand = brand;
                        }
                    }
                    else if (ParentGroupsSize.Contains(parentGroupId))
                    {
                        string grpDesc = (string)reader["grpDesc"];
                        if (!string.IsNullOrWhiteSpace(grpDesc))
                        {
                            var size = Sizes.Where(s => s.Name == grpDesc).Single();
                            article.Size = size;
                        }
                    }
                    else if (ParentGroupsColor.Contains(parentGroupId))
                    {
                        string grpDesc = (string)reader["grpDesc"];
                        if (!string.IsNullOrWhiteSpace(grpDesc))
                        {
                            var color = Colors.Where(c => c.Name == grpDesc).Single();

                            if (article.Colors == null)
                                article.Colors = new ObservableCollection<Color>();

                            article.Colors.Add(color);
                        }
                    }
                    else if (ParentGroupsMaterial.Contains(parentGroupId))
                    {
                        string grpDesc = (string)reader["grpDesc"];
                        if (!string.IsNullOrWhiteSpace(grpDesc))
                        {
                            var material = Materials.Where(m => m.Name == grpDesc).Single();

                            if (article.Materials == null)
                                article.Materials = new ObservableCollection<Material>();

                            article.Materials.Add(material);
                        }
                    }
                    else if (ParentGroupsParts.Contains(parentGroupId))
                    {
                        string grpDesc = (string)reader["grpDesc"];
                        if (!string.IsNullOrWhiteSpace(grpDesc))
                        {
                            var parts = Parts.Where(p => p.Name == grpDesc).Single();
                            article.Parts = parts;
                        }
                    }
                }

                reader.Close();

                #region Mängel

                string articleInfo = article.Notes;
                int index = articleInfo.IndexOf("artikelzustand", StringComparison.CurrentCultureIgnoreCase);
                if (index == -1)
                {
                    articleInfo.IndexOf("zustand", StringComparison.CurrentCultureIgnoreCase);
                }

                if (index != -1)
                {
                    articleInfo = articleInfo.Substring(index + 14).Trim();
                    string number = "";

                    List<string> articleDefectNames = new List<string>();

                    int i;

                    for (i = 0; i < articleInfo.Length; i++)
                    {
                        char c = articleInfo[i];
                        if (char.IsDigit(c))
                        {
                            number += c;
                        }
                        else if ((c == ' ' || c == ',') && number != "")
                        {
                            try
                            {
                                string defect = DefectsArray[int.Parse(number) - 1];
                                if (defect != null)
                                    articleDefectNames.Add(defect);
                            }
                            catch { }

                            number = "";
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (number != "")
                        try
                        {
                            string defect = DefectsArray[int.Parse(number) - 1];
                            if (defect != null)
                                articleDefectNames.Add(defect);
                        }
                        catch { }

                    article.Defects = new ObservableCollection<Defect>(Defects.Where(d => articleDefectNames.Contains(d.Name)));
                }

                reader.Close();

                #endregion

                #region Neuware

                command = connection.CreateCommand();
                command.CommandText = "SELECT DISTINCT TRIM(conditions.grpDesc) " +
                                      "FROM tblBasGroups conditions, tblBasGroups parentGroups, tblCurArtikelBeschreibung " +
                                      "WHERE parentGroups.grpLevel = 3 AND parentGroups.grpDesc = 'Zustand' " +
                                      "      AND conditions.grpLevel = 4 AND conditions.grpParent = parentGroups.grpID " +
                                      "      AND tblCurArtikelBeschreibung.arbArtID =" + article.Id + " " +
                                      "      AND tblCurArtikelBeschreibung.arbGrpID = conditions.grpID";

                reader = command.ExecuteReader();

                List<string> conditions = new List<string>();

                while (reader.Read())
                {
                    conditions.Add((string)reader[0]);
                }

                if (conditions.Any(c => c == "Neuware"))
                    article.AsNew = true;
                else
                    article.AsNew = false;

                reader.Close();

                #endregion

                #region Kategorie & Typ

                command = connection.CreateCommand();
                command.CommandText = "SELECT TOP 1 TRIM(tblBasGroups.grpDesc), tblBasGroups.grpID AS grpID " +
                                      "FROM tblBasGroups, tblCurArtikelBeschreibung " +
                                      "WHERE tblBasGroups.grpLevel = 1 " +
                                      "      AND tblCurArtikelBeschreibung.arbArtID =" + article.Id + " " +
                                      "      AND tblCurArtikelBeschreibung.arbGrpID = tblBasGroups.grpID";

                reader = command.ExecuteReader();
                if (reader.Read())
                {
                    string categoryName = (string)reader[0];
                    var category = Categories.Where(c => c.Name == categoryName).FirstOrDefault();

                    if (category != null)
                    {
                        article.Category = category;
                        int categoryGrpId = (int)reader["grpID"];

                        reader.Close();

                        command = connection.CreateCommand();
                        command.CommandText = "SELECT TOP 1 TRIM(types.grpDesc), types.grpID AS grpID " +
                                              "FROM tblBasGroups types, tblCurArtikelBeschreibung " +
                                              "WHERE types.grpLevel = 2 AND types.grpParent = " + categoryGrpId + " " +
                                              "      AND tblCurArtikelBeschreibung.arbArtID = " + article.Id + " " +
                                              "      AND tblCurArtikelBeschreibung.arbGrpID = types.grpID";

                        reader = command.ExecuteReader();

                        if (reader.Read())
                        {
                            if (categoryName == "Sonstige")
                            {
                                string genderName = (string)reader[0];
                                int genderGrpId = (int)reader["grpID"];

                                Gender gender = Genders.Where(g => g.Name == genderName).First();
                                article.Gender = gender;

                                reader.Close();

                                command = connection.CreateCommand();

                                command.CommandText = "SELECT TOP 1 TRIM(types.grpDesc) " +
                                                      "FROM tblBasGroups types, tblBasGroups sonstige, tblCurArtikelBeschreibung " +
                                                      "WHERE sonstige.grpLevel = 3 AND sonstige.grpDesc = 'Sonstige' AND sonstige.grpParent = " + genderGrpId + " " +
                                                      "      AND types.grpLevel = 4 AND types.grpParent = sonstige.grpID " +
                                                      "      AND tblCurArtikelBeschreibung.arbArtID = " + article.Id + " " +
                                                      "      AND tblCurArtikelBeschreibung.arbGrpID = types.grpID";

                                reader = command.ExecuteReader();

                                if (reader.Read())
                                {
                                    string typeName = (string)reader[0];
                                    article.SubCategory = category.SubCategories.Where(t => t.Name == typeName).First();
                                }
                            }
                            else if (categoryName == "Accessoires")
                            {
                                categoryName = (string)reader[0];
                                int categoryId = (int)reader[1];

                                category = Categories.Where(c => c.Name == categoryName).FirstOrDefault();
                                if (category != null)
                                {
                                    article.Category = category;

                                    reader.Close();

                                    command = connection.CreateCommand();
                                    command.CommandText = "SELECT TOP 1 TRIM(type.grpDesc), type.grpID " +
                                                          "FROM tblBasGroups type, tblCurArtikelBeschreibung " +
                                                          "WHERE type.grpLevel = 3 AND type.grpParent = " + categoryId + " " +
                                                          "      AND tblCurArtikelBeschreibung.arbArtID = " + article.Id + " " +
                                                          "      AND tblCurArtikelBeschreibung.arbGrpID = type.grpID " +
                                                          "      AND NOT types.grpDesc = 'Marke' AND NOT types.grpDesc = 'Zustand' " +
                                                          "      AND NOT types.grpDesc = 'Zubehör' AND NOT types.grpDesc = 'Discounter' " +
                                                          "      AND NOT types.grpDesc = 'Farbe' AND NOT types.grpDesc = 'für' " +
                                                          "      AND NOT types.grpDesc = 'Material' AND NOT types.grpDesc = 'Größe' " +
                                                          "      AND NOT types.grpDesc = 'zzForm' AND NOT types.grpDesc = 'zzSaison' " +
                                                          "      AND NOT types.grpDesc = 'Länge' AND NOT types.grpDesc = 'Anzahl'";

                                    reader = command.ExecuteReader();

                                    if (reader.Read())
                                    {
                                        string typeName = (string)reader[0];

                                        if (typeName == "Art")
                                        {
                                            int artGrpID = (int)reader[1];

                                            reader.Close();

                                            command = connection.CreateCommand();
                                            command.CommandText = "SELECT TOP 1 TRIM(type.grpDesc) " +
                                                                  "FROM tblBasGroups type, tblCurArtikelBeschreibung " +
                                                                  "WHERE type.grpLevel = 4 AND type.grpParent = " + artGrpID + " " +
                                                                  "      AND tblCurArtikelBeschreibung.arbArtID = " + article.Id + " " +
                                                                  "      AND tblCurArtikelBeschreibung.arbGrpID = type.grpID";

                                            reader = command.ExecuteReader();

                                            typeName = (string)reader[0];
                                        }

                                        if (article.Category.SubCategories.Where(t => t.Name == typeName).FirstOrDefault() is SubCategory type)
                                            article.SubCategory = type;
                                    } else
                                    {
                                        reader.Close();

                                        command = connection.CreateCommand();
                                        command.CommandText = "SELECT TOP 1 TRIM(type.grpDesc) " +
                                                              "FROM tblBasGroups art, tblBasGroups type, tblCurArtikelBeschreibung " +
                                                              "WHERE art.grpLevel = 3 AND art.grpDesc = 'Art' and art.grpParent = " + categoryId + " " +
                                                              "      AND type.grpLevel = 4 AND type.grpParent = art.grpID " +
                                                              "      AND tblCurArtikelBeschreibung.arbArtID = " + article.Id + " " +
                                                              "      AND tblCurArtikelBeschreibung.arbGrpID = type.grpID";

                                        reader = command.ExecuteReader();

                                        if (reader.Read())
                                        {
                                            string typeName = (string)reader[0];

                                            if (article.Category.SubCategories.Where(t => t.Name == typeName).FirstOrDefault() is SubCategory type)
                                                article.SubCategory = type;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                string genderName = null;
                                if (categoryName == "Herren Bekleidung")
                                {
                                    genderName = "Herren";
                                }
                                else if (categoryName == "Damen Bekleidung")
                                {
                                    genderName = "Damen";
                                }
                                else if (categoryName == "Kinder Bekleidung")
                                {
                                    genderName = "Kinder";
                                }

                                if (genderName != null && article.Gender == null)
                                    article.Gender = Genders.Where(g => g.Name == genderName).FirstOrDefault();

                                string typeName = (string)reader[0];
                                var type = category.SubCategories.Where(t => t.Name == typeName).FirstOrDefault();

                                if (type != null)
                                    article.SubCategory = type;
                            }
                        }
                    }
                }

                reader.Close();

                #endregion
            }
        }

        private void ImportArticleAttributesWithConversion(Article article)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "SELECT DISTINCT tblBasGroups.grpParent AS grpParent, TRIM(tblBasGroups.grpDesc) AS grpDesc " +
                                  "FROM tblBasGroups, tblCurArtikelBeschreibung " +
                                  "WHERE tblCurArtikelBeschreibung.arbArtID = " + article.Id + " AND tblBasGroups.grpID = tblCurArtikelBeschreibung.arbGrpID " +
                                  "      AND tblBasGroups.grpLevel = 4 " +
                                  "ORDER BY grpParent";

                var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    int parentGroupId = (int)reader["grpParent"];

                    //Gender
                    if (ParentGroupsGender.Contains(parentGroupId))
                    {
                        string grpDesc = (string)reader["grpDesc"];
                        if (!string.IsNullOrWhiteSpace(grpDesc))
                        {
                            if (GenderConversion.TryGetValue(grpDesc, out string newGenderDesc))
                            {
                                var gender = Genders.Where(g => g.Name == newGenderDesc).First();
                                article.Gender = gender;
                            }
                        }
                    }
                    //Brand
                    else if (ParentGroupsBrand.Contains(parentGroupId))
                    {
                        string grpDesc = (string)reader["grpDesc"];
                        if (!string.IsNullOrWhiteSpace(grpDesc))
                        {
                            if (BrandConversion.TryGetValue(grpDesc, out string newBrandDesc))
                            {
                                var brand = Brands.Where(b => b.Name == newBrandDesc).First();
                                article.Brand = brand;
                            }
                        }
                    }
                    //Size
                    else if (ParentGroupsSize.Contains(parentGroupId))
                    {
                        string grpDesc = (string)reader["grpDesc"];
                        if (!string.IsNullOrWhiteSpace(grpDesc))
                        {
                            if (SizeConversion.TryGetValue(grpDesc, out string newSizeDesc))
                            {
                                var size = Sizes.Where(s => s.Name == newSizeDesc).First();
                                article.Size = size;
                            }
                        }
                    }
                    //Color
                    else if (ParentGroupsColor.Contains(parentGroupId))
                    {
                        string grpDesc = (string)reader["grpDesc"];
                        if (!string.IsNullOrWhiteSpace(grpDesc))
                        {
                            if (ColorConversion.TryGetValue(grpDesc, out string newColorDesc))
                            {
                                var color = Colors.Where(c => c.Name == newColorDesc).First();

                                if (article.Colors == null)
                                    article.Colors = new ObservableCollection<Color>();

                                article.Colors.Add(color);
                            }
                        }
                    }
                    //Material
                    else if (ParentGroupsMaterial.Contains(parentGroupId))
                    {
                        string grpDesc = (string)reader["grpDesc"];
                        if (!string.IsNullOrWhiteSpace(grpDesc))
                        {
                            if (MaterialConversion.TryGetValue(grpDesc, out string newMaterialDesc))
                            {
                                var material = Materials.Where(m => m.Name == newMaterialDesc).Single();

                                if (article.Materials == null)
                                    article.Materials = new ObservableCollection<Material>();

                                article.Materials.Add(material);
                            }
                        }
                    }
                    //Parts
                    else if (ParentGroupsParts.Contains(parentGroupId))
                    {
                        string grpDesc = (string)reader["grpDesc"];
                        if (!string.IsNullOrWhiteSpace(grpDesc))
                        {
                            if (PartsConversion.TryGetValue(grpDesc, out string newPartsDesc))
                            {
                                var parts = Parts.Where(p => p.Name == newPartsDesc).Single();
                                article.Parts = parts;
                            }
                        }
                    }
                }

                reader.Close();

                #region Mängel

                string articleInfo = article.Notes;
                int index = articleInfo.IndexOf("artikelzustand", StringComparison.CurrentCultureIgnoreCase);
                if (index == -1)
                {
                    articleInfo.IndexOf("zustand", StringComparison.CurrentCultureIgnoreCase);
                }

                if (index != -1)
                {
                    articleInfo = articleInfo.Substring(index + 14).Trim();
                    string number = "";

                    List<string> articleDefectNames = new List<string>();

                    int i;

                    for (i = 0; i < articleInfo.Length; i++)
                    {
                        char c = articleInfo[i];
                        if (char.IsDigit(c))
                        {
                            number += c;
                        }
                        else if ((c == ' ' || c == ',') && number != "")
                        {
                            try
                            {
                                string defect = DefectsArray[int.Parse(number) - 1];
                                if (defect != null)
                                    articleDefectNames.Add(defect);
                            }
                            catch { }

                            number = "";
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (number != "")
                        try
                        {
                            string defect = DefectsArray[int.Parse(number) - 1];
                            if (defect != null)
                                articleDefectNames.Add(defect);
                        }
                        catch { }

                    article.Defects = new ObservableCollection<Defect>(Defects.Where(d => articleDefectNames.Contains(d.Name)));
                }

                reader.Close();

                #endregion

                #region Neuware

                command = connection.CreateCommand();
                command.CommandText = "SELECT DISTINCT TRIM(conditions.grpDesc) " +
                                      "FROM tblBasGroups conditions, tblBasGroups parentGroups, tblCurArtikelBeschreibung " +
                                      "WHERE parentGroups.grpLevel = 3 AND parentGroups.grpDesc = 'Zustand' " +
                                      "      AND conditions.grpLevel = 4 AND conditions.grpParent = parentGroups.grpID " +
                                      "      AND tblCurArtikelBeschreibung.arbArtID =" + article.Id + " " +
                                      "      AND tblCurArtikelBeschreibung.arbGrpID = conditions.grpID";

                reader = command.ExecuteReader();

                List<string> conditions = new List<string>();

                while (reader.Read())
                {
                    conditions.Add((string)reader[0]);
                }

                if (conditions.Any(c => c == "Neuware"))
                    article.AsNew = true;
                else
                    article.AsNew = false;

                reader.Close();

                #endregion

                #region Kategorie & Typ

                command = connection.CreateCommand();
                command.CommandText = "SELECT TOP 1 TRIM(tblBasGroups.grpDesc) " +
                                      "FROM tblBasGroups, tblCurArtikelBeschreibung " +
                                      "WHERE tblBasGroups.grpLevel = 1 " +
                                      "      AND tblCurArtikelBeschreibung.arbArtID =" + article.Id + " " +
                                      "      AND tblCurArtikelBeschreibung.arbGrpID = tblBasGroups.grpID";

                reader = command.ExecuteReader();
                if (reader.Read())
                {
                    string categoryName = (string)reader[0];

                    if (categoryName == "Sonstige")
                    {
                        reader.Close();

                        command = connection.CreateCommand();
                        command.CommandText = "SELECT TOP 1 TRIM(tblBasGroups.grpDesc), tblBasGroups.grpID " +
                                              "FROM tblBasGroups, tblCurArtikelBeschreibung " +
                                              "WHERE tblBasGroups.grpLevel = 2 AND tblBasGroups.grpParent = " + CategoryIdSonstige + " " +
                                              "      AND tblCurArtikelBeschreibung.arbArtID =" + article.Id + " " +
                                              "      AND tblCurArtikelBeschreibung.arbGrpID = tblBasGroups.grpID";

                        reader = command.ExecuteReader();

                        if (reader.Read())
                        {

                            int parentGrpId = (int)reader[1];
                            string genderName = (string)reader[0];

                            if (GenderConversion.TryGetValue(genderName, out string newGenderName) && Genders.Where(g => g.Name == newGenderName).FirstOrDefault() is Gender gender)
                                article.Gender = gender;

                            reader.Close();

                            command = connection.CreateCommand();
                            command.CommandText = "SELECT TOP 1 TRIM(type.grpDesc) " +
                                                  "FROM tblBasGroups type, tblBasGroups category, tblCurArtikelBeschreibung " +
                                                  "WHERE category.grpLevel = 3 AND category.grpDesc = '" + categoryName + "' AND category.grpParent = " + parentGrpId + " " +
                                                  "      AND type.grpLevel = 4 AND type.grpParent = category.grpID " +
                                                  "      AND tblCurArtikelBeschreibung.arbArtID =" + article.Id + " " +
                                                  "      AND tblCurArtikelBeschreibung.arbGrpID = type.grpID";

                            reader = command.ExecuteReader();

                            string typeName = "";

                            if (reader.Read())
                            {
                                typeName = (string)reader[0];
                            }

                            if (CategoryTypeConversion.TryGetValue((categoryName, typeName), out (string, string) categoryType))
                            {
                                if (categoryType.Item1 != "" && Categories.Where(c => c.Name == categoryType.Item1).FirstOrDefault() is Category category)
                                {
                                    article.Category = category;

                                    if (categoryType.Item2 != "" && category.SubCategories != null && category.SubCategories.Where(t => t.Name == categoryType.Item2).FirstOrDefault() is SubCategory type)
                                    {
                                        article.SubCategory = type;
                                    }
                                }
                            }
                        }
                    }
                    else if (categoryName == "Accessoires")
                    {
                        reader.Close();

                        command = connection.CreateCommand();
                        command.CommandText = "SELECT TOP 1 TRIM(category.grpDesc), category.grpID " +
                                              "FROM tblBasGroups category, tblCurArtikelBeschreibung " +
                                              "WHERE category.grpLevel = 2 AND category.grpParent = " + CategoryIdAccessoires + " " +
                                              "      AND tblCurArtikelBeschreibung.arbArtID = " + article.Id + " " +
                                              "      AND tblCurArtikelBeschreibung.arbGrpID = category.grpID";

                        reader = command.ExecuteReader();

                        if (reader.Read())
                        {
                            categoryName = (string)reader[0];
                            int categoryId = (int)reader[1];

                            reader.Close();

                            command = connection.CreateCommand();
                            command.CommandText = "SELECT TOP 1 TRIM(type.grpDesc), type.grpID " +
                                                  "FROM tblBasGroups type, tblCurArtikelBeschreibung " +
                                                  "WHERE type.grpLevel = 3 AND type.grpParent = " + categoryId + " " +
                                                  "      AND tblCurArtikelBeschreibung.arbArtID = " + article.Id + " " +
                                                  "      AND tblCurArtikelBeschreibung.arbGrpID = type.grpID " +
                                                  "      AND NOT type.grpDesc = 'Marke' AND NOT type.grpDesc = 'Zustand' " +
                                                  "      AND NOT type.grpDesc = 'Zubehör' AND NOT type.grpDesc = 'Discounter' " +
                                                  "      AND NOT type.grpDesc = 'Farbe' AND NOT type.grpDesc = 'für' " +
                                                  "      AND NOT type.grpDesc = 'Material' AND NOT type.grpDesc = 'Größe' " +
                                                  "      AND NOT type.grpDesc = 'zzForm' AND NOT type.grpDesc = 'zzSaison' " +
                                                  "      AND NOT type.grpDesc = 'Länge' AND NOT type.grpDesc = 'Anzahl'";

                            reader = command.ExecuteReader();

                            string typeName = "";

                            if (reader.Read())
                            {
                                typeName = (string)reader[0];

                                if (typeName == "Art")
                                {
                                    int artGrpID = (int)reader[1];

                                    reader.Close();

                                    command = connection.CreateCommand();
                                    command.CommandText = "SELECT TOP 1 TRIM(type.grpDesc) " +
                                                          "FROM tblBasGroups type, tblCurArtikelBeschreibung " +
                                                          "WHERE type.grpLevel = 4 AND type.grpParent = " + artGrpID + " " +
                                                          "      AND tblCurArtikelBeschreibung.arbArtID = " + article.Id + " " +
                                                          "      AND tblCurArtikelBeschreibung.arbGrpID = type.grpID";

                                    reader = command.ExecuteReader();

                                    typeName = (string)reader[0];
                                }
                            }
                            else
                            {
                                reader.Close();

                                command = connection.CreateCommand();
                                command.CommandText = "SELECT TOP 1 TRIM(type.grpDesc) " +
                                                      "FROM tblBasGroups art, tblBasGroups type, tblCurArtikelBeschreibung " +
                                                      "WHERE art.grpLevel = 3 AND art.grpDesc = 'Art' and art.grpParent = " + categoryId + " " +
                                                      "      AND type.grpLevel = 4 AND type.grpParent = art.grpID " +
                                                      "      AND tblCurArtikelBeschreibung.arbArtID = " + article.Id + " " +
                                                      "      AND tblCurArtikelBeschreibung.arbGrpID = type.grpID";

                                reader = command.ExecuteReader();

                                if (reader.Read())
                                {
                                    typeName = (string)reader[0];
                                }
                            }

                            //Neue Kategorie und Art zuordnen
                            if (CategoryTypeConversion.TryGetValue((categoryName, typeName), out (string, string) categoryType))
                            {
                                if (categoryType.Item1 != "" && Categories.Where(c => c.Name == categoryType.Item1).FirstOrDefault() is Category category)
                                {
                                    article.Category = category;

                                    if (categoryType.Item2 != "" && category.SubCategories != null && category.SubCategories.Where(t => t.Name == categoryType.Item2).FirstOrDefault() is SubCategory type)
                                    {
                                        article.SubCategory = type;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        string genderName = null;
                        if (categoryName == "Herren Bekleidung")
                        {
                            genderName = "Herren";
                        }
                        else if (categoryName == "Damen Bekleidung")
                        {
                            genderName = "Damen";
                        }
                        else if (categoryName == "Kinder Bekleidung")
                        {
                            genderName = "Kinder";
                        }

                        if (genderName != null && article.Gender == null && GenderConversion.TryGetValue(genderName, out string newGenderName))
                            article.Gender = Genders.Where(g => g.Name == newGenderName).First();

                        reader.Close();

                        command = connection.CreateCommand();
                        command.CommandText = "SELECT TOP 1 TRIM(types.grpDesc) " +
                                              "FROM tblBasGroups types, tblBasGroups categories, tblCurArtikelBeschreibung " +
                                              "WHERE categories.grpLevel = 1 AND categories.grpDesc = '" + categoryName + "'" +
                                              "      AND types.grpLevel = 2 AND types.grpParent = categories.grpID " +
                                              "      AND tblCurArtikelBeschreibung.arbArtID = " + article.Id + " " +
                                              "      AND tblCurArtikelBeschreibung.arbGrpID = types.grpID";

                        reader = command.ExecuteReader();

                        string typeName = "";

                        if (reader.Read())
                        {
                            typeName = (string)reader[0];
                        }

                        //Neue Kategorie und Art zuordnen
                        if (CategoryTypeConversion.TryGetValue((categoryName, typeName), out (string, string) categoryType))
                        {
                            if (categoryType.Item1 != "" && Categories.Where(c => c.Name == categoryType.Item1).FirstOrDefault() is Category category)
                            {
                                article.Category = category;

                                if (categoryType.Item2 != "" && category.SubCategories != null && category.SubCategories.Where(t => t.Name == categoryType.Item2).FirstOrDefault() is SubCategory type)
                                {
                                    article.SubCategory = type;
                                }
                            }
                        }
                    }
                }

                reader.Close();

                #endregion
            }
        }

        private void ImportArticles()
        {
            Articles = new List<Article>();
            List<Task> tasks = new List<Task>();

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "SELECT artPartnerID, artEigenanteil, artAbholdatum, artVKInklMWSt, artStatus, artInfo, artErfassungsdatum, artID, artDatumAuszahlung, artDatumRueckgabe, artDatumGeaendert, artCalcLastLogEntry " +
                                      "FROM tblCurArtikel articles " +
                                      "WHERE articles.artID > 100000 " +
                                      "ORDER BY artID";

                var reader = command.ExecuteReader();

                int articleId = 100001;

                Console.WriteLine("MissingArticles:");

                while (reader.Read())
                {
                    Article article = new Article
                    {
                        Id = (int)reader["artID"]
                    };

                    if (article.Id > articleId)
                    {
                        for (int i = 0; i < article.Id - articleId; i++)
                        {
                            Articles.Add(new Article
                            {
                                Id = articleId + i,
                                AddedToSortiment = DateTime.Now,
                                AsNew = false,
                                EnteredFinalState = DateTime.Now,
                                Status = Status.ClosedOut
                            });

                            Console.WriteLine(articleId + i);
                        }
                    }

                    article.Notes = (string)reader["artInfo"];

                    if (UseConversionTables)
                        tasks.Add(Task.Run(() => ImportArticleAttributesWithConversion(article)));
                    else
                        tasks.Add(Task.Run(() => ImportArticleAttributes(article)));

                    int supplierId = (int)reader["artPartnerID"];
                    article.Supplier = Suppliers.Where(s => s.Id == supplierId).First();

                    var price = (decimal)reader["artVKInklMWSt"];
                    decimal realShopProportion = (decimal)(float)reader["artEigenanteil"];
                    var supplierProportion = Math.Round((realShopProportion - 1) * article.Price / (-1 - (decimal)0.19 * realShopProportion), 2);
                    article.SetPriceAndPayoutSkipChecks(price, supplierProportion);
                    if (reader["artAbholdatum"] is DateTime pickUp)
                        article.PickUp = pickUp;
                    article.AddedToSortiment = ((DateTime)reader["artErfassungsdatum"]);

                    int statusId = (short)reader["artStatus"];

                    if (statusId == 1 || statusId == 2 || statusId == 5)
                        article.Status = Status.Sortiment;
                    else if (statusId == 3 || statusId == 4)
                        article.Status = Status.Reserved;
                    else if (statusId == 6 || statusId == 10 || statusId == 11 || statusId == 99)
                        article.Status = Status.ClosedOut;
                    else if (statusId == 7)
                        article.Status = Status.Sold;
                    else if (statusId == 8)
                        article.Status = Status.PayedOut;
                    else if (statusId == 9)
                        article.Status = Status.Returned;

                    if (article.Status == Status.PayedOut)
                        article.EnteredFinalState = (DateTime)reader["artDatumAuszahlung"];
                    else if (article.Status == Status.Returned)
                        article.EnteredFinalState = (DateTime)reader["artDatumRueckgabe"];
                    else if (article.Status == Status.ClosedOut)
                    {
                        try
                        {
                            article.EnteredFinalState = (DateTime?)reader["artCalcLastLogEntry"];
                        }
                        catch { }
                    }

                    Articles.Add(article);
                    articleId++;
                }

                Task.WaitAll(tasks.ToArray());
            }
        }

        private void ImportSupplierProportionTable()
        {
            if (MainDb == null)
                MainDb = Data.CreateDbConnection();

            GraduationSupplierProportions = new List<SupplierProportion>();

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "SELECT * " +
                                      "FROM tblBasProvisionen " +
                                      "ORDER BY tblBasProvisionen.prvMin";

                var graduationSupplierProportionReader = command.ExecuteReader();

                while (graduationSupplierProportionReader.Read())
                {
                    GraduationSupplierProportions.Add(new SupplierProportion
                    {
                        FromPrice = (decimal)graduationSupplierProportionReader["prvMin"],
                        Proportion = (1 - (decimal)(double)graduationSupplierProportionReader["prvEigenanteil"]) * 100
                    });
                }
            }
        }

        public void ImportData(bool useAttributeTable)
        {
            UseConversionTables = useAttributeTable;

            Task[] tasks = new Task[]
            {
                Task.Run(() => ImportAttributes()),
                Task.Run(() => ImportSupplierProportionTable()),
                Task.Run(() => ImportSuppliers())
            };

            Task.WaitAll(tasks);

            MainDb.Genders.AddRange(Genders);
            MainDb.Categories.AddRange(Categories);
            MainDb.Brands.AddRange(Brands);
            MainDb.Sizes.AddRange(Sizes);
            MainDb.Colors.AddRange(Colors);
            MainDb.Materials.AddRange(Materials);
            MainDb.Parts.AddRange(Parts);
            MainDb.Defects.AddRange(Defects);

            MainDb.SupplierProportions.AddRange(GraduationSupplierProportions);

            MainDb.Suppliers.AddRange(Suppliers);

            MainDb.SaveChanges();

            Task.Run(() => ImportArticles()).Wait();

            MainDb.Articles.AddRange(Articles);

            MainDb.SaveChanges();
        }

        private class Attribute
        {
            public string NeuesAttribut { get; set; }
            public string AltesAttribut { get; set; }
        }

        private class CategoryType
        {
            public string NeueKategorie { get; set; } = "";
            public string AlteKategorie { get; set; } = "";
            public string NeueArt { get; set; } = "";
            public string AlteArt { get; set; } = "";
        }

        public void CreateAttributeCSVs()
        {
            Directory.CreateDirectory(ConversionFolder);

            ImportAttributes();

            #region Gender

            List<Attribute> gendersToConvert = new List<Attribute>()
            {
                new Attribute()
            };

            foreach (var gender in Genders)
            {
                gendersToConvert.Add(new Attribute
                {
                    NeuesAttribut = gender.Name,
                    AltesAttribut = gender.Name
                });
            }

            using (var writer = new StreamWriter(ConversionFolder + "\\Geschlecht.csv", false, Encoding.UTF8))
            using (var csvWriter = new CsvWriter(writer, new CsvConfiguration(CultureInfo.CurrentCulture) { Encoding = Encoding.UTF8 }))
            {
                csvWriter.WriteRecords(gendersToConvert);

                writer.Flush();
            }

            #endregion

            #region Kategorie & Typ

            List<CategoryType> categoriesAndTypesToConvert = new List<CategoryType>()
            {
                new CategoryType()
            };

            foreach (var category in Categories)
            {
                categoriesAndTypesToConvert.Add(new CategoryType
                {
                    AlteKategorie = category.Name,
                    NeueKategorie = category.Name
                });

                if (category.SubCategories != null)
                {
                    foreach (var type in category.SubCategories)
                    {
                        categoriesAndTypesToConvert.Add(new CategoryType
                        {
                            AlteArt = type.Name,
                            AlteKategorie = category.Name,
                            NeueArt = type.Name,
                            NeueKategorie = category.Name
                        });
                    }
                }
            }

            using (var writer = new StreamWriter(ConversionFolder + "\\KategorienUndArten.csv", false, Encoding.UTF8))
            using (var csvWriter = new CsvWriter(writer, new CsvConfiguration(CultureInfo.CurrentCulture) { Encoding = Encoding.UTF8 }))
            {
                csvWriter.WriteRecords(categoriesAndTypesToConvert);

                writer.Flush();
            }

            #endregion

            #region Brand

            List<Attribute> brandsToConvert = new List<Attribute>()
            {
                new Attribute()
            };

            foreach (var brand in Brands)
            {
                brandsToConvert.Add(new Attribute
                {
                    NeuesAttribut = brand.Name,
                    AltesAttribut = brand.Name
                });
            }

            using (var writer = new StreamWriter(ConversionFolder + "\\Marken.csv", false, Encoding.UTF8))
            using (var csvWriter = new CsvWriter(writer, new CsvConfiguration(CultureInfo.CurrentCulture) { Encoding = Encoding.UTF8 }))
            {
                csvWriter.WriteRecords(brandsToConvert);

                writer.Flush();
            }

            #endregion

            #region Size

            List<Attribute> sizesToConvert = new List<Attribute>()
            {
                new Attribute()
            };

            foreach (var size in Sizes)
            {
                sizesToConvert.Add(new Attribute
                {
                    NeuesAttribut = size.Name,
                    AltesAttribut = size.Name
                });
            }

            using (var writer = new StreamWriter(ConversionFolder + "\\Größen.csv", false, Encoding.UTF8))
            using (var csvWriter = new CsvWriter(writer, new CsvConfiguration(CultureInfo.CurrentCulture) { Encoding = Encoding.UTF8 }))
            {
                csvWriter.WriteRecords(sizesToConvert);

                writer.Flush();
            }

            #endregion

            #region Color

            List<Attribute> colorsToConvert = new List<Attribute>()
            {
                new Attribute()
            };

            foreach (var color in Colors)
            {
                colorsToConvert.Add(new Attribute
                {
                    NeuesAttribut = color.Name,
                    AltesAttribut = color.Name
                });
            }

            using (var writer = new StreamWriter(ConversionFolder + "\\Farben.csv", false, Encoding.UTF8))
            using (var csvWriter = new CsvWriter(writer, new CsvConfiguration(CultureInfo.CurrentCulture) { Encoding = Encoding.UTF8 }))
            {
                csvWriter.WriteRecords(colorsToConvert);

                writer.Flush();
            }

            #endregion

            #region Material

            List<Attribute> materialsToConvert = new List<Attribute>()
            {
                new Attribute()
            };

            foreach (var material in Materials)
            {
                materialsToConvert.Add(new Attribute
                {
                    NeuesAttribut = material.Name,
                    AltesAttribut = material.Name
                });
            }

            using (var writer = new StreamWriter(ConversionFolder + "\\Materialien.csv", false, Encoding.UTF8))
            using (var csvWriter = new CsvWriter(writer, new CsvConfiguration(CultureInfo.CurrentCulture) { Encoding = Encoding.UTF8 }))
            {
                csvWriter.WriteRecords(materialsToConvert);

                writer.Flush();
            }

            #endregion

            #region Teile

            List<Attribute> partsToConvert = new List<Attribute>()
            {
                new Attribute()
            };

            foreach (var part in Parts)
            {
                partsToConvert.Add(new Attribute
                {
                    NeuesAttribut = part.Name,
                    AltesAttribut = part.Name
                });
            }

            using (var writer = new StreamWriter(ConversionFolder + "\\Teile.csv", false, Encoding.UTF8))
            using (var csvWriter = new CsvWriter(writer, new CsvConfiguration(CultureInfo.CurrentCulture) { Encoding = Encoding.UTF8 }))
            {
                csvWriter.WriteRecords(partsToConvert);

                writer.Flush();
            }

            #endregion
        }

        public void Dispose()
        {
            if (MainDb != null)
                MainDb.Dispose();

            Suppliers = null;
            Articles = null;
            Genders = null;
            Categories = null;
            Brands = null;
            Sizes = null;
            Colors = null;
            Materials = null;
            Parts = null;
            Defects = null;
            GraduationSupplierProportions = null;

            ParentGroupsGender = null;
            ParentGroupsBrand = null;
            ParentGroupsSize = null;
            ParentGroupsColor = null;
            ParentGroupsMaterial = null;
            ParentGroupsParts = null;

            GenderConversion = null;
            CategoryTypeConversion = null;
            BrandConversion = null;
            SizeConversion = null;
            ColorConversion = null;
            MaterialConversion = null;
            PartsConversion = null;

            genderFromSonstige = null;
        }
    }
}
