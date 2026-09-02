import { useState } from "react";


/**
 * Custom React hook that manages banking details form state,
 * form validation errors, and input change handling.
 *
 * @param {Object} initialValue - The initial structure of the banking details form data.
 * @returns {Object} Contains banking details data, form errors, and handlers for updating them.
 */

const useBankingDetailsForm = (initialValue) => {
  const [banking, setBanking] = useState(initialValue);
  const [formErrors, setFormErrors] = useState({});

/**
 * Handles input changes for the banking details form.
 * @param {Event} e - The input change event.
 */

  const onInputChange = (e) => {
    const { name, value } = e.target;


    setBanking((prev) => ({ ...prev, [name]: null }));

  
  };

  return {
     banking,
      setBanking,
       onInputChange,
        formErrors,
         setFormErrors };

};

export default useBankingDetailsForm;
