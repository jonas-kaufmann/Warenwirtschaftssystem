package kaufi.jonas.wwsmobile.ui.picksupplier

import android.os.Bundle
import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import androidx.appcompat.widget.SearchView
import androidx.fragment.app.Fragment
import androidx.lifecycle.lifecycleScope
import androidx.recyclerview.widget.RecyclerView
import kaufi.jonas.wwsmobile.R
import kaufi.jonas.wwsmobile.data.db.MainDatabase
import kaufi.jonas.wwsmobile.model.Supplier
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.Job
import kotlinx.coroutines.launch


class PickSupplierFragment : Fragment() {

    var job: Job? = null

    override fun onCreateView(
        inflater: LayoutInflater, container: ViewGroup?,
        savedInstanceState: Bundle?
    ): View? {

        val view = inflater.inflate(R.layout.fragment_pick_supplier, container, false)

        val suppliersRecyclerView = view.findViewById<RecyclerView>(R.id.suppliersRecyclerView)
        val supplierAdapter = SupplierAdapter()
        suppliersRecyclerView.adapter = supplierAdapter

        val searchView = view.findViewById<SearchView>(R.id.pick_supplier_searchView)
        searchView.setOnQueryTextListener(object : SearchView.OnQueryTextListener {

            override fun onQueryTextSubmit(query: String?): Boolean {
                applyQuery(supplierAdapter, query)
                return true
            }

            override fun onQueryTextChange(newText: String?): Boolean {
                applyQuery(supplierAdapter, newText)
                return true
            }

        })

        searchView.isIconified = false
        searchView.requestFocus()

        applyQuery(supplierAdapter, "")

        return view
    }

    fun applyQuery(supplierAdapter: SupplierAdapter, query: String?) {
        if (job != null && job!!.isActive)
            job!!.cancel()

        job = lifecycleScope.launch {
            var suppliers: List<Supplier>? = null

            val databaseJob = lifecycleScope.launch(Dispatchers.IO) {
                suppliers = MainDatabase(requireContext()).allSuppliers(20, query)
            }

            databaseJob.join()
            supplierAdapter.suppliers = suppliers
        }
    }
}

