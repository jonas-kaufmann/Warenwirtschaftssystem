package kaufi.jonas.wwsmobile.ui.picksupplier

import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import android.widget.TextView
import androidx.recyclerview.widget.RecyclerView
import androidx.recyclerview.widget.RecyclerView.Adapter
import kaufi.jonas.wwsmobile.R
import kaufi.jonas.wwsmobile.model.Supplier

class SupplierAdapter : Adapter<SupplierAdapter.SupplierViewHolder>() {

    var suppliers: List<Supplier>? = null
        set(value) {
            field = value
            notifyDataSetChanged()
        }

    class SupplierViewHolder(itemView: View) : RecyclerView.ViewHolder(itemView) {
        val itemHeader: TextView = itemView.findViewById(R.id.supplier_row_item_header)
        val itemDetails: TextView = itemView.findViewById(R.id.supplier_row_item_details)

    }

    override fun onCreateViewHolder(parent: ViewGroup, viewType: Int): SupplierViewHolder {
        val view = LayoutInflater.from(parent.context).inflate(
            R.layout.supplier_row_item,
            parent,
            false
        )
        return SupplierViewHolder(view)
    }

    override fun onBindViewHolder(holder: SupplierViewHolder, position: Int) {
            holder.itemHeader.text = suppliers!![position].toString()
            holder.itemDetails.text = suppliers!![position].detailsToString()
    }

    override fun getItemCount(): Int {
        return if(suppliers == null) 0 else suppliers!!.size
    }

}