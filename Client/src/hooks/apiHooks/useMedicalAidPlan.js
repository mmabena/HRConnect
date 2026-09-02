import { useCallback } from "react";

import {
  getMedicalAidPlans,
  createMedicalAidDeduction,
  createMedicalAidDependent,
  validateMedicalAidDependent,
  getEligibleMedicalAidPlans,
} from "../../api/MedicalAidPlan";

const useMedicalAidPlan = () => {
  const fetchMedicalAidPlans = useCallback(async () => {
    try {
      const response = await getMedicalAidPlans();

      return response;
    } catch (error) {
      console.error("Error fetching medical aid plans:", error);

      throw error;
    }
  }, []);

  const addMedicalAidDeduction = useCallback(async (employeeId, payload) => {
    try {
      const response = await createMedicalAidDeduction(employeeId, payload);

      return response;
    } catch (error) {
      console.error("Error creating medical aid deduction:", error);
      throw error;
    }
  }, []);

  const addMedicalAidDependent = useCallback(async (employeeId, payload) => {
    try {
      const response = await createMedicalAidDependent(employeeId, payload);

      return response;
    } catch (error) {
      console.error("Error creating medical aid dependent:", error);
      throw error;
    }
  }, []);

  const validateMedicalDependent = useCallback(async (employeeId, payload) => {
    try {
      const response = await validateMedicalAidDependent(employeeId, payload);

      return response;
    } catch (error) {
      if (error.response) {
        console.error(
          "Validate medical aid dependent error response data:",
          error.response.data,
        );
        console.error(
          "Validate medical aid dependent error status:",
          error.response.status,
        );
      } else {
        console.error(
          "Validate medical aid dependent error message:",
          error.message,
        );
      }
      throw error;
    }
  }, []);

  const fetchEligibleMedicalAidPlans = useCallback(async (payload) => {
    try {
      const response = await getEligibleMedicalAidPlans(payload);

      return response;
    } catch (error) {
      console.error("Error fetching eligible medical aid plans:", error);
    throw error;
    }
  }, []);

  return {
    fetchMedicalAidPlans,
    addMedicalAidDeduction,
    addMedicalAidDependent,
    validateMedicalDependent,
    fetchEligibleMedicalAidPlans
  };
};
export default useMedicalAidPlan;