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

  useEffect(() => {
    if (location.state?.employees) {
      setEmployees(location.state.employees);
    }
  }, [location.state]);

  const groupedEmployees = {
    GROUP_A: employees.filter((x) => x.groupKey === "GROUP_A"),
    SENIOR: employees.filter((x) => x.groupKey === "SENIOR"),
    EXECUTIVE: employees.filter((x) => x.groupKey === "EXECUTIVE"),
  };

  const [activeTab, setActiveTab] = useState(() => {
    if (groupedEmployees.GROUP_A.length > 0) return "GROUP_A";
    if (groupedEmployees.SENIOR.length > 0) return "SENIOR";
    return "EXECUTIVE";
  });

  const currentEmployees = groupedEmployees[activeTab] || [];

  const getTabLabel = (key) => {
    switch (key) {
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

            <button
              className="confirm-impact-btn"
              onClick={handleConfirmChanges}
            >
              Confirm Changes
            </button>
          </div>

          <div className="impact-table-wrapper">
            <div className="impact-table-title">Employee Impact Table</div>

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
          </div>
        </div>
      </div>
    </div>
  );
};

export default AffectedEmployeesPage;
