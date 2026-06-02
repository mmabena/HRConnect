import React, { useState, useEffect, useMemo } from "react";
import "./MedicalAidModal.css";
import {
  ArrowRight,
  ArrowLeft,
  Plus,
  Info,
  Check,
  X,
} from "lucide-react";

import { getMedicalAidPlans } from "../../../api/MedicalAidPlan";

const MedicalAidModal = ({
  onClose,
  onNext,
  onPrevious,
  medicalAidInfo,
  setMedicalAidInfo,
}) => {
  const [plans, setPlans] = useState([]);
  const [loading, setLoading] = useState(false);

  const salary = medicalAidInfo?.salary || 0;

  const [dependents, setDependents] = useState(
    medicalAidInfo?.dependents || []
  );

  const [showDependentModal, setShowDependentModal] = useState(false);

  const [newDependent, setNewDependent] = useState({
    fullName: "",
    lastName: "",
    gender: "",
    idNumber: "",
    relationship: "",
  });

  const relationshipOptions = [
    "Spouse",
    "Child",
    "Parent",
    "Sibling",
    "Other",
  ];

  const genderOptions = ["Male", "Female"];

  // =========================
  // LOAD DATA
  // =========================
  useEffect(() => {
    const loadPlans = async () => {
      try {
        setLoading(true);

        const result = await getMedicalAidPlans();

        setPlans(Array.isArray(result) ? result : []);
      } catch (error) {
        console.log("Failed to load medical aid plans", error);
        setPlans([]);
      } finally {
        setLoading(false);
      }
    };

    loadPlans();
  }, []);

  // =========================
  // FILTER BY SALARY
  // =========================
const filteredPlans = useMemo(() => {
  const safeSalary = Number(salary) || 0;

  return plans
    .map((category) => ({
      ...category,
      medicalOptions: (category.medicalOptions || []).filter((opt) => {
        const min = Number(opt.salaryBracketMin ?? 0);
        const max = Number(opt.salaryBracketMax ?? Infinity);

        return safeSalary >= min && safeSalary <= max;
      }),
    }))
    .filter((category) => category.medicalOptions.length > 0);
}, [plans, salary]);

  // =========================
  // SELECT PLAN
  // =========================
  const selectPlan = (plan) => {
    setMedicalAidInfo((prev) => ({
      ...prev,
      planId: plan.medicalOptionId,
      medicalAidPlan: plan.medicalOptionName,
      selectedPlan: plan,
    }));
  };

  // =========================
  // ADD DEPENDENT
  // =========================
  const addDependent = () => {
    const updatedDependents = [...dependents, newDependent];

    setDependents(updatedDependents);

    setMedicalAidInfo((prev) => ({
      ...prev,
      dependents: updatedDependents,
    }));

    setNewDependent({
      fullName: "",
      lastName: "",
      gender: "",
      idNumber: "",
      relationship: "",
      dateOfBirth: "",
    });

    setShowDependentModal(false);
  };

  return (
    <div className="emp-medical-aid-container">

      {/* HEADER */}
      <div className="emp-medical-aid-header-frame">

        <div className="emp-medical-aid-personal-details-heading">
          <span>Medical Aid</span>
        </div>

        <div className="emp-medical-aid-sub">
          <span>Add dependents and select a medical plan</span>
        </div>

      </div>

      {/* CONTENT */}
      <div className="emp-medical-aid-content-frame">

        <div className="emp-medical-aid-form-grid">

          {/* DEPENDENTS */}
          <div className="medical-section-title">
            DEPENDENTS
          </div>

          <div className="medical-info-banner">
            <Info size={16} />
            <span>
              Add spouse, children or other dependents before selecting a medical plan.
            </span>
          </div>

          {/* EMPTY STATE */}
          {dependents.length === 0 && (
            <div className="medical-empty-state">
              No dependents added yet.
            </div>
          )}

          {/* DEPENDENTS LIST */}
          <div className="medical-dependent-list">

            {dependents.map((dep, index) => (
              <div
                className="medical-dependent-card"
                key={index}
              >
                <div className="medical-dependent-grid">

                  <div>
                    <strong>
                      {dep.fullName} {dep.lastName}
                    </strong>
                  </div>

                  <div>{dep.relationship}</div>

                  <div>{dep.gender}</div>

                  <div>{dep.idNumber}</div>

                </div>
              </div>
            ))}

          </div>

          {/* ADD BUTTON */}
          <button
            className="medical-add-dependent-button"
            onClick={() => setShowDependentModal(true)}
          >
            <Plus size={18} />
            Add Dependent or Child
          </button>

          {/* =========================
              MEDICAL PLANS
          ========================= */}
          <div className="medical-section-title">
            MEDICAL AID PLANS
          </div>

          {loading ? (
            <div className="medical-loading-state">
              Loading medical aid plans...
            </div>
          ) : filteredPlans.length === 0 ? (
            <div className="medical-empty-state">
              No plans available for your salary
            </div>
          ) : (
            <div className="medical-category-container">

              {filteredPlans.map((category) => (
                <div
                  key={category.medicalOptionCategoryId}
                  className="medical-category-section"
                >

                  <div className="medical-plan-grid">

                    {category.medicalOptions.map((plan) => {
                      const selected =
                        String(medicalAidInfo?.planId) ===
                        String(plan.medicalOptionId);

                      return (
                        <div
                          key={plan.medicalOptionId}
                          className={`medical-plan-card ${
                            selected ? "selected" : ""
                          }`}
                          onClick={() => selectPlan(plan)}
                        >

                          {selected && (
                            <div className="medical-selected-badge">
                              Selected
                            </div>
                          )}

                          <h4>{plan.medicalOptionName}</h4>

                          <div className="medical-plan-status">
                            <Check size={14} />
                            <span>Available</span>
                          </div>

                          

                       <div className="medical-plan-pricing">

  {/* PRINCIPAL */}
  <div className="medical-price-card">

    <div className="medical-price-content">

      <span className="medical-price-title">
        PRINCIPLE
      </span>

      <div className="medical-price-amount-box">

        <span className="medical-price-amount">
          R{" "}
          {Number(
            plan.totalMonthlyContributionsPrincipal || 0
          ).toFixed(2)}
        </span>

      </div>

    </div>

  </div>

  {/* ADULT */}
  <div className="medical-price-card">

    <div className="medical-price-content">

      <span className="medical-price-title">
        ADULT
      </span>

      <div className="medical-price-amount-box">

        <span className="medical-price-amount">
          R{" "}
          {Number(
            plan.totalMonthlyContributionsAdult || 0
          ).toFixed(2)}
        </span>

      </div>

    </div>

  </div>

  {/* CHILD */}
  <div className="medical-price-card">

    <div className="medical-price-content">

      <span className="medical-price-title">
        CHILD
      </span>

      <div className="medical-price-amount-box">

        <span className="medical-price-amount">
          R{" "}
          {Number(
            plan.totalMonthlyContributionsChild || 0
          ).toFixed(2)}
        </span>

      </div>

    </div>

  </div>

  {/* 2ND CHILD */}
  <div className="medical-price-card">

    <div className="medical-price-content">

      <span className="medical-price-title">
        2ND CHILD +
      </span>

      <div className="medical-price-amount-box">

        <span className="medical-price-amount">
          R{" "}
          {Number(
            plan.totalMonthlyContributionsSecondChild || 0
          ).toFixed(2)}
        </span>

      </div>

    </div>

  </div>

</div>

                        </div>
                      );
                    })}

                  </div>
                </div>
              ))}

            </div>
          )}

          {/* BUTTONS */}
          <div className="emp-button-row">

            <button onClick={onPrevious}>
              <ArrowLeft size={20} />
              Back
            </button>

            <button onClick={onNext}>
              Next
              <ArrowRight size={20} />
            </button>

          </div>

        </div>
      </div>

      {/* =========================
          DEPENDENT MODAL
      ========================= */}
      {showDependentModal && (
        <div className="medical-dependent-modal-overlay">

          <div className="medical-dependent-modal">

            {/* HEADER */}
            <div className="medical-dependent-modal-header">

              <span>Add Dependent</span>

              <X
                size={18}
                cursor="pointer"
                onClick={() => setShowDependentModal(false)}
              />

            </div>

            {/* BODY */}
            <div className="medical-dependent-modal-body">

              <div className="medical-dependent-grid">

                <div className="medical-input-group">
                  <label>FIRST NAME</label>

                  <input
                    type="text"
                    placeholder="First Name"
                    value={newDependent.fullName}
                    onChange={(e) =>
                      setNewDependent((prev) => ({
                        ...prev,
                        fullName: e.target.value,
                      }))
                    }
                  />
                </div>

                <div className="medical-input-group">
                  <label>LAST NAME</label>

                  <input
                    type="text"
                    placeholder="Last Name"
                    value={newDependent.lastName}
                    onChange={(e) =>
                      setNewDependent((prev) => ({
                        ...prev,
                        lastName: e.target.value,
                      }))
                    }
                  />
                </div>

                <div className="medical-input-group">
                  <label>GENDER</label>
               
                  <select
                    value={newDependent.gender}
                    onChange={(e) =>
                      setNewDependent((prev) => ({
                        ...prev,
                        gender: e.target.value,
                      }))
                    }
                  >
                    <option value =" disabled">
                      Gender
                    </option>
                    {genderOptions.map((g) => (
                      <option key={g} value={g}>
                        {g}
                      </option>
                    ))}
                  </select>
                  <img
                      src="/images/arrow_drop_down_circle.png"
                      alt="Dropdown icon"
                      className="icon-dropdown-icon"
                    />
                </div>

                <div className="medical-input-group">
                  <label>ID NUMBER</label>

                  <input
                    type="text"
                    placeholder="ID Number"
                    value={newDependent.idNumber}
                    onChange={(e) =>
                      setNewDependent((prev) => ({
                        ...prev,
                        idNumber: e.target.value,
                      }))
                    }
                  />
                </div>

                <div className="medical-input-group">
  <label>RELATIONSHIP</label>

  <div
    className={`medical-select-wrapper ${
      newDependent.relationship ? "has-value" : ""
    }`}
  >
    <select
      value={newDependent.relationship || ""}
      onChange={(e) =>
        setNewDependent((prev) => ({
          ...prev,
          relationship: e.target.value,
        }))
      }
      className="medical-relationship-input"
    >
      {/* PLACEHOLDER */}
      <option value="" disabled hidden>
        Select Relationship
      </option>

      {relationshipOptions.map((r) => (
        <option key={r} value={r}>
          {r}
        </option>
      ))}
    </select>

    <img
      src="/images/arrow_drop_down_circle.png"
      alt="Dropdown icon"
      className="icon-dropdown-icon"
    />
  </div>
</div>

                <div className="medical-input-group">
                  <label>DATE OF BIRTH</label>
                  <input
                    type="date"
                    value={newDependent.dateOfBirth}
                    onChange={(e) =>
                      setNewDependent((prev) => ({
                        ...prev,
                        dateOfBirth: e.target.value,
                      }))
                    }
                  />
                  <img
                        src="/images/calendar-range.svg"
                        alt="Calendar icon"
                        className="dropdown-icon"
                      />
                </div>

              </div>

            </div>

            {/* FOOTER */}
            <div className="medical-dependent-modal-footer">

              <button
                className="medical-btn-cancel"
                onClick={() => setShowDependentModal(false)}
              >
                Cancel
              </button>

              <button
                className="medical-btn-primary"
                onClick={addDependent}
              >
                Add Dependent
              </button>

            </div>

          </div>

        </div>
      )}

    </div>
  );
};

export default MedicalAidModal;