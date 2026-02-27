import axios from "axios";
import { toast } from "react-toastify";

const API_BASE = "http://localhost:5037/api/employee";

/// </summary>
/// Add a response interceptor to handle empty responses gracefully
/// </summary>
axios.interceptors.response.use(
  (response) => {
    /// </summary>
    /// If response data is an empty string, replace it with null to avoid JSON parse errors
    /// </summary>
    if (response.data === "") {
      response.data = null;
    }
    return response;
  },
  (error) => {
    return Promise.reject(error);
  }
);

/// </summary>
/// --- API HANDLERS ---
//  /// </summary>

export const addEmployee = async (employee) => {
  try {
    const response = await axios.post(`${API_BASE}/add`, employee, {
      headers: { "Content-Type": "application/json" },
    });

    /// </summary>
    /// Defensive fallback: ensures the function returns an object even if its empty if the response.data is null or undefined
    /// </summary>
    return response.data || {};
  } catch (error) {
    if (error.response) {
      console.error("Add employee error response data:", error.response.data);
      console.error("Add employee error status:", error.response.status);
    } else {
      console.error("Add employee error message:", error.message);
    }
    throw error;
  }
};

export const editEmployee = async (employeeNumber, employee) => {
  try {
    const response = await axios.put(
      `${API_BASE}/edit/${employeeNumber}`,
      employee,
      {
        headers: { "Content-Type": "application/json" },
      }
    );
    return response.data || {};
  } catch (error) {
    if (error.response) {
      console.error("Edit employee error response data:", error.response.data);
      console.error("Edit employee error status:", error.response.status);
    } else {
      console.error("Edit employee error message:", error.message);
    }
    throw error;
  }
};

export const fetchEmployeeByIdNumber = async (idNumber) => {
  try {
    const response = await axios.get(
      `${API_BASE}/by-idnumber/${encodeURIComponent(idNumber)}`
    );

    /// </summary>
    /// Defensive fallback,returns empty object if response data is null
    /// </summary>
    return response.data || {};
  } catch (error) {
    if (error.response) {
      console.error(
        "Fetch employee by ID number error response data:",
        error.response.data
      );
      console.error(
        "Fetch employee by ID number error status:",
        error.response.status
      );
    } else {
      console.error(
        "Fetch employee by ID number error message:",
        error.message
      );
    }
    throw error;
  }
};

export const GetEmployeeByEmployeeNumberAsync = async (employeeNumber) => {
  try {
    const encoded = encodeURIComponent(employeeNumber);
    const response = await axios.get(
      `${API_BASE}/by-employee-number/${encoded}`
    );
    return response.data || {};
  } catch (error) {
    console.error("Fetch employee by employee number error:", error);
    throw error;
  }
};

export const fetchAllEmployees = async () => {
  try {
    const response = await axios.get(`${API_BASE}/all`);
    return response.data || [];
  } catch (error) {
    if (error.response) {
      console.error(
        "Fetch employees error response data:",
        error.response.data
      );
      console.error("Fetch employees error status:", error.response.status);
    } else {
      console.error("Fetch employees error message:", error.message);
    }
    throw error;
  }
};

/// </summary>
/// --- UTILITY FUNCTIONS ---
/// </summary>

export function deriveDOBFromIdNumber(idNumber) {
  if (!idNumber || idNumber.length < 6) return "";

  const yy = parseInt(idNumber.slice(0, 2), 10);
  const mm = parseInt(idNumber.slice(2, 4), 10);
  const dd = parseInt(idNumber.slice(4, 6), 10);

  if (isNaN(yy) || isNaN(mm) || isNaN(dd)) return "";

  const currentYear = new Date().getFullYear();
  const currentTwoDigits = currentYear % 100;

  const fullYear = yy <= currentTwoDigits ? 2000 + yy : 1900 + yy;

  const dob = new Date(fullYear, mm - 1, dd);

  /// </summary>
  /// Format YYYY-MM-DD from local date to avoid timezone shifts:
  /// </summary>
  const year = dob.getFullYear();
  const month = String(dob.getMonth() + 1).padStart(2, "0");
  const day = String(dob.getDate()).padStart(2, "0");

  return `${year}-${month}-${day}`;
}

