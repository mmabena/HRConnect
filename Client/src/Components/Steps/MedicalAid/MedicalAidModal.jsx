import React, { useState, useEffect } from "react";
import "./MedicalAidModal.css";
import { ArrowRight, ArrowLeft, Plus, Info, Check } from "lucide-react";
import { getMedicalAidPlans } from "../../../api/MedicalAidPlan";

const MedicalAidModal = ({
  onClose,
  onNext,
  onPrevious,
  medicalAidInfo,
  setMedicalAidInfo,
}) => {
  const [plans, setPlans] = useState([]);
  const [dependents, setDependents] = useState(
    medicalAidInfo?.dependents || []
  );

  const relationshipOptions = ["Spouse", "Child", "Parent", "Sibling", "Other"];

  // load plans
  useEffect(() => {
    const loadPlans = async () => {
      try {
        const result = await getMedicalAidPlans();
        setPlans(result || []);
      } catch (error) {
        console.log("Failed to load medical aid plans", error);
      }
    };

    loadPlans();
  }, []);

  // select plan
  const selectPlan = (plan) => {
    setMedicalAidInfo((prev) => ({
      ...prev,
      planId: plan.id,
      medicalAidPlan: plan.name,
      selectedPlan: plan,
    }));
  };

  // add dependent
  const addDependent = () => {
    const newList = [
      ...dependents,
      { fullName: "", relationship: "Child" },
    ];

    setDependents(newList);

    setMedicalAidInfo((prev) => ({
      ...prev,
      dependents: newList,
    }));
  };

  // update dependent
  const updateDependent = (index, field, value) => {
    const newList = [...dependents];
    newList[index][field] = value;

    setDependents(newList);

    setMedicalAidInfo((prev) => ({
      ...prev,
      dependents: newList,
    }));
  };

  return (
    <div className="emp-medical-aid-container">
      <div className="emp-medical-aid-form-grid">

        <div className="emp-medical-aid-personal-details-heading">
          <span>Medical Aid</span>
        </div>

        <div className="emp-medical-aid-sub">
          <span>Add dependents then choose a plan</span>
        </div>

      
        <div className="medical-section-title">DEPENDENTS</div>

        <div className="medical-info-banner">
          <Info size={16} />
          <span>Add spouse, children or dependents</span>
        </div>

        {dependents.length === 0 && (
          <div className="medical-empty-state">
            No dependents added yet
          </div>
        )}

        <div className="medical-dependent-list">
          {dependents.map((dep, index) => (
            <div className="medical-dependent-card" key={index}>
              <div className="medical-dependent-grid">

                <div className="medical-input-group">
                  <label>Full Name</label>
                  <input
                    type="text"
                    placeholder="Enter name"
                    value={dep.fullName}
                    onChange={(e) =>
                      updateDependent(index, "fullName", e.target.value)
                    }
                  />
                </div>

                <div className="medical-input-group">
                  <label>Relationship</label>
                  <select
                    value={dep.relationship}
                    onChange={(e) =>
                      updateDependent(index, "relationship", e.target.value)
                    }
                  >
                    {relationshipOptions.map((r) => (
                      <option key={r} value={r}>
                        {r}
                      </option>
                    ))}
                  </select>
                </div>

              </div>
            </div>
          ))}
        </div>

        <button className="medical-add-dependent-button" onClick={addDependent}>
          <Plus size={18} />
          Add Dependent
        </button>

        {/* plans */}
        <div className="medical-section-title">MEDICAL PLANS</div>

        <div className="medical-plan-grid">
          {plans.map((plan) => {
            const selected =
              String(medicalAidInfo?.planId) === String(plan.id);

            return (
              <div
                key={plan.id}
                className={`medical-plan-card ${selected ? "selected" : ""}`}
                onClick={() => selectPlan(plan)}
              >
                {selected && (
                  <div className="medical-selected-badge">selected</div>
                )}

                <h4>{plan.name}</h4>

                <div className="medical-plan-status">
                  <Check size={14} />
                  <span>Available</span>
                </div>

                <div className="medical-plan-pricing">
                  <div className="medical-price-box">
                    <span>Principal</span>
                    <h5>R {plan.principalAmount}</h5>
                  </div>

                  <div className="medical-price-box">
                    <span>Adult</span>
                    <h5>R {plan.adultDependantAmount}</h5>
                  </div>

                  <div className="medical-price-box">
                    <span>Child</span>
                    <h5>R {plan.childDependantAmount}</h5>
                  </div>
                </div>
              </div>
            );
          })}
        </div>

        {/* buttons */}
        <div className="emp-button-row">
          <button className="emp-medical-aid-back-button" onClick={onPrevious}>
            <ArrowLeft size={20} />
            Back
          </button>

          <button className="emp-medical-aid-next-button" onClick={onNext}>
            Next
            <ArrowRight size={20} />
          </button>
        </div>

      </div>
    </div>
  );
};

export default MedicalAidModal;