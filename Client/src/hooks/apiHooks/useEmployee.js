import { useCallback } from "react";

import {
  addEmployee,
  editEmployee,
  validateEmployee,
  fetchEmployeeByIdNumber,
  GetEmployeeByEmployeeNumberAsync,
  fetchAllEmployees,
} from "../../api/Employee";

const useEmployee = () => {
  const createEmployee = useCallback(async (employee) => {
    try {
      const response = await addEmployee(employee);

      return response;
    } catch (error) {
      if (error.response) {
        console.error("Add employee error response data:", error.response.data);
        console.error("Add employee error status:", error.response.status);
      } else {
        console.error("Add employee error message:", error.message);
      }
      throw error;
    }
  }, []);

  const updateEmployee = useCallback(async (employeeId, employee) => {
    try {
      const response = await editEmployee(employeeId, employee);

      return response;
    } catch (error) {
      if (error.response) {
        console.error("Edit employee error response data:", error.response.data);
        console.error("Edit employee error status:", error.response.status);
      } else {
        console.error("Edit employee error message:", error.message);
      }
      throw error;
    }
  }, []);

  const employeeValidation = useCallback(async (employee) => {
    try {
      const response = await validateEmployee(employee);

      return response;
    } catch (error) {
      if (error.response) {
        console.error("Add employee error response data:", error.response.data);
        console.error("Add employee error status:", error.response.status);
      } else {
        console.error("Add employee error message:", error.message);
      }
      throw error;
    }
  }, []);

  const getEmployeeById = useCallback(async (idNumber) => {
    try {
      const response = await fetchEmployeeByIdNumber(idNumber);

      return response;
    } catch (error) {
      if (error.response) {
        console.error("Fetch employee by ID number error response data:", error.response.data);
        console.error("Fetch employee by ID number error status:", error.response.status);
      } else {
        console.error("Fetch employee by ID number error message:", error.message);
      }
      throw error;
    }
  }, []);

  const fetchEmployeeByEmpNumebr = useCallback(async (employeeId) => {
    try {
      const response = await GetEmployeeByEmployeeNumberAsync(employeeId);
      return response;
    } catch (error) {
      console.error("Fetch employee by employee number error:", error);
      throw error;
    }
  }, []);

  const getAllEmployees = useCallback(async () => {
    try {
      const response = await fetchAllEmployees();

      return response;
    } catch (error) {
      if (error.response) {
        console.error("Fetch employees error response data:", error.response.data);
        console.error("Fetch employees error status:", error.response.status);
      } else {
        console.error("Fetch employees error message:", error.message);
      }
      throw error;
    }
  }, []);

  return {
    createEmployee,
    updateEmployee,
    employeeValidation,
    getEmployeeById,
    fetchEmployeeByEmpNumebr,
    getAllEmployees,

  };
};

export default useEmployee;

