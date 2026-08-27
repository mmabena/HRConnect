import React, { useState, useEffect } from "react";
import "./BankingDetailsModal.css";
import { toast } from "react-toastify";
import {
  getBankBranchCodes,
  validateBankingDetails,
} from "../../../api/BankingDetail";
import { ArrowRight, ArrowLeft } from "lucide-react";

const BankingDetailsModal = ({
  employee,
  setEmployee,
  formErrors,
  setFormErrors,
  onNext,
  onBack,
}) => {
  const [bankData, setBankData] = useState([]);

  const banks = Array.isArray(bankData) ? bankData : [];

  const accountTypes = ["Savings", "Cheque", "Business"];

  const paymentMethods = ["EFT", "Cash", "Cheque"];

  const payFrequencies = ["Weekly", "Bi-Weekly", "Monthly"];

  const referenceType = ["Salary", "Other"];

  // Handle input changes for banking details
  const handleChange = (e) => {
    const { name, value } = e.target;

    if (name === "bankName") {
      const selectedBank = bankData.find((bank) => bank.bankName === value);

      setEmployee((prev) => ({
        ...prev,
        bankName: value,
        branchCode: selectedBank?.branchCode || "",
        bankBranchCodeId: selectedBank?.bankBranchCodeId || null,
      }));

      return;
    }

    setEmployee((prev) => ({
      ...prev,
      [name]: value,
    }));
  };

  // Validate banking details before proceeding to next step
  const validateBanking = () => {
    const errors = {};

    if (!employee.bankName) errors.bankName = "Bank name is required";

    if (!employee.accountNumber?.trim()) {
      errors.accountNumber = "Account number is required";
    } else if (!/^\d+$/.test(employee.accountNumber)) {
      errors.accountNumber = "Account number must contain only numbers";
    } else if (
      employee.accountNumber.length < 6 ||
      employee.accountNumber.length > 12
    ) {
      errors.accountNumber = "Account number must be between 6 and 12 digits";
    }

    if (!employee.branchCode) errors.branchCode = "Branch code is required";

    if (!employee.accountType) errors.accountType = "Account type is required";

    if (!employee.accountHolderName)
      errors.accountHolderName = "Account holder name is required";

    if (!employee.paymentMethod)
      errors.paymentMethod = "Payment method is required";

    if (!employee.referenceType)
      errors.referenceType = "Reference type is required";

    if (!employee.payFrequency)
      errors.payFrequency = "Pay frequency is required";

    if (!employee.payDate) errors.payDate = "Pay date is required";

    return errors;
  };

  // Fetch bank branch codes on component mount
  useEffect(() => {
    const fetchBankCodes = async () => {
      try {
        const data = await getBankBranchCodes();

        const cleaned = (Array.isArray(data) ? data : data.data || []).map(
          (b) => ({
            bankName: b.bankName,
            branchCode: b.universalCode,
            bankBranchCodeId: b.bankBranchCodeId,
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

  // Auto-populate account holder name based on employee's name and surname
  useEffect(() => {
    if (employee.name && employee.surname) {
      // Split first names into array
      const names = employee.name.trim().split(" ");

      // Get initials
      const initials = names
        .map((name) => name.charAt(0).toUpperCase())
        .join(" ");

      // Build full account holder name
      const fullName =
        `${employee.title || ""} ${initials} ${employee.surname}`.trim();

      setEmployee((prev) => ({
        ...prev,
        accountHolderName: fullName,
      }));
    }
  }, [employee.name, employee.surname, employee.title]);

  // Auto-populate pay date to the 26th of the current month
  useEffect(() => {
    if (!employee.payDate) {
      const today = new Date();

      const year = today.getFullYear();
      const month = String(today.getMonth() + 1).padStart(2, "0");
      const day = "26";

      const formattedDate = `${year}-${month}-${day}`;

      setEmployee((prev) => ({
        ...prev,
        payDate: formattedDate,
      }));
    }
  }, [employee.payDate, setEmployee]);

  // Handle clicking the "Next" button
  const handleNext = async () => {
    const errors = validateBanking();

    setFormErrors(errors);

    if (Object.keys(errors).length > 0) {
      toast.error("Please complete all required fields.");
      return;
    }

    const payload = {
      employeeId: "",
      name: employee.accountHolderName,
      surname: employee.surname,
      idNumber: employee.idNumber || "",
      passportNumber: employee.passportNumber || "",
      bankName: employee.bankName,
      bankBranchCodeId: employee.bankBranchCodeId,
      accountNumber: employee.accountNumber,
      accountType: employee.accountType,
      paymentMethod: employee.paymentMethod,
      referenceType: employee.referenceType,
      payFrequency: employee.payFrequency,
    };

    try {
      await validateBankingDetails(payload);

      onNext();
    } catch (error) {
      console.log(error.response?.data);

      if (error.response?.data?.errors) {
        setFormErrors(error.response.data.errors);

        toast.error("Validation failed. Please check the form for errors.");
        return;
      }
      toast.error("Validation failed.");
    }
  };

  return (
    <div className="emp-name-surname-container">
      <div className="emp-banking-form-grid">
        <div className="emp-bank-personal-details-heading">
          <span>Banking Details</span>
        </div>

        <div className="emp-bank-personal-details-sub">
          <span>Salary payment account information</span>
        </div>

        <div className="bank-section-title">Banking details</div>

        {/* BANK */}
        <div className="emp-full-width dropdown-wrapper emp-input-wrapper">
          <div
            className={`emp-select-wrapper ${employee.bankName ? "has-value" : ""}`}
          >
            <select
              name="bankName"
              value={employee.bankName || ""}
              onChange={handleChange}
              className={`emp-bank-name-input ${formErrors.bankName ? "emp-error-input" : ""}`}
            >
              <option value="" disabled hidden>
                Select Bank
              </option>
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

          {formErrors.bankName && (
            <span className="emp-error-message">{formErrors.bankName}</span>
          )}
        </div>

        {/* ACCOUNT + BRANCH */}
        <div className="emp-two-col">
          <div className="emp-input-wrapper">
            <input
              type="text"
              name="accountNumber"
              placeholder="Account Number"
              value={employee.accountNumber || ""}
              onChange={(e) => {
                const value = e.target.value.replace(/\D/g, "");

                setEmployee((prev) => ({
                  ...prev,
                  accountNumber: value,
                }));
              }}
              maxLength={12}
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
              readOnly
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
              name="referenceType"
              value={employee.referenceType || ""}
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
            {formErrors.referenceType && (
              <span className="emp-error-message">
                {formErrors.referenceType}
              </span>
            )}
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

            {formErrors.payFrequency && (
              <span className="emp-error-message">
                {formErrors.payFrequency}
              </span>
            )}
          </div>

          <div className="emp-input-wrapper">
            <div className="date-wrapper">
              <div className="bank-date-wrapper">
                <input
                  type="date"
                  name="payDate"
                  value={employee.payDate || ""}
                  readOnly
                  onChange={handleChange}
                  className="emp-payDate-input"
                />
                <label className="date-label">Pay Date</label>{" "}
                {/* moved after input */}
                <img
                  src="/images/calendar-range.svg"
                  alt="Calendar icon"
                  className="dropdown-icon"
                />
              </div>
              {formErrors.payDate && (
                <span className="emp-error-message">{formErrors.payDate}</span>
              )}
            </div>
          </div>
        </div>

        {/* BUTTONS */}
        <div className="emp-medical-button-row">
          <button className="emp-bank-back-button" onClick={onBack}>
            <ArrowLeft size={20} className="back-save-button-icon" />
            Back
          </button>

          <button className="emp-bank-next-button" onClick={handleNext}>
            Next
            <ArrowRight size={20} className="next-save-button-icon" />
          </button>
        </div>
      </div>
    </div>
  );
};

export default BankingDetailsModal;
