import React, { useEffect, useState } from "react";
import {
  getLeaveHistory,
  getEmployeeLeave,
} from "../../api/leaveApplicationApi";
import "./LeaveHistory.css";
import { Dot, X, Check, SlidersHorizontal, Plus } from "lucide-react";
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
  const [showFilterMenu, setShowFilterMenu] = useState(false);
  const [selectedStatus, setSelectedStatus] = useState("All");
  const [currentPage, setCurrentPage] = useState(1);
  const rowsPerPage = 8;

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
      setCurrentPage(1);

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
  const sortedData = [...data].sort(
    (a, b) => new Date(b.startDate) - new Date(a.startDate),
  );

  const filteredData =
    selectedStatus === "All"
      ? sortedData
      : sortedData.filter((item) => item.status === selectedStatus);

  const totalPages = Math.ceil(filteredData.length / rowsPerPage);

  const indexOfLastRow = currentPage * rowsPerPage;
  const indexOfFirstRow = indexOfLastRow - rowsPerPage;

  const currentRows = filteredData.slice(indexOfFirstRow, indexOfLastRow);

  const goToPage = (pageNumber) => {
    setCurrentPage(pageNumber);
  };

  const goToPreviousPage = () => {
    if (currentPage > 1) {
      setCurrentPage(currentPage - 1);
    }
  };

  const goToNextPage = () => {
    if (currentPage < totalPages) {
      setCurrentPage(currentPage + 1);
    }
  };
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
          <div className="filter-container">
            <button
              className="filter-btn"
              onClick={() => setShowFilterMenu(!showFilterMenu)}
            >
              <SlidersHorizontal className="action-icon" />
              Filter
            </button>

            {showFilterMenu && (
              <div className="filter-menu">
                <div
                  className="filter-option"
                  onClick={() => {
                    setSelectedStatus("All");
                    setShowFilterMenu(false);
                    setCurrentPage(1);
                  }}
                >
                  All
                </div>

                <div
                  className="filter-option"
                  onClick={() => {
                    setSelectedStatus("Pending");
                    setShowFilterMenu(false);
                    setCurrentPage(1);
                  }}
                >
                  Pending
                </div>

                <div
                  className="filter-option"
                  onClick={() => {
                    setSelectedStatus("Approved");
                    setShowFilterMenu(false);
                    setCurrentPage(1);
                  }}
                >
                  Approved
                </div>

                <div
                  className="filter-option"
                  onClick={() => {
                    setSelectedStatus("Rejected");
                    setShowFilterMenu(false);
                    setCurrentPage(1);
                  }}
                >
                  Rejected
                </div>
              </div>
            )}
          </div>

          <button className="apply-btn" onClick={() => setShowApply(true)}>
            <Plus className="action-icon" />
            Apply for leave
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
                currentRows.map((item) => (
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
