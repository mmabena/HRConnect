import React, { useEffect, useState } from "react";
import { useLocation } from "react-router-dom";
import "../Components/EmployeeManagement/EditEmployee.css";
import api from "../api/api.js";
import { toast } from "react-toastify";
import {
  fetchAllEmployees,
  showConfirmationToast,
  editEmployee,
  formatDateToYYYYMMDD,
  formatDateForDisplay,
} from "../api/Employee.js";
import axios from "axios";

const PersonalInformation = () => {
  const location = useLocation();
  const readOnly = location.state?.readOnly || false;
  const [loading, setLoading] = useState(true);
  const [uploading, setUploading] = useState(false);
  const [uploadError, setUploadError] = useState("");
  const [activeTab, setActiveTab] = useState("Personal");
  const [isEditable, setIsEditable] = useState(false);
  const [formErrors, setFormErrors] = useState({});
  const [allEmployees, setAllEmployees] = useState([]);
  const branches = ["Johannesburg", "CapeTown", "UK"];
  const titles = ["Mr", "Mrs", "Ms", "Dr", "Prof"];
  const employmentStatuses = ["Permanent", "FixedTerm", "Contract"];
  const [employeeData, setEmployeeData] = useState({
    employeeId: "",
    name: "",
    surname: "",
    title: "",
    dateOfBirth: "",
    idType: "id",
    idNumber: "",
    gender: "",
    contactNumber: "",
    nationality: "",
    citizenship: "",
    disability: false,
    disabilityType: "",
    email: "",
    physicalAddress: "",
    city: "",
    zipCode: "",
    branch: "",
    monthlySalary: "",
    jobTitle: "",
    employeeStatus: "",
    reportsTo: "",
    profileImage: "",
  });

  const [originalIdNumber, setOriginalIdNumber] = useState("");
  const [originalDateOfBirth, setOriginalDateOfBirth] = useState("");
  const [originalTaxNumber, setOriginalTaxNumber] = useState("");

  useEffect(() => {
    const loadEmployees = async () => {
      const employeesData = await fetchAllEmployees();
      setAllEmployees(employeesData);
    };

    loadEmployees();
  }, []);

  const getInitials = (name, surname) => {
    let initials = "";
    if (name) initials += name.charAt(0).toUpperCase();
    if (surname) initials += surname.charAt(0).toUpperCase();
    return initials;
  };

  useEffect(() => {
    const fetchEmployee = async () => {
      try {
        const currentUserRaw = localStorage.getItem("currentUser");
        if (!currentUserRaw) {
          toast.error("User not logged in");
          setLoading(false);
          return;
        }
        const currentUser = JSON.parse(currentUserRaw);

        if (!currentUser.email) {
          toast.error("User email missing");
          setLoading(false);
          return;
        }

        const response = await api.get(`/employee/email/${currentUser.email}`);
        const emp = response.data;

        // Transform to match your form
        const transformed = {
          employeeId: emp.employeeId || "",
          name: emp.name || "",
          surname: emp.surname || "",
          title: emp.title || "",
          branch: emp.branch || "",
          dateOfBirth: emp.dateOfBirth
            ? formatDateForDisplay(emp.dateOfBirth)
            : "",
          idNumber: emp.idNumber || emp.passportNumber || "",
          nationality: emp.nationality || "",
          gender: emp.gender || "",
          contactNumber: emp.contactNumber || "",
          email: emp.email || "",
          physicalAddress: emp.physicalAddress || "",
          city: emp.city || "",
          monthlySalary: emp.monthlySalary || "",
          zipCode: emp.zipCode || "",
          disability: emp.hasDisability || false,
          disabilityType: emp.disabilityDescription || "N/A",
          jobTitle: emp.positionTitle || "",
          positionId: emp.positionId || 0,
          employeeStatus: emp.employmentStatus || "",
          reportsTo: emp.careerManagerID || "",
          startDate: emp.startDate || "",
          profileImage: emp.profileImage || "",
          initials: getInitials(emp.name, emp.surname),
        };
        setEmployeeData(transformed);
      } catch (error) {
        console.error("Failed to fetch employee", error);
        toast.error("Failed to load employee data");
      } finally {
        setLoading(false);
      }
    };

    fetchEmployee();
  }, []);

  const handleFileChange = async (
    e,
    setEmployee,
    setUploading,
    setErrorMessage,
  ) => {
    const file = e.target.files[0];
    if (
      file &&
      (file.type === "image/jpeg" ||
        file.type === "image/jpg" ||
        file.type === "image/png")
    ) {
      try {
        setUploading(true);
        setErrorMessage("");
        const formData = new FormData();
        formData.append("file", file);
        formData.append("upload_preset", "unsigned_preset");
        formData.append("folder", "samples/ecommerce");

        const response = await axios.post(
          "https://api.cloudinary.com/v1_1/djmafre5k/image/upload",
          formData,
        );

        const imageUrl = response.data.secure_url;
        // <-- Set profileImage instead of documentPath
        setEmployee((prev) => ({ ...prev, profileImage: imageUrl }));
        setUploading(false);
      } catch (error) {
        console.error("Error uploading image:", error);
        setErrorMessage("Error uploading image.");
        setUploading(false);
      }
    } else {
      setErrorMessage("Only .jpg, .jpeg or .png images are allowed.");
      setEmployee((prev) => ({ ...prev, profileImage: "" }));
    }
  };

  const onFileChange = async (e) => {
    await handleFileChange(e, setEmployeeData, setUploading, setUploadError);
  };

  const handleInputChange = (e) => {
    const { id, value, type, checked } = e.target;

    if (["dateOfBirth", "idNumber"].includes(id)) {
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

    if (formErrors[id]) {
      setFormErrors((prev) => {
        const updated = { ...prev };
        delete updated[id];
        return updated;
      });
    }

    if (loading) {
      return <div className="emp-loading">Loading employee...</div>;
    }
  };

  const handleEditSaveClick = async () => {
    if (uploading) {
      toast.warning("Please wait for image upload to complete before saving.");
      return;
    }

    if (!isEditable) {
      setIsEditable(true);
      return;
    }

    const validateEmployee = () => {
      const errors = {};

      if (!employeeData.title) errors.title = "Title is required";
      if (!employeeData.name?.trim()) errors.name = "First name is required";
      if (!employeeData.surname?.trim())
        errors.surname = "Last name is required";

      if (!employeeData.contactNumber?.trim())
        errors.contactNumber = "Contact number is required";

      if (!employeeData.email?.trim())
        errors.email = "Email address is required";

      if (!employeeData.physicalAddress?.trim())
        errors.physicalAddress = "Home address is required";

      if (!employeeData.city?.trim()) errors.city = "City is required";

      if (!employeeData.zipCode?.trim())
        errors.zipCode = "Postal code is required";

      if (!employeeData.monthlySalary)
        errors.monthlySalary = "Monthly salary is required";

      if (!employeeData.branch) errors.branch = "Department is required";

      if (!employeeData.employeeStatus)
        errors.employeeStatus = "Employment status is required";

      if (!employeeData.reportsTo)
        errors.reportsTo = "Career Manager is required";

      if (employeeData.disability && !employeeData.disabilityType?.trim()) {
        errors.disabilityType =
          "Disability description is required when disability is Yes";
      }

      setFormErrors(errors);

      return Object.keys(errors).length === 0;
    };

    const confirmed = await showConfirmationToast(
      "Are you sure you want to save changes?",
    );
    if (!confirmed) {
      setIsEditable(false);
      return;
    }

    if (!validateEmployee()) {
      toast.error("Please correct the highlighted fields.");
      return;
    }

    const idNumberTrimmed = employeeData.idNumber.trim();

    const payload = {
      employeeId: employeeData.employeeId,
      title: employeeData.title || null,
      name: employeeData.name || null,
      surname: employeeData.surname || null,
      idNumber: employeeData.idNumber,
      nationality: employeeData.nationality || null,
      gender: employeeData.gender || null,
      contactNumber: employeeData.contactNumber || null,
      taxNumber: employeeData.taxNumber || null,
      email: employeeData.email || null,
      physicalAddress: employeeData.physicalAddress || null,
      city: employeeData.city || null,
      zipCode: employeeData.zipCode || null,
      hasDisability: employeeData.disability || false,
      disabilityDescription:
        employeeData.disabilityType === "N/A"
          ? null
          : employeeData.disabilityType,
      dateOfBirth: formatDateToYYYYMMDD(employeeData.dateOfBirth),
      startDate: formatDateToYYYYMMDD(employeeData.startDate),
      branch: employeeData.branch || "Johannesburg", // map string to enum in backend if needed
      monthlySalary: Number(employeeData.monthlySalary) || 0,
      positionId: employeeData.positionId,
      employmentStatus: employeeData.employeeStatus || "Permanent",
      careerManagerID: employeeData.reportsTo || null,
      profileImage: employeeData.profileImage || null,
    };

    if (employeeData.passportNumber) {
      payload.passportNumber = employeeData.passportNumber;
    }

    payload.taxNumber = employeeData.taxNumber || originalTaxNumber;

    try {
      setLoading(true);

      await editEmployee(payload.employeeId, payload);

      toast.success("Employee updated successfully!");
      setIsEditable(false);

      setOriginalIdNumber(payload.idNumber);
      setOriginalTaxNumber(payload.taxNumber);
      setOriginalDateOfBirth(payload.dateOfBirth);
    } catch (error) {
      const response = error.response;

      if (!response) {
        toast.error("Server not reachable.");
        return;
      }

      const { message, errors, type } = response.data;

      setFormErrors({});

      if (message) toast.error(message);

      const mappedErrors = {};

      if (errors) {
        // If backend returns field-specific errors
        Object.entries(errors).forEach(([field, msg]) => {
          if (field === "general") {
            const lowerMsg = msg.toLowerCase();
            if (lowerMsg.includes("zip") || lowerMsg.includes("postal")) {
              mappedErrors.zipCode = msg;
            } else if (lowerMsg.includes("city")) {
              mappedErrors.city = msg;
            } else if (lowerMsg.includes("contact")) {
              mappedErrors.contactNumber = msg;
            } else if (lowerMsg.includes("email")) {
              mappedErrors.email = msg;
            } else if (lowerMsg.includes("name")) {
              mappedErrors.name = msg;
            } else if (lowerMsg.includes("surname")) {
              mappedErrors.surname = msg;
            } else if (lowerMsg.includes("address")) {
              mappedErrors.physicalAddress = msg;
            } else {
              // If cannot map, still show toast
              toast.error(msg);
            }
          } else {
            mappedErrors[field] = msg;
          }
        });
      }

      setFormErrors(mappedErrors);

      if (type === "BusinessRuleException") {
        toast.error(message);
      }

      if (type === "NotFoundException") {
        toast.error("Employee not found.");
      }
    } finally {
      setLoading(false);
    }
  };

  if (loading) return <div>Loading employee profile...</div>;
  if (!employeeData) return <div>No employee data found</div>;

  return (
    <div className="emp-menu-background">
      <div className="emp-edit-employee-top-container">
        <div
          className="emp-photo-block"
          onClick={() => document.getElementById("emp-photo-input").click()}
        >
          <img
            src={employeeData.profileImage || "/default-profile.png"}
            alt="Employee"
          />
          {uploading && (
            <div className="emp-uploading-overlay">Uploading...</div>
          )}
        </div>
        <input
          type="file"
          id="emp-photo-input"
          className="emp-photo-input"
          onChange={onFileChange}
        />
        {uploadError && <div className="emp-error-text">{uploadError}</div>}

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
                readOnly
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
                readOnly={!isEditable}
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
