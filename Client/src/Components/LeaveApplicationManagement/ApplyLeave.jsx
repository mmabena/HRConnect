import React, { useEffect, useState } from "react";
import { getEmployeeLeave, applyLeave } from "../../api/leaveApplicationApi";
import "./ApplyLeave.css";

const ApplyLeave = () => {
  const [leaveData, setLeaveData] = useState(null);
  const [selectedLeaveId, setSelectedLeaveId] = useState("");
  const [description, setDescription] = useState("");
  const [files, setFiles] = useState([]);
  const selectedBalance = leaveData?.leaveBalances?.find(
  (l) => l.leaveTypeId === Number(selectedLeaveId)
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
const safeRemaining =
  remainingBalance < 0 ? 0 : remainingBalance;
const handleSubmit = async () => {
  if (isSubmitting) return; 

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

    alert("Leave application submitted successfully");

  } catch (error) {
    console.error(error);
    alert("Submission failed");
  } finally {
    setIsSubmitting(false);  
  }
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
  return (
    <div className="leave-page">

      <div className="leave-tabs">
        <div className="leave-tab">Personal Information</div>
        <div className="leave-tab">Payroll Information</div>
        <div className="leave-tab active">Leave</div>
        <div className="leave-tab">Payroll Tools</div>
      </div>

      {/* HEADER */}
      <div className="apply-header">
        <div>
          <h2 className="apply-title">Apply for Leave</h2>
          <p className="apply-subtitle">
            Submit a new leave request for approval
          </p>
        </div>

        <button className="back-btn">← Back to History</button>
      </div>

      {/* MAIN GRID */}
      <div className="apply-grid">

        {/* LEFT PANEL */}
        <div className="apply-left">

          {/* INFO BOX */}
          <div className="info-box">
            <span className="info-icon">i</span>
            <p>
            {selectedBalance
              ? `Your current ${selectedBalance.leaveType} balance is ${selectedBalance.availableDays} days. All Applications are viewed within 2 business days.`
              : "Select a leave type to view your balance."}
          </p>
          </div>

          {/* LEAVE DETAILS */}
          <div className="section">
            <p className="section-title">LEAVE DETAILS</p>

            {/* Leave Type */}
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
            </div>

            {/* Dates */}
            <div className="row">
              <div className="form-group">
                <label>Start Date</label>
                <input
                type="date"
                className="input"
                value={startDate}
                onChange={(e) => {
                setStartDate(e.target.value);                           
                setTimeout(() => {
                  e.target.blur();
                }, 0);
              }}
              />
              </div>

              <div className="form-group">
                <label>End Date</label>
                <input
                  type="date"
                  className="input"
                  value={endDate}
                  onChange={(e) => {
                  setEndDate(e.target.value);
                  setTimeout(() => {
                    e.target.blur();
                  }, 0);
                }}
                />
              </div>
            </div>

            {/* Days + Balance */}
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
          </div>

          {/* SUPPORTING INFO */}
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
                <p>Click to upload or drag a file here</p>
                <small>PDF, JPG or PNG - max 5MB</small>
              </div>
            </div>
          </div>
          </div>
          {/* ACTION BUTTONS */}
          <div className="form-actions">
           <button
              className="submit-btn"
              onClick={handleSubmit}
              disabled={isSubmitting}
            >
              {isSubmitting ? "Submitting..." : "✓ Submit Application"}
            </button>
            <button className="cancel-btn">Cancel</button>
          </div>

        </div>

        {/* RIGHT PANEL */}
        <div className="apply-right">

          {/* ENTITLEMENTS */}
          <div className="card">
            <p className="card-title">LEAVE ENTITLEMENTS</p>
            
            <div className="progress-item">
              <div className="progress-row">
                <span>Annual Leave</span>
                <span>4.85 / 15 Days</span>
              </div>
              <div className="progress-bar red"></div>
            </div>
            
            <div className="progress-item">
              <div className="progress-row">
                <span>Sick Leave</span>
                <span>26 / 30 Days</span>
              </div>
              <div className="progress-bar blue"></div>
            </div>
            
            <div className="progress-item">
              <div className="progress-row">
                <span>Family Responsibility Leave</span>
                <span>1 / 3 Days</span>
              </div>
              <div className="progress-bar blue"></div>
            </div>
            
            <div className="progress-item">
              <div className="progress-row">
                <span>Maternity Leave</span>
                <span>0 / 121 Days</span>
              </div>
              <div className="progress-bar grey"></div>
            </div>
          </div>

          {/* POLICY */}
          <div className="card">
            <p className="card-title">LEAVE POLICY</p>

            <ul className="policy-list">
              <li>Annual leave must be approved at least 5 business days in advance.</li>
              <li>Sick leave requires a medical certificate for absences exceeding 2 consecutive days.</li>
              <li>Family responsibility leave covers up to 3 days per year for qualifying events.</li>
              <li>Unused leave does not carry over to the following year.</li>
            </ul>
          </div>
        

        </div>

      </div>
    </div>
  );
};

export default ApplyLeave;