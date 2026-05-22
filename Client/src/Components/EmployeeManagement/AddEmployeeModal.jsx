import React, { useState, useRef, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { toast } from "react-toastify";
import "./AddEmployeeModal.css";
import BankingDetailsModal from "../../Components/Steps/BankingManagement/BankingDetailsModal.jsx";
import LeaveTypesModal from "../Steps/LeaveType/LeaveTypesModal.jsx";
import PreviewModal from "../Steps/Preview/PreviewModal.jsx";
import PensionFundOptionsModal from "../Steps/PensionFundOptions/PensionFundOptionsModal.jsx";

import { addEmployee } from "../../api/Employee";

import useEmployeeForm from "../../hooks/useEmployeeForm";
import useEmployeeData from "../../hooks/useEmployeeData";
import useEmployeeValidation from "../../hooks/useEmployeeValidation";
import useImageUpload from "../../hooks/useImageUpload";
import useUserRole from "../../hooks/useUserRole";
import { ArrowRight, Upload, UserRoundPlus, X } from "lucide-react";

const AddEmployeeModal = ({ closeModal }) => {
  const navigate = useNavigate();
  const role = useUserRole();
  const { positions, allEmployees, loading: dataLoading } = useEmployeeData();
  const { validateEmployee } = useEmployeeValidation();
  const { uploadImage, uploading } = useImageUpload();
  const [loading, setLoading] = useState(false);
  const [currentStep, setCurrentStep] = useState(1);

  const titles = ["Mr", "Mrs", "Ms", "Dr", "Prof"];
  const genders = ["Male", "Female"];
  const branches = ["Johannesburg", "CapeTown", "UK"];
  const employmentStatuses = ["Permanent", "FixedTerm", "Contract"];

  const { employee, setEmployee, formErrors, setFormErrors, onInputChange } =
    useEmployeeForm({
      title: "",
      name: "",
      surname: "",
      idType: "id",
      idNumber: "",
      passportNumber: "",
      nationality: "",
      gender: "",
      dateOfBirth: "",
      contactNumber: "",
      taxNumber: "",
      email: "",
      physicalAddress: "",
      city: "",
      zipCode: "",
      startDate: "",
      branch: "",
      monthlySalary: "",
      jobTitle: "",
      employeeStatus: "",
      reportsTo: "",
      disability: false,
      disabilityType: "",
      profileImage: "",
      // Banking Fields
      bankName: "",
      accountNumber: "",
      branchCode: "",
      accountType: "",
      paymentMethod: "",
      accountHolderName: "",
      reference: "",
      payFrequency: "",
      payDate: "",
    });

  // =========================
  // WIZARD CONFIG BASED ON EMPLOYMENT STATUS
  // =========================

  const wizardConfig = {
    Permanent: [
      "Personal Details",
      "Banking Details",
      "Leave",
      "Pension",
      "Medical Aid",
      "Preview",
    ],
    FixedTerm: ["Personal Details", "Banking Details", "Leave", "Preview"],
    Contract: ["Personal Details", "Banking Details", "Leave", "Preview"],
  };

  const defaultSteps = [
    "Personal Details",
    "Banking Details",
    "Leave",
    "Pension",
    "Medical Aid",
    "Preview",
  ];

  const activeSteps = wizardConfig[employee.employeeStatus] || defaultSteps;

  const steps = (wizardConfig[employee.employeeStatus] || defaultSteps).map(
    (label, index) => ({
      step: index + 1,
      label,
    }),
  );

  const currentStepLabel = steps[currentStep - 1]?.label || "";

  const totalSteps = steps.length;

  const nextStep = () => {
    if (currentStep < totalSteps) setCurrentStep((s) => s + 1);
  };

  const prevStep = () => {
    if (currentStep > 1) setCurrentStep((s) => s - 1);
  };

  const fileInputRef = useRef(null);
  const [fileName, setFileName] = useState(
    "Click to upload or drag a file here PDF, JPG or PNG · max 5MB",
  );

  const handleImageClick = () => {
    fileInputRef.current.click();
  };

  if (role !== "superuser") {
    return <div>Access Denied. Only super users can access this page.</div>;
  }

  const onFileChange = async (e) => {
    const file = e.target.files[0];
    if (!file) return;
    setFileName(file.name);
    const url = await uploadImage(file);
    if (url) {
      setEmployee((prev) => ({ ...prev, profileImage: url }));
    }
  };

  const handleSave = async () => {
    const errors = validateEmployee(employee);
    setFormErrors(errors);
    if (Object.keys(errors).length > 0) return;

    try {
      setLoading(true);

      const payload = {
        title: employee.title,
        name: employee.name,
        surname: employee.surname,
        nationality: employee.nationality,
        gender: employee.gender,
        contactNumber: employee.contactNumber,
        taxNumber: employee.taxNumber,
        email: employee.email,
        idNumber: employee.idNumber ?? "",
        passportNumber: employee.passportNumber ?? "",
        physicalAddress: employee.physicalAddress,
        city: employee.city,
        zipCode: employee.zipCode,
        hasDisability: employee.disability,
        disabilityDescription: employee.disabilityType,
        dateOfBirth: employee.dateOfBirth,
        startDate: employee.startDate,
        branch: employee.branch,
        monthlySalary: employee.monthlySalary
          ? parseFloat(employee.monthlySalary)
          : 0,
        positionId: employee.jobTitle ? parseInt(employee.jobTitle) : null,
        employmentStatus: employee.employeeStatus,
        careerManagerID: employee.reportsTo || null,
        profileImage: employee.profileImage,
        bankName: employee.bankName,
        accountNumber: employee.accountNumber,
        branchCode: employee.branchCode,
        accountType: employee.accountType,
        paymentMethod: employee.paymentMethod,
        accountHolderName: employee.accountHolderName,
        reference: employee.reference,
        payFrequency: employee.payFrequency,
        payDate: employee.payDate,
      };

      if (employee.idType === "id") {
        payload.idNumber = employee.idNumber ?? "";
        payload.passportNumber = "";
      } else {
        payload.passportNumber = employee.passportNumber ?? "";
        payload.idNumber = "";
      }

      await addEmployee(payload);
      toast.success("Employee created successfully!");
      closeModal();
    } catch (error) {
      if (error.response && error.response.data?.errors) {
        setFormErrors(error.response.data.errors);
      } else {
        toast.error("Failed to create employee.");
      }
      console.error("Add employee error response data:", error.response?.data);
      console.error("Add employee error status:", error.response?.status);
    } finally {
      setLoading(false);
    }
  };

  const formatCurrency = (value) => {
    if (!value) return "";
    return new Intl.NumberFormat("en-ZA", {
      style: "currency",
      currency: "ZAR",
      minimumFractionDigits: 2,
    }).format(value);
  };

  if (dataLoading) return <div>Loading...</div>;

  return (
    // Outer shell — always full width, vertical layout
    <div className="emp-modal-shell">
      {/* ── HEADER — always visible ── */}
      <div className="emp-headings-container">
        <div className="emp-left-icon-wrapper">
          <UserRoundPlus size={24} />
        </div>
        <span className="emp-title-wrapper emp-center-logo-text">
          New Employee
        </span>
        <div className="emp-right-icon-wrapper">
          <X size={24} onClick={closeModal} style={{ cursor: "pointer" }} />
        </div>
      </div>

      {/* ── WIZARD — always visible ── */}
      <div className="emp-wizard-container">
        <div className="emp-wizard-frame">
          {steps.map(({ step, label }, index, arr) => (
            <React.Fragment key={step}>
              <div className="emp-step-wrapper">
                <div
                  className={
                    currentStep >= step
                      ? "emp-step-active"
                      : "emp-step-inactive"
                  }
                >
                  {step}
                </div>
                <span className="emp-wizard-step">{label}</span>
              </div>
              {index < arr.length - 1 && <div className="emp-line-step" />}
            </React.Fragment>
          ))}
        </div>
      </div>

      {/* ── STEP CONTENT ── */}
      <div className={`emp-center-frame${currentStep >= 2 ? " step-2" : ""}`}>
        {/* LEFT FRAME — step 1 only */}
        <div className="emp-left-frame">
          {currentStep === 1 && (
            <div className="emp-name-surname-container">
              <div className="emp-form-grid">
                <div className="emp-personal-details-heading">
                  <span>Personal Details</span>
                </div>
                <div className="emp-personal-details-sub">
                  <span>Employee identity information</span>
                </div>

                {/* Title */}
                <div className="emp-full-width dropdown-wrapper emp-input-wrapper">
                  <select
                    className={`emp-name-input ${formErrors.title ? "emp-error-input" : ""}`}
                    value={employee.title}
                    onChange={onInputChange}
                    name="title"
                  >
                    <option value="">Title</option>
                    {titles.map((t) => (
                      <option key={t} value={t}>
                        {t}
                      </option>
                    ))}
                  </select>
                  {formErrors.title && (
                    <span className="emp-error-message">
                      {formErrors.title}
                    </span>
                  )}
                  <img
                    src="/images/arrow_drop_down_circle.png"
                    alt="Dropdown icon"
                    className="dropdown-icon"
                  />
                </div>

                {/* First Name | Last Name */}
                <div className="emp-two-col">
                  <div className="emp-input-wrapper">
                    <input
                      type="text"
                      placeholder="First Name"
                      className={`emp-name-input ${formErrors.name ? "emp-error-input" : ""}`}
                      name="name"
                      value={employee.name}
                      onChange={onInputChange}
                    />
                    {formErrors.name && (
                      <span className="emp-error-message">
                        {formErrors.name}
                      </span>
                    )}
                  </div>
                  <div className="emp-input-wrapper">
                    <input
                      type="text"
                      placeholder="Last Name"
                      className={`emp-name-input ${formErrors.surname ? "emp-error-input" : ""}`}
                      name="surname"
                      value={employee.surname}
                      onChange={onInputChange}
                    />
                    {formErrors.surname && (
                      <span className="emp-error-message">
                        {formErrors.surname}
                      </span>
                    )}
                  </div>
                </div>

                {/* ID Type | ID / Passport Number */}
                <div className="emp-two-col">
                  <div className="emp-input-wrapper dropdown-wrapper">
                    <select
                      className={`emp-name-input ${formErrors.idType ? "emp-error-input" : ""}`}
                      name="idType"
                      value={employee.idType}
                      onChange={onInputChange}
                    >
                      <option value="id">ID Number</option>
                      <option value="passport">Passport Number</option>
                    </select>
                    <img
                      src="/images/arrow_drop_down_circle.png"
                      alt="Dropdown icon"
                      className="dropdown-icon"
                    />
                  </div>
                  <div className="emp-input-wrapper">
                    <input
                      type="text"
                      className={`emp-name-input ${formErrors.idNumber ? "emp-error-input" : ""}`}
                      name={
                        employee.idType === "passport"
                          ? "passportNumber"
                          : "idNumber"
                      }
                      value={
                        employee.idType === "passport"
                          ? employee.passportNumber
                          : employee.idNumber
                      }
                      onChange={onInputChange}
                      placeholder={
                        employee.idType === "passport"
                          ? "Passport Number"
                          : "ID Number"
                      }
                    />
                    {formErrors.idNumber && employee.idType === "id" && (
                      <span className="emp-error-message">
                        {formErrors.idNumber}
                      </span>
                    )}
                    {formErrors.passportNumber &&
                      employee.idType === "passport" && (
                        <span className="emp-error-message">
                          {formErrors.passportNumber}
                        </span>
                      )}
                  </div>
                </div>

                {/* Nationality */}
                <div className="emp-full-width emp-input-wrapper">
                  <input
                    type="text"
                    placeholder="Nationality"
                    className={`emp-name-input ${formErrors.nationality ? "emp-error-input" : ""}`}
                    name="nationality"
                    value={employee.nationality}
                    onChange={onInputChange}
                    disabled={employee.idType === "id"}
                  />
                  {formErrors.nationality && (
                    <span className="emp-error-message">
                      {formErrors.nationality}
                    </span>
                  )}
                </div>

                {/* DOB | Gender */}
                <div className="emp-two-col">
                  <div className="emp-input-wrapper">
                    <div className="date-wrapper">
                      <label className="date-label">DOB</label>
                      <input
                        type="date"
                        name="dateOfBirth"
                        value={employee.dateOfBirth}
                        onChange={onInputChange}
                        className={`emp-name-input ${formErrors.dateOfBirth ? "emp-error-input" : ""}`}
                        disabled={employee.idType === "id"}
                      />
                      {formErrors.dateOfBirth && (
                        <span className="emp-error-message">
                          {formErrors.dateOfBirth}
                        </span>
                      )}
                      <img
                        src="/images/calendar-range.svg"
                        alt="Calendar icon"
                        className="dropdown-icon"
                      />
                    </div>
                  </div>
                  <div className="emp-input-wrapper dropdown-wrapper">
                    <select
                      name="gender"
                      className="emp-name-input"
                      value={employee.gender}
                      onChange={onInputChange}
                      disabled={employee.idType === "id"}
                    >
                      <option value="">Gender</option>
                      {genders.map((g) => (
                        <option key={g} value={g.toLowerCase()}>
                          {g}
                        </option>
                      ))}
                    </select>
                    {formErrors.gender && (
                      <span className="emp-error-message">
                        {formErrors.gender}
                      </span>
                    )}
                    <img
                      src="/images/arrow_drop_down_circle.png"
                      alt="Dropdown icon"
                      className="dropdown-icon"
                    />
                  </div>
                </div>

                {/* Disability */}
                <div className="emp-disability-wrapper">
                  <span className="emp-disability-label">Disability:</span>
                  <div className="emp-disability-row">
                    <label className="emp-radio-option">
                      <input
                        type="radio"
                        name="disability"
                        value="yes"
                        checked={employee.disability === true}
                        onChange={onInputChange}
                      />
                      Yes
                    </label>
                    <label className="emp-radio-option">
                      <input
                        type="radio"
                        name="disability"
                        value="no"
                        checked={employee.disability === false}
                        onChange={onInputChange}
                      />
                      No
                    </label>
                    {employee.disability && (
                      <input
                        type="text"
                        name="disabilityType"
                        className={`emp-disability-input ${formErrors.disabilityType ? "emp-error-input" : ""}`}
                        placeholder="Enter disability"
                        value={employee.disabilityType}
                        onChange={onInputChange}
                      />
                    )}
                  </div>
                  {formErrors.disabilityType && (
                    <span className="emp-error-message">
                      {formErrors.disabilityType}
                    </span>
                  )}
                </div>

                {/* Contact Number */}
                <div className="emp-full-width emp-input-wrapper">
                  <input
                    type="text"
                    placeholder="Contact Number"
                    className={`emp-name-input ${formErrors.contactNumber ? "emp-error-input" : ""}`}
                    name="contactNumber"
                    value={employee.contactNumber}
                    onChange={onInputChange}
                  />
                  {formErrors.contactNumber && (
                    <span className="emp-error-message">
                      {formErrors.contactNumber}
                    </span>
                  )}
                </div>

                {/* Email */}
                <div className="emp-full-width emp-input-wrapper">
                  <input
                    type="email"
                    placeholder="Email Address"
                    className={`emp-name-input ${formErrors.email ? "emp-error-input" : ""}`}
                    name="email"
                    value={employee.email}
                    onChange={onInputChange}
                  />
                  {formErrors.email && (
                    <span className="emp-error-message">
                      {formErrors.email}
                    </span>
                  )}
                </div>

                {/* Home Address */}
                <div className="emp-full-width emp-input-wrapper">
                  <input
                    type="text"
                    placeholder="Home Address"
                    className={`emp-name-input ${formErrors.physicalAddress ? "emp-error-input" : ""}`}
                    name="physicalAddress"
                    value={employee.physicalAddress}
                    onChange={onInputChange}
                  />
                  {formErrors.physicalAddress && (
                    <span className="emp-error-message">
                      {formErrors.physicalAddress}
                    </span>
                  )}
                </div>

                {/* City | Postal Code */}
                <div className="emp-two-col">
                  <div className="emp-input-wrapper">
                    <input
                      type="text"
                      placeholder="City"
                      className={`emp-name-input ${formErrors.city ? "emp-error-input" : ""}`}
                      name="city"
                      value={employee.city}
                      onChange={onInputChange}
                    />
                    {formErrors.city && (
                      <span className="emp-error-message">
                        {formErrors.city}
                      </span>
                    )}
                  </div>
                  <div className="emp-input-wrapper">
                    <input
                      type="text"
                      placeholder="Postal Code"
                      name="zipCode"
                      className={`emp-name-input ${formErrors.zipCode ? "emp-error-input" : ""}`}
                      value={employee.zipCode}
                      onChange={onInputChange}
                    />
                    {formErrors.zipCode && (
                      <span className="emp-error-message">
                        {formErrors.zipCode}
                      </span>
                    )}
                  </div>
                </div>
              </div>
            </div>
          )}
        </div>

        {/* RIGHT FRAME */}
        <div className="emp-right-frame">
          <div className="emp-right-form-container">
            <div className="emp-right-frame-content">
              {/* Step 1 — right side: employment fields */}
              {currentStep === 1 && (
                <div className="emp-name-surname-container">
                  <div className="emp-personal-details-container" />

                  {/* Start Date */}
                  <div className="emp-form-group emp-input-wrapper">
                    <div className="date-wrapper">
                      <label className="date-label">Start Date</label>

                      <input
                        type="date"
                        id="startDate"
                        name="startDate"
                        value={employee.startDate}
                        onChange={onInputChange}
                        className={`emp-start-date ${formErrors.startDate ? "emp-error-input" : ""}`}
                      />
                      {formErrors.startDate && (
                        <span className="emp-error-message">
                          {formErrors.startDate}
                        </span>
                      )}
                      <img
                        src="/images/calendar-range.svg"
                        alt="Dropdown icon"
                        className="dropdown-icon"
                      />
                    </div>
                  </div>

                  {/* Branch */}
                  <div className="emp-full-width dropdown-wrapper emp-input-wrapper">
                    <select
                      className={`emp-name-input ${formErrors.branch ? "emp-error-input" : ""}`}
                      value={employee.branch}
                      onChange={onInputChange}
                      name="branch"
                    >
                      <option value="">Department</option>
                      {branches.map((b) => (
                        <option key={b} value={b}>
                          {b}
                        </option>
                      ))}
                    </select>
                    {formErrors.branch && (
                      <span className="emp-error-message">
                        {formErrors.branch}
                      </span>
                    )}
                    <img
                      src="/images/arrow_drop_down_circle.png"
                      alt="Dropdown icon"
                      className="dropdown-icon"
                    />
                  </div>

                  {/* Monthly Salary */}
                  <div className="emp-full-width emp-input-wrapper">
                    <input
                      type="text"
                      placeholder="Monthly Salary"
                      className={`emp-name-input ${formErrors.monthlySalary ? "emp-error-input" : ""}`}
                      name="monthlySalary"
                      value={employee.monthlySalary}
                      onChange={(e) => {
                        let rawValue = e.target.value.replace(/[^0-9.]/g, "");
                        const parts = rawValue.split(".");
                        if (parts.length > 2)
                          rawValue = parts[0] + "." + parts[1];
                        setEmployee((prev) => ({
                          ...prev,
                          monthlySalary: rawValue,
                        }));
                      }}
                      onBlur={(e) => {
                        if (employee.monthlySalary)
                          e.target.value = formatCurrency(
                            employee.monthlySalary,
                          );
                      }}
                      onFocus={(e) => {
                        e.target.value = employee.monthlySalary || "";
                      }}
                    />
                    {formErrors.monthlySalary && (
                      <span className="emp-error-message">
                        {formErrors.monthlySalary}
                      </span>
                    )}
                  </div>

                  {/* Tax Number */}
                  <div className="emp-full-width emp-input-wrapper">
                    <input
                      type="text"
                      placeholder="Tax Number"
                      className={`emp-name-input ${formErrors.taxNumber ? "emp-error-input" : ""}`}
                      name="taxNumber"
                      value={employee.taxNumber}
                      onChange={onInputChange}
                    />
                    {formErrors.taxNumber && (
                      <span className="emp-error-message">
                        {formErrors.taxNumber}
                      </span>
                    )}
                  </div>

                  {/* Job Title */}
                  <div className="emp-full-width dropdown-wrapper emp-input-wrapper">
                    <select
                      name="jobTitle"
                      value={employee.jobTitle}
                      onChange={(e) => {
                        const selectedId = e.target.value;

                        const selectedPosition = positions.find(
                          (p) => String(p.positionId) === String(selectedId),
                        );

                        setEmployee((prev) => ({
                          ...prev,

                          // existing behaviour
                          jobTitle: selectedId,

                          // new behaviour - auto-populate job grade group based on selected position
                          jobGradeGroup:
                            selectedPosition?.jobGradeGroup || null,
                        }));

                        // still call your generic handler if needed
                        onInputChange(e);
                      }}
                      className={`emp-name-input ${formErrors.jobTitle ? "emp-error-input" : ""}`}
                    >
                      <option value="">Select Job Title</option>
                      {positions.map((p) => (
                        <option key={p.positionId} value={p.positionId}>
                          {p.positionTitle}
                        </option>
                      ))}
                    </select>
                    {formErrors.jobTitle && (
                      <span className="emp-error-message">
                        {formErrors.jobTitle}
                      </span>
                    )}
                    <img
                      src="/images/arrow_drop_down_circle.png"
                      alt="Dropdown icon"
                      className="dropdown-icon"
                    />
                  </div>

                  {/* Employment Status */}
                  <div className="emp-full-width dropdown-wrapper emp-input-wrapper">
                    <select
                      className={`emp-name-input ${formErrors.employeeStatus ? "emp-error-input" : ""}`}
                      value={employee.employeeStatus}
                      onChange={onInputChange}
                      name="employeeStatus"
                    >
                      <option value="">Employment Status</option>
                      {employmentStatuses.map((s) => (
                        <option key={s} value={s}>
                          {s}
                        </option>
                      ))}
                    </select>
                    {formErrors.employeeStatus && (
                      <span className="emp-error-message">
                        {formErrors.employeeStatus}
                      </span>
                    )}
                    <img
                      src="/images/arrow_drop_down_circle.png"
                      alt="Dropdown icon"
                      className="dropdown-icon"
                    />
                  </div>

                  {/* Career Manager */}
                  <div className="emp-full-width dropdown-wrapper emp-input-wrapper">
                    <select
                      name="reportsTo"
                      value={employee.reportsTo}
                      onChange={onInputChange}
                      className={`emp-name-input ${formErrors.reportsTo ? "emp-error-input" : ""}`}
                    >
                      <option value="">Select Career Manager</option>
                      {allEmployees.map((emp) => (
                        <option key={emp.employeeId} value={emp.employeeId}>
                          {emp.name} {emp.surname}
                        </option>
                      ))}
                    </select>
                    {formErrors.reportsTo && (
                      <span className="emp-error-message">
                        {formErrors.reportsTo}
                      </span>
                    )}
                    <img
                      src="/images/arrow_drop_down_circle.png"
                      alt="Dropdown icon"
                      className="dropdown-icon"
                    />
                  </div>

                  {/* Profile Image Upload */}
                  <div className="emp-input-wrapper">
                    <div className="emp-upload-wrapper">
                      <span className="upload-label">Attach Profile Image</span>

                      <div className="upload-container">
                        <input
                          type="file"
                          ref={fileInputRef}
                          className={`emp-name-input hidden-file-input ${formErrors.startDate ? "emp-error-input" : ""}`}
                          onChange={onFileChange}
                          name="profileImage"
                          accept="image/*"
                        />

                        <Upload
                          alt="Upload icon"
                          className="upload-icon"
                          onClick={handleImageClick}
                        />

                        <span className="upload-text">
                          {uploading ? "Uploading..." : fileName}
                        </span>
                      </div>
                      {formErrors.profileImage && (
                        <span className="emp-error-message">
                          {formErrors.profileImage}
                        </span>
                      )}
                    </div>
                  </div>
                  {formErrors.general && (
                    <div className="emp-error-message">
                      {formErrors.general}
                    </div>
                  )}

                  {/* Next Button */}
                  <button
                    className="emp-save-button"
                    onClick={() => {
                      if (totalSteps > 1) {
                        setCurrentStep(currentStep + 1);
                      }
                    }}
                    disabled={loading}
                  >
                    Next
                    <ArrowRight size={20} className="save-button-icon" />
                  </button>
                </div>
              )}

              {currentStepLabel === "Banking Details" && (
                <BankingDetailsModal
                  employee={employee}
                  setEmployee={setEmployee}
                  formErrors={formErrors}
                  setFormErrors={setFormErrors}
                  onNext={nextStep}
                  onBack={prevStep}
                />
              )}

              {currentStepLabel === "Leave" && (
                <LeaveTypesModal
                positions={positions} 
                  employee={employee}
                  setEmployee={setEmployee}
                  formErrors={formErrors}
                  setFormErrors={setFormErrors}
                  onNext={nextStep}
                  onBack={prevStep}
                />
              )}

              {currentStepLabel === "Pension" && (
                <PensionFundOptionsModal
                  employee={employee}
                  setEmployee={setEmployee}
                  formErrors={formErrors}
                  setFormErrors={setFormErrors}
                  onNext={nextStep}
                  onBack={prevStep}
                />
              )}

              {
                currentStepLabel === "Preview" && (
                  <PreviewModal
                    employee={employee}
                    onBack={prevStep}
                    onSave={handleSave}
                  />
                )
              }

            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default AddEmployeeModal;
