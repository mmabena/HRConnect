import DOMPurify from 'dompurify';

/**
 *  Medical Aid options validator Helper Class
 *  Provides validation utilities for Medical Aid options, categories, and related numeric inputs
 */

class MedicalAidOptionsValidator {
  /**
   * Sanitizes input to prevent XSS attacks
   * @param {any} value - The value to sanitize
   * @return {string} sanitized string value
   */
   static sanitizeInput(value){
     // Convert to string
     let stringValue = String(value || '');

     // Use DOMPurify to remove any HTML/script tags
     const clean = DOMPurify.sanitize(stringValue, {
         ALLOWED_TAGS: [],      // No HTML tags allowed
         ALLOWED_ATTR: [],      // No attributes allowed
         KEEP_CONTENT:true      // Keep text content
     });

     return clean.trim();
  }

  /**
   * Safely parses and sanitizes JSON input
   * @param {string|object} value - The JSON string or Object to sanitize
   * @returns {object} - {isValid: boolean, data: object|null, error: string|null}
   */
   static sanitizeJSON(value){
     try{
       let jsonObject;

       // If already and object, use it, else parse it
       if(typeof value === 'object' && value !== null){
         jsonObject = value;
       }
       else if (typeof value === 'object' || value === 'string'){
         // Attempt to parse it
         jsonObject = JSON.parse(value);
       }
       else {
         return{
           isValid: false,
           data: null,
           error: 'input must be a valid JSON object or string'
         };
       }

       // Sanitize each property
       const sanitized = {};
       for(const key in jsonObject){
         if(Object.prototype.hasOwnProperty.call(jsonObject, key)) {
           const sanitizedKey = this.sanitizeInput(key);
           const sanitizedValue = this.sanitizeInput(jsonObject[key]);
           sanitized[sanitizedKey] = sanitizedValue;
         }
       }

       return {
         isValid: true,
         data: sanitized,
         error: null
       };
     }
     catch (error) {
       return {
         isValid: false,
         data: null,
         error: `Invalid JSON: ${error}}`
       };
     }
   }


  /**
   *  Validates if a values is a real number >= 1 (1 to positive infinity)
   *  @params {number|string} value - the value to validate
   *  @returns {object} - { isValid: boolean, error: string|null}
   */
  static validateRealNumber(value){
    // Sanitize the input
    const sanitizedValue  = this.sanitizeInput(value);

    // Check if value is empty
    if (sanitizedValue === null || sanitizedValue === undefined || sanitizedValue === '') {
        return {
            isValid: false,
            errors: 'value cannot be empty'
        };
    }

    const num = Number(sanitizedValue);

    // Check if it is a valid number
    if(isNaN(num)) {
      return {
          isValid: false,
          error: 'value must be an integer/valid positive number'
      };
    }
    // Check if it is finite (not Infinity or -Infinity)
    if(!isFinite(num)) {
      return {
        isValid: false,
        erorr: 'Value must be a finite number'
      };
    }

    // Check if it's >= 1
    if(num < 1){
      return {
        isValid: false,
        error: 'Value must be 1 or greater than 1'
      }
    }

    return{
      isValid: true,
      error: null
    }
  }

  /*
  *  Validate if a value is a positive integer (1 to infinity)
  *  @params {number|string} value - The value to validate
  *  @params {object} - {isValid: boolean, error: string|null}
  */
  static validatePositiveInteger(value){
    const numValidation = this.validateRealNumber(value);

    if(!numValidation.isValid){
    return {numValidation}
  }

    const num = Number(this.sanitizeInput(value));

    if(!Number.isInteger(num)){
      return {
      isValid: false,
      error: 'Value must be a whole number'
      }
    }

    return {
        isValid: true,
        error: null
    };
  }

 /**
  *  Validates if a value is a positive decimal (float)
  *  Allows numbers like 1.5, 2.99, 100.001, etc.
  *  @params {number|string|decimal} value - the value to validate
  *  @returns {object} - {isValid: boolean, error: string|null}
  */
  static validatePositiveDecimal(value) {
    // Sanitize the input
    const sanitizedValue  = this.sanitizeInput(value);
    
    // Check if value is null
    if(sanitizedValue === '' || sanitizedValue === null) {
      return{
        isValid: false,
        error: 'Value cannot be empty'
      };
    }
    
    const num = Number(sanitizedValue);
    
    // Check if it's a valid number
    if(isNaN(num)){
      return {
        isValid: false,
        error: 'Value must be a valid whole number'
      };
    }
    if(!isFinite(num)){
      return{
      isValid: false,
      error: 'Value must be a finite number'
      };
    }
    if(num <= 0){
      return{
        isValid: false,
        error: 'Value must be greater than 0'
      };
    }

    return{
      isValid: true,
      error: null
    };
  }

  /**
   * Validates if a value is a decimal with specific decimal places
   * @param {number|string} value - The value to validate
   * @param {number} maxDecimalPlaces - Maximum decimal places allowed (default: 2)
   * @returns {object} - { isValid: boolean, error: string|null }
   */
  static validateDecimalWithPrecision(value, maxDecimalPlaces ) {
    const sanitizedValue = this.sanitizeInput(value);

    if(sanitizedValue === '' || sanitizedValue === null) {
      return {
        isValid: false,
        error: 'Value cannot be empty'
      };
    }

    const num = Number(sanitizedValue);

    if(isNaN(num)) {
      return {
        isValid: false,
        error: 'Value must be a valid number'
      };
    }

    if(!isFinite(num))
    {
      return {
        isValid: false,
        error: 'Value must be a finite number'
      };
    }

    if(num <= 0) {
      return {
        isValid: false,
        error: 'Value must be greater than 0'
      }
    }

    // Check decimal places
    const decimalPart = sanitizedValue.split('.')[1];

    if (decimalPart && decimalPart.length > maxDecimalPlaces) {
      return {
        isvalid: false,
        error: `Value cannot have more than ${maxDecimalPlaces} decimal places`
      };
    }

    return {
      isValid: true,
        error: null
    };
  }


 /**
   * validate if Salary Amount is a floating point decimal/integer
   * @parmas {number|string|decimal} value - The value to validate
   * @returns {object} - {isValid: boolean, error: string|null}
   */
  static validateSalaryAmount(value){
    // Salary should be positive decimal with max 2 decimal places
    return this.validateDecimalWithPrecision(value, 2);
  }
  
 /**
  * Validates category ID
  * @param {number|string} categoryId - The category ID to validate
  * @returns {object} - {isValid: boolean, error: string|null}
  */
  static validateCategoryId(categoryId){
    return this.validatePositiveInteger(categoryId);
  }

 /**
  * Validate category name
  * @param {string} name -  The category name to validate
  * @returns {object} - {isValid: boolean, error: string|null}
  */
  static validateCategoryName(name){
    const sanitized = this.sanitizeInput(name);

    if(!sanitized || typeof sanitized !== 'string'){
      return {
        isValid: false,
        error: 'Category name must be a non-empty string'};
    }
    if (sanitized.length < 2) {
      return{
        isValid: false,
        error: 'Category name must be at least 2 characters long'
      };
    }
    if(sanitized.length > 100){
      return {
        isValid: false,
        error: 'Category name must not exceed 100 characters'
      };
    }

    return {
      isValid: true,
      error: null
    };
  }
}

export default MedicalAidOptionsValidator;