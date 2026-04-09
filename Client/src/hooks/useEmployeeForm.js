import { useState } from "react";
import { populateFromIdNumber } from "../api/Employee";

/**
 * Custom React hook that manages employee form state,
 * form validation errors, and input change handling.
 *
 * @param {Object} initialState - The initial structure of the employee form data.
 * @returns {Object} Contains employee data, form errors, and handlers for updating them.
 */
const useEmployeeForm = (initialState) => {
  const [employee, setEmployee] = useState(initialState);
  const [formErrors, setFormErrors] = useState({});
  /**
   * Handles changes for all form inputs and updates the
   * corresponding employee state field.
   *
   * @param {Event} e - The input change event
   */
  const onInputChange = (e) => {
    const { name, value } = e.target;

    setFormErrors((prev) => ({ ...prev, [name]: null }));
    /**
     * Handles disability selection logic.
     * Converts "yes/no" values into a boolean and
     * clears disabilityType if "no" is selected.
     */
    if (name === "disability") {
      setEmployee((prev) => ({
        ...prev,
        disability: value === "yes",
        disabilityType: value === "no" ? "" : prev.disabilityType,
      }));
      return;
    }
    
    if (name === "jobTitle") {
      setEmployee((prev) => ({
        ...prev,
        jobTitle: value,
      }));
      return;
    }
    /**
     * When an ID number is entered and the ID type is "id",
     * automatically derive date of birth, gender, and nationality.
     */
    if (name === "idNumber" && employee.idType === "id") {
      const derived = populateFromIdNumber(value);

      setEmployee((prev) => ({
        ...prev,
        idNumber: value,
        dateOfBirth: derived.dateOfBirth || "",
        gender: derived.gender || "",
        nationality: derived.nationality || "",
      }));

      return;
    }

    if (name === "passportNumber" && employee.idType === "passport") {
      setEmployee((prev) => ({
        ...prev,
        passportNumber: value,
      }));
      return;
    }

    setEmployee((prev) => ({ ...prev, [name]: value }));
  };

  return {
    employee,
    setEmployee,
    formErrors,
    setFormErrors,
    onInputChange,
  };
};

export default useEmployeeForm;