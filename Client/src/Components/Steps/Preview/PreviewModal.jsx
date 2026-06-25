import React from "react";
import "./PreviewModal.css";
import {
  ArrowLeft,
  UserPlus,
  X,
  Check,
} from "lucide-react";

const PreviewModal = ({
  employee,
  positions,
  onBack,
  onSave,
  onClose,
}) => {
  // =========================
  // POSITION LOOKUP
  // =========================
  const selectedPosition = positions?.find(
    (p) => String(p.positionId) === String(employee?.jobTitle),
  );

  // =========================
  // EMPLOYEE TYPE
  // =========================
  const employeeTypeRaw =
    employee?.employeeType ||
    employee?.employmentType ||
    employee?.employeeStatus;

  const employeeType = employeeTypeRaw
    ?.toString()
    .trim()
    .toLowerCase()
    .replace(/\s+/g, "-");

  const isContract = employeeType === "contract";

  const isFixedTerm =
    employeeType === "fixed-term" ||
    employeeType === "fixedterm";

  const isPermanent = !isContract && !isFixedTerm;

  //console.log("Preview onSave:", onSave);

  return (
    <div className="preview-container">

        {/* =========================
            BODY
        ========================= */}
        

          {/* =========================
              REVIEW TEXT
          ========================= */}
          <div className="preview-review-heading">
            <h3>Review & Confirm</h3>

            <span>
              Check all details before saving
            </span>
          </div>

          <div className="preview-body">

          {/* =========================
              PERSONAL DETAILS
          ========================= */}
          <div className="preview-section">

            <div className="preview-section-title">
              PERSONAL & EMPLOYMENT
            </div>

            <div className="preview-grid">

              <div className="preview-card">
                <span>FULL NAME</span>

                <h4>
                  {employee?.name || "N/A"}{" "}
                  {employee?.surname || ""}
                </h4>
              </div>

              <div className="preview-card">
                <span>EMPLOYMENT TYPE</span>

                <h4>
                  {employee?.employeeStatus || "N/A"}
                </h4>
              </div>

              <div className="preview-card">
                <span>EMAIL</span>

                <h4>{employee?.email || "N/A"}</h4>
              </div>

              <div className="preview-card">
                <span>CONTACT</span>

                <h4>
                  {employee?.contactNumber || "N/A"}
                </h4>
              </div>

              <div className="preview-card">
                <span>START DATE</span>

                <h4>
                  {employee?.startDate || "N/A"}
                </h4>
              </div>

              <div className="preview-card">
                <span>MONTHLY SALARY</span>

                <h4>
                  {employee?.monthlySalary || "N/A"}
                </h4>
              </div>

              <div className="preview-card">
                <span>JOB TITLE</span>

                <h4>
                  {selectedPosition?.jobTitle ||
                    "Not Selected"}
                </h4>
              </div>

              <div className="preview-card">
                <span>BRANCH</span>

                <h4>{employee?.branch || "N/A"}</h4>
              </div>

            </div>
          </div>

          {/* =========================
              BANKING
          ========================= */}
          <div className="preview-section">

            <div className="preview-section-title">
              BANKING
            </div>

            <div className="preview-grid">

              <div className="preview-card">
                <span>BANK</span>

                <h4>
                  {employee?.bankName || "N/A"}
                </h4>
              </div>

              <div className="preview-card">
                <span>ACCOUNT TYPE</span>

                <h4>
                  {employee?.accountType || "N/A"}
                </h4>
              </div>

              <div className="preview-card">
                <span>ACCOUNT NUMBER</span>

                <h4>
                  {employee?.accountNumber || "N/A"}
                </h4>
              </div>

              <div className="preview-card">
                <span>BRANCH CODE</span>

                <h4>
                  {employee?.branchCode || "N/A"}
                </h4>
              </div>

            </div>
          </div>

          {/* =========================
              LEAVE
          ========================= */}
          <div className="preview-section">

            <div className="preview-section-title">
              LEAVE
            </div>

            <div className="preview-grid">

              <div className="preview-card">
                <span>LEAVE TYPE</span>

                <h4>
                  {employee?.leaveTypeName || "N/A"}
                </h4>
              </div>

              <div className="preview-card">
                <span>LEAVE GROUP</span>

                <h4>
                  {employee?.leaveType || "N/A"}
                </h4>
              </div>

            </div>

            {(isContract || isFixedTerm) && (
              <div className="preview-info-banner">
                Non-permanent employees receive
                predefined leave allocations for the
                duration of employment.
              </div>
            )}
          </div>

          {/* =========================
              PENSION
          ========================= */}
          {isPermanent && (
            <div className="preview-section">

              <div className="preview-section-title">
                PENSION
              </div>

              <div className="preview-grid">

                <div className="preview-card">
                  <span>PENSION FUND</span>

                  <h4>
                    {employee?.pensionFund || "N/A"}
                  </h4>
                </div>

                <div className="preview-card">
                  <span>CONTRIBUTION</span>

                  <h4>
                    {employee?.employeeContribution ||
                      "N/A"}
                  </h4>
                </div>

              </div>
            </div>
          )}
          

          {/* =========================
              MEDICAL AID
          ========================= */}
          {isPermanent && (
            <div className="preview-section">

              <div className="preview-section-title">
                MEDICAL AID
              </div>

              <div className="preview-grid">

                <div className="preview-card">
                  <span>MEDICAL AID</span>

                  <h4>
                    {employee?.medicalAidInfo?.medicalAidCategory || "N/A"}
                  </h4>
                </div>

                <div className="preview-card">
                  <span>MEDICAL PLAN</span>

                  <h4>
                    {employee?.medicalAidInfo?.medicalAidPlan || "N/A"}
                  </h4>
                </div>

              </div>
            </div>
          )}
          </div>

        {/* =========================
            FOOTER
        ========================= */}
        <div className="preview-footer">

          <button
            className="preview-back-button"
            onClick={onBack}
          >
            <ArrowLeft size={18} />
            Back
          </button>

          <button
            className="preview-save-button"
            onClick={onSave}
          >
            Save
          </button>
        </div>
      </div>
  
  );
};

export default PreviewModal;