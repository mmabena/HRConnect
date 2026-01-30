import { useState, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import AddCompanyUI from './Components/companyManagement/addCompanyUI.jsx';
import './Navy.css';
import { addCompany } from './Components/Services/companyService.js';
import { INDUSTRY_OPTIONS } from './utils/constants.js'
import { validateCompanyForm } from './utils/companyValidators.js'; 

function AddCompany() {
  const navigate = useNavigate();

  const [formState, setFormState] = useState({
    companyName: '',
    registrationNumber: '',
    taxNumber: '',
    sdlNumber: '',
    email: '',
    phoneNumber: '',
    physicalAddress: '',
    industry: 'Select Industry',
  });

  const [validationErrors, setValidationErrors] = useState({});

  // industryOptions is now imported
  const industryOptions = INDUSTRY_OPTIONS;

  const handleChange = useCallback((field, value) => {
    setFormState(prev => ({ ...prev, [field]: value }));
    if (validationErrors[field]) {
      setValidationErrors(prev => {
        const newErrors = { ...prev };
        delete newErrors[field];
        return newErrors;
      });
    }
  }, [validationErrors]);

  // validateForm now uses the external validator
  const validateForm = useCallback(() => {
    const { errors, isValid } = validateCompanyForm(formState);
    setValidationErrors(errors);
    return isValid;
  }, [formState]);


  // handleSave is responsible for validating the form data and submitting it
  // to the backend API via the addCompany service.
  const handleSave = useCallback(async (e) => {
    e.preventDefault();
    if (validateForm()) {
      const companyData = {
        ...formState,
      };

      try {
        // Calls the API to add a new company
        const response = await addCompany(companyData);
        if (response.status === 201) {
          alert('Company added successfully!');
          navigate('/companyManagement?tab=Manage Companies');
        }
      } catch (error) {
        if (error.response && error.response.status === 400) {
          const errorData = error.response.data.errors || {};
          setValidationErrors(errorData);
          alert('Validation failed. Please check the form.');
        } else {
          console.error('Failed to save company:', error);
          alert('An error occurred while saving the company. Please try again.');
        }
      }
    }
  }, [validateForm, formState, navigate]);


  const handleCancel = useCallback(() => {
    navigate('/companyManagement?tab=Manage Companies');
  }, [navigate]);

  const handlers = {
    handleSave,
    handleCancel,
    handleChange,
  };

  const config = {
    industryOptions,
    validationErrors,
  };

  return (
    <AddCompanyUI
      formState={formState}
      handlers={handlers}
      config={config}
    />
  );
}

export default AddCompany;