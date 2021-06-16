package kaufi.jonas.wwsmobile.data.db

import android.content.Context
import android.content.SharedPreferences
import androidx.preference.PreferenceManager
import kaufi.jonas.wwsmobile.model.Supplier
import java.lang.IllegalArgumentException
import java.sql.Connection
import java.sql.DriverManager
import java.sql.ResultSet


class MainDatabase(private val context: Context) {

    fun createConnection(): Connection {
        val prefs: SharedPreferences = PreferenceManager.getDefaultSharedPreferences(context)

        val host = prefs.getString("db_host", "")
        val dbName = prefs.getString("db_dbname", "")
        val user = prefs.getString("db_user", "")
        val password = prefs.getString("db_password", "")

        val connectionUrl = ("jdbc:jtds:sqlserver://$host/$dbName;"
                + "user=$user;"
                + "password=$password;"
                + "ssl=required;")

        return DriverManager.getConnection(connectionUrl)
    }

    /**
     * @param resultsLimit 0 for umlimited
     */
    suspend fun allSuppliers(resultsLimit: Int, filterBy: String?): List<Supplier> {
        if (resultsLimit < 0)
            throw IllegalArgumentException("maxNumberResults must not be less than 0")

        val connection = createConnection()
        val statement = connection.createStatement()

        val columns = "Id, Name, Place, Street, SupplierProportion, PickUp, Notes"
        val limit = if (resultsLimit == 0) "" else "TOP $resultsLimit"
        var condition = ""

        if (!filterBy.isNullOrBlank()) {
            val id = filterBy.toIntOrNull()
            condition = if (id != null) "WHERE Id = $id" else "WHERE Name LIKE '%$filterBy%'"
        }

        val results = statement.executeQuery("SELECT $limit $columns FROM Suppliers $condition")



        connection.close()
        return null
    }

    fun resultsToSuppliers(results: ResultSet): List<Supplier> {
        val suppliers = ArrayList<Supplier>(results.fetchSize)

        while (results.next()) {
            val id = results.getInt("Id")
            val name = results.getString("Name")
            val place = results.getString("Place")
            val street = results.getString("Street")
            val supplierProportion = results.getBigDecimal("SupplierProportion")
            val pickUp = results.getInt("PickUp")
            val notes = results.getString("Notes")

            val supplier = Supplier(id, name, place, street, supplierProportion, pickUp, notes)
            suppliers.add(supplier)
        }

        return suppliers
    }
}