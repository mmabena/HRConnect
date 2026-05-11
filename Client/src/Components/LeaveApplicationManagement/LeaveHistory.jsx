import React, { useEffect, useState } from "react";
import {
  getLeaveHistory,
  getEmployeeLeave,
} from "../../api/leaveApplicationApi";
import "./LeaveHistory.css";
import { Dot, X, Check } from "lucide-react";
import ApplyLeave from "./ApplyLeave";
import LeaveDetailsModal from "./LeaveDetailsModal";
import {
  startLeaveHubConnection,
  getLeaveHubConnection,
} from "../../signalr/leaveHubConnection";

const LeaveHistory = () => {
  const [data, setData] = useState([]);
  const [showApply, setShowApply] = useState(false);
  const [balances, setBalances] = useState([]);
  const [selectedApplication, setSelectedApplication] = useState(null);

  const fetchData = async () => {
    try {
      const employee = JSON.parse(localStorage.getItem("currentEmployee"));

      const employeeId = employee?.employeeId;

      if (!employeeId) {
        console.error("No employeeId found");
        return;
      }

      const res = await getLeaveHistory(employeeId);
      setData(res);

      const leaveRes = await getEmployeeLeave(employeeId);
      setBalances(leaveRes.leaveBalances);
    } catch (error) {
      console.error(error);
    }
  };
  useEffect(() => {
    fetchData();

    const setupSignalR = async () => {
      const employee = JSON.parse(localStorage.getItem("currentEmployee"));

      const employeeId = employee?.employeeId;

      if (!employeeId) {
        return;
      }

      const connection = await startLeaveHubConnection(employeeId);

      if (!connection) {
        return;
      }

      connection.on("LeaveUpdated", async (message) => {
        console.log("Realtime leave update received:", message);

        await fetchData();
      });
    };

    setupSignalR();

    return () => {
      const connection = getLeaveHubConnection();

      if (connection) {
        connection.off("LeaveUpdated");
      }
    };
  }, []);

  const formatDate = (dateString) => {
    if (!dateString) return "";

    const date = new Date(dateString);

    if (isNaN(date.getTime())) return "";

    return date.toLocaleDateString("en-GB", {
      day: "2-digit",
      month: "2-digit",
      year: "numeric",
    });
  };

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
  const openModal = (item) => {
    setSelectedApplication(item);
  };

  const closeModal = () => {
    setSelectedApplication(null);
  };

  if (showApply) {
    return <ApplyLeave />;
  }

  return (
    <>
      <div className="leave-page">
        <h1 className="leave-title">Leave Application</h1>

        <div className="leave-tabs">
          <div className="leave-tab">Personal Information</div>
          <div className="leave-tab">Payroll Information</div>
          <div className="leave-tab active">Leave</div>
          <div className="leave-tab">Payroll Tools</div>
        </div>

        <div className="leave-actions">
          <button className="filter-btn">Filter</button>
          <button className="apply-btn" onClick={() => setShowApply(true)}>
            + Apply for leave
          </button>
        </div>

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
                    <td className="view-link" onClick={() => openModal(item)}>
                      View
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>

          <div className="pagination">
            <button>{"<"}</button>
            <button className="active">1</button>
            <button>2</button>
            <button>{">"}</button>
          </div>
        </div>
      </div>
      <LeaveDetailsModal
        selectedApplication={selectedApplication}
        closeModal={closeModal}
        mapLeaveType={mapLeaveType}
        formatDate={formatDate}
      />
    </>
  );
};

export default LeaveHistory;
