import React, { useState, useEffect } from "react";
import "./LeaveTypesModal.css";
import { ArrowRight, ArrowLeft, check } from "lucide-react";
import { getJobGradeGroups } from "../../../api/JobGradeGroup";

const LeaveTypesModal = ({
  employee,
  setEmployee,
  positions,
  leaveOptions,
  formErrors,
  setFormErrors,
  onNext,
  onBack,
}) => {


  // =========================
  // AUTO SELECT LEAVE TYPE
  // =========================
  useEffect(() => {
    if (!leaveOptions.length || !positions?.length) return;

    const jobTitle = employee?.jobTitle;

    if (!jobTitle) return;

    const selectedPosition = positions.find(
      (p) => String(p.positionId) === String(jobTitle),
    );

    if (!selectedPosition?.jobGradeId) return;

    const matchedOption = leaveOptions.find(
      (o) => String(o.jobGradeId) === String(selectedPosition.jobGradeId),
    );

    if (!matchedOption) return;

    setEmployee((prev) => {
      // IMPORTANT: ensure state actually changes
      if (prev.leaveType === matchedOption.groupKey) return prev;

      return {
        ...prev,
        leaveType: matchedOption.groupKey,
        leaveTypeName: getGroupTitle(matchedOption.groupKey),
      };
    });
  }, [employee?.jobTitle, positions, leaveOptions]);

  // =========================
  // STATUTORY LEAVES
  // =========================
  const statutoryLeaves = [
    { key: "sick", label: "Sick Leave", desc: "30 Days" },
    {
      key: "family",
      label: "Family Responsibility Leave",
      desc: "3 Days per year",
    },
    { key: "maternity", label: "Maternity Leave", desc: "121 Days" },
    { key: "unpaid", label: "Unpaid Leave", desc: "By arrangement" },
  ];

  const isMale = employee.gender?.toLowerCase() === "male";

  const selectedPosition = positions.find(
    (p) => String(p.positionId) === String(employee.jobTitle),
  );

  const selectedLeaveOption = leaveOptions.find(
    (o) => String(o.jobGradeId) === String(selectedPosition?.jobGradeId),
  );

  const filteredStatutoryLeaves = statutoryLeaves.filter((leave) => {
    if (isMale && leave.key === "maternity") return false;
    return true;
  });

  // =========================
  // ORDER CONTROL
  // =========================
  const order = ["GROUP_A", "SENIOR", "EXECUTIVE"];

  // =========================
  // LABEL MAPPING
  // =========================
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

  const handleSelect = (option) => {
    setEmployee((prev) => ({
      ...prev,
      leaveType: option.groupKey,
      leaveTypeName: getGroupTitle(option.groupKey),
    }));
  };

  // =========================
  // VALIDATION
  // =========================
  const handleNext = () => {
    if (!employee.leaveType) {
      setFormErrors((prev) => ({
        ...prev,
        leaveType: "Please select a leave type",
      }));
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

        <div className="leave-section-title">ANNUAL LEAVE ENTITLEMENT</div>

        {/* =========================
            LEAVE OPTIONS
        ========================= */}
        <div className="leave-options">
          {[
            ...new Map(
              leaveOptions.map((item) => [item.groupKey, item]),
            ).values(),
          ]
            .sort(
              (a, b) => order.indexOf(a.groupKey) - order.indexOf(b.groupKey),
            )
            .map((option) => {
              const isActive =
                selectedLeaveOption &&
                String(option.groupKey).trim().toUpperCase() ===
                  String(selectedLeaveOption.groupKey).trim().toUpperCase();

              return (
                <div
                  key={option.groupKey}
                  className={`leave-card ${isActive ? "active" : ""}`}
                >
                  <input type="radio" checked={isActive} readOnly />

                  <div className="leave-text">
                    <h4>{getGroupTitle(option.groupKey)}</h4>

                    <p>{getLeaveRanges(option.groupKey)}</p>
                  </div>
                </div>
              );
            })}
        </div>

        {/* =========================
            STATUTORY LEAVES
        ========================= */}
        <>
          <div className="leave-section-title">
            STATUTORY LEAVE (AUTO - INCLUDED)
          </div>

          <div className="leave-grid-cards">
            {filteredStatutoryLeaves.map((item) => (
              <div key={item.key} className="leave-small-card">
                <h4>{item.label}</h4>
                <p>{item.desc}</p>
              </div>
            ))}
          </div>
        </>

        {/* ERROR */}
        {formErrors.leaveType && (
          <span className="error">{formErrors.leaveType}</span>
        )}

        {/* BUTTONS */}
        <div className="emp-button-row">
          <button className="emp-leave-back-button" onClick={onBack}>
            <ArrowLeft size={20} />
            Back
          </button>

          <button className="emp-leave-next-button" onClick={handleNext}>
            Next
            <ArrowRight size={20} />
          </button>
        </div>
      </div>
    </div>
  );
};

export default LeaveTypesModal;
