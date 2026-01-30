export const validateCompanyForm = (formState) => {
  let errors = {};
  let isValid = true;

  if (!formState.companyName.trim()) {
    errors.companyName = 'Company Name is required.';
    isValid = false;
  } else if (formState.companyName.length > 255) {
    errors.companyName = 'Company Name cannot exceed 255 characters.';
    isValid = false;
  }

  if (!formState.registrationNumber.trim()) {
    errors.registrationNumber = 'Registration Number is required.';
    isValid = false;
  } else if (!/^(\d{4}\/\d{6}\/\d{2}|[CKNOT]\d{4}\/\d{6})$/.test(formState.registrationNumber)) {
    errors.registrationNumber = 'Invalid South African Registration Number format. Expected formats: YYYY/NNNNNN/XX or [RegionCode]YYYY/NNNNNN.';
    isValid = false;
  } else if (formState.registrationNumber.length > 50) {
    errors.registrationNumber = 'Registration Number cannot exceed 50 characters.';
    isValid = false;
  }

  if (!formState.taxNumber.trim()) {
    errors.taxNumber = 'Tax Number is required.';
    isValid = false;
  } else if (!/^[01239]\d{9}$/.test(formState.taxNumber)) {
    errors.taxNumber = 'Invalid South African Tax Number format. Expected 10 digits starting with 0, 1, 2, 3, or 9.';
    isValid = false;
  } else if (formState.taxNumber.length > 50) {
    errors.taxNumber = 'Tax Number cannot exceed 50 characters.';
    isValid = false;
  }

  if (!formState.sdlNumber.trim()) {
    errors.sdlNumber = 'SDL Number is required.';
    isValid = false;
  } else if (!/^7\d{9}$/.test(formState.sdlNumber)) {
    errors.sdlNumber = 'Invalid South African SDL Number format. Expected 10 digits starting with 7.';
    isValid = false;
  } else if (formState.sdlNumber.length > 50) {
    errors.sdlNumber = 'SDL Number cannot exceed 50 characters.';
    isValid = false;
  }

  if (!formState.email.trim()) {
    errors.email = 'Email Address is required.';
    isValid = false;
  } else if (!/^[^@\s]+@[^@\s]+\.[^@\s]+$/.test(formState.email)) {
    errors.email = 'Invalid Email Address format.';
    isValid = false;
  } else if (formState.email.length > 255) {
    errors.email = 'Email Address cannot exceed 255 characters.';
    isValid = false;
  }

  if (!formState.phoneNumber.trim()) {
    errors.phoneNumber = 'Phone Number is required.';
    isValid = false;
  } else if (!/^(\+27|0)[1-8][0-9]{8}$/.test(formState.phoneNumber)) {
    errors.phoneNumber = 'Invalid South African Phone Number format. Examples: 0821234567 or +27821234567.';
    isValid = false;
  } else if (formState.phoneNumber.length > 50) {
    errors.phoneNumber = 'Phone Number cannot exceed 50 characters.';
    isValid = false;
  }

  if (!formState.physicalAddress.trim()) {
    errors.physicalAddress = 'Physical Address is a core business requirement.';
    isValid = false;
  } else if (formState.physicalAddress.length > 500) {
    errors.physicalAddress = 'Physical Address cannot exceed 500 characters.';
    isValid = false;
  }

  if (formState.industry === 'Select Industry') {
    errors.industry = 'Industry is a core business requirement.';
    isValid = false;
  }

  return { errors, isValid };
};