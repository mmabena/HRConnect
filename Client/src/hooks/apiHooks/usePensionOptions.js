import { useCallback } from "react";

import {
  fetchAllPensionOptions,
  createPensionFund,
  selectEmployeePensionOption,
  enrollEmployeeInPension,
  fetchAllEmployeePensionEnrollments,
  fetchEmployeePensionEnrollment,
  fetchPensionEnrollmentsByPayrollRun,
  updateEmployeePensionEnrollment,
} from "../../api/PenstionOptions";

const usePensionOptions = () => {
  const getAllPensionOptions = useCallback(async () => {
    try {
      const response = await fetchAllPensionOptions();

      return response;
    } catch (error) {
      if (error.response) {
        console.error(
          "Fetch pension options error response data:",
          error.response.data,
        );
        console.error(
          "Fetch pension options error status:",
          error.response.status,
        );
      } else {
        console.error("Fetch pension options error message:", error.message);
      }

      throw error;
    }
  }, []);

  const addPensionFund = useCallback(async (pensionFundData) => {
    try {
      const response = await createPensionFund(pensionFundData);

      return response;
    } catch (error) {
      if (error.response) {
        console.error(
          "Create pension fund error response data:",
          error.response.data,
        );
        console.error(
          "Create pension fund error status:",
          error.response.status,
        );
      } else {
        console.error("Create pension fund error message:", error.message);
      }

      throw error;
    }
  }, []);

  const selectEmployeeByPensionOption = useCallback(
    async (employeeId, pensionOptionId) => {
      try {
        const response = await selectEmployeePensionOption(
          employeeId,
          pensionOptionId,
        );

        return response;
      } catch (error) {
        if (error.response) {
          console.error(
            "Select employee pension option error response data:",
            error.response.data,
          );
          console.error(
            "Select employee pension option error status:",
            error.response.status,
          );
        } else {
          console.error(
            "Select employee pension option error message:",
            error.message,
          );
        }

        throw error;
      }
    }, []);

  const enrollEmployeePension = useCallback(
    async (
      employeeId,
      effectiveDate,
      voluntaryContribution,
      IsVoluntaryContributionPermanent,
    ) => {
      try {
        const response = await enrollEmployeeInPension(
          employeeId,
          effectiveDate,
          voluntaryContribution,
          IsVoluntaryContributionPermanent,
        );

        return response;
      } catch (error) {
        if (error.response) {
          console.error(
            "Enroll employee in pension error response data:",
            error.response.data,
          );
          console.error(
            "Enroll employee in pension error status:",
            error.response.status,
          );
        } else {
          console.error(
            "Enroll employee in pension error message:",
            error.message,
          );
        }

        throw error;
      }
    }, []);


    const getAllEmployeePensionEnrollments = useCallback(async () => {
    try {
      const response = await fetchAllEmployeePensionEnrollments();

      return response;
    } catch (error) {
      if (error.response) {
      console.error(
        "Fetch all employee pension enrollments error response data:",
        error.response.data,
      );
      console.error(
        "Fetch all employee pension enrollments error status:",
        error.response.status,
      );
    } else {
      console.error(
        "Fetch all employee pension enrollments error message:",
        error.message,
      );
    }

    throw error;
    }
  }, []);

  const getEmployeePensionEnrollment = useCallback(async (employeeId) => {
    try {
      const response = await fetchEmployeePensionEnrollment(employeeId);

      return response;
    } catch (error) {
      if (error.response) {
      console.error(
        "Fetch employee pension enrollment error response data:",
        error.response.data,
      );
      console.error(
        "Fetch employee pension enrollment error status:",
        error.response.status,
      );
    } else {
      console.error(
        "Fetch employee pension enrollment error message:",
        error.message,
      );
    }

    throw error;
    }
  }, []);


  const getPensionEnrollmentsByPayrollRun = useCallback(async (payrollRunId) => {
    try {
      const response = await fetchPensionEnrollmentsByPayrollRun(payrollRunId);

      return response;
    } catch (error) {
      if (error.response) {
      console.error(
        "Fetch pension enrollments by payroll run error response data:",
        error.response.data,
      );
      console.error(
        "Fetch pension enrollments by payroll run error status:",
        error.response.status,
      );
    } else {
      console.error(
        "Fetch pension enrollments by payroll run error message:",
        error.message,
      );
    }

    throw error;
    }
  }, []);


  const editEmployeePensionEnrollment = useCallback(async (pensionEnrollmentData) => {
    try {
      const response = await updateEmployeePensionEnrollment(pensionEnrollmentData);

      return response;
    } catch (error) {
      if (error.response) {
      console.error(
        "Update employee pension enrollment error response data:",
        error.response.data,
      );
      console.error(
        "Update employee pension enrollment error status:",
        error.response.status,
      );
    } else {
      console.error(
        "Update employee pension enrollment error message:",
        error.message,
      );
    }

    throw error;
    }
  }, []);

  return {
    getAllPensionOptions,
    addPensionFund,
    selectEmployeeByPensionOption,
    enrollEmployeePension,
    getAllEmployeePensionEnrollments,
    getEmployeePensionEnrollment,
    getPensionEnrollmentsByPayrollRun,
    editEmployeePensionEnrollment
  }
};

export default usePensionOptions;