export const toISOStringSafe = (dateStr) => {
  const parsed = new Date(dateStr);
  return isNaN(parsed.getTime()) ? null : parsed.toISOString();
};
export const generateEmployeeNumber = (lastName, existingCount = 0) => {
  if (!lastName) return "";
  const prefix =
    lastName.length >= 3
      ? lastName.substring(0, 3).toUpperCase()
      : lastName.toUpperCase().padEnd(3, "X");

  return `${prefix}${(existingCount + 1).toString().padStart(3, "0")}`;
};

export const populateFromIdNumber = (idNumber) => {
  if (!idNumber || idNumber.toString().length !== 13) {
    console.log("Invalid ID number length:", idNumber);
    return {};
  }

  try {
    const idStr = idNumber.toString().padStart(13, "0");
    console.log("Padded ID String:", idStr);

    const dobStr = idStr.slice(0, 6);
    const yearPrefix = parseInt(dobStr.slice(0, 2), 10) < 30 ? 2000 : 1900;
    const year = yearPrefix + parseInt(dobStr.slice(0, 2), 10);
    const month = parseInt(dobStr.slice(2, 4), 10) - 1;
    const day = parseInt(dobStr.slice(4, 6), 10);
    const dob = new Date(year, month, day);

    const genderStr = idStr.slice(6, 10);
    console.log("Gender digits:", genderStr);

    const genderVal = parseInt(genderStr, 10);
    console.log("Parsed gender value:", genderVal);

    let gender = null;

    if (genderVal < 5000) {
      gender = "Female";
    }

    if (genderVal >= 5000) {
      gender = "Male";
    }

    const nationality =
      idStr[10] === "0" ? "South African" : "Non-South African";

    console.log({
      dateOfBirth: dob.toLocaleDateString("en-CA"),
      gender,
      nationality,
    });

    return {
      dateOfBirth: dob.toLocaleDateString("en-CA"),
      gender: gender.toLowerCase(),
      nationality,
      citizenship: nationality === "South African" ? "South African" : "",
    };
  } catch (error) {
    console.error("Error parsing ID number:", error);
    return {};
  }
};

export const populateFromPassportNumber = (idNumber) => {
  const regex = /^[a-zA-Z0-9]{6,9}$/;
  if (!regex.test(idNumber)) return {};

  return {
    dateOfBirth: null,
    gender: null,
    nationality: "Non-South African",
  };
};

export const validateIdNumber = (type, value) => {
  if (!value) return "ID/Passport number is required";

  if (type === "id") {
    const idRegex = /^\d{13}$/;
    if (!idRegex.test(value)) return "ID Number must be 13 digits";
  } else if (type === "passport") {
    const passportRegex = /^[a-zA-Z0-9]{6,9}$/;
    if (!passportRegex.test(value))
      return "Passport must be 6–9 alphanumeric chars";
  }

  return "";
};

export const validateEmail = (email) => {
  if (!email) return "Email is required";

  const emailRegex = /^[^\s@]+@singular\.co\.za$/;
  if (!emailRegex.test(email)) {
    return "Email must be a singular.co.za address";
  }

  return "";
};

export const validateRequiredFields = (employee) => {
  const requiredFields = [
    "firstName",
    "lastName",
    "idNumber",
    "dateOfBirth",
    "contactNumber",
    "maritalStatus",
    "homeAddress",
    "postalCode",
    "city",
    "gender",
    "startDate",
    "department",
    "employeeNumber",
    "jobTitle",
    "employeeStatus",
    "reportsTo",
    "email",
    "documentPath",
  ];

  const errors = {};
  requiredFields.forEach((field) => {
    if (!employee[field] || employee[field].toString().trim() === "") {
      errors[field] = "This field is required";
    }
  });

  const idErr = validateIdNumber(employee.idType, employee.idNumber);
  if (idErr) errors.idNumber = idErr;

  const emailErr = validateEmail(employee.email);
  if (emailErr) errors.email = emailErr;

  return { errors, isValid: Object.keys(errors).length === 0 };
};

