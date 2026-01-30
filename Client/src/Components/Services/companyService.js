import api from '../../api.js';

const basePath = '/company';

export const getCompany = async () => {
  try {
    console.log("Calling GET /company..."); 
    return await api.get(basePath);
  } catch (error) {
    console.error('Error fetching companies:', error);
    throw error;
  }
};

export const getCompanyById = async (companyId) => {
  try {
    return await api.get(`${basePath}/${companyId}`);
  } catch (error) {
    console.error(`Error fetching company by id ${companyId}:`, error);
    throw error;
  }
};

export const addCompany = async (companyData) => {
  try {
    return await api.post(basePath, companyData);
  } catch (error) {
    console.error('Error adding company:', error);
    throw error;
  }
};

export const updateCompany = async (companyId, companyData) => {
  try {
    return await api.put(`${basePath}/${companyId}`, companyData);
  } catch (error) {
    console.error(`Error updating company ${companyId}:`, error);
    throw error;
  }
};

/**
 * WARNING: Deleting a company is generally not allowed due to business rules.
 * This function exists for exceptional cases only and should be used with caution.
 * Prefer deactivating or archiving companies instead of deletion to preserve data integrity.
 */

export const deleteCompany = async (companyId) => {
  try {
    return await api.delete(`${basePath}/${companyId}`);
  } catch (error) {
    console.error(`Error deleting company ${companyId}:`, error);
    throw error;
  }
};

export const updateCompanyStatus = async (companyId, isActive) => {
  try {
    return await api.patch(`${basePath}/${companyId}/status?isActive=${isActive}`);
  } catch (error) {
    console.error(`Error updating status for company ${companyId}:`, error);
    throw error;
  }
};