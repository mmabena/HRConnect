import {createContext, useCallback, useContext, useState} from "react";
import medicalOptionServices from '../../../../../Components/Services/medicalOptionServices.js';
import MedicalAidOptionsValidator from '../../../../../utils/medicalAidOptionsValidator'

const MedicalAidOptions = createContext();

export const useMedicalAidOptionContext = () => {
  const context = useContext(MedicalAidOptions);

  if(!context) {
    throw new Error('useMedicalAidOptionContext must be used within a MedicalAidOptionsProvider');
  }

  return context;
};

export const MedicalAidOptionsProvider = ({children}) => {
  const [medicalAidOptions, setMedicalAidOptions] = useState([]);
  const [medicalAidOptionsCategories, setMedicalAidOptionsCategories] = useState([]);
  const [salaryBasedOptions, setSalaryBasedOptions] = useState([]);
  const [eligibleOptionsForEmployee, setEligibleOptionsForEmployee] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  // Api Service Layer Calls
  const getAllOptionsGroupedByCategory = useCallback( async () => {
    try{
      setLoading(true);
      setError(null);

      const data = await medicalOptionServices.getAllOptionsGroupedByCategory();
      const response = Array.isArray(data) ? data : Array.isArray(data.data) ? data.data : [];
      setMedicalAidOptions(response);
        return response;
    }
    catch(error){
      setError(error || "Failed to fetch grouped medical aid options");
      console.error('Error getting medical aid options:', error);
    }
    finally{
      setLoading(false);
    }
  }, []);

  const getAllMedicalOptionsCategories = useCallback( async () => {
    try{
      setLoading(true);
      setError(null);

      //make api service call
        const data = await medicalOptionServices.getAllMedicalOptionsCategories();
        const response = Array.isArray(data) ? data.flat() : Array.isArray(data.data) ? data.data.flat() : [];
      setMedicalAidOptionsCategories(response);
        return response;
    }
    catch(error){
      setError(error || "Failed to fetch all medical aid option categories");
      console.error('Error getting list of medical aid options categories: ', error);
    }
    finally{
        setLoading(false);
    }
  }, []);

    const getMedicalOptionsSnapshot = useCallback(async () => {
        try {
            setLoading(true);
            setError(null);

            const data = await medicalOptionServices.getMedicalOptionsSnapshot();
            console.log('Dump from Context : ', data);
            const response = Array.isArray(data) ? data.flat() : Array.isArray(data.data) ? data.data.flat() : [];
            setMedicalAidOptions(response);
            return response;
        } catch (error) {
            setError(error || "Failed to fetch medical aid options snapshot");
            console.error('Error retrieving medical options data : ', error);
        } finally {
            setLoading(false);
    }
  }, []);

  const getCategoryById = useCallback(async (categoryId) => {
    try{
      setLoading(true);
      setError(null);

      //Validate input
      const sanitized = MedicalAidOptionsValidator.sanitizeInput(categoryId);
      const validatedInput = MedicalAidOptionsValidator.validateCategoryId(sanitized);

      if(!validatedInput.isValid){
          throw new Error(validatedInput.error.toString());
      }

      const data = await medicalOptionServices.getCategoryById(sanitized);
      const response = Array.isArray(data) ? data : Array.isArray(data.data) ? data.data : [];
      setMedicalAidOptionsCategories(response);
        return response;
    }
    catch(error){
      setError(error || "Failed to fetch medical aid option category by id");
      console.error('Error fetching medical aid option category by id: ', error);
    }
    finally{
      setLoading(false);
    }
  }, []);

  const getMedicalOptionsByCategoryId = useCallback( async (categoryId) => {
    try{
      setLoading(true);
      setError(null);

      //validate Input
      const sanitized = MedicalAidOptionsValidator.sanitizeInput(categoryId);
      const validatedInput = MedicalAidOptionsValidator.validateCategoryId(categoryId);

      if(!validatedInput.isValid){
        throw new Error(validatedInput.error.toString());
      }

      const data = await medicalOptionServices.getMedicalOptionsByCategoryId(sanitized);
      const response = Array.isArray(data) ? data : Array.isArray(data.data) ? data.data : [];
      setMedicalAidOptions(response);
        return response;
    }
    catch (error){
      setError(error || "Failed to fetch medical aid options category by id: ', error);")
      console.error("Error fetching medical aid options by their category id");
    }
    finally{
      setLoading(false);
    }
  },[]);

  const getMedicalOptionsSalaryBracketMatchingEmployeeSalary = useCallback( async (salaryAmount) => {
    try{
      setLoading(true);
      setError(null);

      // Validate Input
      const sanitized = MedicalAidOptionsValidator.sanitizeInput(salaryAmount);
      const validAmount = MedicalAidOptionsValidator.validateDecimalWithPrecision(sanitized)
        
      if(!validAmount.isValid){
        throw new Error(validAmount.error.toString());
      }
      
      const data = await medicalOptionServices.getMedicalOptionsSalaryBracketMatchingEmployeeSalary(sanitized);
      const response = Array.isArray(data) ? data : Array.isArray(data.data) ? data.data : [];
      setSalaryBasedOptions(response);
        return response;
    }
    catch(error){
      setError(error || "Failed to fetch medical aid options matching employee salary");
      console.error('Error fetching medical aid options matching employee salary: ', error);
    }
    finally{
      setLoading(false);
    }
  }, []);

  const getMemberEligibilityOptionsByEmployeeId = useCallback( async (employeeId) => {
    try{
      setLoading(true);
      setError(null);
      
      // Validate input
      const sanitized = MedicalAidOptionsValidator.sanitizeInput(employeeId);
      
      if(sanitized === null || sanitized === '' || sanitized === undefined){
        throw new Error("Invalid employee ID provided. Employee ID cannot be empty");
      }
      
      const data =  await medicalOptionServices.getMemberEligibilityOptionsByEmployeeId(sanitized);
      const response = Array.isArray(data) ? data : Array.isArray(data.data) ? data.data : [];
      setEligibleOptionsForEmployee(response);
        return response;
    }
    catch(error){
      setError(error || "Failed to fetch eligible medical aid options for employee");
      console.error(`Error fetching eligible options for employee : ${error}`);
    }
    finally{
      setLoading(false);
    }
  }, []);

  //POSTS
  const createMedicalOptionCategory = useCallback(async (request) => {
    try{
      setLoading(true);
      setError(null);
        
      // Validate Input
      const sanitizedRequest = MedicalAidOptionsValidator.sanitizeJSON(request);

      if(!sanitizedRequest.isValid){
          throw new Error(sanitizedRequest.error.toString());
      }

      const data = medicalOptionServices.createMedicalOptionCategory(sanitizedRequest.data);
      const response = Array.isArray(data) ? data : Array.isArray(data.data) ? data.data : [];
      setMedicalAidOptionsCategories(response);
        return response;
    }
    catch(error){
      setError(error || "Failed to create medical aid option category");
      console.error('Error creating medical aid option category: ', error);
    }
    finally{
      setLoading(false);
    }
  }, []);

  const createBulkMedicalOptionCategoryOptionsByCategoryId = useCallback(async (categoryId,request) => {
    try{
      setLoading(true);
      setError(null);

      // Validate Input
      const sanitizedId = MedicalAidOptionsValidator.sanitizeInput(categoryId);
      const sanitizedRequest = MedicalAidOptionsValidator.sanitizeJSON(request);
      const sanitizedCategoryId = MedicalAidOptionsValidator.validateCategoryId(sanitizedId);

      if(!sanitizedRequest.isValid && !sanitizedCategoryId.isValid){
          throw new Error(sanitizedRequest.error.toString() + '\n' + sanitizedCategoryId.error.toString());
      }

      const data = medicalOptionServices.createBulkMedicalOptionCategoryOptionsByCategoryId(sanitizedId, sanitizedRequest.data);
      const response = Array.isArray(data) ? data : Array.isArray(data.data) ? data.data : [];
      //setMedicalAidOptions(response);
        return response;
    }
    catch(error){
      setError(error || "Failed to create bulk medical aid options by category");
      console.error('Error creating bulk medical aid options by category: ', error);
    }
    finally{
      setLoading(false);
    }
  }, []);

  //PUTS
  const updateCategoryById = useCallback(async (categoryId, request) => {
    try{
      setLoading(true);
      setError(null);

      // Validate Input
      //const sanitizedId = MedicalAidOptionsValidator.sanitizeInput(categoryId);
      //const sanitizedRequest = MedicalAidOptionsValidator.sanitizeJSON(request);
      //const sanitizedCategoryId = MedicalAidOptionsValidator.validateCategoryId(sanitizedId);

      //if(!sanitizedRequest.isValid && !sanitizedCategoryId.isValid){
      //    throw new Error(sanitizedRequest.error.toString() + '\n' + sanitizedCategoryId.error.toString());
      //}

      const data = medicalOptionServices.updateCategoryById(categoryId, request);
      const response = Array.isArray(data) ? data : Array.isArray(data.data) ? data.data : [];
      //setMedicalAidOptionsCategories(response);
      return response;

    }
    catch(error){
      setError(error || "Failed to update medical aid option category by id");
      console.error('Error updating medical aid option category by id: ', error);
    }
    finally{
      setLoading(false);
    }
  }, []);

  const updateBulkMedicalOptionsByCategoryId = useCallback( async (categoryId, request) => {
    try{
      setLoading(true);
      setError(null);

      // Validate Input
      //const sanitizedId = MedicalAidOptionsValidator.sanitizeInput(categoryId);
      //const sanitizedRequest = MedicalAidOptionsValidator.sanitizeJSON(request);
      //const sanitizedCategoryId = MedicalAidOptionsValidator.validateCategoryId(sanitizedId);

      //if(!sanitizedRequest.isValid && !sanitizedCategoryId.isValid){
      //    throw new Error(sanitizedRequest.error.toString() + '\n' + sanitizedCategoryId.error.toString());
      //}

      const data = medicalOptionServices.updateBulkMedicalOptionsByCategoryId(categoryId, request);
      const response = Array.isArray(data) ? data : Array.isArray(data.data) ? data.data : [];
      // use effect to reflect changes
        return response;
    }
    catch(error){
      setError(error || "Failed to update medical aid option category by category");
      console.error('Error updating medical aid option category by id: ', error);
    }
    finally{
      setLoading(false);
    }
  }, []);

  const value = {
      medicalAidOptions,
      medicalAidOptionsCategories,
      salaryBasedOptions,
      eligibleOptionsForEmployee,
      loading,
      error,
      // Callback Functions
      getAllOptionsGroupedByCategory,
      getAllMedicalOptionsCategories,
      getMedicalOptionsSnapshot,
      getCategoryById,
      getMedicalOptionsByCategoryId,
      getMedicalOptionsSalaryBracketMatchingEmployeeSalary,
      getMemberEligibilityOptionsByEmployeeId,
      createMedicalOptionCategory,
      createBulkMedicalOptionCategoryOptionsByCategoryId,
      updateCategoryById,
      updateBulkMedicalOptionsByCategoryId,
  };

  return (
    <MedicalAidOptions.Provider value={value}>
        {children}
    </MedicalAidOptions.Provider>
  );
};