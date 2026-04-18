import React, { useState } from "react";
import { useLocation, useNavigate } from "react-router-dom";
import usePagination from "../../hooks/usePagination.js";
import usePayrollPeriod from "../../hooks/usePayroll.js";
import "./Payslip.css"
import SummaryBox from "../PayrollInfo/SummaryBox.jsx"
// import "../../Cmponents/MenuBar/MenuBar.css";

const tabs = [
  //   { label: "Personal Information", value: "Personal" },
  { label: "Payroll Information", value: "Payroll" },
  { label: "Leave", value: "Leave" },
  { label: "Payroll Tools", value: "Tools" }
];

const Payslip = () => {

  const navigate = useNavigate();
  const location = useLocation();
  const [showModal, setShowModal] = useState(false);
  const [selectedTab, setSelectedTab] = useState("Payroll");

  const { payrollPeriod: payrollPeriods, loading, error } = usePayrollPeriod(location.pathname);

  payrollPeriods.map((p) => {
    console.log(`PayrollPeriod:`);
    console.log(p)
  });

  //Pagination  
  const {
    currentPage,
    //itemsPerPage,
    totalPages,
    setCurrentPage,
    handlePrev,
    handleNext,
    handlePageClick,
    currentItems,
    // changeItemsPerPage,

  } = usePagination();


  //use this to view a payroll
  const handleViewPayrollClick = () => {
    setShowModal(true);
  }
  const handleItemsPerPageChange = (option) => {
    // setItemsPerPage(option);
    setCurrentPage(1);
  }
  return (

    <div className="menu-background">
      <div className="menu-bar"></div>

      <div className="wrapper-container">
        <div className="singular-staff-heading-container">Payroll Information</div>

        <div className="list-heading-row">
          {tabs.map((tab) => (
            <div
              key={tab.value}
              className={`heading-item ${selectedTab === tab.value ? "selected" : ""
                }`}
              onClick={() => {
                setSelectedTab(tab.value);
                // setActivePage(1); 
              }}
            >
              {tab.label}
            </div>
          ))}

        </div>

        <div className="filter-header-row">

        </div>
        {/* Payslip Summary Tables*/}
        <div className="payslip-summary-container">
          <div className="payslip-summary-frame" >
            <SummaryBox
              className="gross"
              title="Gross Earnings"
              amount={48026}
              subtext="Before Deductions" />
            <SummaryBox
              className="deductions"
              title="Deductions"
              amount={`-${12837}`}
              subtext="Deductions debiting" />
            <SummaryBox
              className="net"
              title="Net Pay"
              amount={35189}
              subtext="Deposited to account" />
          </div>
        </div>
        <div className="content-container">
          <div className="employee-table-grid">
            <div className="table-header">Period</div>
            <div className="table-header">Gross Earnings</div>
            <div className="table-header">Deductions</div>
            <div className="table-header">Net Pay</div>
            <div className="table-header">Actions</div>

          </div>
          {loading && <div className="loading-row">Loading payslips...</div>}

          {error && <div className="error-row">{error}</div>}

          {!loading && !error && currentItems.length === 0 && (
            <div className="no-data-row">No payslips found.</div>
          )}

          {!loading &&
            !error &&
            currentItems.map((p, index) => (
              <React.Fragment key={p.employeeId}>
                <div className="table-cell">{p.employeeId}</div>

                <div className="table-cell name-surname-cell">
                  <div
                    className={`initials-circle`}
                  >
                    {(
                      p.initials ||
                      `${(p.name || "").charAt(0)}${(
                        p.surname || ""
                      ).charAt(0)}`
                    ).toUpperCase()}
                  </div>

                  <span className="name-text">{`${p.name} ${p.surname}`}</span>
                </div>

              </React.Fragment>
            ))}
        </div>
      </div>

    </div>

  );
};
export default Payslip