export const handleFileChange = async (
  e,
  setEmployee,
  setUploading,
  setErrorMessage
) => {
  const file = e.target.files[0];
  if (file && (file.type === "image/jpeg" || file.type === "image/jpg")) {
    try {
      setUploading(true);
      setErrorMessage("");
      const formData = new FormData();
      formData.append("file", file);
      formData.append("upload_preset", "unsigned_preset");
      formData.append("folder", "samples/ecommerce");

      const response = await axios.post(
        "https://api.cloudinary.com/v1_1/djmafre5k/image/upload",
        formData
      );

      const imageUrl = response.data.secure_url;
      setEmployee((prev) => ({ ...prev, documentPath: imageUrl }));
      setUploading(false);
    } catch (error) {
      console.error("Error uploading image:", error);
      setErrorMessage("Error uploading image.");
      setUploading(false);
    }
  } else {
    setErrorMessage("Only .jpg or .jpeg images are allowed.");
    setEmployee((prev) => ({ ...prev, documentPath: "" }));
  }
};

/// </summary>
// --- DATE FORMATTER ---
/// </summary>
export const formatDateToYYYYMMDD = (dateStr) => {
  if (!dateStr) return null;

  const date = new Date(dateStr);
  if (isNaN(date.getTime())) return null;

  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, "0");
  const day = String(date.getDate()).padStart(2, "0");

  return `${year}-${month}-${day}`;
};

export const formatDateForDisplay = (isoDateStr) => {
  if (!isoDateStr) return "";
  const date = new Date(isoDateStr);
  const day = String(date.getDate()).padStart(2, "0");
  const month = String(date.getMonth() + 1).padStart(2, "0");
  const year = date.getFullYear();
  return `${day}/${month}/${year}`;
};

export const convertDDMMYYYYtoISO = (ddmmyyyy) => {
  if (!ddmmyyyy) return null;
  const parts = ddmmyyyy.split("/");
  if (parts.length !== 3) return null;
  const [dd, mm, yyyy] = parts;

  /// </summary>
  /// Build ISO-compatible date string yyyy-mm-dd
  /// </summary>
  const isoString = `${yyyy}-${mm}-${dd}`;
  return toISOStringSafe(isoString);
};

export const handleInputChange = (
  e,
  employee,
  setEmployee,
  setIdNumberError,
  employeesList = []
) => {
  const { name, value, type, checked } = e.target;

  setEmployee((prev) => {
    let updated = {
      ...prev,
      [name]: type === "checkbox" ? checked : value,
    };

    if (name === "lastName") {
      const existingCount = employeesList.filter((emp) =>
        emp.lastName.toLowerCase().startsWith(value.toLowerCase())
      ).length;
      updated.employeeNumber = generateEmployeeNumber(value, existingCount);
    }

    if (name === "idNumber" || name === "idType") {
      const idType = name === "idType" ? value : employee.idType;
      const idNum = name === "idNumber" ? value : employee.idNumber;
      const error = validateIdNumber(idType, idNum);
      setIdNumberError(error);

      const extracted =
        idType === "id"
          ? populateFromIdNumber(idNum)
          : populateFromPassportNumber(idNum);
      updated = { ...updated, ...extracted };
    }

    return updated;
  });
};

/// </summary>
/// Promise-based confirmation toast
/// </summary>
export const showConfirmationToast = (message) => {
  return new Promise((resolve) => {
    const toastId = toast(
      ({ closeToast }) => (
        <div className="toast-confirm-container">
          <div>{message}</div>
          <div className="toast-confirm-buttons">
            <button
              className="toast-confirm-btn confirm"
              onClick={() => {
                toast.dismiss(toastId);
                resolve(true);
              }}
            >
              Confirm
            </button>
            <button
              className="toast-confirm-btn cancel"
              onClick={() => {
                toast.dismiss(toastId);
                resolve(false);
              }}
            >
              Cancel
            </button>
          </div>
        </div>
      ),
      {
        autoClose: false,
        closeOnClick: false,
        draggable: false,
      }
    );
  });
};
