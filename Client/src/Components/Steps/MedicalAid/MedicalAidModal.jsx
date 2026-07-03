import React, { useState, useEffect, useMemo } from "react";
import "./MedicalAidModal.css";
import {
  ArrowRight,
  ArrowLeft,
  Plus,
  Info,
  Check,
  X,
  ShieldCheck,
} from "lucide-react";

import { getMedicalAidPlans } from "../../../api/MedicalAidPlan";
import { populateDependentFromIdNumber } from "../../../utils/medicalAidHelpers";

const MedicalAidModal = ({
  closeModal,
  onClose,
  employee,
  setEmployee,
  onNext,
  onBack,
}) => {
  const [plans, setPlans] = useState([]);
  const [loading, setLoading] = useState(false);

  const salary = employee?.monthlySalary || 0;

  const [dependents, setDependents] = useState(employee?.dependents || []);

  const [showDependentModal, setShowDependentModal] = useState(false);

  const [newDependent, setNewDependent] = useState({
    fullName: "",
    lastName: "",
    gender: "",
    idNumber: "",
    relationship: "",
    dateOfBirth: "",
  });

  const relationshipOptions = ["Spouse", "Child", "Parent", "Sibling", "Other"];

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
        console.log("Loaded medical aid plans:", result);
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
    setEmployee((prev) => ({
      ...prev,
      medicalAidInfo: {
        ...prev.medicalAidInfo,
        planId: plan.medicalOptionId,
        medicalAidCategory: plan.medicalOptionCategoryName,
        medicalAidPlan: plan.medicalOptionName,
        selectedPlan: plan,
      },
    }));
  };

  //Helper to convert dependents to counts
  const getDependentCounts = (deps = []) => {
    let principalCount = 1; // employee always counts as principal
    let adultCount = 0;
    let childrenCount = 0;

    deps.forEach((d) => {
      const rel = (d.relationship || "").toLowerCase();

      if (rel === "spouse" || rel === "parent" || rel === "sibling") {
        adultCount++;
      } else if (rel === "child") {
        childrenCount++;
      } else {
        adultCount++;
      }
    });

    return {
      principalCount,
      adultCount,
      childrenCount,
    };
  };

  // =========================
  // ADD DEPENDENT
  // =========================
  const addDependent = () => {
    const updatedDependents = [...dependents, newDependent];

    setDependents(updatedDependents);

    setEmployee((prev) => ({
      ...prev,
      medicalAidInfo: {
        ...prev.medicalAidInfo,
        dependents: updatedDependents,
      },
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

  // =========================
  // REMOVE DEPENDENT
  // =========================
  const removeDependent = (indexToRemove) => {
    const updatedDependents = dependents.filter((dependent, index) => {
      return index !== indexToRemove;
    });

    setDependents(updatedDependents);

    setEmployee((prev) => ({
      ...prev,
      medicalAidInfo: {
        ...prev.medicalAidInfo,
        dependents: updatedDependents,
      },
    }));
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
          <div className="medical-section-title">DEPENDENTS</div>

          <div className="medical-info-banner">
            <Info size={16} />
            <span>
              Add spouse, children or other dependents before selecting a
              medical plan.
            </span>
          </div>

          {/* EMPTY STATE */}
          {dependents.length === 0 && (
            <div className="medical-empty-state">No dependents added yet.</div>
          )}

          {/* DEPENDENTS LIST */}
          <div className="medical-dependent-list">
            {dependents.map((dep, index) => (
              <div className="medical-dependent-card" key={index}>
                <div className="medical-dependent-card-inner">
                  {/* LEFT — name + detail line */}
                  <div className="medical-dependent-info">
                    <span className="medical-dependent-name">
                      {dep.fullName} {dep.lastName}
                    </span>
                    <span className="medical-dependent-meta">
                      {[dep.relationship, dep.dateOfBirth, dep.gender]
                        .filter(Boolean)
                        .join(" . ")}
                    </span>
                  </div>

                  {/* RIGHT — remove button */}
                  <button
                    className="medical-dependent-remove-btn"
                    onClick={() => removeDependent(index)}
                  >
                    Remove
                  </button>
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
          <div className="medical-section-title">MEDICAL AID PLANS</div>

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
              {/* FLATTENED GRID — all plans share one grid so they
                  always fill 2 columns regardless of category grouping */}
              <div className="medical-plan-grid">
                {filteredPlans.flatMap((category) =>
                  category.medicalOptions.map((plan) => {
                    const selected =
                      String(employee?.medicalAidInfo?.planId) ===
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
                          <div className="medical-selected-badge">Selected</div>
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
                                    plan.totalMonthlyContributionsPrincipal ||
                                      0,
                                  ).toFixed(2)}
                                </span>
                              </div>
                            </div>
                          </div>

                          {/* ADULT */}
                          <div className="medical-price-card">
                            <div className="medical-price-content">
                              <span className="medical-price-title">ADULT</span>
                              <div className="medical-price-amount-box">
                                <span className="medical-price-amount">
                                  R{" "}
                                  {Number(
                                    plan.totalMonthlyContributionsAdult || 0,
                                  ).toFixed(2)}
                                </span>
                              </div>
                            </div>
                          </div>

                          {/* CHILD */}
                          <div className="medical-price-card">
                            <div className="medical-price-content">
                              <span className="medical-price-title">CHILD</span>
                              <div className="medical-price-amount-box">
                                <span className="medical-price-amount">
                                  R{" "}
                                  {Number(
                                    plan.totalMonthlyContributionsChild || 0,
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
                                    plan.totalMonthlyContributionsSecondChild ||
                                      0,
                                  ).toFixed(2)}
                                </span>
                              </div>
                            </div>
                          </div>
                        </div>
                      </div>
                    );
                  }),
                )}
              </div>
            </div>
          )}
        </div>
      </div>

      {/* BUTTONS */}
      <div className="medical-button-row">
        <button className="medical-back-btn" onClick={onBack}>
          <ArrowLeft size={20} />
          Back
        </button>

        <button className="medical-next-btn" onClick={onNext}>
          Next
          <ArrowRight size={20} />
        </button>
      </div>

      {/* =========================
          DEPENDENT MODAL
      ========================= */}
      {showDependentModal && (
        <div
          className="medical-dependent-modal-overlay"
          onClick={() => setShowDependentModal(false)}
        >
          <div
            className="medical-dependent-modal"
            onClick={(e) => e.stopPropagation()}
          >
            {/* HEADER */}
            <div className="medical-dependent-modal-header">
              <div className="emp-left-icon-wrapper">
                <ShieldCheck size={24} />
              </div>
              <span>Add Dependent</span>

              <div className="emp-right-icon-wrapper">
                <X
                  size={24}
                  onClick={() => setShowDependentModal(false)}
                  style={{ cursor: "pointer" }}
                />
              </div>
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
                    disabled={newDependent.idNumber.length === 13}
                    onChange={(e) =>
                      setNewDependent((prev) => ({
                        ...prev,
                        gender: e.target.value,
                      }))
                    }
                  >
                    <option value="" disabled hidden>
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
                    placeholder="Id Number"
                    value={newDependent.idNumber}
                    onChange={(e) => {
                      const idNumber = e.target.value;

                      const populated = populateDependentFromIdNumber(idNumber);

                      setNewDependent((prev) => ({
                        ...prev,
                        idNumber,
                        ...(idNumber.length === 13 ? populated : {}),
                      }));
                    }}
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

                <div className="medical-input-group date-input-group">
                  <label>DATE OF BIRTH</label>
                  <div className="date-input-wrapper">
                    <input
                      type="date"
                      value={newDependent.dateOfBirth}
                      disabled={newDependent.idNumber.length === 13}
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
                      className="date-picker-dropdown-icon"
                    />
                  </div>
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

              <button className="medical-btn-primary" onClick={addDependent}>
                <Plus size={18} />
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
