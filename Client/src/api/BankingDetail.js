import api from "./api.js";

/* =========================
   CREATE BANKING DETAILS
========================= */
export const addBankingDetails = async (bankingDetails) => {
  const response = await api.post(
    `BankingDetails/CreateBankingDetails`,
    bankingDetails,
  );

  return response.data || {};
};

/* =========================
   EDIT BANKING DETAILS
========================= */
export const editBankingDetails = async (employeeId, bankingDetails) => {
  const response = await api.put(
    `BankingDetails/${employeeId}`,
    bankingDetails,
  );

  return response.data || {};
};

/* =========================
   VALIDATE BANKING DETAILS
========================= */
export const validateBankingDetails = async (bankingDetails) => {
  const response = await api.post(`BankingDetails/validate`, bankingDetails,);

  return response.data;
};

/* =========================
   GET BANK BRANCH CODES
========================= */
export const getBankBranchCodes = async () => {
  const response = await api.get(`BankingDetails/BankBranchCodes`);

  return response.data || [];
};
