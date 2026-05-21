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
  const employeesPerPage = 6;

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

  const totalPages = Math.ceil(activeEmployees.length / employeesPerPage);

  const startIndex = (currentPage - 1) * employeesPerPage;

  const endIndex = startIndex + employeesPerPage;

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
  const goToPage = (page) => {
    setCurrentPage(page);
  };

  const goToPreviousPage = () => {
    if (currentPage > 1) {
      setCurrentPage((prev) => prev - 1);
    }
  };

  const goToNextPage = () => {
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
            <div className="pagination">
              <button onClick={goToPreviousPage} disabled={currentPage === 1}>
                {"<"}
              </button>

              {[...Array(totalPages)].map((_, index) => {
                const pageNumber = index + 1;

                return (
                  <button
                    key={pageNumber}
                    className={currentPage === pageNumber ? "active" : ""}
                    onClick={() => goToPage(pageNumber)}
                  >
                    {pageNumber}
                  </button>
                );
              })}

              <button
                onClick={goToNextPage}
                disabled={currentPage === totalPages || totalPages === 0}
              >
                {">"}
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default AffectedEmployeesPage;
