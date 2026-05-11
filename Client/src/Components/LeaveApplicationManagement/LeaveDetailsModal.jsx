import React from "react";
import { X, Check } from "lucide-react";
import "./LeaveDetailsModal.css";

const LeaveDetailsModal = ({
  selectedApplication,
  closeModal,
  mapLeaveType,
  formatDate,
}) => {
  if (!selectedApplication) return null;

  return (
    <div className="leave-modal-overlay">
      <div className="leave-modal">
        {/* HEADER */}
        <div className="leave-modal-header">
          <div>
            <h2 className="leave-modal-title">
              {mapLeaveType(selectedApplication.leaveTypeCode)}
            </h2>

            <p className="leave-modal-subtitle">
              Application #{selectedApplication.leaveTypeCode}-
              {selectedApplication.id}
              {" · "}
              {selectedApplication.employeeName}
            </p>
          </div>

          <button className="leave-modal-close" onClick={closeModal}>
            <X size={20} />
          </button>
        </div>

        {/* BODY */}
        <div className="leave-modal-body">
          {/* GRID */}
          <div className="leave-modal-grid">
            <div className="leave-modal-item">
              <span className="leave-modal-label">Employee Name</span>
              <span className="leave-modal-value">
                {selectedApplication.employeeName}
              </span>
            </div>

            <div className="leave-modal-item">
              <span className="leave-modal-label">Status</span>

              <span
                className={`leave-modal-status ${selectedApplication.status.toLowerCase()}`}
              >
                {selectedApplication.status}
              </span>
            </div>

            <div className="leave-modal-item">
              <span className="leave-modal-label">Start Date</span>

              <span className="leave-modal-value">
                {formatDate(selectedApplication.startDate)}
              </span>
            </div>

            <div className="leave-modal-item">
              <span className="leave-modal-label">End Date</span>

              <span className="leave-modal-value">
                {formatDate(selectedApplication.endDate)}
              </span>
            </div>

            <div className="leave-modal-item">
              <span className="leave-modal-label">Days Taken</span>

              <span className="leave-modal-value">
                {selectedApplication.daysRequested} Days
              </span>
            </div>

            <div className="leave-modal-item">
              <span className="leave-modal-label">Entitlement</span>

              <span className="leave-modal-value">
                {selectedApplication.daysAllocated} Days
              </span>
            </div>
          </div>

          <div className="leave-modal-divider"></div>

          {/* DESCRIPTION */}
          <div className="leave-modal-section">
            <p className="leave-modal-section-title">Description</p>

            <div className="leave-modal-description">
              {selectedApplication.description || "No description provided"}
            </div>
          </div>

          {/* TIMELINE */}
          <div className="leave-modal-section timeline-section">
            <p className="leave-modal-section-title">Application Timeline</p>

            <div className="timeline">
              {/* APPLICATION SUBMITTED */}
              <div className="timeline-item completed">
                <div className="timeline-icon">
                  <Check size={14} />
                </div>

                <div className="timeline-content">
                  <p className="timeline-title">Application Submitted</p>

                  <p className="timeline-date">
                    {formatDate(selectedApplication.startDate)}
                  </p>
                </div>
              </div>

              {/* APPLICATION REVIEW */}
              <div className="timeline-item completed">
                <div className="timeline-icon">
                  <Check size={14} />
                </div>

                <div className="timeline-content">
                  <p className="timeline-title">Application Review</p>

                  <p className="timeline-date">Within 2 business day</p>
                </div>
              </div>

              {/* FINAL STATUS */}
              <div
                className={`timeline-item ${
                  selectedApplication.status === "Pending"
                    ? "pending"
                    : "completed"
                }`}
              >
                <div
                  className={`timeline-icon ${
                    selectedApplication.status === "Rejected" ? "rejected" : ""
                  }`}
                >
                  {selectedApplication.status === "Approved" && (
                    <Check size={14} />
                  )}

                  {selectedApplication.status === "Rejected" && <X size={14} />}
                </div>

                <div className="timeline-content">
                  <p className="timeline-title">
                    {selectedApplication.status === "Approved" &&
                      "Application Approved"}

                    {selectedApplication.status === "Rejected" &&
                      "Application Rejected"}

                    {selectedApplication.status === "Pending" &&
                      "Awaiting Approval"}
                  </p>

                  {selectedApplication.status === "Pending" ? (
                    <p className="timeline-date">Pending Review</p>
                  ) : (
                    selectedApplication.decisionDate && (
                      <p className="timeline-date">
                        {formatDate(selectedApplication.decisionDate)}
                      </p>
                    )
                  )}
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default LeaveDetailsModal;
