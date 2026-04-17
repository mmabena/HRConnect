import React, { useState } from "react";
import "./BankingDetailsModal.css";
import { ArrowRight, Upload, UserRoundPlus, X } from "lucide-react";


const BankingDetailsModal = ({ employee, setEmployee, formErrors, setFormErrors, onNext, onBack }) => {
  
  const banks = ["FNB", "Standard Bank", "ABSA", "Nedbank", "Capitec"];
  const accountTypes = ["Savings", "Cheque", "Business"];
  const paymentMethods = ["EFT", "Cash", "Cheque"];
  const payFrequencies = ["Weekly", "Bi-Weekly", "Monthly"];

  const handleChange = (e) => {
    const { name, value } = e.target;

    setEmployee((prev) => ({
      ...prev,
      [name]: value,
    }));
  };

  const validateBanking = () => {
    const errors = {};

    if (!employee.bankName) errors.bankName = "Bank is required";
    if (!employee.accountNumber) errors.accountNumber = "Account number is required";
    if (!employee.branchCode) errors.branchCode = "Branch code is required";
    if (!employee.accountType) errors.accountType = "Account type is required";
    if (!employee.accountHolderName) errors.accountHolderName = "Account holder name is required";
    if (!employee.paymentMethod) errors.paymentMethod = "Payment method is required";

    return errors;
  };

  const handleNext = () => {
    const errors = validateBanking();
    setFormErrors(errors);

    if (Object.keys(errors).length > 0) return;

    onNext(); // move to Leave step
  };

return (
    <div className="emp-center-frame">
      <div className="emp-left-frame">
        <div className="emp-left-frame-centered">
          <div className="emp-headings-container">
            <div className="emp-left-icon-wrapper">
              <UserRoundPlus size={24} />
            </div> 
            <span className="emp-title-wrapper emp-center-logo-text">
              New Employee
            </span>
            <div className="emp-right-icon-wrapper">
              <X size={24} />
            </div>
          </div>
          <div className="emp-wizard-container">
            <div className="emp-wizard-frame">
              <div className="emp-step-wrapper">
                <div className="emp-step-active">1</div>
                <span className="emp-wizard-step">Personal Details</span>
              </div>

              <div className="emp-line-step"></div>

              <div className="emp-step-wrapper">
                <div className="emp-step-inactive">2</div>
                <span className="emp-wizard-step">Banking Details</span>
              </div>

              <div className="emp-line-step"></div>
              <div className="emp-step-wrapper">
                <div className="emp-step-inactive">3</div>
                <span className="emp-wizard-step">Leave</span>
              </div>

              <div className="emp-line-step"></div>

              <div className="emp-step-wrapper">
                <div className="emp-step-inactive">4</div>
                <span className="emp-wizard-step">Pension</span>
              </div>

              <div className="emp-line-step"></div>

              <div className="emp-step-wrapper">
                <div className="emp-step-inactive">5</div>
                <span className="emp-wizard-step">Medical Aid</span>
              </div>
              <div className="emp-line-step"></div>

              <div className="emp-step-wrapper">
                <div className="emp-step-inactive">6</div>
                <span className="emp-wizard-step">Preview</span>
              </div>
            </div>
          </div>
        </div>

        <div className="emp-personal-details-heading">
          <span>Banking Details</span>
        </div>

        <div className="emp-personal-details-sub">
          <span>Salary payment account</span>
        </div>

        {/* Bank */}
        <div className="emp-full-width dropdown-wrapper emp-input-wrapper">
          <select
            name="bankName"
            value={employee.bankName || ""}
            onChange={handleChange}
            className={`emp-name-input ${formErrors.bankName ? "emp-error-input" : ""}`}
          >
            <option value="">Select Bank</option>
            {banks.map((b) => (
              <option key={b} value={b}>{b}</option>
            ))}
          </select>
          {formErrors.bankName && <span className="emp-error-message">{formErrors.bankName}</span>}
        </div>

        {/* Account Number + Branch Code */}
        <div className="emp-two-col">
          <div className="emp-input-wrapper">
            <input
              type="text"
              name="accountNumber"
              placeholder="Account Number"
              value={employee.accountNumber || ""}
              onChange={handleChange}
              className={`emp-name-input ${formErrors.accountNumber ? "emp-error-input" : ""}`}
            />
            {formErrors.accountNumber && <span className="emp-error-message">{formErrors.accountNumber}</span>}
          </div>

          <div className="emp-input-wrapper">
            <input
              type="text"
              name="branchCode"
              placeholder="Branch Code"
              value={employee.branchCode || ""}
              onChange={handleChange}
              className={`emp-name-input ${formErrors.branchCode ? "emp-error-input" : ""}`}
            />
            {formErrors.branchCode && <span className="emp-error-message">{formErrors.branchCode}</span>}
          </div>
        </div>

        {/* Account Type + Payment Method */}
        <div className="emp-two-col">
          <div className="emp-input-wrapper dropdown-wrapper">
            <select
              name="accountType"
              value={employee.accountType || ""}
              onChange={handleChange}
              className={`emp-name-input ${formErrors.accountType ? "emp-error-input" : ""}`}
            >
              <option value="">Account Type</option>
              {accountTypes.map((t) => (
                <option key={t} value={t}>{t}</option>
              ))}
            </select>
            {formErrors.accountType && <span className="emp-error-message">{formErrors.accountType}</span>}
          </div>

          <div className="emp-input-wrapper dropdown-wrapper">
            <select
              name="paymentMethod"
              value={employee.paymentMethod || ""}
              onChange={handleChange}
              className={`emp-name-input ${formErrors.paymentMethod ? "emp-error-input" : ""}`}
            >
              <option value="">Payment Method</option>
              {paymentMethods.map((p) => (
                <option key={p} value={p}>{p}</option>
              ))}
            </select>
            {formErrors.paymentMethod && <span className="emp-error-message">{formErrors.paymentMethod}</span>}
          </div>
        </div>

        {/* Account Holder + Reference */}
        <div className="emp-two-col">
          <div className="emp-input-wrapper">
            <input
              type="text"
              name="accountHolderName"
              placeholder="Account Holder Name"
              value={employee.accountHolderName || ""}
              onChange={handleChange}
              className={`emp-name-input ${formErrors.accountHolderName ? "emp-error-input" : ""}`}
            />
            {formErrors.accountHolderName && <span className="emp-error-message">{formErrors.accountHolderName}</span>}
          </div>

          <div className="emp-input-wrapper">
            <input
              type="text"
              name="reference"
              placeholder="Reference"
              value={employee.reference || ""}
              onChange={handleChange}
              className="emp-name-input"
            />
          </div>
        </div>

        {/* Pay Frequency + Pay Date */}
        <div className="emp-two-col">
          <div className="emp-input-wrapper dropdown-wrapper">
            <select
              name="payFrequency"
              value={employee.payFrequency || ""}
              onChange={handleChange}
              className="emp-name-input"
            >
              <option value="">Pay Frequency</option>
              {payFrequencies.map((f) => (
                <option key={f} value={f}>{f}</option>
              ))}
            </select>
          </div>

          <div className="emp-input-wrapper">
            <input
              type="date"
              name="payDate"
              value={employee.payDate || ""}
              onChange={handleChange}
              className="emp-name-input"
            />
          </div>
        </div>

        {/* Buttons */}
        <div className="emp-button-row">
          <button className="emp-back-button" onClick={onBack}>
            Back
          </button>

          <button className="emp-save-button" onClick={handleNext}>
            Next
          </button>
        </div>

      </div>
    </div>
  );
};

export default BankingDetailsModal;