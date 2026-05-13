import React, { useState } from "react";
import "./LeaveTypesModal.css";
import { ArrowRight, ArrowLeft } from "lucide-react";

const LeaveTypesModal = ({
  employee,
  setEmployee,
  formErrors,
  setFormErrors,
  onNext,
  onBack,
}) => {
  const [selectedCategory, setSelectedCategory] = useState("unskilled");

  const leaveOptions = {
    unskilled: {
      title: "Unskilled / Middle Management",
      ranges: "0-3yrs: 15 Days, 3-5yrs: 18 Days, 5+yrs: 20 Days",
    },
    senior: {
      title: "Senior Management",
      ranges: "0-3yrs: 18 Days, 3-5yrs: 20 Days, 5+yrs: 25 Days",
    },
    executive: {
      title: "Executive",
      ranges: "0-3yrs: 20 Days, 3-5yrs: 25 Days, 5+yrs: 30 Days",
    },
  };

  const statutoryLeaves = [
    { key: "sick", label: "Sick Leave", desc: "30 Days" },
    { key: "family", label: "Family Responsibility Leave", desc: "3 Days per year" },
    { key: "maternity", label: "Maternity Leave", desc: "121 Days" },
    { key: "unpaid", label: "Unpaid Leave", desc: "By arrangement" },
  ];

  const handleSelect = (value) => {
    setEmployee((prev) => ({
      ...prev,
      leaveType: value,
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
    <div className="emp-name-surname-container">
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
          {Object.entries(leaveOptions).map(([key, option]) => (
            <div
              key={key}
              className={`leave-card ${
                employee.leaveType === key ? "active" : ""
              }`}
              onClick={() => handleSelect(key)}
            >
              <input
                type="radio"
                checked={employee.leaveType === key}
                readOnly
              />
              <div>
                <div className="leave-text">
                <h4>{option.title}</h4>
                <p>{option.ranges}</p>
              </div>
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
             <button className="emp-bank-back-button" onClick={onBack}>
               <ArrowLeft size={20} className="back-save-button-icon" />
               Back
             </button>
   
             <button className="emp-next-button" onClick={handleNext}>
               Next
               <ArrowRight size={20} className="next-save-button-icon" />
             </button>
           </div>
      </div>
    </div>
  );
};

export default LeaveTypesModal;