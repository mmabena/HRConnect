import React,{useState}from "react";
import { useLocation, useNavigate } from "react-router-dom";
import usePagination from "../hooks/usePagination";
import usePayrollPeriod from "../hooks/usePayroll";


import "../../Components/MenuBar/MenuBar.css";

export const Payslip = () => {
    const tabs=[
      {label:"Personal Information",value:"All"},
      {label:"Payroll Information",value:"Payroll"},
      {label: "Payroll Tools",value:"Tools"}
    ]
    const navigate=useNavigate();
    const location=useLocation();
    const [showModal,setShowModal]=useState(false);
    const [selectedTab,setSelectedTab]=useState("All");

    const {payrollPeriods,loading,error}=usePayrollPeriod(location.key);
     //Pagination  
    const{
        currentPage,
        totalPages,
        // currentItems,
        setCurrentPage,
        handlePrev,
        handleNext,
        handlePageClick,
        // changeItemsPerPage,
    } = usePagination();

   const getHeaderTitle=()=>{
    //    switch(activeTab){
    //     case "Deductions":
    //         return "Deductions Management";
    //     default:
    //         return activeTab;
    //    } 
    // };


    //use this to view a payroll
    const handleViewPayrollClick=()=>{
        setShowModal(true);
    }
    const handleItemsPerPageChange=(option)=>{
        setItemsPerPage(option);
        setCurrentPage(1);
    }
    return(
      <div className="menu-background">
        <div className="menu-bar"></div>

        <div className="wrapper-container">
         <div className="payroll-management-heading-container">
          Payroll Information 
         </div> 
        </div>
        </div>

        <div className="employee-list-heading-row">
          {tabs.map((tab) => (
            <div
              key={tab.value}
              className={`heading-item ${
                selectedTab === tab.value ? "selected" : ""
              }`}
              onClick={() => {
                setSelectedTab(tab.value); 
                setActivePage(1); 
              }} 
            >
              {tab.label}
            </div>
          ))}

        </div>
    );
};
export default Payslip
