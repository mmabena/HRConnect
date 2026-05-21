import React, { useEffect, useState } from "react";
import { useLocation, useNavigate } from "react-router-dom";
import "../../Components/MenuBar/MenuBar.css";
import NavBar from "../NavBar";
import "./affected-employees-page.css";
import { updateLeaveType } from "../../api/leaveTypeApi";

const AffectedEmployeesPage = () => {
  const location = useLocation();
  const navigate = useNavigate();
  const payload = location.state?.payload;
  const leaveTypeId = location.state?.leaveTypeId;

  const [employees, setEmployees] = useState([]);
  const [currentPage, setCurrentPage] = useState(1);
  const [itemsPerPage, setItemsPerPage] = useState(6);
  const [showPageOptions, setShowPageOptions] = useState(false);

  const pageOptions = [6, 10, 20, 50];

  useEffect(() => {
    if (location.state?.employees) {
      setEmployees(location.state.employees);
    }
  }, [location.state]);

  const groupedEmployees = {
    ALL: employees,

    GROUP_A: employees.filter((x) => x.groupKey === "GROUP_A"),

    SENIOR: employees.filter((x) => x.groupKey === "SENIOR"),

    EXECUTIVE: employees.filter((x) => x.groupKey === "EXECUTIVE"),
  };

  const [activeTab, setActiveTab] = useState("ALL");

  useEffect(() => {
    setCurrentPage(1);
  }, [activeTab]);

  const activeEmployees = groupedEmployees[activeTab] || [];

  const totalPages = Math.ceil(activeEmployees.length / itemsPerPage);

  const startIndex = (currentPage - 1) * itemsPerPage;

  const endIndex = startIndex + itemsPerPage;

  const currentEmployees = activeEmployees.slice(startIndex, endIndex);

  const getTabLabel = (key) => {
    switch (key) {
      case "ALL":
        return "All Employees";
      case "GROUP_A":
        return "Unskilled - Middle";
      case "SENIOR":
        return "Senior";
      case "EXECUTIVE":
        return "Executive";
      default:
        return key;
    }
  };
  const handleConfirmChanges = async () => {
    try {
      await updateLeaveType(leaveTypeId, payload);

      navigate("/leaveManagement");
    } catch (err) {
      console.error(err);
    }
  };
  const handlePageClick = (page) => {
    setCurrentPage(page);
  };

  const handlePrev = () => {
    if (currentPage > 1) {
      setCurrentPage((prev) => prev - 1);
    }
  };

  const handleNext = () => {
    if (currentPage < totalPages) {
      setCurrentPage((prev) => prev + 1);
    }
  };
  return (
    <div className="menu-background custom-scrollbar">
      <div className="lt-page">
        <div className="wrap-container">
          <div className="heading-container">Company Management</div>
        </div>

        <div className="navbar-with-button">
          <NavBar />
        </div>

        <div className="lt-page-container">
          <div className="impact-top-section">
            <div className="impact-tabs">
              <button
                className={activeTab === "ALL" ? "active" : ""}
                onClick={() => setActiveTab("ALL")}
              >
                All ({employees.length})
              </button>
              {groupedEmployees.GROUP_A.length > 0 && (
                <button
                  className={activeTab === "GROUP_A" ? "active" : ""}
                  onClick={() => setActiveTab("GROUP_A")}
                >
                  Unskilled - Middle ({groupedEmployees.GROUP_A.length})
                </button>
              )}

              {groupedEmployees.SENIOR.length > 0 && (
                <button
                  className={activeTab === "SENIOR" ? "active" : ""}
                  onClick={() => setActiveTab("SENIOR")}
                >
                  Senior ({groupedEmployees.SENIOR.length})
                </button>
              )}

              {groupedEmployees.EXECUTIVE.length > 0 && (
                <button
                  className={activeTab === "EXECUTIVE" ? "active" : ""}
                  onClick={() => setActiveTab("EXECUTIVE")}
                >
                  Executive ({groupedEmployees.EXECUTIVE.length})
                </button>
              )}
            </div>
            <div className="impact-action-buttons">
              <button
                className="back-btn2"
                onClick={() => navigate("/leaveManagement")}
              >
                Back
              </button>

              <button
                className="confirm-impact-btn"
                onClick={handleConfirmChanges}
              >
                Confirm Changes
              </button>
            </div>
          </div>

          <div className="impact-table-wrapper">
            <div className="impact-table-title">
              {getTabLabel(activeTab)} Impact Table
            </div>

            <table className="impact-table">
              <thead>
                <tr>
                  <th>Employee</th>
                  <th>Position</th>
                  <th>Years Of Service</th>
                  <th>Previous Entitlement</th>
                  <th>Current Entitlement</th>
                </tr>
              </thead>

              <tbody>
                {currentEmployees.map((employee) => (
                  <tr key={employee.employeeId}>
                    <td>{employee.employeeName}</td>

                    <td>{employee.position}</td>

                    <td>{employee.yearsOfService}yrs</td>

                    <td>{employee.previousEntitlement} Days</td>

                    <td>{employee.newEntitlement} Days</td>
                  </tr>
                ))}
              </tbody>
            </table>
            <div className="impact-pagination-shell">
              <div className="impact-pagination-left-section">
                <span className="impact-pagination-range-text">
                  <strong className="impact-pagination-range-bold">
                    {activeEmployees.length === 0 ? 0 : startIndex + 1} -{" "}
                    {Math.min(endIndex, activeEmployees.length)}
                  </strong>{" "}
                  of {activeEmployees.length}
                </span>

                <div
                  className="impact-pagination-page-size-box"
                  onClick={() => setShowPageOptions(!showPageOptions)}
                >
                  <span className="impact-pagination-page-size-number">
                    {itemsPerPage}
                  </span>

                  <img
                    src="/images/arrow_drop_down_circle.png"
                    alt="Dropdown"
                    className="impact-pagination-dropdown-icon"
                  />

                  {showPageOptions && (
                    <ul className="impact-pagination-dropdown-menu">
                      {pageOptions.map((option) => (
                        <li
                          key={option}
                          className="impact-pagination-dropdown-item"
                          onClick={(e) => {
                            e.stopPropagation();

                            setItemsPerPage(option);
                            setShowPageOptions(false);
                            setCurrentPage(1);
                          }}
                        >
                          {option}
                        </li>
                      ))}
                    </ul>
                  )}
                </div>

                <span className="impact-pagination-page-size-label">
                  Per page
                </span>
              </div>

              <div className="impact-pagination-right-section">
                <img
                  src="/images/arrow_drop_down_circle.png"
                  alt="Previous"
                  className={`impact-pagination-arrow-button impact-pagination-arrow-prev ${
                    currentPage === 1 ? "impact-pagination-arrow-disabled" : ""
                  }`}
                  onClick={handlePrev}
                />

                <div className="impact-pagination-page-number-group">
                  {Array.from({ length: totalPages }, (_, i) => (
                    <button
                      key={i + 1}
                      className={`impact-pagination-page-button ${
                        currentPage === i + 1
                          ? "impact-pagination-page-button-active"
                          : ""
                      }`}
                      onClick={() => handlePageClick(i + 1)}
                    >
                      {i + 1}
                    </button>
                  ))}
                </div>

                <img
                  src="/images/arrow_drop_down_circle.png"
                  alt="Next"
                  className={`impact-pagination-arrow-button impact-pagination-arrow-next ${
                    currentPage === totalPages || totalPages === 0
                      ? "impact-pagination-arrow-disabled"
                      : ""
                  }`}
                  onClick={handleNext}
                />

                <div className="impact-pagination-total-info">
                  {activeEmployees.length} Affected Employee
                  {activeEmployees.length !== 1 ? "s" : ""}
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default AffectedEmployeesPage;
