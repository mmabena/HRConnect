import React, { useState, useEffect } from "react";
import "./BankingDetailsModal.css";
import { getBankBranchCodes } from "../../../api/BankingDetail";
import { ArrowRight, ArrowLeft, Upload, UserRoundPlus, X } from "lucide-react";

const BankingDetailsModal = ({
  employee,
  setEmployee,
  formErrors,
  setFormErrors,
  onNext,
  onBack,
}) => {
  const [bankData, setBankData] = useState([]);
  const [currentStep] = useState(2);

  const banks = Array.isArray(bankData) ? bankData : [];

  const accountTypes = ["Savings", "Cheque", "Business"];

  const paymentMethods = ["EFT", "Cash", "Cheque"];

  const payFrequencies = ["Weekly", "Bi-Weekly", "Monthly"];

  const referenceType = ["Salary", "Bonus", "Other"];

  const handleChange = (e) => {
    const { name, value } = e.target;

    if (name === "bankName") {
      const selectedBank = bankData.find((bank) => bank.bankName === value);

      const branchCode =
        selectedBank?.branchCode || selectedBank?.BranchCode || "";

      setEmployee((prev) => ({
        ...prev,
        bankName: value,
        branchCode: selectedBank?.branchCode || "",
      }));

      return;
    }

    setEmployee((prev) => ({
      ...prev,
      [name]: value,
    }));
  };

  const validateBanking = () => {
    const errors = {};

    if (!employee.bankName) errors.bankName = "Bank is required";

    if (!employee.accountNumber)
      errors.accountNumber = "Account number is required";

    if (!employee.branchCode) errors.branchCode = "Branch code is required";

    if (!employee.accountType) errors.accountType = "Account type is required";

    if (!employee.accountHolderName)
      errors.accountHolderName = "Account holder name is required";

    if (!employee.paymentMethod)
      errors.paymentMethod = "Payment method is required";

    return errors;
  };

  useEffect(() => {
    const fetchBankCodes = async () => {
      try {
        const data = await getBankBranchCodes();

        const cleaned = (Array.isArray(data) ? data : data.data || []).map(
          (b) => ({
            bankName: b.bankName,
            branchCode: b.universalCode,
          }),
        );

        console.log("Clean bank data:", cleaned);

        setBankData(cleaned);
      } catch (err) {
        console.error("Failed to load bank branch codes", err);
      }
    };

    fetchBankCodes();
  }, []);

  const handleNext = () => {
    const errors = validateBanking();

    setFormErrors(errors);

    if (Object.keys(errors).length > 0) return;

    onNext();
  };

  return (
    <div className="emp-name-surname-container">
      <div className="emp-banking-form-grid">
        <div className="emp-personal-details-heading">
          <span>Banking Details</span>
        </div>

        <div className="emp-personal-details-sub">
          <span>Salary payment account information</span>
        </div>

        {/* BANK */}
        <div className="emp-full-width dropdown-wrapper emp-input-wrapper">
          <div className="emp-select-wrapper">
            <select
              name="bankName"
              value={employee.bankName || ""}
              onChange={handleChange}
              className={`emp-bank-name-input ${
                formErrors.bankName ? "emp-error-input" : ""
              }`}
            >
              <option value="">Select Bank</option>

              {banks.map((bank, index) => (
                <option key={index} value={bank.bankName}>
                  {bank.bankName}
                </option>
              ))}
            </select>

            <img
              src="/images/arrow_drop_down_circle.png"
              alt="Dropdown icon"
              className="dropdown-icon"
            />
          </div>
        </div>

        {/* ACCOUNT + BRANCH */}
        <div className="emp-two-col">
          <div className="emp-input-wrapper">
            <input
              type="text"
              name="accountNumber"
              placeholder="Account Number"
              value={employee.accountNumber || ""}
              onChange={handleChange}
              className={`emp-account-name-input ${
                formErrors.accountNumber ? "emp-error-input" : ""
              }`}
            />

            {formErrors.accountNumber && (
              <span className="emp-error-message">
                {formErrors.accountNumber}
              </span>
            )}
          </div>

          <div className="emp-input-wrapper">
            <input
              type="text"
              name="branchCode"
              placeholder="Branch Code"
              value={employee.branchCode ?? ""}
              readOnly
              className={`emp-branch-name-input ${
                formErrors.branchCode ? "emp-error-input" : ""
              }`}
            />

            {formErrors.branchCode && (
              <span className="emp-error-message">{formErrors.branchCode}</span>
            )}
          </div>
        </div>

        {/* TYPE + PAYMENT */}
        <div className="emp-two-col">
          <div className="emp-input-wrapper dropdown-wrapper">
            <select
              name="accountType"
              value={employee.accountType || ""}
              onChange={handleChange}
              className={`emp-accountType-input ${
                formErrors.accountType ? "emp-error-input" : ""
              }`}
            >
              <option value="">Account Type</option>

              {accountTypes.map((t) => (
                <option key={t} value={t}>
                  {t}
                </option>
              ))}
            </select>

            {formErrors.accountType && (
              <span className="emp-error-message">
                {formErrors.accountType}
              </span>
            )}

            <img
              src="/images/arrow_drop_down_circle.png"
              alt="Dropdown icon"
              className="dropdown-icon"
            />
          </div>

          <div className="emp-input-wrapper dropdown-wrapper">
            <select
              name="paymentMethod"
              value={employee.paymentMethod || ""}
              onChange={handleChange}
              className={`emp-paymentMethod-input ${
                formErrors.paymentMethod ? "emp-error-input" : ""
              }`}
            >
              <option value="">Payment Method</option>

              {paymentMethods.map((p) => (
                <option key={p} value={p}>
                  {p}
                </option>
              ))}
            </select>

            {formErrors.paymentMethod && (
              <span className="emp-error-message">
                {formErrors.paymentMethod}
              </span>
            )}

            <img
              src="/images/arrow_drop_down_circle.png"
              alt="Dropdown icon"
              className="dropdown-icon"
            />
          </div>
        </div>

        {/* HOLDER + REF */}
        <div className="emp-two-col">
          <div className="emp-input-wrapper">
            <input
              type="text"
              name="accountHolderName"
              placeholder="Account Holder Name"
              value={employee.accountHolderName || ""}
              onChange={handleChange}
              className={`emp-accountHolder-input ${
                formErrors.accountHolderName ? "emp-error-input" : ""
              }`}
            />

            {formErrors.accountHolderName && (
              <span className="emp-error-message">
                {formErrors.accountHolderName}
              </span>
            )}
          </div>

          <div className="emp-input-wrapper dropdown-wrapper">
            <select
              name="reference"
              value={employee.reference || ""}
              onChange={handleChange}
              className="emp-referenceType-input"
            >
              <option value="">Reference Type</option>

              {referenceType.map((r) => (
                <option key={r} value={r}>
                  {r}
                </option>
              ))}
            </select>

            <img
              src="/images/arrow_drop_down_circle.png"
              alt="Dropdown icon"
              className="dropdown-icon"
            />
          </div>
        </div>

        {/* FREQUENCY + DATE */}
        <div className="emp-two-col">
          <div className="emp-input-wrapper dropdown-wrapper">
            <select
              name="payFrequency"
              value={employee.payFrequency || ""}
              onChange={handleChange}
              className="emp-payFrequency-input"
            >
              <option value="">Pay Frequency</option>

              {payFrequencies.map((f) => (
                <option key={f} value={f}>
                  {f}
                </option>
              ))}
            </select>

            <img
              src="/images/arrow_drop_down_circle.png"
              alt="Dropdown icon"
              className="dropdown-icon"
            />
          </div>

       <div className="emp-input-wrapper">
          <div className="date-wrapper">
            <label className="date-label">Pay Date</label>

            <input
              type="date"
              name="payDate"
              value={employee.payDate || ""}
              onChange={handleChange}
              className="emp-payDate-input"
            />

            <img
              src="/images/calendar-range.svg"
              alt="Calendar icon"
              className="dropdown-icon"
            />
          </div>
        </div>

        </div>

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

export default BankingDetailsModal;
