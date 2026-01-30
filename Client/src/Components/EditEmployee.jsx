import React, { useState, useEffect } from "react";
import { useLocation, useParams } from "react-router-dom";
import "../Navy.css";
import {
  editEmployee,
  formatDateForDisplay,
  toISOStringSafe,
  showConfirmationToast,
  GetEmployeeByEmployeeNumberAsync,
} from "../Employee";

import { toast } from "react-toastify";
/// </summary>
/// MOCK Super user Role
/// </summary>

const getCurrentUserRole = () => {
  return "superuser";
};

const EditEmployee = () => {
  const location = useLocation();
  const readOnly = location.state?.readOnly || false;
  const { employeeNumber } = useParams();

  const [activeTab, setActiveTab] = useState("Personal");
  const [isEditable, setIsEditable] = useState(false);
  const [userRole, setUserRole] = useState(null);
  const [loading, setLoading] = useState(false);
  const [formErrors, setFormErrors] = useState({});

  /// </summary>
  /// Track original Employee number and DOB loaded from DB
  /// </summary>

const [originalEmployeeNumber, setOriginalEmployeeNumber] = useState("");
const [originalIdNumber, setOriginalIdNumber] = useState("");

  const [originalDateOfBirth, setOriginalDateOfBirth] = useState("");


  const [employeeData, setEmployeeData] = useState({
    employeeNumber: "",
    firstName: "",
    lastName: "",
    maidenName: "",
    title: "",
    dateOfBirth: "",
    initials: "",
    idType: "id",
    idNumber: "",
    preferredName: "",
    gender: "",
    middleName: "",
    contactNumber: "",
    nationality: "",
    citizenship: "",
    disability: false,
    disabilityType: "",
    email: "",
    maritalStatus: "",
    homeAddress: "",
    city: "",
    postalCode: "",
    startDate: "",
    department: "",
    jobTitle: "",
    employeeStatus: "",
    reportsTo: "",
    documentPath: "",
  });

  const getInitials = (firstName, middleName, lastName) => {
    let initials = "";

    if (firstName) initials += firstName.charAt(0).toUpperCase();

    if (middleName && middleName.trim().toLowerCase() !== "n/a") {
      initials += middleName.charAt(0).toUpperCase();
    }

    if (lastName) initials += lastName.charAt(0).toUpperCase();

    return initials;
  };
  /// </summary>
  /// Load user role and employee data when component mounts or location.state changes - set loading true at the start for all cases
  /// </summary>
  useEffect(() => {
    setLoading(true);

    const role = getCurrentUserRole();
    setUserRole(role);

    const loadEmployeeIfNeeded = async () => {
      if (!location.state && employeeNumber) {
        try {
          const employee = await GetEmployeeByEmployeeNumberAsync(
            employeeNumber
          );
          console.log("Fetched employee:", employee);

          const transformed = {
            ...employee,
            dateOfBirth: employee.dateOfBirth
              ? formatDateForDisplay(employee.dateOfBirth)
              : "",
            initials: getInitials(
              employee.firstName,
              employee.middleName,
              employee.lastName
            ),
          };

          setEmployeeData(transformed);
          setOriginalEmployeeNumber(employee.employeeNumber);
          setOriginalDateOfBirth(employee.dateOfBirth);
          setOriginalIdNumber(employee.idNumber);
        } catch (error) {
          console.error("Failed to load employee", error);
          toast.error("Could not load employee data.");
        } finally {
          setLoading(false);
        }
      } else if (location.state) {
        setEmployeeData(location.state);
        setOriginalEmployeeNumber(location.state.employeeNumber ?? "");
        setOriginalDateOfBirth(location.state.dateOfBirth ?? "");
        setOriginalIdNumber(location.state.idNumber ?? "");
        setLoading(false);
      }
    };

    loadEmployeeIfNeeded();
  }, [location.state, employeeNumber]);
  useEffect(() => {
  console.log("Current employee data being viewed/edited:", employeeData);
}, [employeeData]);


  useEffect(() => {
    const initials = getInitials(
      employeeData.firstName,
      employeeData.middleName,
      employeeData.lastName
    );
    if (employeeData.initials !== initials) {
      setEmployeeData((prev) => ({
        ...prev,
        initials,
      }));
    }
}, [employeeData.firstName, employeeData.middleName, employeeData.lastName, employeeData.initials]);

useEffect(() => {
  const gender = employeeData.gender?.trim().toLowerCase() || "";
  const maritalStatus = employeeData.maritalStatus?.trim().toLowerCase() || "";
  const maidenName = employeeData.maidenName?.trim() || "";

  if (gender === "female" && maritalStatus === "married") {
    if (!maidenName || maidenName.toLowerCase() === "n/a") {
      setFormErrors((prev) => ({
        ...prev,
        maidenName: "Maiden Name is required for married women.",
      }));
    } else {
      setFormErrors((prev) => {
        const { maidenName, ...rest } = prev;
        return rest;
      });
    }
  } else {
  /// </summary>
  /// Only overwrite maidenName if it is empty and editable
  /// </summary>
   
    if (!maidenName || maidenName.toLowerCase() === "n/a") {
      setEmployeeData((prev) => {
        if (!prev.maidenName || prev.maidenName.toLowerCase() === "n/a") {
          return { ...prev, maidenName: "N/A" };
        }
        return prev; 
      });
    }
  /// </summary>
  /// Clear any maidenName form errors
  /// </summary>
   
    setFormErrors((prev) => {
      const { maidenName, ...rest } = prev;
      return rest;
    });
  }
}, [employeeData.gender, employeeData.maritalStatus, employeeData.maidenName]);



  /// </summary>
  /// Disability validation & styling logic
  /// </summary>
  useEffect(() => {
    if (employeeData.disability) {
      if (
        !employeeData.disabilityType ||
        employeeData.disabilityType === "N/A"
      ) {
        setFormErrors((prev) => ({
          ...prev,
          disabilityType:
            "Disability Type is required when Disability is 'Yes'.",
        }));
      } else {
        setFormErrors((prev) => {
          const { disabilityType, ...rest } = prev;
          return rest;
        });
      }
    } else {
      setFormErrors((prev) => {
        const { disabilityType, ...rest } = prev;
        return rest;
      });
      if (employeeData.disabilityType !== "N/A") {
        setEmployeeData((prev) => ({
          ...prev,
          disabilityType: "N/A",
        }));
      }
    }
  }, [employeeData.disability, employeeData.disabilityType]);

  useEffect(() => {
    setEmployeeData((prev) => {
      const updated = { ...prev };

      if (isEditable) {
        if (updated.maidenName === "N/A") {
          updated.maidenName = "";
        }
        /// </summary>
        /// Clear disabilityType if editable and disability is true and disabilityType is "N/A"
        /// </summary>
        if (updated.disability && updated.disabilityType === "N/A") {
          updated.disabilityType = "";
        }
      } else {
        /// </summary>
        /// Reset disabilityType to "N/A" if not editable and disability is false or disabilityType is empty
        /// </summary>
        if (!updated.disability && !updated.disabilityType) {
          updated.disabilityType = "N/A";
        }
      }

      return updated;
    });
  }, [isEditable, employeeData.disability]);

  useEffect(() => {
    if (!employeeData.disability && employeeData.disabilityType !== "N/A") {
      setEmployeeData((prev) => ({
        ...prev,
        disabilityType: "N/A",
      }));
    }
  }, [employeeData.disability, employeeData.disabilityType]);

  if (userRole !== "superuser") {
    return (
      <div style={{ padding: 20, color: "red" }}>
        Access Denied. Only super users can access this page.
      </div>
    );
  }

  const handleEditSaveClick = async () => {
    if (!isEditable) {
      setIsEditable(true);
      return;
    }

    const confirmed = await showConfirmationToast(
      "Are you sure you want to save changes?"
    );
    if (!confirmed) {
      setIsEditable(false);
      return;
    }
    /// </summary>
    /// Prevent saving if validation errors exist
    /// </summary>
    if (Object.keys(formErrors).length > 0) {
      toast.error("Please fix validation errors before saving.");
      return;
    }

    const idNumberTrimmed = employeeData.idNumber.trim();

    const payload = {
      ...employeeData,
      idNumber: idNumberTrimmed,
      dateOfBirth: originalDateOfBirth,

      startDate: toISOStringSafe(employeeData.startDate),
    };

    try {
      setLoading(true);
      await editEmployee(payload.employeeNumber, payload);

      toast.success("Employee updated successfully!");
      setIsEditable(false);
      setOriginalIdNumber(payload.idNumber);
      setOriginalDateOfBirth(payload.dateOfBirth);
    } catch (error) {
      const responseData = error.response?.data;

      if (responseData) {
        const generalMessage = responseData.message || "Validation failed";
        const errors = responseData.errors;

        setFormErrors({});

        toast.error(generalMessage);

        if (Array.isArray(errors)) {
          const errorMap = {};
          errors.forEach(({ field, message }) => {
            errorMap[field] = message;
            toast.error(`${field}: ${message}`);
          });
          setFormErrors(errorMap);
        } else if (typeof errors === "object" && errors !== null) {
          setFormErrors(errors);
          Object.entries(errors).forEach(([field, message]) => {
            toast.error(`${field}: ${message}`);
          });
        }
      } else {
        /// </summary>
        /// If there is no structured response from server
        /// </summary>

        toast.error("Could not update employee. Please try again.");
      }
    } finally {
      setLoading(false);
    }
  };
  const handleInputChange = (e) => {
    const { id, value, type, checked } = e.target;

    if (["email", "dateOfBirth", "idNumber"].includes(id)) {
      return;
    }

    setEmployeeData((prevData) => {
      const updatedData = {
        ...prevData,
        [id]: type === "checkbox" ? checked : value,
      };

      if (id === "disability") {
        updatedData.disability = value === "yes";
        if (value !== "yes") {
          updatedData.disabilityType = "N/A";
        }
      }

      return updatedData;
    });
  };

  return (
    <div className="edit-employee-background">
      

      <div className="edit-employee-heading-row">
        {[
          "Personal",
          "Career",
          "Leave",
          "Tax Profile",
          "Payroll",
          "Documents",
        ].map((tab) => (
          <div
            key={tab}
            className={`heading-item ${activeTab === tab ? "selected" : ""}`}
            onClick={() => setActiveTab(tab)}
          >
            {tab}
          </div>
        ))}
      </div>

      <div className="edit-button-container">
        {!readOnly && (
          <button
            className="em-edit-button"
            onClick={handleEditSaveClick}
            disabled={
              loading || (isEditable && Object.keys(formErrors).length > 0)
            }
          >
            {isEditable ? "Save" : "Edit Profile"}
          </button>
        )}
      </div>

      <div className="edit-employee-top-container">
        <div className="photo-block">
          <img
            src={employeeData.documentPath || "/default-profile.png"}
            alt="Employee"
          />
        </div>
        <div className="photo-text-container">
          <div className="title">{`${employeeData.firstName} ${employeeData.lastName}`}</div>
          <div className="subtitle">{employeeData.jobTitle}</div>
        </div>
      </div>

      <div className="edit-employee-form-container">
        <div className="custom-header">Personal Information</div>

        <div className="sub-container">
          <div className="fields-container">
            {[
              ["Employee Number*", "employeeNumber"],
              ["Title", "title"],
              ["Initials", "initials"],
              ["ID Number*", "idNumber"],
              ["Nationality*", "nationality"],
              ["Citizenship*", "citizenship"],
            ].map(([label, id]) => (
              <div className="field" key={id}>
                <label className="field-label" htmlFor={id}>
                  {label}
                </label>
                <input
                  className="field-input"
                  id={id}
                  type="text"
                  value={employeeData[id]}
                  onChange={handleInputChange}
                  readOnly={id === "idNumber" ? true : !isEditable}
                />
              </div>
            ))}
          </div>

          <div className="fields-container">
            {[
              ["Marital Status*", "maritalStatus"],
              ["Date of Birth", "dateOfBirth"],
              ["Preferred Name", "preferredName"],
              ["Gender", "gender"],
            ].map(([label, id]) => (
              <div className="field" key={id}>
                <label className="field-label" htmlFor={id}>
                  {label}
                </label>
                <input
                  className="field-input"
                  id={id}
                  type="text"
                  value={
                    id === "maritalStatus" && employeeData[id]
                      ? employeeData[id].charAt(0).toUpperCase() +
                        employeeData[id].slice(1)
                      : id === "dateOfBirth" && employeeData[id]
                      ? formatDateForDisplay(employeeData[id])
                      : employeeData[id] || ""
                  }
                  onChange={handleInputChange}
                  readOnly={id === "dateOfBirth" ? true : !isEditable}
                  style={
                    id === "dateOfBirth"
                      ? { backgroundColor: "#C7D9E5" }
                      : undefined
                  }
                />
              </div>
            ))}

            <div className="field">
              <label className="field-label" htmlFor="disability">
                Disability
              </label>
              <select
                className="field-input"
                id="disability"
                value={employeeData.disability ? "yes" : "no"}
                onChange={handleInputChange}
                disabled={!isEditable}
              >
                <option value="no">No</option>
                <option value="yes">Yes</option>
              </select>
            </div>

            <div className="field">
              <label className="field-label" htmlFor="disabilityType">
                Disability Type
              </label>
              <input
                className="field-input"
                id="disabilityType"
                type="text"
                value={employeeData.disabilityType}
                onChange={handleInputChange}
                readOnly={!isEditable || employeeData.disabilityType === "N/A"}
                style={
                  /// </summary>
                  /// Only apply blue background if NOT editable or value is "N/A" and not required
                  /// </summary>
                  (!isEditable || employeeData.disabilityType === "N/A") &&
                  !employeeData.disability
                    ? { backgroundColor: "#C7D9E5" }
                    : {}
                }
              />
            </div>
          </div>

          <div className="fields-container row-3">
            {[
              ["First Name", "firstName"],
              ["Middle Name", "middleName"],
              ["Last Name", "lastName"],
            ].map(([label, id]) => (
              <div className="field" key={id}>
                <label className="field-label" htmlFor={id}>
                  {label}
                </label>
                <input
                  className="field-input"
                  id={id}
                  type="text"
                  value={employeeData[id]}
                  onChange={handleInputChange}
                  readOnly={!isEditable}
                />
              </div>
            ))}
          </div>
          <div className="fields-container row-3">
            {[
              ["Maiden Name", "maidenName"],
              ["Contact Number*", "contactNumber"],
              ["Email", "email"],
            ].map(([label, id]) => (
              <div className="field" key={id}>
                <label className="field-label" htmlFor={id}>
                  {label}
                  {formErrors[id] && <span style={{ color: "red" }}> *</span>}
                </label>

                <input
                  className="field-input"
                  id={id}
                  type={id === "email" ? "email" : "text"}
                  value={employeeData[id]}
                  onChange={handleInputChange}
                  readOnly={id === "email" ? true : !isEditable}
                  style={
                    (id === "maidenName" &&
                      (!isEditable || employeeData.maidenName === "N/A")) ||
                    (id === "disabilityType" &&
                      (!isEditable || employeeData.disabilityType === "N/A"))
                      ? { backgroundColor: "#C7D9E5" }
                      : {}
                  }
                />

                {formErrors[id] && (
                  <div className="error-text" style={{ color: "red" }}>
                    {formErrors[id]}
                  </div>
                )}
              </div>
            ))}
          </div>
        </div>
      </div>
    </div>
  );
};

export default EditEmployee;