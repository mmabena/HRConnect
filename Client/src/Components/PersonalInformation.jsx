import React, { useEffect, useState } from "react";
import { useLocation } from "react-router-dom";
import "../Components/EditEmployee.css";
import api from "../api/api.js";
import { toast } from "react-toastify";

const PersonalInformation = () => {
  const location = useLocation();
  const readOnly = location.state?.readOnly || false;
  const [employeeData, setEmployeeData] = useState(null);
  const [loading, setLoading] = useState(true);
  const [activeTab, setActiveTab] = useState("Personal");
  const [isEditable, setIsEditable] = useState(false);
  const [formErrors, setFormErrors] = useState({});

  const [allEmployees, setAllEmployees] = useState([
    { employeeId: "EMP002", name: "John", surname: "Doe" },
    { employeeId: "EMP003", name: "Jane", surname: "Smith" },
  ]);

  const titles = ["Mr", "Mrs", "Ms", "Dr", "Prof"];
  const branches = ["Johannesburg", "Cape Town", "Durban"];
  const employmentStatuses = ["Permanent", "Fixed Term", "Contract"];
  const tabs = ["Personal", "Employment", "Salary", "Other"];

  const getInitials = (name, surname) => {
    let initials = "";
    if (name) initials += name.charAt(0).toUpperCase();
    if (surname) initials += surname.charAt(0).toUpperCase();
    return initials;
  };

useEffect(() => {
  const currentUserRaw = localStorage.getItem("currentUser");

  if (!currentUserRaw) {
    toast.error("User not logged in");
    setLoading(false);
    return;
  }

  const currentUser = JSON.parse(currentUserRaw);

  const transformed = {
    employeeId: currentUser.employeeId || "",
    name: currentUser.name || "",
    surname: currentUser.surname || "",
    title: currentUser.title || "",
    branch: currentUser.branch || "",
    jobTitle: currentUser.jobTitle || "",
    dateOfBirth: currentUser.dateOfBirth || "",
    idNumber: currentUser.idNumber || "",
    passportNumber: currentUser.passportNumber || "",
    nationality: currentUser.nationality || "",
    gender: currentUser.gender || "",
    contactNumber: currentUser.contactNumber || "",
    email: currentUser.email || "",
    physicalAddress: currentUser.physicalAddress || "",
    city: currentUser.city || "",
    zipCode: currentUser.zipCode || "",
    monthlySalary: currentUser.monthlySalary || "",
    employeeStatus: currentUser.employmentStatus || "",
    reportsTo: currentUser.careerManagerID || "",
    disability: currentUser.hasDisability || false,
    disabilityType: currentUser.disabilityDescription || "",
    startDate: currentUser.startDate || "",
    profileImage: currentUser.profileImage || "",
    initials: getInitials(currentUser.name, currentUser.surname),
  };

  setEmployeeData(transformed);
  setLoading(false);
}, []);   

  const handleInputChange = (e) => {
    const { id, value } = e.target;
    let newValue = value;

    // Special handling for disability yes/no
    if (id === "disability") {
      newValue = value === "yes";
    }

    setEmployeeData((prev) => ({ ...prev, [id]: newValue }));
  };

  const handleEditSaveClick = () => {
    if (isEditable) {
      console.log("Saving employee data:", employeeData);
      // Add API save call here
    }
    setIsEditable(!isEditable);
  };

  if (loading) return <div>Loading employee profile...</div>;
  if (!employeeData) return <div>No employee data found</div>;

  // The rest of your JSX can remain exactly the same
  return (
    <div className="emp-menu-background">
      {/* TOP PHOTO SECTION */}
      <div className="emp-edit-employee-top-container">
        <div className="emp-photo-block">
          <img
            src={employeeData.profileImage || "/default-profile.png"}
            alt={`${employeeData.name} ${employeeData.surname}`}
          />
        </div>
        <div className="emp-photo-text-container">
          <div className="emp-title">{`${employeeData.name} ${employeeData.surname}`}</div>
          <div className="emp-subtitle">{employeeData.jobTitle}</div>
          <div className="emp-subsubtitle">{employeeData.branch}</div>
          <div className="emp-edit-employee-heading-row side-tabs">
            {[
              "Personal Information",
              "Payroll Information",
              "Leave",
              "Payroll Tools",
            ].map((tab) => (
              <div
                key={tab}
                className={`emp-heading-item ${activeTab === tab ? "selected" : ""}`}
                onClick={() => setActiveTab(tab)}
              >
                {tab}
              </div>
            ))}
          </div>
        </div>
        <div className="emp-edit-button-top-right">
          {!readOnly && (
            <button
              className="emp-em-edit-button"
              onClick={handleEditSaveClick}
            >
              {isEditable ? "Save" : "Edit Profile"}
            </button>
          )}
        </div>
      </div>

      <div className="emp-edit-employee-form-container">
        <div className="emp-custom-header">Personal Information</div>

        <div className="emp-sub-container">
          {/* ROW 1 */}
          <div className="emp-fields-container row-6">
            <div className="emp-field">
              <label className="emp-field-label">Employee Code*</label>
              <input
                className="emp-field-input"
                id="employeeId"
                value={employeeData.employeeId || ""}
                readOnly
              />
            </div>

            <div className="emp-field">
              <label className="emp-field-label">Title</label>
              <select
                className={`emp-field-input ${formErrors.title ? "error" : ""}`}
                id="title"
                value={employeeData.title || ""}
                onChange={handleInputChange}
                disabled={!isEditable}
              >
                <option value="">Select Title</option>
                {titles.map((t) => (
                  <option key={t} value={t}>
                    {t}
                  </option>
                ))}
              </select>
              <div className="emp-error-text">{formErrors.title}</div>
            </div>

            <div className="emp-field">
              <label className="emp-field-label">ID Number*</label>
              <input
                className="emp-field-input"
                id="idNumber"
                value={employeeData.idNumber || ""}
                readOnly
              />
            </div>

            <div className="emp-field">
              <label className="emp-field-label">Passport Number</label>
              <input
                className="emp-field-input"
                id="passportNumber"
                value={employeeData.passportNumber || ""}
                onChange={handleInputChange}
                readOnly
              />
            </div>

            <div className="emp-field">
              <label className="emp-field-label">Nationality*</label>
              <input
                className={`emp-field-input ${formErrors.nationality ? "error" : ""}`}
                id="nationality"
                value={employeeData.nationality || ""}
                onChange={handleInputChange}
                readOnly={!isEditable}
              />
            </div>
            <div className="emp-error-text">{formErrors.nationality}</div>
          </div>

          {/* ROW 2 */}
          <div className="emp-fields-container row-6">
            {[
              ["Date of Birth", "dateOfBirth"],
              ["Gender", "gender"],
            ].map(([label, id]) => (
              <div className="emp-field" key={id}>
                <label className="emp-field-label">{label}</label>
                <input
                  className="emp-field-input"
                  id={id}
                  value={employeeData[id] || ""}
                  readOnly
                />
              </div>
            ))}

            <div className="emp-field">
              <label className="emp-field-label">Disability Status</label>
              <select
                className={`emp-field-input ${formErrors.disability ? "error" : ""}`}
                id="disability"
                value={employeeData.disability ? "yes" : "no"}
                onChange={handleInputChange}
                disabled={!isEditable}
              >
                <option value="no">No</option>
                <option value="yes">Yes</option>
              </select>
              <div className="emp-error-text">{formErrors.disability}</div>
            </div>

            <div className="emp-field">
              <label className="emp-field-label">Disability Description</label>
              <input
                className={`emp-field-input ${formErrors.disabilityType ? "error" : ""}`}
                id="disabilityType"
                value={employeeData.disabilityType || ""}
                onChange={handleInputChange}
                readOnly={!employeeData.disability}
              />
              <div className="emp-error-text">{formErrors.disabilityType}</div>
            </div>
          </div>

          {/* ROW 3 */}
          <div className="emp-fields-container row-3">
            <div className="emp-field">
              <label className="emp-field-label">First Name</label>
              <input
                className={`emp-field-input ${formErrors.name ? "error" : ""}`}
                id="name"
                value={employeeData.name || ""}
                onChange={handleInputChange}
                readOnly={!isEditable}
              />
              <div className="emp-error-text">{formErrors.name}</div>
            </div>
            <div className="emp-field">
              <label className="emp-field-label">Last Name</label>
              <input
                className={`emp-field-input ${formErrors.surname ? "error" : ""}`}
                id="surname"
                value={employeeData.surname || ""}
                onChange={handleInputChange}
                readOnly={!isEditable}
              />
              <div className="emp-error-text">{formErrors.surname}</div>
            </div>
          </div>

          {/* ROW 4 */}
          <div className="emp-fields-container row-3">
            <div className="emp-field">
              <label className="emp-field-label">Contact Number*</label>
              <input
                className={`emp-field-input ${formErrors.contactNumber ? "error" : ""}`}
                id="contactNumber"
                value={employeeData.contactNumber || ""}
                onChange={handleInputChange}
                readOnly={!isEditable}
              />
              <div className="emp-error-text">{formErrors.contactNumber}</div>
            </div>
            <div className="emp-field">
              <label className="emp-field-label">Email Address</label>
              <input
                className={`emp-field-input ${formErrors.email ? "error" : ""}`}
                id="email"
                value={employeeData.email || ""}
                onChange={handleInputChange}
                readOnly={!isEditable}
              />
              <div className="emp-error-text">{formErrors.email}</div>
            </div>
          </div>

          {/* ROW 5 */}
          <div className="emp-fields-container row-3">
            <div className="emp-field">
              <label className="emp-field-label">Home Address*</label>
              <input
                className={`emp-field-input ${formErrors.physicalAddress ? "error" : ""}`}
                id="physicalAddress"
                value={employeeData.physicalAddress || ""}
                onChange={handleInputChange}
                readOnly={!isEditable}
              />
              <div className="emp-error-text">{formErrors.physicalAddress}</div>
            </div>
            <div className="emp-field">
              <label className="emp-field-label">City</label>
              <input
                className={`emp-field-input ${formErrors.city ? "error" : ""}`}
                id="city"
                value={employeeData.city || ""}
                onChange={handleInputChange}
                readOnly={!isEditable}
              />
              <div className="emp-error-text">{formErrors.city}</div>
            </div>
          </div>

          {/* ROW 6 */}
          <div className="emp-fields-container row-3">
            <div className="emp-field">
              <label className="emp-field-label">Postal Code</label>
              <input
                className={`emp-field-input ${formErrors.zipCode ? "error" : ""}`}
                id="zipCode"
                value={employeeData.zipCode || ""}
                onChange={handleInputChange}
                readOnly={!isEditable}
              />
              <div className="emp-error-text">{formErrors.zipCode}</div>
            </div>

            <div className="emp-field">
              <label className="emp-field-label">Monthly Salary</label>
              <input
                type="number"
                min="0"
                step="100"
                className={`emp-field-input ${formErrors.monthlySalary ? "error" : ""}`}
                id="monthlySalary"
                value={employeeData.monthlySalary || ""}
                onChange={handleInputChange}
                readOnly
              />
              <div className="emp-error-text">{formErrors.monthlySalary}</div>
            </div>

            <div className="emp-field">
              <label className="emp-field-label">Department</label>
              <select
                className={`emp-field-input ${formErrors.branch ? "error" : ""}`}
                id="branch"
                value={employeeData.branch || ""}
                onChange={handleInputChange}
                disabled={!isEditable}
              >
                <option value="">Select Department</option>
                {branches.map((branch) => (
                  <option key={branch} value={branch}>
                    {branch}
                  </option>
                ))}
              </select>
              <div className="emp-error-text">{formErrors.branch}</div>
            </div>
          </div>

          {/* ROW 7 */}
          <div className="emp-fields-container row-3">
            <div className="emp-field">
              <label className="emp-field-label">Employment Status</label>
              <select
                className={`emp-field-input ${formErrors.employeeStatus ? "error" : ""}`}
                id="employeeStatus"
                value={employeeData.employeeStatus || ""}
                onChange={handleInputChange}
                disabled={!isEditable}
              >
                <option value="">Select Status</option>
                {employmentStatuses.map((status) => (
                  <option key={status} value={status}>
                    {status}
                  </option>
                ))}
              </select>
              <div className="emp-error-text">{formErrors.employeeStatus}</div>
            </div>

            {/* Career Manager DROPDOWN */}
            <div className="emp-field">
              <label className="emp-field-label">Career Manager</label>
              <select
                className={`emp-field-input ${formErrors.reportsTo ? "error" : ""}`}
                id="reportsTo"
                value={employeeData.reportsTo}
                onChange={handleInputChange}
                disabled={!isEditable}
              >
                <option value="">Select Career Manager</option>
                {/* Replace this later with real employee list */}
                {allEmployees.map((emp) => (
                  <option key={emp.employeeId} value={emp.employeeId}>
                    {emp.name} {emp.surname}
                  </option>
                ))}
              </select>
              <div className="emp-error-text">{formErrors.reportsTo}</div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default PersonalInformation;
