import React,{useState,useEffect}from 'react';
import { SlidersHorizontal } from "lucide-react"
import "./FilterTable.css"

const FilterTable=({data,filterKey,onFilter,isOpen,onClose})=>{
if(!isOpen)return null;
// const [open,setOpen]=useState(false);
const uniqueOpts=[...new Set(data.map(i=>i[filterKey]))]
//Get the  the unique filter options when the data changes


return (
    <>
    <div className="filter-container" 
    // onClick={(e)=>e.stopPropagation()}
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
            // setOpen(false)
            }}>
                Clear Filter
            </div>

        </div>
    </div>
</>
)
}

export default FilterTable;