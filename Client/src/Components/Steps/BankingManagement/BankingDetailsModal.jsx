import React, { useState, useEffect } from "react";
import "./BankingDetailsModal.css";

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

  const banks = Array.isArray(bankData)
    ? bankData.map((b) => b.bankName)
    : [];

  const accountTypes = ["Savings", "Cheque", "Business"];

  const paymentMethods = ["EFT", "Cash", "Cheque"];

  const payFrequencies = [
    "Weekly",
    "Bi-Weekly",
    "Monthly",
  ];

  const handleChange = (e) => {
    const { name, value } = e.target;

    if (name === "bankName") {
      const selectedBank = bankData.find(
        (b) =>
          b.bankName?.toLowerCase() ===
          value.toLowerCase(),
      );

      setEmployee((prev) => ({
        ...prev,
        bankName: value,
        branchCode:
          selectedBank?.branchCode || "",
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

    if (!employee.bankName)
      errors.bankName = "Bank is required";

    if (!employee.accountNumber)
      errors.accountNumber =
        "Account number is required";

    if (!employee.branchCode)
      errors.branchCode =
        "Branch code is required";

    if (!employee.accountType)
      errors.accountType =
        "Account type is required";

    if (!employee.accountHolderName)
      errors.accountHolderName =
        "Account holder name is required";

    if (!employee.paymentMethod)
      errors.paymentMethod =
        "Payment method is required";

    return errors;
  };

  useEffect(() => {
    const fetchBankCodes = async () => {
      try {
        const res = await fetch(
          "/api/BankingDetails/BankBranchCodes",
        );

        const data = await res.json();

        setBankData(
          Array.isArray(data)
            ? data
            : data.data || [],
        );
      } catch (err) {
        console.error(
          "Failed to load bank branch codes",
          err,
        );
      }
    };

    fetchBankCodes();
  }, []);

  const handleNext = () => {
    const errors = validateBanking();

    setFormErrors(errors);

    if (Object.keys(errors).length > 0)
      return;

    onNext();
  };

return (
  <div className="emp-name-surname-container">
    <div className="emp-form-grid">

      <div className="emp-personal-details-heading">
        <span>Banking Details</span>
      </div>

      <div className="emp-personal-details-sub">
        <span>Salary payment account information</span>
      </div>

      {/* BANK */}
      <div className="emp-full-width dropdown-wrapper emp-input-wrapper">
        <select
          name="bankName"
          value={employee.bankName || ""}
          onChange={handleChange}
          className={`emp-name-input ${
            formErrors.bankName ? "emp-error-input" : ""
          }`}
        >
          <option value="">Select Bank</option>

          {banks.map((b) => (
            <option key={b} value={b}>
              {b}
            </option>
          ))}
        </select>

        {formErrors.bankName && (
          <span className="emp-error-message">
            {formErrors.bankName}
          </span>
        )}

        <img
          src="/images/arrow_drop_down_circle.png"
          alt="Dropdown icon"
          className="dropdown-icon"
        />
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
            className={`emp-name-input ${
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
            value={employee.branchCode || ""}
            readOnly
            className={`emp-name-input ${
              formErrors.branchCode ? "emp-error-input" : ""
            }`}
          />

          {formErrors.branchCode && (
            <span className="emp-error-message">
              {formErrors.branchCode}
            </span>
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
            className={`emp-name-input ${
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
            className={`emp-name-input ${
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
            className={`emp-name-input ${
              formErrors.accountHolderName ? "emp-error-input" : ""
            }`}
          />

          {formErrors.accountHolderName && (
            <span className="emp-error-message">
              {formErrors.accountHolderName}
            </span>
          )}
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

      {/* FREQUENCY + DATE */}
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
              className="emp-name-input"
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