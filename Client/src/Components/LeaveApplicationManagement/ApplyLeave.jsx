import React, { useEffect, useState } from "react";
import { getEmployeeLeave, applyLeave } from "../../api/leaveApplicationApi";
import { Upload, Check, ArrowLeft, Trash2 } from "lucide-react";
import "./ApplyLeave.css";
import LeaveHistory from "./LeaveHistory";

const ApplyLeave = () => {
  const [leaveData, setLeaveData] = useState(null);
  const [selectedLeaveId, setSelectedLeaveId] = useState("");
  const [description, setDescription] = useState("");
  const [files, setFiles] = useState([]);
  const employee = JSON.parse(localStorage.getItem("currentEmployee"));
  const [showHistory, setShowHistory] = useState(false);
  const [errors, setErrors] = useState({});
  const [successMessage, setSuccessMessage] = useState("");

  const selectedBalance = leaveData?.leaveBalances?.find(
    (l) => l.leaveTypeId === Number(selectedLeaveId),
  );

  const [startDate, setStartDate] = useState("");
  const [endDate, setEndDate] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);
  const calculateDays = () => {
    if (!startDate || !endDate) return 0;

    const start = new Date(startDate);
    const end = new Date(endDate);

    const diffTime = end - start;

    if (diffTime < 0) return 0;

    return Math.floor(diffTime / (1000 * 60 * 60 * 24)) + 1;
  };

  const requestedDays = calculateDays();

  const remainingBalance =
    selectedBalance && requestedDays
      ? (selectedBalance.availableDays - requestedDays).toFixed(2)
      : null;
  const safeRemaining = remainingBalance < 0 ? 0 : remainingBalance;
  const validateForm = () => {
    const newErrors = {};
    if (!selectedLeaveId) {
      newErrors.leaveType = "Leave type is required";
    }

    if (!startDate) {
      newErrors.startDate = "Start date is required";
    }

    if (!endDate) {
      newErrors.endDate = "End date is required";
    }

    if (startDate && endDate) {
      const start = new Date(startDate);
      const end = new Date(endDate);

      if (start > end) {
        newErrors.startDate = "Start date cannot be after end date";
        newErrors.endDate = "End date cannot be before start date";
      }
    }
    if (selectedBalance && requestedDays > selectedBalance.availableDays) {
      newErrors.leaveBalance = "Requested leave days exceed available balance";
    }
    const selectedLeave = leaveData?.leaveBalances?.find(
      (l) => l.leaveTypeId === Number(selectedLeaveId),
    );

    const isAnnualLeave =
      selectedLeave?.leaveType?.toLowerCase() === "annual leave";

    if (!isAnnualLeave && !description.trim()) {
      newErrors.description = "Description is required";
    }

    if (!isAnnualLeave && files.length === 0) {
      newErrors.documents =
        "Supporting document is required for this leave type";
    }

    const allowedTypes = ["image/png", "image/jpeg", "application/pdf"];

    const invalidFile = files.find((file) => !allowedTypes.includes(file.type));

    if (invalidFile) {
      newErrors.documents = "Only PNG, JPG, JPEG and PDF files are allowed";
    }
    setErrors(newErrors);

    return Object.keys(newErrors).length === 0;
  };
  const handleSubmit = async () => {
    if (isSubmitting) return;
    if (!validateForm()) {
      return;
    }
    setIsSubmitting(true);

    try {
      const employee = JSON.parse(localStorage.getItem("currentEmployee"));
      const employeeId = employee?.employeeId;

      if (!employeeId) {
        setIsSubmitting(false);
        return alert("Employee not found");
      }

      const formData = new FormData();

      formData.append("EmployeeId", employeeId);
      formData.append("LeaveTypeId", selectedLeaveId);
      formData.append("StartDate", startDate);
      formData.append("EndDate", endDate);
      formData.append("Description", description);

      files.forEach((file) => {
        formData.append("Documents", file);
      });

      await applyLeave(formData);

      setSuccessMessage("Leave submitted successfully");

      setErrors({});

      setSelectedLeaveId("");
      setDescription("");
      setFiles([]);
      setStartDate("");
      setEndDate("");

      const refreshedLeave = await getEmployeeLeave(employeeId);

      setLeaveData(refreshedLeave);
    } catch (error) {
      console.error(error);
      setErrors({
        submit:
          error?.response?.data?.message ||
          "Failed to submit leave application",
      });
    } finally {
      setIsSubmitting(false);
    }
  };
  const formatDate = (dateString) => {
    if (!dateString) return "";

    return new Date(dateString).toLocaleDateString("en-GB", {
      day: "numeric",
      month: "long",
      year: "numeric",
    });
  };

  useEffect(() => {
    const fetchLeave = async () => {
      const employee = JSON.parse(localStorage.getItem("currentEmployee"));
      const employeeId = employee?.employeeId;

      if (!employeeId) return;

      const res = await getEmployeeLeave(employeeId);
      setLeaveData(res);
    };

    fetchLeave();
  }, []);
  useEffect(() => {
    if (successMessage) {
      const timer = setTimeout(() => {
        setSuccessMessage("");
      }, 5000);

      return () => clearTimeout(timer);
    }
  }, [successMessage]);
  if (showHistory) {
    return <LeaveHistory />;
  }
  return (
    <div className="leave-page">
      <div className="apply-header">
        <div>
          <h2 className="apply-title">Apply for Leave</h2>
          <p className="apply-subtitle">
            Submit a new leave request for approval
          </p>
        </div>

        <button className="back-btn" onClick={() => setShowHistory(true)}>
          <ArrowLeft className="back-icon" />
          Back to History
        </button>
      </div>

      <div className="apply-grid">
        <div className="apply-left">
          <div className="section">
            {successMessage && (
              <div className="success-message">{successMessage}</div>
            )}

            {errors.submit && (
              <div className="submit-error-message">{errors.submit}</div>
            )}
            <p className="section-title">LEAVE DETAILS</p>

            <div className="form-group">
              <label>Leave Type</label>
              <select
                className="input"
                onChange={(e) => setSelectedLeaveId(e.target.value)}
              >
                <option value="">Leave Type</option>

                {leaveData?.leaveBalances.map((l, index) => (
                  <option key={l.leaveTypeId} value={l.leaveTypeId}>
                    {l.leaveType}
                  </option>
                ))}
              </select>
              {errors.leaveType && (
                <span className="error-text">{errors.leaveType}</span>
              )}
            </div>

            <div className="row date-row">
              <div className="form-group">
                <label>Start Date</label>

                <div className="date-wrapper">
                  <input
                    type="date"
                    className="input date-input"
                    value={startDate}
                    onChange={(e) => {
                      setStartDate(e.target.value);
                    }}
                  />

                  <span className="formatted-date">
                    {formatDate(startDate)}
                  </span>

                  <img
                    src="/images/calendar-range.svg"
                    alt="calendar icon"
                    className="calendar-icon"
                  />
                </div>
              </div>

              <div className="form-group">
                <label>End Date</label>

                <div className="date-wrapper">
                  <input
                    type="date"
                    className="input date-input"
                    value={endDate}
                    onChange={(e) => {
                      setEndDate(e.target.value);
                    }}
                  />

                  <span className="formatted-date">{formatDate(endDate)}</span>

                  <img
                    src="/images/calendar-range.svg"
                    alt="calendar icon"
                    className="calendar-icon"
                  />
                </div>
              </div>
            </div>

            {errors.startDate && (
              <span className="error-text">{errors.startDate}</span>
            )}
            {errors.endDate && (
              <span className="error-text2">{errors.endDate}</span>
            )}
            <div className="row">
              <div className="form-group">
                <label>Number of Days</label>
                <input
                  type="text"
                  className="input disabled"
                  value={requestedDays > 0 ? `${requestedDays} Days` : ""}
                  disabled
                />
              </div>

              <div className="form-group">
                <label>Leave balance</label>
                <input
                  type="text"
                  className="input disabled"
                  value={
                    remainingBalance !== null
                      ? `${safeRemaining} Days remaining`
                      : ""
                  }
                  disabled
                />
              </div>
            </div>

            {errors.leaveBalance && (
              <span className="error-text">{errors.leaveBalance}</span>
            )}
          </div>
          <div className="section">
            <p className="section-title">SUPPORTING INFORMATION</p>

            <div className="form-group">
              <label>Description / Reason</label>
              <textarea
                className="textarea"
                value={description}
                onChange={(e) => setDescription(e.target.value)}
                placeholder="Briefly describe the reason for your leave request..."
              />
              {errors.description && (
                <span className="error-text">{errors.description}</span>
              )}
            </div>

            <div className="form-group">
              <label>Attach Supporting Document</label>

              <div
                className="upload-box"
                onClick={() => document.getElementById("fileInput").click()}
                onDragOver={(e) => e.preventDefault()}
                onDrop={(e) => {
                  e.preventDefault();
                  setFiles([...e.dataTransfer.files]);
                }}
              >
                <input
                  id="fileInput"
                  type="file"
                  multiple
                  style={{ display: "none" }}
                  onChange={(e) => setFiles([...e.target.files])}
                />

                <div className="upload-content">
                  {files.length === 0 ? (
                    <>
                      <Upload className="upload-icon" />

                      <p>Click to upload or drag a file here</p>

                      <small>PDF, JPG or PNG - max 5MB</small>
                    </>
                  ) : (
                    <div className="uploaded-files">
                      {files.map((file, index) => (
                        <div key={index} className="uploaded-file-item">
                          <div className="uploaded-file-left">
                            <Check className="file-check-icon" />

                            <span className="uploaded-file-name">
                              {file.name}
                            </span>
                          </div>

                          <button
                            type="button"
                            className="remove-file-btn"
                            onClick={(e) => {
                              e.stopPropagation();

                              setFiles(files.filter((_, i) => i !== index));
                            }}
                          >
                            <Trash2 className="remove-file-icon" />
                          </button>
                        </div>
                      ))}
                    </div>
                  )}
                </div>
              </div>
              {errors.documents && (
                <span className="error-text">{errors.documents}</span>
              )}
            </div>
          </div>
        </div>

        <div className="apply-right">
          <div className="card">
            <p className="card-title">LEAVE ENTITLEMENTS</p>

            {leaveData?.leaveBalances
              ?.filter((leave) => {
                if (
                  leave.leaveType === "Maternity Leave" &&
                  employee?.gender !== "Female"
                ) {
                  return false;
                }

                return true;
              })
              .map((leave, index) => {
                const entitlement =
                  leave.leaveType === "Annual Leave" ? 22 : leave.accruedDays;

                const percentage =
                  entitlement > 0
                    ? (leave.availableDays / entitlement) * 100
                    : 0;

                const isAnnualLeave = leave.leaveType === "Annual Leave";

                let progressClass = "blue";

                if (isAnnualLeave) {
                  if (percentage >= 75) {
                    progressClass = "red";
                  } else if (percentage >= 50) {
                    progressClass = "orange";
                  } else {
                    progressClass = "blue";
                  }
                }

                if (leave.availableDays <= 0) {
                  progressClass = "grey";
                }

                return (
                  <div className="progress-item" key={index}>
                    <div className="progress-row">
                      <span>{leave.leaveType}</span>

                      <span>
                        {leave.availableDays} / {entitlement} Days
                      </span>
                    </div>

                    <div className="progress-track">
                      <div
                        className={`progress-bar ${progressClass}`}
                        style={{
                          width: `${Math.min(percentage, 100)}%`,
                        }}
                      ></div>
                    </div>
                  </div>
                );
              })}
          </div>
          <div className="policy-card">
            <p className="card-title">LEAVE POLICY</p>

            <ul className="policy-list">
              <li>
                Annual leave must be approved at least 5 business days in
                advance.
              </li>

              <li>
                Sick leave requires a medical certificate for absences exceeding
                2 consecutive days.
              </li>

              <li>
                Family responsibility leave covers up to 3 days per year for
                qualifying events.
              </li>

              <li>Unused leave does not carry over to the following year.</li>
            </ul>
          </div>
        </div>
      </div>
      <div className="form-actions">
        <button
          className="submit-btn"
          onClick={handleSubmit}
          disabled={isSubmitting}
        >
          <Check className="submit-icon" />

          {isSubmitting ? "Submitting..." : "Submit Application"}
        </button>

        <button
          className="cancel-btn"
          onClick={() => {
            setSelectedLeaveId("");
            setDescription("");
            setFiles([]);
            setStartDate("");
            setEndDate("");
            setErrors({});
          }}
        >
          Cancel
        </button>
      </div>
    </div>
  );
};

export default ApplyLeave;
