import React,{useState,useEffect}from 'react';

const FilterTable=({data,filterKey,onFilter})=>{
const [open,setOpen]=useState(false);
const uniqueOpts=[...new Set(data.map(i=>i[filterKey]))]
//Get the  the unique filter options when the data changes


return (
    <div className="filter-container">
        <button onClick={()=>setOpen(!open)}>
            Filter
        </button>

        {open && (
            <div className="filter-popup">
                {uniqueOpts.map((val,i)=>(
                    <div key={i}
                    className="filter-option"
                    onClick={()=>{
                        onFilter(val)
                        setOpen(false)
                    }}
                    >
                        {val} 
                    </div>
                ))}
            <div className="filter-option-clear"
            onClick={()=>{
                onFilter(null)//reset the filter
            setOpen(false)
            }}>
                Clear Filter
            </div>
            </div>
        )}
    </div>
)
}

export default FilterTable;