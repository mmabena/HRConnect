// import React, {useState, useEffect} from "react";
// import "./MedicalAidModal.css";
// import { ArrowRight, ArrowLeft } from "lucide-react";
// import { getMedicalAidPlans } from "../../../api/MedicalAidPlan";


// const MedicalAidModal = ({
//   onClose,
//   onNext,
//   onPrevious,
//   medicalAidInfo,
//   setMedicalAidInfo,
// }) => {
//   const [medicalAidPlans, setMedicalAidPlans] = useState([]);
//   const [dependents, setDependents] = useState(medicalAidInfo.dependents || []);
//   const relationshipOptions = [
//     "Spouse",
//     "Child",
//     "Parent",
//     "Sibling",
//     "Other",
//   ];
//   const [selectedPlan, setSelectedPlan] = useState(medicalAidInfo.planId || "");
//   const [dependentCount, setDependentCount] = useState(medicalAidInfo.dependentCount || 0);

//   // =========================
//   // FETCH MEDICAL AID PLANS
//   // =========================
//   useEffect(() => {
//     const fetchMedicalAidPlans = async () => {
//       try {
//         const plans = await getMedicalAidPlans();
//         setMedicalAidPlans(plans);
//       } catch (error) {
//         console.error("Error fetching medical aid plans:", error);
//       }
//     };

//     fetchMedicalAidPlans();
//   }, []);

//   // =========================
//   // SELECT PLANS 
//   // =========================
//   const handleSelectPlan = (planId) => {
//     setSelectedPlan(planId);
//     setMedicalAidInfo((prev) => ({ ...prev, planId }));
//   }

//   const handlePlanChange = (e) => {
//     setSelectedPlan(e.target.value);
//     setMedicalAidInfo((prev) => ({ ...prev, planId: e.target.value }));
//   };

//   const handleDependentChange = (e) => {
//     const count = parseInt(e.target.value, 10);
//     setDependentCount(count);
//     setMedicalAidInfo((prev) => ({ ...prev, dependentCount: count }));
//   };

//   return (
//     <div className="emp-medical-aid-container">
//       <div className="emp-medical-aid-form-grid">
//         <div className="emp-medical-aid-personal-details-heading">
//           <span>Medical Aid </span>
//         </div>

//         <div className="emp-medical-aid-type-sub">
//           <span>Add dependents then select a plan</span>
//         </div>
//         <div className="form-group">
//           <label htmlFor="medical-aid-plan">Select Medical Aid Plan:</label>
//           <select
//             id="medical-aid-plan"
//             value={selectedPlan}
//             onChange={handlePlanChange}
//           >
//             <option value="">-- Select a Plan --</option>
//             {medicalAidPlans.map((plan) => (
//               <option key={plan.id} value={plan.id}>
//                 {plan.name}
//               </option>
//             ))}
//           </select>
//         </div>

//         <div className="form-group">
//           <label htmlFor="dependent-count">Number of Dependents:</label>
//           <input
//             type="number"
//             id="dependent-count"
//             min="0"
//             value={dependentCount}
//             onChange={handleDependentChange}
//           />
//         </div>

//         <div className="modal-buttons">
//           <button className="previous-button" onClick={onPrevious}>
//             <ArrowLeft size={