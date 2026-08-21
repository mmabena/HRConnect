import api from "./api.js";

/// </summary>
/// --- PENSION API HANDLERS ---
/// </summary>

/**
 * Get all available pension options
 */
export const fetchALLPensionOptions = async () => {
  try {
    const response = await api.get(`/Pension/options`);

    return response.data || [];
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
};

/**
 * Create a new pension fund/provider
 */
export const createPensionFund = async (pensionFundData) => {
  try {
    const response = await api.post(
      `/Pension/funds`,
      pensionFundData,
      {
        headers: {
          "Content-Type": "application/json",
        },
      },
    );

    return response.data || {};
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
};

/**
 * Assign a pension option to an employee
 */
export const selectEmployeePensionOption = async ({
  employeeId,
  pensionOptionId,
}) => {
  try {
    const response = await api.post(
      `/Pension/select-option`,
      {
        employeeId,
        pensionOptionId,
      },
      {
        headers: {
          "Content-Type": "application/json",
        },
      },
    );

    return response.data || {};
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
};

/**
 * Enroll an employee in a pension plan
 */
export const enrollEmployeeInPension = async ({
  employeeId,
  effectiveDate,
  voluntaryContribution,
  isVoluntaryContributionPermament,
}) => {
  try {
    const response = await api.post(
      `/employeepensionenrollment/enroll`,
      {
        employeeId,
        effectiveDate,
        voluntaryContribution,
        isVoluntaryContributionPermament,
      },
      {
        headers: {
          "Content-Type": "application/json",
        },
      },
    );

    return response.data || {};
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
};

/**
 * Get all employee pension enrollments
 */
export const fetchAllEmployeePensionEnrollments = async () => {
  try {
    const response = await api.get(
      `/employeepensionenrollment`,
    );

    return response.data || [];
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
};

/**
 * Get an employee's latest pension enrollment
 */
export const fetchEmployeePensionEnrollment = async (employeeId) => {
  try {
    const encodedEmployeeId = encodeURIComponent(employeeId);

    const response = await api.get(
      `/employeepensionenrollment/employee/${encodedEmployeeId}`,
    );

    return response.data || {};
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
};

/**
 * Get pension enrollments by payroll run
 */
export const fetchPensionEnrollmentsByPayrollRun = async (payrollRunId) => {
  try {
    const response = await api.get(
      `/employeepensionenrollment/employeepensionenrollment/${payrollRunId}`,
    );

    return response.data || [];
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
};

/**
 * Update employee pension enrollment
 */
export const updateEmployeePensionEnrollment = async (
  pensionEnrollmentData,
) => {
  try {
    const response = await api.put(
      `/employeepensionenrollment`,
      pensionEnrollmentData,
      {
        headers: {
          "Content-Type": "application/json",
        },
      },
    );

    return response.data || {};
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
};