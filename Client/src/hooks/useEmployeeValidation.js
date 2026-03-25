const useEmployeeValidation = () => {
  const validateEmployee = (employee) => {
    const errors = {};

    if (!employee.title) errors.title = "Title is required";

    if (!employee.name?.trim()) errors.name = "First name is required";

    if (!employee.surname?.trim()) errors.surname = "Last name is required";

    if (employee.idType === "id") {
      if (!employee.idNumber?.trim()) errors.idNumber = "ID Number is required";
      else if (!/^\d{13}$/.test(employee.idNumber))
        errors.idNumber = "ID Number must be 13 digits";
    }

    if (employee.idType === "passport") {
      if (!employee.passportNumber?.trim()) {
        errors.passportNumber = "Passport Number is required";
      } else if (!/^[a-zA-Z0-9]{9,}$/.test(employee.passportNumber)) {
        errors.passportNumber =
          "Passport must be at least 9 alphanumeric characters";
      }
    }

    if (!employee.contactNumber?.trim())
      errors.contactNumber = "Contact number is required";

    if (!employee.nationality?.trim())
      errors.nationality = "Nationality is required";

    if (!employee.gender?.trim()) errors.gender = "Gender is required";

    if (!employee.email?.trim()) errors.email = "Email is required";

    if (!employee.physicalAddress?.trim())
      errors.physicalAddress = "Home address is required";

    if (!employee.city?.trim()) errors.city = "City is required";

    if (!employee.zipCode?.trim()) errors.zipCode = "Postal code is required";

    if (!employee.branch) errors.branch = "Department is required";

    if (!employee.monthlySalary)
      errors.monthlySalary = "Monthly salary is required";

    if (!employee.employeeStatus)
      errors.employeeStatus = "Employment status is required";

    if (!employee.reportsTo) errors.reportsTo = "Career manager is required";

    if (!employee.startDate) errors.startDate = "Start date is required";

    if (!employee.jobTitle) errors.jobTitle = "Job title is required";

    if (!employee.profileImage)
      errors.profileImage = "Profile Image is required";

    if (!employee.dateOfBirth) errors.dateOfBirth = "Date of Birth is required";

    if (!employee.taxNumber) errors.taxNumber = "Tax Number is required";

    if (employee.disability && !employee.disabilityType?.trim())
      errors.disabilityType = "Disability description required";

    return errors;
  };

  return { validateEmployee };
};

export default useEmployeeValidation;
