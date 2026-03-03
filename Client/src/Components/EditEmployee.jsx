import React, { useState, useEffect } from "react";
import { useLocation, useParams } from "react-router-dom";
import "../Components/EditEmployee.css";
import {
  editEmployee,
  formatDateForDisplay,
  toISOStringSafe,
  fetchAllEmployees,
  showConfirmationToast,
  GetEmployeeByEmployeeNumberAsync,
  formatDateToYYYYMMDD,
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
  const { employeeId } = useParams();
  const [activeTab, setActiveTab] = useState("Personal");
  const [isEditable, setIsEditable] = useState(false);
  const [userRole, setUserRole] = useState(null);
  const [loading, setLoading] = useState(false);
  const [formErrors, setFormErrors] = useState({});
  //const positionTitle = positions.find(p => p.positionId === employeeData.jobTitle)?.positionTitle || "";
  /// </summary>
  /// Track original Employee number and DOB loaded from DB
  /// </summary>
  const [originalEmployeeId, setOriginalEmployeeId] = useState("");
  const [allEmployees, setAllEmployees] = useState([]);
  const [originalIdNumber, setOriginalIdNumber] = useState("");
  const [originalDateOfBirth, setOriginalDateOfBirth] = useState("");
  const [originalTaxNumber, setOriginalTaxNumber] = useState("");
  const branches = ["Johannesburg", "CapeTown", "UK"];
  const titles = ["Mr", "Mrs", "Ms", "Dr", "Prof"];
  const employmentStatuses = ["Permanent", "FixedTerm", "Contract"];
  const [employeeData, setEmployeeData] = useState({
    employeeId: "",
    firstName: "",
    lastName: "",
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
    homeAddress: "",
    city: "",
    postalCode: "",
    department: "",
    jobTitle: "",
    positionTitle: "",
    employeeStatus: "",
    reportsTo: "",
    documentPath: "",
  });

  const getInitials = (firstName, lastName) => {
    let initials = "";

    if (firstName) initials += firstName.charAt(0).toUpperCase();

    if (lastName) initials += lastName.charAt(0).toUpperCase();

    return initials;
  };

  useEffect(() => {
    const loadEmployees = async () => {
      const employeesData = await fetchAllEmployees();
      setAllEmployees(employeesData);
    };

    loadEmployees();
  }, []);

  /// </summary>
  /// Load user role and employee data when component mounts or location.state changes - set loading true at the start for all cases
  /// </summary>
  useEffect(() => {
    setLoading(true);

    const role = getCurrentUserRole();
    setUserRole(role);

    const loadEmployeeIfNeeded = async () => {
      if (!location.state && employeeId) {
        try {
          const employee = await GetEmployeeByEmployeeNumberAsync(employeeId);
          console.log("Fetched employee:", employee);
          console.log("Employee object keys:", Object.keys(employee));

          const transformed = {
            employeeId: employee.employeeId || "",
            firstName: employee.name || "",
            lastName: employee.surname || "",
            title: employee.title || "",
            department: employee.branch || "",
            dateOfBirth: employee.dateOfBirth
              ? formatDateForDisplay(employee.dateOfBirth)
              : "",
            idNumber: employee.idNumber || employee.passportNumber || "",
            nationality: employee.nationality || "",
            gender: employee.gender || "",
            contactNumber: employee.contactNumber || "",
            email: employee.email || "",
            homeAddress: employee.physicalAddress || "",
            city: employee.city || "",
            postalCode: employee.zipCode || "",
            disability: employee.hasDisability || false,
            disabilityType: employee.disabilityDescription || "N/A",
            jobTitle: employee.positionTitle || "",
            positionId: employee.positionId || 0,
            employeeStatus: employee.employmentStatus || "",
            reportsTo: employee.careerManagerID || "",
            startDate: employee.startDate || "",
            documentPath: employee.profileImage || "",
            initials: getInitials(employee.name, employee.surname),
          };

          setEmployeeData(transformed);
          setOriginalEmployeeId(employee.employeeId);
          setOriginalTaxNumber(employee.taxNumber);
          setOriginalDateOfBirth(employee.dateOfBirth);
          setOriginalIdNumber(employee.idNumber);
        } catch (error) {
          console.error("Failed to load employee", error);
          toast.error("Could not load employee data.");
        } finally {
          setLoading(false);
        }
      } else if (location.state) {
        const employee = location.state;

        const transformed = {
          employeeId: employee.employeeId || "",
          firstName: employee.name || "",
          lastName: employee.surname || "",
          title: employee.title || "",
          dateOfBirth: employee.dateOfBirth
            ? formatDateForDisplay(employee.dateOfBirth)
            : "",
          idNumber: employee.idNumber || employee.passportNumber || "",
          nationality: employee.nationality || "",
          gender: employee.gender || "",
          contactNumber: employee.contactNumber || "",
          email: employee.email || "",
          homeAddress: employee.physicalAddress || "",
          city: employee.city || "",
          passportNumber: employee.passportNumber || "",
          monthlySalary: employee.monthlySalary || "",
          department: employee.branch || "",
          postalCode: employee.zipCode || "",
          disability: employee.hasDisability || false,
          disabilityType: employee.disabilityDescription || "N/A",
          jobTitle: employee.positionTitle || "",
          positionId: employee.positionId || 0,
          employeeStatus: employee.employmentStatus || "",
          reportsTo: employee.careerManagerID || "",
          startDate: employee.startDate || "",
          documentPath: employee.profileImage || "",
        };

        setEmployeeData(transformed);
        setOriginalEmployeeId(employee.employeeId ?? "");
        setOriginalDateOfBirth(employee.dateOfBirth ?? "");
        setOriginalIdNumber(employee.idNumber ?? "");
        setOriginalTaxNumber(employee.taxNumber ?? "");
        setLoading(false);
      }
    };

    loadEmployeeIfNeeded();
  }, [location.state, employeeId]);
  useEffect(() => {
    console.log("Current employee data being viewed/edited:", employeeData);
  }, [employeeData]);

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
    if (!employeeData.disability && employeeData.disabilityType !== "N/A") {
      setEmployeeData((prev) => ({
        ...prev,
        disabilityType: "N/A",
      }));
    }
  }, [employeeData.disability, employeeData.disabilityType]);

  if (userRole !== "superuser") {
    return <div>Access Denied. Only super users can access this page.</div>;
  }

  const handleEditSaveClick = async () => {
    if (!isEditable) {
      setIsEditable(true);
      return;
    }

    const confirmed = await showConfirmationToast(
      "Are you sure you want to save changes?",
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
      employeeId: employeeData.employeeId,
      title: employeeData.title || null,
      name: employeeData.firstName || null,
      surname: employeeData.lastName || null,
      idNumber: employeeData.idNumber,
      nationality: employeeData.nationality || null,
      gender: employeeData.gender || null,
      contactNumber: employeeData.contactNumber || null,
      taxNumber: employeeData.taxNumber || null,
      email: employeeData.email || null,
      physicalAddress: employeeData.homeAddress || null,
      city: employeeData.city || null,
      zipCode: employeeData.postalCode || null,
      hasDisability: employeeData.disability || false,
      disabilityDescription:
        employeeData.disabilityType === "N/A"
          ? null
          : employeeData.disabilityType,
      dateOfBirth: formatDateToYYYYMMDD(employeeData.dateOfBirth),
      startDate: formatDateToYYYYMMDD(employeeData.startDate),
      branch: employeeData.department || "Johannesburg", // map string to enum in backend if needed
      monthlySalary: employeeData.monthlySalary
        ? parseFloat(employeeData.monthlySalary)
        : 0,
      positionId: employeeData.positionId,
      employmentStatus: employeeData.employeeStatus || "Permanent",
      careerManagerID: employeeData.reportsTo || null,
      profileImage: employeeData.documentPath || null,
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
    <div className="new-menu-background">
      <div className="new-edit-employee-top-container">
        <div className="new-photo-block">
          <img
            src={employeeData.documentPath || "/default-profile.png"}
            alt="Employee"
          />
        </div>
        <div className="new-photo-text-container">
          <div className="new-title">{`${employeeData.firstName} ${employeeData.lastName}`}</div>
          <div className="new-subtitle">{employeeData.jobTitle}</div>
          <div className="new-subsubtitle">{employeeData.department}</div>
        </div>
      </div>

      <div className="new-edit-employee-heading-row">
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

      <div className="new-edit-employee-form-container">
        <div className="new-edit-button-top-right">
          {!readOnly && (
            <button
              className="new-em-edit-button"
              onClick={handleEditSaveClick}
            >
              {isEditable ? "Save" : "Edit Profile"}
            </button>
          )}
        </div>
        <div className="new-custom-header">Personal Information</div>

        <div className="new-sub-container">
          {/* ROW 1 */}
          <div className="new-fields-container row-6">
            <div className="new-field">
              <label className="new-field-label">Employee Id</label>
              <input
                className="new-field-input"
                id="employeeId"
                value={employeeData.employeeId || ""}
                readOnly
              />
            </div>

            <div className="new-field">
              <label className="new-field-label">Title</label>
              <select
                className="new-field-input"
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

            <div className="new-field">
              <label className="new-field-label">Id Number</label>
              <input
                className="new-field-input"
                id="idNumber"
                value={employeeData.idNumber || ""}
                readOnly
              />
            </div>

            <div className="new-field">
              <label className="new-field-label">Passport Number</label>
              <input
                className="new-field-input"
                id="passportNumber"
                value={employeeData.passportNumber || ""}
                onChange={handleInputChange}
                readOnly
              />
            </div>

            <div className="new-field">
              <label className="new-field-label">Nationality</label>
              <input
                className="new-field-input"
                id="nationality"
                value={employeeData.nationality || ""}
                onChange={handleInputChange}
                readOnly={!isEditable}
              />
            </div>
          </div>

          {/* ROW 2 */}
          <div className="new-fields-container row-6">
            {[
              ["Date of Birth", "dateOfBirth"],
              ["Gender", "gender"],
            ].map(([label, id]) => (
              <div className="new-field" key={id}>
                <label className="new-field-label">{label}</label>
                <input
                  className="new-field-input"
                  id={id}
                  value={
                    id === "dateOfBirth"
                      ? formatDateForDisplay(employeeData[id])
                      : employeeData[id] || ""
                  }
                  readOnly
                />
              </div>
            ))}

            <div className="new-field">
              <label className="new-field-label">Disability Status</label>
              <select
                className="new-field-input"
                id="disability"
                value={employeeData.disability ? "yes" : "no"}
                onChange={handleInputChange}
                disabled={(!isEditable)}
              >
                <option value="no">No</option>
                <option value="yes">Yes</option>
              </select>
            </div>

            <div className="new-field">
              <label className="new-field-label">Disability Description</label>
              <input
                className="new-field-input"
                id="disabilityType"
                value={employeeData.disabilityType || ""}
                onChange={handleInputChange}
                readOnly={!employeeData.disability == "yes"}
              />
            </div>
          </div>

          {/* ROW 3 */}
          <div className="new-fields-container row-3">
            <div className="new-field">
              <label className="new-field-label">First Name</label>
              <input
                className="new-field-input"
                id="firstName"
                value={employeeData.firstName || ""}
                onChange={handleInputChange}
                readOnly={(!isEditable)}
              />
            </div>
            <div className="new-field">
              <label className="new-field-label">Last Name</label>
              <input
                className="new-field-input"
                id="lastName"
                value={employeeData.lastName || ""}
                onChange={handleInputChange}
                readOnly={(!isEditable)}
              />
            </div>
          </div>

          {/* ROW 4 */}
          <div className="new-fields-container row-3">
            <div className="new-field">
              <label className="new-field-label">Contact Number</label>
              <input
                className="new-field-input"
                id="contactNumber"
                value={employeeData.contactNumber || ""}
                onChange={handleInputChange}
                readOnly={(!isEditable)}
              />
            </div>
            <div className="new-field">
              <label className="new-field-label">Email Address</label>
              <input
                className="new-field-input"
                id="email"
                value={employeeData.email || ""}
                onChange={handleInputChange}
                readOnly={(!isEditable)}
              />
            </div>
          </div>

          {/* ROW 5 */}
          <div className="new-fields-container row-3">
            <div className="new-field">
              <label className="new-field-label">Home Address</label>
              <input
                className="new-field-input"
                id="homeAddress"
                value={employeeData.homeAddress || ""}
                onChange={handleInputChange}
                readOnly={(!isEditable)}
              />
            </div>
            <div className="new-field">
              <label className="new-field-label">City</label>
              <input
                className="new-field-input"
                id="city"
                value={employeeData.city || ""}
                onChange={handleInputChange}
                readOnly={(!isEditable)}
              />
            </div>
          </div>

          {/* ROW 6 */}
          <div className="new-fields-container row-3">
            <div className="new-field">
              <label className="new-field-label">Postal Code</label>
              <input
                className="new-field-input"
                id="postalCode"
                value={employeeData.postalCode || ""}
                onChange={handleInputChange}
                readOnly={(!isEditable)}
              />
            </div>

            <div className="new-field">
              <label className="new-field-label">Monthly Salary</label>
              <input
                className="new-field-input"
                id="monthlySalary"
                value={employeeData.monthlySalary || ""}
                onChange={handleInputChange}
                readOnly={(!isEditable)}
              />
            </div>

            <div className="new-field">
              <label className="new-field-label">Department</label>
              <select
                className="new-field-input"
                id="department"
                value={employeeData.department || ""}
                onChange={handleInputChange}
                disabled={(!isEditable)}
              >
                <option value="">Select Department</option>
                {branches.map((branch) => (
                  <option key={branch} value={branch}>
                    {branch}
                  </option>
                ))}
              </select>
            </div>
          </div>

          {/* ROW 7 */}
          <div className="new-fields-container row-3">
            <div className="new-field">
              <label className="new-field-label">Employment Status</label>
              <select
                className="new-field-input"
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
            </div>

            {/* Career Manager DROPDOWN */}
            <div className="new-field">
              <label className="new-field-label">Career Manager</label>
              <select
                className="new-field-input"
                id="reportsTo"
                value={employeeData.reportsTo}
                onChange={handleInputChange}
                disabled={(!isEditable)}
              >
                <option value="">Select Career Manager</option>
                {/* Replace this later with real employee list */}
                {allEmployees.map((emp) => (
                  <option key={emp.employeeId} value={emp.employeeId}>
                    {emp.name} {emp.surname}
                  </option>
                ))}
                
              </select>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default EditEmployee;
