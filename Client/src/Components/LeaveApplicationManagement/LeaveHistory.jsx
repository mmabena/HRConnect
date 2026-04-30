import React, { useEffect, useState } from "react";
import { getLeaveHistory } from "../../api/leaveApplicationApi";
import "./LeaveHistory.css";
import { Dot } from "lucide-react";
import ApplyLeave from "./ApplyLeave";

const LeaveHistory = () => {
  const [data, setData] = useState([]);
  const [showApply, setShowApply] = useState(false);

  useEffect(() => {
    const fetchData = async () => {
      try {
        const employee = JSON.parse(localStorage.getItem("currentEmployee"));
        const employeeId = employee?.employeeId;

        if (!employeeId) {
          console.error("No employeeId found");
          return;
        }

        const res = await getLeaveHistory(employeeId);

        console.log("API DATA:", res); // DEBUG

        setData(res);
      } catch (error) {
        console.error(error);
      }
    };

    fetchData();
  }, []);

  // ✅ FIX 1: DATE FORMAT
  const formatDate = (date) => {
    return new Date(date).toLocaleDateString("en-GB");
  };

  // ✅ FIX 2: LEAVE TYPE MAPPING
  const mapLeaveType = (code) => {
    switch (code) {
      case "AL":
        return "Annual Leave";
      case "SL":
        return "Sick Leave";
      case "ML":
        return "Maternity Leave";
      case "FRL":
        return "Family Responsibility Leave";
      default:
        return code;
    }
  };

  // ✅ FIX 3: STATUS STYLE
  const getStatusClass = (status) => {
    switch (status) {
      case "Approved":
        return "status approved";
      case "Rejected":
        return "status rejected";
      default:
        return "status pending";
    }
  };
if (showApply) {
  return <ApplyLeave />;
}
  return (
    <div className="leave-page">

      {/* HEADER TITLE */}
      <h1 className="leave-title">Leave Application</h1>

      {/* NAV */}
      <div className="leave-tabs">
        <div className="leave-tab">Personal Information</div>
        <div className="leave-tab">Payroll Information</div>
        <div className="leave-tab active">Leave</div>
        <div className="leave-tab">Payroll Tools</div>
      </div>

      {/* ACTION BUTTONS */}
      <div className="leave-actions">
        <button className="filter-btn">Filter</button>
        <button className="apply-btn" onClick={() => setShowApply(true)}>
          + Apply for leave
        </button>
      </div>

      {/* TABLE CARD */}
      <div className="leave-card">

        <div className="leave-header">Leave History</div>

        <table className="leave-table">
          <thead>
            <tr>
              <th>Leave Type</th>
              <th>Start Date</th>
              <th>End Date</th>
              <th>Leave Entitlement</th>
              <th>Days Requested</th>
              <th>Status</th>
              <th></th>
            </tr>
          </thead>

          <tbody>
            {data.length === 0 ? (
              <tr>
                <td colSpan="7" className="empty">
                  No leave history found
                </td>
              </tr>
            ) : (
              data.map((item) => (
                <tr key={item.id}>
                  <td>{mapLeaveType(item.leaveTypeCode)}</td>

                  <td>{formatDate(item.startDate)}</td>
                  <td>{formatDate(item.endDate)}</td>

                  <td>{item.daysAllocated} Days</td>
                  <td>{item.daysRequested} Days</td>

                 <td>
                  <span className={getStatusClass(item.status)}>
                      <Dot
                        className={`status-dot-icon ${item.status.toLowerCase()}`}
                      />
                      {item.status}
                    </span>
                </td>

                  <td className="view-link">View</td>
                </tr>
              ))
            )}
          </tbody>
        </table>

        {/* PAGINATION */}
        <div className="pagination">
          <button>{"<"}</button>
          <button className="active">1</button>
          <button>2</button>
          <button>{">"}</button>
        </div>

      </div>
    </div>
  );
};

export default LeaveHistory;