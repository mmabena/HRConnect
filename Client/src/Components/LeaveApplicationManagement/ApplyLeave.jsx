import React from "react";
import "./ApplyLeave.css";

const ApplyLeave = () => {
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
              Your current annual leave balance is 4.85 days. All Applications are viewed within 2 business days.
            </p>
          </div>

          {/* LEAVE DETAILS */}
          <div className="section">
            <p className="section-title">LEAVE DETAILS</p>

            {/* Leave Type */}
            <div className="form-group">
              <label>Leave Type</label>
              <select className="input">
                <option>Leave Type</option>
              </select>
            </div>

            {/* Dates */}
            <div className="row">
              <div className="form-group">
                <label>Start Date</label>
                <input type="date" className="input" />
              </div>

              <div className="form-group">
                <label>End Date</label>
                <input type="date" className="input" />
              </div>
            </div>

            {/* Days + Balance */}
            <div className="row">
              <div className="form-group">
                <label>Number of Days</label>
                <input type="text" className="input disabled" placeholder="3 Days" disabled />
              </div>

              <div className="form-group">
                <label>Leave balance</label>
                <input
                type="text"
                className="input disabled"
                value="1.85 Days remaining"
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
                placeholder="Briefly describe the reason for your leave request..."
              ></textarea>
            </div>

           <div className="form-group">
            <label>Attach Supporting Document</label>

            <div className="upload-box">
              <div className="upload-content">
                <p>Click to upload or drag a file here</p>
                <small>PDF, JPG or PNG - max 5MB</small>
              </div>
            </div>
          </div>
          </div>
  {/* ACTION BUTTONS */}
          <div className="form-actions">
            <button className="submit-btn">✓ Submit Application</button>
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