import React, { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { toast } from "react-toastify";
import "../Components/AddEmployeeModal.css";
import axios from "axios";

import {
  addEmployee,
  validateRequiredFields,
  validateEmail,
  handleFileChange,
  handleInputChange,
  fetchAllEmployees,
  toISOStringSafe,
  populateFromIdNumber,
  showConfirmationToast,
  convertDDMMYYYYtoISO,
  GetEmployeeByEmployeeNumberAsync,
  validateIdNumber,
  generateEmployeeNumber,
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

  const [idType, setIdType] = useState("ID"); // "ID" or "Passport"
  const [idValue, setIdValue] = useState("");
  const [dob, setDob] = useState("");
  const [gender, setGender] = useState("");
  const [nationality, setNationality] = useState("");
  const [employeesList, setEmployeesList] = useState([]);
  const [loading, setLoading] = useState(false);
  const [idNumberError, setIdNumberError] = useState("");
  const [touched, setTouched] = useState({});
  const [reportsToOptions, setReportsToOptions] = useState([]);
  const [allEmployees, setAllEmployees] = useState([]);

  const titles = ["Mr", "Mrs", "Ms", "Dr", "Prof"];
  const genders = ["Male", "Female"];
  const branches = ["Johannesburg", "CapeTown", "UK"];
  const employmentStatuses = ["Permanent", "FixedTerm", "Contract"];

  const [employee, setEmployee] = useState({
    title: "",
    firstName: "",
    lastName: "",
    idType: "id",
    idNumber: "",
    passportNumber: "",
    nationality: "",
    gender: "",
    contactNumber: "",
    taxNumber: "",
    email: "",
    homeAddress: "",
    city: "",
    postalCode: "",
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
      ["contactNumber", "postalCode", "monthlySalary", "taxNumber"].includes(
        name,
      )
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

      const token = localStorage.getItem("token");

      const payload = {
        employeeId: "",

        title: employee.title || null,
        name: employee.firstName || null,
        surname: employee.lastName || null,

        nationality: employee.nationality || null,
        gender: employee.gender
          ? employee.gender.charAt(0).toUpperCase() + employee.gender.slice(1)
          : null,

        contactNumber: employee.contactNumber || null,
        taxNumber: employee.taxNumber || null,
        email: employee.email || null,

        physicalAddress: employee.homeAddress || null,
        city: employee.city || null,
        zipCode: employee.postalCode || null,

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

      console.log("Sending employee payload:", payload);

      const response = await fetch("http://localhost:5147/api/employee", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${token}`,
        },
        body: JSON.stringify(payload),
      });

      if (!response.ok) {
        const errorData = await response.json();
        if (errorData.errors) {
          setFormErrors(errorData.errors);
        } else if (errorData.message) {
          const message = errorData.message;

          const newErrors = {};

          if (message.includes("Email")) newErrors.email = message;
          else if (message.includes("Contact number"))
            newErrors.contactNumber = message;
          else if (message.includes("ID number")) newErrors.idNumber = message;
          else if (message.includes("Tax Number")) newErrors.taxNumber = message;
          else if (message.includes("Start date")) newErrors.startDate = message;
          else if (message.includes("Position")) newErrors.jobTitle = message;
          else if (message.includes("Branch")) newErrors.branch = message;
          else if (message.includes("Monthly salary"))
            newErrors.monthlySalary = message;
          else newErrors.general = message;

          setFormErrors(newErrors);
        }
        throw new Error("Validation failed");
      }

      await response.json();

      toast.success("Employee created successfully!");
      closeModal();
    } catch (error) {
      console.error("Error saving employee:", error);
      toast.error("Failed to create employee.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="center-frame">
      <div className="left-frame">
        <div className="left-frame-centered">
          <div className="headings-container">
            <div className="center-logo">
              <span className="center-logo-text-bold">singular</span>
              <span className="center-logo-text-light">express</span>
            </div>
            <h1 className="new-employee-title">New Employee</h1>
          </div>
        </div>

        <div className="personal-details-container">
          <div className="personal-details-heading">
            <span>Personal</span> <span>Details</span>
          </div>
        </div>

        <div className="name-surname-container">
          <div className="form-grid">
            {/* Title */}
            <div className="full-width">
              <select
                className="name-input"
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
            </div>
            {/* First Name | Last Name */}
            <div className="two-col">
              <input
                type="text"
                placeholder="Full Name"
                className="name-input-col"
                name="firstName"
                value={employee.firstName}
                onChange={onInputChange}
              />
              <input
                type="text"
                placeholder="Last Name"
                className="name-input-col"
                name="lastName"
                value={employee.lastName}
                onChange={onInputChange}
              />
            </div>

            {/* ID Type | ID Number */}
            <div className="two-col">
              <select
                className="name-input-col"
                name="idType"
                value={employee.idType}
                onChange={onInputChange}
              >
                <option value="id">ID Number</option>
                <option value="passport">Passport Number</option>
              </select>

              <input
                type="text"
                className="name-input-col"
                name="idNumber"
                value={employee.idNumber}
                onChange={onInputChange}
                placeholder={
                  employee.idType === "passport"
                    ? "Passport Number"
                    : "ID Number"
                }
              />
            </div>

            {/* Nationality */}
            <div className="full-width">
              <input
                type="text"
                name="nationality"
                placeholder="Nationality"
                value={employee.nationality}
                onChange={onInputChange}
                disabled={employee.idType === "id"}
                className="name-input"
              />
            </div>

            {/* DOB | Gender */}
            <div className="two-col">
              <input
                className="name-input-col"
                type="date"
                name="dateOfBirth"
                value={employee.dateOfBirth}
                onChange={onInputChange}
                disabled={employee.idType === "id"}
              />
              <select
                name="gender"
                className="name-input-col"
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
            </div>

            {/* Disability (radio buttons) */}
            <div className="two-col">
              <div className="disability-row">
                <span className="disability-label">Disability:</span>

                <label className="radio-option">
                  <input
                    type="radio"
                    name="disability"
                    value="yes"
                    checked={employee.disability === true}
                    onChange={onInputChange}
                  />
                  Yes
                </label>

                <label className="radio-option">
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
                    className="disability-input"
                  />
                )}
              </div>
            </div>

            {/* Contact Number */}
            <div className="full-width">
              <input
                type="text"
                placeholder="Contact Number"
                className="name-input"
                name="contactNumber"
                value={employee.contactNumber}
                onChange={onInputChange}
              />
            </div>

            {/* Email */}
            <div className="full-width">
              <input
                type="email"
                placeholder="Email Address"
                className="name-input"
                name="email"
                value={employee.email}
                onChange={onInputChange}
              />
              {formErrors.email && <span className="error-message">{formErrors.email}</span>}
            </div>
            {/* Home Address */}
            <div className="full-width">
              <input
                type="text"
                placeholder="Home Address"
                className="name-input"
                name="homeAddress"
                value={employee.homeAddress}
                onChange={onInputChange}
              />
            </div>

            {/* City | Postal Code */}
            <div className="two-col">
              <input
                type="text"
                placeholder="City"
                className="name-input-col"
                name="city"
                value={employee.city}
                onChange={onInputChange}
              />
              <input
                type="text"
                placeholder="Postal Code"
                name="postalCode"
                className="name-input-col"
                value={employee.postalCode}
                onChange={onInputChange}
              />
            </div>
          </div>
        </div>
      </div>

      {/* Right frame */}
      <div className="right-frame">
        <div className="right-form-container">
          <div className="right-frame-content">
            <div className="name-surname-container">
              <div className="personal-details-container"></div>
              <div className="form-group">
                {/* Start Date */}
                <input
                  type="date"
                  className="name-input"
                  placeholder="Employee Start Date"
                  value={employee.startDate}
                  onChange={onInputChange}
                  name="startDate"
                />
              </div>
              {/* Branch */}
              <div className="full-width">
                <select
                  className="name-input"
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
                <img
                  src="/images/arrow_drop_down_circle.png"
                  alt="Dropdown icon"
                  className="dropdown-icon"
                />
              </div>
              {/* Monthly Salary */}
              <div className="full-width">
                <input
                  type="text"
                  placeholder="Monthly Salary"
                  className="name-input"
                  name="monthlySalary"
                  value={employee.monthlySalary}
                  onChange={onInputChange}
                />
              </div>
              {/* Tax Number */}
              <div className="full-width">
                <input
                  type="text"
                  placeholder="Tax Number"
                  className="name-input"
                  name="taxNumber"
                  value={employee.taxNumber}
                  onChange={onInputChange}
                />
              </div>
              {/* Job Title */}
              <div className="full-width">
                <select
                  name="jobTitle"
                  value={employee.jobTitle}
                  onChange={onInputChange}
                  className="name-input"
                >
                  <option value="">Select Job Title</option>
                  {positions.map((p) => (
                    <option key={p.positionId} value={p.positionId}>
                      {p.positionTitle}
                    </option>
                  ))}
                </select>
              </div>
              {/* Employment Status */}
              <div className="full-width">
                <select
                  className="name-input"
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
              </div>
              {/* Career Manager */}
              <div className="full-width">
                <select
                  name="reportsTo"
                  value={employee.reportsTo}
                  onChange={onInputChange}
                  className="name-input"
                >
                  <option value="">Select Career Manager</option>
                  {allEmployees.map((emp) => (
                    <option key={emp.employeeId} value={emp.employeeId}>
                      {emp.name} {emp.surname}
                    </option>
                  ))}
                </select>
              </div>

              <div className="full-width">
                <input
                  type="file"
                  className="name-input"
                  onChange={onFileChange}
                  name="documentPath"
                />
              </div>

              {/* Save Button */}
              <button
                className="save-button"
                onClick={handleSave}
                disabled={loading}
              >
                {loading ? "Saving..." : "Save"}
              </button>

              <div className="right-frame-bottom">
                <p className="right-frame-bottom-text">
                  <span className="align-right">
                    Privacy Policy | Terms & Conditions
                  </span>
                  <br />
                  <span className="align-left">
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
