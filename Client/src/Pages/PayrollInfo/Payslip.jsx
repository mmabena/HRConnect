import React, { useState } from "react";
import { useLocation, useNavigate } from "react-router-dom";
import usePagination from "../../hooks/usePagination.js";
import usePayrollPeriod from "../../hooks/usePayroll.js";
import "./Payslip.css"
import SummaryBox from "../PayrollInfo/SummaryBox.jsx"
// import "../../Cmponents/MenuBar/MenuBar.css";

const tabs = [
  { label: "Personal Information", value: "Personal" },
  { label: "Payroll Information", value: "Payroll" },
  { label: "Leave", value: "Leave" },
  { label: "Payroll Tools", value: "Tools" }
];

const Payslip = () => {

  const navigate = useNavigate();
  const location = useLocation();
  const [showModal, setShowModal] = useState(false);
  const [selectedTab, setSelectedTab] = useState("Payroll");

  const { payrollPeriods, loading, error } = usePayrollPeriod(location.key);

  //Pagination  
  const {
    currentPage,
    //itemsPerPage,
    totalPages,
    // currentItems,
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
        {/* Payslip Summary Tables*/}
        <div className="payslip-summary-container">
          <div class="payslip-summary-frame">
            <SummaryBox
              title="Gross Earnings"
              amount={48026}
              subtext="Before Deductions" />
            <SummaryBox
              title="Deductions"
              amount={`-${12837}`}
              subtext="Deductions debiting" />
            <SummaryBox
              title="Net Pay"
              amount={35189}
              subtext="Deposited to account" />
          </div>
        </div>
        <div className="content-container">
          <div className="employee-table-grid">
            <div className="table-header">Employee Code</div>
            <div className="table-header">Name & Surname</div>
            <div className="table-header">Job Title</div>
            <div className="table-header">Contact Number</div>
            <div className="table-header">Email</div>
            <div className="table-header">Employment Status</div>
            <div className="table-header">Branch</div>
            <div className="table-header-action">Action</div>

            {loading && <div className="loading-row">Loading payslips...</div>}

            {error && <div className="error-row">{error}</div>}

            {!loading && !error && currentItems.length === 0 && (
              <div className="no-data-row">No payslips found.</div>
            )}

            {!loading &&
              !error &&
              currentItems.map((emp, index) => (
                <React.Fragment key={emp.employeeId}>
                  <div className="table-cell">{emp.employeeId}</div>

                  <div className="table-cell name-surname-cell">
                    <div
                      className={`initials-circle`}
                    >
                      {(
                        emp.initials ||
                        `${(emp.name || "").charAt(0)}${(
                          emp.surname || ""
                        ).charAt(0)}`
                      ).toUpperCase()}
                    </div>

                    <span className="name-text">{`${emp.name} ${emp.surname}`}</span>
                  </div>

                  <div className="table-cell">{emp.positionTitle}</div>
                  <div className="table-cell">{emp.contactNumber}</div>
                  <div className="table-cell">{emp.email}</div>
                  <div className="table-cell">{emp.employmentStatus}</div>
                  <div className="table-cell">{emp.branch}</div>

                </React.Fragment>
              ))}
          </div>
        </div>
      </div>

    </div>

  );
};
export default Payslip
