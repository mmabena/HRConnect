import React, { useState, useEffect, useRef  } from "react";
import { useLocation, useParams } from "react-router-dom";
import "./EditEmployee.css";
import {
  editEmployee,
  formatDateForDisplay,
  fetchAllEmployees,
  showConfirmationToast,
  GetEmployeeByEmployeeNumberAsync,
  formatDateToYYYYMMDD,
} from "../../api/Employee";

import useEmployeeData from "../../hooks/useEmployeeData";
import useEmployeeForm from "../../hooks/useEmployeeForm";
import useEmployeeValidation from "../../hooks/useEmployeeValidation";
import useImageUpload from "../../hooks/useImageUpload";
import useUserRole from "../../hooks/useUserRole";

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
  const role = useUserRole();
  const [loading, setLoading] = useState(true);
  const fileInputRef = useRef(null);
  //const positionTitle = positions.find(p => p.positionId === employeeData.jobTitle)?.positionTitle || "";
  /// </summary>
  /// Track original Employee number and DOB loaded from DB
  /// </summary>
  const [originalEmployeeId, setOriginalEmployeeId] = useState("");
  const { allEmployees, positions } = useEmployeeData();

  const { validateEmployee } = useEmployeeValidation();

  const { uploadImage, uploading } = useImageUpload();
  const [originalTaxNumber, setOriginalTaxNumber] = useState("");
  const branches = ["Johannesburg", "CapeTown", "UK"];
  const titles = ["Mr", "Mrs", "Ms", "Dr", "Prof"];
  const employmentStatuses = ["Permanent", "FixedTerm", "Contract"];
  const { employee, setEmployee, formErrors, setFormErrors, onInputChange } =
    useEmployeeForm({
      employeeId: "",
      name: "",
      surname: "",
      title: "",
      dateOfBirth: "",
      idType: "",
      idNumber: "",
      passportNumber: "",
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
      positionTitle: "",
      jobTitle: "",
      startDate: "",
      employeeStatus: "",
      reportsTo: "",
      profileImage: "",
    });

  /// </summary>
  /// Load user role and employee data when component mounts or location.state changes - set loading true at the start for all cases
  /// </summary>
  useEffect(() => {
    const loadEmployee = async () => {
      setLoading(true);
      try {
        let emp;

        if (location.state) {
          emp = location.state;
        } else {
          emp = await GetEmployeeByEmployeeNumberAsync(employeeId);
        }

        const idType = emp.idNumber
          ? "id"
          : emp.passportNumber
            ? "passport"
            : "id";

        setEmployee({
          employeeId: emp.employeeId || "",
          title: emp.title || "",
          name: emp.name || "",
          surname: emp.surname || "",
          idType,
          idNumber: idType === "id" ? emp.idNumber || "" : "",
          passportNumber: idType === "passport" ? emp.passportNumber || "" : "",
          nationality: emp.nationality || "",
          gender: emp.gender || "",
          dateOfBirth: emp.dateOfBirth || "",
          contactNumber: emp.contactNumber || "",
          taxNumber: emp.taxNumber || "",
          email: emp.email || "",
          physicalAddress: emp.physicalAddress || "",
          city: emp.city || "",
          zipCode: emp.zipCode || "",
          branch: emp.branch || "",
          monthlySalary: emp.monthlySalary || "",
          jobTitle: emp.positionId?.toString() || "",
          positionTitle: emp.positionTitle || "",
          startDate: emp.startDate || "",
          employeeStatus: emp.employmentStatus || "",
          reportsTo: emp.careerManagerID || "",
          disability: emp.hasDisability || false,
          disabilityType: emp.disabilityDescription || "",
          profileImage: emp.profileImage || "",
        });

        
      } catch (err) {
        toast.error("Could not load employee");
        console.error(err);
      } finally {
        setLoading(false);
      }
    };

    loadEmployee();
  }, [employeeId, location.state, setEmployee]);

  if (role !== "superuser") {
    return <div>Access Denied</div>;
  }

  if (loading) {
    return <div>Loading employee...</div>;
  }

  const positionTitle =
    positions.find((p) => p.positionId === Number(employee.jobTitle))
      ?.positionTitle || employee.positionTitle;

  /// Image upload
  const onFileChange = async (e) => {
    const file = e.target.files[0];

    const url = await uploadImage(file);

    if (url) {
      setEmployee((prev) => ({
        ...prev,
        profileImage: url,
      }));
    }
  };

  /// Save
  const handleSave = async () => {
    if (!isEditable) {
      setIsEditable(true);
      return;
    }

    const confirmed = await showConfirmationToast(
      "Are you sure you want to save changes?",
    );

    if (!confirmed) return;

    const errors = validateEmployee(employee);
    setFormErrors(errors);

    if (Object.keys(errors).length > 0) {
      console.error(errors);
      toast.error("Please fix validation errors");
      return;
    }

    console.log("Validating employee:", employee);

    try {
      setLoading(true);

      const payload = {
        employeeId: employee.employeeId,
        title: employee.title,
        name: employee.name,
        surname: employee.surname,
        idNumber: employee.idType === "id" ? employee.idNumber : undefined,
        passportNumber:
          employee.idType === "passport" ? employee.passportNumber : undefined,
        nationality: employee.nationality,
        gender: employee.gender,
        dateOfBirth: employee.dateOfBirth,
        contactNumber: employee.contactNumber,
        taxNumber: employee.taxNumber,
        email: employee.email,
        physicalAddress: employee.physicalAddress,
        city: employee.city,
        zipCode: employee.zipCode,
        branch: employee.branch,
        monthlySalary: Number(employee.monthlySalary),
        positionId: employee.jobTitle ? Number(employee.jobTitle) : null,
        employmentStatus: employee.employeeStatus,
        careerManagerID: employee.reportsTo,
        hasDisability: employee.disability,
        disabilityDescription: employee.disabilityType,
        startDate: employee.startDate,
        profileImage: employee.profileImage,
      };

      await editEmployee(payload.employeeId, payload);

      toast.success("Employee updated successfully!");

      setIsEditable(false);
    } catch (err) {
      if (err.response && err.response.data?.errors) {
        setFormErrors(err.response.data.errors);
      } else {
        toast.error("Failed to update employee");
        console.error(err);
      }
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="emp-menu-background">
      <div className="emp-edit-employee-top-container">
        <div
          className="emp-photo-block"
          onClick={() => fileInputRef.current?.click()}
        >
          <img
            src={employee.profileImage || "/default-profile.png"}
            alt="Employee"
          />
          {uploading && (
            <div className="emp-uploading-overlay">Uploading...</div>
          )}
        </div>
        <input
          type="file"
          className="emp-photo-input"
          onChange={onFileChange}
          ref={fileInputRef}
        />

        <div className="emp-photo-text-container">
          <div className="emp-title">{`${employee.name} ${employee.surname}`}</div>
          <div className="emp-subtitle">{positionTitle}</div>
          <div className="emp-subsubtitle">{employee.branch}</div>
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
            <button className="emp-em-edit-button" onClick={handleSave}>
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
                name="employeeId"
                value={employee.employeeId || ""}
                readOnly
              />
            </div>

            <div className="emp-field">
              <label className="emp-field-label">Title</label>
              <div className="emp-field-dropdown">
                <select
                  className={`emp-field-input ${formErrors.title ? "error" : ""}`}
                  name="title"
                  value={employee.title || ""}
                  onChange={onInputChange}
                  disabled={!isEditable}
                >
                  <option value="">Select Title</option>
                  {titles.map((t) => (
                    <option key={t} value={t}>
                      {t}
                    </option>
                  ))}
                </select>
                <img
                  src="/images/arrow_drop_down_circle.png"
                  alt="Dropdown icon"
                  className="empdropdown-icon"
                />
              </div>
              <div className="emp-error-text">{formErrors.title}</div>
            </div>

            <div className="emp-field">
              <label className="emp-field-label">ID Number*</label>
              <input
                className="emp-field-input"
                name="idNumber"
                value={employee.idNumber || ""}
                readOnly
              />
            </div>

            <div className="emp-field">
              <label className="emp-field-label">Passport Number</label>
              <input
                className="emp-field-input"
                name="passportNumber"
                value={employee.passportNumber || ""}
                onChange={onInputChange}
                readOnly
              />
            </div>

            <div className="emp-field">
              <label className="emp-field-label">Nationality*</label>
              <input
                className={`emp-field-input ${formErrors.nationality ? "error" : ""}`}
                name="nationality"
                value={employee.nationality || ""}
                onChange={onInputChange}
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
                  name={id}
                  value={employee[id] || ""}
                  readOnly
                />
              </div>
            ))}

            <div className="emp-field">
              <label className="emp-field-label">Disability Status</label>
              <div className="emp-field-dropdown">
                <select
                  className={`emp-field-input ${formErrors.disability ? "error" : ""}`}
                  name="disability"
                  value={employee.disability ? "yes" : "no"}
                  onChange={onInputChange}
                  disabled={!isEditable}
                >
                  <option value="no">No</option>
                  <option value="yes">Yes</option>
                </select>

                <img
                  src="/images/arrow_drop_down_circle.png"
                  alt="Dropdown icon"
                  className="empdropdown-icon"
                />
              </div>
              <div className="emp-error-text">{formErrors.disability}</div>
            </div>

            <div className="emp-field">
              <label className="emp-field-label">Disability Description</label>
              <input
                className={`emp-field-input ${formErrors.disabilityType ? "error" : ""}`}
                name="disabilityType"
                value={employee.disabilityType || ""}
                onChange={onInputChange}
                readOnly={!employee.disability}
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
                name="name"
                value={employee.name || ""}
                onChange={onInputChange}
                readOnly={!isEditable}
              />
              <div className="emp-error-text">{formErrors.name}</div>
            </div>
            <div className="emp-field">
              <label className="emp-field-label">Last Name</label>
              <input
                className={`emp-field-input ${formErrors.surname ? "error" : ""}`}
                name="surname"
                value={employee.surname || ""}
                onChange={onInputChange}
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
                name="contactNumber"
                value={employee.contactNumber || ""}
                onChange={onInputChange}
                readOnly={!isEditable}
              />
              <div className="emp-error-text">{formErrors.contactNumber}</div>
            </div>
            <div className="emp-field">
              <label className="emp-field-label">Email Address</label>
              <input
                className={`emp-field-input ${formErrors.email ? "error" : ""}`}
                name="email"
                value={employee.email || ""}
                onChange={onInputChange}
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
                name="physicalAddress"
                value={employee.physicalAddress || ""}
                onChange={onInputChange}
                readOnly={!isEditable}
              />
              <div className="emp-error-text">{formErrors.physicalAddress}</div>
            </div>
            <div className="emp-field">
              <label className="emp-field-label">City</label>
              <input
                className={`emp-field-input ${formErrors.city ? "error" : ""}`}
                name="city"
                value={employee.city || ""}
                onChange={onInputChange}
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
                name="zipCode"
                value={employee.zipCode || ""}
                onChange={onInputChange}
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
                name="monthlySalary"
                value={employee.monthlySalary || ""}
                onChange={onInputChange}
                readOnly={!isEditable}
              />
              <div className="emp-error-text">{formErrors.monthlySalary}</div>
            </div>

            <div className="emp-field">
              <label className="emp-field-label">Department</label>
              <div className="emp-field-dropdown">
                <select
                  className={`emp-field-input ${formErrors.branch ? "error" : ""}`}
                  name="branch"
                  value={employee.branch || ""}
                  onChange={onInputChange}
                  disabled={!isEditable}
                >
                  <option value="">Select Department</option>
                  {branches.map((branch) => (
                    <option key={branch} value={branch}>
                      {branch}
                    </option>
                  ))}
                </select>
                <img
                  src="/images/arrow_drop_down_circle.png"
                  alt="Dropdown icon"
                  className="empdropdown-icon"
                />
              </div>
              <div className="emp-error-text">{formErrors.branch}</div>
            </div>
          </div>

          {/* ROW 7 */}
          <div className="emp-fields-container row-3">
            <div className="emp-field">
              <label className="emp-field-label">Employment Status</label>
              <div className="emp-field-dropdown">
                <select
                  className={`emp-field-input ${formErrors.employeeStatus ? "error" : ""}`}
                  name="employeeStatus"
                  value={employee.employeeStatus || ""}
                  onChange={onInputChange}
                  disabled={!isEditable}
                >
                  <option value="">Select Status</option>
                  {employmentStatuses.map((status) => (
                    <option key={status} value={status}>
                      {status}
                    </option>
                  ))}
                </select>
                <img
                  src="/images/arrow_drop_down_circle.png"
                  alt="Dropdown icon"
                  className="empdropdown-icon"
                />
              </div>
              <div className="emp-error-text">{formErrors.employeeStatus}</div>
            </div>

            {/* Career Manager DROPDOWN */}
            <div className="emp-field">
              <label className="emp-field-label">Career Manager</label>
              <div className="emp-field-dropdown">
                <select
                  className={`emp-field-input ${formErrors.reportsTo ? "error" : ""}`}
                  name="reportsTo"
                  value={employee.reportsTo}
                  onChange={onInputChange}
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
                <img
                  src="/images/arrow_drop_down_circle.png"
                  alt="Dropdown icon"
                  className="empdropdown-icon"
                />
              </div>
              <div className="emp-error-text">{formErrors.reportsTo}</div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default EditEmployee;