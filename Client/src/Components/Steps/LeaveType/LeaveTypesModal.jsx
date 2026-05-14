import React, { useState, useEffect } from "react";
import "./LeaveTypesModal.css";
import { ArrowRight, ArrowLeft } from "lucide-react";
import { getJobGradeGroups } from "../../../api/JobGradeGroup";

const LeaveTypesModal = ({
  employee,
  setEmployee,
  formErrors,
  setFormErrors,
  onNext,
  onBack,
}) => {
  const [leaveOptions, setLeaveOptions] = useState([]);

  const statutoryLeaves = [
    { key: "sick", label: "Sick Leave", desc: "30 Days" },
    { key: "family", label: "Family Responsibility Leave", desc: "3 Days per year" },
    { key: "maternity", label: "Maternity Leave", desc: "121 Days" },
    { key: "unpaid", label: "Unpaid Leave", desc: "By arrangement" },
  ];

  useEffect(() => {
    fetchJobGradeGroups();
  }, []);

  const fetchJobGradeGroups = async () => {
    try {
      const data = await getJobGradeGroups();
      console.log("JOB GRADE GROUPS API DATA:", data);

      setLeaveOptions(data);
    } catch (error) {
      console.error("Error fetching job grade groups:", error);
    }
  };

 // ORDER CONTROL (IMPORTANT)
  const order = ["GROUP_A", "SENIOR", "EXECUTIVE"];

  //TITLE MAPPING
  const getGroupTitle = (groupKey) => {
    switch (groupKey) {
      case "GROUP_A":
        return "Unskilled / Middle Management";
      case "SENIOR":
        return "Senior Management";
      case "EXECUTIVE":
        return "Executive";
      default:
        return groupKey;
    }
  };

  // LEAVE RANGE RULES
  const getLeaveRanges = (groupKey) => {
    switch (groupKey) {
      case "GROUP_A":
        return "0-3yrs: 15 Days, 3-5yrs: 18 Days, 5+yrs: 22 Days";

      case "SENIOR":
        return "0-3yrs: 18 Days, 3-5yrs: 21 Days, 5+yrs: 25 Days";

      case "EXECUTIVE":
        return "0-3yrs: 20 Days, 3-5yrs: 23 Days, 5+yrs: 27 Days";

      default:
        return "No leave data available";
    }
  };

  //SELECT HANDLER
  const handleSelect = (option) => {
    setEmployee((prev) => ({
      ...prev,
      leaveType: option.groupKey,
      leaveTypeName: getGroupTitle(option.groupKey),
    }));
  };


  const handleNext = () => {
    if (!employee.leaveType) {
      setFormErrors({ leaveType: "Please select a leave type" });
      return;
    }

    onNext();
  };

  return (
    <div className="emp-leave-container">
      <div className="emp-leave-form-grid">

        <div className="emp-leave-personal-details-heading">
          <span>Leave Configuration</span>
        </div>

        <div className="emp-leave-type-sub">
          <span>Assign leave entitlement</span>
        </div>

        {/* ANNUAL LEAVE */}
        <div className="leave-section-title">
          ANNUAL LEAVE TYPE - SELECT ONE
        </div>

        <div className="emp-leave-type-line" />

        <div className="leave-options">
          {[...leaveOptions]
            .sort(
              (a, b) =>
                order.indexOf(a.groupKey) - order.indexOf(b.groupKey)
            )
            .map((option) => (
              <div
                key={option.groupKey}
                className={`leave-card ${
                  employee.leaveType === option.groupKey ? "active" : ""
                }`}
                onClick={() => handleSelect(option)}
              >
                <input
                  type="radio"
                  checked={employee.leaveType === option.groupKey}
                  readOnly
                />

                <div className="leave-text">
                  <h4>{getGroupTitle(option.groupKey)}</h4>
                  <p>{getLeaveRanges(option.groupKey)}</p>
                </div>
              </div>
            ))}
        </div>
        {/* STATUTORY */}
        <div className="leave-section-title">
          STATUTORY LEAVE (AUTO - INCLUDED)
        </div>

        <div className="emp-leave-type-line" />

        <div className="leave-grid-cards">
          {statutoryLeaves.map((item) => (
            <div key={item.key} className="leave-small-card">
              <h4>{item.label}</h4>
              <p>{item.desc}</p>
            </div>
          ))}
        </div>

        {/* ERROR */}
        {formErrors.leaveType && (
          <span className="error">{formErrors.leaveType}</span>
        )}

        {/* BUTTONS */}
        <div className="emp-button-row">
          <button className="emp-leave-back-button" onClick={onBack}>
            <ArrowLeft size={20} className="back-save-button-icon" />
            Back
          </button>

          <button className="emp-leave-next-button" onClick={handleNext}>
            Next
            <ArrowRight size={20} className="next-save-button-icon" />
          </button>
        </div>

      </div>
    </div>
  );
};

export default LeaveTypesModal;