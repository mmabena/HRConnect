// BankingDetail.js

import api from "./api.js";
import { toast } from "react-toastify";

const API_BASE =
  "http://localhost:5147/api/BankingDetails";

/* =========================
   RESPONSE INTERCEPTOR
========================= */
api.interceptors.response.use(
  (response) => {

    if (response.data === "") {
      response.data = null;
    }

    return response;
  },

  (error) => {
    return Promise.reject(error);
  }
);

/* =========================
   CREATE BANKING DETAILS
========================= */
export const addBankingDetails = async (
  bankingDetails
) => {

  try {

    const response = await api.post(
      `${API_BASE}/CreateBankingDetails`,
      bankingDetails,
      {
        headers: {
          "Content-Type":
            "application/json",
        },
      }
    );

    toast.success(
      "Banking details added successfully"
    );

    return response.data || {};

  } catch (error) {

    if (error.response) {

      console.error(
        "Add banking details error response data:",
        error.response.data
      );

      console.error(
        "Add banking details error status:",
        error.response.status
      );

    } else {

      console.error(
        "Add banking details error message:",
        error.message
      );
    }

    toast.error(
      "Failed to add banking details"
    );

    throw error;
  }
};

/* =========================
   EDIT BANKING DETAILS
========================= */
export const editBankingDetails = async (
  employeeId,
  bankingDetails
) => {

  try {

    const response = await api.put(
      `${API_BASE}/${employeeId}`,
      bankingDetails,
      {
        headers: {
          "Content-Type":
            "application/json",
        },
      }
    );

    toast.success(
      "Banking details updated successfully"
    );

    return response.data || {};

  } catch (error) {

    if (error.response) {

      console.error(
        "Edit banking details error response data:",
        error.response.data
      );

      console.error(
        "Edit banking details error status:",
        error.response.status
      );

    } else {

      console.error(
        "Edit banking details error message:",
        error.message
      );
    }

    toast.error(
      "Failed to update banking details"
    );

    throw error;
  }
};

/* =========================
   GET BANK BRANCH CODES
========================= */
export const getBankBranchCodes =
  async () => {

    try {

      const response = await api.get(
        `${API_BASE}/BankBranchCodes`
      );

      console.log(
        "Bank Branch Codes:",
        response.data
      );

      return response.data || [];

    } catch (error) {

      console.error(
        "Error fetching bank branch codes:",
        error
      );

      toast.error(
        "Failed to load bank names"
      );

      throw error;
    }
  };