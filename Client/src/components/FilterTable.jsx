import React,{useState,useEffect}from 'react';
import { SlidersHorizontal } from "lucide-react"
import "./FilterTable.css"

const FilterTable=({data,filterKey,onFilter,isOpen,onClose})=>{
if(!isOpen)return null;
const uniqueOpts=[...new Set(data.map(i=>i[filterKey]))]

return (
    <div className="filter-container" 
    >
        <div className="filter-popup">
            - Select Option -
        </div>

            <div className="filter-popup">
                {uniqueOpts.map((val,i)=>(
                    <div key={i}
                    className="filter-option"
                    onClick={()=>{
                        onFilter(val)
                        // setOpen(false)
                    }}
                    >
                        {val} 
                    </div>
                ))}
            <div className="filter-option-clear"
            onClick={()=>{
                onFilter(null)//reset the filter
            }}>
                Clear Filter
            </div>

        </div>
    </div>
)
}

export default FilterTable;