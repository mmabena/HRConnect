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
import { toast } from "react-toastify";

import {
  getMedicalAidPlans,
  getEligibleMedicalAidPlans,
  validateMedicalAidDependent,
} from "../../../api/MedicalAidPlan";
import { populateDependentFromIdNumber } from "../../../utils/medicalAidHelpers";

const MedicalAidModal = ({
  closeModal,
  onClose,
  employee,
  setEmployee,
  formErrors,
  setFormErrors,
  onNext,
  onBack,
}) => {
  const [plans, setPlans] = useState([]);
  const [loading, setLoading] = useState(false);

  const salary = employee?.monthlySalary || 0;

  const [dependents, setDependents] = useState(
    employee?.medicalAidInfo?.dependents || [],
  );

  const [showDependentModal, setShowDependentModal] = useState(false);

  const [newDependent, setNewDependent] = useState({
    firstName: "",
    lastName: "",
    identificationType: "idNumber",
    gender: "",
    passportNumber: "",
    idNumber: "",
    relationship: "",
    dateOfBirth: "",
  });

  const relationshipOptions = ["Adult", "Child"];

  const genderOptions = ["Male", "Female"];

  const loadEligiblePlans = async () => {
    try {
      setLoading(true);

      const counts = getDependentCounts(dependents);

      const result = await getEligibleMedicalAidPlans({
        salary: employee.monthlySalary,
        employmentStatus: employee.employmentStatus,
        employeeName: employee.firstName,
        employeeSurname: employee.lastName,
        numberOfPrincipals: counts.principalCount,
        numberOfAdults: counts.adultCount,
        numberOfChildren: counts.childrenCount,
      });

      setPlans(result);

      // Keep selected plan in sync
      const selectedPlanId = employee?.medicalAidInfo?.planId;

      if (selectedPlanId) {
        const updatedPlan = result
          .flatMap((category) => category.medicalOptions)
          .find(
            (plan) => String(plan.medicalOptionId) === String(selectedPlanId),
          );

        if (updatedPlan) {
          setEmployee((prev) => ({
            ...prev,
            medicalAidInfo: {
              ...prev.medicalAidInfo,

              // Update all values that may have changed for the Preview step.
              selectedPlan: updatedPlan,
              estimatedTotalMonthlyPremium:
                updatedPlan.estimatedTotalMonthlyPremium,

              totalMonthlyContributionsPrincipal:
                updatedPlan.totalMonthlyContributionsPrincipal,

              totalMonthlyContributionsAdult:
                updatedPlan.totalMonthlyContributionsAdult,

              totalMonthlyContributionsChild:
                updatedPlan.totalMonthlyContributionsChild,

              totalMonthlyContributionsSecondChild:
                updatedPlan.totalMonthlyContributionsSecondChild,
            },
          }));
        }
      }
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadEligiblePlans();
  }, [employee.monthlySalary, employee.employmentStatus, dependents]);

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
        estimatedTotalMonthlyPremium: plan.estimatedTotalMonthlyPremium ?? 0,
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

      if (rel === "adult") {
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

  const validateDependent = () => {
    const errors = {};

    if (!newDependent.firstName?.trim()) {
      errors.firstName = "First name is required";
    }

    if (!newDependent.lastName?.trim()) {
      errors.lastName = "Last name is required";
    }

    if (newDependent.identificationType === "idNumber") {
      if (!newDependent.idNumber?.trim()) {
        errors.idNumber = "ID number is required";
      } else if (!/^\d{13}$/.test(newDependent.idNumber)) {
        errors.idNumber = "ID number must be 13 digits";
      }
    }

    if (newDependent.identificationType === "passportNumber") {
      if (!newDependent.passportNumber?.trim()) {
        errors.passportNumber = "Passport number is required";
      }
    }

    if (!newDependent.gender) {
      errors.gender = "Gender is required";
    }

    if (!newDependent.relationship) {
      errors.relationship = "Relationship is required";
    }

    if (!newDependent.dateOfBirth) {
      errors.dateOfBirth = "Date of birth is required";
    }

    return errors;
  };

  // =========================
  // ADD DEPENDENT
  // =========================
  const addDependent = async () => {
    // =====================================
    // 1. FRONTEND REQUIRED-FIELD VALIDATION
    // =====================================
    const errors = validateDependent();

    setFormErrors(errors);

    if (Object.keys(errors).length > 0) {
      toast.error("Please complete all required fields.");
      return;
    }

    // =====================================
    // 2. BACKEND BUSINESS VALIDATION
    // =====================================
    try {
      await validateMedicalAidDependent(employee.employeeId, newDependent);
    } catch (error) {
      console.log(
        "Medical aid dependent validation error:",
        error.response?.data,
      );

      if (error.response?.data?.errors) {
        const backendErrors = error.response.data.errors;

        setFormErrors(backendErrors);

        toast.error("Validation failed. Please check the dependent details.");

        return;
      }

      toast.error("Validation failed.");
      return;
    }

    // =====================================
    // 3. ADD DEPENDENT TO FRONTEND STATE
    // =====================================
    const updatedDependents = [...dependents, newDependent];

    setDependents(updatedDependents);

    setEmployee((prev) => ({
      ...prev,
      medicalAidInfo: {
        ...prev.medicalAidInfo,
        dependents: updatedDependents,
      },
    }));

    // =====================================
    // 4. RESET FORM
    // =====================================
    setNewDependent({
      firstName: "",
      lastName: "",
      identificationType: "idNumber",
      passportNumber: "",
      gender: "",
      idNumber: "",
      relationship: "",
      dateOfBirth: "",
    });

    setFormErrors({});

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

  const formatCurrency = (value) => {
    if (!value) return "N/A";

    return new Intl.NumberFormat("en-ZA", {
      style: "currency",
      currency: "ZAR",
      minimumFractionDigits: 2,
    }).format(Number(value));
  };

  const handleNext = async () => {
    const selectedPlanId = employee?.medicalAidInfo?.planId;

    if (!selectedPlanId) {
      toast.error("Please select a medical aid plan before proceeding.");
      return;
    }

    onNext();
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
                      {dep.firstName} {dep.lastName}
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
                                  {formatCurrency(
                                    plan.totalMonthlyContributionsPrincipal ||
                                      "0.00",
                                  )}
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
                                  {formatCurrency(
                                    plan.totalMonthlyContributionsAdult ||
                                      "0.00",
                                  )}
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
                                  {formatCurrency(
                                    plan.totalMonthlyContributionsChild ||
                                      "0.00",
                                  )}
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
                                  {formatCurrency(
                                    plan.totalMonthlyContributionsChild2 ??
                                      plan.totalMonthlyContributionsChild ??
                                      "0.00",
                                  )}
                                </span>
                              </div>
                            </div>
                          </div>
                        </div>
                        <div className="medical-total-price-card">
                          <span className="medical-price-title">TOTAL:</span>

                          <span className="medical-price-amount">
                            {formatCurrency(
                              plan.estimatedTotalMonthlyPremium || "0.00",
                            )}
                          </span>
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

        <button className="medical-next-btn" onClick={handleNext}>
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
                    className={
                      formErrors?.firstName ? "medical-error-input" : ""
                    }
                    value={newDependent.firstName}
                    onChange={(e) => {
                      setNewDependent((prev) => ({
                        ...prev,
                        firstName: e.target.value,
                      }));

                      setFormErrors((prev) => ({
                        ...prev,
                        firstName: "",
                      }));
                    }}
                  />

                  {formErrors?.firstName && (
                    <span className="medical-error-message">
                      {formErrors.firstName}
                    </span>
                  )}
                </div>

                <div className="medical-input-group">
                  <label>LAST NAME</label>

                  <input
                    type="text"
                    placeholder="Last Name"
                    className={
                      formErrors?.lastName ? "medical-error-input" : ""
                    }
                    value={newDependent.lastName}
                    onChange={(e) => {
                      setNewDependent((prev) => ({
                        ...prev,
                        lastName: e.target.value,
                      }));

                      setFormErrors((prev) => ({
                        ...prev,
                        lastName: "",
                      }));
                    }}
                  />

                  {formErrors?.lastName && (
                    <span className="medical-error-message">
                      {formErrors.lastName}
                    </span>
                  )}
                </div>

                <div className="medical-input-group">
                  <label>ID / PASSPORT</label>

                  <div className="medical-select-wrapper">
                    <select
                      value={newDependent.identificationType}
                      onChange={(e) => {
                        const type = e.target.value;

                        setNewDependent((prev) => ({
                          ...prev,
                          identificationType: type,
                          idNumber: "",
                          passportNumber: "",
                          gender: "",
                          dateOfBirth: "",
                        }));
                      }}
                    >
                      <option value="idNumber">ID Number</option>
                      <option value="passportNumber">Passport Number</option>
                    </select>

                    <img
                      src="/images/arrow_drop_down_circle.png"
                      alt="Dropdown icon"
                      className="icon-dropdown-icon"
                    />
                  </div>
                </div>
                <div className="medical-input-group">
                  <label>
                    {newDependent.identificationType === "idNumber"
                      ? "ID NUMBER"
                      : "PASSPORT NUMBER"}
                  </label>

                  <input
                    type="text"
                    className={
                      formErrors?.idNumber || formErrors?.passportNumber
                        ? "medical-error-input"
                        : ""
                    }
                    placeholder={
                      newDependent.identificationType === "idNumber"
                        ? "ID Number"
                        : "Passport Number"
                    }
                    value={
                      newDependent.identificationType === "idNumber"
                        ? newDependent.idNumber
                        : newDependent.passportNumber
                    }
                    onChange={(e) => {
                      const value = e.target.value;

                      if (newDependent.identificationType === "idNumber") {
                        const populated = populateDependentFromIdNumber(value);

                        setNewDependent((prev) => ({
                          ...prev,
                          idNumber: value,
                          ...(value.length === 13 ? populated : {}),
                        }));

                        setFormErrors((prev) => ({
                          ...prev,
                          idNumber: "",
                        }));
                      } else {
                        setNewDependent((prev) => ({
                          ...prev,
                          passportNumber: value,
                        }));

                        setFormErrors((prev) => ({
                          ...prev,
                          passportNumber: "",
                        }));
                      }
                    }}
                  />

                  {newDependent.identificationType === "idNumber" &&
                    formErrors?.idNumber && (
                      <span className="medical-error-message">
                        {formErrors.idNumber}
                      </span>
                    )}

                  {newDependent.identificationType === "passportNumber" &&
                    formErrors?.passportNumber && (
                      <span className="medical-error-message">
                        {formErrors.passportNumber}
                      </span>
                    )}
                </div>

                <div className="medical-input-group full-width">
                  <label>GENDER</label>

                  <div
                    className={`medical-select-wrapper ${
                      newDependent.relationship ? "has-value" : ""
                    }`}
                  >
                    <select
                      className={
                        formErrors?.gender ? "medical-error-input" : ""
                      }
                      value={newDependent.gender}
                      disabled={newDependent.idNumber.length === 13}
                      onChange={(e) => {
                        setNewDependent((prev) => ({
                          ...prev,
                          gender: e.target.value,
                        }));

                        setFormErrors((prev) => ({
                          ...prev,
                          gender: "",
                        }));
                      }}
                    >
                      {formErrors?.gender && (
                        <span className="medical-error-message">
                          {formErrors.gender}
                        </span>
                      )}
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
                      className={
                        formErrors?.relationship ? "medical-error-input" : ""
                      }
                      onChange={(e) => {
                        setNewDependent((prev) => ({
                          ...prev,
                          relationship: e.target.value,
                        }));

                        setFormErrors((prev) => ({
                          ...prev,
                          relationship: "",
                        }));
                      }}
                    >
                      {formErrors?.relationship && (
                        <span className="medical-error-message">
                          {formErrors.relationship}
                        </span>
                      )}
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
                      className={
                        formErrors?.dateOfBirth ? "medical-error-input" : ""
                      }
                      value={newDependent.dateOfBirth}
                      disabled={
                        newDependent.identificationType === "idNumber" &&
                        newDependent.idNumber.length === 13
                      }
                      onChange={(e) => {
                        setNewDependent((prev) => ({
                          ...prev,
                          dateOfBirth: e.target.value,
                        }));

                        setFormErrors((prev) => ({
                          ...prev,
                          dateOfBirth: "",
                        }));
                      }}
                    />
                    <img
                      src="/images/calendar-range.svg"
                      alt="Calendar icon"
                      className="date-picker-dropdown-icon"
                    />
                    </div>
                    {formErrors?.dateOfBirth && (
                      <span className="medical-error-message">
                        {formErrors.dateOfBirth}
                      </span>
                    )}
                    
                  
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
