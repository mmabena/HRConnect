import { useCallback } from "react";
import { toast } from "react-toastify";

import {
  addBankingDetails,
  editBankingDetails,
  validateBankingDetails,
  getBankBranchCodes,
} from "../../api/BankingDetail";

const useBankingDetails = () => {
  const createBankingDetails = useCallback(async (bankingDetails) => {
    try {
      const response = await addBankingDetails(bankingDetails);

      toast.success("Banking details added successfully");

      return response;
    } catch (error) {
      toast.error("Failed to add banking details");

      throw error;
    }
  }, []);

  const updateBankingDetails = useCallback(
    async (employeeId, bankingDetails) => {
      try {
        const response = await editBankingDetails(employeeId, bankingDetails);

        toast.success("Banking details updated successfully");

        return response;
      } catch (error) {
        toast.error("Failed to update banking details");

        throw error;
      }
    },
    [],
  );

  const validateDetails = useCallback(async (bankingDetails) => {
    try {
      const response = await validateBankingDetails(bankingDetails);

      return response;
    } catch (error) {
      toast.error("Failed to validate banking details");

      throw error;
    }
  }, []);

  const fetchBankBranchCodes = useCallback(async () => {
    try {
      const response = await getBankBranchCodes();

      return response;
    } catch (error) {
      toast.error("Failed to fetch bank branch codes");

      throw error;
    }
  }, []);

  return {
    createBankingDetails,
    updateBankingDetails,
    validateDetails,
    fetchBankBranchCodes,
  };
};

export default useBankingDetails;
