package kaufi.jonas.wwsmobile

import org.junit.Assert
import org.junit.Test
import java.sql.DriverManager

class SimpleDbConnectionTest {

    @Test
    fun tryConnection() {
        val connectionUrl = ("jdbc:jtds:sqlserver://192.168.1.38/WWS;"
                + "user=J;"
                + "password=j;"
                + "ssl=require;")

        val connection = DriverManager.getConnection(connectionUrl)

        val statement = connection.createStatement()
        val sql = "SELECT TOP 100 Name FROM Suppliers"
        val results = statement.executeQuery(sql)

        val suppliers = ArrayList<String>(results.fetchSize)

        while (results.next()) {
            suppliers.add(results.getString("Name"))
        }

        Assert.assertEquals(100, suppliers.size)
    }
}