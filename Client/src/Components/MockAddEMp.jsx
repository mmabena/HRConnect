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

  /*useEffect(() => {
    const query = employee.reportsTo.trim().toLowerCase();

    if (query.length < 2) {
      setReportsToOptions([]);
      return;
    }

    const matches = allEmployees
      .filter((emp) =>
        `${emp.firstName} ${emp.lastName}`.toLowerCase().includes(query),
      )
      .map((emp) => `${emp.firstName} ${emp.lastName}`);

    setReportsToOptions(matches);
  }, [employee.reportsTo, allEmployees]);*/

  // useEffect(() => {
  //   console.log("AddEmployee rendered");
  //   console.log("loading:", loading);
  // }, [loading]);

  if (userRole !== "superuser") {
    return <div>Access Denied. Only super users can access this page.</div>;
  }

  const onInputChange = (e) => {
  const { name, value } = e.target;

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
  if (["contactNumber", "postalCode", "monthlySalary", "taxNumber"].includes(name)) {
    if (/\D/.test(value)) {
      setFormErrors((prev) => ({ ...prev, [name]: "Only numbers allowed" }));
    } else {
      setFormErrors((prev) => ({ ...prev, [name]: null }));
      setEmployee((prev) => ({ ...prev, [name]: value }));
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

  // const handleSave = async () => {
  //   const confirmed = await showConfirmationToast(
  //     "Are you sure you want to save changes?",
  //   );
  //   if (!confirmed) return;

  //   const validation = validateRequiredFields(employee);
  //   const emailError = validateEmail(employee.email);
  //   if (emailError) {
  //     validation.errors.email = emailError;
  //     validation.isValid = false;
  //   }
  //   setFormErrors(validation.errors);
  //   if (!validation.isValid) return;

  //   const payload = {
  //     ...employee,
  //     idType: employee.idType === "id" ? "ZA" : "NZA",
  //     startDate: toISOStringSafe(employee.startDate),
  //     dateOfBirth:
  //       employee.idType === "passport"
  //         ? convertDDMMYYYYtoISO(employee.dateOfBirth)
  //         : toISOStringSafe(employee.dateOfBirth),
  //   };

  // const handleInputChange = (e) => {
  //   const { name, value, type, checked } = e.target;

  //   if (name === "disability") {
  //     setEmployee((prev) => ({
  //       ...prev,
  //       disability: value === "yes",
  //       disabilityType: value === "no" ? "" : prev.disabilityType,
  //     }));
  //     return;
  //   }

  //   if (["contactNumber", "postalCode", "monthlySalary", "taxNumber"].includes(name)) {
  //     if (/\D/.test(value)) {
  //       setFormErrors((prev) => ({ ...prev, [name]: "Only numbers allowed" }));
  //     } else {
  //       setFormErrors((prev) => ({ ...prev, [name]: null }));
  //       setEmployee((prev) => ({ ...prev, [name]: value }));
  //     }
  //     return;
  //   }

  //   if (name === "idType") {
  //     setEmployee((prev) => ({
  //       ...prev,
  //       idType: value,
  //       idNumber: "",
  //       passportNumber: "",
  //       nationality: value === "id" ? "" : "Non-South African",
  //       gender: "",
  //     }));
  //     return;
  //   }

  //   if (name === "idNumber") {
  //     setEmployee((prev) => ({
  //       ...prev,
  //       idNumber: value,
  //     }));
  //     return;
  //   }

  //   if (name === "passportNumber") {
  //     setEmployee((prev) => ({
  //       ...prev,
  //       passportNumber: value,
  //     }));
  //     return;
  //   }

  //   setEmployee((prev) => ({ ...prev, [name]: value }));
  // };

  // const handleSave = async () => {
  //   // Simple validation example
  //   if (!employee.name || !employee.surname || !employee.email) {
  //     toast.error("Please fill in required fields");
  //     return;
  //   }

  // // Map frontend state to backend DTO
  // const payload = {
  //   title: employee.title,
  //   name: employee.name,
  //   surname: employee.surname,
  //   idNumber: employee.idType === "id" ? employee.idNumber : "",
  //   passportNumber: employee.idType === "passport" ? employee.idNumber : "",
  //   nationality: employee.nationality,
  //   gender: employee.gender,
  //   contactNumber: employee.contactNumber,
  //   taxNumber: employee.taxNumber,
  //   email: employee.email,
  //   physicalAddress: employee.homeAddress,
  //   city: employee.city,
  //   zipCode: employee.postalCode,
  //   hasDisability: employee.disability,
  //   disabilityDescription: employee.disabilityType,
  //   dateOfBirth:
  //     employee.idType === "passport"
  //       ? convertDDMMYYYYtoISO(employee.startDate)
  //       : toISOStringSafe(employee.startDate),
  //   startDate: toISOStringSafe(employee.startDate),
  //   branch: employee.branch,
  //   monthlySalary: parseFloat(employee.monthlySalary) || 0,
  //   positionId: parseInt(employee.jobTitle) || 0,
  //   employmentStatus: employee.employeeStatus,
  //   careerManagerID: employee.reportsTo,
  //   profileImage: employee.documentPath || "",
  // };

  // const handleSave = async () => {
  //   const confirmed = await showConfirmationToast(
  //     "Are you sure you want to save changes?",
  //   );
  //   if (!confirmed) return;

  //   function parseSAId(idNumber) {
  //     if (idNumber.length !== 13 || !/^\d+$/.test(idNumber)) return null;

  //     const year = idNumber.substring(0, 2);
  //     const month = idNumber.substring(2, 4);
  //     const day = idNumber.substring(4, 6);

  //     const genderDigits = parseInt(idNumber.substring(6, 10));
  //     const citizenship = idNumber.charAt(10);

  //     const fullYear = parseInt(year) > 30 ? "19" + year : "20" + year;

  //     const parsedGender = genderDigits >= 5000 ? "Male" : "Female";
  //     const parsedNationality = citizenship === "0" ? "South African" : "Other";

  //     return {
  //       dob: `${fullYear}-${month}-${day}`,
  //       gender: parsedGender,
  //       nationality: parsedNationality,
  //     };
  //   }

  //   const employeesByPosition = positions.map((pos) => {
  //     return {
  //       positionId: pos.positionId,
  //       positionTitle: pos.positionTitle,
  //       employees: allEmployees.filter(
  //         (emp) => emp.jobTitle === pos.positionId,
  //       ),
  //     };
  //   });

  //   function handleIdInput(value) {
  //     setIdValue(value);

  //     if (idType === "ID") {
  //       const parsed = parseSAId(value);

  //       if (parsed) {
  //         setDob(parsed.dob);
  //         setGender(parsed.gender);
  //         setNationality(parsed.nationality);
  //       }
  //     }
  //   }

  //   const reportsToValid = allEmployees.some(
  //     (emp) =>
  //       `${emp.firstName} ${emp.lastName}`.toLowerCase() ===
  //       employee.reportsTo.toLowerCase(),
  //   );
  //   const dateOfBirthISO =
  //     employee.idType === "passport"
  //       ? convertDDMMYYYYtoISO(employee.dateOfBirth)
  //       : toISOStringSafe(employee.dateOfBirth);

  //   if (!reportsToValid) {
  //     setFormErrors((prev) => ({
  //       ...prev,
  //       reportsTo: "Select a valid reporting manager",
  //     }));
  //     toast.error("Select a valid reporting manager.");
  //     return;
  //   }

  //   /// </summary>
  //   /// Require gender only if idType is 'passport'
  //   /// </summary>
  //   if (employee.idType === "passport" && !employee.gender) {
  //     setFormErrors((prev) => ({
  //       ...prev,
  //       gender: "Gender is required when ID type is passport",
  //     }));
  //     toast.error("Please select a gender.");
  //     return;
  //   }

  //   const validation = validateRequiredFields(employee);
  //   const emailError = validateEmail(employee.email);
  //   if (emailError) {
  //     validation.errors.email = emailError;
  //     validation.isValid = false;
  //   }
  //   setFormErrors(validation.errors);
  //   if (!validation.isValid) return;
  //   const mappedIdType = employee.idType === "id" ? "ZA" : "NZA";

  //   /// </summary>
  //   /// Make gender drop down accessible if idType is passport
  //   /// </summary>

  //   const payload = {
  //     ...employee,
  //     idType: mappedIdType,
  //     disability: employee.disability,
  //     dateOfBirth: dateOfBirthISO,
  //     startDate: toISOStringSafe(employee.startDate),
  //     gender:
  //       employee.idType === "id" || employee.idType === "ZA"
  //         ? employee.gender
  //         : employee.idType === "passport" || employee.idType === "NZA"
  //           ? employee.gender
  //           : null,
  //   };

  //   try {
  //     setLoading(true);
  //     const saved = await addEmployee(payload);
  //     toast.success("Employee saved successfully!");
  //     closeModal();
  //   } catch (error) {
  //     console.error(error);
  //     toast.error("Failed to save employee.");
  //   } finally {
  //     setLoading(false);
  //   }
  // };

  //   try {
  //     setLoading(true);
  //     console.log("Employee details:", payload);
  //     const saved = await addEmployee(payload);

  //     toast.success("Employee saved successfully!");
  //     console.log(employee.email);
  //     console.log(employee.contactNumber);

  //     const freshEmployee = await GetEmployeeByEmployeeNumberAsync(
  //       saved.employeeNumber,
  //     );
  //     closeModal();
  //   } catch (error) {
  //     if (error.response?.data) {
  //       const data = error.response.data;
  //       const generalMessage = data.message || "Validation failed";

  //       setFormErrors({});

  //       toast.error(generalMessage);

  //       if (Array.isArray(data.errors)) {
  //         /// </summary>
  //         /// errors is an array of objects with {field, message}
  //         /// </summary>
  //         const errorsObj = {};
  //         data.errors.forEach(({ field, message }) => {
  //           errorsObj[field] = message;
  //           toast.error(`${field}: ${message}`);
  //         });
  //         setFormErrors(errorsObj);
  //       } else if (typeof data.errors === "object") {
  //         /// </summary>
  //         /// fallback if errors is an object instead of array
  //         /// </summary>

  //         setFormErrors(data.errors);
  //         Object.entries(data.errors).forEach(([field, msg]) =>
  //           toast.error(`${field}: ${msg}`),
  //         );
  //       }
  //     } else {
  //       toast.error("Failed to save employee. Please try again.");
  //     }
  //   } finally {
  //     setLoading(false);
  //   }
  // };

  // function onBlurField(e) {
  //   const { name } = e.target;
  //   setTouched((prev) => ({ ...prev, [name]: true }));
  // }

  // const onInputChange = (e) => {
  //   const { name, value } = e.target;
  //   let newValue = value;

  //   if (name === "disability") {
  //     setEmployee((prev) => ({
  //       ...prev,
  //       disability: value === "yes",
  //       disabilityType: value === "no" ? "" : prev.disabilityType,
  //     }));
  //     return;
  //   }

  //   if (["contactNumber", "postalCode"].includes(name)) {
  //     if (/\D/.test(value)) {
  //       setFormErrors((prev) => ({ ...prev, [name]: "Only numbers allowed" }));
  //       return;
  //     } else {
  //       setFormErrors((prev) => ({ ...prev, [name]: null }));
  //     }
  //   }

  //   if (name === "idType") {
  //     /// </summary>
  //     /// When idType changes, reset fields accordingly
  //     /// </summary>

  //     setEmployee((prev) => ({
  //       ...prev,
  //       idType: value,
  //       idNumber: "",
  //       dateOfBirth: value === "id" ? "" : prev.dateOfBirth,
  //       nationality: value === "id" ? "" : "Non-South African",
  //       citizenship: value === "id" ? "" : "Non-South African",
  //       /// </summary>
  //       /// For passport, keep existing gender or force user to select
  //       /// </summary>

  //       gender: value === "id" ? "" : "",
  //     }));
  //     return;
  //   }

  //   if (name === "idNumber") {
  //     if (value.length <= 13) {
  //       if (employee.idType === "id" && value.length === 13) {
  //         const derived = populateFromIdNumber(value);
  //         setEmployee((prev) => ({
  //           ...prev,
  //           idNumber: value,
  //           dateOfBirth: derived.dateOfBirth || "",
  //           gender: derived.gender || "",
  //           nationality: derived.nationality || "",
  //           citizenship: derived.citizenship || "",
  //         }));
  //       } else {
  //         /// </summary>
  //         /// for passport or idNumber less than 13 for id, just update idNumber
  //         /// </summary>

  //         setEmployee((prev) => ({
  //           ...prev,
  //           idNumber: value,
  //           ...(employee.idType === "id"
  //             ? {
  //                 dateOfBirth: "",
  //                 gender: "",
  //                 nationality: "",
  //                 citizenship: "",
  //               }
  //             : {}),
  //         }));
  //       }
  //     }
  //     return;
  //   }

  //   if (name === "dateOfBirth" && employee.idType === "id") {
  //     return;
  //     /// </summary>
  //     /// dateOfBirth auto derived for id
  //     /// </summary>
  //   }

  //   if (name === "gender") {
  //     /// </summary>
  //     /// Only allow manual gender input when idType is NOT "id"
  //     /// </summary>

  //     if (employee.idType !== "id") {
  //       setEmployee((prev) => ({
  //         ...prev,
  //         gender: value,
  //       }));
  //     }
  //     return;
  //   }

  //   handleInputChange(
  //     e,
  //     employee,
  //     setEmployee,
  //     setIdNumberError,
  //     employeesList,
  //   );
  // };

  // const onFileChange = (e) =>
  //   handleFileChange(e, setEmployee, setUploading, setErrorMessage);

  // const idPlaceholder =
  //   employee.idType === "passport" ? "Passport Number" : "ID Number";

  const handleSave = async () => {
    const confirmed = await showConfirmationToast(
      "Are you sure you want to save changes?",
    );
    if (!confirmed) return;

    // --- Frontend Validation ---
    const validation = validateRequiredFields(employee);

    // Email validation
    const emailErr = validateEmail(employee.email);
    if (emailErr) {
      validation.errors.email = emailErr;
      validation.isValid = false;
    }

    // ID/Passport validation
    const idErr = validateIdNumber(employee.idType, employee.idNumber);
    if (idErr) {
      validation.errors.idNumber = idErr;
      validation.isValid = false;
    }

    // Numeric fields validation
    ["contactNumber", "postalCode", "monthlySalary", "taxNumber"].forEach(
      (field) => {
        if (employee[field] && /\D/.test(employee[field])) {
          validation.errors[field] = "Only numbers allowed";
          validation.isValid = false;
        }
      },
    );

    setFormErrors(validation.errors);
    if (!validation.isValid) {
      toast.error("Please fix validation errors.");
      return;
    }

    // Map frontend state to backend payload
    const payload = {
      ...employee,
      idType: employee.idType === "id" ? "ZA" : "NZA",
      startDate: toISOStringSafe(employee.startDate),
      dateOfBirth:
        employee.idType === "passport"
          ? convertDDMMYYYYtoISO(employee.dateOfBirth)
          : toISOStringSafe(employee.dateOfBirth),
      monthlySalary: parseFloat(employee.monthlySalary) || 0,
    };

    try {
      setLoading(true);
      const saved = await addEmployee(payload);
      toast.success("Employee saved successfully!");
      closeModal();
    } catch (error) {
      console.error(error);

      // Handle backend validation errors
      if (error.response?.data) {
        const data = error.response.data;
        setFormErrors({});

        toast.error(data.message || "Failed to save employee.");

        if (Array.isArray(data.errors)) {
          const errorsObj = {};
          data.errors.forEach(({ field, message }) => {
            errorsObj[field] = message;
            toast.error(`${field}: ${message}`);
          });
          setFormErrors(errorsObj);
        } else if (typeof data.errors === "object") {
          setFormErrors(data.errors);
          Object.entries(data.errors).forEach(([field, msg]) =>
            toast.error(`${field}: ${msg}`),
          );
        }
      } else {
        toast.error("Failed to save employee. Please try again.");
      }
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
                value={employee.email}
                onChange={onInputChange}
              />
            </div>
            {/* Home Address */}
            <div className="full-width">
              <input
                type="text"
                placeholder="Home Address"
                className="name-input"
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
                value={employee.city}
                onChange={onInputChange}
              />
              <input
                type="text"
                placeholder="Postal Code"
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
                  <option value="">Branch</option>
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
                  name="EmployeeStatus"
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
