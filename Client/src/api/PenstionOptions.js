import api from "./api.js";

/// </summary>
/// --- PENSION API HANDLERS ---
/// </summary>

/**
 * Get all available pension options
 */
export const fetchAllPensionOptions = async () => {
    const response = await api.get(`/Pension/options`);

    return response.data || [];
};

/**
 * Create a new pension fund/provider
 */
export const createPensionFund = async (pensionFundData) => {
    const response = await api.post(
      `/Pension/funds`,
      pensionFundData);

    return response.data || {};
};

/**
 * Assign a pension option to an employee
 */
export const selectEmployeePensionOption = async ({
  employeeId,
  pensionOptionId,
}) => {
    const response = await api.post(
      `/Pension/select-option`,
      {
        employeeId,
        pensionOptionId,
      });

    return response.data || {};
};

/**
 * Enroll an employee in a pension plan
 */
export const enrollEmployeeInPension = async ({
  employeeId,
  effectiveDate,
  voluntaryContribution,
  isVoluntaryContributionPermanent,
}) => {
    const response = await api.post(
      `/employeepensionenrollment/enroll`,
      {
        employeeId,
        effectiveDate,
        voluntaryContribution,
        isVoluntaryContributionPermanent,
      });

    return response.data || {};
};

/**
 * Get all employee pension enrollments
 */
export const fetchAllEmployeePensionEnrollments = async () => {
    const response = await api.get(
      `/employeepensionenrollment`,
    );

    return response.data || [];
};

/**
 * Get an employee's latest pension enrollment
 */
export const fetchEmployeePensionEnrollment = async (employeeId) => {
    const encodedEmployeeId = encodeURIComponent(employeeId);

    const response = await api.get(
      `/employeepensionenrollment/employee/${encodedEmployeeId}`,
    );

    return response.data || {};
};

/**
 * Get pension enrollments by payroll run
 */
export const fetchPensionEnrollmentsByPayrollRun = async (payrollRunId) => {
    const response = await api.get(
      `/employeepensionenrollment/employeepensionenrollment/${payrollRunId}`,
    );

    return response.data || [];
};

/**
 * Update employee pension enrollment
 */
export const updateEmployeePensionEnrollment = async (
  pensionEnrollmentData,
) => {
    const response = await api.put(
      `/employeepensionenrollment`,
      pensionEnrollmentData);

    return response.data || {};
};