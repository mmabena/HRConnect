import React, { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { toast } from "react-toastify";
import "../Components/AddEmployeeModal.css";
import axios from "axios";

import {
  addEmployee,
  validateRequiredFields,
  handleFileChange,
  fetchAllEmployees,
  populateFromIdNumber,
} from "../Employee";

/// </summary>
/// MOCK super user role
/// </summary>

const getCurrentUserRole = () => "superuser";

const AddEmployeeModal = ({ closeModal }) => {
  const navigate = useNavigate();
  const [userRole, setUserRole] = useState(null);
  const [formErrors, setFormErrors] = useState({});
  const [positions, setPositions] = useState([]);
  const [uploading, setUploading] = useState(false);
  const [errorMessage, setErrorMessage] = useState("");
  const [loading, setLoading] = useState(false);
  const [allEmployees, setAllEmployees] = useState([]);
  const titles = ["Mr", "Mrs", "Ms", "Dr", "Prof"];
  const genders = ["Male", "Female"];
  const branches = ["Johannesburg", "CapeTown", "UK"];
  const employmentStatuses = ["Permanent", "FixedTerm", "Contract"];
  const [employee, setEmployee] = useState({
    title: "",
    name: "",
    surname: "",
    idType: "id",
    idNumber: "",
    passportNumber: "",
    nationality: "",
    gender: "",
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
    documentPath: "",
  });

  useEffect(() => {
    setUserRole(getCurrentUserRole());
    const fetchData = async () => {
      try {
        const employeesData = await fetchAllEmployees();
        setAllEmployees(employeesData);

        const positionsRes = await axios.get(
          "http://localhost:5147/api/positions",
        );
        setPositions(positionsRes.data);
      } catch (err) {
        console.error(err);
      }
    };
    fetchData();
  }, []);

  if (userRole !== "superuser") {
    return <div>Access Denied. Only super users can access this page.</div>;
  }

  const onInputChange = (e) => {
    const { name, value } = e.target;

    setFormErrors((prev) => ({ ...prev, [name]: null }));

    // Handle disability
    if (name === "disability") {
      setEmployee((prev) => ({
        ...prev,
        disability: value === "yes",
        disabilityType: value === "no" ? "" : prev.disabilityType,
      }));
      return;
    }

    // Numeric validation
    if (
      ["contactNumber", "zipCode", "monthlySalary", "taxNumber"].includes(name)
    ) {
      // allow empty string
      const regex = name === "contactNumber" ? /^[\d+-]*$/ : /^\d*$/;
      if (value === "" || regex.test(value)) {
        setFormErrors((prev) => ({ ...prev, [name]: null }));
        setEmployee((prev) => ({ ...prev, [name]: value }));
      } else {
        setFormErrors((prev) => ({ ...prev, [name]: "Invalid characters" }));
      }
      return;
    }

    // ID type change
    if (name === "idType") {
      setEmployee((prev) => ({
        ...prev,
        idType: value,
        idNumber: "",
        dateOfBirth: "",
        nationality: value === "id" ? "" : "Non-South African",
        gender: "",
      }));
      return;
    }

    // ID number -> derive DOB/gender/nationality for SA IDs
    if (name === "idNumber" && employee.idType === "id") {
      setEmployee((prev) => {
        const derived = populateFromIdNumber(value);
        return {
          ...prev,
          idNumber: value,
          dateOfBirth: derived.dateOfBirth || "",
          gender: derived.gender || "",
          nationality: derived.nationality || "",
          citizenship: derived.citizenship || "",
        };
      });
      return;
    }

    setEmployee((prev) => ({ ...prev, [name]: value }));
  };

  const onFileChange = (e) =>
    handleFileChange(e, setEmployee, setUploading, setErrorMessage);

  const formatDateForBackend = (dateStr) => {
    if (!dateStr) return null;
    const d = new Date(dateStr);
    const yyyy = d.getFullYear();
    const mm = String(d.getMonth() + 1).padStart(2, "0");
    const dd = String(d.getDate()).padStart(2, "0");
    return `${yyyy}-${mm}-${dd}`;
  };

  const handleSave = async () => {
    try {
      setLoading(true);

      const { errors: requiredErrors, isValid } =
        validateRequiredFields(employee);

      setFormErrors(requiredErrors);

      if (!isValid) {
        setLoading(false);
        return;
      }

      const payload = {
        employeeId: "",
        title: employee.title || null,
        name: employee.name || null,
        surname: employee.surname || null,
        nationality: employee.nationality || null,
        gender: employee.gender
          ? employee.gender.charAt(0).toUpperCase() + employee.gender.slice(1)
          : null,
        contactNumber: employee.contactNumber,
        taxNumber: employee.taxNumber || null,
        email: employee.email || null,
        physicalAddress: employee.physicalAddress || null,
        city: employee.city || null,
        zipCode: employee.zipCode || null,
        hasDisability: employee.disability || false,
        disabilityDescription: employee.disabilityType || null,
        dateOfBirth: employee.dateOfBirth || null,
        startDate: employee.startDate || null,
        branch: employee.branch || null,
        monthlySalary: employee.monthlySalary
          ? parseFloat(employee.monthlySalary)
          : 0,

        positionId: employee.jobTitle ? parseInt(employee.jobTitle) : null,

        employmentStatus: employee.employeeStatus || null,
        careerManagerID: employee.reportsTo || null,

        profileImage: employee.documentPath || null,
      };

      if (employee.idType === "id") {
        payload.idNumber = employee.idNumber;
      } else {
        payload.passportNumber = employee.idNumber;
      }

      await addEmployee(payload);

      toast.success("Employee created successfully!");
      closeModal();
    } catch (error) {
      console.error("Error saving employee:", error);

      // Optionally, if the API returns validation errors
      if (error.response?.data?.errors) {
        setFormErrors((prevErrors) => ({
          ...prevErrors,
          ...error.response.data.errors,
        }));
      } else {
        toast.error("Failed to create employee.");
      }
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="emp-center-frame">
      <div className="emp-left-frame">
        <div className="emp-left-frame-centered">
          <div className="emp-headings-container">
            <div className="emp-center-logo">
              <span className="emp-center-logo-text-bold">singular</span>
              <span className="emp-center-logo-text-light">express</span>
            </div>
            <h1 className="emp-new-employee-title">New Employee</h1>
          </div>
        </div>

        <div className="emp-personal-details-container">
          <div className="emp-personal-details-heading">
            <span>Personal</span> <span>Details</span>
          </div>
        </div>

        <div className="emp-name-surname-container">
          <div className="emp-form-grid">
            {/* Title */}
            <div className="emp-full-width dropdown-wrapper">
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
                <span className="emp-error-message">{formErrors.title}</span>
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
                  className={`emp-name-input-col ${formErrors.name ? "emp-error-input" : ""}`}
                  name="name"
                  value={employee.name}
                  onChange={onInputChange}
                />
                {formErrors.name && (
                  <span className="emp-error-message">{formErrors.name}</span>
                )}
              </div>
              <div className="emp-input-wrapper">
                <input
                  type="text"
                  placeholder="Last Name"
                  className={`emp-name-input-col ${formErrors.surname ? "emp-error-input" : ""}`}
                  name="surname"
                  value={employee.surname}
                  onChange={onInputChange}
                />
                {formErrors.lastNsurnameame && (
                  <span className="emp-error-message">
                    {formErrors.surname}
                  </span>
                )}
              </div>
            </div>

            {/* ID Type | ID Number */}
            <div className="emp-two-col">
              <div className="emp-input-wrapper dropdown-wrapper">
                <select
                  className={`emp-name-input-col ${formErrors.idType ? "emp-error-input" : ""}`}
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
                  className={`emp-name-input-col ${formErrors.idNumber ? "emp-error-input" : ""}`}
                  name="idNumber"
                  value={employee.idNumber}
                  onChange={onInputChange}
                  placeholder={
                    employee.idType === "passport"
                      ? "Passport Number"
                      : "ID Number"
                  }
                />
                {formErrors.idNumber && (
                  <span className="emp-error-message">
                    {formErrors.idNumber}
                  </span>
                )}
              </div>
            </div>

            {/* Nationality */}
            <div className="emp-full-width">
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
                <input
                  className="emp-name-input-col"
                  type="date"
                  placeholder="Date of Birth"
                  name="dateOfBirth"
                  value={employee.dateOfBirth}
                  onChange={onInputChange}
                  disabled={employee.idType === "id"}
                />
                {formErrors.dateOfBirth && (
                  <span className="emp-error-message">
                    {formErrors.dateOfBirth}
                  </span>
                )}
              </div>
              <div className="emp-input-wrapper dropdown-wrapper">
                <select
                  name="gender"
                  className="emp-name-input-col"
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
                  <span className="emp-error-message">{formErrors.gender}</span>
                )}
                <img
                src="/images/arrow_drop_down_circle.png"
                alt="Dropdown icon"
                className="dropdown-icon"
              />
              </div>
            </div>

            {/* Disability (radio buttons) */}
           <div className="emp-two-col">
          <div className="emp-disability-row">    
                <span className="emp-disability-label">Disability:</span>
 
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
                    placeholder="Describe disability"
                    value={employee.disabilityType}
                    onChange={onInputChange}
                    className="emp-disability-input"
                  />
                )}
              </div>
            </div>

            {/* Contact Number */}
            <div className="emp-full-width">
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
            <div className="emp-full-width">
              <input
                type="email"
                placeholder="Email Address"
                className={`emp-name-input ${formErrors.email ? "emp-error-input" : ""}`}
                name="email"
                value={employee.email}
                onChange={onInputChange}
              />
              {formErrors.email && (
                <span className="emp-error-message">{formErrors.email}</span>
              )}
            </div>
            {/* Home Address */}
            <div className="emp-input-wrapper full-width">
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
                  className={`emp-name-input-col ${formErrors.city ? "emp-error-input" : ""}`}
                  name="city"
                  value={employee.city}
                  onChange={onInputChange}
                />
                {formErrors.city && (
                  <span className="emp-error-message">{formErrors.city}</span>
                )}
              </div>
              <div className="emp-input-wrapper">
                <input
                  type="text"
                  placeholder="Postal Code"
                  name="zipCode"
                  className={`emp-name-input-col ${formErrors.zipCode ? "emp-error-input" : ""}`}
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
      </div>

      {/* Right frame */}
      <div className="emp-right-frame">
        <div className="emp-right-form-container">
          <div className="emp-right-frame-content">
            <div className="emp-name-surname-container">
              <div className="emp-personal-details-container"></div>
              <div className="emp-form-group">
                {/* Start Date */}
                <input
                  type="date"
                  className={`emp-name-input ${formErrors.startDate ? "emp-error-input" : ""}`}
                  placeholder="Employee Start Date"
                  value={employee.startDate}
                  onChange={onInputChange}
                  name="startDate"
                />

                {formErrors.startDate && (
                  <span className="emp-error-message">
                    {formErrors.startDate}
                  </span>
                )}
              </div>
              {/* Branch */}
              <div className="emp-full-width dropdown-wrapper">
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
                  <span className="emp-error-message">{formErrors.branch}</span>
                )}
                <img
                  src="/images/arrow_drop_down_circle.png"
                  alt="Dropdown icon"
                  className="dropdown-icon"
                />
              </div>
              {/* Monthly Salary */}
              <div className="emp-input-wrapper full-width">
                <input
                  type="number"
                  min="0"
                  step="100"
                  placeholder="Monthly Salary"
                  className={`emp-name-input ${formErrors.monthlySalary ? "emp-error-input" : ""}`}
                  name="monthlySalary"
                  value={employee.monthlySalary}
                  onChange={onInputChange}
                />
                {formErrors.monthlySalary && (
                  <span className="emp-error-message">
                    {formErrors.monthlySalary}
                  </span>
                )}
              </div>
              {/* Tax Number */}
              <div className="emp-full-width">
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
              <div className="emp-full-width dropdown-wrapper">
                <select
                  name="jobTitle"
                  value={employee.jobTitle}
                  onChange={onInputChange}
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
              <div className="emp-full-width dropdown-wrapper">
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
              <div className="emp-full-width dropdown-wrapper">
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

              <div className="emp-full-width dropdown-wrapper">
                <input
                  type="file"
                  className={`emp-name-input ${formErrors.documentPath ? "emp-error-input" : ""}`}
                  onChange={onFileChange}
                  name="documentPath"
                />
                {formErrors.documentPath && (
                  <span className="emp-error-message">
                    {formErrors.documentPath}
                  </span>
                )}
                <img
                src="/images/arrow_upload_ready.png"
                alt="Dropdown icon"
                className="dropdown-icon"
              />
              </div>

              {/* Save Button */}
              {formErrors.general && (
                <div className="emp-error-message">{formErrors.general}</div>
              )}
              <button
                className="emp-save-button"
                onClick={handleSave}
                disabled={loading}
              >
                {loading ? "Saving..." : "Save"}
              </button>

              <div className="emp-right-frame-bottom">
                <p className="emp-right-frame-bottom-text">
                  <span className="emp-align-right">
                    Privacy Policy | Terms & Conditions
                  </span>
                  <br />
                  <span className="emp-align-left">
                    Copyright © 2025 Singular Systems. All rights reserved.
                  </span>
                </p>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default AddEmployeeModal;
